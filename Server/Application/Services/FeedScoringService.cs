using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for scoring feed candidates based on multiple factors
/// </summary>
public class FeedScoringService : IFeedScoringService
{
    private readonly AppDbContext _context;
    private readonly FeedScoringConfig _config;
    private static readonly ConcurrentDictionary<string, (DateTime Timestamp, double Score)> _scoreCache = new();

    public FeedScoringService(AppDbContext context, FeedScoringConfig config)
    {
        _context = context;
        _config = config;
    }

    /// <inheritdoc />
    public double CalculateRecencyScore(DateTime postCreatedAt)
    {
        var hoursSincePost = (DateTime.UtcNow - postCreatedAt).TotalHours;
        
        // Posts under 1 hour get near-perfect score
        if (hoursSincePost < 1)
        {
            return 1.0 - (hoursSincePost / 60.0);
        }
        
        // Posts over 24 hours get floor score
        if (hoursSincePost > 24)
        {
            hoursSincePost = 24;
        }
        
        // Exponential decay: base_decay ^ (hours / half_life)
        var decayFactor = hoursSincePost / _config.RecencyHalfLifeHours;
        return Math.Pow(_config.RecencyBaseDecay, decayFactor);
    }

    /// <inheritdoc />
    public double CalculateInterestScore(string? postTopic, IEnumerable<string> accountInterests)
    {
        if (string.IsNullOrWhiteSpace(postTopic))
        {
            return 0.0;
        }
        
        var interests = accountInterests.Select(i => i.ToLowerInvariant()).ToHashSet();
        var postTopics = postTopic.ToLowerInvariant()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();
        
        // Binary matching: if any topic matches, score is 1.0
        return postTopics.Any(t => interests.Contains(t)) ? 1.0 : 0.0;
    }

    /// <inheritdoc />
    public double CalculateRelationshipScore(int viewerId, int authorId, bool isFollowing, 
        double? familiarity = null, double? friendship = null, double? trust = null, double? admiration = null)
    {
        // If not following, use baseline
        if (!isFollowing)
        {
            return _config.UnknownAccountBaseline;
        }
        
        // If following but no explicit relationship data, use baseline for followers
        if (familiarity == null && friendship == null && trust == null && admiration == null)
        {
            return _config.FollowedAccountBaseline;
        }
        
        // Use the strongest relationship dimension
        var maxRelationship = new[] { familiarity, friendship, trust, admiration }
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .DefaultIfEmpty(_config.FollowedAccountBaseline)
            .Max();
        
        return Math.Min(1.0, maxRelationship / 100.0);
    }

    /// <inheritdoc />
    public double CalculateEngagementScore(int likeCount, int commentCount, int repostCount,
        int recentLikeCount = 0, int recentCommentCount = 0, int recentRepostCount = 0)
    {
        var totalEngagement = likeCount + commentCount + repostCount;
        var recentEngagement = recentLikeCount + recentCommentCount + recentRepostCount;
        
        // Logarithmic normalization to prevent viral posts from dominating
        var normalizedEngagement = Math.Min(1.0, 
            Math.Log(1 + totalEngagement) / Math.Log(_config.MaxExpectedEngagement));
        
        // Velocity bonus: high recent engagement relative to total = trending
        double velocityBonus = 0;
        if (totalEngagement > 0)
        {
            var velocityRatio = (double)recentEngagement / totalEngagement;
            velocityBonus = velocityRatio * _config.VelocityBonusMultiplier;
        }
        
        return Math.Min(1.0, normalizedEngagement + velocityBonus);
    }

    /// <inheritdoc />
    public double CalculateCommunityAffinityScore(int? postCommunityId, IEnumerable<int> viewerCommunityIds)
    {
        if (!postCommunityId.HasValue)
        {
            return 0.0;
        }
        
        return viewerCommunityIds.Contains(postCommunityId.Value) 
            ? _config.CommunityMemberAffinityScore 
            : 0.0;
    }

