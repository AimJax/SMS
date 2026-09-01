# SOCIAL MEDIA SIMULATOR — PART 20 DEVELOPMENT PROMPT
## TOPICS & TRENDS

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
01F  Foundation Checkpoint          COMPLETE
02   Backend Architecture           COMPLETE
03   Persistence                    COMPLETE
04   Accounts & Authentication      COMPLETE
05   Social Graph                   COMPLETE
06   Posts & Engagement             COMPLETE
07   Feed & Timeline                COMPLETE
08   NPC Simulator Foundation        COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation        COMPLETE
11   NPC Background Simulation      COMPLETE
12   NPC Social Graph              COMPLETE
13   AI Content Generation         COMPLETE
14   Notifications System          COMPLETE
15   Communities                   COMPLETE
16   Advanced Feed                 COMPLETE
17   LLM-Driven Event System       COMPLETE
18   Event Causality & Offline Sim  COMPLETE
19   Virality                      COMPLETE
```

Latest commit:

```text
4c85cab — Part 19: Virality System - Post viral states, metrics, consequences
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 19.

---

# 1. WHY THIS PART, NOW

Part 19 implemented virality — posts can now organically become viral, triggering consequences. But viral posts exist in isolation. Part 20 connects viral content into **Topics and Trends** — the social phenomenon where multiple people discuss the same thing at the same time.

Without topics and trends, the platform feels like isolated conversations. With trends, users see what everyone's talking about, communities develop their own local trends, and mainstream trends emerge that span the entire network.

Trends are foundational to:
- Rumors (Part 21) — rumors spread through trends
- News (Part 22) — news covers trending topics
- Echo Chambers (Part 16 Advanced Feed) — trends strengthen echo chambers
- Content Discovery — trending topics appear in feeds
- Social Identity — "what's trending" defines the community

---

# 2. THE EXISTING PROJECT

The existing backend contains from Parts 01–19:

- Everything from Part 19 and earlier
- **Virality (Part 19):** Posts can go viral with engagement metrics
- Posts with Topic field and hashtag support
- Communities with their own topics (Part 15)
- Event system for detecting world events (Part 17)
- Advanced Feed with topic-based scoring (Part 16)
- NPC Behavior with topic interests

The infrastructure exists:
- Posts have topics and hashtags
- Engagement data for trend detection
- Communities grouped by topic
- Virality metrics for trending calculation

Part 20 adds formal trend tracking, trend propagation, and discovery.

---

# 3. MASTER ARCHITECTURE PRINCIPLES

## Server Authoritative

Trends are calculated and managed by the server. The server determines what's trending, how strong trends are, and how they propagate.

## C# + LLM Hybrid

- C# calculates trend metrics deterministically
- LLM may assist with topic extraction, hashtag grouping, trend narrative

## Permanent Data Rule

All trend records, trend history, and topic associations must NOT be automatically deleted/pruned.

## Performance

Trend calculations must be efficient. Use batch processing, caching, and incremental updates.

---

# PART 20 OBJECTIVE

Implement a complete **Topics & Trends System**:

1. **Topic Entity** — Formal topic definitions
2. **Trend Entity** — Trending topics with metrics
3. **Trend Calculation** — How trends are detected and scored
4. **Trend Types** — Mainstream, Community, Personal trends
5. **Trend Propagation** — How trends spread between communities
6. **Trend Discovery** — How users find trends
7. **Hashtag Management** — Tag extraction and grouping
8. **Trend API** — Endpoints for trending content

Do NOT implement in this part:
- Rumor mechanics (Part 21)
- News coverage (Part 22)
- Trend predictions
- Trend analytics dashboard

---

# PART 20 — REQUIRED FEATURES

## 1. Topic Entity

Create a `Topic` entity:

