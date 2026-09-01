# SOCIAL MEDIA SIMULATOR — PART 03 DEVELOPMENT TASK

## CURRENT PROJECT STATE

We are continuing an existing Social Media Simulator project.

**Project location:**

```text
D:\SMS
```

The actual project files are the source of truth.

The project has already completed:

```text
01A — Development Environment       COMPLETE
01B — Repository Foundation         COMPLETE
01C — ASP.NET Core Server           COMPLETE
01D — SQLite Foundation             COMPLETE
01E — Android Client Foundation     COMPLETE
01F — Foundation Checkpoint         COMPLETE

02 — Backend Architecture            COMPLETE
```

Latest known Git checkpoint:

```text
ae1958e — Establish backend architecture
```

The working tree was clean at the last checkpoint.

The current backend architecture includes:

```text
Server/
├── API/
│   ├── Controllers/
│   └── Middleware/
├── Application/
│   └── Services/
├── Domain/
│   └── Entities/
├── Infrastructure/
│   └── Persistence/
├── Contracts/
│   ├── Requests/
│   └── Responses/
├── Extensions/
└── Program.cs
```

The existing backend architecture was successfully compiled and tested.

The project currently has working:

```text
Android Client
      ↓
HTTP REST
      ↓
ASP.NET Core Server
      ↓
EF Core / SQLite
```

The existing persistence test functionality must be preserved unless there is a concrete reason to change it.

---

# CURRENT TASK

# PART 03 — PERSISTENCE

This task is ONLY about strengthening and formalizing the backend persistence architecture.

Do NOT begin Part 04.

Do NOT implement authentication.

Do NOT implement accounts.

Do NOT implement users.

Do NOT implement social graphs.

Do NOT implement posts.

Do NOT implement feeds.

Do NOT implement NPCs.

Do NOT implement relationships.

Do NOT implement LLM integration.

Do NOT create the 10,000-account population.

Do NOT create the complete future database schema.

Do NOT add future gameplay systems.

---

# FIRST RULE — INSPECT THE REAL PROJECT

Before modifying anything:

1. Inspect `D:\SMS`.
2. Inspect `README.md`.
3. Inspect Git status.
4. Inspect the latest commit.
5. Inspect the complete current Server structure.
6. Inspect `Server.csproj`.
7. Inspect `AppDbContext`.
8. Inspect the current EF Core configuration.
9. Inspect the current SQLite configuration.
10. Inspect the current persistence test entity.
11. Inspect the persistence test service.
12. Inspect the existing controllers.
13. Inspect dependency injection registration.
14. Inspect configuration files.
15. Inspect any existing migrations.
16. Inspect any existing database file.
17. Determine exactly what Part 02 already implemented.

Do NOT assume the architecture from this prompt is identical to the actual files.

The actual files are authoritative.

---

# PART 03 GOAL

The goal of Part 03 is to establish a clean, safe, reusable persistence foundation that future systems can build on.

We want:

```text
Application
      ↓
Persistence abstraction
      ↓
Infrastructure
      ↓
EF Core
      ↓
SQLite
```

The persistence system must be suitable for the future Social Media Simulator without prematurely implementing the future game.

---

# 03A — DATABASE FACTORY / CONNECTION HANDLING

Inspect the current database connection and DbContext creation.

Determine whether the current architecture has appropriate handling for:

```text
DbContext creation
Database configuration
Connection strings
Dependency injection
Connection lifetime
SQLite configuration
```

Improve only what is actually necessary.

Do NOT create unnecessary abstractions just for the sake of having more files.

The goal is maintainability, not abstraction for abstraction's sake.

---

# 03B — DATABASE CONFIGURATION

Database configuration must not be scattered throughout the codebase.

Centralize appropriate configuration.

The application should be able to configure the database location without changing source code.

Development configuration should remain simple.

Do not hardcode:

```text
D:\SMS
```

inside persistence classes merely because that is the current development location.

The project should remain portable.

---

# 03C — EF CORE CONFIGURATION

Inspect the current `AppDbContext`.

Ensure EF Core configuration is clean and appropriate.

Use:

```text
DbContext
DbSet<T>
Entity configuration
Dependency injection
SQLite provider
```

where appropriate.

If entity configuration is needed, use a clean approach that scales as the number of entities grows.

Do NOT create configurations for future entities that do not exist yet.

---

# 03D — SQLITE CONFIGURATION

Verify the SQLite database is configured appropriately.

