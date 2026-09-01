using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class AiContentGenerationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IAiProviderService> _mockAiProvider;
    private readonly ContentGeneratorService _templateGenerator;
    private readonly AiPromptBuilder _promptBuilder;
    private readonly Mock<ILogger<AiContentGeneratorService>> _mockLogger;
    private readonly AiContentGeneratorService _service;

    public AiContentGenerationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _mockAiProvider = new Mock<IAiProviderService>();
        _templateGenerator = new ContentGeneratorService();
        _promptBuilder = new AiPromptBuilder();
        _mockLogger = new Mock<ILogger<AiContentGeneratorService>>();
        
        _service = new AiContentGeneratorService(
            _mockAiProvider.Object,
            _templateGenerator,
            _promptBuilder,
            _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<NpcProfile> CreateTestNpcAsync(
        string username = "testnpc",
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

        var npc = new NpcProfile
        {
            AccountId = account.Id,
            IsActive = true,
            SimulationIntervalSeconds = 60,
            NextSimulationAt = DateTime.UtcNow,
            ActivityState = NpcActivityState.Idle,
            Personality = new NpcPersonality
            {
                Openness = 0.5,
                Conscientiousness = 0.5,
                Extraversion = 0.5,
                Agreeableness = 0.5,
                Neuroticism = 0.5
            },
            Interests = new List<NpcInterest>()
        };

        _context.NpcProfiles.Add(npc);
        await _context.SaveChangesAsync();

        return npc;
    }

    private Post CreateTestPost(int authorAccountId, string content)
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
        _context.SaveChanges();
        return post;
    }

    [Fact]
    public async Task GeneratePostContent_WhenAiDisabled_UsesTemplateGenerator()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        _mockAiProvider.Setup(x => x.IsEnabled).Returns(false);

        // Act
        var result = _service.GeneratePostContent(npc, new Random());

        // Assert
        Assert.NotNull(result);
        // Template-based content should match known templates
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GeneratePostContent_WhenAiEnabled_UsesAiProvider()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        _mockAiProvider.Setup(x => x.IsEnabled).Returns(true);
        
        var mockService = new Mock<IAiTextGenerationService>();
        mockService.Setup(x => x.IsConfigured).Returns(true);
        mockService.Setup(x => x.GenerateAsync(It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AiGenerationResult.Successful("AI-generated post content", "TestProvider", "test-model", 100));
        mockService.Setup(x => x.GetProviderName()).Returns("TestProvider");
        mockService.Setup(x => x.GetModelName()).Returns("test-model");
        
        _mockAiProvider.Setup(x => x.GetTextGenerationService()).Returns(mockService.Object);

        // Act
        var result = _service.GeneratePostContent(npc, new Random());

        // Assert
        Assert.Equal("AI-generated post content", result);
        mockService.Verify(x => x.GenerateAsync(It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GeneratePostContent_WhenAiFails_FallsBackToTemplate()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        _mockAiProvider.Setup(x => x.IsEnabled).Returns(true);
        
        var mockService = new Mock<IAiTextGenerationService>();
        mockService.Setup(x => x.IsConfigured).Returns(true);
        mockService.Setup(x => x.GenerateAsync(It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AiGenerationResult.Failed("Network error", "NETWORK_ERROR"));
        mockService.Setup(x => x.GetProviderName()).Returns("TestProvider");
        mockService.Setup(x => x.GetModelName()).Returns("test-model");
        
        _mockAiProvider.Setup(x => x.GetTextGenerationService()).Returns(mockService.Object);

        // Act
        var result = _service.GeneratePostContent(npc, new Random());

        // Assert - Should get template-based content, not error
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual("Network error", result);
    }

    [Fact]
    public async Task GenerateCommentContent_WhenAiDisabled_UsesTemplateGenerator()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var authorAccount = new Account
        {
            Username = "author",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active
        };
        _context.Accounts.Add(authorAccount);
        await _context.SaveChangesAsync();
        
        var post = CreateTestPost(authorAccount.Id, "Test post content");
        
        _mockAiProvider.Setup(x => x.IsEnabled).Returns(false);

        // Act
        var result = _service.GenerateCommentContent(npc, post, new Random());

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateCommentContent_WhenAiEnabled_UsesAiProvider()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var authorAccount = new Account
        {
            Username = "author",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active
        };
        _context.Accounts.Add(authorAccount);
        await _context.SaveChangesAsync();
        
        var post = CreateTestPost(authorAccount.Id, "Test post content");
        
        _mockAiProvider.Setup(x => x.IsEnabled).Returns(true);
        
        var mockService = new Mock<IAiTextGenerationService>();
        mockService.Setup(x => x.IsConfigured).Returns(true);
        mockService.Setup(x => x.GenerateAsync(It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AiGenerationResult.Successful("Great post!", "TestProvider", "test-model", 100));
        mockService.Setup(x => x.GetProviderName()).Returns("TestProvider");
        mockService.Setup(x => x.GetModelName()).Returns("test-model");
        
        _mockAiProvider.Setup(x => x.GetTextGenerationService()).Returns(mockService.Object);

        // Act
        var result = _service.GenerateCommentContent(npc, post, new Random());

        // Assert
        Assert.Equal("Great post!", result);
    }

    [Fact]
    public void IsAiEnabled_DelegatesToProviderService()
    {
        // Arrange
        _mockAiProvider.Setup(x => x.IsEnabled).Returns(true);

        // Act
        var result = _service.IsAiEnabled;

        // Assert
        Assert.True(result);
        _mockAiProvider.Verify(x => x.IsEnabled, Times.Once);
    }

    [Fact]
    public async Task BuildPostPrompt_IncludesPersonalityContext()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        npc.Personality = new NpcPersonality
        {
            Openness = 0.8,
            Conscientiousness = 0.7,
            Extraversion = 0.9,
            Agreeableness = 0.6,
            Neuroticism = 0.2
        };

        // Act
        var prompt = _promptBuilder.BuildPostPrompt(npc, new Random());

        // Assert
        Assert.NotEmpty(prompt.SystemPrompt);
        Assert.NotEmpty(prompt.UserPrompt);
        Assert.True(prompt.MaxTokens > 0);
        Assert.True(prompt.Temperature > 0);
        // High extraversion should be reflected in personality context
        Assert.Contains("enthusiastic", prompt.SystemPrompt.ToLowerInvariant());
    }

    [Fact]
    public async Task BuildCommentPrompt_IncludesTargetPostContent()
    {
        // Arrange
        var npc = await CreateTestNpcAsync();
        var authorAccount = new Account
        {
            Username = "author",
            PasswordHash = "hash",
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active
        };
        _context.Accounts.Add(authorAccount);
        await _context.SaveChangesAsync();
        
        var post = CreateTestPost(authorAccount.Id, "This is a test post about AI content generation");

        // Act
        var prompt = _promptBuilder.BuildCommentPrompt(npc, post, new Random());

        // Assert
        Assert.NotEmpty(prompt.SystemPrompt);
        Assert.Contains("This is a test post", prompt.UserPrompt);
    }
}

public class AiProviderServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly ContentGeneratorService _templateGenerator;
    private readonly Mock<ILogger<AiProviderService>> _mockLogger;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public AiProviderServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _templateGenerator = new ContentGeneratorService();
        _mockLogger = new Mock<ILogger<AiProviderService>>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetConfig_WhenNoConfig_ReturnsEmptyConfig()
    {
        // Arrange
        var service = new AiProviderService(
            _context, 
            _mockHttpClientFactory.Object, 
            _templateGenerator, 
            _mockLogger.Object,
            _mockLoggerFactory.Object);

        // Act
        var config = await service.GetConfigAsync();

        // Assert
        Assert.False(config.HasApiKey);
        Assert.Null(config.Provider);
        Assert.False(config.IsEnabled);
    }

    [Fact]
    public async Task UpdateConfig_WithValidOpenAiConfig_ReturnsConfig()
    {
        // Arrange
        var service = new AiProviderService(
            _context, 
            _mockHttpClientFactory.Object, 
            _templateGenerator, 
            _mockLogger.Object,
            _mockLoggerFactory.Object);

        // Act
        var config = await service.UpdateConfigAsync(new UpdateAiConfigRequest
        {
            Provider = "OpenAI",
            Model = "gpt-4o",
            ApiKey = "sk-test-key-12345",
            IsEnabled = true,
            TimeoutSeconds = 30
        });

        // Assert
        Assert.Equal("OpenAI", config.Provider);
        Assert.Equal("gpt-4o", config.Model);
        Assert.True(config.HasApiKey);
        Assert.True(config.IsEnabled);
        // Should not expose raw key
        Assert.Equal("****2345", config.ApiKeyMasked);
        Assert.DoesNotContain("sk-test", config.ApiKeyMasked);
    }

    [Fact]
    public async Task UpdateConfig_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var service = new AiProviderService(
            _context, 
            _mockHttpClientFactory.Object, 
            _templateGenerator, 
            _mockLogger.Object,
            _mockLoggerFactory.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.UpdateConfigAsync(new UpdateAiConfigRequest
            {
                Provider = "InvalidProvider",
                Model = "some-model",
                ApiKey = "test-key"
            }));
    }

    [Fact]
    public async Task UpdateConfig_WithGenericProvider_RequiresBaseUrl()
    {
        // Arrange
        var service = new AiProviderService(
            _context, 
            _mockHttpClientFactory.Object, 
            _templateGenerator, 
            _mockLogger.Object,
            _mockLoggerFactory.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.UpdateConfigAsync(new UpdateAiConfigRequest
            {
                Provider = "Generic",
                Model = "some-model",
                ApiKey = "test-key",
                BaseUrl = null // Missing required BaseUrl
            }));
    }

    [Fact]
    public async Task UpdateConfig_WithValidGenericConfig_StoresBaseUrl()
    {
        // Arrange
        var service = new AiProviderService(
            _context, 
            _mockHttpClientFactory.Object, 
            _templateGenerator, 
            _mockLogger.Object,
            _mockLoggerFactory.Object);

        // Act
        var config = await service.UpdateConfigAsync(new UpdateAiConfigRequest
        {
            Provider = "Generic",
            Model = "deepseek-chat",
            ApiKey = "sk-test-key",
            BaseUrl = "https://api.deepseek.com",
            IsEnabled = true,
            TimeoutSeconds = 30
        });

        // Assert
        Assert.Equal("Generic", config.Provider);
        Assert.Equal("https://api.deepseek.com", config.BaseUrl);
    }

    [Fact]
    public void IsEnabled_WhenNotConfigured_ReturnsFalse()
    {
        // Arrange
        var service = new AiProviderService(
            _context, 
            _mockHttpClientFactory.Object, 
            _templateGenerator, 
            _mockLogger.Object,
            _mockLoggerFactory.Object);

        // Act
        var isEnabled = service.IsEnabled;

        // Assert
        Assert.False(isEnabled);
    }
}

