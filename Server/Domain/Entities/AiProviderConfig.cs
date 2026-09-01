using System.ComponentModel.DataAnnotations;

namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Stores the AI provider configuration.
/// Stored in the database to allow runtime reconfiguration without server restart.
/// Note: ApiKey is stored plaintext. For production use, consider encryption at rest.
/// </summary>
public class AiProviderConfig
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// The AI provider name (e.g., "OpenAI", "Anthropic", "Generic").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// The model identifier (e.g., "gpt-4o", "claude-3-5-sonnet-20241022").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// The API key for authentication.
    /// WARNING: Stored plaintext. Do not expose in logs or API responses.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Base URL for the API endpoint.
    /// Used primarily by the Generic provider for OpenAI-compatible APIs.
    /// Default values are used when empty.
    /// </summary>
    [MaxLength(500)]
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// Whether AI generation is enabled.
    /// When false, the system falls back to template-based generation.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Timeout for API calls in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// When this configuration was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Returns a masked version of the API key for display purposes.
    /// Shows only the last 4 characters.
    /// </summary>
    public string GetMaskedApiKey()
    {
        if (string.IsNullOrEmpty(ApiKey) || ApiKey.Length < 4)
            return "****";
        return "****" + ApiKey[^4..];
    }
}

/// <summary>
/// Supported AI providers.
/// </summary>
public static class AiProviders
{
    public const string OpenAI = "OpenAI";
    public const string Anthropic = "Anthropic";
    public const string Generic = "Generic";
    
    public static readonly string[] All = { OpenAI, Anthropic, Generic };
    
    public static bool IsValid(string provider)
    {
        return All.Contains(provider, StringComparer.OrdinalIgnoreCase);
    }
}
