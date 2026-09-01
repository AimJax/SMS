# SOCIAL MEDIA SIMULATOR — PART 21 DEVELOPMENT PROMPT
## DEPLOYMENT & TESTING — Make It Runnable

You are continuing development of the **Social Media Simulator** from the existing project.

**DO NOT restart, redesign, or replace the existing architecture.**

You must inspect the current repository first and build directly on everything already implemented.

---

# ⚠️ THIS PART MAKES THE APP RUNNABLE

This part is **NOT** about adding new features. It's about **making everything work together** so you can:
- Run the server locally or on your network
- Connect your Android app to the server
- Have Ollama generate content for NPCs
- Test the full simulation end-to-end

---

# CURRENT PROJECT CHECKPOINT

Completed:

```text
01A  Development Environment         COMPLETE
01B  Repository Foundation           COMPLETE
01C  ASP.NET Core Server            COMPLETE
01D  SQLite Foundation              COMPLETE
01E  Android Client Foundation      COMPLETE
01F  Foundation Checkpoint           COMPLETE
02   Backend Architecture           COMPLETE
03   Persistence                   COMPLETE
04   Accounts & Authentication      COMPLETE
05   Social Graph                  COMPLETE
06   Posts & Engagement             COMPLETE
07   Feed & Timeline               COMPLETE
08   NPC Simulator Foundation       COMPLETE
09   NPC Population Generation      COMPLETE
10   NPC Behavior Simulation       COMPLETE
11   NPC Background Simulation      COMPLETE
12   NPC Social Graph             COMPLETE
13   AI Content Generation         COMPLETE
14   Notifications System          COMPLETE
15   Communities                  COMPLETE
16   Advanced Feed                COMPLETE
17   LLM-Driven Event System       COMPLETE
18   Event Causality & Offline Sim COMPLETE
19   Virality                    COMPLETE
20   Topics & Trends             COMPLETE
```

Latest commit:

```text
6c0f956 — Part 20: Topics and trends system
```

Remote:

```text
origin/main
```

---

# YOUR CONFIGURATION

Based on your setup:
- **Ollama URL:** `http://localhost:11434`
- **Ollama API Key:** `eb83536349244577bc482f76d21bc55f.JFqxh_kUnppKPlpmBsDZBMeG`
- **Ollama Model:** Extract from key — model name is `JFqxh_kUnppKPlpmBsDZBMeG`
- **Server Port:** `5225`
- **Server URL:** `http://localhost:5225` (local) or `http://YOUR_IP:5225` (network)
- **Android Client:** MAUI .NET app

---

# PART 21 — REQUIRED TASKS

## 1. Server Configuration — DO THIS FIRST

### 1.1 Update appsettings.json

**File:** `Server\appsettings.json`

Add Ollama configuration at the end:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "SocialMediaSimulator": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=..\\Database\\sms.db"
  },
  "Jwt": {
    "SecretKey": "SocialMediaSimulatorSecretKey2024ThisShouldBeAtLeast32Characters!",
    "Issuer": "SocialMediaSimulator",
    "Audience": "SocialMediaSimulator",
    "ExpirationDays": 7
  },
  "Simulation": {
    "Enabled": true,
    "TickIntervalSeconds": 10,
    "MaxNpcsPerTick": 100,
    "DetailedLogging": true
  },
  "AiProvider": {
    "Provider": "Generic",
    "BaseUrl": "http://localhost:11434",
    "Model": "JFqxh_kUnppKPlpmBsDZBMeG",
    "ApiKey": "eb83536349244577bc482f76d21bc55f.JFqxh_kUnppKPlpmBsDZBMeG",
    "IsEnabled": true,
    "TimeoutSeconds": 120
  },
  "FeedScoring": { ... },
  "EventSystem": { ... },
  "OfflineSimulation": { ... },
  "Virality": { ... },
  "Trends": { ... }
}
```

### 1.2 Add AI Configuration Seeding

**File:** `Server\Program.cs`

Add this seeding code AFTER database initialization:

```csharp
// Seed AI provider config on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var configService = scope.ServiceProvider.GetRequiredService<IAiProviderService>();
    
    // Check if AI config exists
    var existingConfig = await dbContext.AiProviderConfigs.FirstOrDefaultAsync();
    if (existingConfig == null)
    {
        // Create default Generic/Ollama config
        await configService.UpdateConfigAsync(new UpdateAiConfigRequest
        {
            Provider = "Generic",
            BaseUrl = builder.Configuration["AiProvider:BaseUrl"] ?? "http://localhost:11434",
            Model = builder.Configuration["AiProvider:Model"] ?? "qwen3-4b",
            ApiKey = builder.Configuration["AiProvider:ApiKey"] ?? "",
            IsEnabled = true,
            TimeoutSeconds = 120
        });
        Console.WriteLine("AI provider configuration seeded.");
    }
}
```

**Note:** If `UpdateAiConfigAsync` requires an authenticated context, create a seeder service instead.

### 1.3 Create AI Config Seeder Service

Create file: `Server\Application\Services\AiConfigSeederService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class AiConfigSeederService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AiConfigSeederService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task SeedIfNeededAsync()
    {
        var exists = await _context.AiProviderConfigs.AnyAsync();
        if (exists) return;

        var config = new Domain.Entities.AiProviderConfig
        {
            Provider = _configuration["AiProvider:Provider"] ?? "Generic",
            BaseUrl = _configuration["AiProvider:BaseUrl"] ?? "http://localhost:11434",
            Model = _configuration["AiProvider:Model"] ?? "qwen3-4b",
            ApiKey = _configuration["AiProvider:ApiKey"] ?? "",
            IsEnabled = true,
            TimeoutSeconds = 120,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AiProviderConfigs.Add(config);
        await _context.SaveChangesAsync();
        Console.WriteLine($"AI provider seeded: {config.Provider} / {config.Model}");
    }
}
```

### 1.4 Register the Seeder

**File:** `Server\Extensions\ServiceCollectionExtensions.cs` or wherever services are registered

Add:
```csharp
builder.Services.AddScoped<AiConfigSeederService>();
```

### 1.5 Call the Seeder in Program.cs

```csharp
// Seed AI provider config
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AiConfigSeederService>();
    await seeder.SeedIfNeededAsync();
}
```

---

## 2. Server Startup

### 2.1 Build Server

```bash
cd Server
dotnet build
```

### 2.2 Run Server

```bash
dotnet run
```

You should see:
```
Now listening on: http://0.0.0.0:5225
Database initialized successfully.
AI provider seeded: Generic / JFqxh_kUnppKPlpmBsDZBMeG
```

### 2.3 Test Server Endpoints

Open browser or use curl:

```bash
# Health check
curl http://localhost:5225/api/health

