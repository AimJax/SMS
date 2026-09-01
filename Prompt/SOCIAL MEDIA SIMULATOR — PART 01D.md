# SOCIAL MEDIA SIMULATOR — PART 01D

# SQLITE FOUNDATION

We are continuing the Social Media Simulator project from the verified checkpoint.

## CURRENT CHECKPOINT

```text
01A — Development Environment     COMPLETE
01B — Repository Foundation       COMPLETE
01C — ASP.NET Core Server         COMPLETE

NEXT:
01D — SQLite Foundation
```

The project is located locally at:

```text
D:\SMS
```

The local filesystem is the source of truth.

Do NOT clone GitHub.

Do NOT download the repository.

Do NOT push to GitHub.

Do NOT redo completed work unless inspection proves something is missing or broken.

---

# 1. YOUR ONLY OBJECTIVE

Build and verify the first real SQLite persistence foundation.

The target architecture is:

```text
ASP.NET Core Server
        ↓
Application / Persistence layer
        ↓
SQLite
        ↓
Persistent data
```

The server must be able to:

```text
Start
 ↓
Connect to SQLite
 ↓
Create/write persistent data
 ↓
Read persistent data
 ↓
Stop
 ↓
Restart
 ↓
Read the same data again
```

The most important test is:

# DATA MUST SURVIVE SERVER RESTART.

Do not proceed to Android yet.

Do not implement accounts.

Do not implement NPCs.

Do not implement posts.

Do not implement authentication.

Do not implement the LLM.

Do not implement the social graph.

Do not implement future game systems.

This checkpoint is ONLY about SQLite persistence.

---

# 2. FIRST ACTION — INSPECT

Before changing anything, inspect the actual project.

Check:

```text
D:\SMS
D:\SMS\Server
D:\SMS\Shared
D:\SMS\Client
D:\SMS\Database
D:\SMS\Tests
D:\SMS\Documentation
```

Inspect:

```text
Server.csproj
Program.cs
solution/project files
existing NuGet packages
configuration files
appsettings.json
appsettings.Development.json
existing services
existing folders
existing tests
Git status
Git log
```

Determine whether SQLite infrastructure already exists.

If something already exists:

# REUSE IT.

Do not create duplicate:

```text
DbContext
DatabaseService
SQLiteService
Repository
ConnectionFactory
Database configuration
```

---

# 3. ARCHITECTURAL GOAL

Create a clean persistence boundary.

SQLite access must NOT become scattered throughout:

```text
Program.cs
Controllers
Endpoints
Business logic
```

Avoid this:

```csharp
var connection = new SqliteConnection(...);
```

randomly throughout the application.

Instead, establish a dedicated persistence/data-access structure.

The exact folder structure may be chosen based on the existing project, but keep responsibilities separated.

A reasonable starting structure is:

```text
Server/
│
├── Program.cs
│
├── Data/
│   ├── Database/
│   ├── Repositories/
│   └── ...
│
├── Services/
│
└── ...
```

Do not over-engineer this checkpoint.

We need a solid foundation, not the entire future database architecture.

---

# 4. SQLITE LOCATION

The SQLite database must live under the project's persistent data area.

Prefer:

```text
D:\SMS\Database\
```

or another clearly defined project-local database directory.

Do NOT place the project's actual SQLite database inside:

```text
bin/
obj/
```

Do NOT depend on a temporary directory.

Do NOT hardcode an absolute developer-specific path such as:

```text
D:\Users\Someone\...
```

The project should remain portable.

Use configuration for the database location.

For development, a relative/project-controlled location is acceptable.

---

# 5. SQLITE PACKAGE

Use an appropriate maintained SQLite provider compatible with:

```text
.NET 10
ASP.NET Core
SQLite
```

Inspect the current project first.

If a suitable provider is already installed, reuse it.

Do not install unnecessary database packages.

Do not add an ORM merely because it is convenient unless there is a clear architectural reason.

For this checkpoint, a lightweight SQLite data-access layer is sufficient.

The important requirement is:

```text
Clean persistence boundary
+
Real SQLite database
+
Reliable persistence
```

---

# 6. DATABASE CONFIGURATION

Database configuration should not be scattered through source code.

