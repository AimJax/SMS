# SOCIAL MEDIA SIMULATOR — PART 16 DEVELOPMENT PROMPT
## ADVANCED FEED

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
01D  SQLite Foundation               COMPLETE
01E  Android Client Foundation       COMPLETE
01F  Foundation Checkpoint           COMPLETE
02   Backend Architecture            COMPLETE
03   Persistence                     COMPLETE
04   Accounts & Authentication       COMPLETE
05   Social Graph                    COMPLETE
06   Posts & Engagement              COMPLETE
07   Feed & Timeline                 COMPLETE
08   NPC Simulator Foundation        COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation         COMPLETE
11   NPC Background Simulation       COMPLETE
12   NPC Social Graph                COMPLETE
13   AI Content Generation           COMPLETE
14   Notifications System            COMPLETE
15   Communities                    COMPLETE
```

Latest commit:

```text
6d3ec56 — Part 15 - Communities: Community entity, memberships, browsing, search, join/leave, NPC awareness, seed data, and comprehensive tests
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 15.

---

# 1. WHY THIS PART, NOW

Parts 01–15 built the core social media infrastructure with a basic chronological feed. Currently, the feed (Part 07) shows posts in reverse-chronological order from followed accounts, filtered by block/mute rules. This is functional but feels like a basic Twitter clone.

Part 16 transforms the feed into an **algorithmic experience** where different users see meaningfully different content based on their interests, behavior, relationships, and community affiliations. This is where the world starts to feel like a real social network rather than a simple timeline.

The Advanced Feed is foundational to several future systems:
- Echo chambers and information bubbles (Section 39 of Master Prompt)
- Trend propagation through communities (Part 20)
- Personalized discovery and recommendations (Part 08 already has basic search, but Part 16 feeds into it)
- Virality mechanics (Part 19) — the feed must surface viral content appropriately

Without an advanced feed, the network feels flat and undifferentiated. With it, each user's experience becomes unique, making the world feel alive and personalized.

---

# 2. THE EXISTING PROJECT

The existing backend already contains, among everything from Parts 01–15:

- Accounts, Profiles, Authentication, JWT
- `SocialGraphService` — Follow/Block/Mute rules
- Posts, Likes, Comments, Reposts (soft-deletable)
- `FeedService` — chronological, paginated, block/mute-aware feed (Part 07)
- `NotificationService` — follow/like/comment notifications (Part 14)
- `CommunityService` — communities with members and community-scoped posts (Part 15)
- `NpcSimulationHostedService` — autonomous background tick loop
- `NpcBehaviorService` / `NpcDecisionService` — NPCs with personality-driven reasoning
- `AiContentGeneratorService` + provider-agnostic `IAiTextGenerationService`
- Existing account interests and personality traits (Parts 09-13)
- Existing relationship dimensions (Familiarity, Friendship, Trust, etc.)

This means the raw data for feed scoring already exists:
- `Account.Interests` — what topics an account cares about
- `Relationship.*` dimensions — Familiarity, Friendship, Trust, etc.
- `CommunityMembership` — which communities an account belongs to
- `Post` engagement metrics — Likes, Comments, Reposts, Views
- `Post.Topic` — topic tags on posts
- Post author's attributes — Fame, Influence, Verified status

Part 16's job is to **score and rank** posts using this existing data, not to create new raw data.

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

The feed is generated and ranked by the server. The client cannot manipulate feed ranking. The server determines which posts to surface based on the user's profile, behavior, and algorithmic scoring.

## Layered architecture

```text
API
Application
Domain
Infrastructure
Contracts
```

Feed scoring follows the same layered pattern. Business logic (scoring algorithms) lives in Application or Domain layers, not in controllers.

## Reuse, don't duplicate

Do not recreate the feed endpoint from scratch. Extend the existing `FeedService` with algorithmic ranking capabilities. Do not duplicate post-fetching logic.

## Permanent data rule

Per the project's established permanent-history principle (Part 01B), all post data, engagement metrics, and feed interaction data must NOT be automatically deleted/pruned.

## Performance

With potentially thousands of candidate posts per feed request, scoring must remain efficient. Use:
- Caching for frequently-accessed scoring data
- Batch scoring where appropriate
- Appropriate query optimization
- Consider early filtering before expensive scoring