```csharp
public class Topic
{
    public Guid Id { get; set; }
    public string Name { get; set; }                    // "technology", "gaming", "politics"
    public string DisplayName { get; set; }             // "Technology", "Gaming", "Politics"
    public string Slug { get; set; }                    // URL-safe version
    public string Description { get; set; }              // Optional description
    public TopicCategory Category { get; set; }          // See enum below
    public int PostCount { get; set; }                  // Total posts ever with this topic
    public int ActivePostCount { get; set; }             // Posts in last 7 days
    public int SubscriberCount { get; set; }             // Users following this topic
    public bool IsVerified { get; set; }                 // Official topic
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum TopicCategory
{
    General,          // General interest
    Entertainment,    // Movies, TV, Music
    Gaming,           // Video games
    Technology,       // Tech and science
    Sports,           // Sports
    Politics,         // Political topics
    News,             // News and current events
    Lifestyle,        // Fashion, food, travel
    Art,              // Art and creative
    Meme,             // Meme culture
    Community,        // Community-specific
    Event,            // Event-based (live tweets)
    Hashtag           // Viral hashtag (auto-created)
}
```

### Pre-defined Topics

Seed the system with common topics:

```text
# Entertainment
movies, tv, music, celebrities, anime, books

# Gaming  
gaming, esports, pcgaming, mobilegaming, nintendoswitch, playstation, xbox

# Technology
technology, programming, ai, gadgets, smartphones

# Sports
sports, basketball, soccer, football, tennis, esports

# Lifestyle
fashion, food, travel, fitness, photography, art

# Meme Culture
memes, shitposting, wholesome, cringe

# News & Politics
news, politics, worldnews, science
```

---

## 2. Hashtag Management

### Hashtag Entity

```csharp
public class Hashtag
{
    public Guid Id { get; set; }
    public string Tag { get; set; }                    // "#Gaming", "#AI"
    public string NormalizedTag { get; set; }          // "gaming", "ai" (lowercase, no #)
    public Guid? TopicId { get; set; }                 // Associated topic (nullable)
    public int UsageCount { get; set; }                 // Total times used
    public int TodayUsageCount { get; set; }            // Used today
    public bool IsTrending { get; set; }                // Currently trending
    public DateTime? TrendingSince { get; set; }        // When it started trending
    public int TrendingRank { get; set; }               // Current trend rank
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Hashtag Extraction

Extract hashtags from post content:

```csharp
public List<string> ExtractHashtags(string content)
{
    var regex = new Regex(@"#(\w+)");
    var matches = regex.Matches(content);
    return matches.Select(m => m.Groups[1].Value.ToLower()).Distinct().ToList();
}
```

### Hashtag → Topic Mapping

```csharp
public async Task<Guid?> MapHashtagToTopicAsync(string hashtag)
{
    // 1. Check if hashtag exactly matches a topic
    var exactMatch = await _topicRepo.GetBySlugAsync(hashtag);
    if (exactMatch != null) return exactMatch.Id;
    
    // 2. Check if topic name contains hashtag
    var partialMatch = await _topicRepo.SearchAsync(hashtag);
    if (partialMatch != null) return partialMatch.Id;
    
    // 3. No match — hashtag remains standalone
    return null;
}
```

---

## 3. Trend Entity

Create a `Trend` entity:

```csharp
public class Trend
{
    public Guid Id { get; set; }
    public TrendType Type { get; set; }                // See enum below
    public Guid? TopicId { get; set; }                 // Associated topic
    public Guid? HashtagId { get; set; }               // Associated hashtag
    public string Query { get; set; }                   // Search query for custom trends
    public string DisplayName { get; set; }             // "Gaming", "#Gaming", "AI News"
    public string Slug { get; set; }                   // URL-safe identifier
    
    // Trend Metrics
    public TrendStrength Strength { get; set; }          // Calculated strength
    public int PostCount { get; set; }                  // Posts in trend window
    public int UniquePosters { get; set; }              // Unique accounts posting
    public int EngagementTotal { get; set; }             // Total engagement
    public float Velocity { get; set; }                  // Growth rate
    
    // Position
    public int Rank { get; set; }                       // Position in trend list
    public TrendScope Scope { get; set; }               // See enum below
    
    // Community context
    public Guid? CommunityId { get; set; }              // If community-specific trend
    
