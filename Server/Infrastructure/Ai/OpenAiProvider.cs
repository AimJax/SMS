using System.Net.Http.Json;
using System.Text.Json;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.Infrastructure.Ai;

/// <summary>
/// OpenAI API provider implementation.
/// Uses the OpenAI Chat Completions API format.
/// </summary>
public class OpenAiProvider : IAiTextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OpenAiProvider> _logger;
    private const string DefaultBaseUrl = "https://api.openai.com/v1";
    private const string ProviderName = "OpenAI";

    public OpenAiProvider(HttpClient httpClient, string model, ILogger<OpenAiProvider> logger)
    {
        _httpClient = httpClient;
        _model = model;
        _logger = logger;
    }

    public string GetProviderName() => ProviderName;
    public string GetModelName() => _model;
    public bool IsConfigured => !string.IsNullOrEmpty(_model);

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

            var response = await _httpClient.PostAsJsonAsync(
                $"{DefaultBaseUrl}/chat/completions",
                apiRequest,
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorCode = GetOpenAiErrorCode(errorBody);
                _logger.LogWarning("OpenAI API error: {StatusCode}, ErrorCode: {ErrorCode}", 
                    response.StatusCode, errorCode);
                
                return AiGenerationResult.Failed(
                    $"API error: {response.StatusCode}",
                    errorCode,
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
                
                _logger.LogDebug("OpenAI generated {TokenCount} tokens in {DurationMs}ms", 
                    tokenUsage?.TotalTokens ?? 0, durationMs);
                
                return AiGenerationResult.Successful(text, ProviderName, _model, durationMs, tokenUsage);
            }

            return AiGenerationResult.Failed("No content in response", "EMPTY_RESPONSE", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("OpenAI request cancelled");
            return AiGenerationResult.Failed("Request cancelled", "CANCELLED", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("OpenAI request timed out");
            return AiGenerationResult.Failed("Request timed out", "TIMEOUT", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "OpenAI HTTP error");
            return AiGenerationResult.Failed("Network error", "NETWORK_ERROR", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI unexpected error");
            return AiGenerationResult.Failed("Unexpected error", "INTERNAL_ERROR", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
    }

    private static string? GetOpenAiErrorCode(string errorBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            if (doc.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("code", out var code))
            {
                return code.GetString();
            }
        }
        catch { }
        return null;
    }
}
