using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for creating and managing NPC accounts
/// </summary>
public interface INpcService
{
    /// <summary>
    /// Create a new NPC with all associated data
    /// </summary>
    Task<NpcProfile> CreateNpcAsync(string username, string? displayName, string? bio, AccountType accountType);
    
    /// <summary>
    /// Get NPC profile by NpcId
    /// </summary>
    Task<NpcProfile?> GetByNpcIdAsync(Guid npcId);
    
    /// <summary>
    /// Get NPC profile by AccountId
    /// </summary>
    Task<NpcProfile?> GetByAccountIdAsync(int accountId);
    
    /// <summary>
    /// Check if an account is an NPC
    /// </summary>
    Task<bool> IsNpcAsync(int accountId);
    
    /// <summary>
    /// Check if an account is an NPC by AccountId (GUID)
    /// </summary>
    Task<bool> IsNpcByAccountIdAsync(Guid accountId);
    
    /// <summary>
    /// Deactivate an NPC
    /// </summary>
    Task<bool> DeactivateAsync(Guid npcId);
    
    /// <summary>
    /// Activate an NPC
    /// </summary>
    Task<bool> ActivateAsync(Guid npcId);
    
    /// <summary>
    /// Generate deterministic personality based on seed
    /// </summary>
    NpcPersonality GeneratePersonality(Guid seed);
    
    /// <summary>
    /// Generate interests based on account type and seed
    /// </summary>
    IEnumerable<NpcInterest> GenerateInterests(AccountType accountType, Guid seed);
}
