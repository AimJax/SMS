# SOCIAL MEDIA SIMULATOR — PART 23 DEVELOPMENT PROMPT
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
03   Persistence                     COMPLETE
04   Accounts & Authentication       COMPLETE
05   Social Graph                    COMPLETE
06   Posts & Engagement              COMPLETE
07   Feed & Timeline                 COMPLETE
08   NPC Simulator Foundation       COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation         COMPLETE
11   NPC Background Simulation       COMPLETE
12   NPC Social Graph                COMPLETE
13   AI Content Generation           COMPLETE
14   Notifications System            COMPLETE
15   Communities                     COMPLETE
16   Advanced Feed                   COMPLETE
17   LLM-Driven Event System        COMPLETE
18   Event Causality & Offline Sim   COMPLETE
19   Virality                        COMPLETE
20   Topics & Trends                 COMPLETE
21   Rumors & Misinformation         COMPLETE
22   Deployment & Testing           COMPLETE
```

Latest commit:

```text
c47a0bc — Part 22: Rumors & Misinformation
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 22.

---

# 1. WHY THIS PART, NOW

Parts 01–22 built a social media platform with rumors, trends, virality, communities, and events. NPCs can gossip, rumors spread with uncertain truth, trends emerge — but there's no **organized reporting** of what's happening.

Part 23 introduces **News Accounts** — special accounts that detect events, identify trends, and report on what's happening in the network. News transforms the platform from "random social chatter" into a platform that **generates its own media**.

Without news, the platform is like a town where everyone talks but no one reports. With news:
- News accounts cover trending topics
- Viral stories get investigated
- Events get reported
- Rumors get fact-checked
- The network can report on itself
- Information becomes organized journalism

News is foundational to:
- Information credibility (news reports on rumors from Part 22)
- Reputation (accounts get reported on)
- Social dynamics (news can create drama)
- NPC memory (news records important events)

---

# 2. THE EXISTING PROJECT

The existing backend contains from Parts 01–22:

- Everything from Part 22 and earlier
- **Rumors & Misinformation (Part 22):** Information spreading with uncertain truth
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

Part 23 adds the news layer: special accounts that report on the network.

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

# PART 23 OBJECTIVE

Implement a **News System** where special accounts report on the network:

1. **NewsAccount Entity** — Special accounts that report news
2. **NewsArticle Entity** — Articles written by news accounts
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
- News image generation

---

# PART 23 — REQUIRED FEATURES

## 1. NewsAccount Entity

Create a `NewsAccount` entity:

```csharp
public class NewsAccount
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }                // The NPC account this news account belongs to
    
    // News Account Identity
    public string NewsName { get; set; }              // "TechDaily", "SportsWire", "GossipDaily"
    public string NewsTagline { get; set; }            // "Your daily tech news"
    public string NewsBio { get; set; }               // Description of the news outlet
    
    // Coverage
    public NewsCategory Category { get; set; }         // See enum
    public int CredibilityScore { get; set; }          // 0-100, starts at 50
    public int SubscriberCount { get; set; }           // Accounts following for news
    
    // Performance
    public int ArticlesPublished { get; set; }
    public int TotalArticleViews { get; set; }
    public double AccuracyRating { get; set; }        // How often they're correct (0.0-1.0)
    public int BreakingNewsCount { get; set; }        // Breaking news they've reported
    
    // Style
    public NewsTone Tone { get; set; }                // Serious, Casual, Sensational, Balanced
    public int ReportFrequency { get; set; }          // Articles per hour target
    
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

## 2. NewsArticle Entity

```csharp
public class NewsArticle
{
    public Guid Id { get; set; }
    public Guid NewsAccountId { get; set; }
    
    // Article Content
    public string Headline { get; set; }               // "Breaking: Major Update Announced"
    public string Summary { get; set; }               // Brief summary (2-3 sentences)
    public string Body { get; set; }                   // Full article (LLM generated)
    
    // Article Metadata
    public List<string> Tags { get; set; }             // Topics covered
    public NewsCategory Category { get; set; }
    public ArticleType Type { get; set; }             // See enum
    
