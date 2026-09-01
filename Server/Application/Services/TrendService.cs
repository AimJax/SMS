using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing topics, hashtags, and trends
/// </summary>
public class TrendService : ITrendService
{
    private readonly AppDbContext _context;
    private readonly IPostService _postService;
    private readonly IAccountService _accountService;
    private readonly ICommunityService _communityService;
    private readonly IAiTextGenerationService? _aiService;
    private readonly TrendConfig _config;
    private readonly ILogger<TrendService> _logger;
    
    private static readonly Regex HashtagRegex = new(@"(?:^|\s)(#\w+)", RegexOptions.Compiled);

    public TrendService(
        AppDbContext context,
        IPostService postService,
        IAccountService accountService,
        ICommunityService communityService,
        IAiTextGenerationService? aiService,
        TrendConfig config,
        ILogger<TrendService> logger)
    {
        _context = context;
        _postService = postService;
        _accountService = accountService;
        _communityService = communityService;
        _aiService = aiService;
        _config = config;
        _logger = logger;
    }

    #region Topic Operations

    public async Task<Topic?> GetTopicBySlugAsync(string slug)
    {
        var normalizedSlug = slug.ToLowerInvariant().Trim();
        return await _context.Topics
            .FirstOrDefaultAsync(t => t.Slug == normalizedSlug && t.IsActive);
    }

    public async Task<Topic?> GetTopicByIdAsync(Guid topicId)
    {
        return await _context.Topics
            .FirstOrDefaultAsync(t => t.TopicId == topicId && t.IsActive);
    }

