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

**NEXT: PART 06 — POSTS & ENGAGEMENT**

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
│   ├── Controllers/       API endpoints (Auth, Account, Graph)
│   └── Middleware/       Exception handling
├── Application/
│   └── Services/         Business logic (AccountService, JwtService, SocialGraphService)
├── Domain/
│   └── Entities/         Account, Profile, Follow, Block, Mute, AccountHistory
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

## License

To be determined.