    /// <inheritdoc />
    public double CalculateAuthorFameScore(double? fame, double? influence)
    {
        // If no fame data, return neutral score
        if (!fame.HasValue)
        {
            return 0.5;
        }
        
        // Normalize to 0-1 scale (assuming max 100)
        var normalized = Math.Min(1.0, fame.Value / 100.0);
        
        // Center around 0.5 with slight boost for famous users
        return 0.5 + (normalized * 0.5);
    }

    /// <inheritdoc />
    public double CalculateDiscoveryScore(bool isFollowing, bool hasSeenBefore)
    {
        if (isFollowing)
        {
            return 0.0;
        }
        
        return hasSeenBefore ? _config.SeenDiscoveryScore : _config.NewDiscoveryScore;
    }

    /// <inheritdoc />
    public double CalculateFinalScore(FeedScoreBreakdown breakdown)
    {
        return (breakdown.RecencyScore * _config.RecencyWeight)
             + (breakdown.InterestScore * _config.InterestWeight)
             + (breakdown.RelationshipScore * _config.RelationshipWeight)
             + (breakdown.EngagementScore * _config.EngagementWeight)
             + (breakdown.CommunityScore * _config.CommunityWeight)
             + (breakdown.FameScore * _config.FameWeight)
             + (breakdown.DiscoveryScore * _config.DiscoveryWeight);
    }

    /// <inheritdoc />
    public FeedScoreBreakdown CalculateScoreBreakdown(
        Post post,
        Account author,
        IEnumerable<string> accountInterests,
        IEnumerable<int> viewerCommunityIds,
        bool isFollowing,
        bool hasSeenAuthorBefore,
        int? familiarity = null,
        int? friendship = null,
        int? trust = null,
        int? admiration = null)
    {
        // Get engagement counts
        var likeCount = post.Likes?.Count ?? 0;
        var commentCount = post.Comments?.Count ?? 0;
        
        // Calculate recent engagement (last 1 hour)
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentLikeCount = post.Likes?.Count(l => l.CreatedAt >= oneHourAgo) ?? 0;
        var recentCommentCount = post.Comments?.Count(c => c.CreatedAt >= oneHourAgo) ?? 0;
        
        // Get NPC profile data for fame (based on account type)
        double? fame = null;
        if (author.NpcProfile != null || author.AccountType != AccountType.OrdinaryUser)
        {
            // Calculate fame based on account type
            fame = author.AccountType switch
            {
                AccountType.Celebrity => 80,
                AccountType.Influencer => 60,
                AccountType.Creator => 40,
                AccountType.News => 50,
                AccountType.Official => 45,
                _ => 20
            };
        }
        
        return new FeedScoreBreakdown
        {
            RecencyScore = CalculateRecencyScore(post.CreatedAt),
            InterestScore = CalculateInterestScore(post.Topic, accountInterests),
            RelationshipScore = CalculateRelationshipScore(
                0, // viewerId not needed for this calculation
                author.Id,
                isFollowing,
                familiarity,
                friendship,
                trust,
                admiration),
            EngagementScore = CalculateEngagementScore(likeCount, commentCount, 0, recentLikeCount, recentCommentCount),
            CommunityScore = CalculateCommunityAffinityScore(post.CommunityId, viewerCommunityIds),
            FameScore = CalculateAuthorFameScore(fame, null),
            DiscoveryScore = CalculateDiscoveryScore(isFollowing, hasSeenAuthorBefore)
        };
    }

    /// <inheritdoc />
    public ScoredFeedItem ScorePost(
        Post post,
        Account author,
        Profile? profile,
        IEnumerable<string> accountInterests,
        IEnumerable<int> viewerCommunityIds,
        bool isFollowing,
        bool hasSeenAuthorBefore,
        bool isLikedByViewer,
        int? familiarity = null,
        int? friendship = null,
        int? trust = null,
        int? admiration = null)
    {
        var breakdown = CalculateScoreBreakdown(
            post, author, accountInterests, viewerCommunityIds, 
            isFollowing, hasSeenAuthorBefore, 
            familiarity, friendship, trust, admiration);
        
        breakdown.FinalScore = CalculateFinalScore(breakdown);
        
        return new ScoredFeedItem
        {
            Post = post,
            AuthorAccount = author,
            AuthorProfile = profile,
            LikeCount = post.Likes?.Count ?? 0,
            CommentCount = post.Comments?.Count ?? 0,
            IsLikedByCurrentUser = isLikedByViewer,
            CommunityId = post.CommunityId,
            FinalScore = breakdown.FinalScore,
            ScoreBreakdown = breakdown
        };
    }

