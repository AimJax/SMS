# SOCIAL MEDIA SIMULATOR — PART 19 DEVELOPMENT PROMPT
## VIRALITY

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
01E  Android Client Foundation       COMPLETE
01F  Foundation Checkpoint           COMPLETE
02   Backend Architecture            COMPLETE
03   Persistence                   COMPLETE
04   Accounts & Authentication       COMPLETE
05   Social Graph                    COMPLETE
06   Posts & Engagement             COMPLETE
07   Feed & Timeline                COMPLETE
08   NPC Simulator Foundation        COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation         COMPLETE
11   NPC Background Simulation      COMPLETE
12   NPC Social Graph               COMPLETE
13   AI Content Generation          COMPLETE
14   Notifications System           COMPLETE
15   Communities                   COMPLETE
16   Advanced Feed                 COMPLETE
17   LLM-Driven Event System       COMPLETE
18   Event Causality & Offline Sim  COMPLETE
```

Latest commit:

```text
ef6e247 — Part 18: Event Causality & Offline World Simulation
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 18.

---

# 1. WHY THIS PART, NOW

Parts 01–18 built the core social media infrastructure, LLM-driven events, causality tracking, and offline simulation. The world now has emergent storytelling and persistent history — but content has no **virality** mechanics.

Without virality, posts are either popular or not, with no organic spread, no viral thresholds, and no real consequences for going viral. A post with 100 likes and a post with 100,000 likes are treated identically.

Part 19 transforms the platform by making posts capable of **organic viral spread**. A post doesn't just "get popular" — it spreads through networks, crosses thresholds, triggers events, causes follower changes, creates fame consequences, and makes the poster briefly feel like they're at the center of a social explosion.

Virality is foundational to:
- Trending Topics (Part 20) — viral posts create trends
- Rumors (Part 21) — virality spreads rumors
- News (Part 22) — news accounts cover viral content
- Fame & Reputation — virality changes account influence
- Social Drama (Part 27) — virality amplifies drama

---

# 2. THE EXISTING PROJECT

The existing backend contains from Parts 01–18:

- Everything from Part 18 and earlier
- **Event System (Part 17):** Events are created and tracked
- **Causality (Part 18):** Event chains are recorded
- **Offline Simulation (Part 18):** World runs when player is away
- **Advanced Feed (Part 16):** Posts are scored and ranked
- Posts, Likes, Comments, Reposts with engagement counters
- Account Fame and Influence metrics (Part 13)
- NPC Behavior with personality-driven actions

The infrastructure exists:
- Engagement data (likes, comments, reposts, views)
- Social graph (followers, following)
- Event detection
- Feed scoring

Part 19 adds the virality detection, spread mechanics, and consequences.

---

# 3. MASTER ARCHITECTURE PRINCIPLES

## Server Authoritative

Virality is calculated by the server. The server determines when a post is viral, how it spreads, and what consequences occur. The client cannot fake viral status.

## C# Simulation + LLM Enhancement

- C# calculates virality metrics deterministically
- LLM may assist with content analysis (controversy detection, sentiment)
- Consequences are applied by C# services

## Permanent Data Rule

All virality records, viral thresholds crossed, and spread data must NOT be automatically deleted/pruned.

## Performance

Virality calculations must be efficient. Do not recalculate for every post on every tick. Use caching, batch processing, and event-driven updates.

---

# PART 19 OBJECTIVE

Implement a complete **Virality System**:

1. **Virality State** — Track each post's current virality state
2. **Virality Metrics** — Calculate engagement velocity, reach, acceleration
3. **Viral Thresholds** — Define and detect threshold crossings
4. **Spread Mechanics** — Model how content spreads through the network
5. **Exposure Calculation** — Determine who sees a post
6. **Viral Consequences** — Apply effects when posts go viral
7. **Viral Events** — Create events for viral moments
8. **Virality API** — Endpoints for viral content

Do NOT implement in this part:
- Full trending system (Part 20)
- Rumor mechanics (Part 21)
- News coverage (Part 22)
- Virality predictions
- Anti-viral/super-viral mechanics beyond defined states

