namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Virality state enumeration for posts
/// </summary>
public enum ViralityState
{
    /// <summary>
    /// Standard engagement, below trending threshold
    /// </summary>
    Normal = 0,
    
    /// <summary>
    /// Gaining traction, crossed trending threshold
    /// </summary>
    Trending = 1,
    
    /// <summary>
    /// Above average engagement
    /// </summary>
    Popular = 2,
    
    /// <summary>
    /// Crossed viral threshold
    /// </summary>
    Viral = 3,
    
    /// <summary>
    /// Extremely viral, massively popular
    /// </summary>
    MassivelyViral = 4,
    
    /// <summary>
    /// Was viral, now cooling down
    /// </summary>
    Declining = 5
}

/// <summary>
/// Tracks virality metrics for a post
/// </summary>
public class PostVirality
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid PostViralityId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The post this virality data belongs to
    /// </summary>
    public Guid PostId { get; set; }
    
    /// <summary>
    /// Current virality state
    /// </summary>
    public ViralityState State { get; set; } = ViralityState.Normal;
    
    /// <summary>
    /// Calculated virality score (0-100)
    /// </summary>
    public float Score { get; set; }
    
    /// <summary>
    /// Total engagement count (likes + comments + reposts)
    /// </summary>
    public int TotalEngagement { get; set; }
    
    /// <summary>
    /// Current engagement velocity (engagements per hour)
    /// </summary>
    public float Velocity { get; set; }
    
    /// <summary>
    /// Peak velocity reached during this post's lifetime
    /// </summary>
    public float PeakVelocity { get; set; }
    
    /// <summary>
    /// Estimated unique viewers
    /// </summary>
    public int Reach { get; set; }
    
    /// <summary>
    /// Estimated share count
    /// </summary>
    public int ShareCount { get; set; }
    
    /// <summary>
    /// When the post crossed the viral threshold
    /// </summary>
    public DateTime? ViralAt { get; set; }
    
    /// <summary>
    /// When the post crossed the massively viral threshold
    /// </summary>
    public DateTime? MassivelyViralAt { get; set; }
    
    /// <summary>
    /// When the post started declining
    /// </summary>
    public DateTime? DeclinedAt { get; set; }
    
    /// <summary>
    /// First viral threshold crossed
    /// </summary>
    public ViralityState? FirstViralThresholdCrossed { get; set; }
    
    /// <summary>
    /// Controversy level 0-10 (from LLM analysis)
    /// </summary>
    public int ControversyLevel { get; set; }
    
    /// <summary>
    /// Whether LLM analysis has been performed
    /// </summary>
    public bool HasControversyAnalysis { get; set; }
    
    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this virality record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Post? Post { get; set; }
}

/// <summary>
/// Logs virality state transitions for audit trail
/// </summary>
public class ViralityTransition
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid TransitionId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The post this transition belongs to
    /// </summary>
    public Guid PostId { get; set; }
    
    /// <summary>
    /// Previous virality state
    /// </summary>
    public ViralityState FromState { get; set; }
    
    /// <summary>
    /// New virality state
    /// </summary>
    public ViralityState ToState { get; set; }
    
    /// <summary>
    /// Virality score at time of transition
    /// </summary>
    public float ScoreAtTransition { get; set; }
    
    /// <summary>
    /// Total engagement at time of transition
    /// </summary>
    public int EngagementAtTransition { get; set; }
    
    /// <summary>
    /// Velocity at time of transition
    /// </summary>
    public float VelocityAtTransition { get; set; }
    
    /// <summary>
    /// When this transition occurred
    /// </summary>
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional metadata about the transition
    /// </summary>
    public string Metadata { get; set; } = "{}";
    
    // Navigation property
    public Post? Post { get; set; }
}