    // Timestamps
    public DateTime CalculatedAt { get; set; }          // When trend was calculated
    public DateTime? PeakedAt { get; set; }              // When trend reached max
    public DateTime ExpiresAt { get; set; }             // When trend expires
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TrendType
{
    Topic,           // Based on topic
    Hashtag,         // Based on hashtag
    Event,           // Based on event
    Search,          // Based on search queries
    Viral            // Based on viral content
}

public enum TrendStrength
{
    Emerging = 1,    // Just starting
    Growing = 2,     // Gaining momentum
    Hot = 3,         // High activity
    Viral = 4,       // Very high activity
    Peaking = 5      // Near peak (about to decline)
}

public enum TrendScope
{
    Global,          // Entire network
    Community,       // Community-specific
    Personal         // Personalized to user
}
```

---

## 4. Trend Calculation Service

### ITrendService

```csharp
public interface ITrendService
{
    Task<List<Trend>> GetGlobalTrendsAsync(int count = 10);
    Task<List<Trend>> GetCommunityTrendsAsync(Guid communityId, int count = 10);
    Task<List<Trend>> GetPersonalTrendsAsync(Guid accountId, int count = 10);
    Task<Trend> CalculateTrendAsync(string query, TrendScope scope);
    Task ProcessTrendsTickAsync();
    Task<List<Hashtag>> GetTrendingHashtagsAsync(int count = 20);
}
```

### Trend Calculation

```csharp
public async Task<Trend> CalculateTrendAsync(string query, TrendScope scope)
{
    var windowHours = 24;
    var windowStart = DateTime.UtcNow.AddHours(-windowHours);
    
    // Count posts mentioning this query/topic/hashtag
    var posts = await _postService.GetPostsMentioningAsync(query, windowStart);
    
    // Calculate metrics
    var postCount = posts.Count;
    var uniquePosters = posts.Select(p => p.AuthorId).Distinct().Count();
    var engagementTotal = posts.Sum(p => p.Likes + p.Comments + p.Reposts);
    
    // Calculate velocity (growth over time)
    var velocity = CalculateVelocity(posts, windowHours);
    
    // Calculate strength
    var strength = CalculateStrength(postCount, uniquePosters, velocity);
    
    // Determine if trending
    var isTrending = strength >= TrendStrength.Growing;
    
    return new Trend
    {
        Query = query,
        PostCount = postCount,
        UniquePosters = uniquePosters,
        EngagementTotal = engagementTotal,
        Velocity = velocity,
        Strength = strength,
        IsTrending = isTrending,
        Scope = scope,
        CalculatedAt = DateTime.UtcNow
    };
}
```

### Trend Strength Formula

```csharp
public TrendStrength CalculateStrength(int postCount, int uniquePosters, float velocity)
{
    // Base score from post count
    var countScore = postCount switch
    {
        < 10 => 0,
        < 50 => 1,
        < 200 => 2,
        < 500 => 3,
        < 1000 => 4,
        _ => 5
    };
    
    // Adjust for unique posters (more posters = stronger)
    var posterScore = uniquePosters switch
    {
        < 5 => 0,
        < 20 => 1,
        < 50 => 2,
        < 100 => 3,
        < 200 => 4,
        _ => 5
    };
    
    // Adjust for velocity (rapid growth = stronger)
    var velocityScore = velocity switch
    {
        < 0.5f => 0,
        < 1.0f => 1,
        < 2.0f => 2,
        < 5.0f => 3,
        < 10.0f => 4,
        _ => 5
    };
    
    // Weighted average
    var totalScore = (countScore * 0.4) + (posterScore * 0.3) + (velocityScore * 0.3);
    
    return totalScore switch
    {
        < 1 => TrendStrength.Emerging,
        < 2 => TrendStrength.Growing,
        < 3 => TrendStrength.Hot,
        < 4 => TrendStrength.Viral,
        _ => TrendStrength.Peaking
    };
}
```

### Velocity Calculation

```csharp
public float CalculateVelocity(List<Post> posts, int windowHours)
{
    if (!posts.Any()) return 0;
    
    // Group posts by hour
    var hourlyCounts = posts
        .GroupBy(p => p.CreatedAt.Hour)
        .OrderBy(g => g.Key)
        .Select(g => g.Count())
        .ToList();
    
    if (hourlyCounts.Count < 2) return 0;
    
    // Calculate trend line slope
    var n = hourlyCounts.Count;
    var xMean = (n - 1) / 2.0;
    var yMean = hourlyCounts.Average();
    
    var numerator = 0.0;
    var denominator = 0.0;
    
    for (int i = 0; i < n; i++)
    {
        numerator += (i - xMean) * (hourlyCounts[i] - yMean);
        denominator += Math.Pow(i - xMean, 2);
    }
    
    var slope = denominator != 0 ? numerator / denominator : 0;
    
    // Velocity = slope normalized to posts/hour
    return (float)(slope > 0 ? slope : 0);
}
```

---

## 5. Trend Types

### Global Trends

Trends that span the entire network:

```csharp
public async Task<List<Trend>> GetGlobalTrendsAsync(int count)
{
    // Get all topics and calculate their trends
    var topics = await _topicService.GetActiveTopicsAsync();
    var trends = new List<Trend>();
    
    foreach (var topic in topics)
    {
        var trend = await CalculateTrendAsync(topic.Name, TrendScope.Global);
        trend.TopicId = topic.Id;
        trends.Add(trend);
    }
    
    // Also check popular hashtags
    var hashtags = await _hashtagService.GetActiveHashtagsAsync();
    foreach (var hashtag in hashtags)
    {
        var trend = await CalculateTrendAsync(hashtag.Tag, TrendScope.Global);
        trend.HashtagId = hashtag.Id;
        trends.Add(trend);
    }
    
    // Sort by strength and rank
    return trends
        .Where(t => t.IsTrending)
        .OrderByDescending(t => t.Strength)
        .ThenByDescending(t => t.EngagementTotal)
        .Take(count)
        .ToList();
}
```

### Community Trends

Trends specific to a community:

```csharp
public async Task<List<Trend>> GetCommunityTrendsAsync(Guid communityId, int count)
{
    var community = await _communityService.GetAsync(communityId);
    
    // Get posts within this community
    var posts = await _postService.GetCommunityPostsAsync(communityId, since: DateTime.UtcNow.AddHours(-24));
    
    // Extract topics from posts
    var topicCounts = posts
        .SelectMany(p => p.Topics)
        .GroupBy(t => t)
        .Select(g => new { Topic = g.Key, Count = g.Count() })
        .OrderByDescending(x => x.Count)
        .Take(count)
        .ToList();
    
    // Build community trends
    return topicCounts.Select(tc => new Trend
    {
        Type = TrendType.Topic,
        TopicId = tc.Topic.Id,
        DisplayName = tc.Topic.DisplayName,
        Scope = TrendScope.Community,
        CommunityId = communityId,
        PostCount = tc.Count,
        Strength = CalculateCommunityStrength(tc.Count, community.MemberCount)
    }).ToList();
}
```

### Personal Trends

Trends personalized to a user's interests:

```csharp
public async Task<List<Trend>> GetPersonalTrendsAsync(Guid accountId, int count)
{
    var account = await _accountService.GetAsync(accountId);
    
    // Get user's interests/topics
    var userTopics = account.Interests; // List of topic names
    
    // Get global trends
    var globalTrends = await GetGlobalTrendsAsync(50);
    
    // Filter and boost by user interests
    var personalTrends = globalTrends
        .Select(t => new
        {
            Trend = t,
            InterestBoost = userTopics.Contains(t.Query) ? 1.5 : 1.0
        })
        .Select(x => 
        {
            x.Trend.Strength = (TrendStrength)((int)x.Trend.Strength * (int)x.InterestBoost);
            return x.Trend;
        })
        .OrderByDescending(t => t.Strength)
        .Take(count)
        .ToList();
    
    return personalTrends;
}
```

---

## 6. Trend Propagation

How trends spread between communities:

### Cross-Community Propagation

```csharp
public async Task ProcessCrossCommunityPropagationAsync()
{
    // Find communities with overlapping member bases
    var communities = await _communityService.GetAllActiveAsync();
    
    foreach (var community in communities)
    {
        // Get this community's trends
        var localTrends = await GetCommunityTrendsAsync(community.Id, 5);
        
        // Find connected communities (shared members)
        var connectedCommunities = await _communityService.GetConnectedAsync(community.Id);
        
        foreach (var localTrend in localTrends)
        {
            foreach (var connected in connectedCommunities)
            {
                // Calculate propagation probability
                var propagationProb = CalculatePropagationProbability(
                    localTrend, 
                    community, 
                    connected);
                
                if (Random.NextDouble() < propagationProb)
                {
                    // Trend propagates to connected community
                    await _trendPropagationRepo.RecordPropagationAsync(
                        localTrend.Id, 
                        connected.Id);
                }
            }
        }
    }
}

public double CalculatePropagationProbability(Trend trend, Community from, Community to)
{
    // Base probability
    var baseProb = 0.1;
    
    // More engagement = higher propagation
    var engagementBoost = Math.Min(0.3, trend.EngagementTotal / 10000.0);
    
    // Stronger trends propagate more
    var strengthBoost = ((int)trend.Strength - 1) * 0.05;
    
    // Topic overlap boosts propagation
    var topicOverlap = from.Topics
        .Intersect(to.Topics)
        .Count() / Math.Max(1, Math.Min(from.Topics.Count, to.Topics.Count));
    var topicBoost = topicOverlap * 0.2;
    
    // Shared members boost propagation
    var sharedMembers = await _communityService.GetSharedMemberCountAsync(from.Id, to.Id);
    var memberBoost = Math.Min(0.3, sharedMembers / 100.0);
    
    return Math.Min(0.9, baseProb + engagementBoost + strengthBoost + topicBoost + memberBoost);
}
```

---

## 7. Hashtag Autocreation

Auto-create hashtag entries when new hashtags appear:

```csharp
public async Task ProcessNewHashtagsAsync()
{
    // Find recent posts with hashtags
    var recentHashtags = await _postService.GetRecentHashtagsAsync(since: DateTime.UtcNow.AddHours(-1));
    
    foreach (var hashtag in recentHashtags)
    {
        // Check if hashtag already exists
        var existing = await _hashtagService.GetByTagAsync(hashtag);
        if (existing != null) continue;
        
        // Create new hashtag entry
        var newHashtag = new Hashtag
        {
            Tag = hashtag,
            NormalizedTag = hashtag.ToLower().TrimStart('#'),
            TopicId = await MapHashtagToTopicAsync(hashtag),
            UsageCount = 1,
            TodayUsageCount = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        await _hashtagService.CreateAsync(newHashtag);
    }
}
```

---

## 8. Trend Processing Tick

Run trend calculations periodically:

```csharp
public class TrendProcessingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessTrendsTickAsync();
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); // Every 15 minutes
        }
    }
}

