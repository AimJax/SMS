using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcSimulationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NpcService _npcService;
    private readonly NpcSimulationService _simulationService;

    public NpcSimulationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new AppDbContext(options);
        _npcService = new NpcService(_context);
        _simulationService = new NpcSimulationService(_context, _npcService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetDueNpcsAsync_ReturnsOnlyDueActiveNpcs()
    {
        // Arrange - Create NPCs and make them due for simulation
        var npc1 = await _npcService.CreateNpcAsync("DueNpc1", null, null, AccountType.Creator);
        var npc2 = await _npcService.CreateNpcAsync("DueNpc2", null, null, AccountType.Celebrity);
        
        // Make npc1 and npc2 due now (reset NextSimulationAt)
        npc1.NextSimulationAt = DateTime.UtcNow.AddSeconds(-5);
        npc2.NextSimulationAt = DateTime.UtcNow.AddSeconds(-5);
        await _context.SaveChangesAsync();
        
        // NPC3 is not due yet (NextSimulationAt in future)
        var npc3 = await _npcService.CreateNpcAsync("FutureNpc3", null, null, AccountType.Creator);

        // Act
        var dueNpcs = await _simulationService.GetDueNpcsAsync();

        // Assert - Only npc1 and npc2 should be due
        var dueList = dueNpcs.ToList();
        Assert.Equal(2, dueList.Count);
        Assert.Contains(dueList, n => n.NpcId == npc1.NpcId);
        Assert.Contains(dueList, n => n.NpcId == npc2.NpcId);
        Assert.DoesNotContain(dueList, n => n.NpcId == npc3.NpcId);
    }

    [Fact]
    public async Task GetDueNpcsAsync_ExcludesInactiveNpcs()
    {
        // Arrange
        var npc1 = await _npcService.CreateNpcAsync("ActiveNpc1", null, null, AccountType.Creator);
        var npc2 = await _npcService.CreateNpcAsync("InactiveNpc2", null, null, AccountType.Creator);
        
        // Make npc1 due for simulation
        npc1.NextSimulationAt = DateTime.UtcNow.AddSeconds(-5);
        await _context.SaveChangesAsync();
        
        // Deactivate npc2
        await _npcService.DeactivateAsync(npc2.NpcId);

        // Act
        var dueNpcs = await _simulationService.GetDueNpcsAsync();

        // Assert
        var dueList = dueNpcs.ToList();
        Assert.Single(dueList);
        Assert.Equal(npc1.NpcId, dueList[0].NpcId);
    }

    [Fact]
    public async Task ProcessNpcAsync_UpdatesLastSimulatedAtAndNextSimulationAt()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("ProcessTestNpc", null, null, AccountType.Celebrity);
        var originalLastSimulated = npc.LastSimulatedAt;
        var originalNextSimulation = npc.NextSimulationAt;

        // Act
        await _simulationService.ProcessNpcAsync(npc.NpcId);

        // Assert
        var updated = await _npcService.GetByNpcIdAsync(npc.NpcId);
        Assert.NotNull(updated!.LastSimulatedAt);
        Assert.True(updated.NextSimulationAt > originalNextSimulation);
        Assert.Equal(2, updated.SimulationVersion);
    }

    [Fact]
    public async Task ProcessNpcAsync_DoesNotProcessInactiveNpc()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("InactiveProcessTest", null, null, AccountType.Creator);
        await _npcService.DeactivateAsync(npc.NpcId);
        var originalVersion = npc.SimulationVersion;

        // Act
        await _simulationService.ProcessNpcAsync(npc.NpcId);

        // Assert
        var updated = await _npcService.GetByNpcIdAsync(npc.NpcId);
        Assert.Equal(originalVersion, updated!.SimulationVersion);
    }

    [Fact]
    public async Task ProcessTickAsync_ProcessesAllDueNpcs()
    {
        // Arrange - Create NPCs and make them due for simulation
        var npc1 = await _npcService.CreateNpcAsync("TickNpc1", null, null, AccountType.Creator);
        var npc2 = await _npcService.CreateNpcAsync("TickNpc2", null, null, AccountType.Celebrity);
        var npc3 = await _npcService.CreateNpcAsync("TickNpc3", null, null, AccountType.Influencer);
        
        // Make all due for simulation
        npc1.NextSimulationAt = DateTime.UtcNow.AddSeconds(-5);
        npc2.NextSimulationAt = DateTime.UtcNow.AddSeconds(-5);
        npc3.NextSimulationAt = DateTime.UtcNow.AddSeconds(-5);
        await _context.SaveChangesAsync();

        // Act
        var result = await _simulationService.ProcessTickAsync(10);

        // Assert
        Assert.Equal(3, result.NpcsProcessed);
        
        // Verify all were processed
        var allNpcs = await _simulationService.GetDueNpcsAsync(10);
        Assert.Empty(allNpcs.ToList());
    }

    [Fact]
    public async Task ProcessTickAsync_RespectsMaxBatchSize()
    {
        // Arrange - Create 5 NPCs and make them due
        for (int i = 0; i < 5; i++)
        {
            var npc = await _npcService.CreateNpcAsync($"BatchNpc{i}", null, null, AccountType.Creator);
            npc.NextSimulationAt = DateTime.UtcNow.AddSeconds(-5);
        }
        await _context.SaveChangesAsync();

        // Act - Process only 3
        var result = await _simulationService.ProcessTickAsync(3);

        // Assert
        Assert.Equal(3, result.NpcsProcessed);
        
        // Verify 2 remain due
        var remaining = await _simulationService.GetDueNpcsAsync(10);
        Assert.Equal(2, remaining.Count());
    }

    [Fact]
    public async Task UpdateNpcAfterSimulationAsync_UpdatesActivityState()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("StateUpdateNpc", null, null, AccountType.Creator);
        Assert.Equal(NpcActivityState.Idle, npc.ActivityState);

        // Act
        await _simulationService.UpdateNpcAfterSimulationAsync(npc.Id, NpcActivityState.Posting);

        // Assert
        var updated = await _npcService.GetByNpcIdAsync(npc.NpcId);
        Assert.Equal(NpcActivityState.Posting, updated!.ActivityState);
    }

    [Fact]
    public async Task ProcessTickAsync_RespectsAccountStatus()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("StatusTestNpc", null, null, AccountType.Creator);
        
        // Suspend the account
        var account = await _context.Accounts.FindAsync(npc.AccountId);
        account!.Status = AccountStatus.Suspended;
        await _context.SaveChangesAsync();

        // Act
        var dueNpcs = await _simulationService.GetDueNpcsAsync();

        // Assert - Should not be in due list
        Assert.Empty(dueNpcs.ToList());
    }

    [Fact]
    public async Task ProcessNpcAsync_DoesNotProcessNonExistentNpc()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act - Should not throw
        await _simulationService.ProcessNpcAsync(nonExistentId);

        // Assert - No error means success
    }

    [Fact]
    public async Task UpdateNpcAfterSimulationAsync_DoesNotUpdateInactiveNpc()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("InactiveStateTest", null, null, AccountType.Creator);
        await _npcService.DeactivateAsync(npc.NpcId);
        
        // Act
        await _simulationService.UpdateNpcAfterSimulationAsync(npc.Id, NpcActivityState.Browsing);

        // Assert - State should remain Idle
        var updated = await _npcService.GetByNpcIdAsync(npc.NpcId);
        Assert.Equal(NpcActivityState.Idle, updated!.ActivityState);
    }
}
