namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Account model for API responses
/// </summary>
public class Account
{
    public int Id { get; set; }
    public Guid AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public AccountProfile? Profile { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Account profile with display info
/// </summary>
public class AccountProfile
{
    public int AccountId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public float FameLevel { get; set; }
}
