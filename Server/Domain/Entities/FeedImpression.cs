namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Tracks feed impressions for future algorithm optimization
/// </summary>
public class FeedImpression
{
    public int Id { get; set; }
    
    /// <summary>
    /// The account that saw this post
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// The post that was shown
    /// </summary>
    public int PostId { get; set; }
    
    /// <summary>
    /// Position in the feed (0-indexed)
    /// </summary>
    public int Position { get; set; }
    
    /// <summary>
    /// Whether the user clicked/expanded the post
    /// </summary>
    public bool Clicked { get; set; }
    
    /// <summary>
    /// Whether the user liked the post
    /// </summary>
    public bool Liked { get; set; }
    
    /// <summary>
    /// Whether the user commented on the post
    /// </summary>
    public bool Commented { get; set; }
    
    /// <summary>
    /// Whether the user shared/reposted the post
    /// </summary>
    public bool Shared { get; set; }
    
    /// <summary>
    /// Whether the user skipped this post (scrolled past without interaction)
    /// </summary>
    public bool Skipped { get; set; }
    
    /// <summary>
    /// Final score when the post was ranked
    /// </summary>
    public double Score { get; set; }
    
    /// <summary>
    /// When the impression was recorded
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? Account { get; set; }
    public Post? Post { get; set; }
}
