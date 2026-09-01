# Social Media Simulator

**Persistent online social-media simulation.**

## Project Status

| Part | Description | Status |
|------|-------------|--------|
| 01A | Development Environment | COMPLETE |
| 01B | Repository Foundation | COMPLETE |
| 01C | ASP.NET Core Server | COMPLETE |
| 01D | SQLite Foundation | COMPLETE |
| 01E | Android Client Foundation | COMPLETE |
| 01F | Foundation Checkpoint | COMPLETE |
| 02 | Backend Architecture | COMPLETE |
| 03 | Persistence | COMPLETE |
| 04 | Accounts & Authentication | COMPLETE |
| 05 | Social Graph | COMPLETE |
| 06 | Posts & Engagement | COMPLETE |
| 07 | Feed & Timeline | COMPLETE |
| 08 | NPC Simulator Foundation | COMPLETE |
| 09 | NPC Population Generation | COMPLETE |
| 10 | NPC Behavior Simulation | COMPLETE |
| 11 | NPC Background Simulation | COMPLETE |
| 12 | NPC Social Graph | COMPLETE |
| 13 | AI Content Generation | COMPLETE |
| 14 | Notifications System | COMPLETE |
| 15 | Communities | COMPLETE |
| 16 | Advanced Feed | COMPLETE |
| 17 | LLM-Driven Event System | COMPLETE |
| 18 | Event Causality & Offline Simulation | COMPLETE |
| 19 | Virality & Trending | COMPLETE |
| 20 | Topics & Trends | COMPLETE |
| 21 | Deployment & Testing | COMPLETE |
| 22 | Rumors & Misinformation | COMPLETE |

**NEXT: PART 23 — Permanent Memory**

## Getting Started

### Prerequisites
- .NET 10 SDK
- Android SDK
- Ollama running locally (for AI content generation)

### Configuration

1. **Ollama Settings** — Edit `Server\appsettings.json`:
   ```json
   "AiProvider": {
     "Provider": "Generic",
     "BaseUrl": "http://localhost:11434",
     "Model": "JFqxh_kUnppKPlpmBsDZBMeG",
     "ApiKey": "eb83536349244577bc482f76d21bc55f.JFqxh_kUnppKPlpmBsDZBMeG"
   }
   ```

2. **Android Settings** — Edit `Client\Configuration\AppConfig.cs`:
   ```csharp
   // For Android emulator: http://10.0.2.2:5225
   // For physical device on same network: http://YOUR_COMPUTER_IP:5225
   public string ApiBaseUrl { get; set; } = "http://10.0.2.2:5225";
   ```

### Running

1. **Start Ollama** (if not running):
   ```bash
   ollama serve
   ```

2. **Start Server**:
   ```bash
   cd Server
   dotnet run
   ```
   Expected output:
   ```
   Now listening on: http://0.0.0.0:5225
   Database initialized successfully.
   AI provider seeded: Generic / JFqxh_kUnppKPlpmBsDZBMeG
   ```

3. **Test Server**:
   ```bash
   curl http://localhost:5225/api/health
   # Expected: {"status":"ok"}
   ```

4. **Build & Install Android**:
   ```bash
   cd Client
   dotnet build -f net10.0-android -c Release
   adb install -r bin/Release/net10.0-android/com.companyname.socialmediasimulator.apk
   ```

5. **Open app and register!**

### First Run

1. Register a new account
2. Wait 1-2 minutes for simulation tick
3. Watch NPCs start posting
4. Join communities
5. See trends emerge

### Testing Checklist

- [x] `dotnet build` succeeds
- [x] `dotnet run` starts without errors
- [x] AI config is seeded
- [ ] NPCs post content
- [ ] Feed populates

## Architecture

```
Android Client
      ↓
HTTP REST
      ↓
API Layer (Controllers, Middleware)
      ↓
Application Layer (Services, Interfaces)
      ↓
Domain Layer (Entities)
      ↓
Infrastructure (EF Core / SQLite)
```

### Backend Layer Structure

```
Server/
├── API/
│   ├── Controllers/       API endpoints (Auth, Account, Graph, Posts)
│   └── Middleware/       Exception handling
├── Application/
│   └── Services/         Business logic (AccountService, JwtService, SocialGraphService, PostService, FeedService)
├── Domain/
│   └── Entities/         Account, Profile, Follow, Block, Mute, AccountHistory, Post, PostLike, Comment, Community, CommunityMembership, NpcProfile, NpcAction
├── Infrastructure/
│   └── Persistence/      EF Core DbContext, Entity configurations, Migrations
├── Contracts/
│   ├── Requests/        API request DTOs
│   └── Responses/       API response DTOs
├── Extensions/           DI registration
└── Program.cs
```

## Account Architecture

### Account Model
- **AccountId** (GUID) — Stable identity, never changes
- **Username** — Unique, case-insensitive
- **PasswordHash** — PBKDF2 with SHA256
- **Email** — Optional
- **AccountType** — OrdinaryUser, Creator, Influencer, Celebrity, Official, News
- **Status** — Active, Disabled, Suspended, Banned
- **CreatedAt/UpdatedAt** — Timestamps

### Profile Model
- **AccountId** (FK) — Links to Account
- **DisplayName** — Public display name
- **Bio** — Optional biography
- **AvatarUrl** — Optional avatar

### Account History
- Permanent record of account events
- Event types: Created, UsernameChanged, DisplayNameChanged, etc.
- Never deleted

## Social Graph

### Follow Model
- **FollowerAccountId** — The account following
- **FollowedAccountId** — The account being followed
- **CreatedAt** — When follow occurred
- Unique constraint prevents duplicate follows
- Self-follow not allowed

### Block Model
- **BlockerAccountId** — The account blocking
- **BlockedAccountId** — The account blocked
- **CreatedAt** — When block occurred
- Blocks remove conflicting follow relationships (transactional)
- Blocked accounts cannot follow the blocker

### Mute Model
- **MuterAccountId** — The account muting
- **MutedAccountId** — The account muted
- **CreatedAt** — When mute occurred
- Muting does NOT remove follow relationships
- Separate from blocking

### Blocking Behavior
When Account A blocks Account B:
1. Any follow A→B is removed
2. Any follow B→A is removed
3. B cannot follow A while blocked
4. B cannot unblock A (only A can)

## Posts & Engagement

### Post Model
- **PostId** (GUID) — Stable identity, never changes
- **AuthorAccountId** — FK to Account
- **Content** — Text content, max 10,000 characters
- **Status** — Active, Deleted (soft delete)
- **CreatedAt/UpdatedAt** — Timestamps

### PostLike Model
- **PostId** — FK to Post
- **AccountId** — FK to Account
- **CreatedAt** — When like occurred
- **UNIQUE** constraint on (PostId, AccountId) prevents duplicate likes

### Comment Model
- **CommentId** (GUID) — Stable identity
- **PostId** — FK to Post
- **AuthorAccountId** — FK to Account
- **Content** — Text content, max 2,000 characters
- **Status** — Active, Deleted (soft delete)
- **CreatedAt/UpdatedAt** — Timestamps

### Soft Delete Behavior
Posts and comments use soft delete (Status field) to preserve data integrity and history.

## Feed & Timeline Architecture

### Feed Service
The feed is generated server-side using `IFeedService`:
- Queries posts from followed accounts
- Filters out blocked and muted accounts
- Uses cursor-based pagination for scalability
- Batches queries to avoid N+1 patterns

### Feed Response Format
```json
{
  "items": [
    {
      "postId": "guid",
      "authorAccountId": "guid",
      "authorUsername": "string",
      "authorDisplayName": "string",
      "authorAvatarUrl": "string|null",
      "content": "string",
      "createdAt": "datetime",
      "likeCount": 0,
      "commentCount": 0,
      "isLikedByCurrentUser": true|false
    }
  ],
  "nextCursor": "cursor-string|null",
  "pageSize": 20
}
```

### Cursor-Based Pagination
- Cursor format: `{timestamp}_{postId}`
- Client stores cursor and sends it to get next page
- `nextCursor: null` indicates no more pages
- Deterministic ordering prevents duplicates

## Authentication

- **JWT Bearer Tokens** — 7-day expiration
- **PBKDF2 Password Hashing** — 100,000 iterations
- **Claims-based Authorization** — NameIdentifier maps to Account.Id

## API Endpoints

### Authentication
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/register` | POST | No | Register new account |
| `/api/auth/login` | POST | No | Login and receive JWT |

### Account
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/account/me` | GET | Yes | Get authenticated account |
| `/api/account/{accountId}` | GET | Yes | Get public profile |

