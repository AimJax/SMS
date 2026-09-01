namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Authentication response (matches server AuthResponse)
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public AccountData? Account { get; set; }
}

/// <summary>
/// Account data from auth endpoints (matches server AccountResponse)
/// </summary>
public class AccountData
{
    public Guid AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AccountType { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Register request
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// Login request
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Error response
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}