Where appropriate, use:

```text
WAL mode
Foreign keys
Busy timeout
Appropriate connection configuration
```

Do not blindly add PRAGMA statements without understanding how they interact with EF Core and SQLite.

Do not optimize prematurely.

Only implement settings that provide a concrete benefit to this project's persistence layer.

---

# 03E — MIGRATION SYSTEM

Inspect whether EF Core migrations already exist.

If migrations are not yet properly established, establish a clean migration workflow.

The project should be able to evolve its database schema through migrations.

Future schema changes must be incremental.

Do NOT use:

```text
Delete database
Recreate database
```

as the normal development workflow.

The persistence architecture must support:

```text
Existing Database
      ↓
Migration
      ↓
Updated Schema
      ↓
Existing Data Preserved
```

---

# DATABASE SAFETY — EXTREMELY IMPORTANT

This project requires permanent historical persistence.

Never implement automatic pruning.

Never implement automatic history deletion.

Never implement automatic memory deletion.

Never implement automatic event deletion.

Never implement automatic post deletion.

Never implement automatic message deletion.

Never implement automatic metric deletion.

Never implement "cleanup old data" as a persistence optimization.

Do NOT add:

```text
Retention policies
Automatic cleanup jobs
TTL deletion
Old-record deletion
Database vacuum as a reason to delete data
```

The project's philosophy is:

> Performance must come from good data architecture, not destroying history.

---

# 03F — TRANSACTIONS

Establish a sensible transaction strategy.

Determine where transactions are actually required.

Transactions should be used when multiple related database changes must succeed or fail together.

Examples for future systems may include:

```text
Create event
+
Create event history
+
Update related state
```

However, DO NOT implement future event systems now.

For this Part, establish the persistence capability and demonstrate it using existing functionality where appropriate.

Do not introduce a giant generic transaction framework unless the existing architecture genuinely requires one.

---

# 03G — INDEXING

Inspect the current database entities.

Add indexes only where they make sense for existing queries.

Do NOT create hundreds of hypothetical indexes for future systems.

Indexes should be based on:

```text
Actual query patterns
Primary keys
Foreign keys
Existing lookup operations
```

Document important indexing decisions.

Future systems can add their own indexes when their queries are implemented.

---

# 03H — PERSISTENCE ABSTRACTION

Review whether application/business logic currently depends directly on EF Core details.

The long-term goal is:

```text
Application
      ↓
Persistence abstraction
      ↓
Infrastructure
      ↓
EF Core
```

Avoid unnecessary coupling such as:

```text
Controller
    ↓
DbContext
```

or:

```text
Business logic
    ↓
Raw SQLite connection everywhere
```

Use appropriate interfaces/services/repositories only where they provide real architectural value.

IMPORTANT:

Do NOT blindly create a repository class for every database table.

Do NOT create:

```text
IUserRepository
IPostRepository
ICommentRepository
...
```

when those entities do not even exist yet.

Design for future growth without building the future prematurely.

---

# 03I — PARAMETERIZED / SAFE DATABASE ACCESS

Any raw SQL that exists should be inspected.

Do not use unsafe string-concatenated SQL.

Prefer:

```text
EF Core LINQ
```

or properly parameterized SQL when raw SQL is genuinely necessary.

Do NOT replace working EF Core queries with raw SQL just for performance without evidence.

---

# 03J — ASYNC DATABASE OPERATIONS

Inspect current database operations.

Where appropriate, use asynchronous EF Core APIs:

```text
ToListAsync
FirstOrDefaultAsync
SingleOrDefaultAsync
SaveChangesAsync
ExecuteAsync
```

Do not use async merely for appearance.

The goal is correct server-side I/O behavior.

---

# 03K — DATABASE ERROR HANDLING

Review persistence failure behavior.

The server should handle database errors without exposing internal implementation details to the client.

The existing exception middleware from Part 02 should remain integrated.

Do not create a second competing global error-handling system.

For example, the client should not receive:

```text
Full SQLite exception
Internal stack trace
Database filesystem path
Internal server details
```

in production responses.

Detailed information can remain in server-side logs.

---

# 03L — PERSISTENCE TESTING

Strengthen the existing persistence verification.

Tests should verify at minimum:

```text
Create record
      ↓
Save
      ↓
Read
      ↓
Restart server
      ↓
Read existing record
```

Also verify that existing data is preserved when the application restarts.

If automated tests are appropriate for the current architecture, add focused persistence tests.

