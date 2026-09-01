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

**NEXT: PART 14 — [To be determined]**

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
│   └── Entities/         Account, Profile, Follow, Block, Mute, AccountHistory, Post, PostLike, Comment
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
| `/api/feed` | GET | Yes | Get personalized feed |

**Feed Behavior:**
- Returns posts from accounts the authenticated user follows
- Excludes posts from blocked accounts (in either direction)
- Excludes posts from muted accounts
- Excludes soft-deleted posts
- Ordered by newest first
- Cursor-based pagination for scalability

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

## License

To be determined.
