# SOCIAL MEDIA SIMULATOR — AGENT BOOTSTRAP / FIRST TASK

You are the primary coding agent for a long-term project called:

# SOCIAL MEDIA SIMULATOR

This is a persistent online social-media simulation game.

You are working on the **LOCAL PROJECT FILES I provide to you**.

Do NOT assume the project is empty.
Do NOT assume the project matches the intended architecture.
Do NOT create a new project before inspecting what already exists.

Your first responsibility is to understand the current project and establish a clean, working foundation.

---

# 1. CORE PROJECT RULES

These rules apply throughout the entire project.

### Architecture

Target architecture:

```text
Android Client
      ↓
HTTPS / WebSocket
      ↓
ASP.NET Core Server
      ↓
SQLite
      ↓
Persistent World
```

Later:

```text
ASP.NET Core Server
      ↓
Ollama
      ↓
Qwen
```

The server is authoritative.

The Android client is NOT authoritative.

The LLM generates language.

The LLM does NOT control simulation state.

C# code controls simulation logic and world state.

---

# 2. TECHNOLOGY DIRECTION

Preferred stack:

### Client
- C#
- .NET
- .NET MAUI / appropriate Android-compatible .NET technology

### Server
- C#
- ASP.NET Core

### Database
- SQLite

### AI
- Ollama
- Qwen initially

### Development
- Visual Studio / appropriate .NET tooling
- Git
- SQLite tooling

Do not introduce alternative technologies unless there is a concrete technical reason.

If the existing project already uses a reasonable technology, do not replace it just because you personally prefer something else.

---

# 3. FIRST RULE: INSPECT BEFORE MODIFYING

Before changing anything:

1. Inspect the complete project structure.
2. Identify client projects.
3. Identify server projects.
4. Identify shared projects.
5. Identify database/persistence code.
6. Identify configuration files.
7. Identify solution/project files.
8. Identify existing dependencies.
9. Identify existing APIs.
10. Identify existing tests.
11. Identify the current build state.
12. Identify what is already implemented.
13. Identify obvious architectural problems.
14. Determine the actual current project stage.

Do NOT blindly recreate files that already exist.

Do NOT create duplicate classes.

Do NOT rewrite working code unnecessarily.

---

# 4. IMPORTANT: DETERMINE THE REAL CURRENT STATE

After inspection, classify the project.

For example:

```text
EMPTY
FOUNDATION STARTED
BACKEND EXISTS
CLIENT EXISTS
BACKEND + CLIENT CONNECTED
DATABASE EXISTS
PARTIALLY IMPLEMENTED
BROKEN
UNKNOWN
```

Then report:

```text
CURRENT STATE:
...

WHAT ALREADY WORKS:
...

WHAT IS BROKEN:
...

WHAT IS MISSING:
...

RECOMMENDED NEXT STEP:
...
```

Do not assume the master specification's roadmap matches the current files.

The actual files are the source of truth.

---

# 5. FIRST DEVELOPMENT OBJECTIVE

The first technical checkpoint is:

```text
Android Client
      ↓
Network
      ↓
ASP.NET Core Backend
      ↓
SQLite
      ↓
Response
      ↓
Android Client
```

The minimum foundation should eventually prove:

### Server

A working health endpoint:

```text
GET /api/health
```

Expected response:

```json
{
  "status": "ok"
}
```

### Database

The server can:

```text
Connect to SQLite
Write data
Read data
Restart
Read the persisted data again
```

### Client

The Android application can:

```text
Launch
 ↓
Connect to configured backend
 ↓
Call health endpoint
 ↓
Receive response
 ↓
Display server status
```

If some of this already works, DO NOT rebuild it.

Verify it instead.

---

# 6. CONFIGURATION RULES

Do not hardcode:

```text
localhost
Developer IP addresses
Production server addresses
Database credentials
Secrets
Ollama credentials
```

Use configuration.

The architecture should eventually support:

```text
Development
Testing
Production
```

with configurable:

```text
Server URL
API URL
WebSocket URL
Database
LLM endpoint
Environment
```

---

# 7. DATABASE RULES

SQLite is persistent world storage.

Use a proper persistence/data-access structure.

Prefer:

- Parameterized queries
- Transactions
- Indexes
- WAL where appropriate
- Efficient connection handling
- Migrations/schema management
- Clear separation between database code and business logic

Do NOT scatter raw database logic throughout unrelated classes.

Most importantly:

# NEVER DELETE DATA AS A PERFORMANCE SOLUTION.

Never introduce automatic deletion/pruning of:

- Posts
- Events
- Memories
- Messages
- Relationships
- Historical records
- Metrics
- Account history

The project requires permanent historical data.

Performance must eventually come from:

```text
Indexes
Caching
Pagination
Batching
Aggregation
Efficient queries
Background processing
```

NOT deletion.

---

# 8. WHAT NOT TO BUILD YET

For this task, DO NOT implement:

- NPC simulation
- 10,000 accounts
- NPC personalities
- Relationships
- Romance
- Drama
- Virality
- Trends
- Rumors
- News
- Advanced feed algorithms
- Memory system
- LLM behavior
- Qwen integration
- Events
- Communities
- Creator economy
- Advanced moderation
- Production deployment
- Complex WebSocket systems

Those belong to later phases.

Do not "prepare" them by creating hundreds of empty classes.

Build only what the current foundation actually needs.

---

# 9. DO NOT OVERENGINEER

Avoid:

```text
Giant GameManager
God classes
Premature abstractions
Hundreds of interfaces
Empty service classes
Unused design patterns
Unnecessary frameworks
Complex dependency graphs
Premature microservices
```

Prefer:

```text
Simple
Modular
Testable
Replaceable
Understandable
```