# Expected: {"status":"ok"}
```

---

## 3. Test Ollama Connection

### 3.1 From Browser

```
http://localhost:5225/api/ai/config
```

### 3.2 From Terminal

```bash
curl -X POST http://localhost:11434/chat/completions \
  -H "Content-Type: application/json" \
  -d '{
    "model": "JFqxh_kUnppKPlpmBsDZBMeG",
    "messages": [{"role": "user", "content": "Say hi"}],
    "stream": false
  }'
```

---

## 4. Android Client Configuration

### 4.1 Update Server URL

**File:** `Client\Configuration\AppConfig.cs`

Change for physical device testing (your computer's IP):

```csharp
namespace SocialMediaSimulator.Client.Configuration;

public class AppConfig
{
    /// <summary>
    /// Base URL of the API server.
    /// For Android emulator use: http://10.0.2.2:5225
    /// For physical device on same network: http://YOUR_COMPUTER_IP:5225
    /// </summary>
    public string ApiBaseUrl { get; set; } = "http://10.0.2.2:5225";
}
```

### 4.2 Allow HTTP Traffic (Android 9+)

**File:** `Client\Platforms\Android\AndroidManifest.xml`

Add `usesCleartextTraffic`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
	<application 
		android:allowBackup="true" 
		android:icon="@mipmap/appicon" 
		android:roundIcon="@mipmap/appicon_round" 
		android:supportsRtl="true"
		android:usesCleartextTraffic="true">
	</application>
	<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
	<uses-permission android:name="android.permission.INTERNET" />
</manifest>
```

### 4.3 Build Android APK

```bash
cd Client
dotnet build -f net10.0-android -c Release
```

### 4.4 Install on Device

```bash
adb install -r bin\Release\net10.0-android\com.companyname.socialmediasimulator-Signed.apk
```

Or open in Visual Studio and run.

---

## 5. Network Configuration for Remote Testing

### 5.1 Find Your Computer's IP

**Windows:**
```bash
ipconfig
```
Look for `IPv4 Address` under Wi-Fi or Ethernet adapter (e.g., `192.168.1.100`)

### 5.2 Update Android Config

```csharp
public string ApiBaseUrl { get; set; } = "http://192.168.1.100:5225";
```

### 5.3 Allow Firewall (if needed)

```powershell
# Run as Administrator
New-NetFirewallRule -DisplayName "SMS Server" -Direction Inbound -Protocol TCP -LocalPort 5225 -Action Allow
```

---

## 6. Testing Checklist

### 6.1 Server Tests

- [ ] `dotnet build` succeeds
- [ ] `dotnet run` starts without errors
- [ ] `curl http://localhost:5225/api/health` returns `{"status":"ok"}`
- [ ] AI config is seeded in database

### 6.2 Ollama Tests