Do NOT build a massive testing framework.

Do NOT create dozens of speculative tests for future gameplay systems.

---

# 03M — DATABASE SURVIVAL TEST

Perform a real persistence test.

Use the existing persistence-test functionality or an appropriate focused test.

Verify:

```text
Server starts
        ↓
Write data
        ↓
Read data
        ↓
Stop server
        ↓
Start server
        ↓
Read same data
```

The data must survive.

Do not delete the database after testing.

Do not reset the database merely to make the test cleaner.

---

# 03N — DATABASE SCHEMA DISCIPLINE

At this stage, keep the schema intentionally small.

The current database should contain only what the existing foundation actually requires.

Do NOT create the eventual tables:

```text
Users
Posts
Comments
Follows
Relationships
Memories
Events
Trends
Rumors
News
Communities
Notifications
Messages
```

just because they appear in the master specification.

Those belong to later Parts.

Build the persistence infrastructure first.

---

# 03O — FUTURE DATABASE MIGRATION READINESS

The persistence layer should make it easy to later add:

```text
Accounts
Posts
Social Graph
Relationships
Events
Memories
Messages
```

without redesigning SQLite from scratch.

But:

> Future readiness does NOT mean implementing future tables now.

Keep today's implementation small.

---

# PERFORMANCE RULES

Do not optimize based on imaginary future bottlenecks.

For the current project:

```text
Correctness
>
Data safety
>
Maintainability
>
Measured performance
>
Premature optimization
```

When optimizing persistence, prefer:

```text
Indexes
Efficient queries
Async I/O
Connection management
Transactions
Batching
Caching where justified
```

Never:

```text
Delete history
```

as an optimization.

---

# DO NOT OVER-ENGINEER

Avoid creating unnecessary:

```text
GenericRepository<T>
GenericUnitOfWork
GenericDatabaseManager
MegaPersistenceService
MegaGameManager
MegaRepository
```

unless inspection proves one is actually necessary.

Avoid abstraction layers that have no current consumer.

A smaller clean architecture is better than a giant architecture with empty abstractions.

---

# PRESERVE PART 02

Part 02 just established the backend architecture.

Do not undo it.

Preserve the separation:

```text
API
Application
Domain
Infrastructure
Contracts
Extensions
```

The persistence work should fit into this architecture.

Do not collapse everything back into:

```text
Program.cs
```

Do not move persistence back into controllers.

Do not bypass the application layer without a concrete reason.

---

# ANDROID SCOPE

The Android client is NOT the focus of Part 03.

Do not redesign the Android application.

Do not add social UI.

Do not add accounts.

Do not add feeds.

Do not add authentication UI.

Only touch the Android project if a persistence/backend change genuinely requires an existing API contract adjustment.

Prefer leaving the client untouched.

---

# README REQUIREMENT — PERMANENT RULE

This is now a permanent project rule:

> **After completing every Part or Sub-Part, update `README.md` before creating the Git checkpoint.**

For Part 03, update:

```text
D:\SMS\README.md
```

The README must accurately describe the actual completed work.

Document:

```text
Part 03 — Persistence
```

Include relevant information such as:

```text
Persistence architecture
EF Core
SQLite
Database configuration
Migration workflow
Transaction strategy
Testing
Current database scope
```

Do NOT copy the entire master specification into the README.

Do NOT claim features that were not implemented.

Do NOT claim tests passed unless they were actually run.

The README should remain a useful project document.

---

# GIT CHECKPOINT

Only after implementation and testing succeeds:

1. Run Git status.
2. Review every changed file.
3. Confirm no unrelated files were modified.
4. Confirm no database files or generated junk were accidentally committed unless intentionally tracked.
5. Update README.
6. Review the README.
7. Commit the completed Part 03 work.

Use a clear commit message such as:

```text
Establish persistence architecture
```

or another accurate equivalent.

Then run:

```text
git status
```

The working tree should be clean.

Verify the new commit exists.

---

# FAILURE RULE

If anything fails:

```text
STOP
 ↓
Inspect
 ↓
Determine root cause
 ↓
Fix
 ↓
Build
 ↓
Test
 ↓
Verify
```

Do not continue into Part 04.

Do not hide errors.

Do not work around failures by deleting the database.

Do not recreate the entire project.

Do not reset working code without a reason.

---

# IMPORTANT — NO DESTRUCTIVE DATABASE RESET

Do NOT casually use:

```text
dotnet ef database drop
```

