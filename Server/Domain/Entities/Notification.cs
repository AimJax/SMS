namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Someone followed the recipient's account
    /// </summary>
    Follow = 0,
    
    /// <summary>
    /// Someone liked one of the recipient's posts
    /// </summary>
    Like = 1,
    
    /// <summary>
    /// Someone commented on one of the recipient's posts
    /// </summary>
    Comment = 2,
    
    /// <summary>
    /// One of the recipient's posts went viral
    /// </summary>
    ViralPost = 3,
    
    /// <summary>
    /// General mention notification
    /// </summary>
    Mention = 4
}

/// <summary>
/// Represents a notification sent to an account.
/// Notifications are persistent (never auto-deleted) and track read/unread state.
/// </summary>
public class Notification
{
    /// <summary>
    /// Unique notification identifier (stable GUID identity per project convention)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The account that receives this notification
    /// </summary>
    public int RecipientAccountId { get; set; }
    
    /// <summary>
    /// The account that caused this notification (may be NPC or player)
    /// Set to 0 for system notifications
    /// </summary>
    public int ActorAccountId { get; set; }
    
    /// <summary>
    /// The type of notification
    /// </summary>
    public NotificationType Type { get; set; }
    
    /// <summary>
    /// Notification title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Notification content/message
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// The ID of the related entity (Follow, PostLike, or Comment)
    /// </summary>
    public int RelatedEntityId { get; set; }
    
    /// <summary>
    /// The post involved as GUID (for ViralPost, Mention notifications)
    /// </summary>
    public Guid? RelatedPostGuid { get; set; }
    
    /// <summary>
    /// The post ID as integer (for Like and Comment notifications)
    /// </summary>
    public int? RelatedPostId { get; set; }
    
    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Whether the notification has been read
    /// </summary>
    public bool IsRead { get; set; }
    
    /// <summary>
    /// When the notification was read (null if unread)
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    // Navigation properties
    public Account? RecipientAccount { get; set; }
    public Account? ActorAccount { get; set; }
    public Post? RelatedPost { get; set; }
}
