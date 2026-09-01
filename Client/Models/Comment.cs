namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Comment model for API responses
/// </summary>
public class Comment
{
    public int Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public int AuthorAccountId { get; set; }
    public string? AuthorUsername { get; set; }
    public string? AuthorDisplayName { get; set; }
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public bool IsLiked { get; set; }
    public DateTime CreatedAt { get; set; }
}