---

# PART 16 OBJECTIVE

Implement an **Advanced Feed System** that replaces or enhances the basic chronological feed with an algorithmic feed:

1. **Feed Scoring System** — A scoring framework that evaluates posts based on multiple factors.
2. **Interest Matching** — Score posts based on topic/interest alignment with the viewing account.
3. **Relationship Affinity** — Score posts higher from accounts the user has strong relationships with.
4. **Engagement Scoring** — Factor in post engagement (likes, comments, reposts) into ranking.
5. **Community Affinity** — Boost posts from communities the user belongs to.
6. **Recency Decay** — Posts age over time but can be "resurrected" by engagement spikes.
7. **Discovery Component** — Include posts from accounts the user doesn't follow (with appropriate attribution).
8. **Echo Chamber Support** — Allow the algorithm to strengthen or弱化 echo chamber effects based on user behavior.

Do NOT implement in this part:

- Real-time feed updates (WebSockets) — deferred to Part 34.
- Full recommendation engine with collaborative filtering — use content-based scoring only for now.
- Feed ad injection or promoted content.
- Read-time tracking (tracking how long users spend on posts).
- Feed customization UI/settings in Android — backend only for now.
- Trending/virality mechanics (Part 19) — but the feed should be aware of viral posts.

---

# PART 16 — REQUIRED FEATURES

## 1. Feed Scoring Framework

Create a scoring framework that evaluates each candidate post for a given user. The framework should be extensible and configurable.

### Scoring Factors (Weights)

Design a weighted scoring system with these primary factors:

```text
RecencyScore          (0.0 - 1.0) — How recent is the post? Exponential decay.
InterestMatchScore    (0.0 - 1.0) — Does the post topic match user's interests?
RelationshipAffinityScore (0.0 - 1.0) — How strong is the user's relationship with the author?
EngagementScore       (0.0 - 1.0) — How much engagement does the post have?
CommunityAffinityScore (0.0 - 1.0) — Is the post from a community the user belongs to?
AuthorFameScore       (0.0 - 1.0) — Boost/penalize based on author's fame/influence.
DiscoveryScore        (0.0 - 1.0) — Boost for posts from non-followed accounts (discovery).
```

### Final Score Calculation

```text
FinalScore = (RecencyScore * W_recency) 
           + (InterestMatchScore * W_interest) 
           + (RelationshipAffinityScore * W_relationship) 
           + (EngagementScore * W_engagement) 
           + (CommunityAffinityScore * W_community) 
           + (AuthorFameScore * W_fame) 
           + (DiscoveryScore * W_discovery)
```

### Default Weights

Start with these reasonable defaults (document them and make them configurable):

```text
W_recency = 0.25      (recency matters, but not everything)
W_interest = 0.20     (content relevance is important)
W_relationship = 0.20 (we trust people we know)
W_engagement = 0.15   (social proof matters)
W_community = 0.10     (community content is relevant)
W_fame = 0.05         (author fame is minor signal)
W_discovery = 0.05    (some discovery is good)
```

Weights should be configurable via app settings (not hardcoded), so they can be tuned without code changes.

---

## 2. Recency Score

Implement time-based decay for posts:

```text
RecencyScore = base_decay ^ (hours_since_post / decay_half_life)
```

Where:
- `base_decay` = 0.5 (or configurable)
- `decay_half_life` = 6 hours (or configurable, meaning post loses half its recency score after 6 hours)

Special cases:
- Posts under 1 hour old should have RecencyScore close to 1.0
- Posts over 24 hours old should have RecencyScore close to 0.0 (but not zero, so they can still appear with high scores from other factors)
- Posts that receive significant engagement (likes, comments, reposts) within the last hour should get a "resurrection" boost — this is handled by the EngagementScore factor, not RecencyScore

---

## 3. Interest Match Score

Match post topics to account interests:

- Posts have `Topic` field (already exists from Part 06)
- Accounts have `Interests` field (already exists from Parts 09-13)
- Calculate Jaccard similarity or simple overlap:

```text
InterestMatchScore = (matching_interests.Count) / (post_topics.Count + account_interests.Count - matching_interests.Count)
```

Or simpler:

```text
InterestMatchScore = matching_interests.Count > 0 ? 1.0 : 0.0
```

For now, use the simpler binary approach. More sophisticated matching (semantic similarity using embeddings) is a future optimization.

---

## 4. Relationship Affinity Score

Use existing relationship data to score posts:

- Query the account's relationship with the post author
- Use the strongest positive relationship dimension:

```text
RelationshipAffinityScore = max(
    Relationship.Familiarity / 100,
    Relationship.Friendship / 100,
    Relationship.Trust / 100,
    Relationship.Admiration / 100
)
```

For accounts with no relationship record, use a baseline score (e.g., 0.1 for unknown accounts, or base it on follower/following status).

Accounts you follow but have no explicit relationship record could get a baseline boost (e.g., 0.3).

---

## 5. Engagement Score

Factor post engagement into ranking:

```text
EngagementScore = normalize(likes + comments + reposts)
```

Use logarithmic normalization to prevent extremely viral posts from dominating:

```text
EngagementScore = min(1.0, log(1 + total_engagement) / log(max_expected_engagement))
```

Where `max_expected_engagement` could be 1000 or configurable.

Also consider engagement velocity (recent engagement vs total engagement):

```text
VelocityBonus = recent_engagement / total_engagement  // High velocity = trending
EngagementScore += VelocityBonus * 0.2
```

---

## 6. Community Affinity Score

If the post is from a community the user belongs to, boost the score:

```text
CommunityAffinityScore = 0.8 if user is community member AND post.CommunityId is not null
                        = 0.5 if post is about a topic user has community membership in
                        = 0.0 otherwise
```

This incentivizes community participation without making the feed exclusively community-based.

---

## 7. Author Fame Score

Use author's Fame and Influence metrics:

```text
AuthorFameScore = normalize(author.Fame + author.Influence)
```

A small boost/penalty based on author status. Don't let fame dominate — it should be a minor signal.

---

## 8. Discovery Score

Encourage content discovery by scoring posts from non-followed accounts higher:

```text
DiscoveryScore = 0.8 if user does NOT follow author AND author is not in user's feed history
               = 0.3 if user does NOT follow author but has seen author before
               = 0.0 if user follows author
```

This ensures users see new accounts and prevents the feed from becoming an echo chamber of only followed accounts.

Add a configurable "discovery quota" — e.g., at least 10% of feed should be discovery content.

---

## 9. Echo Chamber Controls

Add user-level settings or automatic behavior to control echo chamber strength:

### Configurable Echo Chamber Strength

```text
EchoChamberStrength = 0.0 - 1.0  // User-configurable or auto-detected
```

Where:
- 0.0 = No echo chamber (pure chronological/diversity)
- 0.5 = Balanced (default)
- 1.0 = Strong echo chamber (only interests/following content)

The echo chamber affects:
- DiscoveryScore weight: `W_discovery * (1 - EchoChamberStrength)`
- RelationshipAffinityScore weight: `W_relationship * (1 + EchoChamberStrength * 0.5)`

Auto-detection (optional for this part):
- If user predominantly engages with posts from a narrow set of accounts, increase EchoChamberStrength automatically
- If user frequently engages with discovery content, decrease EchoChamberStrength

---

## 10. Feed Service Enhancement

Extend the existing `FeedService` from Part 07:

### New Method: GetAdvancedFeed

```csharp
Task<FeedResponse> GetAdvancedFeedAsync(Guid accountId, FeedCursor cursor, int pageSize = 20);
```

This replaces or augments the existing `GetFeedAsync` method.

### Feed Candidate Generation

Before scoring, generate candidate posts:

```text
1. Posts from followed accounts (last 24 hours)
2. Posts from communities the user belongs to (last 24 hours)
3. Discovery posts (from non-followed accounts with high engagement, last 24 hours)
4. Trending/viral posts (if Part 19 mechanics exist, use them; otherwise skip)
```

Limit candidate pool to ~200-500 posts to avoid expensive scoring on every request.

### Scoring Pipeline

```text
For each candidate post:
    1. Calculate RecencyScore
    2. Calculate InterestMatchScore
    3. Calculate RelationshipAffinityScore
    4. Calculate EngagementScore
    5. Calculate CommunityAffinityScore
    6. Calculate AuthorFameScore
    7. Calculate DiscoveryScore
    8. Apply weights to get FinalScore
    9. Store (PostId, FinalScore)
    
Sort candidates by FinalScore descending
Return top N with pagination
```