---

# PART 19 — REQUIRED FEATURES

## 1. Virality States

Define virality states as an enum:

```csharp
public enum ViralityState
{
    Normal = 0,       // Standard engagement
    Trending = 1,     // Gaining traction
    Popular = 2,      // Above average
    Viral = 3,        // Crossed viral threshold
    MassivelyViral = 4, // Extremely viral
    Declining = 5    // Was viral, now cooling
}
```

### State Thresholds

Configure thresholds (in settings):

```json
{
  "Virality": {
    "TrendingThreshold": 50,        // 50 total engagement
    "PopularThreshold": 200,        // 200 total engagement
    "ViralThreshold": 1000,         // 1000 total engagement
    "MassivelyViralThreshold": 10000, // 10000 total engagement
    "ViralVelocityMin": 10,         // Min engagement velocity to be viral
    "ViralWindowHours": 24          // Time window for velocity calculation
  }
}
```

---

## 2. Post Virality Entity

Extend the `Post` entity with virality tracking:

```text
ViralityState              (enum — current state)
ViralityScore              (float — calculated score 0-100)
TotalEngagement            (int — likes + comments + reposts)
EngagementVelocity         (float — engagement per hour)
PeakEngagementVelocity     (float — highest velocity reached)
Reach                      (int — estimated unique viewers)
ShareCount                 (int — times shared/reposted
ViralAt                    (nullable timestamp — when it crossed viral threshold)
MassivelyViralAt           (nullable timestamp)
DeclinedAt                 (nullable timestamp — when it started declining)
FirstViralThresholdCrossed  (enum — which threshold was crossed first)
```

Or create a separate `PostVirality` entity:

