# SOCIAL MEDIA SIMULATOR — PART 01E

# ANDROID CLIENT FOUNDATION

We are continuing the Social Media Simulator from the verified checkpoint.

## CURRENT VERIFIED STATE

```text
01A — Development Environment     COMPLETE
01B — Repository Foundation       COMPLETE
01C — ASP.NET Core Server         COMPLETE
01D — SQLite Foundation           COMPLETE

NEXT:
01E — Android Client
```

Project location:

```text
D:\SMS
```

GitHub username:

```text
AimJax
```

IMPORTANT:

The repository is local.

Do NOT clone or download the repository.

Do NOT assume GitHub contains the latest state.

The local filesystem at:

```text
D:\SMS
```

is the source of truth.

---

# 0. NEW PERMANENT RULE — README MUST ALWAYS BE UPDATED

Starting with this part:

# EVERY COMPLETED DEVELOPMENT PART MUST UPDATE README.md.

The README is now the project's living development record.

Whenever a part is successfully completed:

1. Update `README.md`.
2. Record what was actually implemented.
3. Record the current project structure if it materially changed.
4. Record important technology/configuration decisions.
5. Record verification/test results.
6. Record the current completed checkpoint.
7. Record the next checkpoint.
8. Do NOT claim features are complete if they were not actually implemented.
9. Do NOT erase useful historical development information from the README.
10. Keep the README accurate and concise enough to remain maintainable.

For this task, update the README only AFTER 01E actually works.

The README should reflect reality, not the intended roadmap.

---

# 1. CURRENT PROJECT STATE

Before making changes, inspect the actual project.

Inspect:

```text
D:\SMS
D:\SMS\Server
D:\SMS\Client
D:\SMS\Shared
D:\SMS\Database
D:\SMS\Tests
D:\SMS\Documentation
```

Inspect:

```text
README.md
.gitignore
solution/project files
Server/Server.csproj
Server/Program.cs
Server/appsettings.json
existing Client files
existing configuration
existing Git history
```

Also inspect the existing Git state:

```text
git status
git log --oneline -5
```

Confirm the previous SQLite checkpoint exists.

Expected latest checkpoint should be approximately:

```text
3a29309 Add SQLite persistence foundation
```

Do not blindly assume the commit hash if the local Git history differs. Inspect it.

---

# 2. DO NOT REDO COMPLETED WORK

Do NOT rebuild:

```text
01A
01B
01C
01D
```

unless inspection shows something is broken.

The backend and SQLite foundation already work.

The existing endpoint:

```text
GET /api/health
```

must remain functional.

The existing SQLite persistence infrastructure must remain functional.

Do not rewrite the backend just to connect the Android client.

---

# 3. OBJECTIVE

Build the first Android client foundation.

The target is:

```text
Android App
      ↓
HTTP
      ↓
ASP.NET Core Server
      ↓
GET /api/health
      ↓
JSON response
      ↓
Android UI
```

The Android application must display whether the backend is reachable.

The minimum successful experience is:

```text
Social Media Simulator

Server Status:
ONLINE
```

when the backend responds successfully.

And an appropriate offline/error state when it does not.

---

# 4. TECHNOLOGY

The master specification specifies:

```text
C#
.NET
Android-compatible application framework
Preferred direction: .NET MAUI
```

Use:

# .NET MAUI

unless inspection of the installed environment reveals a concrete technical blocker.

Do NOT use Unity.

Do NOT use:

```text
Unity
MonoBehaviour
Unity scenes
Unity prefabs
Unity networking
```

This is a standalone Android application.

---

# 5. BEFORE CREATING THE CLIENT

Inspect the installed environment.

Verify:

```text
.NET SDK
.NET MAUI workload
Android SDK
Android build tools
Android emulator/device support
```

Use appropriate commands such as:

```text
dotnet --info
dotnet workload list
```

and inspect the Android environment as appropriate.

Do not install random packages.

Do not upgrade unrelated tooling unless necessary.

If .NET MAUI is already installed and working, use it.

If it is missing, install only the required workload/tooling.

---

# 6. CLIENT LOCATION

The Android client belongs under:

```text
D:\SMS\Client
```

Inspect whether `Client` already contains anything.

If an existing client project exists:

# REUSE IT.

Do not create a second Android project beside an existing one.

If the client directory is empty or only contains placeholder files, create the MAUI project there.

---

# 7. CLIENT ARCHITECTURE