    /// <inheritdoc />
    public IEnumerable<ScoredFeedItem> ScorePosts(
        IEnumerable<Post> posts,
        int viewerId,
        IEnumerable<string> accountInterests,
        IEnumerable<int> viewerCommunityIds,
        ILookup<int, bool> followingLookup,
        ILookup<int, bool> seenAuthorsLookup,
        ILookup<int, int> likeCountsLookup,
        ILookup<int, int> commentCountsLookup,
        ILookup<int, bool> likedPostsLookup)
    {
        var interestsList = accountInterests.ToList();
        var communityIdsList = viewerCommunityIds.ToList();
        
        foreach (var post in posts)
        {
            var authorId = post.AuthorAccountId;
            var isFollowing = followingLookup.Contains(authorId) && followingLookup[authorId].Any();
            var hasSeenAuthor = seenAuthorsLookup.Contains(authorId) && seenAuthorsLookup[authorId].Any();
            var likeCount = likeCountsLookup.Contains(authorId) ? likeCountsLookup[authorId].First() : 0;
            var commentCount = commentCountsLookup.Contains(authorId) ? commentCountsLookup[authorId].First() : 0;
            var isLiked = likedPostsLookup.Contains(post.Id) && likedPostsLookup[post.Id].Any();
            
            var author = post.AuthorAccount ?? throw new InvalidOperationException("Post must have AuthorAccount loaded");
            var profile = author.Profile;
            
            yield return ScorePost(
                post,
                author,
                profile,
                interestsList,
                communityIdsList,
                isFollowing,
                hasSeenAuthor,
                isLiked);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ScoredFeedItem> ApplyEchoChamberAdjustment(
        IEnumerable<ScoredFeedItem> items,
        double echoChamberStrength)
    {
        // Adjust discovery score weight based on echo chamber strength
        var adjustedDiscoveryWeight = _config.DiscoveryWeight * (1.0 - echoChamberStrength);
        var adjustedRelationshipWeight = _config.RelationshipWeight * (1.0 + echoChamberStrength * 0.5);
        
        foreach (var item in items)
        {
            // Recalculate with adjusted weights
            var adjustedScore = (item.ScoreBreakdown.RecencyScore * _config.RecencyWeight)
                              + (item.ScoreBreakdown.InterestScore * _config.InterestWeight)
                              + (item.ScoreBreakdown.RelationshipScore * adjustedRelationshipWeight)
                              + (item.ScoreBreakdown.EngagementScore * _config.EngagementWeight)
                              + (item.ScoreBreakdown.CommunityScore * _config.CommunityWeight)
                              + (item.ScoreBreakdown.FameScore * _config.FameWeight)
                              + (item.ScoreBreakdown.DiscoveryScore * adjustedDiscoveryWeight);
            
            item.FinalScore = adjustedScore;
            yield return item;
        }
    }

    /// <inheritdoc />
    public IEnumerable<ScoredFeedItem> EnforceDiscoveryQuota(
        IEnumerable<ScoredFeedItem> items,
        int pageSize)
    {
        var minDiscoveryCount = Math.Max(1, (int)(pageSize * _config.MinDiscoveryPercentage));
        var itemsList = items.ToList();
        
        if (itemsList.Count <= pageSize)
        {
            return itemsList.Take(pageSize);
        }
        
        var discoveryItems = itemsList
            .Where(i => i.ScoreBreakdown.DiscoveryScore > 0)
            .OrderByDescending(i => i.ScoreBreakdown.DiscoveryScore)
            .Take(minDiscoveryCount)
            .ToList();
        
        var otherItems = itemsList
            .Where(i => i.ScoreBreakdown.DiscoveryScore == 0)
            .OrderByDescending(i => i.FinalScore)
            .Take(pageSize - minDiscoveryCount)
            .ToList();
        
        return discoveryItems.Concat(otherItems)
            .OrderByDescending(i => i.FinalScore)
            .Take(pageSize);
    }
}

/// <summary>
/// Interface for feed scoring operations
/// </summary>
public interface IFeedScoringService
{
    /// <summary>
    /// Calculate recency score based on post age
    /// </summary>
    double CalculateRecencyScore(DateTime postCreatedAt);
    
    /// <summary>
    /// Calculate interest match score
    /// </summary>
    double CalculateInterestScore(string? postTopic, IEnumerable<string> accountInterests);
    
    /// <summary>
    /// Calculate relationship affinity score
    /// </summary>
    double CalculateRelationshipScore(int viewerId, int authorId, bool isFollowing,
        double? familiarity = null, double? friendship = null, double? trust = null, double? admiration = null);
    
    /// <summary>
    /// Calculate engagement score
    /// </summary>
    double CalculateEngagementScore(int likeCount, int commentCount, int repostCount,
        int recentLikeCount = 0, int recentCommentCount = 0, int recentRepostCount = 0);
    
    /// <summary>
    /// Calculate community affinity score
    /// </summary>
    double CalculateCommunityAffinityScore(int? postCommunityId, IEnumerable<int> viewerCommunityIds);
    
    /// <summary>
    /// Calculate author fame score
    /// </summary>
    double CalculateAuthorFameScore(double? fame, double? influence);
    
    /// <summary>
    /// Calculate discovery score
    /// </summary>
    double CalculateDiscoveryScore(bool isFollowing, bool hasSeenBefore);
    
    /// <summary>
    /// Calculate final weighted score
    /// </summary>
    double CalculateFinalScore(FeedScoreBreakdown breakdown);
    
    /// <summary>
    /// Calculate complete score breakdown
    /// </summary>
    FeedScoreBreakdown CalculateScoreBreakdown(
        Post post,
        Account author,
        IEnumerable<string> accountInterests,
        IEnumerable<int> viewerCommunityIds,
        bool isFollowing,
        bool hasSeenAuthorBefore,
        int? familiarity = null,
        int? friendship = null,
        int? trust = null,
        int? admiration = null);
    
    /// <summary>
    /// Score a single post
    /// </summary>
    ScoredFeedItem ScorePost(
        Post post,
        Account author,
        Profile? profile,
        IEnumerable<string> accountInterests,
        IEnumerable<int> viewerCommunityIds,
        bool isFollowing,
        bool hasSeenAuthorBefore,
        bool isLikedByViewer,
        int? familiarity = null,
        int? friendship = null,
        int? trust = null,
        int? admiration = null);
    
    /// <summary>
    /// Score multiple posts efficiently
    /// </summary>
    IEnumerable<ScoredFeedItem> ScorePosts(
        IEnumerable<Post> posts,
        int viewerId,
        IEnumerable<string> accountInterests,
        IEnumerable<int> viewerCommunityIds,
        ILookup<int, bool> followingLookup,
        ILookup<int, bool> seenAuthorsLookup,
        ILookup<int, int> likeCountsLookup,
        ILookup<int, int> commentCountsLookup,
        ILookup<int, bool> likedPostsLookup);
    
    /// <summary>
    /// Apply echo chamber adjustment to scores
    /// </summary>
    IEnumerable<ScoredFeedItem> ApplyEchoChamberAdjustment(
        IEnumerable<ScoredFeedItem> items,
        double echoChamberStrength);
    
    /// <summary>
    /// Enforce minimum discovery content quota
    /// </summary>
    IEnumerable<ScoredFeedItem> EnforceDiscoveryQuota(
        IEnumerable<ScoredFeedItem> items,
        int pageSize);
}
