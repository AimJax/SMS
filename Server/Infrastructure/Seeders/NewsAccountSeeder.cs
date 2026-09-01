using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaSimulator.Server.Infrastructure.Seeders;

/// <summary>
/// Seeds news accounts on first run
/// </summary>
public class NewsAccountSeeder
{
    private static readonly List<NewsAccountSeedData> DefaultAccounts = new()
    {
        new() { NewsName = "TechDaily", Category = NewsCategory.Technology, Tone = NewsTone.Serious },
        new() { NewsName = "SportsWire", Category = NewsCategory.Sports, Tone = NewsTone.Casual },
        new() { NewsName = "Entertainment Now", Category = NewsCategory.Entertainment, Tone = NewsTone.Sensational },
        new() { NewsName = "GossipDaily", Category = NewsCategory.Gossip, Tone = NewsTone.Casual },
        new() { NewsName = "ScienceWeekly", Category = NewsCategory.Science, Tone = NewsTone.Serious },
        new() { NewsName = "GamingInsider", Category = NewsCategory.Gaming, Tone = NewsTone.Casual },
        new() { NewsName = "LifestyleHub", Category = NewsCategory.Lifestyle, Tone = NewsTone.Balanced },
        new() { NewsName = "PoliticsToday", Category = NewsCategory.Politics, Tone = NewsTone.Serious },
        new() { NewsName = "GeneralNews", Category = NewsCategory.General, Tone = NewsTone.Balanced },
        new() { NewsName = "BusinessDaily", Category = NewsCategory.Business, Tone = NewsTone.Serious }
    };

    public async Task SeedIfNeededAsync(AppDbContext context)
    {
        // Check if already seeded
        if (await context.NewsAccounts.AnyAsync())
        {
            return;
        }

        foreach (var seed in DefaultAccounts)
        {
            var username = seed.NewsName.ToLower().Replace(" ", "") + "_news";
            var account = new Account
            {
                Username = username,
                UsernameNormalized = username.ToUpperInvariant(),
                Email = $"{username}@news.sms",
                AccountType = AccountType.News, // News account type
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            // Create profile with display name and bio
            var profile = new Profile
            {
                AccountId = account.Id,
                DisplayName = seed.NewsName,
                Bio = seed.NewsName + " - Your source for " + seed.Category.ToString().ToLower() + " news",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Profiles.Add(profile);

            var newsAccount = new NewsAccount
            {
                AccountId = account.Id,
                NewsName = seed.NewsName,
                NewsTagline = $"Your {seed.Category} news source",
                NewsBio = $"Official {seed.Category} news outlet",
                Category = seed.Category,
                Tone = seed.Tone,
                CredibilityScore = 50,
                ReportFrequency = 2,
                IsActive = true,
                IsVerified = seed.Category is NewsCategory.General or NewsCategory.Politics,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.NewsAccounts.Add(newsAccount);
            await context.SaveChangesAsync();
        }
    }

    private class NewsAccountSeedData
    {
        public string NewsName { get; set; } = string.Empty;
        public NewsCategory Category { get; set; }
        public NewsTone Tone { get; set; }
    }
}
