# Social Media Simulator

**Persistent online social-media simulation.**

## Architecture

```
Android Client
      ↓
ASP.NET Core Server
      ↓
SQLite
```

## Technology Stack

- **Client:** C# / .NET MAUI (Android)
- **Server:** C# / ASP.NET Core
- **Database:** SQLite
- **Future AI:** Ollama + Qwen

## Repository Structure

```
├── Client/         Android application (.NET MAUI)
├── Server/         ASP.NET Core backend
├── Shared/         Shared contracts and models
├── Database/       Database tooling and migrations
├── Tests/          Automated tests
├── Documentation/  Project documentation
├── .gitignore
├── README.md
└── SocialMediaSimulator.sln
```

## Current Stage

**Part 01B — Repository Foundation**

See `Documentation/` for additional notes.

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

### Run Client (Android emulator)

```bash
cd Client
dotnet build -t:Run
```

## Configuration

Server and client configuration is managed via `appsettings.json` files and environment variables. See individual project documentation for details.

## License

To be determined.
