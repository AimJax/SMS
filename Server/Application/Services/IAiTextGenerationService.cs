namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Provider-agnostic abstraction for AI text generation.
/// All business logic depends only on this interface, never on concrete provider implementations.
/// </summary>
public interface IAiTextGenerationService
{
    /// <summary>
    /// Generate text using the configured AI provider.
    /// </summary>
    /// <param name="request">The generation request with prompt and parameters.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The generation result containing the generated text or error information.</returns>
    Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the name of the currently configured provider.
    /// </summary>
    string GetProviderName();
    
    /// <summary>
    /// Get the name of the currently configured model.
    /// </summary>
    string GetModelName();
    
    /// <summary>
    /// Check if the service is enabled and has a valid configuration.
    /// </summary>
    bool IsConfigured { get; }
}

/// <summary>
/// Request parameters for AI text generation.
/// Provider-neutral DTO that contains everything needed to construct an API call.
/// </summary>
public class AiGenerationRequest
{
    /// <summary>
    /// The system prompt that defines the AI's role and constraints.
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;
    
    /// <summary>
    /// The user prompt containing the actual generation request.
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum number of tokens to generate.
    /// </summary>
    public int MaxTokens { get; set; } = 150;
    
    /// <summary>
    /// Temperature for sampling (0.0-2.0, lower = more deterministic).
    /// </summary>
    public double Temperature { get; set; } = 0.8;
    
    /// <summary>
    /// Optional request identifier for logging/tracing.
    /// </summary>
    public string? RequestId { get; set; }
}

/// <summary>
/// Result of AI text generation.
/// Provider-neutral DTO that contains the generated text or error information.
/// </summary>
public class AiGenerationResult
{
    /// <summary>
    /// Whether the generation was successful.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The generated text, if successful.
    /// </summary>
    public string? Text { get; set; }
    
    /// <summary>
    /// Error message if the generation failed.
    /// Must NOT contain any sensitive information (API keys, etc.).
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Provider-specific error code, if applicable.
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// The name of the provider that generated this result.
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// The model that was used for generation.
    /// </summary>
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// Token usage information, if available from the provider.
    /// </summary>
    public AiTokenUsage? TokenUsage { get; set; }
    
    /// <summary>
    /// Duration of the API call in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }
    
    /// <summary>
    /// Whether this was a fallback result (e.g., due to timeout or error).
    /// </summary>
    public bool IsFallback { get; set; }
    
    /// <summary>
    /// Factory method for successful results.
    /// </summary>
    public static AiGenerationResult Successful(string text, string provider, string model, long durationMs, AiTokenUsage? tokenUsage = null)
    {
        return new AiGenerationResult
        {
            Success = true,
            Text = text,
            Provider = provider,
            Model = model,
            DurationMs = durationMs,
            TokenUsage = tokenUsage
        };
    }
    
    /// <summary>
    /// Factory method for failed results.
    /// </summary>
    public static AiGenerationResult Failed(string errorMessage, string? errorCode = null, string provider = "", string model = "", long durationMs = 0)
    {
        return new AiGenerationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            Provider = provider,
            Model = model,
            DurationMs = durationMs
        };
    }
}

/// <summary>
/// Token usage information from AI providers.
/// </summary>
public class AiTokenUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}