```csharp
public class PostVirality
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public ViralityState State { get; set; }
    public float Score { get; set; }
    public int TotalEngagement { get; set; }
    public float Velocity { get; set; }
    public float PeakVelocity { get; set; }
    public int Reach { get; set; }
    public DateTime? ViralAt { get; set; }
    public DateTime? MassivelyViralAt { get; set; }
    public DateTime? DeclinedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

---

## 3. Virality Calculation Service

### IViralityService

```csharp
public interface IViralityService
{
    Task<PostVirality> CalculateViralityAsync(Guid postId);
    Task<ViralityState> GetViralityStateAsync(Guid postId);
    Task<List<Post>> GetViralPostsAsync(int count, ViralityState minState);
    Task TrackEngagementAsync(Guid postId, EngagementType type);
    Task ProcessViralityTickAsync();
}
```

### CalculateViralityAsync

```csharp
public async Task<PostVirality> CalculateViralityAsync(Guid postId)
{
    var post = await _postService.GetAsync(postId);
    var engagement = await _engagementService.GetEngagementAsync(postId);
    
    var totalEngagement = engagement.Likes + engagement.Comments + engagement.Reposts;
    var velocity = await CalculateVelocityAsync(postId);
    var reach = await EstimateReachAsync(postId);
    
    var score = CalculateViralityScore(totalEngagement, velocity, reach, post.Author.FollowerCount);
    
    var state = DetermineState(score, velocity);
    
    return new PostVirality
    {
        PostId = postId,
        TotalEngagement = totalEngagement,
        Velocity = velocity,
        Reach = reach,
        Score = score,
        State = state,
        LastUpdated = DateTime.UtcNow
    };
}
```

### Virality Score Formula

```csharp
public float CalculateViralityScore(int totalEngagement, float velocity, int reach, int authorFollowers)
{
    // Engagement score (0-30)
    var engagementScore = Math.Min(30, (float)Math.Log10(totalEngagement + 1) * 10);
    
    // Velocity score (0-30) — recent growth matters more
    var velocityScore = Math.Min(30, velocity * 3);
    
    // Reach score (0-20) — how many people saw it
    var reachScore = Math.Min(20, (float)Math.Log10(reach + 1) * 6);
    
    // Relative engagement (0-20) — engagement relative to author's reach
    var relativeEngagement = authorFollowers > 0 
        ? (float)totalEngagement / authorFollowers 
        : 0;
    var relativeScore = Math.Min(20, relativeEngagement * 100);
    
    return engagementScore + velocityScore + reachScore + relativeScore;
}
```

### Engagement Velocity Calculation

```csharp
public async Task<float> CalculateVelocityAsync(Guid postId)
{
    var windowHours = _config.ViralWindowHours;
    var windowStart = DateTime.UtcNow.AddHours(-windowHours);
    
    // Get engagement in the time window
    var recentEngagement = await _engagementService.GetEngagementInWindowAsync(postId, windowStart);
    
    // Calculate posts per hour
    var velocity = (float)recentEngagement.Total / windowHours;
    
    return velocity;
}
```

### Reach Estimation

```csharp
public async Task<int> EstimateReachAsync(Guid postId)
{
    var post = await _postService.GetAsync(postId);
    var authorFollowers = await _socialGraphService.GetFollowerCountAsync(post.AuthorId);
    
    // Base reach = author's follower count
    var baseReach = authorFollowers;
    
    // Engagement multiplier (posts with engagement get shared, increasing reach)
    var engagement = await _engagementService.GetEngagementAsync(postId);
    var engagementMultiplier = 1 + (engagement.Total / 100.0);
    
    // Viral multiplier (if already crossing thresholds)
    var currentVelocity = await CalculateVelocityAsync(postId);
    var viralMultiplier = currentVelocity > _config.ViralVelocityMin ? 2.0 : 1.0;
    
    return (int)(baseReach * engagementMultiplier * viralMultiplier);
}
```

---

## 4. Viral Threshold Detection

Detect when posts cross viral thresholds:

```csharp
public async Task CheckThresholdsAsync(Guid postId)
{
    var virality = await CalculateViralityAsync(postId);
    var previousState = await _viralityRepo.GetStateAsync(postId);
    
    if (virality.State != previousState)
    {
        // State changed — handle transition
        await HandleStateTransitionAsync(postId, previousState, virality.State);
    }
}
```

### State Transition Handling

```csharp
public async Task HandleStateTransitionAsync(Guid postId, ViralityState from, ViralityState to)
{
    // Log the transition
    await _viralityRepo.SaveTransitionAsync(postId, from, to);
    
    // Trigger consequences based on state
    switch (to)
    {
        case ViralityState.Viral:
            await OnPostBecomesViralAsync(postId);
            break;
        case ViralityState.MassivelyViral:
            await OnPostBecomesMassivelyViralAsync(postId);
            break;
        case ViralityState.Declining:
            await OnPostDeclinesAsync(postId);
            break;
    }
}
```

---

## 5. Viral Consequences

When posts go viral, apply consequences:

### OnPostBecomesViralAsync

```csharp
public async Task OnPostBecomesViralAsync(Guid postId)
{
    var post = await _postService.GetAsync(postId);
    var author = post.Author;
    
    // 1. Create ViralityEvent
    await _eventService.CreateEventAsync(new Event
    {
        Type = "Content.ViralPost",
        Title = $"Post by @{author.Username} went viral",
        Description = $"A post by @{author.Username} crossed the viral threshold with {post.Virality.TotalEngagement} engagements",
        RelatedPostId = postId,
        RelatedAccountId = author.Id,
        Topic = post.Topic
    });
    
    // 2. Apply follower consequences
    var followerGain = CalculateFollowerGain(post);
    await _accountService.AdjustFollowersAsync(author.Id, followerGain);
    
    // 3. Apply fame consequences
    var fameGain = CalculateFameGain(post);
    await _accountService.AdjustFameAsync(author.Id, fameGain);
    
    // 4. Create notifications
    await _notificationService.NotifyFollowerMilestoneAsync(author.Id);
    
    // 5. LLM generates reaction content (optional)
    // NPCs and news accounts may react
    
    // 6. Update post virality record
    post.Virality.ViralAt = DateTime.UtcNow;
    post.Virality.FirstViralThresholdCrossed = ViralityState.Viral;
    await _postService.UpdateAsync(post);
}
```

### Follower Gain Calculation

```csharp
public int CalculateFollowerGain(Post post)
{
    var baseGain = 10;
    
    // More engagement = more follower gain
    var engagementBonus = post.Virality.TotalEngagement / 50;
    
    // Celebrities gain fewer followers (already popular)
    var famePenalty = post.Author.Fame / 100.0 * 0.5;
    
    // Viral multiplier
    var viralMultiplier = post.Virality.State == ViralityState.MassivelyViral ? 5.0 : 1.0;
    
    return (int)((baseGain + engagementBonus) * (1 - famePenalty) * viralMultiplier);
}
```

### Fame Gain Calculation

```csharp
public float CalculateFameGain(Post post)
{
    var baseGain = 5.0f;
    
    // More engagement = more fame
    var engagementBonus = post.Virality.TotalEngagement / 200.0f;
    
    // Viral multiplier
    var viralMultiplier = post.Virality.State == ViralityState.MassivelyViral ? 3.0f : 1.0f;
    
    return (baseGain + engagementBonus) * viralMultiplier;
}
```

---

## 6. Content Spread Mechanics

Model how content spreads through the network:

### Share/Repost Cascade

```csharp
public async Task ProcessShareCascadeAsync(Guid postId)
{
    var post = await _postService.GetAsync(postId);
    var virality = post.Virality;
    
    // Calculate share probability based on virality
    var shareProbability = CalculateShareProbability(virality);
    
    // Get people who saw the post but didn't engage
    var exposedAccounts = await _exposureTracker.GetExposedAccountsAsync(postId);
    var engagedAccounts = await _engagementService.GetEngagedAccountsAsync(postId);
    
    var unengagedAccounts = exposedAccounts.Except(engagedAccounts);
    
    foreach (var account in unengagedAccounts)
    {
        if (ShouldAccountShare(account, post, shareProbability))
        {
            // Account shares/reposts
            await _postService.CreateRepostAsync(account.Id, postId);
            
            // This increases reach for next iteration
            virality.Reach += await _socialGraphService.GetFollowerCountAsync(account.Id);
        }
    }
}

