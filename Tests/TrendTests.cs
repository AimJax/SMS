using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Tests;

public class TrendTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ITestOutputHelper _output;

    public TrendTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Topic Tests

    [Fact]
    public async Task CreateTopic_CreatesTopicSuccessfully()
    {
        // Arrange
        var topic = new Topic
        {
            Name = "gaming",
            DisplayName = "Gaming",
            Slug = "gaming",
            Category = TopicCategory.Gaming,
            Description = "Video games discussion",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "gaming");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("gaming", retrieved.Name);
        Assert.Equal("Gaming", retrieved.DisplayName);
        Assert.Equal("gaming", retrieved.Slug);
        Assert.Equal(TopicCategory.Gaming, retrieved.Category);
        Assert.True(retrieved.IsActive);
    }

    [Fact]
    public async Task GetTopicBySlug_ReturnsCorrectTopic()
    {
        // Arrange
        _context.Topics.Add(new Topic
        {
            Name = "movies",
            DisplayName = "Movies",
            Slug = "movies",
            Category = TopicCategory.Entertainment,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Slug == "movies");

        // Assert
        Assert.NotNull(topic);
        Assert.Equal("movies", topic.Name);
    }

    [Fact]
    public async Task GetTopicBySlug_ReturnsNullForNonexistent()
    {
        // Act
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Slug == "nonexistent");

        // Assert
        Assert.Null(topic);
    }

    [Fact]
    public async Task GetAllTopics_ReturnsAllActiveTopics()
    {
        // Arrange
        _context.Topics.AddRange(
            new Topic { Name = "tech", DisplayName = "Tech", Slug = "tech", Category = TopicCategory.Technology, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Topic { Name = "sports", DisplayName = "Sports", Slug = "sports", Category = TopicCategory.Sports, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Topic { Name = "inactive", DisplayName = "Inactive", Slug = "inactive", Category = TopicCategory.General, IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var topics = await _context.Topics.Where(t => t.IsActive).ToListAsync();

        // Assert
        Assert.Equal(2, topics.Count);
    }

    #endregion

    #region Hashtag Tests

    [Fact]
    public async Task ExtractHashtags_ExtractsCorrectly()
    {
        // Arrange
        var content = "Check out this #gaming post! #Technology is awesome #AI";
        var regex = new System.Text.RegularExpressions.Regex(@"(?:^|\s)(#\w+)");
        var matches = regex.Matches(content);
        var hashtags = matches.Select(m => m.Groups[1].Value.ToLowerInvariant()).Distinct().ToList();

        // Assert
        Assert.Equal(3, hashtags.Count);
        Assert.Contains("gaming", hashtags);
        Assert.Contains("technology", hashtags);
        Assert.Contains("ai", hashtags);
    }

    [Fact]
    public async Task ExtractHashtags_ReturnsEmptyForNoHashtags()
    {
        // Arrange
        var content = "This post has no hashtags";
        var regex = new System.Text.RegularExpressions.Regex(@"(?:^|\s)(#\w+)");
        var matches = regex.Matches(content);
        var hashtags = matches.Select(m => m.Groups[1].Value.ToLowerInvariant()).ToList();

        // Assert
        Assert.Empty(hashtags);
    }

    [Fact]
    public async Task GetOrCreateHashtag_CreatesNewHashtag()
    {
        // Arrange
        var hashtag = new Hashtag
        {
            Tag = "#gaming",
            NormalizedTag = "gaming",
            UsageCount = 0,
            TodayUsageCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Hashtags.Add(hashtag);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _context.Hashtags.FirstOrDefaultAsync(h => h.NormalizedTag == "gaming");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("#gaming", retrieved.Tag);
        Assert.Equal("gaming", retrieved.NormalizedTag);
        Assert.Equal(0, retrieved.UsageCount);
    }

    [Fact]
    public async Task GetOrCreateHashtag_ReturnsExistingHashtag()
    {
        // Arrange
        _context.Hashtags.Add(new Hashtag
        {
            Tag = "#gaming",
            NormalizedTag = "gaming",
            UsageCount = 5,
            TodayUsageCount = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var existing = await _context.Hashtags.FirstOrDefaultAsync(h => h.NormalizedTag == "gaming");

        // Assert
        Assert.NotNull(existing);
        Assert.Equal(5, existing.UsageCount);
    }

    [Fact]
    public async Task UpdateHashtagUsage_IncrementsCounts()
    {
        // Arrange
        _context.Hashtags.Add(new Hashtag
        {
            Tag = "#trending",
            NormalizedTag = "trending",
            UsageCount = 0,
            TodayUsageCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var hashtag = await _context.Hashtags.FirstOrDefaultAsync(h => h.NormalizedTag == "trending");
        if (hashtag != null)
        {
            hashtag.UsageCount++;
            hashtag.TodayUsageCount++;
            await _context.SaveChangesAsync();
        }

        // Assert
        var updated = await _context.Hashtags.FirstOrDefaultAsync(h => h.NormalizedTag == "trending");
        Assert.NotNull(updated);
        Assert.Equal(1, updated.UsageCount);
        Assert.Equal(1, updated.TodayUsageCount);
    }

    #endregion

    #region Trend Configuration Tests

    [Fact]
    public void TrendConfig_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new TrendConfig();

        // Assert
        Assert.True(config.Enabled);
        Assert.Equal(15, config.ProcessingIntervalMinutes);
        Assert.Equal(24, config.TrendWindowHours);
        Assert.Equal(10, config.MinPostsForTrend);
        Assert.Equal(20, config.MaxTrendingHashtags);
        Assert.Equal(24, config.TrendDurationHours);
    }

    [Fact]
    public void TrendStrength_EnumHasCorrectValues()
    {
        // Assert
        Assert.Equal(1, (int)TrendStrength.Emerging);
        Assert.Equal(2, (int)TrendStrength.Growing);
        Assert.Equal(3, (int)TrendStrength.Hot);
        Assert.Equal(4, (int)TrendStrength.Viral);
        Assert.Equal(5, (int)TrendStrength.Peaking);
    }

    [Fact]
    public void TrendType_EnumHasCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)TrendType.Topic);
        Assert.Equal(1, (int)TrendType.Hashtag);
        Assert.Equal(2, (int)TrendType.Event);
        Assert.Equal(3, (int)TrendType.Search);
        Assert.Equal(4, (int)TrendType.Viral);
    }

    [Fact]
    public void TrendScope_EnumHasCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)TrendScope.Global);
        Assert.Equal(1, (int)TrendScope.Community);
        Assert.Equal(2, (int)TrendScope.Personal);
    }

    #endregion

    #region Entity Tests

    [Fact]
    public void Topic_EntityHasCorrectProperties()
    {
        // Arrange
        var topic = new Topic
        {
            Name = "test",
            DisplayName = "Test",
            Slug = "test",
            Category = TopicCategory.General
        };

        // Assert
        Assert.Equal("test", topic.Name);
        Assert.Equal("Test", topic.DisplayName);
        Assert.Equal("test", topic.Slug);
        Assert.Equal(TopicCategory.General, topic.Category);
        Assert.True(topic.IsActive);
    }

    [Fact]
    public void Hashtag_EntityHasCorrectProperties()
    {
        // Arrange
        var hashtag = new Hashtag
        {
            Tag = "#test",
            NormalizedTag = "test"
        };

        // Assert
        Assert.Equal("#test", hashtag.Tag);
        Assert.Equal("test", hashtag.NormalizedTag);
        Assert.False(hashtag.IsTrending);
        Assert.Equal(0, hashtag.UsageCount);
    }

    [Fact]
    public void Trend_EntityHasCorrectProperties()
    {
        // Arrange
        var trend = new Trend
        {
            Query = "gaming",
            DisplayName = "Gaming",
            Strength = TrendStrength.Hot,
            Scope = TrendScope.Global
        };

        // Assert
        Assert.Equal("gaming", trend.Query);
        Assert.Equal("Gaming", trend.DisplayName);
        Assert.Equal(TrendStrength.Hot, trend.Strength);
        Assert.Equal(TrendScope.Global, trend.Scope);
        Assert.True(trend.IsActive);
    }

    [Fact]
    public void TopicSubscription_EntityHasCorrectProperties()
    {
        // Arrange
        var subscription = new TopicSubscription
        {
            AccountId = 1,
            TopicId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(1, subscription.AccountId);
        Assert.NotEqual(Guid.Empty, subscription.TopicId);
    }

    [Fact]
    public void TrendPropagation_EntityHasCorrectProperties()
    {
        // Arrange
        var propagation = new TrendPropagation
        {
            TrendId = Guid.NewGuid(),
            FromCommunityId = 1,
            ToCommunityId = 2,
            PropagatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, propagation.TrendId);
        Assert.Equal(1, propagation.FromCommunityId);
        Assert.Equal(2, propagation.ToCommunityId);
    }

    #endregion

    #region Topic Category Tests

    [Fact]
    public void TopicCategory_EnumHasCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)TopicCategory.General);
        Assert.Equal(1, (int)TopicCategory.Entertainment);
        Assert.Equal(2, (int)TopicCategory.Gaming);
        Assert.Equal(3, (int)TopicCategory.Technology);
        Assert.Equal(4, (int)TopicCategory.Sports);
        Assert.Equal(5, (int)TopicCategory.Politics);
        Assert.Equal(6, (int)TopicCategory.News);
        Assert.Equal(7, (int)TopicCategory.Lifestyle);
        Assert.Equal(8, (int)TopicCategory.Art);
        Assert.Equal(9, (int)TopicCategory.Meme);
        Assert.Equal(12, (int)TopicCategory.Hashtag);
    }

    #endregion

    #region Database Persistence Tests

    [Fact]
    public async Task Topics_PersistToDatabase()
    {
        // Arrange
        var topic = new Topic
        {
            Name = "persisttest",
            DisplayName = "PersistTest",
            Slug = "persisttest",
            Category = TopicCategory.Technology,
            PostCount = 100,
            ActivePostCount = 25,
            SubscriberCount = 50,
            IsVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        
        // Clear the change tracker
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "persisttest");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(100, retrieved.PostCount);
        Assert.Equal(25, retrieved.ActivePostCount);
        Assert.Equal(50, retrieved.SubscriberCount);
        Assert.True(retrieved.IsVerified);
    }

    [Fact]
    public async Task Trends_PersistToDatabase()
    {
        // Arrange
        var trend = new Trend
        {
            TrendId = Guid.NewGuid(),
            Type = TrendType.Topic,
            Query = "newtrend",
            DisplayName = "New Trend",
            Slug = "newtrend",
            Strength = TrendStrength.Growing,
            PostCount = 50,
            UniquePosters = 30,
            EngagementTotal = 500,
            Velocity = 2.5f,
            Scope = TrendScope.Global,
            IsActive = true,
            CalculatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Trends.Add(trend);
        await _context.SaveChangesAsync();
        
        // Clear the change tracker
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _context.Trends.FirstOrDefaultAsync(t => t.Query == "newtrend");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(50, retrieved.PostCount);
        Assert.Equal(30, retrieved.UniquePosters);
        Assert.Equal(500, retrieved.EngagementTotal);
        Assert.Equal(TrendStrength.Growing, retrieved.Strength);
    }

    #endregion
}
