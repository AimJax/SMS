using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing topics, hashtags, and trends
/// </summary>
public interface ITrendService
{
    // Topic operations
    Task<Topic?> GetTopicBySlugAsync(string slug);
    Task<Topic?> GetTopicByIdAsync(Guid topicId);
    Task<List<Topic>> GetAllTopicsAsync();
    Task<List<Topic>> GetActiveTopicsAsync();
    Task<Topic> CreateTopicAsync(string name, TopicCategory category, string? description = null);
    Task UpdateTopicPostCountAsync(Guid topicId);
    
    // Hashtag operations
    Task<Hashtag?> GetHashtagByTagAsync(string tag);
    Task<List<Hashtag>> GetTrendingHashtagsAsync(int count = 20);
    Task<List<Hashtag>> GetActiveHashtagsAsync();
    Task<Hashtag> GetOrCreateHashtagAsync(string tag);
    Task<List<string>> ExtractHashtagsAsync(string content);
    Task UpdateHashtagUsageAsync(string tag);
    
    // Trend operations
    Task<List<Trend>> GetGlobalTrendsAsync(int count = 10);
    Task<List<Trend>> GetCommunityTrendsAsync(int communityId, int count = 10);
    Task<List<Trend>> GetPersonalTrendsAsync(int accountId, int count = 10);
    Task<Trend?> GetTrendByIdAsync(Guid trendId);
    Task<Trend> CalculateTrendAsync(string query, TrendScope scope, int? communityId = null);
    Task ProcessTrendsTickAsync();
    
    // Trend propagation
    Task ProcessCrossCommunityPropagationAsync();
    
    // Topic subscription
    Task SubscribeToTopicAsync(int accountId, Guid topicId);
    Task UnsubscribeFromTopicAsync(int accountId, Guid topicId);
    Task<bool> IsSubscribedToTopicAsync(int accountId, Guid topicId);
}

/// <summary>
/// Configuration for trends
/// </summary>
public class TrendConfig
{
    /// <summary>
    /// Enable/disable trend system
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// How often to process trends (minutes)
    /// </summary>
    public int ProcessingIntervalMinutes { get; set; } = 15;
    
    /// <summary>
    /// Time window for trend calculation (hours)
    /// </summary>
    public int TrendWindowHours { get; set; } = 24;
    
    /// <summary>
    /// Minimum posts to be considered a trend
    /// </summary>
    public int MinPostsForTrend { get; set; } = 10;
    
    /// <summary>
    /// Maximum trending hashtags to track
    /// </summary>
    public int MaxTrendingHashtags { get; set; } = 20;
    
    /// <summary>
    /// How long trends last (hours)
    /// </summary>
    public int TrendDurationHours { get; set; } = 24;
    
    /// <summary>
    /// Propagation probability multiplier
    /// </summary>
    public double PropagationMultiplier { get; set; } = 1.0;
    
    /// <summary>
    /// Topic post count window (days)
    /// </summary>
    public int TopicPostCountDays { get; set; } = 7;
}
