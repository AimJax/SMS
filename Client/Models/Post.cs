namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Post model for API responses
/// </summary>
public class Post
{
    public int Id { get; set; }
    public Guid PostId { get; set; }
    public int AuthorAccountId { get; set; }
    public string? AuthorUsername { get; set; }
    public string? AuthorDisplayName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int CommunityId { get; set; }
    public string? Topic { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
    public bool IsBookmarked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