public class AiConfigInfoTests
{
    [Fact]
    public void GetMaskedApiKey_WithValidKey_ReturnsMaskedVersion()
    {
        // Arrange
        var config = new AiProviderConfig
        {
            Provider = "OpenAI",
            Model = "gpt-4",
            ApiKey = "sk-1234567890abcdefghij"
        };

        // Act
        var masked = config.GetMaskedApiKey();

        // Assert
        Assert.StartsWith("****", masked);
        Assert.EndsWith("ghij", masked);
        Assert.DoesNotContain("sk-1234567890", masked);
    }

    [Fact]
    public void GetMaskedApiKey_WithShortKey_ReturnsStars()
    {
        // Arrange
        var config = new AiProviderConfig
        {
            Provider = "OpenAI",
            Model = "gpt-4",
            ApiKey = "abc"
        };

        // Act
        var masked = config.GetMaskedApiKey();

        // Assert
        Assert.Equal("****", masked);
    }

    [Fact]
    public void GetMaskedApiKey_WithEmptyKey_ReturnsStars()
    {
        // Arrange
        var config = new AiProviderConfig
        {
            Provider = "OpenAI",
            Model = "gpt-4",
            ApiKey = ""
        };

        // Act
        var masked = config.GetMaskedApiKey();

        // Assert
        Assert.Equal("****", masked);
    }
}

public class AiProvidersTests
{
    [Theory]
    [InlineData("OpenAI", true)]
    [InlineData("Anthropic", true)]
    [InlineData("Generic", true)]
    [InlineData("openai", true)]
    [InlineData("OPENAI", true)]
    [InlineData("InvalidProvider", false)]
    [InlineData("", false)]
    [InlineData("Google", false)]
    public void IsValid_ReturnsExpectedResult(string provider, bool expected)
    {
        // Act
        var result = AiProviders.IsValid(provider);

        // Assert
        Assert.Equal(expected, result);
    }
}
