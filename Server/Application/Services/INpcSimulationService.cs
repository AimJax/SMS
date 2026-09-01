using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for simulation tick processing
/// </summary>
public interface INpcSimulationService
{
    /// <summary>
    /// Get all NPCs that are due for simulation
    /// </summary>
    Task<IEnumerable<NpcProfile>> GetDueNpcsAsync(int limit = 100);
    
    /// <summary>
    /// Process simulation tick for a single NPC
    /// </summary>
    Task ProcessNpcAsync(Guid npcId);
    
    /// <summary>
    /// Process simulation tick for all due NPCs
    /// </summary>
    Task<int> ProcessTickAsync(int maxBatchSize = 100);
    
    /// <summary>
    /// Update NPC after simulation
    /// </summary>
    Task UpdateNpcAfterSimulationAsync(int npcProfileId, NpcActivityState newState);
}

/// <summary>
/// Result of processing a simulation tick
/// </summary>
public record SimulationTickResult(
    int NpcsProcessed,
    int NpcsSkipped,
    DateTime ProcessedAt
);
