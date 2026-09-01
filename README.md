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

**NEXT: PART 09 — NPC POPULATION GENERATION**

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

### Intentionally Not Implemented

- **LLM Integration** — NPC content generation via Ollama/Qwen (future part)
- **Population Generation** — Mass NPC creation (Part 09)
- **Advanced Behavior** — Following, liking, commenting decisions
- **NPC-Specific API** — Admin endpoints for NPC management
- **Background Processing** — Hosted service for tick execution

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

## License

To be determined.
