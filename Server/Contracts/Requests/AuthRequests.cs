using System.ComponentModel.DataAnnotations;

namespace SocialMediaSimulator.Server.Contracts.Requests;

public record RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; init; }

    [StringLength(500)]
    public string? Bio { get; init; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; init; }
}

public record LoginRequest
{
    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