### Social Graph
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/accounts/{id}/follow` | POST | Yes | Follow account |
| `/api/accounts/{id}/follow` | DELETE | Yes | Unfollow account |
| `/api/accounts/{id}/followers` | GET | No | Get followers (paginated) |
| `/api/accounts/{id}/following` | GET | No | Get following (paginated) |
| `/api/accounts/{id}/block` | POST | Yes | Block account |
| `/api/accounts/{id}/block` | DELETE | Yes | Unblock account |
| `/api/accounts/{id}/mute` | POST | Yes | Mute account |
| `/api/accounts/{id}/mute` | DELETE | Yes | Unmute account |
| `/api/accounts/{id}/relationship` | GET | Yes | Get relationship status |

### Posts & Engagement
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/posts` | POST | Yes | Create a new post |
| `/api/posts/{postId}` | GET | No | Get post by ID |
| `/api/posts/{postId}` | DELETE | Yes (owner) | Delete post |
| `/api/posts/{postId}/like` | POST | Yes | Like a post |
| `/api/posts/{postId}/like` | DELETE | Yes | Unlike a post |
| `/api/posts/{postId}/comments` | GET | No | Get post comments (paginated) |
| `/api/posts/{postId}/comments` | POST | Yes | Add comment to post |
| `/api/comments/{commentId}` | DELETE | Yes (owner) | Delete comment |

### Feed & Timeline
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/feed` | GET | Yes | Get personalized feed (algorithmic) |

**Advanced Feed (Part 16):**
The feed now uses algorithmic scoring with multiple factors:
- **Recency** (25%) — Exponential decay, half-life of 6 hours
- **Interest Match** (20%) — Matches post topics to user interests
- **Relationship Affinity** (20%) — Stronger signal from friends/followed accounts
- **Engagement** (15%) — Log-normalized likes/comments with velocity bonus
- **Community** (10%) — Boost for posts from joined communities
- **Fame** (5%) — Minor boost for high-profile accounts
- **Discovery** (5%) — Boost for non-followed accounts

**Query Parameters:**
- `cursor` — Pagination cursor
- `pageSize` — Items per page (1-50, default 20)
- `includeDiscovery` — Include non-followed content (default true)
- `echoStrength` — Echo chamber strength override (0.0-1.0)

**Echo Chamber Controls:**
- 0.0 = Maximum diversity, strong discovery content
- 0.5 = Balanced (default)
- 1.0 = Strong echo chamber, minimal discovery

**Feed Behavior:**
- Excludes posts from blocked accounts (in either direction)
- Excludes posts from muted accounts
- Excludes soft-deleted posts
- Candidate pool limited to ~200 posts (last 24 hours)
- Discovery quota: minimum 10% non-followed content
- Cursor-based pagination for scalability
- In-memory caching with 5-minute TTL

### Communities
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/communities` | GET | No | Browse public communities |
| `/api/communities/search` | GET | No | Search communities by name/tags |
| `/api/communities/by-topic/{topic}` | GET | No | Get communities by topic |
| `/api/communities/{slug}` | GET | Yes* | Get community details |
| `/api/communities/{slug}/feed` | GET | Yes | Get community posts |
| `/api/communities/{slug}/members` | GET | No | Get community members |
| `/api/communities/{slug}/join` | POST | Yes | Join a community |
| `/api/communities/{slug}/leave` | POST | Yes | Leave a community |
| `/api/account/communities` | GET | Yes | Get user's communities |

*Private communities require membership.

**Community Visibility:**
- **Public** — Visible to everyone, joinable by anyone
- **Private** — Visible to members only
- **Hidden** — Not listed in browse/search

**Community Roles:**
- **Owner** — Full control, cannot leave
- **Admin** — Can manage members
- **Moderator** — Can moderate content
- **Member** — Basic access

## LLM-Driven Event System (Part 17)

### Overview
The LLM-Driven Event System introduces emergent storytelling to the simulation. Local Ollama acts as a narrative director, analyzing the social landscape and proposing interesting, dramatic events that emerge organically from the world state.

### Architecture
```
Simulation Tick Loop
       ↓
World State Analysis (via Ollama)
       ↓
Event Proposal Generation
       ↓
Server Validation
       ↓
Event Execution (consequences applied)
```

### Event Types
- **Drama**: JealousyIncident, PublicArgument, Betrayal, RedemptionArc, ComebackStory, DownfallStory
- **Romance**: NewRelationship, Breakup, LoveTriangle, SecretRelationship, RelationshipMilestone, Reconciliation
- **Social**: NewFriendship, FriendshipEnded, Alliance, Rivalry, FanWar, TrollAttack
- **Fame**: RiseToFame, FallFromGrace, Scandal, Apology, Comeback, Cancellation
- **Community**: CommunityDriven, CommunitySplit, CommunityMilestone, CommunityDrama
- **Content**: ViralPost, ViralComment, QuotePostDrama, PollControversy
- **Trend**: TrendStart, TrendPivot, TrendDeath
- **News**: NewsCoverage, BreakingNews, NewsDebate

### Event Entities
- **Event**: Main event record with title, description, narrative context, participants, status
- **EventParticipation**: Account involvement with role (Protagonist, Antagonist, Supporter, etc.)
- **EventConsequence**: Audit trail of applied consequences (relationship changes, posts, etc.)

### LLM Integration
- Uses existing `IAiTextGenerationService` (Ollama via GenericHttpProvider)
- System prompt defines narrative director role and rules
- Events emerge from actual world state, not random generation
- Validates all proposals before execution (account existence, block rules, etc.)

### Configuration
```json
"EventSystem": {
  "Enabled": true,
  "EventGenerationIntervalTicks": 5,
  "MaxActiveEvents": 20,
  "EventDurationHours": 24,
  "AutoApproveEvents": true
}
```

### API Endpoints
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/events` | GET | No | Browse events with filters |
| `/api/events/{id}` | GET | No | Event details with participants |
| `/api/events/{id}/participants` | GET | No | Event participant list |
| `/api/accounts/{id}/events` | GET | Yes | User's events |

## Event Causality (Part 18)

### Overview
The Event Causality system tracks why events happen by establishing formal causal chains between events. This creates a coherent narrative where the world feels interconnected rather than a series of disconnected incidents.

### Causal Chain Entity
Records causal relationships between events:
- **CauseType**: Direct, Indirect, Contributing, Trigger
- **CauseStrength**: 0.0-1.0 contribution factor
- **CauseDescription**: Human-readable explanation
- **Metadata**: JSON for additional context

### Event Chain Relationships
- **ParentEventId**: The event this emerged from
- **TriggerEventId**: The specific event that started the chain
- **EventChainId**: Groups related events together
- **ChainDepth**: Position in the chain (0 = root)

### Causal Tracking Service
```csharp
ICausalTrackingService
  RecordCausalLinkAsync()     // Record a cause-effect relationship
  GetCausalChainAsync()        // Get causes of an event
  GetEventChainAsync()         // Get all related events in chain
  GetRootCauseAsync()          // Find the original cause
  GetDownstreamEventsAsync()   // Find events caused by this one
  GenerateCausalNarrativeAsync() // LLM-generated story
```

### API Endpoints
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/events/{id}/chain` | GET | No | Get causal chain |
| `/api/events/{id}/event-chain` | GET | No | Get event chain |
| `/api/events/{id}/root-cause` | GET | No | Get root cause |
| `/api/events/{id}/downstream` | GET | No | Get downstream events |
| `/api/events/{id}/narrative` | GET | No | Generate narrative |

## Offline World Simulation (Part 18)

### Overview
The Offline Simulation system ensures the world continues running when players are away. When players return, they receive a catch-up summary of what happened.

### Time Compression Strategy
- **TicksPerHour**: 10 (configurable)
- **MaxTicksPerSession**: 1000 (cap for performance)
- **MinTicksToSimulate**: 5 (minimum even for short offline)
- For a 12-hour offline period: 120 compressed ticks

### Offline NPC Simulation
- Predicts NPC behavior using personality profiles
- Aggregates actions rather than simulating tick-by-tick
- Deterministic results using account-seeded random
- Generates posts, follower changes, and events

### Offline Simulation Service
```csharp
IOfflineSimulationService
  GetOfflineDurationAsync()    // Calculate time away
  ShouldRunOfflineSimulationAsync() // Check if simulation needed
  RunOfflineSimulationAsync()  // Run simulation and return summary
  GetCatchupSummaryAsync()     // Get latest summary
  AcknowledgeCatchupAsync()   // Mark as seen
  HasUnreadCatchupAsync()     // Check for new catchup
```

### Catchup Summary
- Duration of offline period
- Follower changes (gained/lost)
- Posts created during offline
- Major events that occurred
- LLM-generated narrative summary

### Configuration
```json
"OfflineSimulation": {
  "Enabled": true,
  "MinOfflineHoursBeforeSimulation": 1,
  "TicksPerHour": 10,
  "MaxTicksPerSession": 1000,
  "EventProbabilityMultiplier": 0.5
}
```

