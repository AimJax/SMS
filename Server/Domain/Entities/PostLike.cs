namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Represents a like on a post
/// </summary>
public class PostLike
{
    public int Id { get; set; }
    
    /// <summary>
    /// The post that was liked
    /// </summary>
    public int PostId { get; set; }
    
    /// <summary>
    /// The account that liked the post
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// When the like was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Post? Post { get; set; }
    public Account? Account { get; set; }
}
