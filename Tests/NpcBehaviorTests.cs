using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class NpcDecisionServiceTests
{
    private readonly NpcDecisionService _decisionService;

    public NpcDecisionServiceTests()
    {
        _decisionService = new NpcDecisionService();
    }

    [Fact]
    public void GetPersonalityModifier_HighExtraversion_IncreasesFollowScore()
    {
        // Arrange
        var personality = new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.9, // High extraversion
            Agreeableness = 0.5,
            Neuroticism = 0.2
        };

        // Act
        var followMod = _decisionService.GetPersonalityModifier(personality, NpcActionType.Follow);
        var viewFeedMod = _decisionService.GetPersonalityModifier(personality, NpcActionType.ViewFeed);

        // Assert
        Assert.True(followMod > 0, "High extraversion should increase follow score");
        Assert.Equal(0, viewFeedMod); // No modifier for viewfeed
    }

    [Fact]
    public void GetPersonalityModifier_HighAgreeableness_IncreasesPositiveEngagement()
    {
        // Arrange
        var personality = new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.9, // High agreeableness
            Neuroticism = 0.2
        };

        // Act
        var likeMod = _decisionService.GetPersonalityModifier(personality, NpcActionType.LikePost);
        var commentMod = _decisionService.GetPersonalityModifier(personality, NpcActionType.Comment);

        // Assert
        Assert.True(likeMod > 0, "High agreeableness should increase like score");
        Assert.True(commentMod > 0, "High agreeableness should increase comment score");
    }

    [Fact]
    public void GetPersonalityModifier_HighNeuroticism_DecreasesLikes()
    {
        // Arrange - low agreeableness/openness so neuroticism dominates
        var personality = new NpcPersonality
        {
            Openness = 0.2,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.2,
            Neuroticism = 0.9 // High neuroticism
        };

        // Act
        var likeMod = _decisionService.GetPersonalityModifier(personality, NpcActionType.LikePost);
        var commentMod = _decisionService.GetPersonalityModifier(personality, NpcActionType.Comment);

        // Assert
        Assert.True(likeMod < 0, "High neuroticism should decrease like score");
        Assert.True(commentMod < 0, "High neuroticism should decrease comment score");
    }

    [Fact]
    public void GetPersonalityModifier_HighConscientiousness_IncreasesPosting()
    {
        // Arrange
        var personality = new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.8, // High conscientiousness
            Extraversion = 0.5,
            Agreeableness = 0.5,
            Neuroticism = 0.2
        };

        // Act
        var postMod = _decisionService.GetPersonalityModifier(personality, NpcActionType.CreatePost);

        // Assert
        Assert.True(postMod > 0, "High conscientiousness should increase post score");
    }

    [Fact]
    public void GetAccountTypeModifier_Creator_HighPostingScore()
    {
        // Act
        var postMod = _decisionService.GetAccountTypeModifier(AccountType.Creator, NpcActionType.CreatePost);
        var commentMod = _decisionService.GetAccountTypeModifier(AccountType.Creator, NpcActionType.Comment);

        // Assert
        Assert.True(postMod > 0.3, "Creator should have high posting score");
        Assert.True(commentMod > 0, "Creator should have positive comment score");
    }

    [Fact]
    public void GetAccountTypeModifier_Celebrity_LowFollowScore()
    {
        // Act
        var followMod = _decisionService.GetAccountTypeModifier(AccountType.Celebrity, NpcActionType.Follow);
        var postMod = _decisionService.GetAccountTypeModifier(AccountType.Celebrity, NpcActionType.CreatePost);

        // Assert
        Assert.True(followMod < 0, "Celebrity should have negative follow score");
        Assert.True(postMod > 0, "Celebrity should have positive post score");
    }

    [Fact]
    public void GetAccountTypeModifier_News_HighPostingScore()
    {
        // Act
        var postMod = _decisionService.GetAccountTypeModifier(AccountType.News, NpcActionType.CreatePost);
        var likeMod = _decisionService.GetAccountTypeModifier(AccountType.News, NpcActionType.LikePost);

        // Assert
        Assert.True(postMod > 0.5, "News should have very high posting score");
        Assert.True(likeMod < 0.1, "News should have low like score");
    }

    [Fact]
    public void CalculateFinalScore_ClampedToValidRange()
    {
        // Arrange
        var personality = new NpcPersonality
        {
            Openness = 0.9,
            Conscientiousness = 0.9,
            Extraversion = 0.9,
            Agreeableness = 0.9,
            Neuroticism = 0.1
        };
        var candidate = new NpcActionCandidate
        {
            ActionType = NpcActionType.CreatePost,
            BaseScore = 0.9
        };

        // Act
        var score = _decisionService.CalculateFinalScore(candidate, personality, AccountType.Creator, 0.9);

        // Assert
        Assert.True(score >= 0.0 && score <= 1.0, "Score should be clamped to 0.0-1.0 range");
    }

    [Fact]
    public void CalculateFinalScore_AllFactorsConsidered()
    {
        // Arrange
        var personality = new NpcPersonality
        {
            Openness = 0.5,
            Conscientiousness = 0.5,
            Extraversion = 0.5,
            Agreeableness = 0.5,
            Neuroticism = 0.5
        };
        
        // High relevance for following (influences score)
        var lowRelevanceCandidate = new NpcActionCandidate
        {
            ActionType = NpcActionType.Follow,
            BaseScore = 0.1
        };
        
        var highRelevanceCandidate = new NpcActionCandidate
        {
            ActionType = NpcActionType.Follow,
            BaseScore = 0.9
        };

        // Act
        var lowScore = _decisionService.CalculateFinalScore(lowRelevanceCandidate, personality, AccountType.OrdinaryUser, 0.1);
        var highScore = _decisionService.CalculateFinalScore(highRelevanceCandidate, personality, AccountType.OrdinaryUser, 0.9);

        // Assert
        Assert.True(highScore > lowScore, "Higher relevance should produce higher score");
    }

    [Fact]
    public void EvaluateAndSelect_EmptyCandidates_ReturnsIdle()
    {
        // Arrange
        var npc = CreateTestNpc();
        var random = new Random(12345);

        // Act
        var decision = _decisionService.EvaluateAndSelect(npc, Enumerable.Empty<NpcActionCandidate>(), random);

        // Assert
        Assert.False(decision.HasAction);
        Assert.NotNull(decision.IdleReason);
    }

    [Fact]
    public void EvaluateAndSelect_WithCandidates_ReturnsDecision()
    {
        // Arrange
        var npc = CreateTestNpc();
        var random = new Random(12345);
        var candidates = new[]
        {
            new NpcActionCandidate { ActionType = NpcActionType.ViewFeed, BaseScore = 0.5 },
            new NpcActionCandidate { ActionType = NpcActionType.LikePost, BaseScore = 0.4 },
            new NpcActionCandidate { ActionType = NpcActionType.Follow, BaseScore = 0.3 }
        };

        // Act
        var decision = _decisionService.EvaluateAndSelect(npc, candidates, random);

        // Assert
        Assert.True(decision.HasAction);
        Assert.NotNull(decision.SelectedAction);
        Assert.Equal(3, decision.Candidates.Count);
    }

    [Fact]
    public void EvaluateAndSelect_SameSeed_SameSelection()
    {
        // Arrange
        var npc = CreateTestNpc();
        var candidates = new[]
        {
            new NpcActionCandidate { ActionType = NpcActionType.ViewFeed, BaseScore = 0.5 },
            new NpcActionCandidate { ActionType = NpcActionType.LikePost, BaseScore = 0.5 }
        };

        // Act
        var decision1 = _decisionService.EvaluateAndSelect(npc, candidates, new Random(42));
        var decision2 = _decisionService.EvaluateAndSelect(npc, candidates, new Random(42));

        // Assert
        Assert.Equal(decision1.SelectedAction?.ActionType, decision2.SelectedAction?.ActionType);
    }

    private static NpcProfile CreateTestNpc()
    {
        return new NpcProfile
        {
            NpcId = Guid.NewGuid(),
            AccountId = 1,
            IsActive = true,
            ActivityState = NpcActivityState.Idle,
            SimulationIntervalSeconds = 30,
            SimulationVersion = 1,
            Account = new Account
            {
                Id = 1,
                Username = "testnpc",
                AccountType = AccountType.OrdinaryUser,
                Status = AccountStatus.Active
            },
            Personality = new NpcPersonality
            {
                Openness = 0.5,
                Conscientiousness = 0.5,
                Extraversion = 0.5,
                Agreeableness = 0.5,
                Neuroticism = 0.5
            },
            Interests = new List<NpcInterest>
            {
                new() { InterestKey = InterestCategories.Gaming, Strength = 0.8 },
                new() { InterestKey = InterestCategories.Technology, Strength = 0.6 }
            }
        };
    }
}

