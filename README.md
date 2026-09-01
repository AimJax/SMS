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

**NEXT: PART 04 — ACCOUNTS & AUTHENTICATION**

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
│   ├── Controllers/       API endpoints
│   └── Middleware/       Exception handling
├── Application/
│   └── Services/         Business logic interfaces & implementations
├── Domain/
│   └── Entities/         Domain models
├── Infrastructure/
│   └── Persistence/     EF Core DbContext, Entity configurations
├── Contracts/
│   ├── Requests/        API request DTOs
│   └── Responses/       API response DTOs
├── Extensions/           DI registration extensions
└── Program.cs
```

### Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **API** | HTTP routing, request handling, error responses |
| **Application** | Business logic orchestration, service interfaces |
| **Domain** | Entity definitions, domain rules |
| **Infrastructure** | Database access, EF Core configurations |
| **Contracts** | API request/response DTOs |

## Technology Stack

- **Client:** C# / .NET MAUI (Android/iOS/MacCatalyst)
- **Server:** C# / ASP.NET Core 10.0
- **Database:** SQLite via Entity Framework Core
- **Future AI:** Ollama + Qwen

## Repository Structure

```
├── Client/               Android application (.NET MAUI)
│   ├── Configuration/    App configuration (server URL)
│   ├── Models/           Data models
│   └── Services/         API services (ApiService)
├── Server/               ASP.NET Core backend
│   ├── API/              Controllers, middleware
│   ├── Application/       Service interfaces & implementations
│   ├── Domain/           Entity definitions
│   ├── Infrastructure/    Persistence (EF Core, Entity configurations)
│   ├── Contracts/        Request/response DTOs
│   └── Extensions/        DI registration
├── Shared/               Shared contracts and models
├── Database/             Database files (sms.db)
├── Tests/               Automated tests
├── Documentation/       Project documentation
├── .gitignore
├── README.md
└── SocialMediaSimulator.slnx
```

## Persistence Architecture

### SQLite Configuration
- **WAL Mode:** Enabled (Write-Ahead Logging for concurrency)
- **Foreign Keys:** Enabled
- **Busy Timeout:** 5 seconds
- **Synchronous:** NORMAL

### Entity Framework Core
- **DbContext:** `AppDbContext`
- **Connection:** Configured via `appsettings.json`
- **Entity Configuration:** Separate configuration classes per entity

### Unit of Work
- **Interface:** `IUnitOfWork`
- **Implementation:** `UnitOfWork`
- **Purpose:** Transaction management for multi-entity operations

### Current Database Schema
```
PersistenceTests
├── Id (INTEGER, PK, AUTOINCREMENT)
├── Value (TEXT, NOT NULL, max 500)
└── CreatedAt (TEXT, NOT NULL, indexed)
```

## Current Implementation

### Server Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/health` | GET | Returns `{"status": "ok"}` |
| `/api/persistence-test` | POST | Create test record |
| `/api/persistence-test/{id}` | GET | Get test record by ID |
| `/api/persistence-test` | GET | List all test records |

### Client Features

- Server connectivity check via `/api/health`
- Visual status indicator (ONLINE/OFFLINE)
- Error handling with user-friendly messages
- Loading indicator during requests
- Manual "Check Server" button

### Configuration

**Server URL (Android Emulator):** `http://10.0.2.2:5225`  
*(The Android emulator uses 10.0.2.2 to reach the host machine's localhost)*

**Server URL (Physical Device):** Use local network IP address of development machine

**Database Location:** `D:\SMS\Database\sms.db`

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Android SDK (API 35+)
- Java JDK 21+
- Android emulator or device for testing

### Build

```bash
dotnet build
```

### Run Server

```bash
cd Server
dotnet run
```

Server runs on: `http://localhost:5225`

### Run Client (Android)

```bash
cd Client
dotnet build -t:Run -f net10.0-android
```

### Test Backend

```powershell
Invoke-RestMethod http://localhost:5225/api/health
# Returns: @{status=ok}
```

## Verification Results

| Test | Result |
|------|--------|
| Server build | PASS |
| Server health endpoint | PASS |
| Persistence endpoint (POST) | PASS |
| Persistence endpoint (GET) | PASS |
| Persistence after restart | PASS |
| WAL mode | PASS |
| All data preserved | PASS |
| Exception middleware | PASS |
| DI service resolution | PASS |

## License

To be determined.