public bool ShouldAccountShare(Account account, Post post, float baseProbability)
{
    // Personality factors
    var personality = account.Personality;
    
    // Extroverts share more
    var extroversionBoost = personality.Extroversion / 200.0;
    
    // People with high Drama tendency share drama
    var dramaBoost = personality.DramaTendency / 500.0;
    
    // Check if account is in a community that would share
    var communityBoost = account.CommunityMemberships.Any(c => c.Topic == post.Topic) ? 0.1 : 0;
    
    // Final probability
    var finalProbability = baseProbability + extroversionBoost + dramaBoost + communityBoost;
    
    return Random.NextDouble() < finalProbability;
}
```

### Network Effect Calculation

```csharp
public async Task<int> EstimateNetworkSpreadAsync(Guid postId)
{
    var post = await _postService.GetAsync(postId);
    var author = post.Author;
    
    // Start with author's direct followers
    var directReach = author.FollowerCount;
    
    // Add followers of people who liked
    var likers = await _engagementService.GetLikersAsync(postId);
    var likersReach = await _socialGraphService.GetTotalFollowerCountAsync(likers.Select(l => l.AccountId));
    
    // Add followers of people who commented
    var commenters = await _engagementService.GetCommentersAsync(postId);
    var commentersReach = await _socialGraphService.GetTotalFollowerCountAsync(commenters.Select(c => c.AccountId));
    
    // Add followers of people who shared
    var sharers = await _engagementService.GetRepostersAsync(postId);
    var sharersReach = await _socialGraphService.GetTotalFollowerCountAsync(sharers.Select(s => s.AccountId));
    
    // Estimate overlap (not everyone sees every share)
    var overlapFactor = 0.7;
    
    return (int)((directReach + likersReach + commentersReach + sharersReach) * overlapFactor);
}
```

---

## 7. Virality Processing Tick

Run virality calculations periodically:

```csharp
public class ViralityProcessingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessViralityTickAsync();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Every 5 minutes
        }
    }
}

