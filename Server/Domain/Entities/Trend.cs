namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Trend type enumeration
/// </summary>
public enum TrendType
{
    /// <summary>
    /// Based on topic
    /// </summary>
    Topic = 0,
    
    /// <summary>
    /// Based on hashtag
    /// </summary>
    Hashtag = 1,
    
    /// <summary>
    /// Based on event
    /// </summary>
    Event = 2,
    
    /// <summary>
    /// Based on search queries
    /// </summary>
    Search = 3,
    
    /// <summary>
    /// Based on viral content
    /// </summary>
    Viral = 4
}

/// <summary>
/// Trend strength enumeration
/// </summary>
public enum TrendStrength
{
    /// <summary>
    /// Just starting to trend
    /// </summary>
    Emerging = 1,
    
    /// <summary>
    /// Gaining momentum
    /// </summary>
    Growing = 2,
    
    /// <summary>
    /// High activity
    /// </summary>
    Hot = 3,
    
    /// <summary>
    /// Very high activity
    /// </summary>
    Viral = 4,
    
    /// <summary>
    /// Near peak, about to decline
    /// </summary>
    Peaking = 5
}

/// <summary>
/// Trend scope enumeration
/// </summary>
public enum TrendScope
{
    /// <summary>
    /// Entire network
    /// </summary>
    Global = 0,
    
    /// <summary>
    /// Community-specific
    /// </summary>
    Community = 1,
    
    /// <summary>
    /// Personalized to user
    /// </summary>
    Personal = 2
}

/// <summary>
/// Represents a trending topic/hashtag
/// </summary>
public class Trend
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid TrendId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Type of trend
    /// </summary>
    public TrendType Type { get; set; }
    
    /// <summary>
    /// Associated topic ID
    /// </summary>
    public Guid? TopicId { get; set; }
    
    /// <summary>
    /// Associated hashtag ID
    /// </summary>
    public Guid? HashtagId { get; set; }
    
    /// <summary>
    /// Search query for custom trends
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// URL-safe identifier
    /// </summary>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Calculated trend strength
    /// </summary>
    public TrendStrength Strength { get; set; } = TrendStrength.Emerging;
    
    /// <summary>
    /// Posts in trend window
    /// </summary>
    public int PostCount { get; set; }
    
    /// <summary>
    /// Unique accounts posting
    /// </summary>
    public int UniquePosters { get; set; }
    
    /// <summary>
    /// Total engagement on trend
    /// </summary>
    public int EngagementTotal { get; set; }
    
    /// <summary>
    /// Growth rate (posts per hour)
    /// </summary>
    public float Velocity { get; set; }
    
    /// <summary>
    /// Position in trend list
    /// </summary>
    public int Rank { get; set; }
    
    /// <summary>
    /// Trend scope
    /// </summary>
    public TrendScope Scope { get; set; } = TrendScope.Global;
    
    /// <summary>
    /// Community ID if community-specific trend
    /// </summary>
    public int? CommunityId { get; set; }
    
    /// <summary>
    /// When trend was calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When trend reached maximum
    /// </summary>
    public DateTime? PeakedAt { get; set; }
    
    /// <summary>
    /// When trend expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Whether trend is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When the trend record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Topic? Topic { get; set; }
    public Hashtag? Hashtag { get; set; }
    public Community? Community { get; set; }
}

/// <summary>
/// Tracks trend propagation between communities
/// </summary>
public class TrendPropagation
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid PropagationId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The trend that propagated
    /// </summary>
    public Guid TrendId { get; set; }
    
    /// <summary>
    /// Source community
    /// </summary>
    public int FromCommunityId { get; set; }
    
    /// <summary>
    /// Destination community
    /// </summary>
    public int ToCommunityId { get; set; }
    
    /// <summary>
    /// When propagation occurred
    /// </summary>
    public DateTime PropagatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Trend? Trend { get; set; }
    public Community? FromCommunity { get; set; }
    public Community? ToCommunity { get; set; }
}