    // Coverage
    public Guid? CoveredTopicId { get; set; }         // Topic being covered
    public Guid? CoveredRumorId { get; set; }         // Rumor being reported
    public Guid? CoveredEventId { get; set; }         // Event being covered
    public List<Guid> CoveredAccountIds { get; set; }  // Accounts mentioned
    
    // Engagement
    public int Views { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
    public int BreakingNewsBonus { get; set; }        // Extra engagement for breaking
    
    // Article Status
    public ArticleStatus Status { get; set; }         // See enum
    public bool IsBreakingNews { get; set; }
    public bool IsVerified { get; set; }             // Sources verified
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
    Retracted,         // Removed/corrected
    Archived           // Old article
}
```

---

## 3. NewsAccount Seeding

Seed news accounts on first run:

```csharp
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
    
    public async Task SeedAsync(AppDbContext context)
    {
        foreach (var seed in DefaultAccounts)
        {
            // Create NPC account for news
            var account = new Account
            {
                Username = seed.NewsName.ToLower().Replace(" ", "") + "_news",
                Email = $"{seed.NewsName.ToLower().Replace(" ", "")}@news.sms",
                DisplayName = seed.NewsName,
                Bio = seed.NewsName + " - Your source for " + seed.Category.ToString().ToLower() + " news",
                AccountType = AccountType.NPC,
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
                IsVerified = seed.Category is NewsCategory.General or NewsCategory.Politics,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            context.NewsAccounts.Add(newsAccount);
        }
        
        await context.SaveChangesAsync();
    }
}
```

---

## 4. News Service

### INewsService

```csharp
public interface INewsService
{
    // Creation
    Task<NewsArticle> GenerateArticleAsync(Guid newsAccountId, NewsLead lead);
    
    // Queries
    Task<List<NewsArticle>> GetLatestNewsAsync(int count = 20, NewsCategory? category = null);
    Task<NewsArticle?> GetArticleAsync(Guid articleId);
    Task<List<NewsArticle>> GetBreakingNewsAsync(int count = 10);
    Task<List<NewsAccount>> GetNewsAccountsAsync();
    
    // Detection
    Task<List<NewsLead>> DetectNewsWorthyEventsAsync();
    
    // Processing
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
        if (!await HasRecentCoverageAsync(trend.Query))
        {
            leads.Add(new NewsLead
            {
                Type = NewsLeadType.TrendCoverage,
                Headline = $"Trending: {trend.DisplayName}",
                Query = trend.Query,
                TopicId = trend.TopicId,
                Priority = CalculatePriority(trend)
            });
        }
    }
    
    // 2. Detect viral stories
    var viralPosts = await _postService.GetViralPostsAsync(since: DateTime.UtcNow.AddHours(-6));
    foreach (var post in viralPosts)
    {
        if (!await HasRecentCoverageAsync($"post:{post.Id}"))
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
        if (!await HasFactCheckAsync(rumor.Id))
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
        if (!await HasEventCoverageAsync(evt.Id))
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
    
    return leads.OrderByDescending(l => l.Priority).ToList();
}

public class NewsLead
{
    public NewsLeadType Type { get; set; }
    public string Headline { get; set; }
    public string Query { get; set; }
    public Guid? TopicId { get; set; }
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
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    // Set covered entities
    if (lead.RumorId.HasValue) article.CoveredRumorId = lead.RumorId;
    if (lead.EventId.HasValue) article.CoveredEventId = lead.EventId;
    if (lead.TopicId.HasValue) article.CoveredTopicId = lead.TopicId;
    
    await _articleRepo.CreateAsync(article);
    
