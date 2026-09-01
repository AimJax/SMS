using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Configuration for feed scoring weights and parameters
/// </summary>
public class FeedScoringConfig
{
    /// <summary>
    /// Weight for recency score (0.0 - 1.0)
    /// </summary>
    public double RecencyWeight { get; set; } = 0.25;
    
    /// <summary>
    /// Weight for interest match score (0.0 - 1.0)
    /// </summary>
    public double InterestWeight { get; set; } = 0.20;
    
    /// <summary>
    /// Weight for relationship affinity score (0.0 - 1.0)
    /// </summary>
    public double RelationshipWeight { get; set; } = 0.20;
    
    /// <summary>
    /// Weight for engagement score (0.0 - 1.0)
    /// </summary>
    public double EngagementWeight { get; set; } = 0.15;
    
    /// <summary>
    /// Weight for community affinity score (0.0 - 1.0)
    /// </summary>
    public double CommunityWeight { get; set; } = 0.10;
    
    /// <summary>
    /// Weight for author fame score (0.0 - 1.0)
    /// </summary>
    public double FameWeight { get; set; } = 0.05;
    
    /// <summary>
    /// Weight for discovery score (0.0 - 1.0)
    /// </summary>
    public double DiscoveryWeight { get; set; } = 0.05;
    
    /// <summary>
    /// Base decay rate for recency calculation (0.0 - 1.0)
    /// </summary>
    public double RecencyBaseDecay { get; set; } = 0.5;
    
    /// <summary>
    /// Half-life in hours for recency decay
    /// </summary>
    public double RecencyHalfLifeHours { get; set; } = 6.0;
    
    /// <summary>
    /// Maximum expected engagement for normalization (used in log calculation)
    /// </summary>
    public double MaxExpectedEngagement { get; set; } = 1000.0;
    
    /// <summary>
    /// Velocity bonus multiplier for engagement boost
    /// </summary>
    public double VelocityBonusMultiplier { get; set; } = 0.2;
    
    /// <summary>
    /// Community affinity score when user is a member
    /// </summary>
    public double CommunityMemberAffinityScore { get; set; } = 0.8;
    
    /// <summary>
    /// Community affinity score when post is about a joined community topic
    /// </summary>
    public double CommunityTopicAffinityScore { get; set; } = 0.5;
    
    /// <summary>
    /// Baseline relationship score for followed accounts without explicit relationship
    /// </summary>
    public double FollowedAccountBaseline { get; set; } = 0.3;
    
    /// <summary>
    /// Baseline relationship score for unknown accounts
    /// </summary>
    public double UnknownAccountBaseline { get; set; } = 0.1;
    
    /// <summary>
    /// Discovery score for non-followed accounts not seen before
    /// </summary>
    public double NewDiscoveryScore { get; set; } = 0.8;
    
    /// <summary>
    /// Discovery score for non-followed accounts seen before
    /// </summary>
    public double SeenDiscoveryScore { get; set; } = 0.3;
    
    /// <summary>
    /// Minimum discovery content percentage (0.0 - 1.0)
    /// </summary>
    public double MinDiscoveryPercentage { get; set; } = 0.10;
    
    /// <summary>
    /// Maximum candidate posts to score
    /// </summary>
    public int MaxCandidates { get; set; } = 200;
    
    /// <summary>
    /// Hours of post history to consider for feed
    /// </summary>
    public int PostHistoryHours { get; set; } = 24;
    
    /// <summary>
    /// Default echo chamber strength (0.0 - 1.0)
    /// </summary>
    public double DefaultEchoChamberStrength { get; set; } = 0.5;
    
    /// <summary>
    /// Cache TTL in minutes for feed pages
    /// </summary>
    public int FeedCacheTtlMinutes { get; set; } = 5;
    
    /// <summary>
    /// Cache TTL in minutes for individual scores
    /// </summary>
    public int ScoreCacheTtlMinutes { get; set; } = 1;
    
    /// <summary>
    /// Engagement threshold for cache invalidation
    /// </summary>
    public int CacheInvalidationThreshold { get; set; } = 10;
    
    /// <summary>
    /// Discovery content minimum score threshold
    /// </summary>
    public double DiscoveryMinScore { get; set; } = 0.5;
}

/// <summary>
/// Individual score breakdown for debugging/analysis
/// </summary>
public class FeedScoreBreakdown
{
    public double RecencyScore { get; set; }
    public double InterestScore { get; set; }
    public double RelationshipScore { get; set; }
    public double EngagementScore { get; set; }
    public double CommunityScore { get; set; }
    public double FameScore { get; set; }
    public double DiscoveryScore { get; set; }
    public double FinalScore { get; set; }
}

/// <summary>
/// Represents a scored feed candidate
/// </summary>
public class ScoredFeedItem
{
    public Post Post { get; set; } = null!;
    public Account AuthorAccount { get; set; } = null!;
    public Profile? AuthorProfile { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public int? CommunityId { get; set; }
    public double FinalScore { get; set; }
    public FeedScoreBreakdown ScoreBreakdown { get; set; } = new();
}