---

## 11. Feed Caching

For performance, cache computed scores and feed results:

### Cache Keys

```text
feed:{accountId}:page:{cursor}  // Cached feed page
score:{postId}:{accountId}      // Cached score for a post/user combo
```

### Cache Invalidation

Invalidate cache when:
- User follows/unfollows an account
- User joins/leaves a community
- A post in the user's feed receives significant engagement (threshold: 10+ likes/comments/reposts)
- New post from a followed account
- Cache TTL: 5 minutes for feed pages, 1 minute for scores

Do NOT cache for more than 10-15 minutes — the feed must remain reasonably current.

---

## 12. Feed Metrics Tracking

Track feed interaction data to improve the algorithm (for future optimization):

```text
FeedImpression
    AccountId
    PostId
    Position (where in feed it appeared)
    Clicked (did user click/expand)
    Liked (did user like)
    Commented (did user comment)
    Shared (did user share/repost)
    Skipped (did user scroll past without interaction)
    Timestamp
```

This data is used for future A/B testing and algorithm tuning. It does NOT affect Part 16's scoring — it just records for later use.

---

## 13. API Endpoint

Update or add the advanced feed endpoint:

```http
GET /api/feed
```

Existing Part 07 endpoint should continue to work for backward compatibility, but the default behavior should return the advanced feed.

Query parameters:

```text
?cursor={cursor}           // Pagination cursor
&pageSize={size}          // Items per page (default 20, max 50)
&includeDiscovery={bool}  // Include non-followed content (default true)
&echoStrength={0.0-1.0}    // Override echo chamber strength
```

Response includes:
```text
Posts (with full post data)
NextCursor
Metadata:
    - TotalCandidates (how many posts were scored)
    - ScoreBreakdown (optional, for debugging)
```

---

## 14. NPC Behavior Integration

NPCs should respond to the advanced feed:

- NPCs should have a preference for certain content types (based on their interests, Part 10)
- NPCs should be more likely to engage with posts that score highly under the advanced feed algorithm (simulating realistic user behavior)
- This means NPCs will naturally reinforce the algorithm's preferences, making the feed feel more "real"

Document how NPC engagement interacts with the feed algorithm.

---

## 15. Database Migration

This part primarily uses existing data but may require:

- FeedImpression table for tracking (new table)
- Configuration table or app settings for scoring weights
- Indexes on Post for efficient candidate queries (Post.CreatedAt, Post.AuthorId, Post.CommunityId)

---

## 16. Tests

Add tests for the scoring system:

### Unit Tests

```text
RecencyScore decreases exponentially over time
RecencyScore is 1.0 for very recent posts
InterestMatchScore returns 1.0 for matching interests
InterestMatchScore returns 0.0 for non-matching interests
RelationshipAffinityScore returns correct value for known relationships
RelationshipAffinityScore returns baseline for unknown accounts
EngagementScore normalizes correctly
CommunityAffinityScore boosts community posts
DiscoveryScore differentiates followed vs non-followed
FinalScore combines all factors with correct weights
```

### Integration Tests

```text
Advanced feed returns posts sorted by score (descending)
Advanced feed includes discovery posts (non-followed accounts)
Advanced feed respects user's interests
Advanced feed includes community posts
Advanced feed respects block/mute rules
Pagination works correctly
Cache invalidation works
```

### Performance Tests

```text
Scoring 200 posts completes in under 100ms
Scoring 500 posts completes in under 250ms
Feed request with cold cache completes in under 500ms
Feed request with warm cache completes in under 100ms
```

### Regression Tests

```text
Existing Parts 01-15 tests still pass
```

---

## 17. Android

Part 16 is primarily a backend task. Do NOT build a full Android feed customization UI or algorithm settings screen.

If the existing Android project requires a minimal adjustment (e.g., a data model for the feed metadata/breakdown), make only the necessary change. The existing feed display should continue to work with the new backend.

---

## 18. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

Document:

