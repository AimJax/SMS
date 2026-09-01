namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Represents a follow relationship between accounts
/// </summary>
public class Follow
{
    public int Id { get; set; }
    
    /// <summary>
    /// The account that is following
    /// </summary>
    public int FollowerAccountId { get; set; }
    
    /// <summary>
    /// The account being followed
    /// </summary>
    public int FollowedAccountId { get; set; }
    
    /// <summary>
    /// When the follow relationship was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? FollowerAccount { get; set; }
    public Account? FollowedAccount { get; set; }
}
