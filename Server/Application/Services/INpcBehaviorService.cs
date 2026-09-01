using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for NPC behavior execution
/// </summary>
public interface INpcBehaviorService
{
    /// <summary>
    /// Process a single NPC's behavior for a simulation tick
    /// </summary>
    Task<NpcActionResult?> ProcessBehaviorAsync(NpcProfile npc, NpcBehaviorConfig? config = null);
    
    /// <summary>
    /// Generate candidate actions for an NPC
    /// </summary>
    Task<IEnumerable<NpcActionCandidate>> GenerateCandidatesAsync(NpcProfile npc, NpcBehaviorConfig config);
    
    /// <summary>
    /// Check if an NPC can perform a follow action on a target
    /// </summary>
    Task<bool> CanFollowAsync(int npcAccountId, int targetAccountId);
    
    /// <summary>
    /// Check if an NPC can like a post
    /// </summary>
    Task<bool> CanLikeAsync(int npcAccountId, int postId);
    
    /// <summary>
    /// Get recent posts for potential engagement
    /// </summary>
    Task<IEnumerable<Post>> GetRecentPostsAsync(NpcProfile npc, int limit, int hoursBack);
    
    /// <summary>
    /// Get candidate accounts for following
    /// </summary>
    Task<IEnumerable<Account>> GetCandidateAccountsAsync(NpcProfile npc, int limit);
}
