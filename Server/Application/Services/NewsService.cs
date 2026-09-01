using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using System.Text.Json;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for news generation and management
/// </summary>
public class NewsService : INewsService
{
    private readonly AppDbContext _context;
    private readonly IPostService _postService;
    private readonly IViralityService? _viralityService;
    private readonly IRumorService? _rumorService;
    private readonly NewsConfig _config;
    private readonly ILogger<NewsService> _logger;
    private readonly IAiTextGenerationService? _aiService;

    public NewsService(
        AppDbContext context,
        IPostService postService,
        IViralityService? viralityService,
        IRumorService? rumorService,
        NewsConfig config,
        ILogger<NewsService> logger,
        IAiTextGenerationService? aiService = null)
    {
        _context = context;
        _postService = postService;
        _viralityService = viralityService;
        _rumorService = rumorService;
        _config = config;
        _logger = logger;
        _aiService = aiService;
    }

    #region Creation

    public async Task<NewsArticle?> GenerateArticleAsync(Guid newsAccountId, NewsLead lead)
    {
        if (!_config.Enabled) return null;

        var newsAccount = await _context.NewsAccounts
            .Include(n => n.Account)
            .FirstOrDefaultAsync(n => n.NewsAccountId == newsAccountId);
        if (newsAccount == null) return null;

        // Build prompt for LLM
        var prompt = BuildArticlePrompt(newsAccount, lead);
        string headline, summary, body;

        // Try LLM generation
        if (_aiService != null && _aiService.IsConfigured)
        {
            var request = new AiGenerationRequest
            {
                UserPrompt = prompt,
                MaxTokens = 300,
                Temperature = 0.8
            };
            var llmResult = await _aiService.GenerateAsync(request);
            if (llmResult.Success && !string.IsNullOrWhiteSpace(llmResult.Text))
            {
                (headline, summary, body) = ParseArticleContent(llmResult.Text);
            }
            else
            {
                (headline, summary, body) = GenerateTemplateArticle(lead);
            }
        }
        else
        {
            (headline, summary, body) = GenerateTemplateArticle(lead);
        }

        var article = new NewsArticle
        {
            NewsAccountId = newsAccountId,
            Headline = headline,
            Summary = summary,
            Body = body,
            TagsJson = JsonSerializer.Serialize(new[] { lead.Headline }),
            Category = GetCategoryFromLead(lead),
            Type = lead.Type == NewsLeadType.FactCheck ? ArticleType.FactCheck : ArticleType.Report,
            IsBreakingNews = lead.Priority >= _config.MinPriorityForBreaking / 100.0,
            Status = ArticleStatus.Published,
            PublishedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Set covered entities
        if (lead.RumorId.HasValue) article.CoveredRumorId = lead.RumorId;
        if (lead.EventId.HasValue) article.CoveredEventId = lead.EventId;
        if (lead.TopicId.HasValue) article.CoveredTopicId = lead.TopicId;
        if (lead.RelatedPostId.HasValue) article.RelatedPostId = lead.RelatedPostId;

        _context.NewsArticles.Add(article);

        // Update news account stats
        newsAccount.ArticlesPublished++;
        if (article.IsBreakingNews) newsAccount.BreakingNewsCount++;
        newsAccount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated article '{Headline}' for {NewsName}", headline, newsAccount.NewsName);

        // Propagate to communities
        await PropagateNewsAsync(article.ArticleId);

        return article;
    }

    #endregion

    #region Queries

    public async Task<List<NewsArticle>> GetLatestNewsAsync(int count = 20, NewsCategory? category = null)
    {
        var query = _context.NewsArticles
            .Include(a => a.NewsAccount)
            .Where(a => a.Status == ArticleStatus.Published);

        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }

        return await query
            .OrderByDescending(a => a.IsBreakingNews)
            .ThenByDescending(a => a.PublishedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<NewsArticle?> GetArticleAsync(Guid articleId)
    {
        return await _context.NewsArticles
            .Include(a => a.NewsAccount)
            .ThenInclude(n => n!.Account)
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);
    }

    public async Task<List<NewsArticle>> GetBreakingNewsAsync(int count = 10)
    {
        return await _context.NewsArticles
            .Include(a => a.NewsAccount)
            .Where(a => a.IsBreakingNews && a.Status == ArticleStatus.Published)
            .OrderByDescending(a => a.PublishedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<NewsAccount>> GetNewsAccountsAsync()
    {
        return await _context.NewsAccounts
            .Include(n => n.Account)
            .Where(n => n.IsActive)
            .OrderByDescending(n => n.CredibilityScore)
            .ToListAsync();
    }

    public async Task<NewsAccount?> GetNewsAccountAsync(Guid newsAccountId)
    {
        return await _context.NewsAccounts
            .Include(n => n.Account)
            .FirstOrDefaultAsync(n => n.NewsAccountId == newsAccountId);
    }

    #endregion

    #region Detection

    public async Task<List<NewsLead>> DetectNewsWorthyEventsAsync()
    {
        var leads = new List<NewsLead>();

        // 1. Detect trending topics
        var trendingTopics = await _context.Trends
            .Include(t => t.Topic)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.PostCount)
            .Take(5)
            .ToListAsync();

        foreach (var trend in trendingTopics)
        {
            if (!await HasRecentCoverageAsync(trend.TopicId))
            {
                leads.Add(new NewsLead
                {
                    Type = NewsLeadType.TrendCoverage,
                    Headline = $"Trending: {trend.DisplayName}",
                    Query = trend.Query,
                    TopicId = trend.TopicId,
                    Priority = Math.Min(1.0, trend.PostCount / 100.0)
                });
            }
        }

        // 2. Detect viral stories
        var viralPosts = await _context.PostVirality
            .Include(p => p.Post)
            .Where(p => p.State >= ViralityState.Popular)
            .OrderByDescending(p => p.TotalEngagement)
            .Take(5)
            .ToListAsync();

        foreach (var viral in viralPosts)
        {
            if (viral.Post != null && !await HasRecentCoverageAsync(viral.Post.PostId))
            {
                leads.Add(new NewsLead
                {
                    Type = NewsLeadType.ViralStory,
                    Headline = "Viral Story Emerging",
                    RelatedPostId = viral.Post.PostId,
                    Priority = Math.Min(1.0, viral.TotalEngagement / 1000.0)
                });
            }
        }

        // 3. Detect rumors needing fact-check
        var activeRumors = await _context.Rumors
            .Where(r => r.IsActive && r.TruthStatus == RumorTruthStatus.Unverified)
            .OrderByDescending(r => r.ShareCount)
            .Take(3)
            .ToListAsync();

        foreach (var rumor in activeRumors)
        {
            if (rumor.ShareCount > 10 && !await HasFactCheckAsync(rumor.RumorId))
            {
                leads.Add(new NewsLead
                {
                    Type = NewsLeadType.FactCheck,
                    Headline = $"Rumor: {rumor.Summary}",
                    RumorId = rumor.RumorId,
                    Priority = Math.Min(1.0, rumor.ShareCount / 50.0)
                });
            }
        }

        // 4. Detect significant events
        var events = await _context.Events
            .Where(e => e.Status == EventStatus.Active)
            .OrderByDescending(e => e.CreatedAt)
            .Take(3)
            .ToListAsync();

        foreach (var evt in events)
        {
            if (!await HasEventCoverageAsync(evt.EventId))
            {
                leads.Add(new NewsLead
                {
                    Type = NewsLeadType.EventCoverage,
                    Headline = $"Event: {evt.Title}",
                    EventId = evt.EventId,
                    Priority = 0.7 // Default priority for active events
                });
            }
        }

        return leads.OrderByDescending(l => l.Priority).ToList();
    }

    #endregion

    #region Processing

    public async Task ProcessNewsTickAsync()
    {
        if (!_config.Enabled) return;

        _logger.LogDebug("Processing news tick");

        // Detect newsworthy events
        var leads = await DetectNewsWorthyEventsAsync();
        var newsAccounts = await GetNewsAccountsAsync();

        var processed = 0;
        foreach (var lead in leads.Take(_config.MaxArticlesPerTick))
        {
            // Find best matching news account
            var newsAccount = FindBestMatch(newsAccounts, lead);

            if (newsAccount != null && ShouldGenerateArticle(newsAccount))
            {
                await GenerateArticleAsync(newsAccount.NewsAccountId, lead);
                processed++;
            }
        }

        if (processed > 0)
        {
            _logger.LogInformation("Generated {Count} news articles", processed);
        }
    }

    public async Task PropagateNewsAsync(Guid articleId)
    {
        var article = await _context.NewsArticles
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);
        if (article == null) return;

        // Find relevant communities based on category
        var communities = await _context.Communities
            .Where(c => c.IsActive)
            .Take(10)
            .ToListAsync();

        var relevantCommunities = communities
            .Where(c => c.Name.ToLower().Contains(article.Category.ToString().ToLower()))
            .Take(5)
            .ToList();

        foreach (var community in relevantCommunities)
        {
            var existingExposure = await _context.NewsExposures
                .AnyAsync(e => e.ArticleId == articleId && e.CommunityId == community.Id);

            if (!existingExposure)
            {
                _context.NewsExposures.Add(new NewsExposure
                {
                    ArticleId = articleId,
                    CommunityId = community.Id,
                    ExposedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Private Helpers

    private string BuildArticlePrompt(NewsAccount newsAccount, NewsLead lead)
    {
        var toneInstructions = newsAccount.Tone switch
        {
            NewsTone.Serious => "Use formal, professional language. Focus on facts and accuracy.",
            NewsTone.Casual => "Use conversational, friendly language. Keep it accessible.",
            NewsTone.Sensational => "Use dramatic language. Emphasize impact and importance.",
            NewsTone.Balanced => "Present multiple perspectives fairly. Avoid bias.",
            _ => ""
        };

        var articleTypeInstructions = lead.Type switch
        {
            NewsLeadType.FactCheck => "This is a fact-check article. Investigate the claim and provide verification.",
            NewsLeadType.TrendCoverage => "Cover this trending topic. Why is it popular? What's the significance?",
            NewsLeadType.ViralStory => "Report on this viral story. What happened? Who's involved?",
            NewsLeadType.EventCoverage => "Report on this event. What happened? Who was involved?",
            _ => "Write a standard news report on this topic."
        };

        return $@"You are {newsAccount.NewsName}, a {newsAccount.Category} news outlet.
Tone: {toneInstructions}

{articleTypeInstructions}

Topic: {lead.Headline}

Generate a news article with:
- Headline: Catchy but accurate (max 100 characters)
- Summary: 2-3 sentences summarizing the story
- Body: 2-3 paragraphs of detailed coverage

Format your response as:
HEADLINE: [your headline]
SUMMARY: [your summary]
BODY:
[your article body]

Keep the article informative and appropriate for a social media platform.";
    }

    private (string headline, string summary, string body) ParseArticleContent(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var headline = "News Update";
        var summary = "Latest news update.";
        var body = "Full story to follow.";

        foreach (var line in lines)
        {
            if (line.StartsWith("HEADLINE:", StringComparison.OrdinalIgnoreCase))
                headline = line.Substring(9).Trim();
            else if (line.StartsWith("SUMMARY:", StringComparison.OrdinalIgnoreCase))
                summary = line.Substring(8).Trim();
            else if (line.StartsWith("BODY:", StringComparison.OrdinalIgnoreCase))
                body = content.Substring(content.IndexOf("BODY:", StringComparison.OrdinalIgnoreCase) + 5).Trim();
        }

        return (headline, summary, body);
    }

    private (string headline, string summary, string body) GenerateTemplateArticle(NewsLead lead)
    {
        var typeLabel = lead.Type switch
        {
            NewsLeadType.FactCheck => "Fact Check",
            NewsLeadType.TrendCoverage => "Trending",
            NewsLeadType.ViralStory => "Viral",
            NewsLeadType.EventCoverage => "Breaking",
            _ => "Report"
        };

        return (
            $"{typeLabel}: {lead.Headline}",
            $"A significant development has emerged regarding {lead.Headline}. Coverage is ongoing.",
            $"The {lead.Type.ToString().ToLower().Replace("coverage", "coverage").Replace("story", "story")} has attracted attention across the platform. "
            + $"Further updates will be provided as the story develops. "
            + $"Stay tuned for more details on this developing situation."
        );
    }

    private NewsCategory GetCategoryFromLead(NewsLead lead)
    {
        return lead.Type switch
        {
            NewsLeadType.FactCheck => NewsCategory.General,
            NewsLeadType.TrendCoverage => NewsCategory.General,
            NewsLeadType.ViralStory => NewsCategory.Entertainment,
            NewsLeadType.EventCoverage => NewsCategory.General,
            _ => NewsCategory.General
        };
    }

    private NewsAccount? FindBestMatch(List<NewsAccount> accounts, NewsLead lead)
    {
        var targetCategory = GetCategoryFromLead(lead);
        return accounts.FirstOrDefault(a => a.Category == targetCategory && a.IsActive)
            ?? accounts.FirstOrDefault(a => a.IsActive);
    }

    private bool ShouldGenerateArticle(NewsAccount account)
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentArticles = _context.NewsArticles
            .Count(a => a.NewsAccountId == account.NewsAccountId && a.PublishedAt > oneHourAgo);
        return recentArticles < account.ReportFrequency;
    }

    private async Task<bool> HasRecentCoverageAsync(Guid? topicId)
    {
        if (!topicId.HasValue) return false;
        var recent = DateTime.UtcNow.AddHours(-6);
        return await _context.NewsArticles
            .AnyAsync(a => a.CoveredTopicId == topicId && a.PublishedAt > recent);
    }

    private async Task<bool> HasFactCheckAsync(Guid rumorId)
    {
        var recent = DateTime.UtcNow.AddHours(-6);
        return await _context.NewsArticles
            .AnyAsync(a => a.CoveredRumorId == rumorId && a.Type == ArticleType.FactCheck && a.PublishedAt > recent);
    }

    private async Task<bool> HasEventCoverageAsync(Guid eventId)
    {
        var recent = DateTime.UtcNow.AddHours(-6);
        return await _context.NewsArticles
            .AnyAsync(a => a.CoveredEventId == eventId && a.PublishedAt > recent);
    }

    #endregion
}
