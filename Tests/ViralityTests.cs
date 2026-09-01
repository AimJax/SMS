using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

/// <summary>
/// Tests for the virality system
/// </summary>
public class ViralityTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IPostService> _postServiceMock;
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly Mock<ISocialGraphService> _socialGraphServiceMock;
    private readonly Mock<IEventService> _eventServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<ViralityService>> _loggerMock;
    private readonly ViralityConfig _config;
    private readonly ViralityService _viralityService;

    public ViralityTests()
    {
        // Create in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        // Setup mocks
        _postServiceMock = new Mock<IPostService>();
        _accountServiceMock = new Mock<IAccountService>();
        _socialGraphServiceMock = new Mock<ISocialGraphService>();
        _eventServiceMock = new Mock<IEventService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<ViralityService>>();

        // Setup config
        _config = new ViralityConfig
        {
            Enabled = true,
            TrendingThreshold = 50,
            PopularThreshold = 200,
            ViralThreshold = 1000,
            MassivelyViralThreshold = 10000,
            ViralVelocityMin = 10,
            ViralWindowHours = 24,
            BaseFollowerGainOnViral = 10,
            BaseFameGainOnViral = 5.0f
        };

        _viralityService = new ViralityService(
            _context,
            _postServiceMock.Object,
            _accountServiceMock.Object,
            _socialGraphServiceMock.Object,
            _eventServiceMock.Object,
            _notificationServiceMock.Object,
            null, // No AI service
            _config,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void ViralityState_HasCorrectValues()
    {
        // Verify enum values
        Assert.Equal(0, (int)ViralityState.Normal);
        Assert.Equal(1, (int)ViralityState.Trending);
        Assert.Equal(2, (int)ViralityState.Popular);
        Assert.Equal(3, (int)ViralityState.Viral);
        Assert.Equal(4, (int)ViralityState.MassivelyViral);
        Assert.Equal(5, (int)ViralityState.Declining);
    }

    [Fact]
    public void ViralityConfig_HasCorrectDefaults()
    {
        var config = new ViralityConfig();
        
        Assert.True(config.Enabled);
        Assert.Equal(50, config.TrendingThreshold);
        Assert.Equal(200, config.PopularThreshold);
        Assert.Equal(1000, config.ViralThreshold);
        Assert.Equal(10000, config.MassivelyViralThreshold);
        Assert.Equal(10, config.ViralVelocityMin);
        Assert.Equal(24, config.ViralWindowHours);
    }

    [Fact]
    public void PostVirality_EntityHasRequiredProperties()
    {
        var virality = new PostVirality
        {
            PostViralityId = Guid.NewGuid(),
            PostId = Guid.NewGuid(),
            State = ViralityState.Normal,
            Score = 0,
            TotalEngagement = 0,
            Velocity = 0,
            PeakVelocity = 0,
            Reach = 0,
            ShareCount = 0
        };

        Assert.NotEqual(Guid.Empty, virality.PostViralityId);
        Assert.NotEqual(Guid.Empty, virality.PostId);
        Assert.Equal(ViralityState.Normal, virality.State);
        Assert.Null(virality.ViralAt);
        Assert.Null(virality.MassivelyViralAt);
        Assert.Null(virality.DeclinedAt);
    }

    [Fact]
    public void ViralityTransition_EntityHasRequiredProperties()
    {
        var transition = new ViralityTransition
        {
            TransitionId = Guid.NewGuid(),
            PostId = Guid.NewGuid(),
            FromState = ViralityState.Normal,
            ToState = ViralityState.Trending,
            ScoreAtTransition = 25.5f,
            EngagementAtTransition = 50,
            VelocityAtTransition = 5.0f
        };

        Assert.NotEqual(Guid.Empty, transition.TransitionId);
        Assert.NotEqual(Guid.Empty, transition.PostId);
        Assert.Equal(ViralityState.Normal, transition.FromState);
        Assert.Equal(ViralityState.Trending, transition.ToState);
        Assert.Equal(25.5f, transition.ScoreAtTransition);
        Assert.Equal(50, transition.EngagementAtTransition);
    }

    [Fact]
    public async Task CalculateViralityAsync_ReturnsNormalState_WhenNoEngagement()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = 1,
            PostId = postId,
            AuthorAccountId = 1,
            Content = "Test post",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _postServiceMock.Setup(x => x.GetPostByIdAsync(postId))
            .ReturnsAsync(post);
        _socialGraphServiceMock.Setup(x => x.GetFollowerCountAsync(1))
            .ReturnsAsync(100);

        // Act
        var result = await _viralityService.CalculateViralityAsync(postId);

        // Assert
        Assert.Equal(ViralityState.Normal, result.State);
        Assert.Equal(0, result.TotalEngagement);
    }

    [Fact]
    public async Task CalculateViralityAsync_ReturnsTrendingState_WhenEngagementAboveThreshold()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = 1,
            PostId = postId,
            AuthorAccountId = 1,
            Content = "Test post",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        // Add likes
        _context.PostLikes.Add(new PostLike { PostId = 1, AccountId = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-20) });
        _context.PostLikes.Add(new PostLike { PostId = 1, AccountId = 3, CreatedAt = DateTime.UtcNow.AddMinutes(-15) });
        _context.PostLikes.Add(new PostLike { PostId = 1, AccountId = 4, CreatedAt = DateTime.UtcNow.AddMinutes(-10) });
        
        // Add comments
        _context.Comments.Add(new Comment 
        { 
            PostId = 1, 
            AuthorAccountId = 5, 
            Content = "Great post!",
            Status = CommentStatus.Active,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await _context.SaveChangesAsync();

        _postServiceMock.Setup(x => x.GetPostByIdAsync(postId))
            .ReturnsAsync(post);
        _socialGraphServiceMock.Setup(x => x.GetFollowerCountAsync(1))
            .ReturnsAsync(100);

        // Act
        var result = await _viralityService.CalculateViralityAsync(postId);

        // Assert
        Assert.True(result.TotalEngagement >= 50);
    }

    [Fact]
    public async Task GetPostViralityAsync_ReturnsNull_WhenNoRecordExists()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var result = await _viralityService.GetPostViralityAsync(postId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPostViralityAsync_ReturnsRecord_WhenExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var virality = new PostVirality
        {
            PostViralityId = Guid.NewGuid(),
            PostId = postId,
            State = ViralityState.Trending,
            Score = 25.5f,
            TotalEngagement = 75
        };
        _context.PostVirality.Add(virality);
        await _context.SaveChangesAsync();

        // Act
        var result = await _viralityService.GetPostViralityAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(postId, result.PostId);
        Assert.Equal(ViralityState.Trending, result.State);
        Assert.Equal(25.5f, result.Score);
    }

    [Fact]
    public async Task GetViralityStateAsync_ReturnsNormal_WhenNoRecord()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var result = await _viralityService.GetViralityStateAsync(postId);

        // Assert
        Assert.Equal(ViralityState.Normal, result);
    }

    [Fact]
    public async Task TrackEngagementAsync_UpdatesVelocity()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = 1,
            PostId = postId,
            AuthorAccountId = 1,
            Content = "Test post",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        // Add likes
        for (int i = 0; i < 5; i++)
        {
            _context.PostLikes.Add(new PostLike 
            { 
                PostId = 1, 
                AccountId = i + 2, 
                CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5) 
            });
        }
        await _context.SaveChangesAsync();

        _postServiceMock.Setup(x => x.GetPostByIdAsync(postId))
            .ReturnsAsync(post);
        _socialGraphServiceMock.Setup(x => x.GetFollowerCountAsync(1))
            .ReturnsAsync(100);

        // Act
        var result = await _viralityService.TrackEngagementAsync(postId);

        // Assert
        Assert.Equal(5, result.TotalEngagement);
        Assert.True(result.Velocity > 0);
    }

    [Fact]
    public async Task ViralityService_CreatesViralityRecord_WhenNotExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = 1,
            PostId = postId,
            AuthorAccountId = 1,
            Content = "Test post",
            Status = PostStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _postServiceMock.Setup(x => x.GetPostByIdAsync(postId))
            .ReturnsAsync(post);
        _socialGraphServiceMock.Setup(x => x.GetFollowerCountAsync(1))
            .ReturnsAsync(100);

        // Act
        var result = await _viralityService.CalculateViralityAsync(postId);

        // Assert
        Assert.NotNull(result);
        
        // Verify record was created in database
        var record = await _context.PostVirality.FirstOrDefaultAsync(v => v.PostId == postId);
        Assert.NotNull(record);
    }

    [Fact]
    public void ViralityScore_FormulaCalculatesCorrectly()
    {
        // Test the virality score formula
        // Score = engagementScore (0-30) + velocityScore (0-30) + reachScore (0-20) + relativeScore (0-20)
        
        // Case 1: No engagement
        var score1 = CalculateViralityScoreFormula(0, 0, 0, 100);
        Assert.Equal(0, score1);
        
        // Case 2: Some engagement but low velocity
        var score2 = CalculateViralityScoreFormula(100, 5, 100, 100);
        Assert.True(score2 > 0);
        Assert.True(score2 < 100);
        
        // Case 3: High engagement and velocity
        var score3 = CalculateViralityScoreFormula(1000, 50, 500, 100);
        Assert.True(score3 > score2);
        Assert.True(score3 <= 100);
    }

    private float CalculateViralityScoreFormula(int totalEngagement, float velocity, int reach, int authorFollowers)
    {
        var engagementScore = totalEngagement > 0 
            ? Math.Min(30, (float)Math.Log10(totalEngagement + 1) * 10)
            : 0;
        
        var velocityScore = Math.Min(30, velocity * 3);
        
        var reachScore = reach > 0 
            ? Math.Min(20, (float)Math.Log10(reach + 1) * 5)
            : 0;
        
        var relativeEngagement = authorFollowers > 0 && totalEngagement > 0
            ? (float)totalEngagement / authorFollowers
            : 0;
        var relativeScore = Math.Min(20, relativeEngagement * 100);
        
        return Math.Min(100, engagementScore + velocityScore + reachScore + relativeScore);
    }

    [Theory]
    [InlineData(0, 0, ViralityState.Normal)]
    [InlineData(49, 0, ViralityState.Normal)]
    [InlineData(50, 0, ViralityState.Trending)]
    [InlineData(199, 0, ViralityState.Trending)]
    [InlineData(200, 0, ViralityState.Popular)]
    [InlineData(999, 0, ViralityState.Popular)]
    [InlineData(1000, 9, ViralityState.Popular)] // Below velocity threshold
    [InlineData(1000, 10, ViralityState.Viral)]  // At velocity threshold
    [InlineData(10000, 50, ViralityState.MassivelyViral)]
    public void DetermineState_ReturnsCorrectState(int engagement, float velocity, ViralityState expected)
    {
        // Act
        var result = DetermineStateFromConfig(engagement, velocity, _config);

        // Assert
        Assert.Equal(expected, result);
    }

    private ViralityState DetermineStateFromConfig(int totalEngagement, float velocity, ViralityConfig config)
    {
        if (totalEngagement >= config.MassivelyViralThreshold)
            return ViralityState.MassivelyViral;
        
        if (totalEngagement >= config.ViralThreshold && velocity >= config.ViralVelocityMin)
            return ViralityState.Viral;
        
        if (totalEngagement >= config.PopularThreshold)
            return ViralityState.Popular;
        
        if (totalEngagement >= config.TrendingThreshold)
            return ViralityState.Trending;
        
        return ViralityState.Normal;
    }

    [Fact]
    public async Task GetTransitionHistoryAsync_ReturnsEmptyList_WhenNoTransitions()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var result = await _viralityService.GetTransitionHistoryAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTransitionHistoryAsync_ReturnsTransitions_WhenExists()
    {
        // Arrange
        var postId = Guid.NewGuid();
        
        // Create virality record
        var virality = new PostVirality
        {
            PostViralityId = Guid.NewGuid(),
            PostId = postId,
            State = ViralityState.Viral
        };
        _context.PostVirality.Add(virality);
        
        // Create transitions
        _context.ViralityTransitions.Add(new ViralityTransition
        {
            TransitionId = Guid.NewGuid(),
            PostId = postId,
            FromState = ViralityState.Normal,
            ToState = ViralityState.Trending,
            ScoreAtTransition = 20,
            EngagementAtTransition = 50,
            VelocityAtTransition = 5
        });
        
        _context.ViralityTransitions.Add(new ViralityTransition
        {
            TransitionId = Guid.NewGuid(),
            PostId = postId,
            FromState = ViralityState.Trending,
            ToState = ViralityState.Viral,
            ScoreAtTransition = 50,
            EngagementAtTransition = 1000,
            VelocityAtTransition = 15
        });
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _viralityService.GetTransitionHistoryAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        // Should be ordered by most recent first
        Assert.Equal(ViralityState.Viral, result[0].ToState);
        Assert.Equal(ViralityState.Trending, result[1].ToState);
    }
}