public async Task ProcessViralityTickAsync()
{
    // 1. Get posts that need virality updates
    // - Posts created in last 7 days
    // - Posts that are not in "Declined" state
    var candidatePosts = await _postService.GetRecentActivePostsAsync();
    
    // 2. Process in batches
    foreach (var batch in candidatePosts.Chunk(100))
    {
        foreach (var postId in batch)
        {
            await CheckThresholdsAsync(postId);
            await ProcessShareCascadeAsync(postId);
        }
    }
    
    // 3. Mark declining posts
    await MarkDecliningPostsAsync();
}
```

---

## 8. LLM-Assisted Virality Analysis

Use local Ollama for advanced virality insights:

### Controversy Detection

```text
SYSTEM: Analyze this social media post for controversy potential.

POST: {post content}
AUTHOR: @{username} ({follower_count} followers)
ENGAGEMENT: {likes} likes, {comments} comments, {reposts} reposts

Is this post likely to be controversial? Why or why not?
Consider: political content, hot takes, personal attacks, sensitive topics.

Respond with:
{
  "controversyLevel": 0-10,
  "reasons": ["reason 1", "reason 2"],
  "likelyToSpread": true/false,
  "reasoning": "..."
}
```

### Viral Prediction (for NPC behavior)

```text
SYSTEM: Predict if this post will go viral based on content and context.

POST: {post content}
TOPIC: {topic tags}
AUTHOR: @{username}
- Followers: {count}
- Fame: {level}
- Previous viral posts: {count}

Predict:
{
  "viralProbability": 0-100,
  "estimatedReach": number,
  "timeToViral": "hours" or "unlikely",
  "reasoning": "..."
}
```

---

## 9. NPC Virality Awareness

NPCs should react to viral posts:

### NPC Reaction to Virality

```csharp
// In NPC behavior processing
public async Task ProcessViralReactionsAsync(Guid postId)
{
    var post = await _postService.GetAsync(postId);
    var virality = post.Virality;
    
    if (virality.State >= ViralityState.Viral)
    {
        // Get NPCs who might react
        var potentialReactors = await _npcService.GetReactorsAsync(post);
        
        foreach (var npc in potentialReactors)
        {
            // Personality-based reaction probability
            if (ShouldReactToViral(npc, post))
            {
                // NPC reacts - like, comment, or share
                await _npcBehaviorService.NpcReactToViralPostAsync(npc, post);
            }
        }
    }
}
```

---

## 10. Virality API Endpoints

### Get Viral Posts
```http
GET /api/posts/viral?minState={state}&count={count}
```
Returns viral posts sorted by virality score.

### Get Post Virality Details
```http
GET /api/posts/{id}/virality
```
Returns detailed virality metrics for a post.

### Get Trending Posts
```http
GET /api/posts/trending?topic={topic}&count={count}
```
Returns trending posts (combines virality + recency).

### Get Viral Events
```http
GET /api/events?type=Content.ViralPost&status=Active
```
Returns ongoing viral events.

---

## 11. Database Migration

### Add to Post Virality Table (if separate entity)
```sql
CREATE TABLE PostVirality (
    Id TEXT PRIMARY KEY,
    PostId TEXT NOT NULL UNIQUE,
    State INTEGER NOT NULL,
    Score REAL NOT NULL,
    TotalEngagement INTEGER NOT NULL,
    Velocity REAL NOT NULL,
    PeakVelocity REAL NOT NULL,
    Reach INTEGER NOT NULL,
    ViralAt TEXT,
    MassivelyViralAt TEXT,
    DeclinedAt TEXT,
    FirstThresholdCrossed INTEGER,
    LastUpdated TEXT NOT NULL,
    FOREIGN KEY (PostId) REFERENCES Posts(Id)
);