    public async Task<List<Topic>> GetAllTopicsAsync()
    {
        return await _context.Topics
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayName)
            .ToListAsync();
    }

    public async Task<List<Topic>> GetActiveTopicsAsync()
    {
        return await _context.Topics
            .Where(t => t.IsActive && t.ActivePostCount > 0)
            .OrderByDescending(t => t.ActivePostCount)
            .ToListAsync();
    }

    public async Task<Topic> CreateTopicAsync(string name, TopicCategory category, string? description = null)
    {
        var topic = new Topic
        {
            Name = name.ToLowerInvariant().Trim(),
            DisplayName = char.ToUpper(name[0]) + name.Substring(1).ToLower(),
            Slug = GenerateSlug(name),
            Description = description,
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        
        return topic;
    }

    public async Task UpdateTopicPostCountAsync(Guid topicId)
    {
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.TopicId == topicId);
        if (topic == null) return;

        var cutoffDate = DateTime.UtcNow.AddDays(-_config.TopicPostCountDays);
        
        // Count total posts mentioning this topic
        topic.PostCount = await _context.Posts
            .CountAsync(p => p.Topic != null && p.Topic.ToLower() == topic.Name && p.Status != PostStatus.Deleted);

        // Count active posts (in last N days)
        topic.ActivePostCount = await _context.Posts
            .CountAsync(p => p.Topic != null && p.Topic.ToLower() == topic.Name && p.Status != PostStatus.Deleted && p.CreatedAt > cutoffDate);

        topic.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Hashtag Operations

    public async Task<Hashtag?> GetHashtagByTagAsync(string tag)
    {
        var normalizedTag = NormalizeTag(tag);
        return await _context.Hashtags
            .Include(h => h.Topic)
            .FirstOrDefaultAsync(h => h.NormalizedTag == normalizedTag);
    }

    public async Task<List<Hashtag>> GetTrendingHashtagsAsync(int count = 20)
    {
        return await _context.Hashtags
            .Where(h => h.IsTrending && h.TodayUsageCount > 0)
            .OrderByDescending(h => h.TodayUsageCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Hashtag>> GetActiveHashtagsAsync()
    {
        return await _context.Hashtags
            .Where(h => h.UsageCount > 0)
            .OrderByDescending(h => h.UsageCount)
            .ToListAsync();
    }

    public async Task<Hashtag> GetOrCreateHashtagAsync(string tag)
    {
        var normalizedTag = NormalizeTag(tag);
        
        var hashtag = await _context.Hashtags
            .FirstOrDefaultAsync(h => h.NormalizedTag == normalizedTag);

        if (hashtag == null)
        {
            // Try to map to a topic
            var mappedTopic = await GetTopicForHashtagAsync(normalizedTag);
            
            hashtag = new Hashtag
            {
                Tag = tag.StartsWith('#') ? tag : $"#{tag}",
                NormalizedTag = normalizedTag,
                TopicId = mappedTopic?.TopicId,
                UsageCount = 0,
                TodayUsageCount = 0,
                IsTrending = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Hashtags.Add(hashtag);
            await _context.SaveChangesAsync();
        }

        return hashtag;
    }

    public async Task<List<string>> ExtractHashtagsAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new List<string>();

        var matches = HashtagRegex.Matches(content);
        return matches
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    public async Task UpdateHashtagUsageAsync(string tag)
    {
        var hashtag = await GetOrCreateHashtagAsync(tag);
        
        hashtag.UsageCount++;
        hashtag.TodayUsageCount++;
        hashtag.UpdatedAt = DateTime.UtcNow;

        // Check if should be marked as trending
        if (hashtag.TodayUsageCount >= _config.MinPostsForTrend && !hashtag.IsTrending)
        {
            hashtag.IsTrending = true;
            hashtag.TrendingSince = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Trend Operations

    public async Task<List<Trend>> GetGlobalTrendsAsync(int count = 10)
    {
        var windowStart = DateTime.UtcNow.AddHours(-_config.TrendWindowHours);
        var cutoffDate = DateTime.UtcNow.AddHours(-_config.TrendWindowHours);

        // Get active trends
        var existingTrends = await _context.Trends
            .Where(t => t.Scope == TrendScope.Global && t.IsActive && t.ExpiresAt > DateTime.UtcNow)
            .Include(t => t.Topic)
            .Include(t => t.Hashtag)
            .OrderByDescending(t => t.Strength)
            .ThenByDescending(t => t.EngagementTotal)
            .Take(count)
            .ToListAsync();

        if (existingTrends.Any())
        {
            return existingTrends;
        }

        // Calculate fresh trends
        return await CalculateAllGlobalTrendsAsync(count);
    }

    public async Task<List<Trend>> GetCommunityTrendsAsync(int communityId, int count = 10)
    {
        var cutoffDate = DateTime.UtcNow.AddHours(-_config.TrendWindowHours);

        // Get posts from this community
        var communityPosts = await _context.Posts
            .Where(p => p.CommunityId == communityId && p.CreatedAt > cutoffDate && p.Status != PostStatus.Deleted)
            .ToListAsync();

        // Extract topics from posts
        var topicCounts = communityPosts
            .Where(p => !string.IsNullOrEmpty(p.Topic))
            .GroupBy(p => p.Topic!.ToLower())
            .Select(g => new { TopicName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .ToList();

        var trends = new List<Trend>();
        
        foreach (var tc in topicCounts)
        {
            var topic = await GetTopicBySlugAsync(tc.TopicName);
            if (topic != null)
            {
                trends.Add(new Trend
                {
                    Type = TrendType.Topic,
                    TopicId = topic.TopicId,
                    Query = tc.TopicName,
                    DisplayName = topic.DisplayName,
                    Slug = topic.Slug,
                    Strength = CalculateCommunityTrendStrength(tc.Count),
                    PostCount = tc.Count,
                    UniquePosters = communityPosts.Count(p => p.Topic?.ToLower() == tc.TopicName),
                    Scope = TrendScope.Community,
                    CommunityId = communityId,
                    CalculatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(_config.TrendDurationHours),
                    IsActive = true
                });
            }
        }

        return trends;
    }

    public async Task<List<Trend>> GetPersonalTrendsAsync(int accountId, int count = 10)
    {
        // Get user's subscribed topics
        var subscribedTopicIds = await _context.TopicSubscriptions
            .Where(s => s.AccountId == accountId)
            .Select(s => s.TopicId)
            .ToListAsync();

        // Get global trends
        var globalTrends = await GetGlobalTrendsAsync(50);

        // Boost trends that match user interests
        var personalTrends = globalTrends
            .Select(t => new
            {
                Trend = t,
                Boost = subscribedTopicIds.Contains(t.TopicId ?? Guid.Empty) ? 1.5 : 1.0
            })
            .Select(x =>
            {
                // Apply boost by adjusting effective strength
                x.Trend.Strength = (TrendStrength)Math.Min(5, (int)x.Trend.Strength * (int)x.Boost);
                return x.Trend;
            })
            .OrderByDescending(t => t.Strength)
            .ThenByDescending(t => t.EngagementTotal)
            .Take(count)
            .ToList();

        return personalTrends;
    }

    public async Task<Trend?> GetTrendByIdAsync(Guid trendId)
    {
        return await _context.Trends
            .Include(t => t.Topic)
            .Include(t => t.Hashtag)
            .FirstOrDefaultAsync(t => t.TrendId == trendId);
    }

    public async Task<Trend> CalculateTrendAsync(string query, TrendScope scope, int? communityId = null)
    {
        var windowStart = DateTime.UtcNow.AddHours(-_config.TrendWindowHours);

        // Get posts mentioning this query
        var posts = await _context.Posts
            .Where(p => p.CreatedAt > windowStart && p.Status != PostStatus.Deleted)
            .Where(p => p.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       (p.Topic != null && p.Topic.ToLower() == query.ToLower()))
            .ToListAsync();

        // Calculate metrics
        var postCount = posts.Count;
        var uniquePosters = posts.Select(p => p.AuthorAccountId).Distinct().Count();
        
        var engagementTotal = posts.Sum(p => 
            _context.PostLikes.Count(l => l.PostId == p.Id) +
            _context.Comments.Count(c => c.PostId == p.Id && c.Status != CommentStatus.Deleted));

        // Calculate velocity
        var velocity = CalculateVelocity(posts, _config.TrendWindowHours);

        // Calculate strength
        var strength = CalculateTrendStrength(postCount, uniquePosters, velocity);

        // Check for existing trend
        var existingTrend = await _context.Trends
            .FirstOrDefaultAsync(t => t.Query.ToLower() == query.ToLower() && t.Scope == scope);

        if (existingTrend != null)
        {
            // Update existing trend
            existingTrend.PostCount = postCount;
            existingTrend.UniquePosters = uniquePosters;
            existingTrend.EngagementTotal = engagementTotal;
            existingTrend.Velocity = velocity;
            existingTrend.Strength = strength;
            existingTrend.CalculatedAt = DateTime.UtcNow;
            existingTrend.ExpiresAt = DateTime.UtcNow.AddHours(_config.TrendDurationHours);
            
            // Check for peak
            if (strength == TrendStrength.Peaking && existingTrend.PeakedAt == null)
            {
                existingTrend.PeakedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return existingTrend;
        }

        // Create new trend
        var trend = new Trend
        {
            Type = TrendType.Topic,
            Query = query,
            DisplayName = query,
            Slug = GenerateSlug(query),
            Strength = strength,
            PostCount = postCount,
            UniquePosters = uniquePosters,
            EngagementTotal = engagementTotal,
            Velocity = velocity,
            Scope = scope,
            CommunityId = communityId,
            CalculatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(_config.TrendDurationHours),
            IsActive = postCount >= _config.MinPostsForTrend
        };

        _context.Trends.Add(trend);
        await _context.SaveChangesAsync();

        return trend;
    }

    public async Task ProcessTrendsTickAsync()
    {
        // 1. Reset daily hashtag counts
        await ResetDailyHashtagCountsAsync();

        // 2. Process new hashtags from recent posts
        await ProcessNewHashtagsAsync();

        // 3. Calculate global trends
        var globalTrends = await CalculateAllGlobalTrendsAsync(_config.MaxTrendingHashtags);

        // 4. Update trend rankings
        await UpdateTrendRankingsAsync(globalTrends);

        // 5. Process cross-community propagation
        await ProcessCrossCommunityPropagationAsync();

        // 6. Expire old trends
        await ExpireOldTrendsAsync();

        _logger?.LogInformation("Trend processing tick completed. Active trends: {Count}", globalTrends.Count);
    }

    #endregion

    #region Trend Propagation

    public async Task ProcessCrossCommunityPropagationAsync()
    {
        var communities = await _communityService.GetAllActiveAsync();
        var cutoffDate = DateTime.UtcNow.AddHours(-_config.TrendWindowHours);

        foreach (var community in communities)
        {
            // Get this community's recent posts
            var localPosts = await _context.Posts
                .Where(p => p.CommunityId == community.Id && p.CreatedAt > cutoffDate && p.Status != PostStatus.Deleted)
                .Select(p => p.Topic)
                .Where(t => t != null)
                .Distinct()
                .ToListAsync();

            // Find connected communities
            var connectedCommunities = await _communityService.GetConnectedCommunitiesAsync(community.Id);

            foreach (var localTopic in localPosts)
            {
                foreach (var connected in connectedCommunities)
                {
                    // Calculate propagation probability
                    var propProb = CalculatePropagationProbability(localTopic!, community.Id, connected.Id);

                    if (Random.Shared.NextDouble() < propProb)
                    {
                        // Record propagation
                        var existingPropagation = await _context.TrendPropagations
                            .AnyAsync(p => p.FromCommunityId == community.Id && 
                                           p.ToCommunityId == connected.Id &&
                                           p.PropagatedAt > DateTime.UtcNow.AddHours(-1));

                        if (!existingPropagation)
                        {
                            var trend = await _context.Trends
                                .FirstOrDefaultAsync(t => t.Query.ToLower() == localTopic!.ToLower() && 
                                                        t.Scope == TrendScope.Global);

                            if (trend != null)
                            {
                                _context.TrendPropagations.Add(new TrendPropagation
                                {
                                    TrendId = trend.TrendId,
                                    FromCommunityId = community.Id,
                                    ToCommunityId = connected.Id,
                                    PropagatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Topic Subscription

    public async Task SubscribeToTopicAsync(int accountId, Guid topicId)
    {
        var existing = await _context.TopicSubscriptions
            .AnyAsync(s => s.AccountId == accountId && s.TopicId == topicId);

        if (!existing)
        {
            _context.TopicSubscriptions.Add(new TopicSubscription
            {
                AccountId = accountId,
                TopicId = topicId,
                SubscribedAt = DateTime.UtcNow
            });

            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.TopicId == topicId);
            if (topic != null)
            {
                topic.SubscriberCount++;
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task UnsubscribeFromTopicAsync(int accountId, Guid topicId)
    {
        var subscription = await _context.TopicSubscriptions
            .FirstOrDefaultAsync(s => s.AccountId == accountId && s.TopicId == topicId);

        if (subscription != null)
        {
            _context.TopicSubscriptions.Remove(subscription);

            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.TopicId == topicId);
            if (topic != null)
            {
                topic.SubscriberCount = Math.Max(0, topic.SubscriberCount - 1);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<bool> IsSubscribedToTopicAsync(int accountId, Guid topicId)
    {
        return await _context.TopicSubscriptions
            .AnyAsync(s => s.AccountId == accountId && s.TopicId == topicId);
    }

    #endregion

    #region Private Helper Methods

    private async Task<List<Trend>> CalculateAllGlobalTrendsAsync(int count)
    {
        var cutoffDate = DateTime.UtcNow.AddHours(-_config.TrendWindowHours);

        // Get topics with recent activity
        var activeTopics = await _context.Topics
            .Where(t => t.IsActive)
            .ToListAsync();

        // Get hashtags with recent activity
        var activeHashtags = await _context.Hashtags
            .Where(h => h.TodayUsageCount > 0)
            .ToListAsync();

        var trends = new List<Trend>();

        // Calculate trends for topics
        foreach (var topic in activeTopics)
        {
            var posts = await _context.Posts
                .Where(p => p.Topic != null && p.Topic.ToLower() == topic.Name.ToLower() && 
                           p.CreatedAt > cutoffDate && p.Status != PostStatus.Deleted)
                .ToListAsync();

            if (posts.Count >= _config.MinPostsForTrend)
            {
                var uniquePosters = posts.Select(p => p.AuthorAccountId).Distinct().Count();
                var engagement = posts.Sum(p => 
                    _context.PostLikes.Count(l => l.PostId == p.Id) +
                    _context.Comments.Count(c => c.PostId == p.Id && c.Status != CommentStatus.Deleted));
                var velocity = CalculateVelocity(posts, _config.TrendWindowHours);

                trends.Add(new Trend
                {
                    Type = TrendType.Topic,
                    TopicId = topic.TopicId,
                    Query = topic.Name,
                    DisplayName = topic.DisplayName,
                    Slug = topic.Slug,
                    Strength = CalculateTrendStrength(posts.Count, uniquePosters, velocity),
                    PostCount = posts.Count,
                    UniquePosters = uniquePosters,
                    EngagementTotal = engagement,
                    Velocity = velocity,
                    Scope = TrendScope.Global,
                    CalculatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(_config.TrendDurationHours),
                    IsActive = true
                });
            }
        }

        // Calculate trends for hashtags
        foreach (var hashtag in activeHashtags)
        {
            var posts = await _context.Posts
                .Where(p => p.CreatedAt > cutoffDate && p.Status != PostStatus.Deleted &&
                           p.Content.Contains($"#{hashtag.NormalizedTag}", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            if (posts.Count >= _config.MinPostsForTrend)
            {
                var uniquePosters = posts.Select(p => p.AuthorAccountId).Distinct().Count();
                var engagement = posts.Sum(p => 
                    _context.PostLikes.Count(l => l.PostId == p.Id) +
                    _context.Comments.Count(c => c.PostId == p.Id && c.Status != CommentStatus.Deleted));
                var velocity = CalculateVelocity(posts, _config.TrendWindowHours);

                trends.Add(new Trend
                {
                    Type = TrendType.Hashtag,
                    HashtagId = hashtag.HashtagId,
                    Query = hashtag.Tag,
                    DisplayName = hashtag.Tag,
                    Slug = hashtag.NormalizedTag,
                    Strength = CalculateTrendStrength(posts.Count, uniquePosters, velocity),
                    PostCount = posts.Count,
                    UniquePosters = uniquePosters,
                    EngagementTotal = engagement,
                    Velocity = velocity,
                    Scope = TrendScope.Global,
                    CalculatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(_config.TrendDurationHours),
                    IsActive = true
                });
            }
        }

        // Sort by strength and limit
        var sortedTrends = trends
            .OrderByDescending(t => t.Strength)
            .ThenByDescending(t => t.EngagementTotal)
            .Take(count)
            .ToList();

        // Save to database
        foreach (var trend in sortedTrends)
        {
            var existing = await _context.Trends
                .FirstOrDefaultAsync(t => t.Query.ToLower() == trend.Query.ToLower() && t.Scope == TrendScope.Global);

            if (existing != null)
            {
                existing.Strength = trend.Strength;
                existing.PostCount = trend.PostCount;
                existing.UniquePosters = trend.UniquePosters;
                existing.EngagementTotal = trend.EngagementTotal;
                existing.Velocity = trend.Velocity;
                existing.CalculatedAt = DateTime.UtcNow;
                existing.ExpiresAt = trend.ExpiresAt;
            }
            else
            {
                _context.Trends.Add(trend);
            }
        }

        await _context.SaveChangesAsync();
        return sortedTrends;
    }

    private float CalculateVelocity(List<Post> posts, int windowHours)
    {
        if (posts.Count < 2) return 0;

        var cutoff = DateTime.UtcNow.AddHours(-windowHours);
        var relevantPosts = posts.Where(p => p.CreatedAt > cutoff).ToList();
        
        if (relevantPosts.Count < 2) return 0;

        // Group by hour
        var hourlyCounts = relevantPosts
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day, p.CreatedAt.Hour })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .ThenBy(g => g.Key.Day)
            .ThenBy(g => g.Key.Hour)
            .Select(g => g.Count())
            .ToList();

        if (hourlyCounts.Count < 2) return 0;

        // Calculate trend line slope
        var n = hourlyCounts.Count;
        var xMean = (n - 1) / 2.0;
        var yMean = hourlyCounts.Average();

        var numerator = 0.0;
        var denominator = 0.0;

        for (int i = 0; i < n; i++)
        {
            numerator += (i - xMean) * (hourlyCounts[i] - yMean);
            denominator += Math.Pow(i - xMean, 2);
        }

        var slope = denominator != 0 ? numerator / denominator : 0;
        return (float)(slope > 0 ? slope : 0);
    }

    private TrendStrength CalculateTrendStrength(int postCount, int uniquePosters, float velocity)
    {
        // Count score
        var countScore = postCount switch
        {
            < 10 => 0,
            < 50 => 1,
            < 200 => 2,
            < 500 => 3,
            < 1000 => 4,
            _ => 5
        };

        // Poster score
        var posterScore = uniquePosters switch
        {
            < 5 => 0,
            < 20 => 1,
            < 50 => 2,
            < 100 => 3,
            < 200 => 4,
            _ => 5
        };

        // Velocity score
        var velocityScore = velocity switch
        {
            < 0.5f => 0,
            < 1.0f => 1,
            < 2.0f => 2,
            < 5.0f => 3,
            < 10.0f => 4,
            _ => 5
        };

        // Weighted average
        var totalScore = (countScore * 0.4) + (posterScore * 0.3) + (velocityScore * 0.3);

        return totalScore switch
        {
            < 1 => TrendStrength.Emerging,
            < 2 => TrendStrength.Growing,
            < 3 => TrendStrength.Hot,
            < 4 => TrendStrength.Viral,
            _ => TrendStrength.Peaking
        };
    }

    private TrendStrength CalculateCommunityTrendStrength(int postCount)
    {
        return postCount switch
        {
            < 5 => TrendStrength.Emerging,
            < 20 => TrendStrength.Growing,
            < 50 => TrendStrength.Hot,
            < 100 => TrendStrength.Viral,
            _ => TrendStrength.Peaking
        };
    }

    private double CalculatePropagationProbability(string topic, int fromCommunityId, int toCommunityId)
    {
        var baseProb = 0.1 * _config.PropagationMultiplier;

        // Get trend for this topic
        var trend = _context.Trends
            .FirstOrDefault(t => t.Query.ToLower() == topic.ToLower() && t.Scope == TrendScope.Global);

        if (trend != null)
        {
            // More engagement = higher propagation
            var engagementBoost = Math.Min(0.3, trend.EngagementTotal / 10000.0);

            // Stronger trends propagate more
            var strengthBoost = ((int)trend.Strength - 1) * 0.05;

            return Math.Min(0.9, baseProb + engagementBoost + strengthBoost);
        }

        return baseProb;
    }

    private async Task ResetDailyHashtagCountsAsync()
    {
        // Reset trending status for hashtags below threshold
        var trendingHashtags = await _context.Hashtags
            .Where(h => h.IsTrending && h.TodayUsageCount < _config.MinPostsForTrend)
            .ToListAsync();

        foreach (var hashtag in trendingHashtags)
        {
            hashtag.IsTrending = false;
            hashtag.TrendingSince = null;
        }

        // Reset daily counts at midnight (only if new day)
        var lastReset = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "LastHashtagReset");

        if (lastReset == null || lastReset.ValueDate?.Date != DateTime.UtcNow.Date)
        {
            var allHashtags = await _context.Hashtags.ToListAsync();
            foreach (var hashtag in allHashtags)
            {
                hashtag.TodayUsageCount = 0;
            }

            if (lastReset == null)
            {
                _context.AppSettings.Add(new AppSetting { Key = "LastHashtagReset", ValueDate = DateTime.UtcNow });
            }
            else
            {
                lastReset.ValueDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task ProcessNewHashtagsAsync()
    {
        var recentPosts = await _context.Posts
            .Where(p => p.CreatedAt > DateTime.UtcNow.AddHours(-1) && p.Status != PostStatus.Deleted)
            .Select(p => p.Content)
            .ToListAsync();

        var allHashtags = new HashSet<string>();

        foreach (var content in recentPosts)
        {
            var extracted = await ExtractHashtagsAsync(content);
            foreach (var tag in extracted)
            {
                if (!allHashtags.Contains(tag))
                {
                    allHashtags.Add(tag);
                    await GetOrCreateHashtagAsync(tag);
                }
            }
        }
    }

    private async Task UpdateTrendRankingsAsync(List<Trend> trends)
    {
        var sortedTrends = trends.OrderByDescending(t => t.Strength).ThenByDescending(t => t.EngagementTotal).ToList();

        for (int i = 0; i < sortedTrends.Count; i++)
        {
            sortedTrends[i].Rank = i + 1;
        }

        await _context.SaveChangesAsync();
    }

    private async Task ExpireOldTrendsAsync()
    {
        var expiredTrends = await _context.Trends
            .Where(t => t.ExpiresAt < DateTime.UtcNow && t.IsActive)
            .ToListAsync();

        foreach (var trend in expiredTrends)
        {
            trend.IsActive = false;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Topic?> GetTopicForHashtagAsync(string hashtag)
    {
        // Try exact match
        var exactMatch = await GetTopicBySlugAsync(hashtag);
        if (exactMatch != null) return exactMatch;

        // Try partial match
        var partialMatch = await _context.Topics
            .Where(t => t.Name.Contains(hashtag) || hashtag.Contains(t.Name))
            .FirstOrDefaultAsync();

        return partialMatch;
    }

    private static string NormalizeTag(string tag)
    {
        return tag.TrimStart('#').ToLowerInvariant();
    }

    private static string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
        
        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple hyphens
        slug = Regex.Replace(slug, @"-+", "-");
        
        return slug.Trim('-');
    }

    #endregion
}
