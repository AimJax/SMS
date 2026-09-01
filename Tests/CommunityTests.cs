using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class CommunityTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CommunityService _communityService;
    private readonly Account _testAccount;
    private readonly Account _secondAccount;

    public CommunityTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        var logger = new Mock<ILogger<CommunityService>>();
        _communityService = new CommunityService(_context, logger.Object);

        // Create test accounts
        _testAccount = new Account
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            AccountId = Guid.NewGuid(),
            Status = AccountStatus.Active
        };
        _context.Accounts.Add(_testAccount);

        _secondAccount = new Account
        {
            Username = "seconduser",
            Email = "second@example.com",
            PasswordHash = "hash",
            AccountId = Guid.NewGuid(),
            Status = AccountStatus.Active
        };
        _context.Accounts.Add(_secondAccount);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateCommunity_ShouldSucceed()
    {
        // Act
        var community = await _communityService.CreateCommunityAsync(
            "Gaming Hub",
            "gaming",
            _testAccount.Id,
            "A community for gamers",
            "games,pc,console");

        // Assert
        Assert.NotNull(community);
        Assert.Equal("Gaming Hub", community.Name);
        Assert.Equal("gaming-hub", community.Slug);
        Assert.Equal("gaming", community.Topic);
        Assert.Equal(_testAccount.Id, community.OwnerAccountId);
        Assert.True(community.MemberCount >= 1);
    }

    [Fact]
    public async Task GetBySlug_ShouldReturnCommunity()
    {
        // Arrange
        await _communityService.CreateCommunityAsync(
            "Tech Talk",
            "technology",
            _testAccount.Id);

        // Act
        var community = await _communityService.GetBySlugAsync("tech-talk");

        // Assert
        Assert.NotNull(community);
        Assert.Equal("Tech Talk", community.Name);
    }

    [Fact]
    public async Task GetBySlug_ShouldReturnNullForNonexistent()
    {
        // Act
        var community = await _communityService.GetBySlugAsync("nonexistent");

        // Assert
        Assert.Null(community);
    }

    [Fact]
    public async Task JoinCommunity_ShouldCreateMembership()
    {
        // Arrange
        var community = await _communityService.CreateCommunityAsync(
            "Music Lovers",
            "music",
            _testAccount.Id);

        // Act
        var membership = await _communityService.JoinCommunityAsync(_secondAccount.Id, "music-lovers");

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(_secondAccount.Id, membership.AccountId);
        Assert.Equal(community.Id, membership.CommunityId);
        Assert.Equal(CommunityRole.Member, membership.Role);
    }

    [Fact]
    public async Task JoinCommunity_ShouldPreventDuplicate()
    {
        // Arrange
        await _communityService.CreateCommunityAsync(
            "Book Club",
            "literature",
            _testAccount.Id);

        await _communityService.JoinCommunityAsync(_secondAccount.Id, "book-club");

        // Act
        var secondJoin = await _communityService.JoinCommunityAsync(_secondAccount.Id, "book-club");

        // Assert
        Assert.Null(secondJoin);
    }

    [Fact]
    public async Task LeaveCommunity_ShouldSoftDelete()
    {
        // Arrange
        await _communityService.CreateCommunityAsync(
            "Art Space",
            "art",
            _testAccount.Id);

        await _communityService.JoinCommunityAsync(_secondAccount.Id, "art-space");

        // Act
        var result = await _communityService.LeaveCommunityAsync(_secondAccount.Id, "art-space");

        // Assert
        Assert.True(result);
        
        var membership = await _context.CommunityMemberships
            .FirstOrDefaultAsync(m => m.AccountId == _secondAccount.Id && m.Community!.Slug == "art-space");
        Assert.NotNull(membership);
        Assert.False(membership.IsActive);
    }

    [Fact]
    public async Task OwnerCannotLeaveCommunity()
    {
        // Arrange
        await _communityService.CreateCommunityAsync(
            "Sports Zone",
            "sports",
            _testAccount.Id);

        // Act
        var result = await _communityService.LeaveCommunityAsync(_testAccount.Id, "sports-zone");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPublicCommunities_ShouldReturnOnlyPublic()
    {
        // Arrange
        await _communityService.CreateCommunityAsync(
            "Public Gaming",
            "gaming",
            _testAccount.Id,
            visibility: CommunityVisibility.Public);

        await _communityService.CreateCommunityAsync(
            "Private Club",
            "exclusive",
            _testAccount.Id,
            visibility: CommunityVisibility.Private);

        // Act
        var (communities, _) = await _communityService.GetPublicCommunitiesAsync();

        // Assert
        Assert.Single(communities);
        Assert.Equal("Public Gaming", communities.First().Name);
    }

    [Fact]
    public async Task SearchCommunities_ShouldFindByName()
    {
        // Arrange
        await _communityService.CreateCommunityAsync(
            "Retro Gaming Hub",
            "gaming",
            _testAccount.Id);

        // Act
        var (communities, _) = await _communityService.SearchCommunitiesAsync("Retro", null);

        // Assert
        Assert.Single(communities);
        Assert.Equal("Retro Gaming Hub", communities.First().Name);
    }

    [Fact]
    public async Task SearchCommunities_ShouldFilterByTopic()
    {
        // Arrange
        await _communityService.CreateCommunityAsync("Tech Forum", "technology", _testAccount.Id);
        await _communityService.CreateCommunityAsync("Gaming Zone", "gaming", _testAccount.Id);

        // Act
        var (communities, _) = await _communityService.SearchCommunitiesAsync(null, "technology");

        // Assert
        Assert.Single(communities);
        Assert.Equal("Tech Forum", communities.First().Name);
    }

    [Fact]
    public async Task GetCommunityFeed_ShouldReturnCommunityPosts()
    {
        // Arrange
        var community = await _communityService.CreateCommunityAsync(
            "Music Talk",
            "music",
            _testAccount.Id);

        await _communityService.JoinCommunityAsync(_testAccount.Id, "music-talk");

        // Create posts in community
        var post1 = new Post
        {
            AuthorAccountId = _testAccount.Id,
            CommunityId = community.Id,
            Content = "Post 1",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow.AddMinutes(-2)
        };
        var post2 = new Post
        {
            AuthorAccountId = _testAccount.Id,
            CommunityId = community.Id,
            Content = "Post 2",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        };
        _context.Posts.AddRange(post1, post2);
        await _context.SaveChangesAsync();

        // Act
        var (posts, _) = await _communityService.GetCommunityFeedAsync(community.Id);

        // Assert
        Assert.Equal(2, posts.Count());
        Assert.Equal("Post 2", posts.First().Content);
    }

    [Fact]
    public async Task GetAccountCommunities_ShouldReturnMemberCommunities()
    {
        // Arrange
        await _communityService.CreateCommunityAsync("Community 1", "topic1", _testAccount.Id);
        await _communityService.CreateCommunityAsync("Community 2", "topic2", _secondAccount.Id);

        await _communityService.JoinCommunityAsync(_testAccount.Id, "community-1");

        // Act - get _testAccount's communities (own + member)
        var communities = await _communityService.GetAccountCommunitiesAsync(_testAccount.Id);

        // Assert - should include owned community and joined community
        Assert.True(communities.Count() >= 1);
        Assert.Contains(communities, c => c.Name == "Community 1");
    }

    [Fact]
    public async Task IsMember_ShouldReturnTrueForOwner()
    {
        // Arrange
        await _communityService.CreateCommunityAsync(
            "Test Community",
            "testing",
            _testAccount.Id);

        // Act
        var isMember = await _communityService.IsMemberAsync(_testAccount.Id, 
            (await _communityService.GetBySlugAsync("test-community"))!.Id);

        // Assert
        Assert.True(isMember);
    }

    [Fact]
    public async Task GetRelevantCommunitiesForNpc_ShouldMatchByTopic()
    {
        // Arrange
        await _communityService.CreateCommunityAsync("Gaming Hub", "gaming", _testAccount.Id);
        await _communityService.CreateCommunityAsync("Tech Forum", "technology", _testAccount.Id);

        // Act
        var communities = await _communityService.GetRelevantCommunitiesForNpcAsync(new[] { "gaming" }, 10);

        // Assert
        Assert.Single(communities);
        Assert.Equal("Gaming Hub", communities.First().Name);
    }

    [Fact]
    public async Task GetMemberRole_ShouldReturnOwner()
    {
        // Arrange
        var community = await _communityService.CreateCommunityAsync(
            "Owner Test",
            "testing",
            _testAccount.Id);

        // Act
        var role = await _communityService.GetMemberRoleAsync(_testAccount.Id, community.Id);

        // Assert
        Assert.Equal(CommunityRole.Owner, role);
    }

    [Fact]
    public async Task GetMemberRole_ShouldReturnMember()
    {
        // Arrange
        await _communityService.CreateCommunityAsync("Member Test", "testing", _testAccount.Id);
        await _communityService.JoinCommunityAsync(_secondAccount.Id, "member-test");

        var community = await _communityService.GetBySlugAsync("member-test");

        // Act
        var role = await _communityService.GetMemberRoleAsync(_secondAccount.Id, community!.Id);

        // Assert
        Assert.Equal(CommunityRole.Member, role);
    }
}