    // Update news account stats
    newsAccount.ArticlesPublished++;
    newsAccount.TotalArticleViews += article.Views;
    if (article.IsBreakingNews) newsAccount.BreakingNewsCount++;
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
public async Task<List<Post>> GetFeedWithNewsAsync(Guid accountId, int count)
{
    var regularFeed = await _feedService.GetFeedAsync(accountId, count);
    var newsArticles = await GetNewsForAccountAsync(accountId, count / 4); // 25% news
    
    // Interleave news with regular posts
    var combined = new List<Post>();
    var newsIndex = 0;
    
    for (int i = 0; i < regularFeed.Count && combined.Count < count; i++)
    {
        combined.Add(regularFeed[i]);
        
        // Insert news every 4 posts
        if ((i + 1) % 4 == 0 && newsIndex < newsArticles.Count)
        {
            combined.Add(ConvertArticleToPost(newsArticles[newsIndex]));
            newsIndex++;
        }
    }
    
    return combined;
}

public Post ConvertArticleToPost(NewsArticle article)
{
    return new Post
    {
        // News article displayed as a special post
        Id = article.Id,
        AuthorId = article.NewsAccount?.AccountId ?? Guid.Empty,
        Content = $"📰 {article.Headline}\n\n{article.Summary}",
        IsNewsArticle = true,
        NewsArticleId = article.Id,
        Likes = article.Views,
        CreatedAt = article.PublishedAt ?? article.CreatedAt
    };
}
```

---

## 6. Cross-Community Exposure

News reaches beyond the original community:

```csharp
public async Task PropagateNewsAsync(Guid articleId)
{
    var article = await _articleRepo.GetAsync(articleId);
    
    // Find relevant communities based on tags/category
    var communities = await _communityService.GetAllAsync();
    
    var relevantCommunities = communities
        .Where(c => c.Topics.Any(t => article.Tags.Contains(t.Name)))
        .OrderByDescending(c => c.MemberCount)
        .Take(10)
        .ToList();
    
    foreach (var community in relevantCommunities)
    {
        // Record exposure
        await _exposureRepo.CreateAsync(new NewsExposure
        {
            ArticleId = articleId,
            CommunityId = community.Id,
            ExposedAt = DateTime.UtcNow
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
    
    // 2. Get active news accounts
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

public NewsAccount? FindBestMatch(List<NewsAccount> accounts, NewsLead lead)
{
    // Match by category/topic
    return accounts.FirstOrDefault(a => 
        a.Category == GetCategoryFromLead(lead) && a.IsActive);
}

public bool ShouldGenerateArticle(NewsAccount account)
{
    var recentArticles = _articleRepo.GetRecentForAccount(
        account.Id, since: DateTime.UtcNow.AddHours(1));
    
    return recentArticles.Count < account.ReportFrequency;
}
```

---

## 8. News API Endpoints

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
GET /api/news?cursor={cursor}&pageSize={size}&category={category}
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

### News by Topic

```http
GET /api/news/topic/{topic}
```
Returns news about a topic.

---

## 9. Database Migration

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

### NewsExposures Table

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

## 10. Tests

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
Existing Parts 01-22 tests still pass
```

---

## 11. Android

Part 23 is backend-focused. Ensure Android models include:
- NewsArticle model with headline, summary, body
- NewsAccount model

No UI changes required for this part.

---

## 12. README — REQUIRED

Document:
- Part 23 completion
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

## 13. Git

After implementation:
1. Inspect `git status`
2. Commit: `Implement news system (Part 23)`
3. Push to `origin/main`
4. Verify against origin

---

## 14. DO NOT IMPLEMENT YET

- News comments or engagement
- Breaking news alerts
- News moderation
- Paywalls or premium news
- News image generation

---

## 15. QUALITY REQUIREMENTS

- Correct (news generated appropriately)
- Performant (batch processing)
- Testable
- Permanent (all records persist)
- Realistic (news feels authentic)

---

## 16. FINAL VERIFICATION

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

## 17. FINAL SESSION REPORT

```text
# PART 23 — COMPLETE

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
01A-23 COMPLETE

## 15. Intentionally Not Implemented
- News image generation
- Breaking news alerts
- News moderation

## 16. NEXT
NEXT: PART 24 — Android UI Implementation (Make the app usable)
```

**STOP after completing Part 23 and reporting the session log.**
