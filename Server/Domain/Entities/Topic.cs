namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Topic category enumeration
/// </summary>
public enum TopicCategory
{
    /// <summary>
    /// General interest topics
    /// </summary>
    General = 0,
    
    /// <summary>
    /// Entertainment topics (movies, TV, music)
    /// </summary>
    Entertainment = 1,
    
    /// <summary>
    /// Gaming topics
    /// </summary>
    Gaming = 2,
    
    /// <summary>
    /// Technology topics
    /// </summary>
    Technology = 3,
    
    /// <summary>
    /// Sports topics
    /// </summary>
    Sports = 4,
    
    /// <summary>
    /// Political topics
    /// </summary>
    Politics = 5,
    
    /// <summary>
    /// News and current events
    /// </summary>
    News = 6,
    
    /// <summary>
    /// Lifestyle topics (fashion, food, travel)
    /// </summary>
    Lifestyle = 7,
    
    /// <summary>
    /// Art and creative topics
    /// </summary>
    Art = 8,
    
    /// <summary>
    /// Meme culture topics
    /// </summary>
    Meme = 9,
    
    /// <summary>
    /// Community-specific topics
    /// </summary>
    Community = 10,
    
    /// <summary>
    /// Event-based topics (live tweets)
    /// </summary>
    Event = 11,
    
    /// <summary>
    /// Viral hashtag (auto-created)
    /// </summary>
    Hashtag = 12
}

/// <summary>
/// Represents a topic/category for posts and trends
/// </summary>
public class Topic
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid TopicId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Topic name (lowercase, no spaces)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for UI (proper casing)
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// URL-safe slug
    /// </summary>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Topic category
    /// </summary>
    public TopicCategory Category { get; set; } = TopicCategory.General;
    
    /// <summary>
    /// Total posts ever with this topic
    /// </summary>
    public int PostCount { get; set; }
    
    /// <summary>
    /// Posts in last 7 days
    /// </summary>
    public int ActivePostCount { get; set; }
    
    /// <summary>
    /// Users following this topic
    /// </summary>
    public int SubscriberCount { get; set; }
    
    /// <summary>
    /// Whether this is an official/verified topic
    /// </summary>
    public bool IsVerified { get; set; }
    
    /// <summary>
    /// Whether this topic is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When the topic was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the topic was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Hashtag> Hashtags { get; set; } = new List<Hashtag>();
    public ICollection<Community> Communities { get; set; } = new List<Community>();
}