public class CommunitySeedTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CommunitySeedService _seedService;

    public CommunitySeedTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _seedService = new CommunitySeedService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SeedCommunities_ShouldCreateCommunities()
    {
        // Act
        var result = await _seedService.SeedCommunitiesAsync(10);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10, result.CommunitiesCreated);
    }

    [Fact]
    public async Task SeedCommunities_ShouldNotDuplicate()
    {
        // Arrange
        await _seedService.SeedCommunitiesAsync(5);

        // Act
        var secondSeed = await _seedService.SeedCommunitiesAsync(5);

        // Assert
        Assert.False(secondSeed.Success);
        Assert.Contains("already exist", secondSeed.ErrorMessage);
    }

    [Fact]
    public async Task CommunitiesExist_ShouldReturnFalseWhenEmpty()
    {
        // Act
        var exists = await _seedService.CommunitiesExistAsync();

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task CommunitiesExist_ShouldReturnTrueAfterSeeding()
    {
        // Arrange
        await _seedService.SeedCommunitiesAsync(5);

        // Act
        var exists = await _seedService.CommunitiesExistAsync();

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task SeededCommunities_ShouldHaveValidTopics()
    {
        // Act
        await _seedService.SeedCommunitiesAsync(50);
        var communities = await _context.Communities.ToListAsync();

        // Assert
        Assert.NotEmpty(communities);
        Assert.All(communities, c => Assert.False(string.IsNullOrEmpty(c.Topic)));
        Assert.All(communities, c => Assert.Equal(CommunityVisibility.Public, c.Visibility));
    }
}
