using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Represents a candidate action an NPC can perform
/// </summary>
public class NpcActionCandidate
{
    /// <summary>
    /// Type of action
    /// </summary>
    public NpcActionType ActionType { get; set; }
    
    /// <summary>
    /// Target account ID if applicable
    /// </summary>
    public int? TargetAccountId { get; set; }
    
    /// <summary>
    /// Target post ID if applicable
    /// </summary>
    public int? TargetPostId { get; set; }
    
    /// <summary>
    /// Target comment ID if applicable
    /// </summary>
    public int? TargetCommentId { get; set; }
    
    /// <summary>
    /// Pre-computed base score (0.0 - 1.0)
    /// </summary>
    public double BaseScore { get; set; }
    
    /// <summary>
    /// Why this action is being considered
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Content for post/comment actions
    /// </summary>
    public string? GeneratedContent { get; set; }
}

/// <summary>
/// Result of evaluating and selecting an action
/// </summary>
public class NpcActionDecision
{
    /// <summary>
    /// Whether an action was selected
    /// </summary>
    public bool HasAction { get; set; }
    
    /// <summary>
    /// Selected action (if any)
    /// </summary>
    public NpcActionCandidate? SelectedAction { get; set; }
    
    /// <summary>
    /// All evaluated candidates with scores
    /// </summary>
    public List<NpcActionCandidate> Candidates { get; set; } = new();
    
    /// <summary>
    /// Why idle was chosen (if applicable)
    /// </summary>
    public string? IdleReason { get; set; }
}

/// <summary>
/// Result of executing an NPC action
/// </summary>
public class NpcActionResult
{
    /// <summary>
    /// Whether the action was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The action that was attempted
    /// </summary>
    public NpcActionType ActionType { get; set; }
    
    /// <summary>
    /// The NpcAction record ID if created
    /// </summary>
    public int? NpcActionId { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Whether this was a "no-op" rather than a failure
    /// </summary>
    public bool WasSkipped { get; set; }
}
