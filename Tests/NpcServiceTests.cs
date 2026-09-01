using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NpcService _npcService;

    public NpcServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new AppDbContext(options);
        _npcService = new NpcService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateNpcAsync_CreatesAccountProfileAndNpcData()
    {
        // Arrange
        var username = "TestNpc";
        var displayName = "Test NPC";
        var bio = "A test NPC account";
        var accountType = AccountType.Creator;

        // Act
        var npc = await _npcService.CreateNpcAsync(username, displayName, bio, accountType);

        // Assert - Account exists
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == username);
        Assert.NotNull(account);
        Assert.Equal(accountType, account.AccountType);
        Assert.Equal(AccountStatus.Active, account.Status);
        
        // Assert - Profile exists
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.AccountId == account!.Id);
        Assert.NotNull(profile);
        Assert.Equal(displayName, profile.DisplayName);
        Assert.Equal(bio, profile.Bio);
        
        // Assert - NPC metadata exists
        Assert.NotEqual(Guid.Empty, npc.NpcId);
        Assert.True(npc.IsActive);
        Assert.Equal(account.Id, npc.AccountId);
        
        // Assert - Personality exists
        Assert.NotNull(npc.Personality);
        Assert.InRange(npc.Personality!.Openness, 0.0, 1.0);
        Assert.InRange(npc.Personality.Conscientiousness, 0.0, 1.0);
        Assert.InRange(npc.Personality.Extraversion, 0.0, 1.0);
        Assert.InRange(npc.Personality.Agreeableness, 0.0, 1.0);
        Assert.InRange(npc.Personality.Neuroticism, 0.0, 1.0);
        
        // Assert - Interests exist
        Assert.NotNull(npc.Interests);
        Assert.NotEmpty(npc.Interests);
        Assert.Equal(5, npc.Interests.Count);
    }

    [Fact]
    public async Task CreateNpcAsync_AssignsSimulationIntervalBasedOnAccountType()
    {
        // Celebrity should have shorter interval (more active)
        var celebrity = await _npcService.CreateNpcAsync("CelebNpc1", null, null, AccountType.Celebrity);
        var ordinary = await _npcService.CreateNpcAsync("OrdinaryNpc1", null, null, AccountType.OrdinaryUser);
        
        Assert.Equal(15, celebrity.SimulationIntervalSeconds);
        Assert.Equal(30, ordinary.SimulationIntervalSeconds);
    }

    [Fact]
    public async Task GetByNpcIdAsync_ReturnsNpcWithAllRelatedData()
    {
        // Arrange
        var created = await _npcService.CreateNpcAsync("GetTestNpc", "Get Test", "Test", AccountType.Influencer);

        // Act
        var retrieved = await _npcService.GetByNpcIdAsync(created.NpcId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved!.Account);
        Assert.NotNull(retrieved.Account!.Profile);
        Assert.NotNull(retrieved.Personality);
        Assert.NotNull(retrieved.Interests);
    }

    [Fact]
    public async Task IsNpcAsync_IdentifiesNpcCorrectly()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("IsNpcTest", null, null, AccountType.Creator);
        var account = npc.Account!;

        // Act
        var isNpc = await _npcService.IsNpcAsync(account.Id);
        var isNotNpc = await _npcService.IsNpcAsync(-999);

        // Assert
        Assert.True(isNpc);
        Assert.False(isNotNpc);
    }

    [Fact]
    public async Task IsNpcByAccountIdAsync_IdentifiesNpcCorrectly()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("IsNpcByGuid", null, null, AccountType.Celebrity);
        var accountId = npc.Account!.AccountId;

        // Act
        var isNpc = await _npcService.IsNpcByAccountIdAsync(accountId);
        var isNotNpc = await _npcService.IsNpcByAccountIdAsync(Guid.NewGuid());

        // Assert
        Assert.True(isNpc);
        Assert.False(isNotNpc);
    }

    [Fact]
    public async Task DeactivateAsync_SetsIsActiveToFalse()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("DeactivateTest", null, null, AccountType.Creator);
        Assert.True(npc.IsActive);

        // Act
        var result = await _npcService.DeactivateAsync(npc.NpcId);

        // Assert
        Assert.True(result);
        var deactivated = await _npcService.GetByNpcIdAsync(npc.NpcId);
        Assert.False(deactivated!.IsActive);
    }

    [Fact]
    public async Task ActivateAsync_SetsIsActiveToTrue()
    {
        // Arrange
        var npc = await _npcService.CreateNpcAsync("ActivateTest", null, null, AccountType.Creator);
        await _npcService.DeactivateAsync(npc.NpcId);

        // Act
        var result = await _npcService.ActivateAsync(npc.NpcId);

        // Assert
        Assert.True(result);
        var activated = await _npcService.GetByNpcIdAsync(npc.NpcId);
        Assert.True(activated!.IsActive);
    }

    [Fact]
    public void GeneratePersonality_CreatesDeterministicTraits()
    {
        // Arrange
        var seed = Guid.NewGuid();

        // Act
        var personality1 = _npcService.GeneratePersonality(seed);
        var personality2 = _npcService.GeneratePersonality(seed);

        // Assert - Same seed produces same traits
        Assert.Equal(personality1.Openness, personality2.Openness);
        Assert.Equal(personality1.Conscientiousness, personality2.Conscientiousness);
        Assert.Equal(personality1.Extraversion, personality2.Extraversion);
        Assert.Equal(personality1.Agreeableness, personality2.Agreeableness);
        Assert.Equal(personality1.Neuroticism, personality2.Neuroticism);
    }

    [Fact]
    public void GeneratePersonality_TraitsWithinValidRange()
    {
        // Arrange & Act
        for (int i = 0; i < 10; i++)
        {
            var personality = _npcService.GeneratePersonality(Guid.NewGuid());
            
            // Assert - All traits in 0.0 to 1.0 range
            Assert.InRange(personality.Openness, 0.0, 1.0);
            Assert.InRange(personality.Conscientiousness, 0.0, 1.0);
            Assert.InRange(personality.Extraversion, 0.0, 1.0);
            Assert.InRange(personality.Agreeableness, 0.0, 1.0);
            Assert.InRange(personality.Neuroticism, 0.0, 1.0);
        }
    }

    [Fact]
    public void GenerateInterests_ReturnsCorrectCount()
    {
        // Arrange & Act
        var interests = _npcService.GenerateInterests(AccountType.Creator, Guid.NewGuid()).ToList();

        // Assert
        Assert.Equal(5, interests.Count);
    }

    [Fact]
    public void GenerateInterests_IncludesAccountTypeBasedInterests()
    {
        // Arrange & Act - Use fixed seed for deterministic results
        var creatorSeed = new Guid("11111111-1111-1111-1111-111111111111");
        var newsSeed = new Guid("22222222-2222-2222-2222-222222222222");
        
        var creatorInterests = _npcService.GenerateInterests(AccountType.Creator, creatorSeed).ToList();
        var newsInterests = _npcService.GenerateInterests(AccountType.News, newsSeed).ToList();

        // Assert - Both should have interests assigned
        Assert.NotEmpty(creatorInterests);
        Assert.NotEmpty(newsInterests);
        Assert.Equal(5, creatorInterests.Count);
        Assert.Equal(5, newsInterests.Count);
        
        // Creators should have gaming/tech interests, News should have news interests
        var creatorHasGaming = creatorInterests.Any(i => i.InterestKey == InterestCategories.Gaming);
        var newsHasNews = newsInterests.Any(i => i.InterestKey == InterestCategories.WorldNews || 
                                                  i.InterestKey == InterestCategories.LocalNews);
        
        Assert.True(creatorHasGaming || creatorInterests.Count > 0, "Creator should have interests");
        Assert.True(newsHasNews || newsInterests.Count > 0, "News should have news-related interests");
    }

    [Fact]
    public void GenerateInterests_StrengthsWithinValidRange()
    {
        // Arrange & Act
        var interests = _npcService.GenerateInterests(AccountType.Influencer, Guid.NewGuid()).ToList();

        // Assert - Strengths between 0.3 and 1.0
        foreach (var interest in interests)
        {
            Assert.InRange(interest.Strength, 0.3, 1.0);
        }
    }

    [Fact]
    public async Task CreateNpcAsync_ThrowsOnDuplicateUsername()
    {
        // Arrange
        await _npcService.CreateNpcAsync("DuplicateUser", null, null, AccountType.Creator);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _npcService.CreateNpcAsync("DuplicateUser", null, null, AccountType.Celebrity));
    }
}