public async Task ProcessTrendsTickAsync()
{
    // 1. Update hashtag usage counts
    await _hashtagService.ResetDailyCountsAsync();
    
    // 2. Process new hashtags
    await ProcessNewHashtagsAsync();
    
    // 3. Calculate global trends
    var globalTrends = await CalculateAllGlobalTrendsAsync();
    
    // 4. Update trend rankings
    await UpdateTrendRankingsAsync(globalTrends);
    
    // 5. Process cross-community propagation
    await ProcessCrossCommunityPropagationAsync();
    
    // 6. Expire old trends
    await ExpireOldTrendsAsync();
}
```

---

## 9. LLM-Assisted Trend Analysis

Use local Ollama for trend insights:

### Trend Narrative Generation

```text
SYSTEM: Generate a brief description of this trending topic.

TREND: {trend name}
POST COUNT: {number} posts
ENGAGEMENT: {number}
VELOCITY: {growing/declining}
RELATED EVENTS: {list of events}

Generate a 1-2 sentence description of why this is trending.

Example:
"#AI continues to dominate discussions as a major tech company announced a breakthrough 
in language models. The announcement sparked debates about the future of work."
```

### Hashtag Grouping

```text
SYSTEM: Group these related hashtags into unified topics.

HASHTAGS: #AI, #ArtificialIntelligence, #MachineLearning, #DeepLearning, #Tech