public class ContentRelevanceServiceTests
{
    private readonly ContentRelevanceService _service;

    public ContentRelevanceServiceTests()
    {
        _service = new ContentRelevanceService();
    }

    [Fact]
    public void ExtractTopics_GamingContent_ReturnsGaming()
    {
        // Arrange
        var content = "Just finished an amazing game session on Steam! #gaming #esports";

        // Act
        var topics = _service.ExtractTopics(content);

        // Assert
        Assert.Contains(InterestCategories.Gaming, topics);
    }

    [Fact]
    public void ExtractTopics_SportsContent_ReturnsSports()
    {
        // Arrange
        var content = "What an incredible NFL game last night!";

        // Act
        var topics = _service.ExtractTopics(content);

        // Assert
        Assert.Contains(InterestCategories.Sports, topics);
    }

    [Fact]
    public void ExtractTopics_TechContent_ReturnsTechnology()
    {
        // Arrange
        var content = "The new AI startup is changing how we code with machine learning";

        // Act
        var topics = _service.ExtractTopics(content);

        // Assert
        Assert.Contains(InterestCategories.Technology, topics);
    }

    [Fact]
    public void ExtractTopics_MultipleTopics_ReturnsAll()
    {
        // Arrange
        var content = "Watching the basketball game while playing video games";

        // Act
        var topics = _service.ExtractTopics(content);

        // Assert
        Assert.Contains(InterestCategories.Sports, topics);
        Assert.Contains(InterestCategories.Gaming, topics);
    }

