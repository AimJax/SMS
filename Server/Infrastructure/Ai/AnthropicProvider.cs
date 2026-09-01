using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.Infrastructure.Ai;

/// <summary>
/// Anthropic API provider implementation.
/// Uses the Anthropic Messages API format.
/// </summary>
public class AnthropicProvider : IAiTextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<AnthropicProvider> _logger;
    private const string DefaultBaseUrl = "https://api.anthropic.com/v1";
    private const string ApiVersion = "2023-06-01";
    private const string ProviderName = "Anthropic";

    public AnthropicProvider(HttpClient httpClient, string model, ILogger<AnthropicProvider> logger)
    {
        _httpClient = httpClient;
        _model = model;
        _logger = logger;
        
        // Set the required Anthropic API version header
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
    }

    public string GetProviderName() => ProviderName;
    public string GetModelName() => _model;
    public bool IsConfigured => !string.IsNullOrEmpty(_model);

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var apiRequest = new AnthropicMessageRequest
            {
                Model = _model,
                SystemPrompt = request.SystemPrompt,
                Messages = new[]
                {
                    new AnthropicMessage { Role = "user", Content = request.UserPrompt }
                },
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{DefaultBaseUrl}/messages",
                apiRequest,
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorType = GetAnthropicErrorType(errorBody);
                _logger.LogWarning("Anthropic API error: {StatusCode}, Type: {ErrorType}", 
                    response.StatusCode, errorType);
                
                return AiGenerationResult.Failed(
                    $"API error: {response.StatusCode}",
                    errorType,
                    ProviderName,
                    _model,
                    (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
            }

            var result = await response.Content.ReadFromJsonAsync<AnthropicMessageResponse>(cancellationToken: cancellationToken);
            
            if (result?.Content?.Length > 0 && result.Content[0].Type == "text" && !string.IsNullOrEmpty(result.Content[0].Text))
            {
                var text = result.Content[0].Text.Trim();
                var durationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                
                AiTokenUsage? tokenUsage = null;
                if (result.Usage != null)
                {
                    tokenUsage = new AiTokenUsage
                    {
                        PromptTokens = result.Usage.InputTokens,
                        CompletionTokens = result.Usage.OutputTokens,
                        TotalTokens = result.Usage.InputTokens + result.Usage.OutputTokens
                    };
                }
                
                _logger.LogDebug("Anthropic generated {TokenCount} tokens in {DurationMs}ms", 
                    tokenUsage?.CompletionTokens ?? 0, durationMs);
                
                return AiGenerationResult.Successful(text, ProviderName, _model, durationMs, tokenUsage);
            }

            return AiGenerationResult.Failed("No content in response", "EMPTY_RESPONSE", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Anthropic request cancelled");
            return AiGenerationResult.Failed("Request cancelled", "CANCELLED", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Anthropic request timed out");
            return AiGenerationResult.Failed("Request timed out", "TIMEOUT", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Anthropic HTTP error");
            return AiGenerationResult.Failed("Network error", "NETWORK_ERROR", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anthropic unexpected error");
            return AiGenerationResult.Failed("Unexpected error", "INTERNAL_ERROR", ProviderName, _model,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds);
        }
    }

    private static string? GetAnthropicErrorType(string errorBody)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(errorBody);
            if (doc.RootElement.TryGetProperty("type", out var type))
            {
                return type.GetString();
            }
        }
        catch { }
        return null;
    }
}

// Anthropic API request/response models
internal class AnthropicMessageRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("system")]
    public string SystemPrompt { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public AnthropicMessage[] Messages { get; set; } = Array.Empty<AnthropicMessage>();

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
}

internal class AnthropicMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class AnthropicMessageResponse
{
    [JsonPropertyName("content")]
    public AnthropicContentBlock[]? Content { get; set; }

    [JsonPropertyName("usage")]
    public AnthropicUsage? Usage { get; set; }
}

internal class AnthropicContentBlock
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

internal class AnthropicUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }
}
