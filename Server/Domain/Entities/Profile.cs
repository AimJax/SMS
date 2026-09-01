namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Public profile associated with an account
/// </summary>
public class Profile
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to Account
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// Display name shown publicly
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// User biography/description
    /// </summary>
    public string? Bio { get; set; }
    
    /// <summary>
    /// Avatar image URL/path
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Follower count (cached for performance)
    /// </summary>
    public int FollowerCount { get; set; }
    
    /// <summary>
    /// Fame level (0-100, affects feed visibility)
    /// </summary>
    public float FameLevel { get; set; }
    
    /// <summary>
    /// When the profile was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the profile was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Account? Account { get; set; }
}
