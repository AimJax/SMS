using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for news generation and management
/// </summary>
public interface INewsService
{
    // Creation
    Task<NewsArticle?> GenerateArticleAsync(Guid newsAccountId, NewsLead lead);
    
    // Queries
    Task<List<NewsArticle>> GetLatestNewsAsync(int count = 20, NewsCategory? category = null);
    Task<NewsArticle?> GetArticleAsync(Guid articleId);
    Task<List<NewsArticle>> GetBreakingNewsAsync(int count = 10);
    Task<List<NewsAccount>> GetNewsAccountsAsync();
    Task<NewsAccount?> GetNewsAccountAsync(Guid newsAccountId);
    
    // Detection
    Task<List<NewsLead>> DetectNewsWorthyEventsAsync();
    
    // Processing
    Task ProcessNewsTickAsync();
    
    // Cross-community
    Task PropagateNewsAsync(Guid articleId);
}

/// <summary>
/// Represents a potential news story to cover
/// </summary>
public class NewsLead
{
    public NewsLeadType Type { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public Guid? TopicId { get; set; }
    public Guid? RelatedPostId { get; set; }
    public Guid? RumorId { get; set; }
    public Guid? EventId { get; set; }
    public double Priority { get; set; }
}

/// <summary>
/// Configuration for news system
/// </summary>
public class NewsConfig
{
    public bool Enabled { get; set; } = true;
    public int ProcessingIntervalMinutes { get; set; } = 30;
    public int MaxArticlesPerTick { get; set; } = 10;
    public int MinPriorityForBreaking { get; set; } = 80;
    public int ArticleExpirationDays { get; set; } = 365;
}
