using Microsoft.Extensions.Logging;
using Moq;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class FeedScoringServiceTests
{
    private readonly FeedScoringService _scoringService;
    private readonly FeedScoringConfig _config;

    public FeedScoringServiceTests()
    {
        _config = new FeedScoringConfig();
        _scoringService = new FeedScoringService(null!, _config);
    }

    [Fact]
    public void RecencyScore_VeryRecent_ReturnsHighScore()
    {
        // Arrange
        var postCreatedAt = DateTime.UtcNow.AddMinutes(-30); // 30 minutes ago

        // Act
        var score = _scoringService.CalculateRecencyScore(postCreatedAt);

        // Assert
        Assert.True(score > 0.9, "Very recent post should have high score");
        Assert.True(score <= 1.0, "Score should not exceed 1.0");
    }

    [Fact]
    public void RecencyScore_OldPost_ReturnsLowScore()
    {
        // Arrange
        var postCreatedAt = DateTime.UtcNow.AddHours(-20); // 20 hours ago

        // Act
        var score = _scoringService.CalculateRecencyScore(postCreatedAt);

        // Assert
        Assert.True(score < 0.3, "Old post should have low score");
    }

    [Fact]
    public void RecencyScore_ExponentialDecay_Works()
    {
        // Arrange - two posts at different ages
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var twoHoursAgo = DateTime.UtcNow.AddHours(-2);
        var fourHoursAgo = DateTime.UtcNow.AddHours(-4);

        // Act
        var scoreOneHour = _scoringService.CalculateRecencyScore(oneHourAgo);
        var scoreTwoHours = _scoringService.CalculateRecencyScore(twoHoursAgo);
        var scoreFourHours = _scoringService.CalculateRecencyScore(fourHoursAgo);

        // Assert
        Assert.True(scoreOneHour > scoreTwoHours, "1h post should score higher than 2h post");
        Assert.True(scoreTwoHours > scoreFourHours, "2h post should score higher than 4h post");
    }

    [Fact]
    public void InterestScore_MatchingInterest_ReturnsOne()
    {
        // Arrange
        var postTopic = "gaming,technology";
        var interests = new[] { "gaming", "music" };

        // Act
        var score = _scoringService.CalculateInterestScore(postTopic, interests);

        // Assert
        Assert.Equal(1.0, score);
    }

    [Fact]
    public void InterestScore_NoMatchingInterest_ReturnsZero()
    {
        // Arrange
        var postTopic = "sports,politics";
        var interests = new[] { "gaming", "music" };

        // Act
        var score = _scoringService.CalculateInterestScore(postTopic, interests);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void InterestScore_EmptyTopic_ReturnsZero()
    {
        // Arrange
        var postTopic = "";
        var interests = new[] { "gaming" };

        // Act
        var score = _scoringService.CalculateInterestScore(postTopic, interests);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void InterestScore_CaseInsensitive_Works()
    {
        // Arrange
        var postTopic = "GAMING";
        var interests = new[] { "gaming" };

        // Act
        var score = _scoringService.CalculateInterestScore(postTopic, interests);

        // Assert
        Assert.Equal(1.0, score);
    }

    [Fact]
    public void RelationshipScore_Following_ReturnsPositive()
    {
        // Arrange
        var isFollowing = true;

        // Act
        var score = _scoringService.CalculateRelationshipScore(1, 2, isFollowing);

        // Assert
        Assert.True(score >= _config.FollowedAccountBaseline, "Following should give at least baseline score");
    }

    [Fact]
    public void RelationshipScore_NotFollowing_ReturnsLowerScore()
    {
        // Arrange
        var isFollowing = false;

        // Act
        var score = _scoringService.CalculateRelationshipScore(1, 2, isFollowing);

        // Assert
        Assert.True(score < _config.FollowedAccountBaseline, "Not following should give lower score");
    }

    [Fact]
    public void RelationshipScore_StrongRelationship_ReturnsHigherScore()
    {
        // Arrange
        var isFollowing = true;

        // Act
        var scoreWithRelationship = _scoringService.CalculateRelationshipScore(
            1, 2, isFollowing, 
            familiarity: 80, friendship: 70, trust: 60);
        var scoreWithoutRelationship = _scoringService.CalculateRelationshipScore(
            1, 2, isFollowing);

        // Assert
        Assert.True(scoreWithRelationship > scoreWithoutRelationship, 
            "Strong relationship should score higher");
    }

    [Fact]
    public void EngagementScore_NormalizesCorrectly()
    {
        // Arrange
        var lowEngagement = (0, 0);
        var highEngagement = (100, 50);

        // Act
        var lowScore = _scoringService.CalculateEngagementScore(
            lowEngagement.Item1, lowEngagement.Item2, 0);
        var highScore = _scoringService.CalculateEngagementScore(
            highEngagement.Item1, highEngagement.Item2, 0);

        // Assert
        Assert.True(highScore > lowScore, "High engagement should score higher");
        Assert.True(highScore <= 1.0, "Score should not exceed 1.0");
    }

    [Fact]
    public void EngagementScore_LogarithmicNormalization_DoesNotDominate()
    {
        // Arrange - extreme engagement
        var extremeEngagement = (10000, 5000, 1000);

        // Act
        var score = _scoringService.CalculateEngagementScore(
            extremeEngagement.Item1, extremeEngagement.Item2, extremeEngagement.Item3);

        // Assert
        Assert.True(score <= 1.0, "Log normalization should prevent extreme domination");
    }

    [Fact]
    public void EngagementScore_VelocityBonus_HighRecentEngagement()
    {
        // Arrange
        var totalEngagement = (100, 50, 0);
        var recentEngagement = (90, 45, 0); // Most engagement is recent

        // Act
        var scoreWithVelocity = _scoringService.CalculateEngagementScore(
            totalEngagement.Item1, totalEngagement.Item2, totalEngagement.Item3,
            recentEngagement.Item1, recentEngagement.Item2, recentEngagement.Item3);
        var scoreWithoutVelocity = _scoringService.CalculateEngagementScore(
            totalEngagement.Item1, totalEngagement.Item2, totalEngagement.Item3);

        // Assert
        Assert.True(scoreWithVelocity > scoreWithoutVelocity, 
            "High velocity engagement should get a bonus");
    }

    [Fact]
    public void CommunityAffinityScore_MemberPost_ReturnsHighScore()
    {
        // Arrange
        var postCommunityId = 1;
        var viewerCommunityIds = new[] { 1, 2, 3 };

        // Act
        var score = _scoringService.CalculateCommunityAffinityScore(postCommunityId, viewerCommunityIds);

        // Assert
        Assert.Equal(_config.CommunityMemberAffinityScore, score);
    }

    [Fact]
    public void CommunityAffinityScore_NonMemberPost_ReturnsZero()
    {
        // Arrange
        var postCommunityId = 5;
        var viewerCommunityIds = new[] { 1, 2, 3 };

        // Act
        var score = _scoringService.CalculateCommunityAffinityScore(postCommunityId, viewerCommunityIds);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void CommunityAffinityScore_NoCommunity_ReturnsZero()
    {
        // Arrange
        int? postCommunityId = null;
        var viewerCommunityIds = new[] { 1, 2, 3 };

        // Act
        var score = _scoringService.CalculateCommunityAffinityScore(postCommunityId, viewerCommunityIds);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void AuthorFameScore_Celebrity_ReturnsHighScore()
    {
        // Arrange
        var celebrityFame = 80.0;

        // Act
        var score = _scoringService.CalculateAuthorFameScore(celebrityFame, null);

        // Assert
        Assert.True(score > 0.5, "Celebrity should get above-neutral fame score");
    }

    [Fact]
    public void AuthorFameScore_UnknownAccount_ReturnsNeutral()
    {
        // Arrange
        double? fame = null;

        // Act
        var score = _scoringService.CalculateAuthorFameScore(fame, null);

        // Assert
        Assert.InRange(score, 0.49, 0.51);
    }

    [Fact]
    public void DiscoveryScore_FollowedAccount_ReturnsZero()
    {
        // Arrange
        var isFollowing = true;

        // Act
        var score = _scoringService.CalculateDiscoveryScore(isFollowing, false);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void DiscoveryScore_NonFollowedNewAuthor_ReturnsHighScore()
    {
        // Arrange
        var isFollowing = false;
        var hasSeenBefore = false;

        // Act
        var score = _scoringService.CalculateDiscoveryScore(isFollowing, hasSeenBefore);

        // Assert
        Assert.Equal(_config.NewDiscoveryScore, score);
    }

    [Fact]
    public void DiscoveryScore_NonFollowedSeenAuthor_ReturnsLowerScore()
    {
        // Arrange
        var isFollowing = false;
        var hasSeenBefore = true;

        // Act
        var score = _scoringService.CalculateDiscoveryScore(isFollowing, hasSeenBefore);

        // Assert
        Assert.Equal(_config.SeenDiscoveryScore, score);
        Assert.True(score < _config.NewDiscoveryScore);
    }

    [Fact]
    public void FinalScore_CombinesAllFactors()
    {
        // Arrange
        var breakdown = new FeedScoreBreakdown
        {
            RecencyScore = 0.8,
            InterestScore = 1.0,
            RelationshipScore = 0.6,
            EngagementScore = 0.4,
            CommunityScore = 0.8,
            FameScore = 0.7,
            DiscoveryScore = 0.3
        };

        // Act
        var finalScore = _scoringService.CalculateFinalScore(breakdown);

        // Assert
        var expectedScore = 
            (breakdown.RecencyScore * _config.RecencyWeight) +
            (breakdown.InterestScore * _config.InterestWeight) +
            (breakdown.RelationshipScore * _config.RelationshipWeight) +
            (breakdown.EngagementScore * _config.EngagementWeight) +
            (breakdown.CommunityScore * _config.CommunityWeight) +
            (breakdown.FameScore * _config.FameWeight) +
            (breakdown.DiscoveryScore * _config.DiscoveryWeight);

        Assert.Equal(expectedScore, finalScore, 3);
    }

    [Fact]
    public void EchoChamberAdjustment_StrongEchoChamber_ReducesDiscovery()
    {
        // Arrange - verify that discovery score is reduced when echo chamber is strong
        var items = new List<ScoredFeedItem>
        {
            new() { 
                FinalScore = 0.5, 
                ScoreBreakdown = new FeedScoreBreakdown 
                { 
                    DiscoveryScore = 0.8,
                    RecencyScore = 0.0,
                    InterestScore = 0.0,
                    RelationshipScore = 0.0,
                    EngagementScore = 0.0,
                    CommunityScore = 0.0,
                    FameScore = 0.0
                } 
            }
        };
        var strongEchoChamber = 1.0;
        var originalDiscoveryContribution = 0.8 * _config.DiscoveryWeight;

        // Act
        var adjusted = _scoringService.ApplyEchoChamberAdjustment(items, strongEchoChamber).ToList();

        // Assert - with echo chamber strength 1.0, discovery weight becomes 0
        // So the discovery contribution should be reduced to near 0
        var adjustedDiscoveryContribution = adjusted[0].FinalScore;
        Assert.True(adjustedDiscoveryContribution < originalDiscoveryContribution,
            $"Discovery contribution should be reduced. Original: {originalDiscoveryContribution}, Adjusted: {adjustedDiscoveryContribution}");
    }

    [Fact]
    public void DiscoveryQuota_EnforcesMinimumPercentage()
    {
        // Arrange
        var items = new List<ScoredFeedItem>();
        for (int i = 0; i < 20; i++)
        {
            items.Add(new ScoredFeedItem
            {
                FinalScore = 1.0 - (i * 0.01),
                ScoreBreakdown = new FeedScoreBreakdown { DiscoveryScore = i < 2 ? 0.8 : 0.0 }
            });
        }

        // Act
        var result = _scoringService.EnforceDiscoveryQuota(items, 10).ToList();

        // Assert
        var discoveryCount = result.Count(i => i.ScoreBreakdown.DiscoveryScore > 0);
        var minDiscovery = (int)(10 * _config.MinDiscoveryPercentage);
        Assert.True(discoveryCount >= minDiscovery, 
            $"Should have at least {minDiscovery} discovery items, got {discoveryCount}");
    }
}

public class FeedConfigTests
{
    [Fact]
    public void DefaultWeights_SumToOne()
    {
        // Arrange
        var config = new FeedScoringConfig();

        // Act
        var totalWeight = config.RecencyWeight + config.InterestWeight + 
            config.RelationshipWeight + config.EngagementWeight + 
            config.CommunityWeight + config.FameWeight + config.DiscoveryWeight;

        // Assert
        Assert.Equal(1.0, totalWeight, 2);
    }

    [Fact]
    public void Config_CanBeModified()
    {
        // Arrange
        var config = new FeedScoringConfig();

        // Act
        config.RecencyWeight = 0.5;
        config.DiscoveryWeight = 0.0;

        // Assert
        Assert.Equal(0.5, config.RecencyWeight, 2);
        Assert.Equal(0.0, config.DiscoveryWeight, 2);
    }
}