- Part 16 completion
- Feed scoring framework architecture
- All scoring factors and their weights
- Echo chamber controls
- Configuration options for weights
- Feed caching strategy
- FeedImpression tracking for future optimization
- API endpoint changes
- NPC behavior integration with the feed
- Performance characteristics
- Tests performed and results
- Current project status
- Next planned part

---

## 19. Git

After implementation and verification:

1. Inspect `git status`.
2. Review changed files. Ensure no generated junk or unrelated files are committed.
3. Commit the completed work.

Suggested commit message:

```text
Implement advanced feed with algorithmic scoring (Part 16)
```

Push to `origin/main`. Verify the push actually reached the remote before reporting success.

---

## 20. DO NOT IMPLEMENT YET

Do NOT implement the following in Part 16:

```text
Real-time feed updates (WebSockets/SignalR)
Collaborative filtering / matrix factorization
Semantic/embedding-based interest matching
Feed ad injection or promoted content
Feed customization UI/settings in Android
Trending/virality detection (Part 19)
User behavioral profiling beyond basic metrics
Machine learning-based scoring
A/B testing framework
```

Those belong to later parts.

---

## 21. DEVELOPMENT PROCESS

Before changing anything:

1. Confirm `git status`/`git fetch` show a clean, synced state.
2. Inspect the existing `FeedService` from Part 07.
3. Inspect the `Post` entity for relevant fields (Topic, CommunityId, engagement counts).
4. Inspect the account entity for Interests and relationship data.
5. Inspect `CommunityService` for community membership data.
6. Inspect existing scoring/calculation patterns in the codebase.
7. Inspect caching patterns if any exist.
8. Inspect the existing authentication/authorization conventions.
9. Inspect the README.

Then implement Part 16. Do not assume a file does not exist merely because this prompt says to create it. Reuse existing functionality wherever appropriate. Do not duplicate business logic. Do not perform unrelated refactoring.

---

## 22. QUALITY REQUIREMENTS

The implementation must be:

- correct (scores calculate correctly)
- configurable (weights can be changed without code)
- performant (sub-500ms feed generation)
- extensible (easy to add new scoring factors)
- cacheable (warm cache improves performance significantly)
- testable (each scoring factor has unit tests)
- maintainable (clear separation of concerns)
- compatible with existing Part 07 feed behavior

---

## 23. FINAL VERIFICATION

Before declaring Part 16 complete, verify:

```text
Server builds
Feed returns posts sorted by calculated score
Recency affects scoring (newer posts generally rank higher)
Interest matching affects scoring (posts matching user interests rank higher)
Relationship affinity affects scoring (posts from friends rank higher)
Community posts from joined communities rank higher
Discovery posts from non-followed accounts appear in feed
Echo chamber strength affects feed diversity
Weights are configurable via settings
Caching improves performance (document improvement)
FeedImpression tracking records user interactions
Pagination works correctly
Existing Parts 01-15 tests still pass
README updated
Git commit created
Git push succeeds and is verified against origin
Working tree clean
```

---

## 24. FINAL SESSION REPORT

When finished, provide a complete session report in this structure:

```text
# PART 16 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Feed Scoring Architecture
...

## 5. Scoring Factors & Weights
...

## 6. Echo Chamber Implementation
...

## 7. Caching Strategy
...

## 8. Performance Characteristics
...

## 9. API Changes
...

## 10. Tests
...

## 11. README
Updated: YES
...

## 12. Git
Commit: ...
Push: ...
Verified against origin: ...
Working tree: ...

## 13. Current Project Status

01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
01F COMPLETE
02  COMPLETE
03  COMPLETE
04  COMPLETE
05  COMPLETE
06  COMPLETE
07  COMPLETE
08  COMPLETE
09  COMPLETE
10  COMPLETE
11  COMPLETE
12  COMPLETE
13  COMPLETE
14  COMPLETE
15  COMPLETE
16  COMPLETE

## 14. Intentionally Not Implemented
- Real-time feed updates (WebSockets)
- Collaborative filtering
- Semantic interest matching
- Feed ad injection
- Android feed customization UI
- Virality detection
- ML-based scoring

## 15. NEXT

NEXT: PART 17 — ...
```

Do not claim completion until the implementation and verification have actually succeeded.

**STOP after completing Part 16 and reporting the session log.**
