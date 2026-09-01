# SOCIAL MEDIA SIMULATOR — PART 11 DEVELOPMENT PROMPT
## NPC BACKGROUND SIMULATION (AUTONOMOUS TICK PROCESSING)

You are continuing development of the **Social Media Simulator** from the existing project.

**DO NOT restart, redesign, or replace the existing architecture.**

You must inspect the current repository first and build directly on everything already implemented.

---

# CURRENT PROJECT CHECKPOINT

Completed:

```text
01A  Development Environment       COMPLETE
01B  Repository Foundation         COMPLETE
01C  ASP.NET Core Server           COMPLETE
01D  SQLite Foundation             COMPLETE
01E  Android Client Foundation     COMPLETE
01F  Foundation Checkpoint         COMPLETE
02   Backend Architecture          COMPLETE
03   Persistence                   COMPLETE
04   Accounts & Authentication     COMPLETE
05   Social Graph                  COMPLETE
06   Posts & Engagement            COMPLETE
07   Feed & Timeline               COMPLETE
08   NPC Simulator Foundation      COMPLETE
09   NPC Population Generation     COMPLETE
10   NPC Behavior Simulation       COMPLETE
```

Latest commit:

```text
eb17783 — Implement NPC behavior simulation (Part 10)
```

Remote:

```text
origin/main
```

Working tree should currently be clean.

Repository:

```text
https://github.com/AimJax/SMS.git
```

The existing backend already contains:

- ASP.NET Core server, layered architecture, EF Core, SQLite
- Accounts, Profiles, Authentication, JWT
- Follow relationships, Blocks, Mutes
- Posts, Likes, Comments, soft deletion
- Chronological feed with pagination
- NPC profiles, personalities (Big Five), interests
- `NpcSimulationService` — tick processing
- `NpcBehaviorService` — decision-making, content relevance, content generation
- `NpcDecisionService`, `ContentRelevanceService`, `ContentGeneratorService`
- NPC action history (`NpcAction`)
- 103 passing automated tests

Explicitly **NOT yet implemented** (per Part 10 session report):

```text
LLM Integration (Ollama/Qwen)
NPC-specific admin API
Background/hosted-service tick execution
NPC-to-NPC social graph (follows/relationships)
```

Part 11 addresses **background/hosted-service tick execution** specifically.

The project is a **standalone online Social Media Simulator**, not a Life Simulator.

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

The server owns all simulation state and timing. NPC ticks must never depend on a client request arriving. The simulation must run **on its own**, independent of whether anyone is using the API.

## Layered architecture

Continue using the existing separation between:

```text
API
Application
Domain
Infrastructure
Contracts
```

The background execution mechanism belongs in **Infrastructure/Application**, not in a controller, and not in `Program.cs` as inline logic.

## Persistence

All NPC action results (posts, likes, comments, follows, action history) must persist through the existing EF Core + SQLite stack. Do not introduce a second data path.

## Performance

The simulation must eventually support hundreds to thousands of NPC accounts running continuously in the background without blocking the API, without exhausting the connection pool, and without unbounded memory growth per tick.

---

# PART 11 OBJECTIVE

Turn the existing `NpcSimulationService` / `NpcBehaviorService` tick pipeline (currently only invoked manually or from tests) into a **real, autonomous, continuously running background process** hosted inside the ASP.NET Core server.

Today, ticks only happen when something explicitly calls the simulation service (e.g., a test or a manual trigger). Part 11 makes the simulation **self-sustaining**: once the server is running, the world keeps moving on its own, tick after tick, without any external caller.

Do NOT implement NPC-to-NPC social graph in this part.

Do NOT implement LLM/Ollama/Qwen integration in this part.

Do NOT implement a full admin dashboard.

Those remain future parts.

---

# PART 11 — REQUIRED FEATURES

## 1. Hosted background service

Implement a proper ASP.NET Core background execution mechanism (e.g., `BackgroundService` / `IHostedService`) that:

- Starts automatically when the server starts.
- Runs continuously for the lifetime of the application.
- Stops gracefully when the server shuts down (respect `CancellationToken`, no abrupt kills, no orphaned work).
- Does not block server startup or the API from serving requests.

Inspect the existing `NpcSimulationService` / `INpcSimulationService` first. Reuse it as the thing the hosted service calls — do not duplicate tick logic inside the hosted service itself. The hosted service's only job is **scheduling and lifetime**, not simulation logic.

---

## 2. Tick interval configuration

The interval between ticks must be configurable, not hardcoded.

```text
appsettings.json → Simulation:TickIntervalSeconds (or equivalent)
```

Provide a sensible development default (document the chosen value and why). Do not choose an interval so aggressive that it saturates SQLite or the CPU with the target NPC population size, and do not choose one so slow that the simulation feels inert during development/testing.

