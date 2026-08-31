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

**NEXT: PART 02 — Backend Architecture**

## Architecture

```
Android Client
      ↓
HTTP REST
      ↓
ASP.NET Core Server
      ↓
SQLite (EF Core)
```

## Technology Stack

- **Client:** C# / .NET MAUI (Android/iOS/MacCatalyst)
- **Server:** C# / ASP.NET Core 10.0
- **Database:** SQLite via Entity Framework Core
- **Future AI:** Ollama + Qwen

## Repository Structure

```
├── Client/               Android application (.NET MAUI)
│   ├── Configuration/     App configuration (server URL, etc.)
│   ├── Models/           Data models
│   └── Services/          API services (ApiService)
├── Server/               ASP.NET Core backend
│   ├── Data/             EF Core DbContext and entities
│   └── Services/         Business services
├── Shared/               Shared contracts and models
├── Database/             Database files (sms.db)
├── Tests/               Automated tests
├── Documentation/       Project documentation
├── .gitignore
├── README.md
└── SocialMediaSimulator.slnx
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
| Android build | PASS |
| Android (all targets) build | PASS |
| Server build | PASS |
| Server health endpoint | PASS |
| SQLite persistence (write) | PASS |
| SQLite persistence (read) | PASS |
| SQLite persistence (read after restart) | PASS |
| SQLite CLI verification | PASS |
| Backend restart/reconnect | PASS |

## License

To be determined.
