using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcBehaviorPerformanceTests
{
    [Fact]
    public async Task ProcessBehavior_100Npcs_PerformsAcceptably()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var relevanceService = new ContentRelevanceService();
        var contentGenerator = new ContentGeneratorService();
        var socialGraph = new SocialGraphService(context);
        var npcSocialGraph = new NpcSocialGraphService(context, relevanceService);
        var behaviorService = new NpcBehaviorService(context, relevanceService, contentGenerator, socialGraph, npcSocialGraph);

        var config = new NpcBehaviorConfig
        {
            MaxCandidateAccounts = 50,
            MaxCandidatePosts = 30,
            BaseActionProbability = 1.0,
            PostCooldownSeconds = 0
        };

        // Create 100 NPCs
        await CreateTestPopulationAsync(context, 100);

        var npcs = await context.NpcProfiles
            .Include(n => n.Account)
            .Include(n => n.Personality)
            .Include(n => n.Interests)
            .Take(100)
            .ToListAsync();

        // Act
        var startTime = DateTime.UtcNow;
        var processed = 0;

        foreach (var npc in npcs)
        {
            await behaviorService.ProcessBehaviorAsync(npc, config);
            processed++;
        }

        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.Equal(100, processed);
        Assert.True(elapsed.TotalSeconds < 60, $"Processing 100 NPCs took {elapsed.TotalSeconds:F2}s, expected < 60s");
    }

    [Fact]
    public async Task GenerateCandidates_1000Candidates_PerformsAcceptably()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var relevanceService = new ContentRelevanceService();
        var contentGenerator = new ContentGeneratorService();
        var socialGraph = new SocialGraphService(context);
        var npcSocialGraph = new NpcSocialGraphService(context, relevanceService);
        var behaviorService = new NpcBehaviorService(context, relevanceService, contentGenerator, socialGraph, npcSocialGraph);

        // Create NPC and lots of content
        var npc = await CreateSingleNpcAsync(context);
        await CreateTestContentAsync(context, 50, 200);

        var config = new NpcBehaviorConfig
        {
            MaxCandidateAccounts = 50,
            MaxCandidatePosts = 30
        };

        // Act
        var startTime = DateTime.UtcNow;
        var candidates = (await behaviorService.GenerateCandidatesAsync(npc, config)).ToList();
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(candidates.Count > 0, "Should have generated candidates");
        Assert.True(elapsed.TotalSeconds < 5, $"Generating candidates took {elapsed.TotalSeconds:F2}s, expected < 5s");
    }

    [Fact]
    public async Task ContentRelevance_CalculatesEfficiently()
    {
        // Arrange
        var relevanceService = new ContentRelevanceService();
        var post = new Post { Content = "Just finished an amazing game on Steam! #gaming #tech" };
        var interests = Enumerable.Range(0, 10)
            .Select(i => new NpcInterest
            {
                InterestKey = InterestCategories.All[i % InterestCategories.All.Length],
                Strength = 0.5 + (i % 5) * 0.1
            })
            .ToList();

        // Act
        var startTime = DateTime.UtcNow;
        double totalRelevance = 0;

        for (int i = 0; i < 1000; i++)
        {
            totalRelevance += relevanceService.CalculatePostRelevance(post, interests);
        }

        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(totalRelevance > 0, "Should calculate relevance");
        Assert.True(elapsed.TotalSeconds < 1, $"1000 relevance calculations took {elapsed.TotalSeconds:F2}s, expected < 1s");
    }

    private static async Task CreateTestPopulationAsync(AppDbContext context, int count)
    {
        var accounts = new List<Account>();
        var npcProfiles = new List<NpcProfile>();
        var personalities = new List<NpcPersonality>();
        var interests = new List<NpcInterest>();

        for (int i = 0; i < count; i++)
        {
            var npcId = Guid.NewGuid();
            var account = new Account
            {
                AccountId = Guid.NewGuid(),
                Username = $"npc_{i}_{npcId:N}".Substring(0, 20),
                UsernameNormalized = $"NPC_{i}_{npcId:N}".Substring(0, 20).ToUpper(),
                PasswordHash = "hash",
                AccountType = (AccountType)(i % 6),
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            accounts.Add(account);
        }

        context.Accounts.AddRange(accounts);
        await context.SaveChangesAsync();

        for (int i = 0; i < count; i++)
        {
            var npcProfile = new NpcProfile
            {
                NpcId = Guid.NewGuid(),
                AccountId = accounts[i].Id,
                IsActive = true,
                ActivityState = NpcActivityState.Idle,
                NextSimulationAt = DateTime.UtcNow,
                SimulationIntervalSeconds = 30,
                SimulationVersion = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            npcProfiles.Add(npcProfile);
        }

        context.NpcProfiles.AddRange(npcProfiles);
        await context.SaveChangesAsync();

        for (int i = 0; i < count; i++)
        {
            var personality = new NpcPersonality
            {
                NpcProfileId = npcProfiles[i].Id,
                Openness = 0.3 + (i % 7) * 0.1,
                Conscientiousness = 0.3 + (i % 7) * 0.1,
                Extraversion = 0.3 + (i % 7) * 0.1,
                Agreeableness = 0.3 + (i % 7) * 0.1,
                Neuroticism = 0.3 + (i % 7) * 0.1,
                GeneratedAt = DateTime.UtcNow
            };
            personalities.Add(personality);

            for (int j = 0; j < 5; j++)
            {
                interests.Add(new NpcInterest
                {
                    NpcProfileId = npcProfiles[i].Id,
                    InterestKey = InterestCategories.All[(i + j) % InterestCategories.All.Length],
                    Strength = 0.5 + (j % 5) * 0.1,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        context.NpcPersonalities.AddRange(personalities);
        context.NpcInterests.AddRange(interests);
        await context.SaveChangesAsync();
    }

    private static async Task<NpcProfile> CreateSingleNpcAsync(AppDbContext context)
    {
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Username = "testnpc",
            UsernameNormalized = "TESTNPC",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var npcProfile = new NpcProfile
        {
            NpcId = Guid.NewGuid(),
            AccountId = account.Id,
            IsActive = true,
            ActivityState = NpcActivityState.Idle,
            NextSimulationAt = DateTime.UtcNow,
            SimulationIntervalSeconds = 30,
            SimulationVersion = 1
        };
        context.NpcProfiles.Add(npcProfile);
        await context.SaveChangesAsync();

        var personality = new NpcPersonality
        {
            NpcProfileId = npcProfile.Id,
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.5,
            Neuroticism = 0.5
        };
        context.NpcPersonalities.Add(personality);
        await context.SaveChangesAsync();

        return (await context.NpcProfiles
            .Include(n => n.Account)
            .Include(n => n.Personality)
            .Include(n => n.Interests)
            .FirstAsync(n => n.Id == npcProfile.Id))!;
    }

    private static async Task CreateTestContentAsync(AppDbContext context, int accounts, int postsPerAccount)
    {
        var accountList = new List<Account>();
        
        for (int i = 0; i < accounts; i++)
        {
            var account = new Account
            {
                AccountId = Guid.NewGuid(),
                Username = $"user_{i}",
                UsernameNormalized = $"USER_{i}",
                PasswordHash = "hash",
                AccountType = AccountType.OrdinaryUser,
                Status = AccountStatus.Active
            };
            accountList.Add(account);
        }
        
        context.Accounts.AddRange(accountList);
        await context.SaveChangesAsync();

        var postContents = new[]
        {
            "Gaming update: new release coming soon! #gaming",
            "Tech news: AI is changing everything",
            "Sports: exciting game last night!",
            "Music festival this weekend!",
            "Travel: exploring new destinations"
        };

        var posts = new List<Post>();
        foreach (var account in accountList)
        {
            for (int i = 0; i < postsPerAccount; i++)
            {
                posts.Add(new Post
                {
                    PostId = Guid.NewGuid(),
                    AuthorAccountId = account.Id,
                    Content = postContents[i % postContents.Length],
                    Status = PostStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
        }

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
    }
}
