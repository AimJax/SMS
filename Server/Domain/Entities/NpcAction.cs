namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Types of actions an NPC can perform
/// </summary>
public enum NpcActionType
{
    ViewPost = 0,
    LikePost = 1,
    UnlikePost = 2,
    Comment = 3,
    Follow = 4,
    Unfollow = 5,
    Mute = 6,
    Unmute = 7,
    Block = 8,
    CreatePost = 9,
    ViewFeed = 10,
    Search = 11
}

/// <summary>
/// Represents a scheduled or completed NPC action
/// Future parts will expand this with more details
/// </summary>
public class NpcAction
{
    public int Id { get; set; }
    
    /// <summary>
    /// Reference to the NPC profile
    /// </summary>
    public int NpcProfileId { get; set; }
    
    /// <summary>
    /// Type of action
    /// </summary>
    public NpcActionType ActionType { get; set; }
    
    /// <summary>
    /// Target post ID if applicable (GUID stored as string)
    /// </summary>
    public string? TargetPostId { get; set; }
    
    /// <summary>
    /// Target account ID if applicable (GUID stored as string)
    /// </summary>
    public string? TargetAccountId { get; set; }
    
    /// <summary>
    /// Content for post/comment actions
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// Whether the action was executed
    /// </summary>
    public bool Executed { get; set; }
    
    /// <summary>
    /// When the action was scheduled
    /// </summary>
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the action was executed (if applicable)
    /// </summary>
    public DateTime? ExecutedAt { get; set; }
    
    // Navigation property
    public NpcProfile? NpcProfile { get; set; }
}
