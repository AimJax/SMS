# SOCIAL MEDIA SIMULATOR — PART 08 DEVELOPMENT PROMPT
## NPC SIMULATOR FOUNDATION

You are continuing development of the existing **Social Media Simulator** project.

**DO NOT restart the project. DO NOT redesign the existing architecture. DO NOT replace working systems.**

Build directly on the current repository and all completed Parts 01–07.

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
```

Latest commit:

```text
d85339f — Implement feed and timeline (Part 07)
```

Remote:

```text
origin/main
```

Part 07 established:

- authenticated feed
- chronological timeline
- cursor pagination
- followed-account filtering
- block filtering
- mute filtering
- soft-delete filtering
- engagement counts
- current-user like state

The project is now ready to begin building the simulated population.

---

# IMPORTANT PROJECT VISION

This is a **Social Media Simulator**.

It is NOT primarily:

- a Life Simulator
- a chatbot
- an NPC dialogue demo
- an AI benchmark
- a static fake social-media UI

The long-term goal is a living simulated social network where hundreds or thousands of accounts can:

- exist
- develop identities
- follow each other
- post
- like
- comment
- interact
- gain popularity
- lose popularity
- form relationships
- react to events
- participate in trends
- create drama
- consume content
- influence other users
- eventually generate content through controlled AI systems

The player is one participant in this world.

The NPC simulation is therefore a core gameplay system.

---

# PART 08 OBJECTIVE

Build the **foundation of the NPC simulation system**.

Do NOT attempt to create fully intelligent AI NPCs yet.

This part establishes the deterministic simulation framework and persistent NPC state that later parts will use.

The NPC system must be designed so that future behavior systems can be added without rewriting the entire architecture.

---

# CORE DESIGN PRINCIPLE

Separate:

```text
NPC identity
NPC persistent state
NPC simulation scheduling
NPC decision making
NPC actions
NPC content generation
```

Do NOT combine all of these into one giant NPC class.

The long-term architecture should allow:

```text
NPC
 │
 ├── Identity
 ├── Personality
 ├── Interests
 ├── Social Graph
 ├── State
 ├── Simulation Schedule
 ├── Decision System
 └── Actions
```

Part 08 only implements the foundation required for these future systems.

---

# 1. NPC ACCOUNT TYPES

The simulator needs account categories.

Do not make every account identical.

Introduce an appropriate account-type system if the existing `AccountType` enum cannot already support the required categories.

At minimum support categories conceptually equivalent to:

```text
RegularUser
Influencer
Celebrity
NewsOutlet
OfficialOrganization
Business
Creator
PublicFigure
```

You may use the existing `AccountType` where appropriate.

**Do not duplicate AccountType if Part 04 already provides a suitable enum.**

Inspect the existing implementation first.

The architecture must allow more account types later.

---

# 2. NPC IDENTIFICATION

NPCs must be distinguishable from human/player-controlled accounts.

Inspect the existing `Account` model first.

If there is no suitable mechanism, add an appropriate representation such as:

```text
IsNpc
```

or an equivalent account classification.

Do not rely on usernames such as:

```text
npc_001
npc_002
```

to determine whether an account is an NPC.

NPC status must be explicit and persistent.

---

# 3. NPC PROFILE

An NPC needs persistent identity information.

Build an appropriate NPC/domain model without duplicating existing Account/Profile data unnecessarily.

Potential attributes include:

```text
NpcId
AccountId
CreatedAt
Active
```

Use GUID identity where consistent with the existing architecture.

The NPC record should reference the existing Account.

Conceptually:

```text
Account
   │
   └── NPC metadata
          │
          ├── AccountType
          ├── Active
          └── Simulation state
