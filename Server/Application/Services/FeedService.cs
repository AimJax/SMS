using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class FeedService : IFeedService
{
    private readonly AppDbContext _context;

    public FeedService(AppDbContext context)
    {
        _context = context;
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
}
