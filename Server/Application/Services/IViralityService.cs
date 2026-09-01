using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for calculating and managing post virality
/// </summary>
public interface IViralityService
{
    /// <summary>
    /// Calculate virality metrics for a post
    /// </summary>
    Task<PostVirality> CalculateViralityAsync(Guid postId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current virality state for a post
    /// </summary>
    Task<ViralityState> GetViralityStateAsync(Guid postId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get virality data for a post
    /// </summary>
    Task<PostVirality?> GetPostViralityAsync(Guid postId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get posts by minimum virality state
    /// </summary>
    Task<List<Post>> GetViralPostsAsync(int count = 10, ViralityState minState = ViralityState.Trending, string? topic = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get trending posts (combines virality + recency)
    /// </summary>
    Task<List<Post>> GetTrendingPostsAsync(int count = 10, string? topic = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get recent state transitions for a post
    /// </summary>
    Task<List<ViralityTransition>> GetTransitionHistoryAsync(Guid postId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update virality for a post after new engagement
    /// </summary>
    Task<PostVirality> TrackEngagementAsync(Guid postId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check and handle threshold crossings for a post
    /// </summary>
    Task CheckThresholdsAsync(Guid postId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process share cascade for a post
    /// </summary>
    Task ProcessShareCascadeAsync(Guid postId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze post for controversy (uses LLM)
    /// </summary>
    Task<int> AnalyzeControversyAsync(Guid postId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for virality settings
/// </summary>
public class ViralityConfig
{
    /// <summary>
    /// Enable/disable virality system
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Total engagement threshold for trending
    /// </summary>
    public int TrendingThreshold { get; set; } = 50;
    
    /// <summary>
    /// Total engagement threshold for popular
    /// </summary>
    public int PopularThreshold { get; set; } = 200;
    
    /// <summary>
    /// Total engagement threshold for viral
    /// </summary>
    public int ViralThreshold { get; set; } = 1000;
    
    /// <summary>
    /// Total engagement threshold for massively viral
    /// </summary>
    public int MassivelyViralThreshold { get; set; } = 10000;
    
    /// <summary>
    /// Minimum velocity (engagements/hour) to be considered viral
    /// </summary>
    public float ViralVelocityMin { get; set; } = 10;
    
    /// <summary>
    /// Time window in hours for velocity calculation
    /// </summary>
    public int ViralWindowHours { get; set; } = 24;
    
    /// <summary>
    /// How often to process virality updates (minutes)
    /// </summary>
    public int ProcessingIntervalMinutes { get; set; } = 5;
    
    /// <summary>
    /// Maximum posts to process per tick
    /// </summary>
    public int MaxPostsPerTick { get; set; } = 100;
    
    /// <summary>
    /// Days to keep posts in active virality processing
    /// </summary>
    public int ActivePostDays { get; set; } = 7;
    
    /// <summary>
    /// Velocity drop percentage to trigger declining state
    /// </summary>
    public float DeclineVelocityDropPercent { get; set; } = 0.7f;
    
    /// <summary>
    /// Base follower gain when going viral
    /// </summary>
    public int BaseFollowerGainOnViral { get; set; } = 10;
    
    /// <summary>
    /// Base fame gain when going viral
    /// </summary>
    public float BaseFameGainOnViral { get; set; } = 5.0f;
}