```

Do not create a second Account system.

---

# 4. NPC SIMULATION STATE

Create persistent state for NPC simulation.

The exact schema should be determined after inspecting the existing project.

The foundation should support state such as:

```text
NpcId
LastSimulatedAt
NextSimulationAt
SimulationVersion
IsActive
```

The purpose is to know:

- when an NPC was last processed
- when it should next be processed
- whether it is active
- which simulation version/state it belongs to

Do not implement a complex scheduler yet.

---

# 5. NPC PERSONALITY FOUNDATION

Create the foundation for persistent NPC personality.

Do NOT implement a full psychology engine.

Do NOT use an LLM for personality decisions yet.

Establish a small set of numerical personality traits that future behavior systems can use.

For example:

```text
Openness
Extraversion
Agreeableness
Conscientiousness
Neuroticism
```

or an equivalent compact trait model.

Each value should have a documented range.

Prefer a normalized range such as:

```text
0.0 → 1.0
```

The important requirement is consistency.

Personality must be:

- persistent
- deterministic
- editable by future systems
- independent of the LLM

Do not regenerate personality every simulation tick.

An NPC's personality should remain stable unless a later system deliberately changes it.

---

# 6. NPC INTERESTS FOUNDATION

Create the foundation for NPC interests.

An NPC should eventually have interests such as:

```text
Gaming
Politics
Sports
Technology
Music
Movies
Fashion
Food
LocalNews
Science
```

Do not hardcode a tiny closed list if the architecture can support extensible interests.

A suitable structure could be:

```text
NpcInterest
    NpcId
    InterestKey
    Strength
```

where:

```text
Strength = 0.0 → 1.0
```

Interests must persist in SQLite.

Do not use an in-memory dictionary as the authoritative source.

---

# 7. NPC STATE VS PERSONALITY VS INTERESTS

Keep these concepts separate.

### Personality

Stable behavioral tendencies.

Example:

```text
Extraversion = 0.82
Agreeableness = 0.35
```

### Interests

What the NPC cares about.

Example:

```text
Gaming = 0.91
Football = 0.62
Politics = 0.18
```

### State

Temporary simulation condition.

Example:

```text
LastSimulatedAt
NextSimulationAt
CurrentActivity
Energy
```

Do not mix these concepts together.

---

# 8. NPC ACTIVITY STATE

Create only the minimum activity/state foundation required for future simulation.

Potential states:

```text
Idle
Browsing
Posting
Reading
Engaging
Offline
```

Use an enum or equivalent strongly typed representation.

Do not implement detailed schedules yet.

Do not build a real-world life simulation.

This is a **social-media activity simulator**.

The NPC's activity exists primarily to determine what it does on the platform.

---

# 9. NPC SIMULATION SERVICE

Create a dedicated simulation abstraction.

For example:

```text
INpcSimulationService
NpcSimulationService
```

Use the existing Application-layer conventions.

The service should eventually become responsible for:

- finding NPCs due for simulation
- loading persistent NPC state
- determining simulation work
- updating simulation state
- executing future NPC actions

For Part 08, implement only the basic lifecycle.

Do NOT build sophisticated behavior decisions yet.

---

# 10. SIMULATION TICK

Create a basic simulation tick mechanism.

The system should be capable of processing NPCs that are due for simulation.

Conceptually:

```text
Simulation Tick
      ↓
Find NPCs where NextSimulationAt <= now
      ↓
Process NPC
      ↓
Update LastSimulatedAt
      ↓
Calculate NextSimulationAt
```

Do not run every NPC every frame.

This is a backend simulation, not a Unity Update loop.

Do not create:

```text
foreach NPC
    every frame
```

That would scale poorly.

---

# 11. SIMULATION INTERVAL

Use a sensible initial simulation interval.

It can be something simple such as:

```text
5–30 seconds
```

but the architecture must allow different NPCs to have different future activity frequencies.

Do not hardcode the scheduling system so deeply that it cannot later support:

- highly active users
- casual users
- inactive users
- celebrities
- news accounts
- automated accounts

The exact initial interval is less important than creating a flexible foundation.

---

# 12. DETERMINISTIC SIMULATION

The NPC foundation should support deterministic behavior.

Do not use uncontrolled randomness everywhere.

Use a controllable random source if randomness is needed.

Future simulation should be reproducible when given the same:

```text
world state
NPC state
simulation seed
simulation time
```

Do not introduce randomness that makes debugging impossible.

---

# 13. NPC ACTION ABSTRACTION

Prepare an abstraction for future NPC actions.

For example:

```text
NpcAction
```

or an equivalent design.

Future actions will include:

```text
ViewPost
LikePost
Comment
Follow
Unfollow
Mute
Block
CreatePost
Reply
Search
```

Part 08 does NOT need to implement all of these.

The goal is to establish a clean extension point.

Do not create dozens of empty classes solely for future features.

A minimal extensible action representation is sufficient.

---

# 14. NPC GENERATION

Create the ability to create NPC accounts through a controlled service.

For example:

```text
INpcService
NpcService
```

or an equivalent existing architecture.

NPC creation should create the required persistent records:

```text
Account
Profile
NPC metadata
Personality
Interests
Simulation state
```

Use the existing AccountService where appropriate.

Do not duplicate password hashing, username normalization, account persistence, etc.

---

# 15. NPC USERNAME GENERATION

Create a deterministic or controlled username generation mechanism.

NPC usernames must:

- be unique
- be valid according to existing account rules
- not collide with player accounts

Do not depend on random generation without collision handling.

The exact naming style is not important yet.

Do not create hundreds of meaningless accounts automatically during application startup.

NPC population generation will be implemented in a later part.

---

# 16. NPC INITIAL PERSONALITY

When an NPC is created, assign its initial personality once.

Use controlled randomness.

For example:

```text
seed → deterministic trait values
```

Do not regenerate personality every time the NPC is loaded.

Personality should be stored in the database.

---

# 17. NPC INITIAL INTERESTS

When an NPC is created, assign an initial interest profile.

The interests should be influenced by account type where appropriate.

For example:

```text
NewsOutlet
→ LocalNews
→ Politics
→ WorldNews

