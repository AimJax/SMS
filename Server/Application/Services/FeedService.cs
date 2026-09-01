using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class FeedService : IFeedService
{
    private readonly AppDbContext _context;
    private readonly IFeedScoringService _scoringService;
    private readonly IFeedCacheService _cacheService;
    private readonly FeedScoringConfig _config;

    public FeedService(
        AppDbContext context, 
        IFeedScoringService scoringService,
        IFeedCacheService cacheService,
        FeedScoringConfig config)
    {
        _context = context;
        _scoringService = scoringService;
        _cacheService = cacheService;
        _config = config;
    }

    public async Task<(IEnumerable<FeedItem> Items, string? NextCursor)> GetFeedAsync(int accountId, string? cursor = null, int pageSize = 20)
    {
        // Parse cursor if provided (format: "timestamp_postId")
        DateTime? cursorTimestamp = null;
        int? cursorPostId = null;
        
        if (!string.IsNullOrEmpty(cursor))
        {
            var parts = cursor.Split('_');
            if (parts.Length == 2)
            {
                if (DateTime.TryParse(parts[0], out var ts))
                {
                    cursorTimestamp = ts;
                }
                if (int.TryParse(parts[1], out var pid))
                {
                    cursorPostId = pid;
                }
            }
        }

        // Get IDs of accounts to exclude (blocked by, blocked, muted)
        var blockedByIds = await _context.Blocks
            .Where(b => b.BlockedAccountId == accountId)
            .Select(b => b.BlockerAccountId)
            .ToListAsync();

        var blockedIds = await _context.Blocks
            .Where(b => b.BlockerAccountId == accountId)
            .Select(b => b.BlockedAccountId)
            .ToListAsync();

        var mutedIds = await _context.Mutes
            .Where(m => m.MuterAccountId == accountId)
            .Select(m => m.MutedAccountId)
            .ToListAsync();

        // Combine excluded IDs
        var excludedIds = blockedByIds.Union(blockedIds).Union(mutedIds).ToHashSet();
        excludedIds.Add(accountId); // Don't include own posts in feed (optional - can be changed)

        // Build the feed query
        var query = _context.Posts
            .Include(p => p.AuthorAccount)
                .ThenInclude(a => a.Profile)
            .Include(p => p.AuthorAccount!.NpcProfile)
            .Where(p => p.Status == PostStatus.Active)
            .Where(p => !excludedIds.Contains(p.AuthorAccountId))
            .Where(p => _context.Follows.Any(f => f.FollowerAccountId == accountId && f.FollowedAccountId == p.AuthorAccountId));

        // Apply cursor-based pagination
        if (cursorTimestamp.HasValue && cursorPostId.HasValue)
        {
            query = query.Where(p => 
                p.CreatedAt < cursorTimestamp.Value ||
                (p.CreatedAt == cursorTimestamp.Value && p.Id < cursorPostId.Value));
        }

        // Order by CreatedAt DESC, then by Id DESC for deterministic ordering
        query = query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

        // Take one extra to determine if there's a next page
        var posts = await query.Take(pageSize + 1).ToListAsync();

        // Determine next cursor
        string? nextCursor = null;
        if (posts.Count > pageSize)
        {
            var lastPost = posts[pageSize - 1];
            nextCursor = $"{lastPost.CreatedAt:O}_{lastPost.Id}";
            posts = posts.Take(pageSize).ToList();
        }

        if (!posts.Any())
        {
            return (Enumerable.Empty<FeedItem>(), null);
        }

        // Get post IDs for batch queries
        var postIds = posts.Select(p => p.Id).ToList();

        // Batch query for like counts
        var likeCounts = await _context.PostLikes
            .Where(l => postIds.Contains(l.PostId))
            .GroupBy(l => l.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);

        // Batch query for comment counts (active comments only)
        var commentCounts = await _context.Comments
            .Where(c => postIds.Contains(c.PostId) && c.Status == CommentStatus.Active)
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);

        // Batch query for current user's likes
        var userLikes = await _context.PostLikes
            .Where(l => l.AccountId == accountId && postIds.Contains(l.PostId))
            .Select(l => l.PostId)
            .ToHashSetAsync();

        // Build feed items
        var feedItems = posts.Select(p => new FeedItem
        {
            Post = p,
            AuthorAccount = p.AuthorAccount!,
            AuthorProfile = p.AuthorAccount?.Profile,
            LikeCount = likeCounts.GetValueOrDefault(p.Id, 0),
            CommentCount = commentCounts.GetValueOrDefault(p.Id, 0),
            IsLikedByCurrentUser = userLikes.Contains(p.Id)
        }).ToList();

        return (feedItems, nextCursor);
    }

    public async Task<AdvancedFeedResponse> GetAdvancedFeedAsync(
        int accountId, 
        string? cursor = null, 
        int pageSize = 20,
        bool includeDiscovery = true,
        double? echoStrength = null)
    {
        // Check cache first
        var cached = await _cacheService.GetCachedFeedAsync(accountId, cursor);
        if (cached != null)
        {
            return cached;
        }

        var effectiveEchoStrength = echoStrength ?? _config.DefaultEchoChamberStrength;
        var effectivePageSize = Math.Min(pageSize, 50); // Max 50 per page

        // Parse cursor for pagination
        int? cursorPostId = null;
        double? cursorScore = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            var parts = cursor.Split('_');
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out var pid))
                {
                    cursorPostId = pid;
                }
                if (double.TryParse(parts[1], out var score))
                {
                    cursorScore = score;
                }
            }
        }

        // Get excluded account IDs
        var blockedByIds = await _context.Blocks
            .Where(b => b.BlockedAccountId == accountId)
            .Select(b => b.BlockerAccountId)
            .ToListAsync();

        var blockedIds = await _context.Blocks
            .Where(b => b.BlockerAccountId == accountId)
            .Select(b => b.BlockedAccountId)
            .ToListAsync();

        var mutedIds = await _context.Mutes
            .Where(m => m.MuterAccountId == accountId)
            .Select(m => m.MutedAccountId)
            .ToListAsync();

        var excludedIds = blockedByIds.Union(blockedIds).Union(mutedIds).ToHashSet();
        excludedIds.Add(accountId);

        // Get followed account IDs
        var followedIds = await _context.Follows
            .Where(f => f.FollowerAccountId == accountId)
            .Select(f => f.FollowedAccountId)
            .ToHashSetAsync();

        // Get viewer's interests
        var accountInterests = await _context.NpcInterests
            .Where(i => i.NpcProfile != null && i.NpcProfile.AccountId == accountId)
            .Select(i => i.InterestKey)
            .ToListAsync();

        // Get viewer's community memberships
        var viewerCommunityIds = await _context.CommunityMemberships
            .Where(m => m.AccountId == accountId && m.IsActive)
            .Select(m => m.CommunityId)
            .ToHashSetAsync();

        // Get seen authors (from impressions)
        var seenAuthorIds = await _context.FeedImpressions
            .Where(f => f.AccountId == accountId)
            .Select(f => f.Post!.AuthorAccountId)
            .Distinct()
            .ToHashSetAsync();

        // Get cutoff time for post history
        var cutoffTime = DateTime.UtcNow.AddHours(-_config.PostHistoryHours);

        // Generate candidate posts
        var candidates = new List<Post>();

        // 1. Posts from followed accounts (highest priority)
        var followedPosts = await _context.Posts
            .Include(p => p.AuthorAccount)
                .ThenInclude(a => a.Profile)
            .Include(p => p.AuthorAccount!.NpcProfile)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Include(p => p.Community)
            .Where(p => p.Status == PostStatus.Active)
            .Where(p => followedIds.Contains(p.AuthorAccountId))
            .Where(p => !excludedIds.Contains(p.AuthorAccountId))
            .Where(p => p.CreatedAt >= cutoffTime)
            .OrderByDescending(p => p.CreatedAt)
            .Take(_config.MaxCandidates)
            .ToListAsync();

        candidates.AddRange(followedPosts);

        // 2. Posts from joined communities (if not already included)
        if (viewerCommunityIds.Count > 0)
        {
            var communityPosts = await _context.Posts
                .Include(p => p.AuthorAccount)
                    .ThenInclude(a => a.Profile)
                .Include(p => p.AuthorAccount!.NpcProfile)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.Community)
                .Where(p => p.Status == PostStatus.Active)
                .Where(p => p.CommunityId != null && viewerCommunityIds.Contains(p.CommunityId.Value))
                .Where(p => !excludedIds.Contains(p.AuthorAccountId))
                .Where(p => !followedIds.Contains(p.AuthorAccountId)) // Prefer followed
                .Where(p => p.CreatedAt >= cutoffTime)
                .OrderByDescending(p => p.CreatedAt)
                .Take(_config.MaxCandidates / 2)
                .ToListAsync();

            candidates.AddRange(communityPosts);
        }

        // 3. Discovery posts (from non-followed accounts with high engagement)
        if (includeDiscovery)
        {
            var discoveryPosts = await _context.Posts
                .Include(p => p.AuthorAccount)
                    .ThenInclude(a => a.Profile)
                .Include(p => p.AuthorAccount!.NpcProfile)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.Community)
                .Where(p => p.Status == PostStatus.Active)
                .Where(p => !followedIds.Contains(p.AuthorAccountId))
                .Where(p => !excludedIds.Contains(p.AuthorAccountId))
                .Where(p => p.CreatedAt >= cutoffTime)
                .OrderByDescending(p => p.Likes!.Count + p.Comments!.Count)
                .Take(_config.MaxCandidates / 4)
                .ToListAsync();

            candidates.AddRange(discoveryPosts);
        }

        // Remove duplicates and limit total candidates
        candidates = candidates
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .Take(_config.MaxCandidates)
            .ToList();

        var totalCandidates = candidates.Count;

        // Build lookup structures for efficient scoring
        var followingLookup = followedIds.ToLookup(id => id, _ => true);
        var seenAuthorsLookup = seenAuthorIds.ToLookup(id => id, _ => true);

        var postIds = candidates.Select(p => p.Id).ToList();

        var likeCounts = await _context.PostLikes
            .Where(l => postIds.Contains(l.PostId))
            .GroupBy(l => l.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);

        var commentCounts = await _context.Comments
            .Where(c => postIds.Contains(c.PostId) && c.Status == CommentStatus.Active)
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count);

        var userLikes = await _context.PostLikes
            .Where(l => l.AccountId == accountId && postIds.Contains(l.PostId))
            .Select(l => l.PostId)
            .ToHashSetAsync();

        // Update posts with engagement counts
        foreach (var post in candidates)
        {
            post.Likes ??= new List<PostLike>();
            post.Comments ??= new List<Comment>();
        }

        // Score all candidates
        var scoredItems = candidates
            .Select(p => ScorePostWithCounts(
                p,
                accountInterests,
                viewerCommunityIds,
                followingLookup,
                seenAuthorsLookup,
                likeCounts,
                commentCounts,
                userLikes))
            .ToList();

        // Apply echo chamber adjustment
        scoredItems = _scoringService.ApplyEchoChamberAdjustment(scoredItems, effectiveEchoStrength).ToList();

        // Sort by score descending
        scoredItems = scoredItems.OrderByDescending(i => i.FinalScore).ToList();

        // Enforce discovery quota
        if (includeDiscovery)
        {
            scoredItems = _scoringService.EnforceDiscoveryQuota(scoredItems, effectivePageSize).ToList();
        }

        // Apply cursor pagination
        if (cursorPostId.HasValue && cursorScore.HasValue)
        {
            scoredItems = scoredItems
                .Where(i => i.Post.Id < cursorPostId.Value || 
                            (i.Post.Id == cursorPostId.Value && i.FinalScore < cursorScore.Value))
                .ToList();
        }

        // Take page size + 1 for next cursor
        var hasMore = scoredItems.Count > effectivePageSize;
        var pageItems = scoredItems.Take(effectivePageSize).ToList();

        // Generate next cursor
        string? nextCursor = null;
        if (hasMore && pageItems.Count > 0)
        {
            var lastItem = pageItems.Last();
            nextCursor = $"{lastItem.Post.Id}_{lastItem.FinalScore:F6}";
        }

        // Build response
        var response = new AdvancedFeedResponse
        {
            Items = pageItems.Select(i => new AdvancedFeedItemResponse
            {
                PostId = i.Post.PostId,
                AuthorAccountId = i.AuthorAccount.AccountId,
                AuthorUsername = i.AuthorAccount.Username,
                AuthorDisplayName = i.AuthorProfile?.DisplayName ?? i.AuthorAccount.Username,
                AuthorAvatarUrl = i.AuthorProfile?.AvatarUrl,
                Content = i.Post.Content,
                CreatedAt = i.Post.CreatedAt,
                LikeCount = i.LikeCount,
                CommentCount = i.CommentCount,
                IsLikedByCurrentUser = i.IsLikedByCurrentUser,
                CommunityId = i.Post.CommunityId,
                CommunitySlug = i.Post.Community?.Slug,
                CommunityName = i.Post.Community?.Name,
                Score = i.FinalScore
            }),
            NextCursor = nextCursor,
            PageSize = effectivePageSize,
            TotalCandidates = totalCandidates
        };

        // Cache the response
        await _cacheService.SetCachedFeedAsync(accountId, cursor, response);

        return response;
    }

    private ScoredFeedItem ScorePostWithCounts(
        Post post,
        List<string> accountInterests,
        HashSet<int> viewerCommunityIds,
        ILookup<int, bool> followingLookup,
        ILookup<int, bool> seenAuthorsLookup,
        Dictionary<int, int> likeCounts,
        Dictionary<int, int> commentCounts,
        HashSet<int> userLikes)
    {
        var authorId = post.AuthorAccountId;
        var isFollowing = followingLookup.Contains(authorId);
        var hasSeenAuthor = seenAuthorsLookup.Contains(authorId);

        var author = post.AuthorAccount ?? throw new InvalidOperationException("Post must have AuthorAccount loaded");
        var profile = author.Profile;

        var likeCount = likeCounts.GetValueOrDefault(post.Id, 0);
        var commentCount = commentCounts.GetValueOrDefault(post.Id, 0);
        var isLiked = userLikes.Contains(post.Id);

        var breakdown = _scoringService.CalculateScoreBreakdown(
            post,
            author,
            accountInterests,
            viewerCommunityIds,
            isFollowing,
            hasSeenAuthor);

        breakdown.FinalScore = _scoringService.CalculateFinalScore(breakdown);

        return new ScoredFeedItem
        {
            Post = post,
            AuthorAccount = author,
            AuthorProfile = profile,
            LikeCount = likeCount,
            CommentCount = commentCount,
            IsLikedByCurrentUser = isLiked,
            CommunityId = post.CommunityId,
            FinalScore = breakdown.FinalScore,
            ScoreBreakdown = breakdown
        };
    }
}
