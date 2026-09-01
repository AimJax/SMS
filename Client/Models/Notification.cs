namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Notification model for API responses
/// </summary>
public class Notification
{
    public int Id { get; set; }
    public Guid NotificationId { get; set; }
    public int AccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? FromAccountId { get; set; }
    public string? FromUsername { get; set; }
    public Guid? RelatedPostId { get; set; }
    public Guid? RelatedCommentId { get; set; }
    public int? RelatedAccountId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
