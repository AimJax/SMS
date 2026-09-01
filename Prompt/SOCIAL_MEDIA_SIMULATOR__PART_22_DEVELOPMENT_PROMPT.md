# SOCIAL MEDIA SIMULATOR — PART 22 DEVELOPMENT PROMPT
## NEWS

You are continuing development of the **Social Media Simulator** from the existing project.

**DO NOT restart, redesign, or replace the existing architecture.**

You must inspect the current repository first and build directly on everything already implemented.

---

# CURRENT PROJECT CHECKPOINT

Completed:

```text
01A  Development Environment         COMPLETE
01B  Repository Foundation           COMPLETE
01C  ASP.NET Core Server            COMPLETE
01D  SQLite Foundation              COMPLETE
01E  Android Client Foundation      COMPLETE
01F  Foundation Checkpoint           COMPLETE
02   Backend Architecture           COMPLETE
03   Persistence                   COMPLETE
04   Accounts & Authentication      COMPLETE
05   Social Graph                  COMPLETE
06   Posts & Engagement             COMPLETE
07   Feed & Timeline               COMPLETE
08   NPC Simulator Foundation       COMPLETE
09   NPC Population Generation      COMPLETE
10   NPC Behavior Simulation       COMPLETE
11   NPC Background Simulation      COMPLETE
12   NPC Social Graph             COMPLETE
13   AI Content Generation         COMPLETE
14   Notifications System          COMPLETE
15   Communities                   COMPLETE
16   Advanced Feed                 COMPLETE
17   LLM-Driven Event System       COMPLETE
18   Event Causality & Offline Sim  COMPLETE
19   Virality                      COMPLETE
20   Topics & Trends               COMPLETE
21   Deployment & Testing          COMPLETE
```

Latest commit:

