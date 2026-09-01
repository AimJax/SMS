using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _context;
    private readonly INotificationService? _notificationService;

    public PostService(AppDbContext context, INotificationService? notificationService = null)
    {
        _context = context;
        _notificationService = notificationService;
    }

    #region Post Operations

    public async Task<Post?> CreatePostAsync(int authorAccountId, string content)
    {
        // Validate content
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Post content cannot be empty");
        }

        if (content.Length > 10000)
        {
            throw new InvalidOperationException("Post content exceeds maximum length of 10000 characters");
        }

        // Verify account exists and is active
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == authorAccountId);

        if (account == null)
        {
            throw new InvalidOperationException("Account not found");
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException("Account is not allowed to post");
        }

        var post = new Post
        {
            PostId = Guid.NewGuid(),
            AuthorAccountId = authorAccountId,
            Content = content,
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return post;
    }

    public async Task<Post?> GetPostByIdAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.AuthorAccount)
                .ThenInclude(a => a.Profile)
            .FirstOrDefaultAsync(p => p.PostId == postId && p.Status == PostStatus.Active);
    }

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.AuthorAccount)
                .ThenInclude(a => a.Profile)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == PostStatus.Active);
    }

    public async Task<bool> DeletePostAsync(int accountId, Guid postId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId && p.Status == PostStatus.Active);

        if (post == null)
        {
            return false;
        }

        // Verify ownership
        if (post.AuthorAccountId != accountId)
        {
            throw new InvalidOperationException("Cannot delete another user's post");
        }

        // Soft delete
        post.Status = PostStatus.Deleted;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Like Operations

    public async Task<PostLike?> LikePostAsync(int accountId, Guid postId)
    {
        // Verify post exists and is active
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId && p.Status == PostStatus.Active);

        if (post == null)
        {
            throw new InvalidOperationException("Post not found");
        }

        // Check if already liked
        var existingLike = await _context.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == post.Id && l.AccountId == accountId);

        if (existingLike != null)
        {
            throw new InvalidOperationException("Post already liked");
        }

        var like = new PostLike
        {
            PostId = post.Id,
            AccountId = accountId,
            CreatedAt = DateTime.UtcNow
        };

        _context.PostLikes.Add(like);
        await _context.SaveChangesAsync();

        // Create notification (fire-and-forget pattern for non-blocking notification creation)
        if (_notificationService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.NotifyLikeAsync(like.Id, accountId, post.AuthorAccountId, post.Id);
                }
                catch
                {
                    // Notification service already logs failures internally; swallow exception
                }
            });
        }

        return like;
    }

    public async Task<bool> UnlikePostAsync(int accountId, Guid postId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId && p.Status == PostStatus.Active);

        if (post == null)
        {
            return false;
        }

        var like = await _context.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == post.Id && l.AccountId == accountId);

        if (like == null)
        {
            return false;
        }

        _context.PostLikes.Remove(like);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsPostLikedByAccountAsync(int accountId, Guid postId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null)
        {
            return false;
        }

        return await _context.PostLikes
            .AnyAsync(l => l.PostId == post.Id && l.AccountId == accountId);
    }

    #endregion

    #region Comment Operations

    public async Task<Comment?> CreateCommentAsync(int authorAccountId, Guid postId, string content)
    {
        // Validate content
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Comment content cannot be empty");
        }

        if (content.Length > 2000)
        {
            throw new InvalidOperationException("Comment content exceeds maximum length of 2000 characters");
        }

        // Verify post exists and is active
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId && p.Status == PostStatus.Active);

        if (post == null)
        {
            throw new InvalidOperationException("Post not found");
        }

        // Verify account exists and is active
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == authorAccountId);

        if (account == null)
        {
            throw new InvalidOperationException("Account not found");
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException("Account is not allowed to comment");
        }

        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            PostId = post.Id,
            AuthorAccountId = authorAccountId,
            Content = content,
            Status = CommentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Create notification (fire-and-forget pattern for non-blocking notification creation)
        if (_notificationService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.NotifyCommentAsync(comment.Id, authorAccountId, post.AuthorAccountId, post.Id);
                }
                catch
                {
                    // Notification service already logs failures internally; swallow exception
                }
            });
        }

        return comment;
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
    {
        return await _context.Comments
            .Include(c => c.AuthorAccount)
                .ThenInclude(a => a.Profile)
            .FirstOrDefaultAsync(c => c.CommentId == commentId && c.Status == CommentStatus.Active);
    }

    public async Task<bool> DeleteCommentAsync(int accountId, Guid commentId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId && c.Status == CommentStatus.Active);

        if (comment == null)
        {
            return false;
        }

        // Verify ownership
        if (comment.AuthorAccountId != accountId)
        {
            throw new InvalidOperationException("Cannot delete another user's comment");
        }

        // Soft delete
        comment.Status = CommentStatus.Deleted;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Engagement Counts

    public async Task<int> GetLikeCountAsync(Guid postId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null)
        {
            return 0;
        }

        return await _context.PostLikes.CountAsync(l => l.PostId == post.Id);
    }

    public async Task<int> GetCommentCountAsync(Guid postId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null)
        {
            return 0;
        }

        return await _context.Comments.CountAsync(c => c.PostId == post.Id && c.Status == CommentStatus.Active);
    }

    public async Task<(IEnumerable<Post> Items, string? NextCursor)> GetPostsByTopicAsync(string topic, string? cursor = null, int pageSize = 20)
    {
        var query = _context.Posts
            .Include(p => p.AuthorAccount)
                .ThenInclude(a => a.Profile)
            .Where(p => p.Topic != null && p.Topic.ToLower() == topic.ToLower() && p.Status != PostStatus.Deleted)
            .OrderByDescending(p => p.CreatedAt);

        // Apply cursor if provided
        if (!string.IsNullOrEmpty(cursor))
        {
            if (Guid.TryParse(cursor, out var cursorGuid))
            {
                var cursorPost = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == cursorGuid);
                if (cursorPost != null)
                {
                    query = (IOrderedQueryable<Post>)query.Where(p => p.CreatedAt < cursorPost.CreatedAt || (p.CreatedAt == cursorPost.CreatedAt && p.Id < cursorPost.Id));
                }
            }
        }

        var items = await query.Take(pageSize + 1).ToListAsync();
        
        string? nextCursor = null;
        if (items.Count > pageSize)
        {
            items = items.Take(pageSize).ToList();
            nextCursor = items.Last().PostId.ToString();
        }

        return (items, nextCursor);
    }

    public async Task<IEnumerable<Post>> GetRecentPostsAsync(DateTime since, int limit = 100)
    {
        return await _context.Posts
            .Where(p => p.CreatedAt > since && p.Status != PostStatus.Deleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    #endregion

    #region Comment Queries

    public async Task<(IEnumerable<Comment> Items, int TotalCount)> GetCommentsAsync(Guid postId, int page = 1, int pageSize = 20)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null)
        {
            return (Enumerable.Empty<Comment>(), 0);
        }

        var query = _context.Comments
            .Include(c => c.AuthorAccount)
                .ThenInclude(a => a.Profile)
            .Where(c => c.PostId == post.Id && c.Status == CommentStatus.Active)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion
}