### API Endpoints
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/catchup` | GET | Yes | Get catchup summary |
| `/api/catchup/acknowledge` | POST | Yes | Mark as seen |
| `/api/catchup/has-unread` | GET | Yes | Check for unread |
| `/api/catchup/duration` | GET | No | Get offline duration |

### Database Tables
- **CausalChain**: Tracks cause-effect relationships
- **OfflineSimulationResult**: Persists simulation results

## Virality System (Part 19)

The virality system enables posts to achieve organic viral spread through the social network. Posts that cross engagement thresholds become "viral" and trigger consequences for their authors.

### Virality States
Posts progress through these virality states based on engagement:

| State | Engagement Threshold | Velocity Required | Description |
|-------|---------------------|-------------------|-------------|
| Normal | < 50 | No | Standard post |
| Trending | ≥ 50 | No | Gaining traction |
| Popular | ≥ 200 | No | Above average |
| Viral | ≥ 1000 | ≥ 10/hr | Crossed viral threshold |
| MassivelyViral | ≥ 10000 | Yes | Extremely viral |
| Declining | (was viral) | (velocity dropped 70%) | Cooling down |

### Virality Score Formula
```
Score = EngagementScore (0-30) + VelocityScore (0-30) + ReachScore (0-20) + RelativeScore (0-20)
- EngagementScore: log10(totalEngagement + 1) * 10, capped at 30
- VelocityScore: velocity * 3, capped at 30
- ReachScore: log10(reach + 1) * 5, capped at 20
- RelativeScore: (engagement/followers) * 100, capped at 20
```

### Virality Metrics
- **TotalEngagement**: Likes + Comments + Reposts
- **Velocity**: Engagements per hour (within 24-hour window)
- **Reach**: Estimated unique viewers
- **PeakVelocity**: Highest velocity reached during post lifetime
- **ControversyLevel**: 0-10 score from LLM analysis

### Viral Consequences
When a post goes viral, the system applies:

1. **Follower Gain**: Base 10 + (engagement/50), multiplied by viral state
2. **Fame Gain**: Base 5 + (engagement/200), multiplied by viral state
3. **Viral Event**: Creates a ViralPost event for the event system
4. **Notification**: Alerts the author of their viral success

### Virality Service
```csharp
IViralityService
  CalculateViralityAsync(postId)     // Calculate metrics for a post
  GetViralityStateAsync(postId)      // Get current state
  GetPostViralityAsync(postId)       // Get full virality data
  GetViralPostsAsync(count, minState) // Get posts by virality level
  GetTrendingPostsAsync(count, topic) // Get trending posts
  TrackEngagementAsync(postId)        // Quick update after new engagement
  CheckThresholdsAsync(postId)        // Check for state transitions
  AnalyzeControversyAsync(postId)     // LLM controversy analysis
```

### Background Processing
ViralityProcessingService runs as a background service:
- Processes every 5 minutes (configurable)
- Processes up to 100 posts per tick
- Active posts tracked for 7 days
- Automatically marks declining posts

### Configuration
```json
"Virality": {
  "Enabled": true,
  "TrendingThreshold": 50,
  "PopularThreshold": 200,
  "ViralThreshold": 1000,
  "MassivelyViralThreshold": 10000,
  "ViralVelocityMin": 10,
  "ViralWindowHours": 24,
  "ProcessingIntervalMinutes": 5,
  "MaxPostsPerTick": 100,
  "ActivePostDays": 7,
  "DeclineVelocityDropPercent": 0.7,
  "BaseFollowerGainOnViral": 10,
  "BaseFameGainOnViral": 5.0
}
```

### API Endpoints
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/posts/viral` | GET | No | Get viral posts |
| `/api/posts/trending` | GET | No | Get trending posts |
| `/api/posts/{id}/virality` | GET | No | Get virality details |
| `/api/posts/{id}/virality-state` | GET | No | Get virality state |
| `/api/posts/{id}/virality-history` | GET | No | Get state transitions |
| `/api/posts/{id}/calculate-virality` | POST | No | Trigger calculation |
| `/api/posts/{id}/analyze-controversy` | POST | No | LLM controversy analysis |

### Database Tables
- **PostVirality**: Tracks virality metrics per post
- **ViralityTransition**: Logs state transitions

## Topics & Trends System (Part 20)

The Topics & Trends system connects viral content into social phenomena where multiple people discuss the same thing at the same time. Trends are foundational to rumors, news coverage, echo chambers, and content discovery.

### Topic Entity
Topics categorize posts and enable trend tracking:

```csharp
Topic {
    TopicId (Guid)     // Stable identifier
    Name               // "technology", "gaming"
    DisplayName        // "Technology", "Gaming"
    Slug               // URL-safe version
    Category           // Gaming, Technology, Sports, etc.
    PostCount          // Total posts ever
    ActivePostCount    // Posts in last 7 days
    SubscriberCount    // Users following topic
    IsVerified         // Official topic
}
```

### Topic Categories
- General, Entertainment, Gaming, Technology, Sports, Politics, News, Lifestyle, Art, Meme, Community, Event, Hashtag

### Pre-defined Topics
The system seeds 39 topics across categories:
- **Entertainment**: movies, tv, music, celebrities, anime, books
- **Gaming**: gaming, esports, pcgaming, mobilegaming, nintendoswitch, playstation, xbox
- **Technology**: technology, programming, ai, gadgets, smartphones, science
- **Sports**: sports, basketball, soccer, football, tennis
- **Lifestyle**: fashion, food, travel, fitness, photography, art
- **Meme**: memes, shitposting, wholesome, cringe
- **News/Politics**: news, politics, worldnews

### Hashtag Management
Hashtags are extracted from post content and tracked:

```csharp
Hashtag {
    HashtagId (Guid)
    Tag              // "#Gaming"
    NormalizedTag    // "gaming"
    TopicId          // Associated topic (nullable)
    UsageCount       // Total times used
    TodayUsageCount  // Used today
    IsTrending       // Currently trending
    TrendingSince    // When started trending
    TrendingRank     // Current rank (1 = most)
}
```

### Hashtag Extraction
```csharp
// Extracts #hashtags from content
ExtractHashtagsAsync("Check out #Gaming and #AI!") → ["gaming", "ai"]
```

### Hashtag → Topic Mapping
- Exact match: "ai" → AI topic
- Partial match: "nintendoswitch" → nintendoswitch topic
- No match: remains standalone hashtag

### Trend Entity
```csharp
Trend {
    TrendId (Guid)
    Type              // Topic, Hashtag, Event, Search, Viral
    TopicId           // Associated topic
    HashtagId         // Associated hashtag
    Query             // Search query
    DisplayName       // "Gaming", "#Gaming"
    Slug              // URL-safe
    
    // Metrics
    Strength          // Emerging, Growing, Hot, Viral, Peaking
    PostCount         // Posts in window
    UniquePosters     // Unique accounts
    EngagementTotal   // Total engagement
    Velocity          // Growth rate (posts/hour)
    Rank              // Position in trend list
    
    Scope             // Global, Community, Personal
    CommunityId       // If community-specific
    CalculatedAt      // When calculated
    PeakedAt          // When reached max
    ExpiresAt         // When expires
}
```

### Trend Strength Formula
```
Strength = weighted_avg(countScore, posterScore, velocityScore)
- CountScore: <10→0, <50→1, <200→2, <500→3, <1000→4, 1000+→5
- PosterScore: <5→0, <20→1, <50→2, <100→3, <200→4, 200+→5
- VelocityScore: <0.5→0, <1→1, <2→2, <5→3, <10→4, 10+→5
- Weights: count 40%, posters 30%, velocity 30%
```

### Trend Types
1. **Global Trends**: Spans entire network
2. **Community Trends**: Specific to a community
3. **Personal Trends**: Personalized to user's interests

### Trend Calculation
```csharp
CalculateTrendAsync(query, scope) {
    window = 24 hours
    posts = GetPostsMentioning(query, window)
    
    postCount = posts.Count
    uniquePosters = posts.Select(p => p.AuthorId).Distinct().Count
    engagement = posts.Sum(p => p.Likes + p.Comments)
    velocity = CalculateVelocity(posts, 24h)
    strength = CalculateStrength(postCount, uniquePosters, velocity)
    
    return Trend { postCount, uniquePosters, engagement, velocity, strength }
}
```

### Velocity Calculation
Uses linear regression on hourly post counts:
```csharp
// Groups posts by hour, calculates trend line slope
// Higher positive slope = faster growth
```

### Cross-Community Propagation
Trends spread between communities based on:
- Base probability: 10%
- Engagement boost: up to 30%
- Strength boost: up to 20%
- Shared members boost: up to 30%

### Trend Service
```csharp
ITrendService {
    // Topics
    GetTopicBySlugAsync(slug) → Topic
    GetAllTopicsAsync() → List<Topic>
    CreateTopicAsync(name, category) → Topic
    
    // Hashtags
    GetTrendingHashtagsAsync(count) → List<Hashtag>
    GetOrCreateHashtagAsync(tag) → Hashtag
    ExtractHashtagsAsync(content) → List<string>
    
    // Trends
    GetGlobalTrendsAsync(count) → List<Trend>
    GetCommunityTrendsAsync(communityId, count) → List<Trend>
    GetPersonalTrendsAsync(accountId, count) → List<Trend>
    CalculateTrendAsync(query, scope) → Trend
    ProcessTrendsTickAsync() → void
    
    // Subscriptions
    SubscribeToTopicAsync(accountId, topicId)
    UnsubscribeToTopicAsync(accountId, topicId)
}
```