Create a clear configuration mechanism for:

```text
Database path
```

The application should obtain the database location through configuration/options/injected services rather than random hardcoded paths.

The design should allow future environments:

```text
Development
Testing
Production
```

without rewriting database code.

---

# 7. DATABASE INITIALIZATION

Create the SQLite database infrastructure.

On application startup, the system should be capable of ensuring the database exists.

The exact initialization strategy should be appropriate for the chosen persistence architecture.

Do NOT create the entire future Social Media Simulator schema.

At this stage, we only need enough schema to prove persistence.

---

# 8. TEST PERSISTENCE ENTITY

Create a tiny temporary persistence model/table specifically for this foundation test.

For example:

```text
PersistenceTest
```

with something similar to:

```text
Id
Value
CreatedAt
```

The exact naming is up to the implementation, but keep it clearly identified as a foundation/test record.

Do NOT pretend this is the final game schema.

This test exists only to prove:

```text
Server
 ↓
Persistence layer
 ↓
SQLite
 ↓
Write
 ↓
Read
```

---

# 9. DATABASE OPERATIONS

Implement the minimum operations required to prove persistence:

```text
Create/write record
Read record
```

Use:

```text
Parameterized queries
```

or the equivalent safe mechanism.

Never construct SQL by concatenating uncontrolled user input.

Example of what NOT to do:

```csharp
"INSERT INTO Test VALUES ('" + value + "')"
```

Use parameters.

---

# 10. HEALTH / TEST ENDPOINT

Do not destroy the existing:

```text
GET /api/health
```

It must continue returning:

```json
{
  "status": "ok"
}
```

Add a temporary persistence verification endpoint only if it is useful for testing.

For example, conceptually:

```text
POST /api/persistence-test
GET  /api/persistence-test/{id}
```

The exact endpoint design is up to you.

Keep it clearly marked as development/foundation testing infrastructure.

Do not expose unnecessary database internals.

---

# 11. CRITICAL RESTART TEST

This is mandatory.

Perform the following exact sequence:

### TEST A — FIRST START

Start the server.

Verify SQLite initializes successfully.

Write a known test record.

Example:

```text
Value = "SQLite persistence test"
```

Read it back.

Verify success.

---

### TEST B — STOP SERVER

Completely stop the ASP.NET Core server.

Verify the process has actually stopped.

---

### TEST C — RESTART SERVER

Start the server again.

Do NOT recreate the test record first.

Read the existing record from SQLite.

Verify that the exact same data exists.

Expected result:

```text
Before restart:
Id = X
Value = SQLite persistence test

Server stopped.

Server restarted.

After restart:
Id = X
Value = SQLite persistence test
```

This proves the database is genuinely persistent.

---

# 12. SQLITE DATABASE VERIFICATION

Use the SQLite CLI if appropriate to independently inspect the database.

Verify:

```text
Database file exists
Table exists
Record exists
```

Do not rely solely on the application to tell you SQLite works.

Where practical, directly inspect the SQLite database.

---

# 13. WAL MODE

Evaluate enabling SQLite WAL mode:

```sql
PRAGMA journal_mode=WAL;
```

If enabled, verify that it is appropriate for this project's architecture.

Do not blindly add configuration without understanding its effect.

If you enable WAL, document why.

Remember that SQLite configuration should be deliberate.

---

# 14. CONNECTION MANAGEMENT

Do not leave SQLite connections open indefinitely.

Use proper disposal/lifetime management.

The persistence layer should safely handle:

```text
Open
Execute
Read
Dispose
```

Do not create a single unmanaged global SQLite connection.

---

# 15. TRANSACTIONS

Establish the foundation so transactions can be used later.

You do not need to implement complicated transaction logic yet.

However, do not build the persistence layer in a way that makes transactions difficult or impossible later.

---

# 16. INDEXES

Do not prematurely create dozens of indexes.

For the tiny persistence test, only create what is actually required.

Future indexes will be designed around real query patterns.

---

# 17. DATABASE SAFETY

This project has a permanent-history requirement.

Even though the actual history systems do not exist yet, the persistence architecture must respect this principle from the beginning.

NEVER introduce automatic deletion/pruning systems.