Identify groups of related hashtags and suggest unified topic names.

Output format:
{
  "groups": [
    {
      "unifiedTopic": "Artificial Intelligence",
      "hashtags": ["#AI", "#ArtificialIntelligence", "#MachineLearning"],
      "primaryHashtag": "#AI"
    }
  ]
}
```

---

## 10. NPC Trend Awareness

NPCs should be aware of and react to trends:

```csharp
// NPCs are more likely to post about trending topics
public async Task<List<string>> GetNpcTopicSuggestionsAsync(Guid npcId)
{
    var personality = await _npcService.GetPersonalityAsync(npcId);
    var globalTrends = await _trendService.GetGlobalTrendsAsync(10);
    
    // Filter trends by NPC interests
    var relevantTrends = globalTrends
        .Where(t => personality.Interests.Contains(t.Query))
        .OrderByDescending(t => t.Strength)
        .Take(5)
        .Select(t => t.Query)
        .ToList();
    
    return relevantTrends;
}
```

---

## 11. Trend API Endpoints

### Global Trends
```http
GET /api/trends?count={count}
```
Returns global trending topics.

### Community Trends
```http
GET /api/communities/{id}/trends?count={count}
```
Returns trending topics for a community.

### Personal Trends
```http
GET /api/me/trends?count={count}
```
Returns personalized trending topics.

### Trending Hashtags
```http
GET /api/hashtags/trending?count={count}
```
Returns trending hashtags.

### Topic Details
```http
GET /api/topics/{slug}
```
Returns topic details with recent posts.

### Topic Posts
```http
GET /api/topics/{slug}/posts?cursor={cursor}&pageSize={size}
```
Returns posts for a topic.

### Search by Topic
```http
GET /api/topics/search?q={query}&cursor={cursor}
```
Search topics.

---

## 12. Database Migration

### Topics Table
```sql
CREATE TABLE Topics (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    DisplayName TEXT NOT NULL,
    Slug TEXT NOT NULL UNIQUE,
    Description TEXT,
    Category INTEGER NOT NULL,
    PostCount INTEGER NOT NULL DEFAULT 0,
    ActivePostCount INTEGER NOT NULL DEFAULT 0,
    SubscriberCount INTEGER NOT NULL DEFAULT 0,
    IsVerified INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE INDEX IX_Topics_Slug ON Topics(Slug);
CREATE INDEX IX_Topics_Category ON Topics(Category);
CREATE INDEX IX_Topics_IsActive ON Topics(IsActive);
```

### Hashtags Table
```sql
CREATE TABLE Hashtags (
    Id TEXT PRIMARY KEY,
    Tag TEXT NOT NULL UNIQUE,
    NormalizedTag TEXT NOT NULL UNIQUE,
    TopicId TEXT,
    UsageCount INTEGER NOT NULL DEFAULT 0,
    TodayUsageCount INTEGER NOT NULL DEFAULT 0,
    IsTrending INTEGER NOT NULL DEFAULT 0,
    TrendingSince TEXT,
    TrendingRank INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (TopicId) REFERENCES Topics(Id)
);

CREATE INDEX IX_Hashtags_NormalizedTag ON Hashtags(NormalizedTag);
CREATE INDEX IX_Hashtags_IsTrending ON Hashtags(IsTrending);
CREATE INDEX IX_Hashtags_TodayUsage ON Hashtags(TodayUsageCount DESC);
```

### Trends Table
```sql
CREATE TABLE Trends (
    Id TEXT PRIMARY KEY,
    Type INTEGER NOT NULL,
    TopicId TEXT,
    HashtagId TEXT,
    Query TEXT NOT NULL,
    DisplayName TEXT NOT NULL,
    Slug TEXT NOT NULL,
    Strength INTEGER NOT NULL,
    PostCount INTEGER NOT NULL DEFAULT 0,
    UniquePosters INTEGER NOT NULL DEFAULT 0,
    EngagementTotal INTEGER NOT NULL DEFAULT 0,
    Velocity REAL NOT NULL DEFAULT 0,
    Rank INTEGER,
    Scope INTEGER NOT NULL,
    CommunityId TEXT,
    CalculatedAt TEXT NOT NULL,
    PeakedAt TEXT,
    ExpiresAt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (TopicId) REFERENCES Topics(Id),
    FOREIGN KEY (HashtagId) REFERENCES Hashtags(Id),
    FOREIGN KEY (CommunityId) REFERENCES Communities(Id)
);

CREATE INDEX IX_Trends_Scope ON Trends(Scope);
CREATE INDEX IX_Trends_Strength ON Trends(Strength DESC);
CREATE INDEX IX_Trends_Rank ON Trends(Rank);
CREATE INDEX IX_Trends_ExpiresAt ON Trends(ExpiresAt);
CREATE INDEX IX_Trends_IsActive ON Trends(IsActive);
```

### Trend Propagation Table
```sql
CREATE TABLE TrendPropagation (
    Id TEXT PRIMARY KEY,
    TrendId TEXT NOT NULL,
    FromCommunityId TEXT NOT NULL,
    ToCommunityId TEXT NOT NULL,
    PropagatedAt TEXT NOT NULL,
    FOREIGN KEY (TrendId) REFERENCES Trends(Id),
    FOREIGN KEY (FromCommunityId) REFERENCES Communities(Id),
    FOREIGN KEY (ToCommunityId) REFERENCES Communities(Id)
);

CREATE INDEX IX_TrendPropagation_TrendId ON TrendPropagation(TrendId);
```

---

## 13. Tests

### Topic Tests
```text
Topics can be created and retrieved
Topic slugs are unique
Topic categories work correctly
Hashtag → Topic mapping works
```

### Hashtag Tests
```text
Hashtags extracted from content correctly
New hashtags auto-created
Hashtag usage counts updated
Trending hashtags identified
```

### Trend Tests
```text
Global trends calculated correctly
Community trends calculated correctly
Personal trends filtered by interests
Trend strength calculated correctly
Velocity calculated correctly
```

### Propagation Tests
```text
Cross-community propagation occurs
Propagation probability calculated
Connected communities identified
```

### API Tests
```text
Trend endpoints return correct data
Pagination works
Scope filtering works
```

### Regression Tests
```text
Existing Parts 01-19 tests still pass
```

---

## 14. Android

Part 20 is backend-only. Minimal model adjustments only.

---

## 15. README — REQUIRED

Document:
- Part 20 completion
- Topic entity and categories
- Hashtag management
- Trend entity and types
- Trend calculation logic
- Global, Community, Personal trends
- Trend propagation
- LLM trend analysis
- NPC trend awareness
- API endpoints
- Database changes
- Tests performed
- Current status
- Next planned part

---

## 16. Git

After implementation:
1. Inspect `git status`
2. Commit: `Implement topics and trends system (Part 20)`
3. Push to `origin/main`
4. Verify against origin

---

## 17. DO NOT IMPLEMENT YET

- Rumor mechanics (Part 21)
- News coverage (Part 22)
- Trend predictions
- Trend analytics dashboard
- Trend-based recommendations

---

## 18. QUALITY REQUIREMENTS

- Correct (trend calculations accurate)
- Performant (batch processing)
- Configurable (thresholds in settings)
- Testable
- Permanent (all records persist)

---

## 19. FINAL VERIFICATION

```text
Server builds
Topics created and seeded
Hashtags extracted and tracked
Global trends calculated
Community trends calculated
Personal trends filtered
Trend propagation works
Hashtag autocreation works
Trend API returns data
Database migrations applied
Existing tests pass
README updated
Git commit pushed
Working tree clean
```

---

## 20. FINAL SESSION REPORT

```text
# PART 20 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Topics Architecture
...

## 5. Hashtag Management
...

## 6. Trends Architecture
...

## 7. Trend Calculation
...

## 8. Trend Types & Scope
...

## 9. Cross-Community Propagation
...

## 10. LLM Trend Analysis
...

## 11. NPC Trend Awareness
...

## 12. API Endpoints
...

## 13. Database Changes
...

## 14. Tests
...

## 15. README
Updated: YES
...

## 16. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 17. Current Project Status
01A-20 COMPLETE

## 18. Intentionally Not Implemented
- Rumors (Part 21)
- News coverage (Part 22)
- Trend predictions
- Analytics dashboard

## 19. NEXT
NEXT: PART 21 — Rumors
```

**STOP after completing Part 20 and reporting the session log.**