CREATE INDEX IX_PostVirality_PostId ON PostVirality(PostId);
CREATE INDEX IX_PostVirality_State ON PostVirality(State);
CREATE INDEX IX_PostVirality_Score ON PostVirality(Score DESC);
CREATE INDEX IX_PostVirality_Velocity ON PostVirality(Velocity DESC);
```

### Virality Transition Log
```sql
CREATE TABLE ViralityTransition (
    Id TEXT PRIMARY KEY,
    PostId TEXT NOT NULL,
    FromState INTEGER NOT NULL,
    ToState INTEGER NOT NULL,
    ScoreAtTransition REAL NOT NULL,
    EngagementAtTransition INTEGER NOT NULL,
    TransitionedAt TEXT NOT NULL,
    FOREIGN KEY (PostId) REFERENCES Posts(Id)
);

CREATE INDEX IX_ViralityTransition_PostId ON ViralityTransition(PostId);
CREATE INDEX IX_ViralityTransition_TransitionedAt ON ViralityTransition(TransitionedAt DESC);
```

---

## 12. Tests

### Virality Calculation Tests
```text
Virality score calculates correctly for normal post
Virality score calculates correctly for viral post
Velocity calculates correctly
Reach estimates correctly
State transitions trigger correctly
```

### Threshold Detection Tests
```text
Post crosses Normal -> Trending threshold
Post crosses Trending -> Popular threshold
Post crosses Popular -> Viral threshold
Post crosses Viral -> MassivelyViral threshold
Post declines from Viral -> Declining
```

### Consequence Tests
```text
Viral post triggers follower gain
Viral post triggers fame gain
Viral post creates event
Viral post creates notifications
```

### Spread Tests
```text
Share cascade processes correctly
Network spread estimates correctly
NPC reactions trigger for viral posts
```

### Performance Tests
```text
Processing 100 posts completes in < 1 second
Virality tick completes in < 5 seconds
```

### Regression Tests
```text
Existing Parts 01-18 tests still pass
```

---

## 13. Android

Part 19 is backend-only. Minimal model adjustments only.

---

## 14. README — REQUIRED

Document:
- Part 19 completion
- Virality states and thresholds
- Virality score calculation
- Engagement velocity
- Reach estimation
- Viral consequences
- Spread mechanics
- NPC virality awareness
- API endpoints
- Database changes
- Tests performed
- Current status
- Next planned part

---

## 15. Git

After implementation:
1. Inspect `git status`
2. Commit: `Implement virality system (Part 19)`
3. Push to `origin/main`
4. Verify against origin

---

## 16. DO NOT IMPLEMENT YET

- Full trending system (Part 20)
- Rumor mechanics (Part 21)
- News coverage (Part 22)
- Virality predictions
- Anti-viral mechanics
- Paid promotion

---

## 17. QUALITY REQUIREMENTS

- Correct (virality scores accurate)
- Performant (batch processing)
- Configurable (thresholds in settings)
- Testable
- Permanent (all records persist)

---

## 18. FINAL VERIFICATION

```text
Server builds
Virality states defined correctly
Virality score calculates accurately
Engagement velocity works
Reach estimation works
Threshold detection triggers correctly
Viral consequences applied
Share cascade processes
NPC reactions trigger
Virality API returns data
Virality records persist
Existing tests pass
README updated
Git commit pushed
Working tree clean
```

---

## 19. FINAL SESSION REPORT

```text
# PART 19 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Virality Architecture
...

## 5. Virality States & Thresholds
...

## 6. Virality Score Calculation
...

## 7. Viral Consequences
...

## 8. Spread Mechanics
...

## 9. NPC Virality Awareness
...

## 10. API Endpoints
...

## 11. Database Changes
...

## 12. Tests
...

## 13. README
Updated: YES
...

## 14. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 15. Current Project Status
01A-19 COMPLETE

## 16. Intentionally Not Implemented
- Full trending (Part 20)
- Rumors (Part 21)
- News coverage (Part 22)

## 17. NEXT
NEXT: PART 20 — Topics & Trends
```

**STOP after completing Part 19 and reporting the session log.**
