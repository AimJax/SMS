using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for creating and managing notifications.
/// Notifications are created through a single consistent mechanism to ensure
/// both player-caused and NPC-caused events are handled uniformly.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a follow notification.
    /// Called from SocialGraphService.FollowAsync after the follow is created.
    /// </summary>
    Task NotifyFollowAsync(int followId, int followerAccountId, int followedAccountId);
    
    /// <summary>
    /// Creates a like notification.
    /// Called from PostService.LikePostAsync after the like is created.
    /// </summary>
    Task NotifyLikeAsync(int postLikeId, int likerAccountId, int postAuthorAccountId, int postId);
    
    /// <summary>
    /// Creates a comment notification.
    /// Called from PostService.CreateCommentAsync after the comment is created.
    /// </summary>
    Task NotifyCommentAsync(int commentId, int commenterAccountId, int postAuthorAccountId, int postId);
    
    /// <summary>
    /// Gets notifications for a recipient with cursor-based pagination.
    /// </summary>
    Task<(IEnumerable<Notification> Items, string? NextCursor)> GetNotificationsAsync(
        int recipientAccountId, 
        string? cursor = null, 
        int pageSize = 20);
    
    /// <summary>
    /// Gets the count of unread notifications for a recipient.
    /// </summary>
    Task<int> GetUnreadCountAsync(int recipientAccountId);
    
    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    Task<bool> MarkAsReadAsync(int recipientAccountId, Guid notificationId);
    
    /// <summary>
    /// Marks all notifications for a recipient as read.
    /// </summary>
    Task<int> MarkAllAsReadAsync(int recipientAccountId);
    
    /// <summary>
    /// Gets a notification by ID (for ownership verification).
    /// </summary>
    Task<Notification?> GetByIdAsync(Guid notificationId);
}
