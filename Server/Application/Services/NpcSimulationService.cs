using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class NpcSimulationService : INpcSimulationService
{
    private readonly AppDbContext _context;
    private readonly INpcService _npcService;

    public NpcSimulationService(AppDbContext context, INpcService npcService)
    {
        _context = context;
        _npcService = npcService;
    }

    public async Task<IEnumerable<NpcProfile>> GetDueNpcsAsync(int limit = 100)
    {
        var now = DateTime.UtcNow;
        
        // Query NPCs that are:
        // 1. Active
        // 2. Due for simulation (NextSimulationAt <= now)
        // Then filter for active accounts separately to avoid Include issues
        var dueNpcs = await _context.NpcProfiles
            .Where(n => n.IsActive)
            .Where(n => n.NextSimulationAt <= now)
            .OrderBy(n => n.NextSimulationAt)
            .Take(limit)
            .ToListAsync();
        
        // Filter out NPCs whose accounts are not active
        return dueNpcs.Where(n => 
            n.Account != null && 
            n.Account.Status == AccountStatus.Active);
    }

    public async Task ProcessNpcAsync(Guid npcId)
    {
        var npc = await _context.NpcProfiles
            .Include(n => n.Account)
            .FirstOrDefaultAsync(n => n.NpcId == npcId);
        
        if (npc == null || !npc.IsActive)
        {
            return;
        }
        
        // Check account status
        if (npc.Account?.Status != AccountStatus.Active)
        {
            return;
        }
        
        // Update simulation state
        npc.LastSimulatedAt = DateTime.UtcNow;
        npc.NextSimulationAt = DateTime.UtcNow.AddSeconds(npc.SimulationIntervalSeconds);
        npc.SimulationVersion++;
        npc.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }

    public async Task<int> ProcessTickAsync(int maxBatchSize = 100)
    {
        var dueNpcs = await GetDueNpcsAsync(maxBatchSize);
        var npcList = dueNpcs.ToList();
        
        foreach (var npc in npcList)
        {
            await ProcessNpcAsync(npc.NpcId);
        }
        
        return npcList.Count;
    }

    public async Task UpdateNpcAfterSimulationAsync(int npcProfileId, NpcActivityState newState)
    {
        var npc = await _context.NpcProfiles.FindAsync(npcProfileId);
        
        if (npc == null || !npc.IsActive)
        {
            return;
        }
        
        npc.ActivityState = newState;
        npc.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
}
