using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcPopulationPerformanceTests : IDisposable
{
    private readonly string _dbName;
    private readonly AppDbContext _context;
    private readonly NpcService _npcService;
    private readonly NpcPopulationService _populationService;

    public NpcPopulationPerformanceTests()
    {
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new AppDbContext(options);
        _npcService = new NpcService(_context);
        _populationService = new NpcPopulationService(_context, _npcService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GeneratePopulation_1000Npcs_Performance()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 1000,
            RandomSeed = 123456
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _populationService.GeneratePopulationAsync(config);
        stopwatch.Stop();

        // Assert
        Assert.True(result.Success, $"Generation failed: {result.ErrorMessage}");
        Assert.Equal(1000, result.NpcsCreated);
        Assert.Equal(0, result.NpcsFailed);
        
        Console.WriteLine($"[PERF] Generated 1000 NPCs in {stopwatch.Elapsed.TotalSeconds:F2}s ({stopwatch.ElapsedMilliseconds}ms)");
        Console.WriteLine($"[PERF] Seed used: {result.SeedUsed}");
        Console.WriteLine($"[PERF] Distribution: {string.Join(", ", result.Distribution.Select(kv => $"{kv.Key}={kv.Value}"))}");
        
        // Performance assertion: Should complete in reasonable time
        Assert.True(stopwatch.Elapsed.TotalSeconds < 300, 
            $"Generation took too long: {stopwatch.Elapsed.TotalSeconds:F2}s");
    }

    [Fact]
    public async Task GeneratePopulation_100Npcs_Performance()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 100,
            RandomSeed = 789012
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _populationService.GeneratePopulationAsync(config);
        stopwatch.Stop();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(100, result.NpcsCreated);
        
        Console.WriteLine($"[PERF] Generated 100 NPCs in {stopwatch.Elapsed.TotalMilliseconds}ms");
        
        // 100 NPCs should be very fast
        Assert.True(stopwatch.Elapsed.TotalSeconds < 60);
    }

    [Fact]
    public async Task GeneratePopulation_10Npcs_Performance()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 10,
            RandomSeed = 345678
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _populationService.GeneratePopulationAsync(config);
        stopwatch.Stop();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10, result.NpcsCreated);
        
        Console.WriteLine($"[PERF] Generated 10 NPCs in {stopwatch.Elapsed.TotalMilliseconds}ms");
        
        // 10 NPCs should be nearly instant
        Assert.True(stopwatch.Elapsed.TotalMilliseconds < 10000);
    }
}
