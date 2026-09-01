using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Ai;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing AI provider configuration and providing the text generation abstraction.
/// Configuration is stored in the database for runtime reconfiguration without restart.
/// </summary>
public interface IAiProviderService
{
    /// <summary>
    /// Get the AI text generation service with current configuration.
    /// </summary>
    IAiTextGenerationService GetTextGenerationService();
    
    /// <summary>
    /// Get the current configuration (without the raw API key).
    /// </summary>
    Task<AiConfigInfo> GetConfigAsync();
    
    /// <summary>
    /// Update the AI provider configuration.
    /// </summary>
    Task<AiConfigInfo> UpdateConfigAsync(UpdateAiConfigRequest request);
    
    /// <summary>
    /// Test the current AI configuration with a simple prompt.
    /// </summary>
    Task<AiTestResult> TestConnectionAsync(string testPrompt = "Say 'Hello, this is a test!' and nothing else.");
    
    /// <summary>
    /// Check if AI generation is enabled and configured.
    /// </summary>
    bool IsEnabled { get; }
}

public class AiProviderService : IAiProviderService
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IContentGeneratorService _fallbackGenerator;
    private readonly ILogger<AiProviderService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly object _lock = new();
    
    // Cached configuration
    private string _cachedProvider = string.Empty;
    private string _cachedModel = string.Empty;
    private string _cachedApiKey = string.Empty;
    private string _cachedBaseUrl = string.Empty;
    private bool _cachedEnabled = false;
    private int _cachedTimeoutSeconds = 30;
    private DateTime _lastConfigLoad = DateTime.MinValue;
    private const int ConfigCacheDurationSeconds = 10;

    public AiProviderService(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        IContentGeneratorService fallbackGenerator,
        ILogger<AiProviderService> logger,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _fallbackGenerator = fallbackGenerator;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public bool IsEnabled
    {
        get
        {
            EnsureConfigLoaded();
            return _cachedEnabled && !string.IsNullOrEmpty(_cachedApiKey);
        }
    }

    public IAiTextGenerationService GetTextGenerationService()
    {
        EnsureConfigLoaded();
        
        // If disabled or no API key, return a no-op service that always falls back
        if (!_cachedEnabled || string.IsNullOrEmpty(_cachedApiKey))
        {
            _logger.LogDebug("AI generation disabled or not configured, using fallback");
            return new NoOpAiService();
        }

        var httpClient = _httpClientFactory.CreateClient("AIProvider");
        httpClient.Timeout = TimeSpan.FromSeconds(_cachedTimeoutSeconds);
        
        // Set the API key header based on provider
        switch (_cachedProvider.ToLowerInvariant())
        {
            case "openai":
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_cachedApiKey}");
                return new OpenAiProvider(httpClient, _cachedModel, 
                    _loggerFactory.CreateLogger<OpenAiProvider>());
                
            case "anthropic":
                httpClient.DefaultRequestHeaders.Remove("x-api-key");
                httpClient.DefaultRequestHeaders.Add("x-api-key", _cachedApiKey);
                return new AnthropicProvider(httpClient, _cachedModel,
                    _loggerFactory.CreateLogger<AnthropicProvider>());
                
            case "generic":
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                if (!_cachedApiKey.StartsWith("$") && !string.IsNullOrEmpty(_cachedApiKey))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_cachedApiKey}");
                }
                return new GenericHttpProvider(httpClient, _cachedBaseUrl, _cachedModel,
                    _loggerFactory.CreateLogger<GenericHttpProvider>());
                
            default:
                _logger.LogWarning("Unknown provider {Provider}, using fallback", _cachedProvider);
                return new NoOpAiService();
        }
    }

    public async Task<AiConfigInfo> GetConfigAsync()
    {
        var config = await _context.AiProviderConfigs.FirstOrDefaultAsync();
        
        if (config == null)
        {
            return new AiConfigInfo
            {
                Provider = null,
                Model = null,
                HasApiKey = false,
                BaseUrl = null,
                IsEnabled = false,
                TimeoutSeconds = 30,
                UpdatedAt = null
            };
        }

        return new AiConfigInfo
        {
            Provider = config.Provider,
            Model = config.Model,
            HasApiKey = !string.IsNullOrEmpty(config.ApiKey),
            ApiKeyMasked = config.GetMaskedApiKey(),
            BaseUrl = config.BaseUrl,
            IsEnabled = config.IsEnabled,
            TimeoutSeconds = config.TimeoutSeconds,
            UpdatedAt = config.UpdatedAt
        };
    }

    public async Task<AiConfigInfo> UpdateConfigAsync(UpdateAiConfigRequest request)
    {
        // Validate provider
        if (!AiProviders.IsValid(request.Provider))
        {
            throw new ArgumentException($"Unknown provider: {request.Provider}. Valid providers: {string.Join(", ", AiProviders.All)}");
        }

        // Validate model
        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new ArgumentException("Model is required");
        }

        // Validate API key
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            throw new ArgumentException("API key is required");
        }

        // Validate timeout
        if (request.TimeoutSeconds < 5 || request.TimeoutSeconds > 120)
        {
            throw new ArgumentException("Timeout must be between 5 and 120 seconds");
        }

        // Validate base URL for Generic provider
        if (request.Provider == AiProviders.Generic && string.IsNullOrWhiteSpace(request.BaseUrl))
        {
            throw new ArgumentException("Base URL is required for the Generic provider");
        }

        var config = await _context.AiProviderConfigs.FirstOrDefaultAsync();
        
        if (config == null)
        {
            config = new AiProviderConfig
            {
                Provider = request.Provider,
                Model = request.Model,
                ApiKey = request.ApiKey,
                BaseUrl = request.BaseUrl,
                IsEnabled = request.IsEnabled,
                TimeoutSeconds = request.TimeoutSeconds,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AiProviderConfigs.Add(config);
        }
        else
        {
            config.Provider = request.Provider;
            config.Model = request.Model;
            config.ApiKey = request.ApiKey;
            config.BaseUrl = request.BaseUrl;
            config.IsEnabled = request.IsEnabled;
            config.TimeoutSeconds = request.TimeoutSeconds;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        
        // Invalidate cache
        InvalidateCache();

        _logger.LogInformation("AI configuration updated: Provider={Provider}, Model={Model}, IsEnabled={IsEnabled}",
            config.Provider, config.Model, config.IsEnabled);

        return await GetConfigAsync();
    }

    public async Task<AiTestResult> TestConnectionAsync(string testPrompt = "Say 'Hello, this is a test!' and nothing else.")
    {
        var service = GetTextGenerationService();
        
        if (!service.IsConfigured)
        {
            return new AiTestResult
            {
                Success = false,
                Message = "AI provider is not configured or disabled. Please configure a provider and API key first."
            };
        }

        try
        {
            var request = new AiGenerationRequest
            {
                SystemPrompt = "You are a helpful assistant. Keep responses very short and to the point.",
                UserPrompt = testPrompt,
                MaxTokens = 50,
                Temperature = 0.7
            };

            // Use a short timeout for testing
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var result = await service.GenerateAsync(request, cts.Token);

            if (result.Success)
            {
                return new AiTestResult
                {
                    Success = true,
                    Message = $"Successfully connected to {result.Provider} ({result.Model}). Generated: \"{result.Text?.Truncate(100)}\"",
                    DurationMs = result.DurationMs
                };
            }
            else
            {
                // Don't leak the API key in the error message
                return new AiTestResult
                {
                    Success = false,
                    Message = $"Generation failed: {result.ErrorMessage}",
                    ErrorCode = result.ErrorCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI connection test failed");
            return new AiTestResult
            {
                Success = false,
                Message = "Connection test failed. Check network connectivity and configuration."
            };
        }
    }

    private void EnsureConfigLoaded()
    {
        lock (_lock)
        {
            if (DateTime.UtcNow - _lastConfigLoad > TimeSpan.FromSeconds(ConfigCacheDurationSeconds))
            {
                LoadConfigFromDatabase();
            }
        }
    }

    private void LoadConfigFromDatabase()
    {
        try
        {
            // Use synchronous query for cache loading (called from locked context)
            var config = _context.AiProviderConfigs.FirstOrDefault();
            
            if (config != null)
            {
                _cachedProvider = config.Provider;
                _cachedModel = config.Model;
                _cachedApiKey = config.ApiKey;
                _cachedBaseUrl = config.BaseUrl ?? string.Empty;
                _cachedEnabled = config.IsEnabled;
                _cachedTimeoutSeconds = config.TimeoutSeconds;
            }
            else
            {
                _cachedProvider = string.Empty;
                _cachedModel = string.Empty;
                _cachedApiKey = string.Empty;
                _cachedBaseUrl = string.Empty;
                _cachedEnabled = false;
                _cachedTimeoutSeconds = 30;
            }
            
            _lastConfigLoad = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load AI configuration from database");
        }
    }

    private void InvalidateCache()
    {
        lock (_lock)
        {
            _lastConfigLoad = DateTime.MinValue;
        }
    }
}

/// <summary>
/// DTO for returning AI configuration (without raw API key).
/// </summary>
public class AiConfigInfo
{
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public bool HasApiKey { get; set; }
    public string? ApiKeyMasked { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsEnabled { get; set; }
    public int TimeoutSeconds { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for updating AI configuration.
/// </summary>
public class UpdateAiConfigRequest
{
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Result of AI connection test.
/// </summary>
public class AiTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public long DurationMs { get; set; }
}

/// <summary>
/// No-op AI service that always returns failure, causing fallback to templates.
/// </summary>
internal class NoOpAiService : IAiTextGenerationService
{
    public bool IsConfigured => false;
    
    public Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AiGenerationResult.Failed("AI generation not configured", "NOT_CONFIGURED"));
    }

    public string GetProviderName() => "None";
    public string GetModelName() => "None";
}

/// <summary>
/// Extension method for string truncation (safe for null).
/// </summary>
internal static class StringExtensions
{
    public static string Truncate(this string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }
}
