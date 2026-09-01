namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Authentication response
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public Guid? UserId { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Login request
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Register request
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
