namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Configuration for NPC behavior system
/// </summary>
public class NpcBehaviorConfig
{
    /// <summary>
    /// Maximum number of candidates to consider for each action type
    /// </summary>
    public int MaxCandidateAccounts { get; set; } = 50;
    
    /// <summary>
    /// Maximum posts to consider for engagement
    /// </summary>
    public int MaxCandidatePosts { get; set; } = 30;
    
    /// <summary>
    /// Base probability of any action occurring
    /// </summary>
    public double BaseActionProbability { get; set; } = 0.7;
    
    /// <summary>
    /// Cooldown in seconds between posts
    /// </summary>
    public int PostCooldownSeconds { get; set; } = 300; // 5 minutes
    
    /// <summary>
    /// Maximum follows per simulation tick
    /// </summary>
    public int MaxFollowsPerTick { get; set; } = 2;
    
    /// <summary>
    /// Maximum likes per simulation tick
    /// </summary>
    public int MaxLikesPerTick { get; set; } = 5;
    
    /// <summary>
    /// Maximum comments per simulation tick
    /// </summary>
    public int MaxCommentsPerTick { get; set; } = 3;
    
    /// <summary>
    /// Maximum unfollows per simulation tick
    /// </summary>
    public int MaxUnfollowsPerTick { get; set; } = 1;
    
    /// <summary>
    /// Time window in hours to look for recent posts
    /// </summary>
    public int RecentPostsHours { get; set; } = 24;
    
    /// <summary>
    /// Maximum following before considering unfollows
    /// </summary>
    public int MaxFollowingBeforeUnfollow { get; set; } = 200;
    
    /// <summary>
    /// Random seed for deterministic behavior (null = random)
    /// </summary>
    public int? RandomSeed { get; set; }
    
    /// <summary>
    /// Whether to enable exploration (following new accounts)
    /// </summary>
    public bool EnableExploration { get; set; } = true;
    
    /// <summary>
    /// Exploration rate (0.0 - 1.0)
    /// </summary>
    public double ExplorationRate { get; set; } = 0.3;
}
