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

**NEXT: PART 05 — SOCIAL GRAPH**

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
│   ├── Controllers/       API endpoints (Auth, Account, PersistenceTest)
│   └── Middleware/       Exception handling
├── Application/
│   └── Services/         Business logic (AccountService, JwtService, etc.)
├── Domain/
│   └── Entities/         Account, Profile, AccountHistory, PersistenceTest
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

### Persistence Test
| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/health` | GET | No | Health check |
| `/api/persistence-test` | POST/GET | No | Persistence test endpoints |

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
| Database schema | PASS |

## License

To be determined.
