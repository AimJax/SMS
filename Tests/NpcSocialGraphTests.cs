using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcSocialGraphServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NpcSocialGraphService _service;
    private readonly ContentRelevanceService _relevanceService;

    public NpcSocialGraphServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _relevanceService = new ContentRelevanceService();
        _service = new NpcSocialGraphService(_context, _relevanceService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<NpcProfile> CreateTestNpcAsync(
        string username = "testnpc",
        AccountType accountType = AccountType.OrdinaryUser,
        NpcPersonality? personality = null,
        List<NpcInterest>? interests = null)
    {
        var account = new Account
        {
            Username = username,
            PasswordHash = "hash",
            AccountType = accountType,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var npc = new NpcProfile
        {
            AccountId = account.Id,
            IsActive = true,
            SimulationIntervalSeconds = 60,
            NextSimulationAt = DateTime.UtcNow,
            ActivityState = NpcActivityState.Idle,
            Personality = personality ?? new NpcPersonality(),
            Interests = interests ?? new List<NpcInterest>()
        };

        if (personality != null)
        {
            npc.Personality.Openness = personality.Openness;
            npc.Personality.Conscientiousness = personality.Conscientiousness;
            npc.Personality.Extraversion = personality.Extraversion;
            npc.Personality.Agreeableness = personality.Agreeableness;
            npc.Personality.Neuroticism = personality.Neuroticism;
        }

        _context.NpcProfiles.Add(npc);
        await _context.SaveChangesAsync();

        return npc;
    }

    private async Task<Account> CreateTestAccountAsync(
        string username = "testaccount",
        AccountType accountType = AccountType.OrdinaryUser)
    {
        var account = new Account
        {
            Username = username,
            PasswordHash = "hash",
            AccountType = accountType,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    private async Task<Post> CreateTestPostAsync(int authorAccountId, string content)
    {
        var post = new Post
        {
            AuthorAccountId = authorAccountId,
            Content = content,
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    [Fact]
    public async Task GetFollowCandidatesAsync_ExcludesAlreadyFollowing()
    {
        // Arrange
        var npc = await CreateTestNpcAsync("npc1");
        var account1 = await CreateTestAccountAsync("account1");
        var account2 = await CreateTestAccountAsync("account2");
        
        // NPC already follows account1
        _context.Follows.Add(new Follow
        {
            FollowerAccountId = npc.AccountId,
            FollowedAccountId = account1.Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var candidates = (await _service.GetFollowCandidatesAsync(npc, 10)).ToList();

        // Assert
        Assert.DoesNotContain(candidates, c => c.AccountId == account1.Id);
        Assert.Contains(candidates, c => c.AccountId == account2.Id);
    }

    [Fact]
    public async Task GetFollowCandidatesAsync_ExcludesBlockedAccounts()
    {
        // Arrange
        var npc = await CreateTestNpcAsync("npc1");
        var account1 = await CreateTestAccountAsync("account1");
        var account2 = await CreateTestAccountAsync("account2");
        
        // NPC blocks account1
        _context.Blocks.Add(new Block
        {
            BlockerAccountId = npc.AccountId,
            BlockedAccountId = account1.Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var candidates = (await _service.GetFollowCandidatesAsync(npc, 10)).ToList();

        // Assert
        Assert.DoesNotContain(candidates, c => c.AccountId == account1.Id);
        Assert.Contains(candidates, c => c.AccountId == account2.Id);
    }

    [Fact]
    public async Task GetFollowCandidatesAsync_ExcludesSelf()
    {
        // Arrange
        var npc = await CreateTestNpcAsync("npc1");

        // Act
        var candidates = (await _service.GetFollowCandidatesAsync(npc, 10)).ToList();

        // Assert
        Assert.DoesNotContain(candidates, c => c.AccountId == npc.AccountId);
    }

    [Fact]
    public async Task GetFollowCandidatesAsync_IncludesAccountsWithMatchingInterests()
    {
        // Arrange
        var npc = await CreateTestNpcAsync("npc1");
        npc.Interests = new List<NpcInterest>
        {
            new NpcInterest { InterestKey = InterestCategories.Technology, Strength = 0.8 }
        };
        await _context.SaveChangesAsync();

        var account = await CreateTestAccountAsync("techaccount");
        await CreateTestPostAsync(account.Id, "New AI programming framework released");

        // Act
        var candidates = (await _service.GetFollowCandidatesAsync(npc, 10)).ToList();

        // Assert
        Assert.NotEmpty(candidates);
        Assert.Contains(candidates, c => c.AccountId == account.Id);
    }

    [Fact]
    public async Task GetFollowCandidatesAsync_IncludesReciprocityCandidates()
    {
        // Arrange
        var npc = await CreateTestNpcAsync("npc1", personality: new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.7,
            Neuroticism = 0.3
        });
        var followerAccount = await CreateTestAccountAsync("follower");
        
        // Follower follows NPC
        _context.Follows.Add(new Follow
        {
            FollowerAccountId = followerAccount.Id,
            FollowedAccountId = npc.AccountId,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        // Act
        var candidates = (await _service.GetFollowCandidatesAsync(npc, 10)).ToList();

        // Assert
        Assert.Contains(candidates, c => c.AccountId == followerAccount.Id && c.ReciprocityScore > 0);
    }

    [Fact]
    public async Task GetUnfollowCandidatesAsync_ReturnsStaleFollows()
    {
        // Arrange
        var npc = await CreateTestNpcAsync("npc1", personality: new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.5,
            Neuroticism = 0.5
        });
        var followedAccount = await CreateTestAccountAsync("followed");
        
        // Follow with no recent posts
        _context.Follows.Add(new Follow
        {
            FollowerAccountId = npc.AccountId,
            FollowedAccountId = followedAccount.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        });
        await _context.SaveChangesAsync();

        // Act
        var candidates = (await _service.GetUnfollowCandidatesAsync(npc, 5, 24)).ToList();

        // Assert
        Assert.Contains(candidates, id => id == followedAccount.Id);
    }

    [Fact]
    public async Task GetUnfollowCandidatesAsync_ExcludesActiveFollows()
    {
        // Arrange
        var npc = await CreateTestNpcAsync("npc1", personality: new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.5,
            Neuroticism = 0.2 // Low neuroticism
        });
        var followedAccount = await CreateTestAccountAsync("activefollowed");
        
        // Follow with recent posts
        _context.Follows.Add(new Follow
        {
            FollowerAccountId = npc.AccountId,
            FollowedAccountId = followedAccount.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });
        await CreateTestPostAsync(followedAccount.Id, "Great day!");
        await _context.SaveChangesAsync();

        // Act
        var candidates = (await _service.GetUnfollowCandidatesAsync(npc, 5, 24)).ToList();

        // Assert
        Assert.DoesNotContain(candidates, id => id == followedAccount.Id);
    }

    [Fact]
    public async Task CalculateReciprocityScore_HigherForAgreeableness()
    {
        // Arrange
        var follower = await CreateTestNpcAsync("follower");
        var followeeHighAgreeableness = await CreateTestNpcAsync("highagree", personality: new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.9,
            Neuroticism = 0.2
        });
        var followeeLowAgreeableness = await CreateTestNpcAsync("lowagree", personality: new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.2,
            Neuroticism = 0.5
        });

        // Act
        var highScore = _service.CalculateReciprocityScore(follower, followeeHighAgreeableness);
        var lowScore = _service.CalculateReciprocityScore(follower, followeeLowAgreeableness);

        // Assert
        Assert.True(highScore > lowScore, "Higher agreeableness should produce higher reciprocity score");
    }

    [Fact]
    public async Task CalculateReciprocityScore_LowerForCelebrity()
    {
        // Arrange
        var follower = await CreateTestNpcAsync("follower");
        var celebrity = await CreateTestNpcAsync("celeb", AccountType.Celebrity, personality: new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.8,
            Neuroticism = 0.2
        });
        var ordinaryUser = await CreateTestNpcAsync("ordinary", AccountType.OrdinaryUser, personality: new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.8,
            Neuroticism = 0.2
        });

        // Act
        var celebrityScore = _service.CalculateReciprocityScore(follower, celebrity);
        var ordinaryScore = _service.CalculateReciprocityScore(follower, ordinaryUser);

        // Assert
        Assert.True(celebrityScore < ordinaryScore, "Celebrities should have lower reciprocity scores");
    }
}

public class SimulationStatusExtensionTests
{
    [Fact]
    public void SimulationStatus_HasSocialGraphMetrics()
    {
        // Arrange & Act
        var status = new SocialMediaSimulator.Server.Application.Models.SimulationStatus
        {
            TotalNpcFollows = 100,
            TotalNpcUnfollows = 10,
            LastTickFollows = 5,
            LastTickUnfollows = 1
        };

        // Assert
        Assert.Equal(100, status.TotalNpcFollows);
        Assert.Equal(10, status.TotalNpcUnfollows);
        Assert.Equal(5, status.LastTickFollows);
        Assert.Equal(1, status.LastTickUnfollows);
    }

    [Fact]
    public void SimulationTickResult_IncludesSocialGraphData()
    {
        // Arrange & Act
        var result = new SimulationTickResult(
            NpcsProcessed: 10,
            NpcsSkipped: 0,
            FollowsCreated: 3,
            UnfollowsCreated: 1,
            ProcessedAt: DateTime.UtcNow);

        // Assert
        Assert.Equal(10, result.NpcsProcessed);
        Assert.Equal(3, result.FollowsCreated);
        Assert.Equal(1, result.UnfollowsCreated);
    }
}
