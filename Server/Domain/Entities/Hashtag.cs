namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Represents a hashtag used in posts
/// </summary>
public class Hashtag
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid HashtagId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Full hashtag with # prefix
    /// </summary>
    public string Tag { get; set; } = string.Empty;
    
    /// <summary>
    /// Normalized tag (lowercase, no #)
    /// </summary>
    public string NormalizedTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Associated topic ID (nullable)
    /// </summary>
    public Guid? TopicId { get; set; }
    
    /// <summary>
    /// Total times this hashtag has been used
    /// </summary>
    public int UsageCount { get; set; }
    
    /// <summary>
    /// Usage count for today
    /// </summary>
    public int TodayUsageCount { get; set; }
    
    /// <summary>
    /// Whether this hashtag is currently trending
    /// </summary>
    public bool IsTrending { get; set; }
    
    /// <summary>
    /// When the hashtag started trending
    /// </summary>
    public DateTime? TrendingSince { get; set; }
    
    /// <summary>
    /// Current trend rank (1 = most trending)
    /// </summary>
    public int TrendingRank { get; set; }
    
    /// <summary>
    /// When the hashtag was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the hashtag was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Topic? Topic { get; set; }
}