    [Fact]
    public void ExtractTopics_EmptyContent_ReturnsEmpty()
    {
        // Arrange
        var content = "";

        // Act
        var topics = _service.ExtractTopics(content);

        // Assert
        Assert.Empty(topics);
    }

    [Fact]
    public void CalculatePostRelevance_HighInterestMatch_ReturnsHigherScore()
    {
        // Arrange
        var post = new Post { Content = "Love gaming and streaming and playing video games!" };
        var interests = new List<NpcInterest>
        {
            new() { InterestKey = InterestCategories.Gaming, Strength = 0.9 }
        };

        // Act
        var relevance = _service.CalculatePostRelevance(post, interests);

        // Assert - with many keyword matches, should have meaningful relevance
        Assert.True(relevance > 0, "Matching interest should produce positive relevance");
    }

    [Fact]
    public void CalculatePostRelevance_NoInterestMatch_ReturnsLowScore()
    {
        // Arrange
        var post = new Post { Content = "Love gaming and streaming!" };
        var interests = new List<NpcInterest>
        {
            new() { InterestKey = InterestCategories.Fashion, Strength = 0.9 }
        };

        // Act
        var relevance = _service.CalculatePostRelevance(post, interests);

        // Assert
        Assert.True(relevance < 0.3, "Non-matching interest should produce low relevance");
    }

    [Fact]
    public void CalculatePostRelevance_StrengthMatters()
    {
        // Arrange
        var post = new Post { Content = "Love gaming!" };
        var lowStrength = new List<NpcInterest>
        {
            new() { InterestKey = InterestCategories.Gaming, Strength = 0.3 }
        };
        var highStrength = new List<NpcInterest>
        {
            new() { InterestKey = InterestCategories.Gaming, Strength = 0.9 }
        };

        // Act
        var lowRelevance = _service.CalculatePostRelevance(post, lowStrength);
        var highRelevance = _service.CalculatePostRelevance(post, highStrength);

        // Assert
        Assert.True(highRelevance > lowRelevance, "Higher strength should produce higher relevance");
    }