Do NOT casually delete:

```text
*.db
```

Do NOT recreate the SQLite database merely to test migrations.

Do NOT use destructive reset operations unless absolutely required and explicitly justified.

If a schema change requires migration:

```text
Existing database
      ↓
Migration
      ↓
Verify
```

not:

```text
Delete database
      ↓
Create new database
```

---

# REQUIRED IMPLEMENTATION PROCESS

Work incrementally.

Recommended sequence:

```text
1. Inspect current persistence
        ↓
2. Identify actual weaknesses
        ↓
3. Improve database configuration
        ↓
4. Improve EF Core setup
        ↓
5. Establish migration workflow
        ↓
6. Review transactions
        ↓
7. Review indexes
        ↓
8. Review persistence abstraction
        ↓
9. Improve persistence tests
        ↓
10. Build
        ↓
11. Run tests
        ↓
12. Run persistence restart test
        ↓
13. Update README
        ↓
14. Git checkpoint
        ↓
15. STOP
```

Do not blindly implement every item if inspection shows it is already correctly implemented.

---

# WHAT IS IMPORTANT

The following are HIGH PRIORITY:

```text
Existing data safety
Database persistence
Migration safety
Clean EF Core configuration
Correct dependency injection
Clear persistence boundaries
Reliable transactions
Safe queries
Focused tests
Restart persistence
README accuracy
Clean Git checkpoint
```

---

# WHAT IS NOT IMPORTANT RIGHT NOW

Do NOT spend this Part implementing:

```text
NPC behavior
LLM
Qwen3
Virality
Relationships
Romance
Drama
News
Rumors
Trends
Communities
Advanced feeds
10,000 accounts
Authentication
Creator economy
Notifications
WebSockets
Advanced Android UI
```

Those are future tasks.

---

# PERMANENT PROJECT MEMORY

Remember these architectural principles:

### 1. Server authoritative

The Android client is never authoritative over world state.

### 2. SQLite persistent

Persistent data must survive server restarts.

### 3. No automatic history pruning

Do not delete old data to solve performance problems.

### 4. No automatic memory forgetting

Future NPC memories remain persistent unless the project owner explicitly changes this rule.

### 5. Current state and history

Eventually use optimized current state alongside permanent historical records.

### 6. C# owns simulation

Future simulation logic belongs to deterministic/probabilistic C# systems.

### 7. LLM generates language

The LLM must not directly own authoritative simulation state.

### 8. Incremental development

Build one working checkpoint at a time.

### 9. Inspect before changing

The actual codebase is always more authoritative than the master prompt.

### 10. README after every Part/Sub-Part

Always update the README before committing completed work.

### 11. No giant implementation dumps

Do not implement multiple future Parts together.

### 12. No unnecessary rewrites

Preserve working systems.

---

# REQUIRED FINAL REPORT

When Part 03 is complete, report:

## 1. What Was Inspected

Summarize the existing persistence implementation.

## 2. What Changed

List the actual changes.

## 3. Persistence Architecture

Show the resulting architecture.

Example:

```text
Application
    ↓
Persistence abstraction
    ↓
Infrastructure
    ↓
EF Core
    ↓
SQLite
```

Only show components that actually exist.

## 4. Database

Report:

```text
SQLite:
EF Core:
Migrations:
WAL:
Transactions:
Indexes:
```

Use actual project state.

## 5. Tests

Show actual results:

```text
Server Build
Persistence Tests
Migration Test
Write/Read Test
Restart Persistence Test
```

Only say `PASS` if actually tested.

## 6. Files Changed

List the actual files modified/created/deleted.

## 7. README

Report:

```text
README updated: YES
```

and summarize what was added.

## 8. Git

Report:

```text
Commit: <actual commit>
Working tree: clean
```

Do not invent the commit hash.

## 9. Current Project Status

Use:

```text
01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
01F COMPLETE
02  COMPLETE
03  COMPLETE
```

## 10. NEXT

Report:

```text
NEXT: PART 04 — AUTHENTICATION & ACCOUNTS
```

Then STOP.

Do not begin Part 04 automatically.

---

# FINAL INSTRUCTION

Work ONLY on:

# PART 03 — PERSISTENCE

Inspect first.

Make the smallest reasonable changes.

Preserve existing functionality.

Protect existing data.

Do not create future gameplay systems.

Do not delete history.

Do not reset the database unnecessarily.

Update README.

Test everything.

Create the Git checkpoint.

Then STOP.