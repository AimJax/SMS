using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SocialMediaSimulator.Server.Application.Models;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// In-memory cache service for feed data
/// </summary>
public class FeedCacheService : IFeedCacheService
{
    private readonly ILogger<FeedCacheService> _logger;
    private readonly FeedScoringConfig _config;
    
    // Cache entries: (key) -> (timestamp, data)
    private static readonly ConcurrentDictionary<string, CacheEntry> _feedCache = new();
    private static readonly ConcurrentDictionary<string, CacheEntry> _scoreCache = new();
    
    private class CacheEntry
    {
        public DateTime Timestamp { get; set; }
        public object Data { get; set; } = null!;
        public TimeSpan Ttl { get; set; }
        
        public bool IsExpired => DateTime.UtcNow - Timestamp > Ttl;
    }

    public FeedCacheService(ILogger<FeedCacheService> logger, FeedScoringConfig config)
    {
        _logger = logger;
        _config = config;
    }

    /// <inheritdoc />
    public string GetFeedCacheKey(int accountId, string? cursor)
    {
        return $"feed:{accountId}:cursor:{cursor ?? "first"}";
    }

    /// <inheritdoc />
    public string GetScoreCacheKey(int postId, int accountId)
    {
        return $"score:{postId}:{accountId}";
    }

    /// <inheritdoc />
    public async Task<AdvancedFeedResponse?> GetCachedFeedAsync(int accountId, string? cursor)
    {
        var key = GetFeedCacheKey(accountId, cursor);
        
        if (_feedCache.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired)
            {
                _logger.LogDebug("Feed cache hit for account {AccountId}", accountId);
                return (AdvancedFeedResponse)entry.Data;
            }
            
            // Remove expired entry
            _feedCache.TryRemove(key, out _);
            _logger.LogDebug("Feed cache expired for account {AccountId}", accountId);
        }
        
        return null;
    }

    /// <inheritdoc />
    public async Task SetCachedFeedAsync(int accountId, string? cursor, AdvancedFeedResponse feed)
    {
        var key = GetFeedCacheKey(accountId, cursor);
        var entry = new CacheEntry
        {
            Timestamp = DateTime.UtcNow,
            Data = feed,
            Ttl = TimeSpan.FromMinutes(_config.FeedCacheTtlMinutes)
        };
        
        _feedCache.AddOrUpdate(key, entry, (_, _) => entry);
        _logger.LogDebug("Cached feed for account {AccountId}", accountId);
    }

    /// <inheritdoc />
    public async Task<double?> GetCachedScoreAsync(int postId, int accountId)
    {
        var key = GetScoreCacheKey(postId, accountId);
        
        if (_scoreCache.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired)
            {
                return (double)entry.Data;
            }
            
            _scoreCache.TryRemove(key, out _);
        }
        
        return null;
    }

    /// <inheritdoc />
    public async Task SetCachedScoreAsync(int postId, int accountId, double score)
    {
        var key = GetScoreCacheKey(postId, accountId);
        var entry = new CacheEntry
        {
            Timestamp = DateTime.UtcNow,
            Data = score,
            Ttl = TimeSpan.FromMinutes(_config.ScoreCacheTtlMinutes)
        };
        
        _scoreCache.AddOrUpdate(key, entry, (_, _) => entry);
    }

    /// <inheritdoc />
    public async Task InvalidateAccountFeedAsync(int accountId)
    {
        // Remove all feed cache entries for this account
        var keysToRemove = _feedCache.Keys
            .Where(k => k.StartsWith($"feed:{accountId}:"))
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _feedCache.TryRemove(key, out _);
        }
        
        _logger.LogDebug("Invalidated {Count} feed cache entries for account {AccountId}", keysToRemove.Count, accountId);
    }

    /// <inheritdoc />
    public async Task InvalidatePostScoresAsync(int postId)
    {
        // Remove all score cache entries for this post
        var keysToRemove = _scoreCache.Keys
            .Where(k => k.StartsWith($"score:{postId}:"))
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _scoreCache.TryRemove(key, out _);
        }
        
        _logger.LogDebug("Invalidated {Count} score cache entries for post {PostId}", keysToRemove.Count, postId);
    }

    /// <inheritdoc />
    public async Task InvalidateFollowingChangeAsync(int followerId, int followedId)
    {
        // Invalidate feed for the follower when they follow/unfollow someone
        await InvalidateAccountFeedAsync(followerId);
        _logger.LogDebug("Invalidated feed cache due to follow change: {FollowerId} -> {FollowedId}", followerId, followedId);
    }

    /// <inheritdoc />
    public async Task InvalidateCommunityChangeAsync(int accountId, int communityId)
    {
        // Invalidate feed when user joins/leaves a community
        await InvalidateAccountFeedAsync(accountId);
        _logger.LogDebug("Invalidated feed cache due to community change: {AccountId} in community {CommunityId}", accountId, communityId);
    }

    /// <inheritdoc />
    public async Task CleanupExpiredEntriesAsync()
    {
        // Clean up expired feed cache entries
        var expiredFeedKeys = _feedCache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in expiredFeedKeys)
        {
            _feedCache.TryRemove(key, out _);
        }
        
        // Clean up expired score cache entries
        var expiredScoreKeys = _scoreCache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in expiredScoreKeys)
        {
            _scoreCache.TryRemove(key, out _);
        }
        
        if (expiredFeedKeys.Count > 0 || expiredScoreKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {FeedCount} feed and {ScoreCount} score cache entries", 
                expiredFeedKeys.Count, expiredScoreKeys.Count);
        }
    }
}

/// <summary>
/// Response model for advanced feed
/// </summary>
public class AdvancedFeedResponse
{
    public IEnumerable<AdvancedFeedItemResponse> Items { get; set; } = new List<AdvancedFeedItemResponse>();
    public string? NextCursor { get; set; }
    public int PageSize { get; set; }
    public int TotalCandidates { get; set; }
    public List<ScoreBreakdownSummary>? ScoreBreakdowns { get; set; }
}

public class AdvancedFeedItemResponse
{
    public Guid PostId { get; set; }
    public Guid AuthorAccountId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public int? CommunityId { get; set; }
    public string? CommunitySlug { get; set; }
    public string? CommunityName { get; set; }
    public double Score { get; set; }
}

public class ScoreBreakdownSummary
{
    public Guid PostId { get; set; }
    public double RecencyScore { get; set; }
    public double InterestScore { get; set; }
    public double RelationshipScore { get; set; }
    public double EngagementScore { get; set; }
    public double CommunityScore { get; set; }
    public double FameScore { get; set; }
    public double DiscoveryScore { get; set; }
}

/// <summary>
/// Interface for feed caching operations
/// </summary>
public interface IFeedCacheService
{
    string GetFeedCacheKey(int accountId, string? cursor);
    string GetScoreCacheKey(int postId, int accountId);
    Task<AdvancedFeedResponse?> GetCachedFeedAsync(int accountId, string? cursor);
    Task SetCachedFeedAsync(int accountId, string? cursor, AdvancedFeedResponse feed);
    Task<double?> GetCachedScoreAsync(int postId, int accountId);
    Task SetCachedScoreAsync(int postId, int accountId, double score);
    Task InvalidateAccountFeedAsync(int accountId);
    Task InvalidatePostScoresAsync(int postId);
    Task InvalidateFollowingChangeAsync(int followerId, int followedId);
    Task InvalidateCommunityChangeAsync(int accountId, int communityId);
    Task CleanupExpiredEntriesAsync();
}
