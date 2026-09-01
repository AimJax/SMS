# SOCIAL MEDIA SIMULATOR — NEXT DEVELOPMENT TASK

## CURRENT PROJECT STATE

We are continuing an existing Social Media Simulator project.

**Project location:**

```text
D:\SMS
```

The project has already completed:

```text
01A — Development Environment       COMPLETE
01B — Repository Foundation         COMPLETE
01C — ASP.NET Core Server           COMPLETE
01D — SQLite Foundation             COMPLETE
01E — Android Client Foundation     COMPLETE
```

Latest Git checkpoint:

```text
4d1e206 — Add Android client foundation
```

The working tree was clean at the last checkpoint.

The current architecture is already:

```text
Android Client
      ↓
HTTP REST
      ↓
ASP.NET Core Server
      ↓
SQLite
```

The Android client currently has working communication with:

```text
GET /api/health
```

The server health test works.

SQLite persistence works.

The Android project builds successfully.

The README has already been updated to document the completed 01A–01E work.

---

# CURRENT TASK

# PART 01F — FOUNDATION CHECKPOINT

This task is ONLY about verifying and formally checkpointing the complete foundation.

Do NOT start Part 02.

Do NOT implement accounts.

Do NOT implement authentication.

Do NOT implement NPCs.

Do NOT implement social graphs.

Do NOT implement posts.

Do NOT implement feeds.

Do NOT implement LLM integration.

Do NOT create the 10,000-account population.

Do NOT create the full database schema.

Do NOT add future systems just because they are mentioned in the master specification.

---

# IMPORTANT — INSPECT FIRST

Before changing anything:

1. Inspect the existing project at:

```text
D:\SMS
```

2. Inspect:

```text
README.md
```

3. Inspect the solution/project structure.

4. Inspect the current Git status.

5. Inspect the latest commit.

6. Inspect the existing Android client.

7. Inspect the existing ASP.NET Core server.

8. Inspect the existing SQLite/EF Core implementation.

9. Determine exactly what has already been implemented.

Do NOT recreate or duplicate anything that already exists.

Do NOT assume the project matches the master prompt perfectly.

The actual project files are the source of truth.

---

# FOUNDATION CHECKLIST

Verify the complete chain:

```text
Android Application
       ↓
HTTP Request
       ↓
ASP.NET Core
       ↓
Application/Service Layer
       ↓
SQLite / EF Core
       ↓
Persistent Data
       ↓
HTTP Response
       ↓
Android UI
```

Verify the following.

## 1. Repository

Confirm:

```text
D:\SMS
```

contains the expected solution/project structure.

Confirm Git is initialized.

Confirm the working tree state.

---

## 2. Server

Verify the ASP.NET Core server builds.

Verify:

```text
GET /api/health
```

returns a successful response.

Confirm the server can start normally.

---

## 3. SQLite

Verify SQLite persistence still works.

Perform a safe persistence test.

Confirm:

```text
Write
 ↓
Read
 ↓
Restart server
 ↓
Read again
```

still works.

Do NOT delete existing persistent data.

Do NOT reset the database unnecessarily.

Do NOT recreate the database if the existing database works.

---

## 4. Android

Verify the Android project builds successfully.

Verify the Android application can communicate with the backend.

Verify the existing server-status functionality.

Confirm:

```text
ONLINE
```

is displayed when the backend is available.

Confirm appropriate failure behavior when the backend is unavailable.

Do not unnecessarily redesign the UI.

---

## 5. Configuration

Inspect the existing server URL configuration.

Confirm it is configurable rather than scattered throughout the codebase.

The Android client must not have hardcoded developer IP addresses scattered across multiple files.

Do not redesign the entire configuration architecture during this checkpoint.

Only fix genuine foundation problems.

---

# TESTING

Run the appropriate build and tests.

At minimum verify:

```text
Server build
Android build
Backend health
SQLite persistence
Android → Backend communication
Backend restart/reconnect
```

If existing automated tests exist, run them.

If there are no automated tests yet, do not build a giant testing framework solely for this checkpoint.

Use focused verification.

---

# IMPORTANT SCOPE RULE

This is a CHECKPOINT task.

The goal is to establish that:

```text
01A
 ↓
01B
 ↓
01C
 ↓
01D
 ↓
01E
 ↓
01F
```

forms a stable working foundation.

Do not expand the architecture unnecessarily.

Do not refactor working code without a concrete reason.

Do not introduce new frameworks merely because they might be useful later.

Do not implement future functionality.

---

# README REQUIREMENT

This is now a permanent project rule:

> **After completing every Part or Sub-Part, update `README.md` before creating the Git checkpoint.**

For this task, update:

```text
D:\SMS\README.md
```

The README must accurately reflect:

```text
01A — COMPLETE
01B — COMPLETE
01C — COMPLETE
01D — COMPLETE
01E — COMPLETE
01F — COMPLETE
```

Document the verified foundation.

Do not claim functionality that was not actually verified.

Include the current architecture and relevant technology stack.

Keep the README useful and concise.

Do not turn it into a copy of the entire master development specification.

---

# GIT CHECKPOINT

After all verification succeeds:

1. Check Git status.
2. Review the changes.
3. Add only the appropriate project files.
4. Create a checkpoint commit.

Use a clear commit message such as:

```text
Complete foundation checkpoint
```

or another accurate equivalent.

Then verify:

```text
git status
```

The working tree should be clean.

Verify the new commit exists.

---

# FAILURE RULE

If something fails:

```text
STOP
 ↓
Investigate
 ↓
Fix
 ↓
Build
 ↓
Test
 ↓
Verify
```

Do not move to Part 02 while the foundation is broken.

Do not hide or ignore errors.

Do not claim PASS unless it was actually tested.

---

# DATABASE SAFETY

This project requires permanent historical persistence.

Never introduce automatic pruning.

Never introduce memory deletion.

Never delete historical records as an optimization.

Do not execute destructive database operations merely to make the checkpoint easier.

Avoid:

```sql
DROP TABLE
DELETE FROM ...
TRUNCATE
```

unless explicitly required and explicitly authorized.

At this stage there should be no reason for destructive database operations.

---

# ARCHITECTURAL RULES TO REMEMBER

These remain permanent:

### Server authoritative

The Android client is never authoritative over world state.

### SQLite persistent

Database persistence must survive server restarts.

### No automatic history pruning

Performance problems must NOT be solved by deleting history.

### LLM is not simulation

The future LLM generates language.

C# will control authoritative simulation state.

### Incremental development

One working checkpoint at a time.

### No giant implementation dumps

Do not implement multiple future Parts together.

### Reuse existing systems

Inspect before creating.

### Minimal changes

Change only what this checkpoint requires.

---

# REQUIRED FINAL REPORT

When finished, report:

## 1. What Was Verified

List the foundation components.

## 2. Tests

Show each test and its result.

Example:

```text
Server Build              PASS
Android Build             PASS
Health Endpoint           PASS
SQLite Persistence        PASS
Android → Server          PASS
Reconnect                 PASS
```

Only report PASS if actually verified.

## 3. Files Changed

List the actual files changed.

## 4. README

Confirm it was updated.

## 5. Git

Show:

```text
Commit:
Working tree:
```

## 6. Current Project Status

Show:

```text
01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
01F COMPLETE
```

## 7. NEXT

State:

```text
NEXT: PART 02 — BACKEND ARCHITECTURE
```

Then STOP.

Do not begin Part 02 automatically.

# END OF TASK

Complete ONLY 01F.