GamingCreator
→ Gaming
→ Technology
→ Streaming
```

Do not implement sophisticated cultural modeling yet.

The goal is simply to ensure NPCs are not identical clones.

---

# 18. NPC ACCOUNT STATUS

NPC simulation must respect existing Account status rules.

Inactive, banned, suspended, or otherwise unavailable accounts should not perform simulation actions.

Inspect the existing `AccountStatus` implementation.

Do not invent conflicting status rules.

---

# 19. SECURITY

NPC simulation must not bypass core domain rules.

NPC actions must eventually use:

- Account rules
- SocialGraph rules
- Post rules
- Engagement rules

Do not create a privileged "god mode" service that directly modifies database tables in ways normal users could not.

Part 08 does not yet need all actions, but the architecture must not make future rule enforcement impossible.

---

# 20. DATABASE DESIGN

Inspect the existing schema before making changes.

Add only the tables required for Part 08.

Likely concepts include:

```text
NPC metadata
NPC personality
NPC interests
NPC simulation state
```

Use EF Core entity configurations consistent with Parts 03–07.

Add appropriate indexes.

Potential indexes:

```text
AccountId
NextSimulationAt
NpcId
InterestKey
```

Only add indexes that match actual access patterns.

Do not blindly index every field.

---

# 21. MIGRATIONS

Create an EF Core migration for the new NPC schema.

Verify:

```text
migration creation
migration application
database startup
```

Do not manually edit SQLite database files.

---

# 22. API

Part 08 is primarily a backend simulation foundation.

Do NOT expose a public endpoint that lets arbitrary users directly control NPC simulation.

If administrative/debug endpoints are useful, keep them explicitly separated from normal player APIs.

A development-only endpoint may be created if the existing architecture has a suitable pattern, but do not compromise production security.

---

# 23. TESTS

Add tests covering the foundation.

At minimum:

### NPC creation

```text
Create NPC
→ Account exists
→ Profile exists
→ NPC metadata exists
→ Personality exists
→ Interests exist
→ Simulation state exists
```

### NPC identification

```text
NPC account is explicitly marked as NPC
Normal player account is not treated as NPC
```

### Personality persistence

```text
Create NPC
→ record traits
→ reload NPC
→ traits unchanged
```

### Interest persistence

```text
Create NPC
→ assign interests
→ restart/reload
→ interests preserved
```

### Simulation scheduling

```text
NPC due for simulation
→ processed
→ LastSimulatedAt updated
→ NextSimulationAt updated
```

### Future NPC

```text
NPC with NextSimulationAt in the future
→ not processed yet
```

### Inactive NPC

```text
Inactive NPC
→ simulation does not process it
```

### Account status

Verify simulation respects existing account status rules.

### Determinism

Where deterministic generation is implemented:

```text
same seed + same inputs
→ same generated personality/interests
```

### Persistence

Restart the application/database and verify NPC state remains intact.

---

# 24. PERFORMANCE REQUIREMENTS

The simulator must eventually support **hundreds to thousands of NPCs**.

Do not design Part 08 around only 10–20 NPCs.

Avoid:

```text
load every NPC
load all personality
load all interests
process everything every tick
```

Prefer querying only NPCs due for processing.

Use:

```text
NextSimulationAt <= currentTime
```

as the primary scheduling filter.

Do not perform unnecessary work for inactive NPCs.

---

# 25. NO LLM YET

Do NOT integrate Ollama into NPC behavior in Part 08.

Do not generate NPC posts with Qwen yet.

Do not ask an LLM to decide every NPC action.

That would be expensive and unnecessary at this foundation stage.

The future architecture should allow LLM-based content/decision modules to be added selectively.

For example:

```text
Deterministic Simulation
        ↓
