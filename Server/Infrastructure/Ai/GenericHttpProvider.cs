using System.Net.Http.Json;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.Infrastructure.Ai;

/// <summary>
/// Generic OpenAI-compatible API provider.
/// Supports DeepSeek, local Ollama servers, and any other OpenAI-compatible API.
/// </summary>
public class GenericHttpProvider : IAiTextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<GenericHttpProvider> _logger;
    private const string ProviderName = "Generic";

    public GenericHttpProvider(HttpClient httpClient, string baseUrl, string model, ILogger<GenericHttpProvider> logger)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _model = model;
        _logger = logger;
    }

    public string GetProviderName() => ProviderName;
    public string GetModelName() => _model;
    public bool IsConfigured => !string.IsNullOrEmpty(_model) && !string.IsNullOrEmpty(_baseUrl);

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var apiRequest = new OpenAiChatRequest
            {
                Model = _model,
                Messages = new[]
                {
                    new OpenAiMessage { Role = "system", Content = request.SystemPrompt },
                    new OpenAiMessage { Role = "user", Content = request.UserPrompt }
                },
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature
            };

            var endpoint = $"{_baseUrl}/chat/completions";
            var response = await _httpClient.PostAsJsonAsync(
                endpoint,
                apiRequest,
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Generic provider API error: {StatusCode} from {Endpoint}", 
                    response.StatusCode, endpoint);
                
                return AiGenerationResult.Failed(
                    $"API error: {response.StatusCode}",
                    $"HTTP_{(int)response.StatusCode}",
                    ProviderName,
                    _model,
                    (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(cancellationToken: cancellationToken);
            
            if (result?.Choices?.Length > 0 && !string.IsNullOrEmpty(result.Choices[0].Message?.Content))
            {
                var text = result.Choices[0].Message.Content.Trim();
                var durationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                
                AiTokenUsage? tokenUsage = null;
                if (result.Usage != null)
                {
                    tokenUsage = new AiTokenUsage
                    {
                        PromptTokens = result.Usage.PromptTokens,
                        CompletionTokens = result.Usage.CompletionTokens,
                        TotalTokens = result.Usage.TotalTokens
                    };
                }
                
                _logger.LogDebug("Generic provider generated {TokenCount} tokens in {DurationMs}ms from {Endpoint}", 
                    tokenUsage?.TotalTokens ?? 0, durationMs, endpoint);
                
                return AiGenerationResult.Successful(text, ProviderName, _model, durationMs, tokenUsage);
            }

            return AiGenerationResult.Failed("No content in response", "EMPTY_RESPONSE", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Generic provider request cancelled");
            return AiGenerationResult.Failed("Request cancelled", "CANCELLED", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Generic provider request timed out from {Endpoint}", _baseUrl);
            return AiGenerationResult.Failed("Request timed out", "TIMEOUT", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Generic provider HTTP error from {Endpoint}", _baseUrl);
            return AiGenerationResult.Failed("Network error", "NETWORK_ERROR", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic provider unexpected error from {Endpoint}", _baseUrl);
            return AiGenerationResult.Failed("Unexpected error", "INTERNAL_ERROR", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
    }
}