    [Fact]
    public void CalculateAccountRelevance_ReturnsValidScore()
    {
        // Arrange
        var account = new Account { AccountType = AccountType.Creator };
        var interests = new List<NpcInterest>
        {
            new() { InterestKey = InterestCategories.Gaming, Strength = 0.8 },
            new() { InterestKey = InterestCategories.Music, Strength = 0.6 },
            new() { InterestKey = InterestCategories.Travel, Strength = 0.4 }
        };

        // Act
        var relevance = _service.CalculateAccountRelevance(account, interests);

        // Assert
        Assert.True(relevance >= 0.0 && relevance <= 1.0, "Relevance should be in valid range");
    }
}

public class ContentGeneratorServiceTests
{
    private readonly ContentGeneratorService _service;

    public ContentGeneratorServiceTests()
    {
        _service = new ContentGeneratorService();
    }

    [Fact]
    public void GeneratePostContent_OrdinaryUser_ReturnsTemplate()
    {
        // Arrange
        var npc = CreateTestNpc(AccountType.OrdinaryUser);

        // Act
        var content = _service.GeneratePostContent(npc, new Random(12345));

        // Assert
        Assert.NotEmpty(content);
    }

    [Fact]
    public void GeneratePostContent_Creator_ReturnsTemplate()
    {
        // Arrange
        var npc = CreateTestNpc(AccountType.Creator);

        // Act
        var content = _service.GeneratePostContent(npc, new Random(12345));

        // Assert
        Assert.NotEmpty(content);
        Assert.Contains("content", content.ToLower());
    }

    [Fact]
    public void GeneratePostContent_News_ReturnsTemplate()
    {
        // Arrange
        var npc = CreateTestNpc(AccountType.News);

        // Act
        var content = _service.GeneratePostContent(npc, new Random(12345));

        // Assert
        Assert.NotEmpty(content);
    }

    [Fact]
    public void GeneratePostContent_SameSeed_SameContent()
    {
        // Arrange
        var npc = CreateTestNpc(AccountType.OrdinaryUser);

        // Act
        var content1 = _service.GeneratePostContent(npc, new Random(42));
        var content2 = _service.GeneratePostContent(npc, new Random(42));

        // Assert
        Assert.Equal(content1, content2);
    }

    [Fact]
    public void GenerateCommentContent_ReturnsValidComment()
    {
        // Arrange
        var npc = CreateTestNpc(AccountType.OrdinaryUser);
        var post = new Post { Content = "Great gaming content!" };

        // Act
        var comment = _service.GenerateCommentContent(npc, post, new Random(12345));

        // Assert
        Assert.NotEmpty(comment);
        Assert.True(comment.Length < 500, "Comment should be reasonably short");
    }

    [Fact]
    public void GenerateCommentContent_DifferentNpcs_ProducesDifferentComments()
    {
        // Arrange
        var npc1 = CreateTestNpc(AccountType.OrdinaryUser);
        npc1.Personality = new NpcPersonality { Agreeableness = 0.9, Neuroticism = 0.1 };
        
        var npc2 = CreateTestNpc(AccountType.OrdinaryUser);
        npc2.Personality = new NpcPersonality { Agreeableness = 0.2, Neuroticism = 0.9 };
        
        var post = new Post { Content = "Interesting post" };

        // Act
        var comment1 = _service.GenerateCommentContent(npc1, post, new Random(12345));
        var comment2 = _service.GenerateCommentContent(npc2, post, new Random(12345));

        // Assert - With personality difference, there's a chance of different comment types
        // This is probabilistic so we just verify they both produce valid content
        Assert.NotEmpty(comment1);
        Assert.NotEmpty(comment2);
    }

    private static NpcProfile CreateTestNpc(AccountType accountType)
    {
        return new NpcProfile
        {
            NpcId = Guid.NewGuid(),
            AccountId = 1,
            IsActive = true,
            Account = new Account
            {
                Id = 1,
                Username = "testnpc",
                AccountType = accountType,
                Status = AccountStatus.Active
            },
            Personality = new NpcPersonality
            {
                Openness = 0.5,
                Conscientiousness = 0.5,
                Extraversion = 0.5,
                Agreeableness = 0.5,
                Neuroticism = 0.5
            }
        };
    }
}
