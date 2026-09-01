using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class NpcSimulationService : INpcSimulationService
{
    private readonly AppDbContext _context;
    private readonly INpcService _npcService;
    private readonly INpcBehaviorService? _behaviorService;
    private readonly NpcBehaviorConfig _behaviorConfig;

    public NpcSimulationService(
        AppDbContext context, 
        INpcService npcService,
        INpcBehaviorService? behaviorService = null,
        NpcBehaviorConfig? behaviorConfig = null)
    {
        _context = context;
        _npcService = npcService;
        _behaviorService = behaviorService;
        _behaviorConfig = behaviorConfig ?? new NpcBehaviorConfig();
    }

    public async Task<IEnumerable<NpcProfile>> GetDueNpcsAsync(int limit = 100)
    {
        var now = DateTime.UtcNow;
        
        // Query NPCs that are:
        // 1. Active
        // 2. Due for simulation (NextSimulationAt <= now)
        var dueNpcs = await _context.NpcProfiles
            .Include(n => n.Account)
            .Where(n => n.IsActive)
            .Where(n => n.NextSimulationAt <= now)
            .OrderBy(n => n.NextSimulationAt)
            .Take(limit)
            .ToListAsync();
        
        // Filter for active accounts
        return dueNpcs.Where(n => 
            n.Account != null && 
            n.Account.Status == AccountStatus.Active);
    }

    public async Task ProcessNpcAsync(Guid npcId)
    {
        var npc = await _context.NpcProfiles
            .Include(n => n.Account)
            .Include(n => n.Personality)
            .Include(n => n.Interests)
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
        
        // Process behavior if service is available
        if (_behaviorService != null)
        {
            // Set activity state based on action
            npc.ActivityState = NpcActivityState.Engaging;
            
            var result = await _behaviorService.ProcessBehaviorAsync(npc, _behaviorConfig);
            
            // Update activity state based on result
            if (result != null)
            {
                npc.ActivityState = result.ActionType switch
                {
                    NpcActionType.ViewFeed or NpcActionType.ViewPost or NpcActionType.Search => NpcActivityState.Reading,
                    NpcActionType.CreatePost => NpcActivityState.Posting,
                    NpcActionType.Follow or NpcActionType.Unfollow or NpcActionType.LikePost or 
                        NpcActionType.UnlikePost or NpcActionType.Comment => NpcActivityState.Engaging,
                    _ => NpcActivityState.Browsing
                };
            }
        }
        else
        {
            // No behavior service - just browse
            npc.ActivityState = NpcActivityState.Browsing;
        }
        
        // Update simulation state
        npc.LastSimulatedAt = DateTime.UtcNow;
        npc.NextSimulationAt = DateTime.UtcNow.AddSeconds(npc.SimulationIntervalSeconds);
        npc.SimulationVersion++;
        npc.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }

    public async Task<SimulationTickResult> ProcessTickAsync(int maxBatchSize = 100)
    {
        var dueNpcs = await GetDueNpcsAsync(maxBatchSize);
        var npcList = dueNpcs.ToList();
        
        int followsCreated = 0;
        int unfollowsCreated = 0;
        
        foreach (var npc in npcList)
        {
            // Track follow/unfollow actions before processing
            var existingFollowCount = await _context.Follows
                .CountAsync(f => f.FollowerAccountId == npc.AccountId);
            
            await ProcessNpcAsync(npc.NpcId);
            
            // Track follow/unfollow changes after processing
            var newFollowCount = await _context.Follows
                .CountAsync(f => f.FollowerAccountId == npc.AccountId);
            
            if (newFollowCount > existingFollowCount)
                followsCreated += (newFollowCount - existingFollowCount);
            else if (newFollowCount < existingFollowCount)
                unfollowsCreated += (existingFollowCount - newFollowCount);
        }
        
        return new SimulationTickResult(
            npcList.Count,
            0,
            followsCreated,
            unfollowsCreated,
            DateTime.UtcNow);
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
