using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcPopulationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NpcService _npcService;
    private readonly NpcPopulationService _populationService;

    public NpcPopulationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
    public async Task GeneratePopulationAsync_DefaultConfig_CreatesCorrectCount()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 10,
            RandomSeed = 12345
        };

        // Act
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10, result.NpcsCreated);
        Assert.Equal(0, result.NpcsFailed);
        Assert.NotNull(result.SeedUsed);
    }

    [Fact]
    public async Task GeneratePopulationAsync_CreatesAllNpcData()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 5,
            RandomSeed = 54321
        };

        // Act
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.True(result.Success);
        
        var npcCount = await _context.NpcProfiles.CountAsync();
        var accountCount = await _context.Accounts.CountAsync();
        var profileCount = await _context.Profiles.CountAsync();
        var personalityCount = await _context.NpcPersonalities.CountAsync();
        var interestCount = await _context.NpcInterests.CountAsync();
        
        Assert.Equal(5, npcCount);
        Assert.Equal(5, accountCount);
        Assert.Equal(5, profileCount);
        Assert.Equal(5, personalityCount);
        Assert.Equal(25, interestCount); // 5 interests per NPC
    }

    [Fact]
    public async Task GeneratePopulationAsync_AllUsernamesUnique()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 100,
            RandomSeed = 11111
        };

        // Act
        await _populationService.GeneratePopulationAsync(config);

        // Assert
        var usernames = await _context.Accounts.Select(a => a.UsernameNormalized).ToListAsync();
        var uniqueUsernames = new HashSet<string>(usernames, StringComparer.OrdinalIgnoreCase);
        
        Assert.Equal(usernames.Count, uniqueUsernames.Count);
    }

    [Fact]
    public async Task GeneratePopulationAsync_RespectsAccountTypeDistribution()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 100,
            RandomSeed = 22222,
            Distribution = new AccountTypeDistribution
            {
                OrdinaryUser = 50,
                Creator = 30,
                Influencer = 10,
                News = 5,
                Official = 3,
                Celebrity = 2
            }
        };

        // Act
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.True(result.Success);
        
        // Check distribution
        Assert.Equal(2, result.Distribution[AccountType.Celebrity]);
        Assert.Equal(3, result.Distribution[AccountType.Official]);
        Assert.Equal(5, result.Distribution[AccountType.News]);
        Assert.Equal(10, result.Distribution[AccountType.Influencer]);
        Assert.Equal(30, result.Distribution[AccountType.Creator]);
        Assert.Equal(50, result.Distribution[AccountType.OrdinaryUser]);
    }

    [Fact]
    public async Task GeneratePopulationAsync_DefaultDistribution()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 100,
            RandomSeed = 33333
            // Uses default distribution
        };

        // Act
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.True(result.Success);
        
        // Default: 70% ordinary, 12% creator, 7% influencer, 5% news, 4% official, 2% celebrity
        Assert.Equal(2, result.Distribution[AccountType.Celebrity]);
        Assert.Equal(4, result.Distribution[AccountType.Official]);
        Assert.Equal(5, result.Distribution[AccountType.News]);
        Assert.Equal(7, result.Distribution[AccountType.Influencer]);
        Assert.Equal(12, result.Distribution[AccountType.Creator]);
        Assert.Equal(70, result.Distribution[AccountType.OrdinaryUser]);
    }

    [Fact]
    public async Task GeneratePopulationAsync_InvalidConfig_ZeroSize()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 0
        };

        // Act
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("greater than 0", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratePopulationAsync_InvalidConfig_NegativeSize()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = -10
        };

        // Act
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("greater than 0", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratePopulationAsync_InvalidConfig_BadDistribution()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 100,
            Distribution = new AccountTypeDistribution
            {
                OrdinaryUser = 50,
                Creator = 30,
                Influencer = 10,
                News = 5,
                Official = 5,
                Celebrity = 5 // Total = 105%
            }
        };

        // Act
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("100", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratePopulationAsync_DuplicateGenerationBlocked()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 5,
            RandomSeed = 44444
        };

        // First generation
        await _populationService.GeneratePopulationAsync(config);

        // Act - Second generation should fail
        var result = await _populationService.GeneratePopulationAsync(config);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.ErrorMessage);
    }

    [Fact]
    public async Task GeneratePopulationAsync_SameSeed_SameStructure()
    {
        // Arrange
        var config1 = new PopulationConfig
        {
            PopulationSize = 10,
            RandomSeed = 55555
        };
        
        // Clear and recreate context for second test
        var config2 = new PopulationConfig
        {
            PopulationSize = 10,
            RandomSeed = 55555
        };

        // Act
        var result1 = await _populationService.GeneratePopulationAsync(config1);
        
        // Get usernames from first run
        var usernames1 = await _context.Accounts.Select(a => a.Username).ToListAsync();
        var distribution1 = result1.Distribution;
        
        // Clear database
        _context.NpcProfiles.RemoveRange(_context.NpcProfiles);
        _context.NpcPersonalities.RemoveRange(_context.NpcPersonalities);
        _context.NpcInterests.RemoveRange(_context.NpcInterests);
        _context.Profiles.RemoveRange(_context.Profiles);
        _context.Accounts.RemoveRange(_context.Accounts);
        await _context.SaveChangesAsync();
        
        // Second run with same seed
        var result2 = await _populationService.GeneratePopulationAsync(config2);
        var usernames2 = await _context.Accounts.Select(a => a.Username).ToListAsync();
        
        // Assert - Same seed produces same username sequence
        Assert.Equal(usernames1.Count, usernames2.Count);
        Assert.Equal(result1.Distribution, result2.Distribution);
    }

    [Fact]
    public async Task GeneratePopulationAsync_DifferentSeed_DifferentStructure()
    {
        // Arrange
        var config1 = new PopulationConfig { PopulationSize = 10, RandomSeed = 66666 };
        var config2 = new PopulationConfig { PopulationSize = 10, RandomSeed = 77777 };

        // Act
        await _populationService.GeneratePopulationAsync(config1);
        var usernames1 = await _context.Accounts.Select(a => a.Username).ToListAsync();
        
        // Clear and recreate
        _context.NpcProfiles.RemoveRange(_context.NpcProfiles);
        _context.NpcPersonalities.RemoveRange(_context.NpcPersonalities);
        _context.NpcInterests.RemoveRange(_context.NpcInterests);
        _context.Profiles.RemoveRange(_context.Profiles);
        _context.Accounts.RemoveRange(_context.Accounts);
        await _context.SaveChangesAsync();
        
        await _populationService.GeneratePopulationAsync(config2);
        var usernames2 = await _context.Accounts.Select(a => a.Username).ToListAsync();

        // Assert - Different seeds produce different usernames
        Assert.NotEqual(usernames1, usernames2);
    }

    [Fact]
    public async Task GeneratePopulationAsync_AllNpcsHaveValidPersonality()
    {
        // Arrange
        var config = new PopulationConfig { PopulationSize = 20, RandomSeed = 88888 };

        // Act
        await _populationService.GeneratePopulationAsync(config);

        // Assert
        var personalities = await _context.NpcPersonalities.ToListAsync();
        
        foreach (var personality in personalities)
        {
            Assert.InRange(personality.Openness, 0.0, 1.0);
            Assert.InRange(personality.Conscientiousness, 0.0, 1.0);
            Assert.InRange(personality.Extraversion, 0.0, 1.0);
            Assert.InRange(personality.Agreeableness, 0.0, 1.0);
            Assert.InRange(personality.Neuroticism, 0.0, 1.0);
        }
    }

    [Fact]
    public async Task GeneratePopulationAsync_AllNpcsHaveValidInterests()
    {
        // Arrange
        var config = new PopulationConfig { PopulationSize = 10, RandomSeed = 99999 };

        // Act
        await _populationService.GeneratePopulationAsync(config);

        // Assert
        var npcs = await _context.NpcProfiles
            .Include(n => n.Interests)
            .ToListAsync();
        
        foreach (var npc in npcs)
        {
            Assert.NotEmpty(npc.Interests);
            Assert.Equal(5, npc.Interests.Count);
            
            foreach (var interest in npc.Interests)
            {
                Assert.InRange(interest.Strength, 0.3, 1.0);
                Assert.NotEmpty(interest.InterestKey);
            }
        }
    }

    [Fact]
    public async Task GeneratePopulationAsync_PopulationExists_ReturnsCorrectCount()
    {
        // Arrange
        var config = new PopulationConfig { PopulationSize = 5, RandomSeed = 10101 };
        await _populationService.GeneratePopulationAsync(config);

        // Act
        var exists = await _populationService.PopulationExistsAsync();
        var count = await _populationService.GetExistingNpcCountAsync();

        // Assert
        Assert.True(exists);
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task GeneratePopulationAsync_ShortMethod_Works()
    {
        // Arrange & Act
        var result = await _populationService.GeneratePopulationAsync(5, seed: 12121);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.NpcsCreated);
    }

    [Fact]
    public async Task GeneratePopulationAsync_AllAccountsActive()
    {
        // Arrange
        var config = new PopulationConfig { PopulationSize = 10, RandomSeed = 13131 };

        // Act
        await _populationService.GeneratePopulationAsync(config);

        // Assert
        var inactiveAccounts = await _context.Accounts
            .Where(a => a.Status != AccountStatus.Active)
            .CountAsync();
        
        Assert.Equal(0, inactiveAccounts);
    }

    [Fact]
    public async Task GeneratePopulationAsync_AllAccountsHaveCorrectType()
    {
        // Arrange
        var config = new PopulationConfig
        {
            PopulationSize = 20,
            RandomSeed = 14141,
            Distribution = new AccountTypeDistribution
            {
                OrdinaryUser = 50,
                Creator = 25,
                Influencer = 15,
                News = 5,
                Official = 3,
                Celebrity = 2
            }
        };

        // Act
        await _populationService.GeneratePopulationAsync(config);

        // Assert
        var accountTypeCounts = await _context.Accounts
            .GroupBy(a => a.AccountType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();
        
        var dict = accountTypeCounts.ToDictionary(x => x.Type, x => x.Count);
        
        // Check all types exist in dict (even if count is 0)
        Assert.Equal(10, dict.GetValueOrDefault(AccountType.OrdinaryUser));
        Assert.Equal(5, dict.GetValueOrDefault(AccountType.Creator));
        Assert.Equal(3, dict.GetValueOrDefault(AccountType.Influencer));
        Assert.Equal(1, dict.GetValueOrDefault(AccountType.News));
        Assert.Equal(1, dict.GetValueOrDefault(AccountType.Official));
        Assert.Equal(0, dict.GetValueOrDefault(AccountType.Celebrity)); // 2% of 20 = 0.4 rounded down
    }
}
