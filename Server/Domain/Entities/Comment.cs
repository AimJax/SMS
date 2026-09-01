namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Comment status enumeration
/// </summary>
public enum CommentStatus
{
    Active = 0,
    Deleted = 1
}

/// <summary>
/// Represents a comment on a post
/// </summary>
public class Comment
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable internal identifier
    /// </summary>
    public Guid CommentId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The post this comment belongs to
    /// </summary>
    public int PostId { get; set; }
    
    /// <summary>
    /// The account that authored this comment
    /// </summary>
    public int AuthorAccountId { get; set; }
    
    /// <summary>
    /// The content of the comment
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Comment status for soft delete
    /// </summary>
    public CommentStatus Status { get; set; } = CommentStatus.Active;
    
    /// <summary>
    /// When the comment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the comment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Post? Post { get; set; }
    public Account? AuthorAccount { get; set; }
}