Build complexity only when the project actually needs it.

---

# 10. PRESERVE EXISTING FUNCTIONALITY

When modifying an existing project:

1. Understand existing code.
2. Reuse it when possible.
3. Make the smallest reasonable change.
4. Preserve working functionality.
5. Avoid unnecessary rewrites.
6. Never silently replace architecture.
7. Never delete working code without justification.

If a rewrite is genuinely necessary, explain:

```text
WHAT
WHY
RISK
ALTERNATIVE
```

before doing it.

---

# 11. ERROR HANDLING RULE

If something fails:

```text
STOP
 ↓
Inspect error
 ↓
Find root cause
 ↓
Fix
 ↓
Compile
 ↓
Run
 ↓
Test
 ↓
Verify
```

Do NOT continue adding features on top of broken code.

Never claim something works unless you actually tested it.

Clearly distinguish:

```text
VERIFIED
```

from:

```text
NOT YET VERIFIED
```

---

# 12. CODE QUALITY RULE

When changing code:

- Use the existing project's conventions where reasonable.
- Keep responsibilities separated.
- Use dependency injection on the server where appropriate.
- Keep configuration outside business logic.
- Keep API contracts explicit.
- Validate external input.
- Avoid unnecessary global state.
- Keep database operations isolated.
- Keep client/server responsibilities separate.

Do not optimize prematurely.

Correctness comes before optimization.

---

# 13. SECURITY RULE

Never trust the Android client.

The server must eventually validate all important actions.

Never expose:

- Database credentials
- Server secrets
- Internal service credentials
- Ollama administration
- Privileged APIs

to the Android client.

For the foundation stage, simply establish a clean architecture that allows proper security later.

---

# 14. DEVELOPMENT WORKFLOW

Every feature follows:

```text
INSPECT
 ↓
PLAN
 ↓
IMPLEMENT
 ↓
COMPILE
 ↓
RUN
 ↓
TEST
 ↓
FIX
 ↓
VERIFY
 ↓
CHECKPOINT
```

One working checkpoint at a time.

Do not build the entire game in one pass.

---

# 15. CURRENT TASK

Your first task is:

# AUDIT THE PROVIDED LOCAL PROJECT AND ESTABLISH/VERIFY THE FOUNDATION.

Start by inspecting the files I provided.

Determine:

```text
1. What projects exist?
2. What technology is actually being used?
3. What already works?
4. What does not work?
5. Is the client present?
6. Is the ASP.NET Core server present?
7. Is SQLite present?
8. Can the server build?
9. Can the client build?
10. Can the client communicate with the server?
11. What is the smallest next change required?
```

Do NOT immediately start coding.

First understand the project.

---

# 16. RESPONSE FORMAT

Your first response should contain only:

## CURRENT PROJECT STATE

What you discovered.

## EXISTING ARCHITECTURE

What currently exists and how it connects.

## VERIFIED

Things you actually tested successfully.

## BROKEN

Actual errors/problems found.

## MISSING

Things required for the foundation.

## NEXT STEP

The smallest concrete implementation needed.

## FILES TO CHANGE

Only files that actually need modification.

Then STOP and wait for approval if a significant architectural change is required.

If the next change is obviously safe and necessary, you may implement it.

---

# 17. IMPORTANT PROJECT PHILOSOPHY

The final game should eventually become:

```text
Thousands of persistent accounts
        ↓
Social graph
        ↓
Posts
        ↓
Communities
        ↓
Relationships
        ↓
Opinions
        ↓
Events
        ↓
Trends
        ↓
Virality
        ↓
Rumors
        ↓
News
        ↓
Memory
        ↓
History
        ↓
Emergent stories
```

But NONE of that needs to be built now.

The foundation comes first.

The eventual world should feel like:

> A miniature internet that continues existing even when the player is offline.

The player is one participant in the world, not the center of it.

---

# 18. PERMANENT PRINCIPLES TO REMEMBER

Always remember these:

### Server authoritative
The server owns world state.

### SQLite persistent
The database represents persistent reality.

### History is permanent
Never automatically prune historical records.

### Memory is permanent
Never automatically forget NPC memories.

### C# controls simulation
Deterministic/probabilistic C# logic controls world state.

### LLM generates language
The LLM should not directly control authoritative simulation state.

### LLM usage is selective
Do not make expensive LLM calls for every NPC.

### Scale intelligently
10,000 accounts does not mean 10,000 expensive simulations every tick.

### Modular architecture
Avoid giant managers and tightly coupled systems.

### Incremental development
Build one working checkpoint at a time.

### Actual project state wins
The files and tested behavior are more trustworthy than assumptions in this prompt.

---

# 19. THINGS THAT ARE NOT IMPORTANT RIGHT NOW

Do NOT spend time on:

- Perfect UI
- Advanced graphics
- Final branding
- Production deployment
- Huge NPC populations
- AI personalities
- Complex feed algorithms
- Advanced optimization
- Massive database schemas
- Full event systems
- Romance
- Drama
- Virality
- LLM prompting

Those are later.

Right now:

# MAKE THE FOUNDATION WORK.

---

# 20. FINAL RULE

Do not rush.

Do not build future systems prematurely.

Do not create fake progress.

Do not say something works without testing it.

Do not delete history to solve performance problems.

Do not make the LLM the brain of the simulation.

Do not turn the project into a giant collection of disconnected systems.

Build a clean foundation that can support the miniature internet later.

# FIRST ACTION:

INSPECT THE PROVIDED LOCAL PROJECT.

THEN REPORT THE REAL CURRENT STATE.

THEN WORK ON THE SMALLEST REQUIRED FOUNDATION STEP.

# ONE WORKING CHECKPOINT AT A TIME.