Decision
        ↓
Optional AI augmentation
```

not:

```text
Every NPC
   ↓
LLM call
   ↓
Every tick
```

---

# 26. NO MASS POPULATION YET

Do NOT automatically generate the full population in Part 08.

Do not create:

```text
1000 NPCs
```

during startup.

Part 08 establishes the NPC architecture.

A later population-generation part will create the large population.

---

# 27. NO FEED ALGORITHM YET

Do not modify the chronological feed into:

- recommendation feed
- popularity ranking
- AI ranking
- trending ranking

Part 07's feed remains chronological.

Future NPCs will consume the feed through later simulation systems.

---

# 28. ANDROID

Do not build a major Android UI for NPCs.

NPC simulation is backend infrastructure.

Only make Android changes if absolutely required to keep the existing project compiling or architecturally consistent.

---

# 29. README — REQUIRED

At the end of Part 08, **UPDATE `README.md`**.

This is mandatory.

Document:

- Part 08 completion
- NPC architecture
- NPC identity
- account types
- NPC metadata
- personality model
- interest model
- simulation state
- simulation tick
- scheduling
- deterministic generation
- NPC creation
- database schema
- migrations
- tests
- performance considerations
- what is intentionally NOT implemented yet
- current project status
- next planned part

Make sure the README accurately reflects the actual implementation.

Do not document features that were not implemented.

---

# 30. GIT

After implementation:

1. Inspect git status.
2. Review changed files.
3. Remove unrelated/generated files from staging.
4. Build the server.
5. Run all relevant tests.
6. Verify the migration.
7. Update README.
8. Commit the completed Part 08 work.
9. Push to `origin/main`.

Suggested commit:

```text
Implement NPC simulator foundation (Part 08)
```

Only report push success if it actually succeeded.

The working tree should be clean after completion.

---

# 31. DEVELOPMENT PROCESS

Before modifying anything:

1. Inspect the repository.
2. Inspect Account and Profile entities.
3. Inspect AccountType and AccountStatus.
4. Inspect AccountService.
5. Inspect SocialGraphService.
6. Inspect PostService.
7. Inspect FeedService.
8. Inspect AppDbContext.
9. Inspect all entity configurations.
10. Inspect migrations.
11. Inspect dependency injection.
12. Inspect existing test/verification conventions.
13. Inspect README.

Then implement Part 08.

Do not assume the architecture from this prompt exactly matches the current files.

The repository is the source of truth.

---

# 32. IMPORTANT — PRESERVE EXISTING SYSTEMS

Do not break:

```text
Authentication
Accounts
Profiles
Social Graph
Posts
Likes
Comments
Feed
Persistence
Pagination
Soft deletion
```

After implementation, re-run relevant existing tests.

Part 08 must be additive.

---

# 33. FINAL VERIFICATION

Before declaring Part 08 complete, verify:

```text
Server builds
Existing Parts 01–07 still work
NPC creation works
NPC identity persists
NPC account type works
Personality persists
Interests persist
Simulation state persists
Scheduling works
Due NPCs are processed
Future NPCs are skipped
Inactive NPCs are skipped
Account status rules are respected
Deterministic generation works where implemented
Database migration works
Persistence survives restart
README updated
Git commit created
Git push succeeds
Working tree clean
```

---

# 34. FINAL SESSION REPORT

When finished, provide a complete report using this structure:

```text
# PART 08 — COMPLETE

## 1. What Was Inspected

...

## 2. What Already Existed

...

## 3. What Changed

...

## 4. NPC Architecture

...

## 5. Database Changes

...

## 6. API / Internal Services

...

## 7. Simulation Scheduling

...

## 8. Personality & Interests

...

## 9. Tests

...

## 10. README

Updated: YES

...

## 11. Git

Commit: ...
Push: ...
Working tree: ...

## 12. Current Project Status

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

## 13. Intentionally Not Implemented

...

## 14. NEXT

NEXT: PART 09 — NPC POPULATION GENERATION
```

Do not claim completion until everything has actually been implemented and verified.

**STOP after completing Part 08 and reporting the session log.**