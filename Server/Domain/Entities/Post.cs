namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Post status enumeration
/// </summary>
public enum PostStatus
{
    Active = 0,
    Deleted = 1
}

/// <summary>
/// Core post entity
/// </summary>
public class Post
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable internal identifier - never changes
    /// </summary>
    public Guid PostId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The account that authored this post (references Account.Id)
    /// </summary>
    public int AuthorAccountId { get; set; }
    
    /// <summary>
    /// The content of the post
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Post status for soft delete
    /// </summary>
    public PostStatus Status { get; set; } = PostStatus.Active;
    
    /// <summary>
    /// When the post was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the post was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? AuthorAccount { get; set; }
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