- [ ] Ollama is running: `curl http://localhost:11434`
- [ ] Model is available: `ollama list`
- [ ] Test generation works

### 6.3 Database Tests

- [ ] `Database\sms.db` file exists
- [ ] Tables created: Accounts, Posts, Communities, etc.

### 6.4 Android Tests

- [ ] App builds without errors
- [ ] App installs on device
- [ ] App connects to server
- [ ] Server health shows "ONLINE"

### 6.5 Full Simulation Tests

- [ ] Register a new account
- [ ] See NPC accounts in the system
- [ ] Wait 1-2 minutes for simulation tick
- [ ] See NPC posts appear in feed
- [ ] See likes and comments on posts

---

## 7. Troubleshooting

### 7.1 Server Won't Start

**Error: Port 5225 in use**
```bash
netstat -ano | findstr :5225
taskkill /PID <pid> /F
```

**Error: Database locked**
```bash
del Database\sms.db-journal
```

### 7.2 Android Can't Connect

1. Check server is running
2. Check IP address is correct
3. Check `usesCleartextTraffic="true"` in manifest
4. Check firewall allows connection
5. Ping server from device: `ping 192.168.1.100`

### 7.3 No NPC Activity

1. Check Ollama is running: `curl http://localhost:11434`
2. Check AI config in database
3. Check server logs for LLM errors
4. Try: Restart server, wait 2 minutes

### 7.4 Slow LLM Responses

1. Use smaller model
2. Reduce `TimeoutSeconds` if you want faster fallback
3. Check Ollama is not overloaded

---

## 8. Quick Start Commands

### Start Everything

**Terminal 1 — Ollama:**
```bash
ollama serve
```

**Terminal 2 — Server:**
```bash
cd D:\SMS\Server
dotnet run
```

**Terminal 3 — Check Health:**
```bash
curl http://localhost:5225/api/health
```

### Build Android

```bash
cd D:\SMS\Client
dotnet build -f net10.0-android -c Release
```

### Install APK

```bash
adb install -r bin\Release\net10.0-android\com.companyname.socialmediasimulator.apk
```

---

## 9. Git & README

### 9.1 Update README.md

Add "Getting Started" section:

```markdown
## Getting Started

### Prerequisites
- .NET 10 SDK
- Android SDK
- Ollama running locally

### Configuration

1. **Ollama Settings** — Edit `Server\appsettings.json`:
   ```json
   "AiProvider": {
     "Provider": "Generic",
     "BaseUrl": "http://localhost:11434",
     "Model": "YOUR_MODEL",
     "ApiKey": "YOUR_API_KEY"
   }
   ```

2. **Android Settings** — Edit `Client\Configuration\AppConfig.cs`:
   ```csharp
   public string ApiBaseUrl { get; set; } = "http://YOUR_SERVER_IP:5225";
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

3. **Build & Install Android**:
   ```bash
   cd Client
   dotnet build -f net10.0-android -c Release
   adb install -r bin/Release/net10.0-android/com.companyname.socialmediasimulator.apk
   ```

4. **Open app and register!**

### First Run

1. Register a new account
2. Wait 1-2 minutes
3. Watch NPCs start posting
4. Join communities
5. See trends emerge
```

### 9.2 Git Commit

```bash
git add .
git commit -m "Part 21: Deployment & Testing - Make it runnable"
git push
```

---

## 10. DELIVERABLES

After this part:

1. ✅ Server runs without errors
2. ✅ Database initializes and migrates
3. ✅ Ollama connects and generates content
4. ✅ NPCs post, follow, and interact
5. ✅ Android app connects to server
6. ✅ Full simulation runs
7. ✅ You can test on your Android device

---

## 11. FINAL SESSION REPORT

```text
# PART 21 — COMPLETE

## 1. Configuration Done
- [ ] appsettings.json updated with Ollama config
- [ ] AI provider config seeded on startup
- [ ] Database initialized
- [ ] Android API URL configurable

## 2. Server Status
- [ ] Builds successfully
- [ ] Runs on port 5225
- [ ] Health endpoint responds
- [ ] AI config seeded

## 3. Ollama Status
- [ ] Connection configured
- [ ] Model name extracted from API key

## 4. Database Status
- [ ] Database created
- [ ] Tables exist
- [ ] AI config record created

## 5. Android Status
- [ ] usesCleartextTraffic enabled
- [ ] Builds successfully
- [ ] Connects to server

## 6. Testing Complete
- [ ] Server health OK
- [ ] App shows ONLINE
- [ ] NPCs post content
- [ ] Feed populates

## 7. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 8. Current Project Status
01A-21 COMPLETE (DEPLOYMENT READY)

## 9. NEXT
NEXT: PART 22 — News
```

**You are now ready to test the full Social Media Simulator on Android!**

**STOP after completing Part 21 and reporting the session log.**