```text
d2d93a8 — Remove old Part 21 prompt (replaced with Deployment Ready version)
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 21.

---

# 1. WHY THIS PART, NOW

Parts 01–21 built a social media platform with rumors, trends, virality, and communities. NPCs can gossip, rumors spread with uncertain truth, trends emerge — but there's no **organized reporting** of what's happening.

Part 22 introduces **News Accounts** — special accounts that detect events, identify trends, and report on what's happening in the network. News transforms the platform from "random social chatter" into a platform that **generates its own media**.

Without news, the platform is like a town where everyone talks but no one reports. With news:
- News accounts cover trending topics
- Viral stories get investigated
- Events get reported
- Rumors get fact-checked
- The network can report on itself
- Information becomes organized journalism

News is foundational to:
- Information credibility (Part 21 rumors → news coverage)
- Reputation (accounts get reported on)
- Social dynamics (news can create drama)
- NPC memory (news records important events)

---

# 2. THE EXISTING PROJECT

The existing backend contains from Parts 01–21:

- Everything from Part 21 and earlier
- **Rumors (Part 21):** Information spreading with uncertain truth
- **Trends (Part 20):** Trending topics tracked
- **Virality (Part 19):** Posts can go viral
- **Events (Part 17):** World events detected
- **Communities (Part 15):** Grouped interests
- **NPCs (Parts 10-13):** NPCs with personalities
- **Posts, Comments, Engagement (Parts 06-07):** Content system

The infrastructure exists:
- Trending topics ready for news coverage
- Viral posts ready to be investigated
- Events ready to be reported
- Rumors ready to be fact-checked
- Communities ready to be covered

Part 22 adds the news layer: special accounts that report on the network.

---

# 3. MASTER ARCHITECTURE PRINCIPLES

## Server Authoritative

News is managed by the server. The server determines what news to generate, which news accounts cover what topics, and how news spreads.

## C# + LLM Hybrid

- C# manages news state, detection, and propagation
- LLM generates news content, articles, and headlines
- Server validates all news-related actions

## Permanent Data Rule

All news articles, news accounts, and news history must NOT be automatically deleted/pruned.

## News ≠ Rumors

```
Rumors spread freely as unverified information.
News reports on rumors, events, and trends with journalistic process.
News can verify or debunk rumors.
```

---

# PART 22 OBJECTIVE

Implement a **News System** where special accounts report on the network:

1. **News Account Entity** — Special accounts that report news
2. **News Article Entity** — Articles written by news accounts
3. **News Coverage** — What news accounts cover
4. **Event Detection** — Detecting what to report
5. **News Generation** — LLM-generated articles
6. **News Feed** — How news appears in feeds
7. **Cross-Community Exposure** — News reaching beyond communities
8. **News API** — Endpoints for news management

Do NOT implement in this part:
- Full fact-checking systems (beyond basic verification)
- News moderation
- Breaking news alerts
- News comments or engagement

---

# PART 22 — REQUIRED FEATURES

## 1. News Account Entity

Create a `NewsAccount` entity (or extend Account with News flag):

```csharp
public class NewsAccount
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }                // The account this news account belongs to
    
    // News Account Identity
    public string NewsName { get; set; }              // "TechDaily", "SportsWire", "GossipDaily"
    public string NewsTagline { get; set; }            // "Your daily tech news"
    public string NewsBio { get; set; }                 // Description of the news outlet
    
    // Coverage
    public NewsCategory Category { get; set; }          // See enum
    public int CredibilityScore { get; set; }          // 0-100, starts at 50
    public int SubscriberCount { get; set; }           // Accounts following for news
    
    // Performance
    public int ArticlesPublished { get; set; }
    public int TotalArticleViews { get; set; }
    public double AccuracyRating { get; set; }         // How often they're correct (0.0-1.0)
    public int BreakingNewsCount { get; set; }         // Breaking news they've reported
    
    // Style
    public NewsTone Tone { get; set; }                // Serious, Casual, Sensational, Balanced
    public int ReportFrequency { get; set; }           // Articles per hour target
    
    // Status
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }              // Verified news source
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum NewsCategory
{
    General,           // General news
    Technology,        // Tech news
    Sports,            // Sports news
    Entertainment,     // Movies, music, celebrities
    Politics,          // Political news
    Business,          // Business and finance
    Science,           // Science and health
    Lifestyle,         // Fashion, food, travel
    Gaming,            // Gaming news
    Gossip,            // Celebrity gossip
    Local,             // Local/community news
    Crime              // Crime and justice
}

public enum NewsTone
{
    Serious,           // Formal, factual
    Casual,            // Relaxed, conversational
    Sensational,       // Dramatic, attention-grabbing
    Balanced           // Neutral, multiple perspectives
}
```

---

## 2. News Article Entity

```csharp
public class NewsArticle
{
    public Guid Id { get; set; }
    public Guid NewsAccountId { get; set; }
    
    // Article Content
    public string Headline { get; set; }               // "Breaking: Major Update Announced"
    public string Summary { get; set; }                // Brief summary (2-3 sentences)
    public string Body { get; set; }                   // Full article (LLM generated)
    public string ImagePrompt { get; set; }            // For potential image generation
    
    // Article Metadata
    public string[] Tags { get; set; }                 // Topics covered
    public NewsCategory Category { get; set; }
    public ArticleType Type { get; set; }             // See enum
    
    // Coverage
    public Guid? CoveredTopicId { get; set; }         // Topic being covered
    public Guid? CoveredRumorId { get; set; }          // Rumor being reported
    public Guid? CoveredEventId { get; set; }          // Event being covered
    public List<Guid> CoveredAccountIds { get; set; } // Accounts mentioned
    
