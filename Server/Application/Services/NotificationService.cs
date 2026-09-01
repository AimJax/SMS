using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Implementation of INotificationService.
/// 
/// Key design decisions:
/// - Self-notification suppression: No notification when actor == recipient
/// - Block suppression: No notification if either party has blocked the other
/// - Mute suppression: No notification if recipient has muted actor (consistent with feed visibility)
/// - Failure isolation: Notification creation failures are logged but don't block the triggering action
/// - Notifications are created AFTER the triggering action commits (fire-and-forget pattern)
/// </summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    
    // Constants for cursor-based pagination
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task NotifyFollowAsync(int followId, int followerAccountId, int followedAccountId)
    {
        // Self-notification suppression
        if (followerAccountId == followedAccountId)
        {
            _logger.LogDebug("Follow notification suppressed: actor and recipient are the same account");
            return;
        }

        try
        {
            // Block suppression - check if either party has blocked the other
            var isBlocked = await IsBlockedEitherDirectionAsync(followerAccountId, followedAccountId);
            if (isBlocked)
            {
                _logger.LogDebug("Follow notification suppressed: block relationship exists between {Actor} and {Recipient}",
                    followerAccountId, followedAccountId);
                return;
            }

            // Mute suppression - check if recipient has muted actor
            var isMuted = await IsMutedAsync(followedAccountId, followerAccountId);
            if (isMuted)
            {
                _logger.LogDebug("Follow notification suppressed: {Recipient} has muted {Actor}",
                    followedAccountId, followerAccountId);
                return;
            }

            var notification = new Notification
            {
                RecipientAccountId = followedAccountId,
                ActorAccountId = followerAccountId,
                Type = NotificationType.Follow,
                RelatedEntityId = followId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Created follow notification: {Actor} -> {Recipient}",
                followerAccountId, followedAccountId);
        }
        catch (Exception ex)
        {
            // Log but don't rethrow - failure isolation
            _logger.LogError(ex, "Failed to create follow notification for {Actor} -> {Recipient}. Error: {Error}",
                followerAccountId, followedAccountId, ex.Message);
        }
    }

    public async Task NotifyLikeAsync(int postLikeId, int likerAccountId, int postAuthorAccountId, int postId)
    {
        // Self-notification suppression
        if (likerAccountId == postAuthorAccountId)
        {
            _logger.LogDebug("Like notification suppressed: actor and recipient are the same account");
            return;
        }

        try
        {
            // Block suppression - check if either party has blocked the other
            var isBlocked = await IsBlockedEitherDirectionAsync(likerAccountId, postAuthorAccountId);
            if (isBlocked)
            {
                _logger.LogDebug("Like notification suppressed: block relationship exists between {Actor} and {Recipient}",
                    likerAccountId, postAuthorAccountId);
                return;
            }

            // Mute suppression - check if recipient has muted actor
            var isMuted = await IsMutedAsync(postAuthorAccountId, likerAccountId);
            if (isMuted)
            {
                _logger.LogDebug("Like notification suppressed: {Recipient} has muted {Actor}",
                    postAuthorAccountId, likerAccountId);
                return;
            }

            var notification = new Notification
            {
                RecipientAccountId = postAuthorAccountId,
                ActorAccountId = likerAccountId,
                Type = NotificationType.Like,
                RelatedEntityId = postLikeId,
                RelatedPostId = postId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Created like notification: {Actor} liked post {PostId} owned by {Recipient}",
                likerAccountId, postId, postAuthorAccountId);
        }
        catch (Exception ex)
        {
            // Log but don't rethrow - failure isolation
            _logger.LogError(ex, "Failed to create like notification for {Actor} on post {PostId}. Error: {Error}",
                likerAccountId, postId, ex.Message);
        }
    }

    public async Task NotifyCommentAsync(int commentId, int commenterAccountId, int postAuthorAccountId, int postId)
    {
        // Self-notification suppression
        if (commenterAccountId == postAuthorAccountId)
        {
            _logger.LogDebug("Comment notification suppressed: actor and recipient are the same account");
            return;
        }

        try
        {
            // Block suppression - check if either party has blocked the other
            var isBlocked = await IsBlockedEitherDirectionAsync(commenterAccountId, postAuthorAccountId);
            if (isBlocked)
            {
                _logger.LogDebug("Comment notification suppressed: block relationship exists between {Actor} and {Recipient}",
                    commenterAccountId, postAuthorAccountId);
                return;
            }

            // Mute suppression - check if recipient has muted actor
            var isMuted = await IsMutedAsync(postAuthorAccountId, commenterAccountId);
            if (isMuted)
            {
                _logger.LogDebug("Comment notification suppressed: {Recipient} has muted {Actor}",
                    postAuthorAccountId, commenterAccountId);
                return;
            }

            var notification = new Notification
            {
                RecipientAccountId = postAuthorAccountId,
                ActorAccountId = commenterAccountId,
                Type = NotificationType.Comment,
                RelatedEntityId = commentId,
                RelatedPostId = postId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Created comment notification: {Actor} commented on post {PostId} owned by {Recipient}",
                commenterAccountId, postId, postAuthorAccountId);
        }
        catch (Exception ex)
        {
            // Log but don't rethrow - failure isolation
            _logger.LogError(ex, "Failed to create comment notification for {Actor} on post {PostId}. Error: {Error}",
                commenterAccountId, postId, ex.Message);
        }
    }

    public async Task<(IEnumerable<Notification> Items, string? NextCursor)> GetNotificationsAsync(
        int recipientAccountId, 
        string? cursor = null, 
        int pageSize = DefaultPageSize)
    {
        // Clamp page size
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        // Parse cursor if provided (format: "timestamp_notificationId")
        DateTime? cursorTimestamp = null;
        Guid? cursorId = null;

        if (!string.IsNullOrEmpty(cursor))
        {
            var parts = cursor.Split('_');
            if (parts.Length == 2)
            {
                if (DateTime.TryParse(parts[0], out var ts))
                {
                    cursorTimestamp = ts;
                }
                if (Guid.TryParse(parts[1], out var id))
                {
                    cursorId = id;
                }
            }
        }

        // Build query
        var query = _context.Notifications
            .Include(n => n.ActorAccount)
                .ThenInclude(a => a!.Profile)
            .Where(n => n.RecipientAccountId == recipientAccountId);

        // Apply cursor-based pagination (consistent with FeedService)
        if (cursorTimestamp.HasValue && cursorId.HasValue)
        {
            query = query.Where(n => 
                n.CreatedAt < cursorTimestamp.Value ||
                (n.CreatedAt == cursorTimestamp.Value && n.Id.CompareTo(cursorId.Value) < 0));
        }

        // Order by CreatedAt DESC, then by Id DESC for deterministic ordering
        query = query.OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id);

        // Take one extra to determine if there's a next page
        var notifications = await query.Take(pageSize + 1).ToListAsync();

        // Determine next cursor
        string? nextCursor = null;
        if (notifications.Count > pageSize)
        {
            var lastNotification = notifications[pageSize - 1];
            nextCursor = $"{lastNotification.CreatedAt:O}_{lastNotification.Id}";
            notifications = notifications.Take(pageSize).ToList();
        }

        return (notifications, nextCursor);
    }

    public async Task<int> GetUnreadCountAsync(int recipientAccountId)
    {
        // Efficient COUNT query with index support
        return await _context.Notifications
            .CountAsync(n => n.RecipientAccountId == recipientAccountId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(int recipientAccountId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientAccountId == recipientAccountId);

        if (notification == null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(int recipientAccountId)
    {
        var now = DateTime.UtcNow;
        
        // Get unread notifications for this recipient
        var unreadNotifications = await _context.Notifications
            .Where(n => n.RecipientAccountId == recipientAccountId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }
        
        await _context.SaveChangesAsync();
        
        return unreadNotifications.Count;
    }

    public async Task<Notification?> GetByIdAsync(Guid notificationId)
    {
        return await _context.Notifications
            .Include(n => n.ActorAccount)
                .ThenInclude(a => a!.Profile)
            .Include(n => n.RelatedPost)
            .FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    #region Private Helper Methods

    /// <summary>
    /// Checks if there's a block relationship in either direction between two accounts.
    /// This is consistent with existing block-check logic in SocialGraphService.
    /// </summary>
    private async Task<bool> IsBlockedEitherDirectionAsync(int accountId1, int accountId2)
    {
        return await _context.Blocks
            .AnyAsync(b => 
                (b.BlockerAccountId == accountId1 && b.BlockedAccountId == accountId2) ||
                (b.BlockerAccountId == accountId2 && b.BlockedAccountId == accountId1));
    }

    /// <summary>
    /// Checks if the potential recipient has muted the actor.
    /// This is consistent with mute suppression in FeedService.
    /// </summary>
    private async Task<bool> IsMutedAsync(int recipientAccountId, int actorAccountId)
    {
        return await _context.Mutes
            .AnyAsync(m => m.MuterAccountId == recipientAccountId && m.MutedAccountId == actorAccountId);
    }

    #endregion
}