Do NOT create:

```text
Delete old records
Delete records after X days
Delete inactive data
Delete old memories
Delete old events
```

Nothing like this belongs in the project.

---

# 18. NO FUTURE SYSTEMS

Do NOT implement:

```text
Users
NPCs
Profiles
Posts
Comments
Likes
Followers
Relationships
Events
Trends
Virality
Rumors
News
Memories
LLM
Ollama
Qwen
Feed algorithms
Authentication
Android UI
WebSockets
```

Those are future phases.

The only objective is SQLite persistence infrastructure.

---

# 19. TESTING

At minimum verify:

```text
1. Server builds.
2. SQLite database is created.
3. Database can be opened.
4. Table can be created.
5. Record can be written.
6. Record can be read.
7. Server can stop.
8. Server can restart.
9. Existing record can be read after restart.
10. /api/health still works.
```

Also verify:

```text
Git working tree
```

before committing.

---

# 20. ERROR HANDLING

If SQLite fails:

```text
STOP.
```

Do not continue building additional systems.

Determine:

```text
Root cause
 ↓
Fix
 ↓
Build
 ↓
Run
 ↓
Test
```

Do not hide database errors.

Do not silently ignore failed writes.

Do not claim persistence works unless the restart test actually succeeded.

---

# 21. CODE QUALITY

Prefer:

```text
Small classes
Clear responsibilities
Dependency injection
Configuration
Safe SQL
Proper disposal
Testable persistence code
```

Avoid:

```text
Giant DatabaseManager
Static global state
Hardcoded paths
Scattered SQL
Duplicated connections
Duplicated repositories
Over-engineering
Premature abstractions
```

Do not build a 20-file enterprise architecture for one test table.

Build the smallest foundation that can grow cleanly.

---

# 22. TESTS

If the existing test infrastructure is already suitable, add an appropriate persistence test.

At minimum, test the persistence layer's ability to:

```text
Write
Read
```

If practical, include a test proving the same SQLite file can be reopened and the record remains.

Do not introduce a complicated testing framework solely for this checkpoint if none exists yet.

---

# 23. GIT

Before committing:

```text
git status
```

Inspect every changed file.

Make sure there are no:

```text
Secrets
Credentials
Temporary dumps
Huge logs
Build artifacts
Unintended files
```

The SQLite development database itself should generally NOT be committed to Git unless there is a specific reason to do so.

Add appropriate database/runtime files to `.gitignore` if necessary.

Do NOT accidentally ignore the entire:

```text
Database/
```

directory if it is intended to contain schema/migration/tooling files.

Distinguish:

```text
Database tooling/schema
```

from:

```text
Runtime SQLite database
```

---

# 24. COMMIT

Only after all tests succeed:

Create a Git checkpoint with a clear message such as:

```text
Add SQLite persistence foundation
```

Verify:

```text
git log --oneline -2
git status
```

Expected:

```text
Working tree clean
```

---

# 25. REQUIRED FINAL REPORT

When finished, report:

## What Was Built

Briefly explain the SQLite foundation.

## Files Created

List exact paths.

## Files Modified

List exact paths.

## Database

Show:

```text
Database location
SQLite provider
Database initialization
```

## Persistence Test

Show:

```text
Write: PASS
Read: PASS
Stop: PASS
Restart: PASS
Read after restart: PASS
```

## API

Confirm:

```text
/api/health = 200
{"status":"ok"}
```

## Build

Show:

```text
Build: SUCCESS
Errors: 0
```

## Git

Show:

```text
Commit:
Working tree:
```

## Current Checkpoint

State:

```text
01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
```

Then:

# STOP.

Do NOT begin Part 01E automatically.

---

# 26. ABSOLUTE STOP CONDITION

The only thing that matters in this checkpoint is:

```text
ASP.NET Core
      ↓
SQLite persistence layer
      ↓
Write data
      ↓
Stop server
      ↓
Restart server
      ↓
Read same data
```

Once that works and is committed:

# STOP.

Wait for the next instruction.

Do not jump ahead.

Do not implement Android.

Do not implement accounts.

Do not implement NPCs.

Do not implement the LLM.

Do not implement the game.

We are building this one verified checkpoint at a time.