    // Engagement
    public int Views { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
    public int BreakingNewsBonus { get; set; }        // Extra engagement for breaking
    
    // Article Status
    public ArticleStatus Status { get; set; }          // See enum
    public bool IsBreakingNews { get; set; }
    public bool IsVerified { get; set; }              // Sources verified
    public DateTime? PublishedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum ArticleType
{
    Breaking,          // Breaking news
    Report,            // Standard report
    Investigation,     // In-depth investigation
    FactCheck,         // Fact-checking article
    Opinion,           // Opinion piece
    Update,            // Follow-up update
    Roundup            // Summary of multiple stories
}

public enum ArticleStatus
{
    Draft,             // Being written
    Published,         // Live
    Updated,           // Updated with new info
    Retracted,        // Removed/corrected
    Archived           // Old article
}
```

---

## 3. News Account Seeding

Seed news accounts on first run:

```csharp
public class NewsAccountSeeder
{
    private static readonly List<NewsAccountSeed> DefaultAccounts = new()
    {
        new NewsAccountSeed { NewsName = "TechDaily", Category = NewsCategory.Technology, Tone = NewsTone.Serious },
        new NewsAccountSeed { NewsName = "SportsWire", Category = NewsCategory.Sports, Tone = NewsTone.Casual },
        new NewsAccountSeed { NewsName = "Entertainment Now", Category = NewsCategory.Entertainment, Tone = NewsTone.Sensational },
        new NewsAccountSeed { NewsName = "GossipDaily", Category = NewsCategory.Gossip, Tone = NewsTone.Casual },
        new NewsAccountSeed { NewsName = "ScienceWeekly", Category = NewsCategory.Science, Tone = NewsTone.Serious },
        new NewsAccountSeed { NewsName = "GamingInsider", Category = NewsCategory.Gaming, Tone = NewsTone.Casual },
        new NewsAccountSeed { NewsName = "LifestyleHub", Category = NewsCategory.Lifestyle, Tone = NewsTone.Balanced },
        new NewsAccountSeed { NewsName = "PoliticsToday", Category = NewsCategory.Politics, Tone = NewsTone.Serious },
        new NewsAccountSeed { NewsName = "GeneralNews", Category = NewsCategory.General, Tone = NewsTone.Balanced },
        new NewsAccountSeed { NewsName = "BusinessDaily", Category = NewsCategory.Business, Tone = NewsTone.Serious }
    };
    
    public async Task SeedAsync(AppDbContext context)
    {
        foreach (var seed in DefaultAccounts)
        {
            // Create NPC account for news
            var account = new Account
            {
                Username = seed.NewsName.ToLower().Replace(" ", ""),
                Email = $"{seed.NewsName.ToLower().Replace(" ", "")}@news.sms",
                PasswordHash = "N/A", // News accounts don't login
                DisplayName = seed.NewsName,
                Bio = seed.NewsName + " - Your source for " + seed.Category.ToString().ToLower() + " news",
                AccountType = AccountType.News,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
            
            // Create news account
            var newsAccount = new NewsAccount
            {
                AccountId = account.Id,
                NewsName = seed.NewsName,
                NewsTagline = $"Your {seed.Category} news source",
                NewsBio = $"Official {seed.Category} news outlet",
                Category = seed.Category,
                Tone = seed.Tone,
                CredibilityScore = 50,
                ReportFrequency = 2, // 2 articles per hour
                IsActive = true,
                IsVerified = seed.Category == NewsCategory.General || seed.Category == NewsCategory.Politics,
                CreatedAt = DateTime.UtcNow
            };
            
            context.NewsAccounts.Add(newsAccount);
        }
        
        await context.SaveChangesAsync();
    }
}
```

---

## 4. News Generation Service

### INewsService

```csharp
public interface INewsService
{
    Task<NewsArticle> GenerateArticleAsync(Guid newsAccountId, NewsLead lead);
    Task<List<NewsArticle>> GetFeedAsync(Guid accountId, int count = 20);
    Task<NewsArticle> GetArticleAsync(Guid articleId);
    Task<List<NewsLead>> DetectNewsWorthyEventsAsync();
    Task ProcessNewsTickAsync();
}
```

### News Detection

```csharp
public async Task<List<NewsLead>> DetectNewsWorthyEventsAsync()
{
    var leads = new List<NewsLead>();
    
    // 1. Detect trending topics that need coverage
    var trendingTopics = await _trendService.GetGlobalTrendsAsync(5);
    foreach (var trend in trendingTopics)
    {
        if (!await _newsService.HasRecentCoverageAsync(trend.Query))
        {
            leads.Add(new NewsLead
            {
                Type = NewsLeadType.TrendCoverage,
                Headline = $"Trending: {trend.DisplayName}",
                Query = trend.Query,
                Priority = CalculatePriority(trend)
            });
        }
    }
    
    // 2. Detect viral stories
    var viralPosts = await _postService.GetViralPostsAsync(since: DateTime.UtcNow.AddHours(-6));
    foreach (var post in viralPosts)
    {
        if (!await _newsService.HasRecentCoverageAsync($"post:{post.Id}"))
        {
            leads.Add(new NewsLead
            {
                Type = NewsLeadType.ViralStory,
                Headline = "Viral Story Emerging",
                RelatedPostId = post.Id,
                Priority = CalculatePriority(post)
            });
        }
    }
    
    // 3. Detect rumors that need fact-checking
    var activeRumors = await _rumorService.GetActiveRumorsAsync();
    foreach (var rumor in activeRumors.Where(r => r.BelieverCount > 10))
    {
        if (!await _newsService.HasFactCheckAsync(rumor.Id))
        {
            leads.Add(new NewsLead
            {
                Type = NewsLeadType.FactCheck,
                Headline = $"Rumor: {rumor.Subject}",
                RumorId = rumor.Id,
                Priority = CalculateRumorPriority(rumor)
            });
        }
    }
    
    // 4. Detect significant events
    var events = await _eventService.GetSignificantEventsAsync();
    foreach (var evt in events)
    {
        if (!await _newsService.HasEventCoverageAsync(evt.Id))
        {
            leads.Add(new NewsLead
            {
                Type = NewsLeadType.EventCoverage,
                Headline = $"Event: {evt.Title}",
                EventId = evt.Id,
                Priority = CalculateEventPriority(evt)
            });
        }
    }
    
    // Sort by priority
    return leads.OrderByDescending(l => l.Priority).ToList();
}

public class NewsLead
{
    public Guid Id { get; set; }
    public NewsLeadType Type { get; set; }
    public string Headline { get; set; }
    public string Query { get; set; }
    public Guid? RelatedPostId { get; set; }
    public Guid? RumorId { get; set; }
    public Guid? EventId { get; set; }
    public double Priority { get; set; }
}

public enum NewsLeadType
{
    TrendCoverage,
    ViralStory,
    FactCheck,
    EventCoverage,
    Investigation
}
```

### LLM Article Generation

```csharp
public async Task<NewsArticle> GenerateArticleAsync(Guid newsAccountId, NewsLead lead)
{
    var newsAccount = await _newsAccountRepo.GetAsync(newsAccountId);
    var account = await _accountRepo.GetAsync(newsAccount.AccountId);
    
    var prompt = BuildArticlePrompt(newsAccount, lead);
    
    var llmResult = await _aiService.GenerateTextAsync(prompt);
    
    if (!llmResult.Success)
    {
        // Fallback to template-based generation
        return GenerateTemplateArticle(newsAccountId, lead);
    }
    
    var content = llmResult.Text;
    var (headline, summary, body) = ParseArticleContent(content);
    
    var article = new NewsArticle
    {
        NewsAccountId = newsAccountId,
        Headline = headline,
        Summary = summary,
        Body = body,
        Category = GetCategoryFromLead(lead),
        Type = lead.Type == NewsLeadType.FactCheck ? ArticleType.FactCheck : ArticleType.Report,
        IsBreakingNews = lead.Priority > 0.8,
        Status = ArticleStatus.Published,
        PublishedAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };
    
    // Set covered entities
    if (lead.RumorId.HasValue) article.CoveredRumorId = lead.RumorId;
    if (lead.EventId.HasValue) article.CoveredEventId = lead.EventId;
    
    await _articleRepo.CreateAsync(article);
    
    // Update news account stats
    newsAccount.ArticlesPublished++;
    newsAccount.TotalArticleViews += article.Views;
    await _newsAccountRepo.UpdateAsync(newsAccount);
    
    return article;
}

public string BuildArticlePrompt(NewsAccount newsAccount, NewsLead lead)
{
    var toneInstructions = newsAccount.Tone switch
    {
        NewsTone.Serious => "Use formal, professional language. Focus on facts.",
        NewsTone.Casual => "Use conversational, friendly language. Keep it light.",
        NewsTone.Sensational => "Use dramatic language. Emphasize the impact and importance.",
        NewsTone.Balanced => "Present multiple perspectives fairly. Avoid bias.",
        _ => ""
    };
    
    var articleTypeInstructions = lead.Type switch
    {
        NewsLeadType.FactCheck => "This is a fact-check article. Investigate the claim and provide verification.",
        NewsLeadType.TrendCoverage => "Cover this trending topic. Why is it popular? What's the significance?",
        NewsLeadType.ViralStory => "Report on this viral story. What happened? Who's involved?",
        _ => "Write a standard news report on this topic."
    };
    
    return $@"You are {newsAccount.NewsName}, a {newsAccount.Category} news outlet.
Tone: {toneInstructions}

{articleTypeInstructions}

Topic: {lead.Headline}

Generate a news article with:
- Headline: Catchy but accurate
- Summary: 2-3 sentences summarizing the story
- Body: 3-5 paragraphs of detailed coverage

Format your response as:
HEADLINE: [your headline]
SUMMARY: [your summary]
BODY:
[your article body]

Keep the article informative and appropriate for a social media platform.";
}
```

---

## 5. News Feed Integration

News articles appear in regular feeds:

```csharp
public async Task<List<Post>> GetFeedAsync(Guid accountId, int count)
{
    var regularFeed = await _feedService.GetFeedAsync(accountId, count);
    var newsArticles = await GetNewsForFeedAsync(accountId, count / 4); // 25% news
    
    // Interleave news with regular posts
    var combined = new List<Post>();
    var newsIndex = 0;
    
    for (int i = 0; i < regularFeed.Count && combined.Count < count; i++)
    {
        combined.Add(regularFeed[i]);
        
        // Insert news every 4 posts
        if ((i + 1) % 4 == 0 && newsIndex < newsArticles.Count)
        {
            combined.Add(newsArticles[newsIndex]);
            newsIndex++;
        }
    }
    
    return combined;
}
```

---

## 6. Cross-Community Exposure

News reaches beyond the original community:

```csharp
public async Task PropagateNewsAsync(Guid articleId)
{
    var article = await _articleRepo.GetAsync(articleId);
    var communities = await _communityService.GetAllAsync();
    
    // Find relevant communities
    var relevantCommunities = communities
        .Where(c => c.Topics.Any(t => article.Tags.Contains(t)))
        .OrderByDescending(c => c.MemberCount)
        .Take(10)
        .ToList();
    
    foreach (var community in relevantCommunities)
    {
        // Notify community members about news
        await _notificationService.CreateBulkAsync(community.MemberIds, new Notification
        {
            Type = NotificationType.NewsFromFavoriteSource,
            Title = article.Headline,
            RelatedArticleId = article.Id
        });
        
        // Record exposure
        await _exposureRepo.RecordAsync(new NewsExposure
        {
            ArticleId = articleId,
            CommunityId = community.Id,
            Views = 0 // Will be updated when members view
        });
    }
}
```

---

## 7. News Processing Tick

```csharp
public class NewsProcessingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessNewsTickAsync();
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}

public async Task ProcessNewsTickAsync()
{
    // 1. Detect newsworthy events
    var leads = await DetectNewsWorthyEventsAsync();
    
    // 2. Assign leads to news accounts
    var newsAccounts = await _newsAccountRepo.GetActiveAsync();
    
    foreach (var lead in leads.Take(10))
    {
        // Find best matching news account
        var newsAccount = FindBestMatch(newsAccounts, lead);
        
        if (newsAccount != null && ShouldGenerateArticle(newsAccount))
        {
            await GenerateArticleAsync(newsAccount.Id, lead);
            
            // Propagate to communities
            var article = await _articleRepo.GetLatestForAccountAsync(newsAccount.Id);
            await PropagateNewsAsync(article.Id);
        }
    }
}

public bool ShouldGenerateArticle(NewsAccount account)
{
    var recentArticles = await _articleRepo.GetRecentForAccountAsync(
        account.Id, since: DateTime.UtcNow.AddHours(1));
    
    return recentArticles.Count < account.ReportFrequency;
}
```

---

## 8. NPC News Consumption

NPCs read and react to news:

```csharp
public async Task ProcessNpcNewsReactionAsync(Guid npcId)
{
    var npc = await _npcService.GetAsync(npcId);
    var newsFeed = await _newsService.GetFeedAsync(npc.AccountId, 5);
    
    foreach (var news in newsFeed)
    {
        // NPCs might share news
        if (npc.Personality.SharingTendency > 0.5 && Random.NextDouble() < 0.3)
        {
            var shareChance = CalculateShareChance(npc, news);
            if (Random.NextDouble() < shareChance)
            {
                await _postService.ShareArticleAsync(npc.AccountId, news.Id);
            }
        }
        
        // NPCs might comment on news
        if (npc.Personality.DebateTendency > 0.3 && Random.NextDouble() < 0.1)
        {
            await _commentService.AddNewsCommentAsync(npc.AccountId, news.Id);
        }
    }
}
```

---

## 9. News API Endpoints

### News Account Endpoints

```http
GET /api/news/accounts
```
Returns all news accounts.

```http
GET /api/news/accounts/{id}
```
Returns news account details.

### News Article Endpoints

```http
GET /api/news?cursor={cursor}&pageSize={size}
```
Returns latest news articles.

```http
GET /api/news/{id}
```
Returns article details.

```http
GET /api/news/breaking
```
Returns breaking news.

```http
GET /api/news/trending
```
Returns trending news.

### News by Category

```http
GET /api/news/category/{category}
```
Returns news for a specific category.

### News by Topic

```http
GET /api/news/topic/{topic}
```
Returns news about a topic.

---

## 10. Database Migration

### NewsAccounts Table

```sql
CREATE TABLE NewsAccounts (
    Id TEXT PRIMARY KEY,
    AccountId TEXT NOT NULL UNIQUE,
    NewsName TEXT NOT NULL,
    NewsTagline TEXT,
    NewsBio TEXT,
    Category INTEGER NOT NULL,
    CredibilityScore INTEGER NOT NULL DEFAULT 50,
    SubscriberCount INTEGER NOT NULL DEFAULT 0,
    ArticlesPublished INTEGER NOT NULL DEFAULT 0,
    TotalArticleViews INTEGER NOT NULL DEFAULT 0,
    AccuracyRating REAL NOT NULL DEFAULT 0.5,
    BreakingNewsCount INTEGER NOT NULL DEFAULT 0,
    Tone INTEGER NOT NULL,
    ReportFrequency INTEGER NOT NULL DEFAULT 2,
    IsActive INTEGER NOT NULL DEFAULT 1,
    IsVerified INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
);

CREATE INDEX IX_NewsAccounts_Category ON NewsAccounts(Category);
CREATE INDEX IX_NewsAccounts_IsVerified ON NewsAccounts(IsVerified);
```

### NewsArticles Table

```sql
CREATE TABLE NewsArticles (
    Id TEXT PRIMARY KEY,
    NewsAccountId TEXT NOT NULL,
    Headline TEXT NOT NULL,
    Summary TEXT,
    Body TEXT,
    ImagePrompt TEXT,
    Tags TEXT, -- JSON array
    Category INTEGER NOT NULL,
    Type INTEGER NOT NULL,
    CoveredTopicId TEXT,
    CoveredRumorId TEXT,
    CoveredEventId TEXT,
    CoveredAccountIds TEXT, -- JSON array
    Views INTEGER NOT NULL DEFAULT 0,
    Shares INTEGER NOT NULL DEFAULT 0,
    Comments INTEGER NOT NULL DEFAULT 0,
    BreakingNewsBonus INTEGER NOT NULL DEFAULT 0,
    Status INTEGER NOT NULL DEFAULT 0,
    IsBreakingNews INTEGER NOT NULL DEFAULT 0,
    IsVerified INTEGER NOT NULL DEFAULT 0,
    PublishedAt TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (NewsAccountId) REFERENCES NewsAccounts(Id)
);

CREATE INDEX IX_NewsArticles_NewsAccountId ON NewsArticles(NewsAccountId);
CREATE INDEX IX_NewsArticles_Category ON NewsArticles(Category);
CREATE INDEX IX_NewsArticles_PublishedAt ON NewsArticles(PublishedAt DESC);
CREATE INDEX IX_NewsArticles_IsBreaking ON NewsArticles(IsBreakingNews);
CREATE INDEX IX_NewsArticles_Status ON NewsArticles(Status);
```

### NewsExposure Table (for cross-community reach)

```sql
CREATE TABLE NewsExposures (
    Id TEXT PRIMARY KEY,
    ArticleId TEXT NOT NULL,
    CommunityId TEXT NOT NULL,
    Views INTEGER NOT NULL DEFAULT 0,
    ExposedAt TEXT NOT NULL,
    FOREIGN KEY (ArticleId) REFERENCES NewsArticles(Id),
    FOREIGN KEY (CommunityId) REFERENCES Communities(Id)
);

CREATE INDEX IX_NewsExposures_ArticleId ON NewsExposures(ArticleId);
```

---

## 11. Tests

### News Account Tests

```text
News accounts seeded on startup
News accounts have correct categories
News accounts can be retrieved
```

### Article Generation Tests

```text
Articles generated for trends
Articles generated for viral posts
Articles generated for rumors (fact-checks)
Articles have correct tone/style
LLM fallback works when needed
```

### News Feed Tests

```text
News appears in regular feed
News interleaved correctly
Category filtering works
Breaking news appears first
```

### Cross-Community Tests

```text
News propagates to relevant communities
News exposure tracked
```

### API Tests

```text
News endpoints return correct data
Pagination works
Category filtering works
```

### Regression Tests

```text
Existing Parts 01-21 tests still pass
```

---

## 12. Android

Part 22 is backend-focused. Ensure Android models include:
- NewsArticle model with headline, summary, body
- NewsAccount model

No UI changes required for this part.

---

## 13. README — REQUIRED

Document:
- Part 22 completion
- News account entity
- News article entity
- News generation process
- News detection (trends, viral, rumors, events)
- LLM article generation
- Cross-community exposure
- API endpoints
- Database changes
- Tests performed
- Current status
- Next planned part

---

## 14. Git

After implementation:
1. Inspect `git status`
2. Commit: `Implement news system (Part 22)`
3. Push to `origin/main`
4. Verify against origin

---

## 15. DO NOT IMPLEMENT YET

- News comments or engagement
- Breaking news alerts
- News moderation
- Paywalls or premium news
- News image generation

---

## 16. QUALITY REQUIREMENTS

- Correct (news generated appropriately)
- Performant (batch processing)
- Testable
- Permanent (all records persist)
- Realistic (news feels authentic)

---

## 17. FINAL VERIFICATION

```text
Server builds
News accounts seeded
Articles generated for newsworthy events
News appears in feed
Cross-community exposure works
News API returns data
Database migrations applied
Existing tests pass
README updated
Git commit pushed
Working tree clean
```

---

## 18. FINAL SESSION REPORT

```text
# PART 22 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. News Account Architecture
...

## 5. News Article Architecture
...

## 6. News Generation
...

## 7. News Detection
...

## 8. Cross-Community Exposure
...

## 9. API Endpoints
...

## 10. Database Changes
...

## 11. Tests
...

## 12. README
Updated: YES
...

## 13. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 14. Current Project Status
01A-22 COMPLETE

## 15. Intentionally Not Implemented
- News image generation
- Breaking news alerts
- News moderation

## 16. NEXT
NEXT: PART 23 — Permanent Memory
```

**STOP after completing Part 22 and reporting the session log.**