### Background Processing
TrendProcessingService runs every 15 minutes:
1. Reset daily hashtag counts
2. Process new hashtags from recent posts
3. Calculate global trends
4. Update trend rankings
5. Process cross-community propagation
6. Expire old trends

### API Endpoints
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/trends` | GET | No | Get global trends |
| `/api/trends/{id}` | GET | No | Get specific trend |
| `/api/communities/{id}/trends` | GET | No | Community trends |
| `/api/me/trends` | GET | Yes | Personal trends |
| `/api/hashtags/trending` | GET | No | Trending hashtags |
| `/api/hashtags/{tag}` | GET | No | Hashtag details |
| `/api/topics` | GET | No | All topics |
| `/api/topics/search` | GET | No | Search topics |
| `/api/topics/{slug}` | GET | No | Topic details |
| `/api/topics/{slug}/posts` | GET | No | Posts for topic |
| `/api/topics/{id}/subscribe` | POST | Yes | Subscribe |
| `/api/topics/{id}/subscribe` | DELETE | Yes | Unsubscribe |
| `/api/trends/calculate` | POST | No | Manual calculation |
| `/api/trends/process` | POST | No | Trigger processing |

### Database Tables
- **Topics**: Topic definitions
- **Hashtags**: Tracked hashtags with usage stats
- **Trends**: Active trends with metrics
- **TrendPropagations**: Cross-community spread records
- **TopicSubscriptions**: User topic subscriptions

### Configuration
```json
"Trends": {
  "Enabled": true,
  "ProcessingIntervalMinutes": 15,
  "TrendWindowHours": 24,
  "MinPostsForTrend": 10,
  "MaxTrendingHashtags": 20,
  "TrendDurationHours": 24,
  "PropagationMultiplier": 1.0,
  "TopicPostCountDays": 7
}
```

## NPC Simulation Architecture

### Overview
The NPC simulation system provides the foundation for populating the social media platform with automated accounts that can interact, post, like, comment, and follow other accounts. The architecture is designed for extensibility, allowing future parts to add sophisticated behavior and LLM-powered content generation.

### NPC Account Types
NPCs use the existing `AccountType` enum with appropriate simulation intervals:
- **OrdinaryUser** — Regular users (30s interval)
- **Creator** — Content creators (30s interval)
- **Influencer** — Influencers (25s interval)
- **Celebrity** — Celebrities (15s interval)
- **Official** — Official/organizations (45s interval)
- **News** — News accounts (20s interval)

### NPC Entities

#### NpcProfile
Core NPC metadata and simulation state:
- **NpcId** (GUID) — Stable identity
- **AccountId** (FK) — Links to Account
- **IsActive** — Whether NPC participates in simulation
- **ActivityState** — Current activity (Idle, Browsing, Posting, Reading, Engaging, Offline)
- **LastSimulatedAt** — When NPC was last processed
- **NextSimulationAt** — When NPC should be processed next
- **SimulationIntervalSeconds** — How often to simulate
- **SimulationVersion** — Tracks state changes

#### NpcPersonality
Persistent Big Five personality traits (normalized 0.0-1.0):
- **Openness** — Curiosity and creativity
- **Conscientiousness** — Self-discipline and organization
- **Extraversion** — Sociability and energy
- **Agreeableness** — Trust and cooperation
- **Neuroticism** — Emotional stability

#### NpcInterest
NPC interests in content categories:
- **InterestKey** — Category name (Gaming, Politics, Sports, etc.)
- **Strength** — Interest strength (0.3-1.0)

Interest categories: Gaming, Politics, Sports, Technology, Music, Movies, Television, Fashion, Food, Travel, Science, Health, Business, Finance, Education, LocalNews, WorldNews, Entertainment, GamingNews, SportsNews, TechNews

#### NpcAction
Represents NPC actions for future behavior systems:
- **ActionType** — Type of action (ViewPost, LikePost, Comment, Follow, etc.)
- **TargetPostId/TargetAccountId** — Action targets
- **Content** — Content for post/comment actions
- **Executed** — Whether action was performed

### NPC Services

#### INpcService
- `CreateNpcAsync()` — Creates Account + Profile + NpcProfile + Personality + Interests
- `GetByNpcIdAsync()` / `GetByAccountIdAsync()` — Retrieves NPC with all related data
- `IsNpcAsync()` — Checks if account is NPC
- `ActivateAsync()` / `DeactivateAsync()` — Toggle NPC participation
- `GeneratePersonality()` — Deterministic personality from seed
- `GenerateInterests()` — Account-type-based interests from seed

#### INpcSimulationService
- `GetDueNpcsAsync()` — Finds NPCs due for simulation
- `ProcessNpcAsync()` — Updates simulation state for one NPC
- `ProcessTickAsync()` — Processes batch of due NPCs
- `UpdateNpcAfterSimulationAsync()` — Updates activity state

### Simulation Scheduling
NPCs are processed based on `NextSimulationAt`:
1. Query NPCs where `NextSimulationAt <= DateTime.UtcNow`
2. Filter for active NPCs with active accounts
3. Update `LastSimulatedAt` and calculate new `NextSimulationAt`
4. Increment `SimulationVersion`

### Deterministic Generation
NPC personality and interests are generated from a GUID seed using deterministic random:
- Same seed always produces identical traits
- Enables reproducible simulation runs
- Seed is the NPC's unique identifier

### Database Schema

#### NpcProfiles
| Column | Type | Constraints |
|--------|------|-------------|
| NpcId | TEXT | UNIQUE (GUID) |
| AccountId | INTEGER | FK → Accounts, UNIQUE |
| IsActive | INTEGER | boolean |
| ActivityState | INTEGER | enum |
| NextSimulationAt | TEXT | indexed |
| SimulationIntervalSeconds | INTEGER | |

#### NpcPersonalities
| Column | Type | Constraints |
|--------|------|-------------|
| Openness/Conscientiousness/etc | REAL | 0.0-1.0 |

#### NpcInterests
| Column | Type | Constraints |
|--------|------|-------------|
| InterestKey | TEXT | max 50 |
| Strength | REAL | 0.3-1.0 |
| **UNIQUE** | | (NpcProfileId, InterestKey) |

#### NpcActions
| Column | Type | Constraints |
|--------|------|-------------|
| ActionType | INTEGER | enum |
| TargetPostId/TargetAccountId | TEXT | nullable |
| Executed | INTEGER | boolean |

### Tests

#### NpcServiceTests (14 tests)
- NPC creation, simulation interval by account type, retrieval, identification, activation/deactivation, deterministic personality, interest generation, username collision

#### NpcSimulationServiceTests (9 tests)
- Due NPC filtering, inactive exclusion, state updates, batch processing, account status respect, activity state management

## NPC Population Generation

### Overview
The population generation system creates large numbers of NPCs with realistic diversity in usernames, display names, bios, personalities, and interests.

### Configuration

#### PopulationConfig
```csharp
public class PopulationConfig
{
    public int PopulationSize { get; set; } = 1000;
    public int? RandomSeed { get; set; }
    public AccountTypeDistribution Distribution { get; set; } = AccountTypeDistribution.Default;
    public string? BatchId { get; set; }
}
```

#### AccountTypeDistribution
Configurable account type percentages:
```csharp
public class AccountTypeDistribution
{
    public double OrdinaryUser { get; set; } = 70;  // 70%
    public double Creator { get; set; } = 12;       // 12%
    public double Influencer { get; set; } = 7;     // 7%
    public double News { get; set; } = 5;           // 5%
    public double Official { get; set; } = 4;       // 4%
    public double Celebrity { get; set; } = 2;       // 2%
}
```

### Services

#### INpcPopulationService
- `GeneratePopulationAsync(config)` — Generate NPCs with configuration
- `GeneratePopulationAsync(size, seed)` — Simple generation with size and optional seed
- `GetExistingNpcCountAsync()` — Count existing NPCs
- `PopulationExistsAsync()` — Check if population exists
- `ValidateConfig(config)` — Validate configuration

### Username Generation
Deterministic username generation with multiple strategies:
- Adjective + Noun (e.g., "pixelwanderer", "nightowl")
- Prefix + Noun (e.g., "techcreator", "cityupdates")
- Name-style (e.g., "alex42", "taylor_smith")
- Numbered (e.g., "gamer123")

Collision detection prevents duplicates within the same generation.

### Profile Generation
Profile data varies by account type:
- **OrdinaryUser** — Personal/social bios
- **Creator** — Content-oriented bios
- **Influencer** — Lifestyle-focused bios
- **Celebrity** — Public personality bios
- **Official** — Institutional bios
- **News** — News/media bios

Avatar URLs use DiceBear API with username as seed.

### PopulationResult
```csharp
public class PopulationResult
{
    public bool Success { get; set; }
    public int NpcsCreated { get; set; }
    public int NpcsFailed { get; set; }
    public TimeSpan Elapsed { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<AccountType, int> Distribution { get; set; }
    public int? SeedUsed { get; set; }
}
```

### Duplicate Generation Prevention
- System prevents duplicate generation if population already exists
- Must clear existing population before regenerating
- Each generation has optional batch identifier

### Performance
- Tested with 1,000 NPCs in ~2 minutes (in-memory database)
- Uses efficient batch processing
- Deterministic seed for reproducible results

### Tests

#### NpcPopulationServiceTests (18 tests)
- Configuration validation
- Account type distribution
- Duplicate generation prevention
- Deterministic generation
- All NPC data creation
- Username uniqueness
- Personality validity
- Interest validity

#### NpcPopulationPerformanceTests (3 tests)
- 1000 NPC generation performance
- 100 NPC generation performance
- 10 NPC generation performance

#### GeneratorTests (10 tests)
- Username generation uniqueness
- Deterministic generation
- Profile generation by type
- Distribution validation

## NPC Behavior Simulation

### Overview
The NPC behavior system brings NPCs to life by making them active social media users capable of following, liking, commenting, posting, and more. All decisions are influenced by personality traits, interests, and account type.

### Behavior Pipeline
```
NPC becomes due
      ↓
Gather social/content context
      ↓
Generate candidate actions
      ↓
Filter impossible actions
      ↓
Score actions
      ↓
Choose action
      ↓
Validate social rules
      ↓
Execute action
      ↓
Record action history
      ↓
Schedule next simulation
```

### Supported Actions
- **ViewFeed** — Browse content
- **ViewPost** — View specific post
- **LikePost** — Like a post
- **UnlikePost** — Unlike a previously liked post
- **Comment** — Comment on a post
- **Follow** — Follow an account
- **Unfollow** — Unfollow an account
- **CreatePost** — Create a new post
- **Search** — Search for content/accounts
- **JoinCommunity** — Join a community
- **LeaveCommunity** — Leave a community

### Behavior Configuration (NpcBehaviorConfig)
```csharp
public class NpcBehaviorConfig
{
    public int MaxCandidateAccounts { get; set; } = 50;
    public int MaxCandidatePosts { get; set; } = 30;
    public double BaseActionProbability { get; set; } = 0.7;
    public int PostCooldownSeconds { get; set; } = 300;
    public int MaxFollowsPerTick { get; set; } = 2;
    public int MaxLikesPerTick { get; set; } = 5;
    public int MaxCommentsPerTick { get; set; } = 3;
    public int MaxUnfollowsPerTick { get; set; } = 1;
    public int RecentPostsHours { get; set; } = 24;
    public int MaxFollowingBeforeUnfollow { get; set; } = 200;
    public bool EnableExploration { get; set; } = true;
    public double ExplorationRate { get; set; } = 0.3;
    public bool EnableCommunityBehavior { get; set; } = true;
    public int MaxCommunityJoinsPerTick { get; set; } = 1;
    public int MaxRelevantCommunities { get; set; } = 10;
}
```

### Services

#### INpcBehaviorService
Main service for NPC behavior execution:
- `ProcessBehaviorAsync(npc, config)` — Execute one simulation tick
- `GenerateCandidatesAsync(npc, config)` — Generate possible actions
- `CanFollowAsync(npcAccountId, targetAccountId)` — Validate follow action
- `CanLikeAsync(npcAccountId, postId)` — Validate like action
- `GetRecentPostsAsync(npc, limit, hours)` — Get posts for engagement
- `GetCandidateAccountsAsync(npc, limit)` — Get accounts for following

#### INpcDecisionService
Decision-making service:
- `EvaluateAndSelect(npc, candidates, random)` — Select best action
- `GetPersonalityModifier(personality, actionType)` — Personality influence
- `GetAccountTypeModifier(accountType, actionType)` — Account type influence
- `CalculateFinalScore(candidate, personality, accountType, relevance)` — Final score

#### IContentRelevanceService
Content relevance scoring:
- `CalculatePostRelevance(post, interests)` — Score post relevance
- `CalculateAccountRelevance(account, interests)` — Score account relevance
- `ExtractTopics(content)` — Extract interest topics from text

#### IContentGeneratorService
Template-based content generation (placeholder for LLM):
- `GeneratePostContent(npc, random)` — Generate post text
- `GenerateCommentContent(npc, post, random)` — Generate comment text

### Personality Influence (Big Five)

| Trait | Effect |
|-------|--------|
| Openness | Increases exploration, commenting, following |
| Conscientiousness | Increases consistent posting |
| Extraversion | Increases social actions (following) |
| Agreeableness | Increases positive engagement (likes, comments) |
| Neuroticism | Decreases engagement (cautious behavior) |

### Account Type Influence

| Type | Posting | Following | Engagement |
|------|---------|-----------|------------|
| OrdinaryUser | Low (15%) | Moderate | Balanced |
| Creator | High (40%) | Moderate | High |
| Influencer | High (45%) | Low | Very High |
| Celebrity | Moderate (35%) | Very Low | Low |
| Official | High (40%) | Low | Low |
| News | Very High (50%) | Moderate | Low |

### Social Graph Rules
NPCs respect the same rules as human users:
- Cannot follow self
- Cannot follow accounts that block them
- Cannot follow accounts they block
- Cannot interact with content from blocked accounts
- Cannot like posts already liked
- Cannot follow accounts already following

### Content Relevance
Deterministic keyword-based relevance using interest categories:
- Gaming, Sports, Technology, Music, Movies, Television, Fashion, Food, Travel, Science, Health, Business, Finance, Education, LocalNews, WorldNews, Entertainment, GamingNews, SportsNews, TechNews

### Activity States
NPCs transition through states based on actions:
- **Idle** — Not currently acting
- **Browsing** — Viewing content
- **Reading** — Consuming posts
- **Posting** — Creating content
- **Engaging** — Social interactions
- **Offline** — Not simulated

### NPC Action History
All NPC actions are recorded in `NpcAction`:
- Action type and target
- Scheduled vs executed timestamps
- Success/failure status
- Content for posts/comments

### Tests

#### NpcDecisionServiceTests (15 tests)
- Personality modifiers for all Big Five traits
- Account type modifiers
- Score calculation and clamping
- Deterministic selection with seed
- Candidate evaluation

#### ContentRelevanceServiceTests (8 tests)
- Topic extraction from text
- Post relevance calculation
- Account relevance calculation
- Interest strength effects

#### ContentGeneratorServiceTests (6 tests)
- Template-based generation
- Account type-specific content
- Deterministic generation with seed
- Comment type selection

#### NpcBehaviorIntegrationTests (14 tests)
- Candidate generation
- Follow/like/comment execution
- Block rule enforcement
- Action recording
- Edge case handling

#### NpcBehaviorPerformanceTests (3 tests)
- 100 NPC processing performance
- Candidate generation performance
- Content relevance calculation performance

## AI Content Generation

### Overview
NPC-generated posts and comments can be powered by AI text generation instead of the template-based system from Part 10. The system is **provider-agnostic** — any OpenAI-compatible API can be used. Configuration is stored in the database for **runtime reconfiguration without server restart**.

### Architecture Change

```
NpcBehaviorService
      ↓
IContentGeneratorService (interface)
      ↓
AiContentGeneratorService (Part 13)
      ├── AI enabled → IAiTextGenerationService → Provider (OpenAI/Anthropic/Generic)
      └── AI disabled/error → ContentGeneratorService (Part 10 templates)
```

### Provider-Agnostic Design
```
Application Layer
      └── IAiTextGenerationService (abstraction)
              ↓
Infrastructure Layer
      ├── OpenAiProvider (OpenAI API)
      ├── AnthropicProvider (Anthropic API)
      └── GenericHttpProvider (OpenAI-compatible: DeepSeek, Ollama, etc.)
```

### Supported Providers

| Provider | Auth Header | Notes |
|----------|-------------|-------|
| OpenAI | `Authorization: Bearer <key>` | Default endpoint: `api.openai.com/v1` |
| Anthropic | `x-api-key: <key>` | Default endpoint: `api.anthropic.com/v1` |
| Generic | `Authorization: Bearer <key>` | Requires `BaseUrl` for custom endpoints |

### Configuration at Runtime
Configuration is stored in SQLite (`AiProviderConfigs` table), allowing changes without server restart:

| Field | Description |
|-------|-------------|
| Provider | "OpenAI", "Anthropic", or "Generic" |
| Model | Model identifier (e.g., "gpt-4o", "claude-3-5-sonnet-20241022") |
| ApiKey | API key (stored plaintext — see Security) |
| BaseUrl | Required for Generic provider (e.g., `https://api.deepseek.com`) |
| IsEnabled | Toggle AI on/off without removing configuration |
| TimeoutSeconds | API timeout (5-120 seconds, default 30) |

### Admin Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/admin/ai/config` | GET | Yes | Get current config (key masked) |
| `/api/admin/ai/config` | PUT | Yes | Update provider/model/key |
| `/api/admin/ai/test` | POST | Yes | Test connection with simple prompt |

#### GET /api/admin/ai/config Response
```json
{
  "provider": "OpenAI",
  "model": "gpt-4o",
  "hasApiKey": true,
  "apiKeyMasked": "****2345",
  "baseUrl": null,
  "isEnabled": true,
  "timeoutSeconds": 30,
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

#### PUT /api/admin/ai/config Request
```json
{
  "provider": "OpenAI",
  "model": "gpt-4o",
  "apiKey": "sk-your-key-here",
  "baseUrl": null,
  "isEnabled": true,
  "timeoutSeconds": 30
}
```

### API Key Security

> **⚠️ Security Note:** The API key is stored **plaintext** in the SQLite database. This is appropriate for local development and self-hosted deployments, but production environments should consider:
> - Restricting database file access
> - Implementing encryption at rest
> - Using a secrets management service

**Never exposed:**
- API key never appears in GET responses (only masked form)
- API key never appears in logs
- Error messages are sanitized to remove key patterns

### Fallback Behavior
If AI generation fails, the system **automatically falls back** to template-based content:

| Failure Scenario | Behavior |
|------------------|----------|
| No provider configured | Use templates |
| AI disabled via `IsEnabled=false` | Use templates |
| Provider call times out | Use templates |
| Provider returns error | Use templates |
| Network error | Use templates |

The NPC's tick continues normally — no NPC is skipped due to AI failure.

### Prompt Construction
`AiPromptBuilder` constructs prompts including:

- **Account type system prompt**: Different guidance for OrdinaryUser vs Celebrity vs News vs Creator, etc.
- **Personality context**: Big Five traits influence tone (e.g., "You are very outgoing", "You are more reserved")
- **Interest context**: Top 3 interests included
- **Content constraints**: Length, format, emoji guidance appropriate for social media

### Observability
Extended simulation status includes AI metrics:

```json
{
  "totalAiAttempts": 450,
  "totalAiSuccesses": 445,
  "totalAiFallbacks": 5,
  "lastAiError": "Request timed out",
  "aiProvider": "OpenAI",
  "aiModel": "gpt-4o",
  "isAiEnabled": true
}
```

### Performance Considerations
- AI calls have a 10-second timeout (configurable via provider config)
- Slow AI calls don't block other NPCs — each NPC's generation runs independently
- Template fallback ensures tick continues even if AI is slow
- HTTP client uses named client factory for connection pooling

### Intentionally Not Implemented
- Image/video generation
- Embeddings/vector search
- Full secrets vault or external key management
- Android UI for AI configuration
- Streaming responses

## Notifications System

### Overview
Notifications surface engagement generated by NPCs and other players directly to the account it happened to. Every activity from the NPC simulation loop (Parts 08-13) now creates notifications when relevant.

### Architecture
```
SocialGraphService / PostService
      ↓
IContentGeneratorService (interface)
      ↓
AiContentGeneratorService (Part 13)
      ├── AI enabled → IAiTextGenerationService → Provider (OpenAI/Anthropic/Generic)
      └── AI disabled/error → ContentGeneratorService (Part 10 templates)
```

### Single-Mechanism Notification Creation
Notifications are created through a single consistent mechanism (`INotificationService`) wired into the existing `SocialGraphService` and `PostService`. Since NPC actions already go through the same shared services as player actions, both NPC-caused and player-caused events are covered by one code path.

```
Action occurs (Follow/Like/Comment)
      ↓
SocialGraphService.FollowAsync() / PostService.LikePostAsync() / PostService.CreateCommentAsync()
      ↓
NotificationService.NotifyFollowAsync() / NotifyLikeAsync() / NotifyCommentAsync()
      ↓
Notification persisted
```

### Notification Types

| Type | Trigger | Recipient | Related Data |
|------|---------|-----------|---------------|
| Follow | Account follows another | Followed account | Follow entity ID |
| Like | Account likes a post | Post author | PostLike entity ID, Post ID |
| Comment | Account comments on a post | Post author | Comment entity ID, Post ID |

### Suppression Rules

#### Self-Notification Suppression
No notification is created when the actor and recipient are the same account. This includes:
- Liking your own post (if possible under existing rules)
- Commenting on your own post

#### Block Suppression
If there's a block relationship in either direction between the actor and recipient, no notification is created:
- Actor blocked recipient → no notification
- Recipient blocked actor → no notification

This is consistent with existing block-check logic in `SocialGraphService`.

#### Mute Suppression
If the recipient has muted the actor, no notification is created.

**Rationale:** Mutes already suppress content visibility in the feed (`FeedService`, Part 07). Notifications should behave consistently — if you've muted someone, you don't want to be notified about their actions either.

### Deleted-Content Handling
If the underlying post is soft-deleted after a notification was created:
- The notification itself is NOT deleted (permanent data rule)
- The `RelatedPost` navigation is `null` (soft-deleted posts aren't loaded)
- API responses include `isPostDeleted: true` flag
- Client can still see the notification existed, who caused it, and when

### API Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/notifications` | GET | Yes | Get paginated notifications (newest first) |
| `/api/notifications/unread-count` | GET | Yes | Get unread notification count |
| `/api/notifications/{id}/read` | POST | Yes | Mark single notification as read |
| `/api/notifications/read-all` | POST | Yes | Mark all notifications as read |

#### GET /api/notifications
Query parameters:
- `cursor` (optional): Pagination cursor from previous response
- `pageSize` (optional): Items per page (default 20, max 100)

Response:
```json
{
  "notifications": [
    {
      "notificationId": "guid",
      "type": "Like",
      "actorAccountId": "guid",
      "actorUsername": "liker123",
      "actorDisplayName": "Liker User",
      "actorAvatarUrl": "https://...",
      "relatedPostId": "guid or null",
      "relatedPostSnippet": "First 100 chars of post...",
      "isPostDeleted": false,
      "createdAt": "2024-01-15T10:30:00Z",
      "isRead": false,
      "readAt": null
    }
  ],
  "nextCursor": "timestamp_guid",
  "hasMore": true
}
```

### Pagination Strategy
Cursor-based pagination consistent with `FeedService` (Part 07):
- Format: `"{timestamp}_{notificationId}"`
- Deterministic ordering: CreatedAt DESC, then Id DESC
- No duplicates, no skipped items across pages

### Database Schema

#### Notifications Table
| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key (stable identity) |
| RecipientAccountId | int | FK to Account |
| ActorAccountId | int | FK to Account (actor who caused it) |
| Type | int | Enum: Follow=0, Like=1, Comment=2 |
| RelatedEntityId | int | Follow/PostLike/Comment ID |
| RelatedPostId | int? | Post ID (for Like/Comment) |
| CreatedAt | datetime | When notification was created |
| IsRead | bool | Read/unread state |
| ReadAt | datetime? | When notification was read |

#### Indexes
- `(RecipientAccountId, CreatedAt DESC)` — Notification feed query
- `(RecipientAccountId, IsRead)` — Unread count query

### Failure Isolation

**Notification creation is fire-and-forget:** The `SocialGraphService` and `PostService` spawn a background task for notification creation. Failures are:
- Logged by `NotificationService`
- Do NOT prevent the triggering action (Follow/Like/Comment) from succeeding
- Do NOT crash the NPC tick loop (Part 11)

This ensures the core simulation loop is never blocked by notification failures.

### Tick-Loop Performance Impact

The fire-and-forget pattern means:
- Notification writes don't block the tick loop
- Each notification runs in its own background task
- The tick completes before notifications are processed

**Expected impact:** Negligible. A SQLite write typically takes <10ms; the tick loop doesn't wait for it.

### Intentionally Not Implemented
- Push notifications (FCM/APNs) — server-side pull only
- Real-time delivery (WebSockets/SignalR) — client polls
- Notification preferences/settings beyond existing mute/block
- Email/SMS notifications
- Grouped/bundled notifications ("Alice and 12 others liked your post")
- Mentions/hashtags

## NPC Social Graph

### Overview
NPCs make intelligent follow/unfollow decisions based on interests, personality, engagement history, and reciprocity. The social graph emerges organically from these decisions.

### Architecture
```
NpcBehaviorService
      ↓
NpcSocialGraphService (Part 12)
      ├── Interest-based candidates
      ├── Reciprocity candidates
      ├── Exploration candidates
      └── Engagement-based candidates
      ↓
SocialGraphService (existing Part 05)
      ↓
Follow Entity (persisted)
```

### Candidate Selection Strategy
The `GetFollowCandidatesAsync` method produces bounded candidates from four sources:

| Source | Weight | Description |
|--------|--------|-------------|
| Interest-based | 50% | Accounts posting about matching interests |
| Reciprocity | 30% | Accounts that follow the NPC but aren't followed back |
| Engagement | Variable | Accounts whose posts the NPC has liked/commented |
| Exploration | 10-30% | Random active accounts (driven by Openness trait) |

**Bounds:** Max 50 interest candidates, 17 reciprocity, 12 engagement, variable exploration

### Personality Influence on Following

| Trait | Effect |
|-------|--------|
| Extraversion | +0.1 + bonus if >0.6 |
| Openness | +0.1 |
| Neuroticism | -0.1 |
| Agreeableness >0.6 | +0.05 |

### Personality Influence on Unfollowing

| Trait | Effect |
|-------|--------|
| Neuroticism | +0.05 (more churn) |
| Conscientiousness | +0.02 (prunes stale follows) |

### Account Type Influence on Following

| Type | Follow Modifier | Notes |
|------|-----------------|-------|
| OrdinaryUser | +0.25 | Follows more |
| Creator | +0.20 | Follows within niche |
| Influencer | +0.12 | Lower follow rate |
| Celebrity | -0.20 | Rarely follows back |
| Official | +0.15 | Follows for relevance |
| News | +0.20 | Follows for sourcing |

### Reciprocity (Follow-Back)
When account A follows NPC B:
- B's future ticks have increased chance to follow A back
- Score = 0.2 base + 0.4×Agreeableness + 0.2×Extraversion + 0.1×Openness
- Celebrity/Influencer types have penalty (-0.3/-0.2)
- Ordinary users have bonus (+0.1)

### Unfollow Rules
NPCs unfollow accounts when:
1. **Stale content:** Followed account hasn't posted in 48+ hours (+0.3)
2. **Low engagement:** Followed account posts infrequently (+0.1)
3. **Personality-driven churn:** High Neuroticism (+0.2×Neuroticism)
4. **Conscientious pruning:** Long follows with low engagement (+0.1 if 90+ days)

### Social Graph Rule Compliance
All NPC-to-NPC follows go through existing `SocialGraphService`:
- Cannot follow self
- Cannot follow if blocked in either direction
- Cannot follow if already following
- Mutes/blocks respected

### Observability
Extended status endpoint includes:

```json
{
  "totalNpcFollows": 1250,
  "totalNpcUnfollows": 87,
  "lastTickFollows": 12,
  "lastTickUnfollows": 3
}
```

### Performance
- Bounded queries: Max 50-100 candidates per NPC per tick
- Database indexes: Uses existing indexes on Follow, Post, Account tables
- No full-table scans: All queries are targeted by interest, engagement, or recency

## NPC Background Simulation

### Overview
The NPC simulation runs automatically in the background as a hosted service. NPCs are continuously processed without requiring external triggers or client requests.

### Background Service Architecture
```
Server Startup
      ↓
NpcSimulationHostedService.StartAsync()
      ↓
Continuously: Check CanStartTick → Execute Tick → Wait Interval
      ↓
Server Shutdown
      ↓
NpcSimulationHostedService.StopAsync() (graceful)
```

### Configuration
```json
{
  "Simulation": {
    "Enabled": true,
    "TickIntervalSeconds": 10,
    "MaxNpcsPerTick": 100,
    "DetailedLogging": false
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| Enabled | true | Enable/disable background simulation |
| TickIntervalSeconds | 10 | Time between ticks (1-3600 seconds) |
| MaxNpcsPerTick | 100 | Max NPCs processed per tick |
| DetailedLogging | false | Enable per-NPC logging |

### Overlap Prevention
- Ticks are atomic — a new tick cannot start while a previous one is running
- If a tick takes longer than the interval, the next scheduled tick is skipped
- Skipped ticks are tracked in statistics

### Failure Isolation
- Individual tick failures are caught and logged
- The service continues scheduling subsequent ticks
- A single bad NPC does not crash the simulation

### Graceful Shutdown
1. Service observes CancellationToken
2. If a tick is in progress, waits up to 30 seconds for completion
3. After timeout, proceeds with shutdown (no half-written state)
4. Server exits cleanly

### State Management
- SimulationStateService tracks: running/paused state, tick counts, last tick time, durations
- State is in-memory (resets on server restart)
- State persists across ticks within a server session

### Admin Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/admin/simulation/status` | GET | Yes | Get simulation status |
| `/api/admin/simulation/pause` | POST | Yes | Pause simulation |
| `/api/admin/simulation/resume` | POST | Yes | Resume simulation |

#### Status Response
```json
{
  "isRunning": true,
  "isPaused": false,
  "isEnabled": true,
  "tickIntervalSeconds": 10,
  "maxNpcsPerTick": 100,
  "totalTicks": 42,
  "totalNpcsProcessed": 1260,
  "totalTicksSkipped": 2,
  "totalTicksFailed": 0,
  "lastTickAt": "2024-01-15T10:30:00Z",
  "lastTickDurationMs": 150.5,
  "lastTickNpcsProcessed": 30,
  "serviceStartedAt": "2024-01-15T08:00:00Z",
  "isTickInProgress": false,
  "currentTickStartedAt": null
}
```

### API Responsiveness
- The background service uses scoped DbContext per tick
- Each tick processes NPCs independently
- API requests are not blocked by simulation ticks
- Connection pool is properly managed with short-lived scopes

### Persistence Test
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/health` | GET | No | Health check |
| `/api/persistence-test` | POST/GET | No | Persistence test endpoints |

### Pagination
Followers and following endpoints support pagination:
```
/api/accounts/{id}/followers?page=1
/api/accounts/{id}/following?page=1
```

Response includes:
- `accounts` — Array of account summaries
- `page` — Current page
- `pageSize` — Items per page (default 20)
- `totalCount` — Total items
- `totalPages` — Total pages

### Relationship Response
```json
{
  "accountId": "guid",
  "isFollowing": true,
  "isFollowedBy": false,
  "isMutual": false,
  "isBlocking": false,
  "isBlockedBy": false,
  "isMuting": false
}
```

## Technology Stack

- **Client:** C# / .NET MAUI (Android/iOS/MacCatalyst)
- **Server:** C# / ASP.NET Core 10.0
- **Database:** SQLite via Entity Framework Core
- **Authentication:** JWT Bearer / PBKDF2
- **Future AI:** Ollama + Qwen

## Database Schema

### Accounts
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| AccountId | TEXT | UNIQUE (GUID) |
| Username | TEXT | max 50 |
| UsernameNormalized | TEXT | UNIQUE, max 50 |
| PasswordHash | TEXT | PBKDF2 |
| Email | TEXT | max 255, nullable |
| AccountType | INTEGER | enum |
| Status | INTEGER | enum |
| CreatedAt | TEXT | datetime |
| UpdatedAt | TEXT | datetime |

### Profiles
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| AccountId | INTEGER | FK → Accounts, UNIQUE |
| DisplayName | TEXT | max 100 |
| Bio | TEXT | max 500, nullable |
| AvatarUrl | TEXT | max 500, nullable |

### AccountHistory
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| AccountId | INTEGER | FK → Accounts |
| EventType | INTEGER | enum |
| Details | TEXT | max 1000, nullable |
| CreatedAt | TEXT | datetime |

### Follows
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| FollowerAccountId | INTEGER | FK → Accounts |
| FollowedAccountId | INTEGER | FK → Accounts |
| CreatedAt | TEXT | datetime |
| **UNIQUE** | | (FollowerAccountId, FollowedAccountId) |

### Blocks
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| BlockerAccountId | INTEGER | FK → Accounts |
| BlockedAccountId | INTEGER | FK → Accounts |
| CreatedAt | TEXT | datetime |
| **UNIQUE** | | (BlockerAccountId, BlockedAccountId) |

### Mutes
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| MuterAccountId | INTEGER | FK → Accounts |
| MutedAccountId | INTEGER | FK → Accounts |
| CreatedAt | TEXT | datetime |
| **UNIQUE** | | (MuterAccountId, MutedAccountId) |

### PersistenceTests
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| Value | TEXT | max 500 |
| CreatedAt | TEXT | datetime |

### Posts
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| PostId | TEXT | UNIQUE (GUID) |
| AuthorAccountId | INTEGER | FK → Accounts |
| Content | TEXT | max 10000 |
| Status | INTEGER | enum |
| CreatedAt | TEXT | datetime |
| UpdatedAt | TEXT | datetime |

### PostLikes
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| PostId | INTEGER | FK → Posts |
| AccountId | INTEGER | FK → Accounts |
| CreatedAt | TEXT | datetime |
| **UNIQUE** | | (PostId, AccountId) |

### Comments
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, AUTOINCREMENT |
| CommentId | TEXT | UNIQUE (GUID) |
| PostId | INTEGER | FK → Posts |
| AuthorAccountId | INTEGER | FK → Accounts |
| Content | TEXT | max 2000 |
| Status | INTEGER | enum |
| CreatedAt | TEXT | datetime |
| UpdatedAt | TEXT | datetime |

## Configuration

**Server URL (Android Emulator):** `http://10.0.2.2:5225`

**Database Location:** `D:\SMS\Database\sms.db`

**JWT Configuration:**
- SecretKey in `appsettings.json`
- 7-day token expiration
- HMAC SHA256 signing

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Android SDK (API 35+)
- Java JDK 21+

### Build

```bash
dotnet build
```

### Run Server

```bash
cd Server
dotnet run
```

### Test Registration

```powershell
Invoke-RestMethod http://localhost:5225/api/auth/register -Method POST -Body (@{username="test";password="Password123!"} | ConvertTo-Json) -ContentType "application/json"
```

### Test Login

```powershell
Invoke-RestMethod http://localhost:5225/api/auth/login -Method POST -Body (@{username="test";password="Password123!"} | ConvertTo-Json) -ContentType "application/json"
```

### Test Follow

```powershell
# After login, use token to follow
$token = "your-jwt-token"
$targetId = "target-account-guid"
Invoke-RestMethod "http://localhost:5225/api/accounts/$targetId/follow" -Method POST -Headers @{"Authorization"="Bearer $token"}
```

### Test Create Post

```powershell
$token = "your-jwt-token"
Invoke-RestMethod http://localhost:5225/api/posts -Method POST -Body (@{content="Hello world!"} | ConvertTo-Json) -ContentType "application/json" -Headers @{"Authorization"="Bearer $token"}
```

### Test Like Post

```powershell
$token = "your-jwt-token"
$postId = "post-guid"
Invoke-RestMethod "http://localhost:5225/api/posts/$postId/like" -Method POST -Headers @{"Authorization"="Bearer $token"}
```

### Test Add Comment

```powershell
$token = "your-jwt-token"
$postId = "post-guid"
Invoke-RestMethod "http://localhost:5225/api/posts/$postId/comments" -Method POST -Body (@{content="Great post!"} | ConvertTo-Json) -ContentType "application/json" -Headers @{"Authorization"="Bearer $token"}
```

### Test Get Feed

```powershell
$token = "your-jwt-token"
Invoke-RestMethod "http://localhost:5225/api/feed" -Headers @{"Authorization"="Bearer $token"}
```

### Test Feed Pagination

```powershell
$token = "your-jwt-token"
# First page
$feed1 = Invoke-RestMethod "http://localhost:5225/api/feed?pageSize=5" -Headers @{"Authorization"="Bearer $token"}
# Next page using cursor
$cursor = $feed1.nextCursor
$feed2 = Invoke-RestMethod "http://localhost:5225/api/feed?cursor=$cursor&pageSize=5" -Headers @{"Authorization"="Bearer $token"}
```

## Verification Results

| Test | Result |
|------|--------|
| Server build | PASS |
| Health endpoint | PASS |
| Account registration | PASS |
| Account login | PASS |
| Duplicate username rejection | PASS |
| Authenticated /me endpoint | PASS |
| Account persistence (restart) | PASS |
| Follow account | PASS |
| Unfollow account | PASS |
| Get followers (paginated) | PASS |
| Get following (paginated) | PASS |
| Mutual follow detection | PASS |
| Self-follow rejection | PASS |
| Block account | PASS |
| Block removes conflicting follows | PASS |
| Blocked user cannot follow | PASS |
| Unblock account | PASS |
| Mute account | PASS |
| Mute does NOT remove follow | PASS |
| Unmute account | PASS |
| Relationship query | PASS |
| Graph persistence (restart) | PASS |
| Database schema | PASS |
| Create post (authenticated) | PASS |
| Create post (unauthenticated rejection) | PASS |
| Get post by ID | PASS |
| Delete post (owner only) | PASS |
| Delete post (not owner rejection) | PASS |
| Like post | PASS |
| Like post (duplicate rejection) | PASS |
| Unlike post | PASS |
| Unlike post (idempotent) | PASS |
| Get comments (public) | PASS |
| Create comment | PASS |
| Delete comment (owner only) | PASS |
| Delete comment (not owner rejection) | PASS |
| Post validation (empty content) | PASS |
| Pagination in comments | PASS |
| Post persistence (restart) | PASS |
| Database schema (Posts/Comments) | PASS |
| Feed endpoint (unauthenticated rejection) | PASS |
| Feed endpoint (authenticated) | PASS |
| Feed empty for new user | PASS |
| Feed shows followed posts | PASS |
| Feed excludes non-followed posts | PASS |
| Feed excludes muted accounts | PASS |
| Feed excludes blocked accounts | PASS |
| Feed excludes reverse-blocked posts | PASS |
| Feed excludes deleted posts | PASS |
| Feed like count | PASS |
| Feed comment count | PASS |
| Feed IsLikedByCurrentUser | PASS |
| Feed ordering (newest first) | PASS |
| Feed pagination (no duplicates) | PASS |
| Feed cursor pagination | PASS |
| Feed persistence (restart) | PASS |
| NPC creation (account/profile/NPC) | PASS |
| NPC simulation interval by type | PASS |
| NPC retrieval with related data | PASS |
| NPC identification (by ID/GUID) | PASS |
| NPC activation/deactivation | PASS |
| NPC deterministic personality | PASS |
| NPC interest generation | PASS |
| NPC username collision handling | PASS |
| NPC due filtering | PASS |
| NPC inactive exclusion | PASS |
| NPC state updates | PASS |
| NPC batch processing | PASS |
| NPC account status respect | PASS |
| NPC activity state management | PASS |
| Population generation (1 NPC) | PASS |
| Population generation (10 NPCs) | PASS |
| Population generation (100 NPCs) | PASS |
| Population generation (1000 NPCs) | PASS |
| Population config validation | PASS |
| Population duplicate prevention | PASS |
| Population deterministic seed | PASS |
| Population account type distribution | PASS |
| Population username uniqueness | PASS |
| Population personality validity | PASS |
| Population interest validity | PASS |
| Username generator uniqueness | PASS |
| Username generator determinism | PASS |
| Profile generator by type | PASS |
| Distribution validation | PASS |
| NPC personality modifiers (Big Five) | PASS |
| NPC account type modifiers | PASS |
| NPC score calculation | PASS |
| NPC deterministic selection | PASS |
| NPC candidate evaluation | PASS |
| Content relevance calculation | PASS |
| Topic extraction | PASS |
| Interest strength effects | PASS |
| Template-based content generation | PASS |
| NPC follow/like/comment execution | PASS |
| Block rule enforcement | PASS |
| NPC action recording | PASS |
| NPC candidate generation | PASS |
| NPC 100 NPC processing performance | PASS |
| Simulation state service initialization | PASS |
| Simulation pause/resume | PASS |
| Simulation overlap prevention | PASS |
| Simulation tick lifecycle | PASS |
| Simulation failure isolation | PASS |
| Simulation disabled state | PASS |
| NPC social graph candidates - exclude following | PASS |
| NPC social graph candidates - exclude blocked | PASS |
| NPC social graph candidates - exclude self | PASS |
| NPC social graph - interest-based candidates | PASS |
| NPC social graph - reciprocity candidates | PASS |
| NPC social graph - unfollow stale follows | PASS |
| NPC social graph - reciprocity score by agreeableness | PASS |
| NPC social graph - reciprocity score by account type | PASS |
| Simulation status social graph metrics | PASS |
| Simulation tick result social graph data | PASS |
| AI content - disabled uses template | PASS |
| AI content - enabled uses AI provider | PASS |
| AI content - fallback on failure | PASS |
| AI config - empty returns no config | PASS |
| AI config - valid OpenAI config stored | PASS |
| AI config - invalid provider rejected | PASS |
| AI config - Generic requires BaseUrl | PASS |
| AI config - masked API key in response | PASS |
| AI providers validation | PASS |
| AI prompt builder - personality context | PASS |
| AI prompt builder - comment includes post | PASS |
| Notification - follow creates notification | PASS |
| Notification - like creates notification | PASS |
| Notification - comment creates notification | PASS |
| Notification - self-notification suppressed | PASS |
| Notification - block suppression | PASS |
| Notification - mute suppression | PASS |
| Notification - unread count | PASS |
| Notification - mark as read | PASS |
| Notification - mark all as read | PASS |
| Notification - pagination newest first | PASS |
| Notification - deleted post handling | PASS |
| Notification - NPC attribution | PASS |
| Notification - integration with SocialGraphService | PASS |
| Notification - ownership verification | PASS |
| Community - create community | PASS |
| Community - get by slug | PASS |
| Community - join community | PASS |
| Community - duplicate join prevention | PASS |
| Community - leave community | PASS |
| Community - owner cannot leave | PASS |
| Community - public communities only | PASS |
| Community - search by name | PASS |
| Community - search by topic | PASS |
| Community - community feed | PASS |
| Community - account communities | PASS |
| Community - is member (owner) | PASS |
| Community - NPC relevant communities | PASS |
| Community - member role (owner) | PASS |
| Community - member role (member) | PASS |
| Community seed - creates communities | PASS |
| Community seed - no duplicates | PASS |
| Community seed - valid topics | PASS |

## License

To be determined.