---

## 3. Scoped service resolution per tick

`BackgroundService` instances are singletons, but EF Core `DbContext` and most application services are scoped. Each tick must:

- Create a new DI scope.
- Resolve `INpcSimulationService` (and anything else needed) from that scope.
- Dispose the scope after the tick completes.

Do not capture a scoped `DbContext` at startup and reuse it forever. Do not resolve scoped services directly from the root service provider.

---

## 4. Overlap prevention

A tick must not start while the previous tick is still running (e.g., if a tick takes longer than the configured interval due to a large NPC population).

Implement a clear strategy — for example, skip the next scheduled tick if the previous one hasn't finished, and log that it was skipped. Document the chosen strategy. Do not allow two ticks to run concurrently against the same data.

---

## 5. Failure isolation

If a single tick throws an exception:

- The background service must NOT crash or stop permanently.
- Log the error with enough detail to diagnose it.
- Continue scheduling future ticks normally.

A bug in one NPC's decision logic must not take down the entire simulation loop or the server.

---

## 6. Start/stop/pause control (server-side, minimal)

Provide a minimal, authenticated way to observe and control the background simulation for development/ops purposes. At minimum:

```http
GET  /api/admin/simulation/status
POST /api/admin/simulation/pause
POST /api/admin/simulation/resume
```

- `status` should report whether the loop is running, the configured interval, the last tick time, the last tick duration, and the last tick's NPC action count (or similar useful metrics already available from `NpcSimulationService`).
- `pause`/`resume` should stop/start actual tick execution without restarting the whole server.
- These endpoints must require authentication. Decide (and document) whether they require an elevated/admin role or are simply authenticated — use the existing authentication/authorization conventions already in the project. Do not build a new auth system for this.

Do not build a full admin panel. Do not build NPC management CRUD endpoints. This is observability/control for the background loop only.

---

## 7. Logging

Use the project's existing logging conventions (e.g., `ILogger<T>`). At minimum log:

```text
Tick started (tick number / timestamp)
Tick completed (duration, NPC actions performed)
Tick skipped (overlap)
Tick failed (exception, but loop continues)
Service starting
Service stopping (graceful shutdown)
```

Avoid excessive per-NPC log spam at default log level; use a lower log level (e.g., Debug) for per-NPC detail if needed.

---

## 8. Graceful shutdown

When the server receives a shutdown signal:

- The hosted service must observe the `CancellationToken` promptly.
- An in-progress tick should be allowed to either finish cleanly or abort safely — no half-written, inconsistent database state.
- The server must actually exit in a reasonable time, not hang indefinitely waiting on the simulation loop.

Verify this explicitly during testing (see Section 15).

---

## 9. Database performance under continuous load

With the loop now running continuously:

- Reuse existing indexes/query patterns from Parts 07–10; only add new indexes if profiling/inspection actually justifies them.
- Ensure `DbContext` scopes are short-lived (created and disposed per tick, not held open across ticks).
- Confirm SQLite WAL mode (established in Part 01D) is still appropriate under continuous write load; document if any adjustment was needed.
- Avoid holding long-running transactions across an entire tick if it can be reasonably scoped smaller, without breaking the consistency the existing `NpcSimulationService` relies on.

Do not introduce a separate database, cache layer, or message queue for this. Keep the existing SQLite + EF Core stack.

---

## 10. Configuration safety

