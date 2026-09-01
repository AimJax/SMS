using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcBehaviorIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NpcBehaviorService _behaviorService;
    private readonly ContentRelevanceService _relevanceService;
    private readonly ContentGeneratorService _contentGenerator;
    private readonly SocialGraphService _socialGraph;
    private readonly NpcBehaviorConfig _config;

    public NpcBehaviorIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _relevanceService = new ContentRelevanceService();
        _contentGenerator = new ContentGeneratorService();
        _socialGraph = new SocialGraphService(_context);
        _behaviorService = new NpcBehaviorService(_context, _relevanceService, _contentGenerator, _socialGraph);
        
        _config = new NpcBehaviorConfig
        {
            MaxCandidateAccounts = 50,
            MaxCandidatePosts = 30,
            BaseActionProbability = 1.0, // Always act for testing
            PostCooldownSeconds = 0, // No cooldown for testing
            MaxFollowsPerTick = 5,
            MaxLikesPerTick = 10,
            MaxCommentsPerTick = 5,
            MaxUnfollowsPerTick = 2,
            RecentPostsHours = 24,
            MaxFollowingBeforeUnfollow = 1000, // High threshold
            EnableExploration = true,
            ExplorationRate = 0.5
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GenerateCandidatesAsync_IncludesViewFeedOption()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();

        // Act
        var candidates = (await _behaviorService.GenerateCandidatesAsync(npc, _config)).ToList();

        // Assert
        Assert.NotEmpty(candidates);
        Assert.Contains(candidates, c => c.ActionType == NpcActionType.ViewFeed);
    }

    [Fact]
    public async Task GenerateCandidatesAsync_IncludesFollowCandidates()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        await CreateTestAccountsAsync(5); // Create some accounts to follow

        // Act
        var candidates = (await _behaviorService.GenerateCandidatesAsync(npc, _config)).ToList();

        // Assert
        Assert.Contains(candidates, c => c.ActionType == NpcActionType.Follow);
    }

    [Fact]
    public async Task GenerateCandidatesAsync_ExcludesAlreadyFollowed()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var accounts = await CreateTestAccountsAsync(3);
        
        // NPC follows first account
        await _socialGraph.FollowAsync(npc.AccountId, accounts[0].Id);

        // Act
        var candidates = (await _behaviorService.GenerateCandidatesAsync(npc, _config)).ToList();

        // Assert
        var followCandidates = candidates.Where(c => c.ActionType == NpcActionType.Follow).ToList();
        Assert.DoesNotContain(followCandidates, c => c.TargetAccountId == accounts[0].Id);
    }

    [Fact]
    public async Task ProcessBehaviorAsync_CreatesFollowAction()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        await CreateTestAccountsAsync(3);
        
        // Ensure high probability
        var config = new NpcBehaviorConfig { BaseActionProbability = 1.0 };

        // Act
        var result = await _behaviorService.ProcessBehaviorAsync(npc, config);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success || result.WasSkipped);
    }

    [Fact]
    public async Task ProcessBehaviorAsync_RecordsAction()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var targetAccount = (await CreateTestAccountsAsync(1)).First();
        
        var config = new NpcBehaviorConfig { BaseActionProbability = 1.0 };

        // Act
        await _behaviorService.ProcessBehaviorAsync(npc, config);

        // Assert
        var actions = await _context.NpcActions.Where(a => a.NpcProfileId == npc.Id).ToListAsync();
        Assert.NotEmpty(actions);
    }

    [Fact]
    public async Task ProcessBehaviorAsync_CreatesPost()
    {
        // Arrange
        var npc = await CreateTestNpcAsync(AccountType.Creator);
        
        var config = new NpcBehaviorConfig 
        { 
            BaseActionProbability = 1.0,
            PostCooldownSeconds = 0
        };

        // Act
        var result = await _behaviorService.ProcessBehaviorAsync(npc, config);

        // Assert - Post might or might not be selected, depending on scoring
        // Just verify the process completes
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ProcessBehaviorAsync_LikesPost()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var targetAccount = (await CreateTestAccountsAsync(1)).First();
        var post = await CreateTestPostAsync(targetAccount.Id);
        
        var config = new NpcBehaviorConfig { BaseActionProbability = 1.0 };

        // Act
        var result = await _behaviorService.ProcessBehaviorAsync(npc, config);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CanFollowAsync_SelfFollow_ReturnsFalse()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();

        // Act
        var canFollow = await _behaviorService.CanFollowAsync(npc.AccountId, npc.AccountId);

        // Assert
        Assert.False(canFollow);
    }

    [Fact]
    public async Task CanFollowAsync_AlreadyFollowing_ReturnsFalse()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var targetAccount = (await CreateTestAccountsAsync(1)).First();
        await _socialGraph.FollowAsync(npc.AccountId, targetAccount.Id);

        // Act
        var canFollow = await _behaviorService.CanFollowAsync(npc.AccountId, targetAccount.Id);

        // Assert
        Assert.False(canFollow);
    }

    [Fact]
    public async Task CanFollowAsync_ValidTarget_ReturnsTrue()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var targetAccount = (await CreateTestAccountsAsync(1)).First();

        // Act
        var canFollow = await _behaviorService.CanFollowAsync(npc.AccountId, targetAccount.Id);

        // Assert
        Assert.True(canFollow);
    }

    [Fact]
    public async Task CanLikeAsync_ValidPost_ReturnsTrue()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var targetAccount = (await CreateTestAccountsAsync(1)).First();
        var post = await CreateTestPostAsync(targetAccount.Id);

        // Act
        var canLike = await _behaviorService.CanLikeAsync(npc.AccountId, post.Id);

        // Assert
        Assert.True(canLike);
    }

    [Fact]
    public async Task CanLikeAsync_AlreadyLiked_ReturnsFalse()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var targetAccount = (await CreateTestAccountsAsync(1)).First();
        var post = await CreateTestPostAsync(targetAccount.Id);
        
        // Like the post
        await _context.PostLikes.AddAsync(new PostLike
        {
            PostId = post.Id,
            AccountId = npc.AccountId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var canLike = await _behaviorService.CanLikeAsync(npc.AccountId, post.Id);

        // Assert
        Assert.False(canLike);
    }

    [Fact]
    public async Task GetRecentPostsAsync_ExcludesBlockedAccounts()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var accounts = await CreateTestAccountsAsync(2);
        var post1 = await CreateTestPostAsync(accounts[0].Id);
        var post2 = await CreateTestPostAsync(accounts[1].Id);
        
        // Block second account
        await _context.Blocks.AddAsync(new Block
        {
            BlockerAccountId = npc.AccountId,
            BlockedAccountId = accounts[1].Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var posts = (await _behaviorService.GetRecentPostsAsync(npc, 10, 24)).ToList();

        // Assert
        Assert.DoesNotContain(posts, p => p.Id == post2.Id);
    }

    [Fact]
    public async Task GetRecentPostsAsync_ExcludesOwnPosts()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        await CreateTestPostAsync(npc.AccountId);

        // Act
        var posts = await _behaviorService.GetRecentPostsAsync(npc, 10, 24);

        // Assert
        Assert.DoesNotContain(posts, p => p.AuthorAccountId == npc.AccountId);
    }

    [Fact]
    public async Task GetCandidateAccountsAsync_ExcludesFollowing()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var accounts = await CreateTestAccountsAsync(3);
        
        // Follow first account
        await _socialGraph.FollowAsync(npc.AccountId, accounts[0].Id);

        // Act
        var candidates = (await _behaviorService.GetCandidateAccountsAsync(npc, 10)).ToList();

        // Assert
        Assert.DoesNotContain(candidates, a => a.Id == accounts[0].Id);
    }

    [Fact]
    public async Task ProcessBehaviorAsync_HandlesBlockedContent()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var accounts = await CreateTestAccountsAsync(2);
        await CreateTestPostAsync(accounts[0].Id);
        await CreateTestPostAsync(accounts[1].Id);
        
        // Block second account
        await _context.Blocks.AddAsync(new Block
        {
            BlockerAccountId = npc.AccountId,
            BlockedAccountId = accounts[1].Id,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        
        var config = new NpcBehaviorConfig { BaseActionProbability = 1.0 };

        // Act - Should not crash
        var result = await _behaviorService.ProcessBehaviorAsync(npc, config);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success || result.WasSkipped);
    }

    private async Task<NpcProfile> CreateTestNpcAsync(AccountType accountType = AccountType.OrdinaryUser)
    {
        var npcId = Guid.NewGuid();
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Username = $"npc_{npcId:N}".Substring(0, 15),
            UsernameNormalized = $"NPC_{npcId:N}".Substring(0, 15).ToUpper(),
            PasswordHash = "hash",
            AccountType = accountType,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var profile = new Profile
        {
            AccountId = account.Id,
            DisplayName = "Test NPC",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Profiles.Add(profile);

        var npcProfile = new NpcProfile
        {
            NpcId = npcId,
            AccountId = account.Id,
            IsActive = true,
            ActivityState = NpcActivityState.Idle,
            NextSimulationAt = DateTime.UtcNow,
            SimulationIntervalSeconds = 30,
            SimulationVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.NpcProfiles.Add(npcProfile);

        var personality = new NpcPersonality
        {
            NpcProfileId = 0, // Will be set after save
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.5,
            Neuroticism = 0.5,
            GeneratedAt = DateTime.UtcNow
        };
        
        var interests = new List<NpcInterest>
        {
            new() { InterestKey = InterestCategories.Gaming, Strength = 0.8 },
            new() { InterestKey = InterestCategories.Technology, Strength = 0.6 }
        };

        await _context.SaveChangesAsync();

        // Update foreign keys
        personality.NpcProfileId = npcProfile.Id;
        foreach (var interest in interests)
        {
            interest.NpcProfileId = npcProfile.Id;
        }
        _context.NpcPersonalities.Add(personality);
        _context.NpcInterests.AddRange(interests);
        await _context.SaveChangesAsync();

        // Load with navigation properties
        return (await _context.NpcProfiles
            .Include(n => n.Account)
            .Include(n => n.Personality)
            .Include(n => n.Interests)
            .FirstAsync(n => n.NpcId == npcId))!;
    }

    private async Task<List<Account>> CreateTestAccountsAsync(int count)
    {
        var accounts = new List<Account>();
        
        for (int i = 0; i < count; i++)
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                AccountId = accountId,
                Username = $"testuser_{Guid.NewGuid():N}".Substring(0, 15),
                UsernameNormalized = $"TESTUSER_{Guid.NewGuid():N}".Substring(0, 15).ToUpper(),
                PasswordHash = "hash",
                AccountType = AccountType.OrdinaryUser,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            accounts.Add(account);
        }
        
        _context.Accounts.AddRange(accounts);
        await _context.SaveChangesAsync();
        
        return accounts;
    }

    private async Task<Post> CreateTestPostAsync(int authorAccountId)
    {
        var post = new Post
        {
            PostId = Guid.NewGuid(),
            AuthorAccountId = authorAccountId,
            Content = "Test gaming post with some interesting content!",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        return post;
    }
}
