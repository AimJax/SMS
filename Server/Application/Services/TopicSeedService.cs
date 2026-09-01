using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for seeding initial topic data
/// </summary>
public interface ITopicSeedService
{
    /// <summary>
    /// Seed initial topics if none exist
    /// </summary>
    Task<TopicSeedResult> SeedTopicsAsync();
    
    /// <summary>
    /// Check if topics have been seeded
    /// </summary>
    Task<bool> TopicsExistAsync();
}

public class TopicSeedResult
{
    public bool Success { get; set; }
    public int TopicsCreated { get; set; }
    public string? ErrorMessage { get; set; }
    
    public static TopicSeedResult SuccessResult(int count) => new()
    {
        Success = true,
        TopicsCreated = count
    };
    
    public static TopicSeedResult FailureResult(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}

/// <summary>
/// Seeds initial topic data
/// </summary>
public class TopicSeedService : ITopicSeedService
{
    private readonly AppDbContext _context;
    
    // Pre-defined topics with categories
    private static readonly List<(string Name, string DisplayName, TopicCategory Category, string? Description)> DefaultTopics = new()
    {
        // Entertainment
        ("movies", "Movies", TopicCategory.Entertainment, "Film discussions and reviews"),
        ("tv", "TV Shows", TopicCategory.Entertainment, "Television series and streaming content"),
        ("music", "Music", TopicCategory.Entertainment, "Music discussions and recommendations"),
        ("celebrities", "Celebrities", TopicCategory.Entertainment, "Celebrity news and gossip"),
        ("anime", "Anime", TopicCategory.Entertainment, "Japanese animation and manga"),
        ("books", "Books", TopicCategory.Entertainment, "Literature and reading discussions"),
        
        // Gaming
        ("gaming", "Gaming", TopicCategory.Gaming, "Video games and gaming culture"),
        ("esports", "Esports", TopicCategory.Gaming, "Competitive gaming and tournaments"),
        ("pcgaming", "PC Gaming", TopicCategory.Gaming, "Desktop and PC gaming"),
        ("mobilegaming", "Mobile Gaming", TopicCategory.Gaming, "Mobile and tablet games"),
        ("nintendoswitch", "Nintendo Switch", TopicCategory.Gaming, "Nintendo Switch games and discussions"),
        ("playstation", "PlayStation", TopicCategory.Gaming, "PlayStation gaming"),
        ("xbox", "Xbox", TopicCategory.Gaming, "Xbox gaming"),
        
        // Technology
        ("technology", "Technology", TopicCategory.Technology, "Tech news and innovations"),
        ("programming", "Programming", TopicCategory.Technology, "Software development and coding"),
        ("ai", "Artificial Intelligence", TopicCategory.Technology, "AI, machine learning, and automation"),
        ("gadgets", "Gadgets", TopicCategory.Technology, "Tech gadgets and devices"),
        ("smartphones", "Smartphones", TopicCategory.Technology, "Mobile phones and mobile tech"),
        ("science", "Science", TopicCategory.Technology, "Scientific discoveries and research"),
        
        // Sports
        ("sports", "Sports", TopicCategory.Sports, "General sports discussions"),
        ("basketball", "Basketball", TopicCategory.Sports, "NBA and basketball"),
        ("soccer", "Soccer", TopicCategory.Sports, "Football/soccer worldwide"),
        ("football", "American Football", TopicCategory.Sports, "NFL and college football"),
        ("tennis", "Tennis", TopicCategory.Sports, "Tennis and tennis players"),
        
        // Lifestyle
        ("fashion", "Fashion", TopicCategory.Lifestyle, "Fashion trends and style"),
        ("food", "Food", TopicCategory.Lifestyle, "Food and cooking"),
        ("travel", "Travel", TopicCategory.Lifestyle, "Travel and destinations"),
        ("fitness", "Fitness", TopicCategory.Lifestyle, "Health and fitness"),
        ("photography", "Photography", TopicCategory.Lifestyle, "Photo and camera discussions"),
        ("art", "Art", TopicCategory.Lifestyle, "Art and creative expression"),
        
        // Meme Culture
        ("memes", "Memes", TopicCategory.Meme, "Internet memes and humor"),
        ("shitposting", "Shitposting", TopicCategory.Meme, "Casual humor and memes"),
        ("wholesome", "Wholesome", TopicCategory.Meme, "Positive and uplifting content"),
        ("cringe", "Cringe", TopicCategory.Meme, "Awkward and cringe content"),
        
        // News & Politics
        ("news", "News", TopicCategory.News, "Breaking news and current events"),
        ("politics", "Politics", TopicCategory.Politics, "Political discussions"),
        ("worldnews", "World News", TopicCategory.News, "International news"),
    };

    public TopicSeedService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TopicsExistAsync()
    {
        return await _context.Topics.AnyAsync();
    }

    public async Task<TopicSeedResult> SeedTopicsAsync()
    {
        try
        {
            // Check if topics already exist
            if (await TopicsExistAsync())
            {
                return TopicSeedResult.FailureResult("Topics already exist");
            }

            var topics = new List<Topic>();
            var now = DateTime.UtcNow;

            foreach (var (name, displayName, category, description) in DefaultTopics)
            {
                var topic = new Topic
                {
                    Name = name,
                    DisplayName = displayName,
                    Slug = GenerateSlug(name),
                    Description = description,
                    Category = category,
                    IsVerified = true, // Pre-defined topics are verified
                    IsActive = true,
                    PostCount = 0,
                    ActivePostCount = 0,
                    SubscriberCount = 0,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                topics.Add(topic);
            }

            _context.Topics.AddRange(topics);
            await _context.SaveChangesAsync();

            return TopicSeedResult.SuccessResult(topics.Count);
        }
        catch (Exception ex)
        {
            return TopicSeedResult.FailureResult($"Failed to seed topics: {ex.Message}");
        }
    }

    private static string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
        
        // Remove invalid characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        
        return slug.Trim('-');
    }
}