Keep the first client extremely small.

At this stage we need:

```text
Client
 ├── UI
 ├── Configuration
 └── Networking
```

Do NOT build a giant client architecture.

Do NOT create:

```text
FeedService
ProfileService
PostService
SocialGraphService
NotificationService
WebSocketManager
AuthenticationManager
NPCManager
SimulationManager
```

Those belong to later phases.

For this checkpoint, the client only needs to communicate with:

```text
GET /api/health
```

---

# 8. SERVER URL CONFIGURATION

Do NOT hardcode the developer machine IP address throughout the application.

The client needs a configurable server base URL.

For example, conceptually:

```text
ApiBaseUrl
```

Development configuration can point to the local ASP.NET Core server.

However:

# DO NOT assume localhost behaves identically on Android and the development PC.

Remember:

```text
Android localhost
```

means the Android device itself, not necessarily the developer PC.

If using:

```text
Android Emulator
```

use the appropriate emulator host mapping where necessary.

If using a physical Android device, use the development machine's reachable local network address for temporary development testing.

Keep this configuration isolated so the production server URL can later be changed without rewriting networking code.

---

# 9. NETWORK SERVICE

Create a small HTTP client/service abstraction.

Its only responsibility for this checkpoint should be something equivalent to:

```text
CheckServerHealthAsync()
```

It should:

1. Send HTTP GET to `/api/health`.
2. Receive the response.
3. Deserialize the response.
4. Determine whether the server is healthy.
5. Return a simple result to the UI.
6. Handle connection failures safely.

Do not put HTTP code directly throughout the UI.

Avoid:

```csharp
new HttpClient()
```

being repeatedly created for every button click/request.

Use an appropriate reusable/injected `HttpClient` design.

---

# 10. HEALTH RESPONSE

The existing backend returns:

```json
{
  "status": "ok"
}
```

The client should consume this existing contract.

Do NOT change the backend response merely to make the Android client easier.

Do NOT create a second health endpoint.

Reuse:

```text
GET /api/health
```

---

# 11. UI

Create a minimal mobile UI.

It should clearly display:

```text
Social Media Simulator
```

and:

```text
Server Status
```

Possible states:

```text
Checking...
Server Online
Server Offline
Connection Error
```

Use a simple layout.

Do NOT spend time designing the final social-media UI.

This is a connectivity checkpoint.

---

# 12. USER ACTION

Provide a way to manually check the server again.

For example:

```text
Check Server
```

button.

Expected flow:

```text
App starts
 ↓
Check server
 ↓
GET /api/health
 ↓
Display result
```

Then:

```text
User taps Check Server
 ↓
Request sent again
 ↓
Result updated
```

---

# 13. ERROR HANDLING

The Android app must NOT crash if the backend is unavailable.

Test at least:

```text
Backend running
Backend stopped
Backend restarted
```

When backend is unavailable, show an appropriate user-facing status.

Do not expose raw exception stack traces in the UI.

Developer logs may contain useful diagnostic information.

---

# 14. TIMEOUT

Do not allow a failed server connection to hang indefinitely.

Use a reasonable HTTP timeout for this basic test.

The exact timeout should be appropriate for development and mobile networking.

Avoid extremely long waits.

---

# 15. NO AUTHENTICATION YET

Do NOT implement:

```text
Registration
Login
JWT
Sessions
Passwords
Refresh tokens
Accounts
```

Those belong to Part 04.

This checkpoint only verifies:

```text
Android → Backend
```

---

# 16. NO WEBSOCKETS YET

Do NOT implement WebSockets.

The master specification says WebSockets should be used where they provide genuine value, but that is a later checkpoint.

For now:

```text
HTTP REST
```

is sufficient.

---

# 17. NO SQLITE CLIENT DATABASE

Do NOT add a local social-network database.

The client does not need SQLite for this checkpoint.

The server already owns persistent SQLite state.

Later the client may have a cache, but that is not part of 01E.

---

# 18. SERVER REMAINS AUTHORITATIVE

The client must not create or own server world state.

Even in this tiny test:

```text
Android
   ↓
Request
   ↓
Server
   ↓
Response
```

The client only presents the server response.

Do not duplicate server state inside the client.

---

# 19. TEST ON EMULATOR OR DEVICE

Prefer testing on an actual Android target if available.

Test:

### Test 1 — App launches

Expected:

```text
Application starts without crashing.
```

### Test 2 — Backend running

Start ASP.NET Core.