The background simulation must be easy to disable entirely for certain environments (e.g., automated test runs that don't want a live ticking loop interfering).

```text
Simulation:Enabled (true/false)
```

If disabled, the hosted service should start but immediately no-op (or not schedule ticks), and this must not break server startup or existing endpoints. Document the default value and why.

---

## 11. Tests

Add tests appropriate to this part. At minimum verify:

### Hosted service lifecycle

```text
Server starts → background service starts
Server stops → background service stops gracefully within a reasonable timeout
```

### Tick execution

```text
Given Simulation:Enabled = true and a short test interval,
after waiting slightly longer than one interval,
at least one tick has executed
(verified via status endpoint or persisted NpcAction records)
```

### Overlap prevention

```text
Simulate/force a slow tick
Verify a second tick does not start concurrently
Verify the skip is logged / reflected in status
```

### Failure isolation

```text
Force a tick to throw
Verify the loop logs the failure and continues on the next interval
Verify the server/API remains responsive throughout
```

### Pause/resume

```text
POST /api/admin/simulation/pause → ticks stop occurring
POST /api/admin/simulation/resume → ticks resume
GET  /api/admin/simulation/status reflects the correct state at each stage
```

### Disabled configuration

```text
Simulation:Enabled = false
Server starts successfully
No ticks occur
Existing endpoints (health, feed, posts, auth) still work normally
```

### Regression

```text
/api/health still returns 200 / {"status":"ok"}
Existing feed, posts, accounts, social graph tests still pass
```

### Persistence

```text
Let the loop run for a few ticks
Stop the server
Restart the server
Verify NPC actions from before the restart are still present
Verify the loop resumes ticking after restart
```

---

## 12. Database migration

Only create a migration if this part actually requires schema changes (for example, if you choose to persist simulation-loop metadata such as last-tick-time in the database rather than in memory). Do NOT create a migration for the sake of it. If in-memory status tracking is sufficient and consistent with the project's conventions, prefer that and document the tradeoff (status resets on restart is acceptable if documented).

---

## 13. Android

Part 11 is a backend-only foundation task.

Do NOT build any Android UI for simulation status/control in this part.

---

## 14. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

Document:

- Part 11 completion
- Background service architecture and how it relates to `NpcSimulationService`
- Tick interval configuration and chosen default
- Overlap-prevention strategy
- Failure-isolation behavior
- Graceful shutdown behavior
- New admin endpoints (status/pause/resume) and their auth requirements
- Logging behavior
- Enabled/disabled configuration switch
- Tests performed and results
- Current project status
- Next planned part

Do not leave the README describing the simulation as manually-triggered-only once this part is complete.

---

## 15. Git

After implementation and verification:

1. Inspect `git status`.
2. Review changed files.
3. Ensure no generated junk, logs, or unrelated files are committed.
4. Commit the completed work.

Suggested commit message:

```text
Implement NPC background simulation loop (Part 11)
```

Push to `origin/main`. Only report success if the push actually succeeds.

---

## 16. DO NOT IMPLEMENT YET

Do NOT implement the following in Part 11:

```text
NPC-to-NPC social graph (NPC follows/relationships)
LLM / Ollama / Qwen content generation
Full admin dashboard / NPC management UI
Multi-tier simulation (active vs. dormant NPC pools)
WebSocket live updates to clients
Notifications
Trending/virality mechanics
Android simulation UI
```

Those belong to later parts.

---

## 17. DEVELOPMENT PROCESS

Before changing anything:

1. Inspect the repository.
2. Inspect `NpcSimulationService` / `INpcSimulationService`.
3. Inspect `NpcBehaviorService` and its dependencies.
4. Inspect `Program.cs` / `ServiceCollectionExtensions.cs` for existing DI/service registration conventions.
5. Inspect `appsettings.json` / `appsettings.Development.json` for existing configuration conventions.
6. Inspect existing authentication/authorization conventions for any admin-style endpoints.
7. Inspect existing logging conventions.
8. Inspect existing tests for how services/hosted behavior are currently tested.
9. Inspect the README.

Then implement Part 11.

Do not assume a file does not exist merely because this prompt says to create it. Reuse existing functionality wherever appropriate. Do not duplicate simulation logic. Do not perform unrelated refactoring.

---

## 18. QUALITY REQUIREMENTS

The implementation must be:

- correct
- persistent
- server-authoritative
- resilient to individual tick failures
- non-blocking to the API
- configurable
- testable
- observable (status endpoint)
- maintainable
- compatible with the existing architecture

---

## 19. FINAL VERIFICATION

Before declaring Part 11 complete, verify:

```text
Server builds
Background service starts automatically with the server
Ticks execute on the configured interval
Overlap prevention works
Failure isolation works (loop survives a bad tick)
Graceful shutdown works within a reasonable timeout
Pause/resume endpoints work and require authentication
Status endpoint reports accurate data
Disabling simulation via config works and doesn't break the server
Existing endpoints (health, auth, feed, posts, social graph) still work
Persistence across restart still works, including for NPC actions
README updated
Git commit created
Git push succeeds
Working tree clean
```

---

## 20. FINAL SESSION REPORT

When finished, provide a complete session report in this structure:

```text
# PART 11 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Background Service Architecture
...

## 5. Configuration
...

## 6. API Endpoints
...

## 7. Overlap Prevention & Failure Isolation
...

## 8. Tests
...

## 9. README
Updated: YES
...

## 10. Git
Commit: ...
Push: ...
Working tree: ...

## 11. Current Project Status

01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
01F COMPLETE
02  COMPLETE
03  COMPLETE
04  COMPLETE
05  COMPLETE
06  COMPLETE
07  COMPLETE
08  COMPLETE
09  COMPLETE
10  COMPLETE
11  COMPLETE

## 12. Intentionally Not Implemented
- NPC-to-NPC social graph
- LLM / Ollama / Qwen integration
- Full admin dashboard
- Android simulation UI

## 13. NEXT

NEXT: PART 12 — ...
```

Do not claim completion until the implementation and verification have actually succeeded.

**STOP after completing Part 11 and reporting the session log.**
