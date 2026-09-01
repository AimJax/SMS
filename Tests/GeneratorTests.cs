using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class UsernameGeneratorTests
{
    [Fact]
    public void Generate_CreatesValidUsername()
    {
        // Arrange
        var generator = new UsernameGenerator(12345);

        // Act
        var username = generator.Generate();

        // Assert
        Assert.NotEmpty(username);
        Assert.DoesNotContain(" ", username);
        Assert.All(username, c => Assert.True(char.IsLetterOrDigit(c) || c == '_'));
    }

    [Fact]
    public void Generate_MultipleCreatesUniqueUsernames()
    {
        // Arrange
        var generator = new UsernameGenerator(54321);

        // Act - Generate 100 usernames and track uniqueness
        var usernames = new List<string>();
        var uniqueCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var username = generator.Generate();
            usernames.Add(username);
            if (usernames.Take(i).All(u => u != username))
            {
                uniqueCount++;
            }
        }
        var uniqueSet = usernames.Distinct().Count();

        // Assert - Expect at least 95% uniqueness (allowing for some collision in algorithm)
        Assert.True(uniqueSet >= 95, $"Expected at least 95 unique, got {uniqueSet}");
    }

    [Fact]
    public void Generate_SameSeed_SameSequence()
    {
        // Arrange
        var generator1 = new UsernameGenerator(11111);
        var generator2 = new UsernameGenerator(11111);

        // Act
        var usernames1 = Enumerable.Range(0, 50).Select(_ => generator1.Generate()).ToList();
        var usernames2 = Enumerable.Range(0, 50).Select(_ => generator2.Generate()).ToList();

        // Assert
        Assert.Equal(usernames1, usernames2);
    }

    [Fact]
    public void Generate_DifferentSeed_DifferentSequences()
    {
        // Arrange
        var generator1 = new UsernameGenerator(22222);
        var generator2 = new UsernameGenerator(33333);

        // Act
        var usernames1 = Enumerable.Range(0, 50).Select(_ => generator1.Generate()).ToList();
        var usernames2 = Enumerable.Range(0, 50).Select(_ => generator2.Generate()).ToList();

        // Assert
        Assert.NotEqual(usernames1, usernames2);
    }

    [Fact]
    public void Generate_Batch_Works()
    {
        // Arrange
        var generator = new UsernameGenerator(44444);

        // Act
        var usernames = generator.GenerateBatch(10).ToList();

        // Assert
        Assert.Equal(10, usernames.Count);
        Assert.Equal(10, usernames.Distinct().Count()); // All unique
    }
}

public class ProfileGeneratorTests
{
    [Fact]
    public void GenerateDisplayName_CreatesNonEmpty()
    {
        // Arrange
        var generator = new ProfileGenerator(12345);

        // Act
        var displayName = generator.GenerateDisplayName(AccountType.OrdinaryUser);

        // Assert
        Assert.NotEmpty(displayName);
    }

    [Fact]
    public void GenerateBio_CreatesNonEmpty()
    {
        // Arrange
        var generator = new ProfileGenerator(12345);

        // Act
        var bio = generator.GenerateBio(AccountType.Creator);

        // Assert
        Assert.NotEmpty(bio);
    }

    [Fact]
    public void GenerateAvatarUrl_CreatesValidUrl()
    {
        // Arrange
        var generator = new ProfileGenerator(12345);

        // Act
        var avatarUrl = generator.GenerateAvatarUrl("testuser");

        // Assert
        Assert.NotEmpty(avatarUrl);
        Assert.StartsWith("https://", avatarUrl);
        Assert.Contains("dicebear", avatarUrl);
    }

    [Fact]
    public void GenerateDisplayName_DifferentTypes_CreatesDifferentStyles()
    {
        // Arrange
        var generator = new ProfileGenerator(99999);

        // Act
        var newsName = generator.GenerateDisplayName(AccountType.News);
        var officialName = generator.GenerateDisplayName(AccountType.Official);

        // Assert - Different account types produce different styles
        Assert.NotEqual(newsName, officialName);
    }

    [Fact]
    public void GenerateBio_AllAccountTypes_Work()
    {
        // Arrange
        var generator = new ProfileGenerator(88888);

        // Act & Assert
        foreach (AccountType type in Enum.GetValues<AccountType>())
        {
            var bio = generator.GenerateBio(type);
            Assert.NotEmpty(bio);
        }
    }
}

public class AccountTypeDistributionTests
{
    [Fact]
    public void IsValid_Default_ReturnsTrue()
    {
        // Arrange
        var distribution = AccountTypeDistribution.Default;

        // Act
        var isValid = distribution.IsValid(out var error);

        // Assert
        Assert.True(isValid);
        Assert.Empty(error);
    }

    [Fact]
    public void IsValid_InvalidSum_ReturnsFalse()
    {
        // Arrange
        var distribution = new AccountTypeDistribution
        {
            OrdinaryUser = 50,
            Creator = 30,
            Influencer = 10,
            News = 5,
            Official = 5,
            Celebrity = 5 // Total = 105
        };

        // Act
        var isValid = distribution.IsValid(out var error);

        // Assert
        Assert.False(isValid);
        Assert.Contains("100", error);
    }

    [Fact]
    public void IsValid_NegativeValue_ReturnsFalse()
    {
        // Arrange
        var distribution = new AccountTypeDistribution
        {
            OrdinaryUser = -10,
            Creator = 30,
            Influencer = 30,
            News = 20,
            Official = 20,
            Celebrity = 10
        };

        // Act
        var isValid = distribution.IsValid(out var error);

        // Assert
        Assert.False(isValid);
        Assert.Contains("non-negative", error);
    }

    [Fact]
    public void GetPercentage_ReturnsCorrectValue()
    {
        // Arrange
        var distribution = new AccountTypeDistribution
        {
            OrdinaryUser = 70,
            Creator = 12,
            Influencer = 7,
            News = 5,
            Official = 4,
            Celebrity = 2
        };

        // Act & Assert
        Assert.Equal(70, distribution.GetPercentage(AccountType.OrdinaryUser));
        Assert.Equal(12, distribution.GetPercentage(AccountType.Creator));
        Assert.Equal(7, distribution.GetPercentage(AccountType.Influencer));
        Assert.Equal(5, distribution.GetPercentage(AccountType.News));
        Assert.Equal(4, distribution.GetPercentage(AccountType.Official));
        Assert.Equal(2, distribution.GetPercentage(AccountType.Celebrity));
    }
}