Open Android app.

Expected:

```text
Server Online
```

### Test 3 — Backend stopped

Stop ASP.NET Core.

Tap:

```text
Check Server
```

Expected:

```text
Server Offline
```

or an equivalent clear error state.

The app must not crash.

### Test 4 — Backend restarted

Start ASP.NET Core again.

Tap:

```text
Check Server
```

Expected:

```text
Server Online
```

---

# 20. VERIFY BACKEND STILL WORKS

After Android implementation, independently verify:

```text
GET /api/health
```

still returns:

```json
{
  "status": "ok"
}
```

Also verify the existing SQLite persistence test still works.

Do not let the client implementation accidentally break the backend.

---

# 21. BUILD TEST

Build the entire relevant solution/project.

Verify:

```text
Build succeeds
Errors = 0
```

Warnings should be inspected.

Do not blindly ignore new warnings.

If there are build errors:

# STOP AND FIX THEM.

Do not proceed to the next part.

---

# 22. README UPDATE — REQUIRED

Once 01E actually works, update:

```text
D:\SMS\README.md
```

The README should now document the actual state.

At minimum include/update:

```text
# Social Media Simulator

## Project Status

01A — Development Environment     COMPLETE
01B — Repository Foundation       COMPLETE
01C — ASP.NET Core Server         COMPLETE
01D — SQLite Foundation           COMPLETE
01E — Android Client              COMPLETE

## Architecture

Android Client
      ↓
HTTP
      ↓
ASP.NET Core
      ↓
SQLite
```

Document:

```text
Android technology: .NET MAUI
Backend: ASP.NET Core
Database: SQLite / EF Core SQLite
Health endpoint: GET /api/health
```

Document the Android connectivity test.

Include the actual result, not an assumed result.

For example:

```text
Android → ASP.NET Core health check: PASS
Backend unavailable handling: PASS
Backend restart/reconnect: PASS
```

Also document any important development URL configuration required for the emulator/device.

Do not put secrets in README.

Do not put private credentials in README.

---

# 23. README PRINCIPLE GOING FORWARD

From this checkpoint onward:

# README.md IS A LIVING PROJECT CHECKPOINT RECORD.

At the end of every completed part:

```text
Implement
 ↓
Compile
 ↓
Run
 ↓
Test
 ↓
Verify
 ↓
Update README
 ↓
Commit
 ↓
STOP
```

Never update the README with functionality that hasn't actually been implemented.

Never leave the README claiming an old state after a successful checkpoint.

---

# 24. GIT SAFETY

Before committing:

```text
git status
```

Inspect every changed file.

Make sure there are no:

```text
Secrets
Credentials
Generated build artifacts
Temporary files
Device-specific private configuration
Huge logs
Unintended files
```

Do not commit runtime databases.

Do not commit passwords or API keys.

---

# 25. GIT CHECKPOINT

Only after:

```text
Android builds
+
Android launches
+
Android reaches backend
+
Health response displayed
+
Offline handling works
+
Backend still works
+
SQLite still works
+
README updated
```

create a checkpoint commit.

Suggested commit message:

```text
Add Android client foundation
```

Then verify:

```text
git status
git log --oneline -3
```

Expected:

```text
Working tree clean
```

---

# 26. REQUIRED FINAL REPORT

Report:

## What Was Built

Explain the Android client foundation.

## Technology

```text
.NET MAUI
C#
HTTP REST
```

## Files Created

List exact paths.

## Files Modified

List exact paths.

## Server Communication

```text
GET /api/health
```

Result:

```text
PASS
```

## Tests

Report actual results:

```text
Android launch: PASS/FAIL
Backend online: PASS/FAIL
Backend offline handling: PASS/FAIL
Reconnect after restart: PASS/FAIL
Backend health endpoint: PASS/FAIL
SQLite regression test: PASS/FAIL
```

## Build

```text
Build: SUCCESS/FAIL
Errors: X
Warnings: X
```

## README

Confirm:

```text
README updated: YES
```

## Git

Report:

```text
Commit:
Working tree:
```

## Current Checkpoint

If everything passes:

```text
01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
```

Then state:

```text
NEXT: 01F — Foundation Checkpoint
```

# STOP.

Do NOT implement 01F automatically.

Do NOT implement accounts.

Do NOT implement authentication.

Do NOT implement NPCs.

Do NOT implement posts.

Do NOT implement the LLM.

Wait for the next instruction.