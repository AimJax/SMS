# SOCIAL MEDIA SIMULATOR — PART 05 DEVELOPMENT PROMPT

# PART 05 — SOCIAL GRAPH

## CURRENT PROJECT STATE

You are continuing development of the existing **Social Media Simulator** project.

Do NOT restart the project or rebuild previous parts.

The project has successfully completed:

```text
01A — Development Environment        ✅
01B — Repository Foundation          ✅
01C — ASP.NET Core Server             ✅
01D — SQLite Foundation               ✅
01E — Android Client                  ✅
01F — Foundation Checkpoint           ✅
02  — Backend Architecture            ✅
03  — Persistence                     ✅
04  — Accounts & Authentication       ✅
```

Latest Git checkpoint:

```text
Commit: 628d6de
Message: Implement accounts and authentication
Working tree: clean
```

Part 04 successfully implemented:

```text
Account
Profile
AccountHistory
AccountType
AccountStatus
Registration
Login
JWT authentication
Authorization
Protected /me endpoint
SQLite persistence
Database migration
```

Verified:

```text
Server Build              PASS
Health Endpoint           PASS
Registration              PASS
Login                     PASS
Duplicate Username        PASS
Authenticated /me         PASS
Persistence After Restart PASS
```

README was updated during Part 04.

The next task from the master development specification is:

# PART 05 — SOCIAL GRAPH

---

# 1. OBJECTIVE

Build the foundational social graph connecting accounts.

The goal is to make it possible for accounts to form connections such as:

```text
Alice
  ↓ follows
Bob
```

and:

```text
Alice
  ↓
Following → Bob, Sarah, Kevin

Followers → Emma, John, David
```

The social graph will eventually become the foundation for:

```text
Feeds
Recommendations
Relationships
Communities
Influence
Notifications
NPC behavior
Virality
Personalization
```

But those systems are NOT part of this task.

---

# 2. STRICT SCOPE

Implement ONLY the Part 05 social graph.

The intended functionality is:

```text
Follow
Unfollow
Followers
Following
Blocking
Muting
Mutual relationships
Social graph queries
```

Do not implement the later systems yet.

---

# 3. DO NOT BUILD THESE YET

Do NOT implement:

```text
Posts
Comments
Likes
Replies
Reposts
Quote-posts
Threads
Polls
Feeds
Feed ranking
Recommendations
NPC simulation
10,000 NPC generation
Personality
Relationships
Romance
Opinions
Communities
Events
Virality
Trends
Rumors
News
Memories
Ollama
Qwen
LLM queue
Creator economy
Advanced notifications
```

Even though the social graph will eventually support these systems, do not implement them now.

---

# 4. FIRST — INSPECT EVERYTHING

Before making changes:

1. Inspect the current project structure.
2. Inspect Account entity.
3. Inspect Profile entity.
4. Inspect AccountHistory.
5. Inspect AppDbContext.
6. Inspect existing entity configurations.
7. Inspect UnitOfWork.
8. Inspect account services.
9. Inspect authentication.
10. Inspect controllers.
11. Inspect API conventions.
12. Inspect DTO/request/response conventions.
13. Inspect existing tests.
14. Inspect database migration strategy.
15. Inspect README.
16. Check current Git status.

Determine exactly what already exists.

Do not create duplicate account infrastructure.

Do not replace working authentication.

---

# 5. SOCIAL GRAPH MODEL

The core graph is:

```text
Account
   │
   ├── follows ──→ Account
   │
   ├── blocks ──→ Account
   │
   └── mutes ──→ Account
```

These are relationships between accounts.

A follow should NOT be stored as fields like:

```text
Account.FollowingIds
Account.FollowerIds
```

Do not serialize entire follower lists into the Account row.

Use dedicated relationship records.

---

# 6. FOLLOW MODEL

Create a dedicated follow relationship.

Conceptually:

```text
Follow
├── FollowerAccountId
├── FollowedAccountId
└── CreatedAt
```

The exact naming should follow the project's conventions.

A follow represents:

```text
A → follows → B
```

It is directional.

Therefore:

```text
Alice follows Bob
```

does NOT imply:

```text
Bob follows Alice
```

---

# 7. FOLLOW CONSTRAINTS

The database must prevent duplicate follows.

This must NOT be possible:

```text
Alice → Bob
Alice → Bob
Alice → Bob
```

There should only be one active follow relationship.

Use an appropriate unique constraint/index such as:

```text
(FollowerAccountId, FollowedAccountId)
```

Do not rely only on application-side checking.

The database should enforce uniqueness.

---

# 8. SELF-FOLLOW

Decide and enforce whether:

```text
Alice → Alice
```

is allowed.

For this project:

# Self-follow should NOT be allowed.

Reject it with an appropriate API response.

Do not create meaningless graph edges.

---

# 9. FOLLOWING

The system must support querying:

```text
Who does Alice follow?
```

Example:

```text
GET /api/accounts/{id}/following
```

Use the existing API routing conventions if different.

Do not return every account in one massive response.

Support pagination.

---

# 10. FOLLOWERS

The system must support:

```text
Who follows Alice?
```

Example:

```text
GET /api/accounts/{id}/followers
```

Again:

# Use pagination.

Do not load an entire follower graph into memory.

This will eventually need to support thousands or more accounts.

---

# 11. PAGINATION

Design follower/following queries for scale.

Do NOT do:

```text
SELECT everything
↓
Load everything into RAM
↓
Skip()
Take()
```

if the architecture can avoid it.

Prefer efficient database-side pagination.

For example:

```text
LIMIT
+
OFFSET
```

is acceptable for the initial implementation.

Keyset/cursor pagination may be introduced later when the graph becomes large.

Do not prematurely over-engineer cursor infrastructure unless the current project needs it.

---

# 12. FOLLOW ACTION

Implement an authenticated action equivalent to:

```text
POST /api/accounts/{id}/follow
```

The exact route should follow existing conventions.

The server must:

1. Authenticate the caller.
2. Verify the target account exists.
3. Verify caller is not the target.
4. Check account status where appropriate.
5. Check blocking rules.
6. Prevent duplicate follows.
7. Create the follow record.
8. Persist it transactionally where appropriate.
9. Return a clear response.

The Android client must never directly modify the database.

---

# 13. UNFOLLOW

Implement:

```text
DELETE /api/accounts/{id}/follow
```

or the equivalent route following current project conventions.

The server must:

1. Authenticate caller.
2. Find the follow relationship.
3. Remove/deactivate the active relationship.
4. Return an appropriate result.

Important:

Unfollowing should not destroy historical information if the project later records follow history.

For the current implementation, determine whether the follow entity itself should represent current state while historical follow changes are handled later.

Do NOT invent an unnecessary history subsystem if Part 05 does not require it.

However:

# Do not design the schema in a way that makes future history impossible.

---

# 14. BLOCKING

Implement account blocking.

Conceptually:

```text
Alice blocks Bob
```

This should be represented by a dedicated relationship/state.

Do not add:

```text
BlockedUserIds
```

as a serialized list on Account.

Use a dedicated record.

Conceptually:

```text
Block
├── BlockerAccountId
├── BlockedAccountId
└── CreatedAt
```

---

# 15. BLOCKING BEHAVIOR

At minimum, blocking must prevent appropriate social-graph actions.

For example:

```text
Alice blocks Bob
```

should prevent Bob from following Alice.

Likewise, Alice should not be able to follow Bob while the block relationship prohibits it.

Define the blocking behavior clearly in the implementation.

Do not implement messaging/content blocking yet unless necessary for the graph itself.

Those systems belong to later parts.

---

# 16. EXISTING FOLLOW RELATIONSHIPS AND BLOCKING

Consider what happens if:

```text
Alice follows Bob
```

then:

```text
Alice blocks Bob
```

The system should not leave contradictory active graph state.

Determine a clean rule.

Recommended:

```text
Block
 ↓
Remove/deactivate conflicting active follow relationships
```

Do this transactionally.

Document the rule.

Do not leave:

```text
Alice blocks Bob
AND
Alice follows Bob
```

as simultaneously active states if the architecture treats blocking as incompatible with following.

---

# 17. MUTING

Implement account muting.

Conceptually:

```text
Alice mutes Bob
```

Muting is NOT blocking.

It should not automatically destroy the follow relationship.

Example:

```text
Alice follows Bob
Alice mutes Bob
```

Both may coexist:

```text
Following = YES
Muted = YES
```

This distinction is important for the future feed system.

Do not implement feed filtering yet.

The graph layer only stores the mute state.

---

# 18. MUTE MODEL

Use a dedicated relationship.

Conceptually:

```text
Mute
├── MuterAccountId
├── MutedAccountId
└── CreatedAt
```

Prevent duplicate active mute relationships.

Self-mute should not be useful and should be rejected.

---

# 19. UNMUTE

Implement the inverse action.

Example:

```text
DELETE /api/accounts/{id}/mute
```

or follow current API conventions.

Unmuting should remove the active mute state.

---

# 20. MUTUAL RELATIONSHIP QUERY

Implement the ability to determine whether two accounts have graph relationships.

For example:

```text
Alice → follows Bob
Bob → follows Alice
```

Therefore:

```text
Mutual Follow = YES
```

Potential response data could include:

```text
IsFollowing
IsFollowedBy
IsMutual
IsBlocked
IsBlockedBy
IsMuted
```

Do not create an enormous social-profile endpoint.

Keep this query focused.

---

# 21. GRAPH QUERY DESIGN

The social graph should be queryable efficiently.

Eventually the system will need questions such as:

```text
Who follows this account?

Who does this account follow?

Do these two accounts follow each other?

Is this account blocked?

Is this account muted?

How many followers does this account have?

How many accounts does this account follow?
```

For Part 05 implement only the queries necessary for the current functionality.

Do not implement advanced graph algorithms yet.

No:

```text
Friend recommendations
Shortest path
Community detection
Influence propagation
PageRank
Social clustering
```

Those come much later.

---

# 22. COUNTS

The system should be able to efficiently obtain:

```text
Follower count
Following count
```

Do NOT immediately add manually maintained counters unless there is a demonstrated need.

For the current scale, database aggregation may be sufficient.

If counters are added, they must remain transactionally consistent with the underlying graph.

Never allow:

```text
Followers table = 100
Account.FollowerCount = 93
```

without a clear consistency strategy.

Do not optimize prematurely.

---

# 23. DATABASE INDEXING

Add appropriate indexes.

At minimum, think about queries involving:

```text
FollowerAccountId
FollowedAccountId
BlockerAccountId
BlockedAccountId
MuterAccountId
MutedAccountId
```

Design indexes around actual queries.

Avoid creating every imaginable index.

Remember:

```text
Indexes improve reads
but increase write/storage overhead.
```

Use only useful indexes.

---

# 24. DATABASE INTEGRITY

Use foreign keys where supported by the current EF Core/SQLite configuration.

Graph records must reference valid accounts.

Do not allow orphaned relationships.

Consider delete behavior carefully.

The account system is intended to be persistent.

Do not casually cascade-delete large amounts of historical/social data.

If account deletion is not implemented yet:

# Do not implement account deletion now.

Do not invent destructive account lifecycle behavior.

---

# 25. ACCOUNT STATUS INTERACTION

The graph should respect the account system created in Part 04.

Consider:

```text
Active
Disabled
Suspended
Banned
```

Determine which statuses are allowed to participate in graph actions.

The exact rules should be simple and documented.

Do not implement the complete moderation system.

The important point is:

# Server-side account status must not be ignored.

---

# 26. AUTHORIZATION

All graph mutation operations must require authentication.

For example:

```text
Follow
Unfollow
Block
Unblock
Mute
Unmute
```

must operate on the authenticated caller.

Never trust a client-provided:

```text
FollowerAccountId
BlockerAccountId
MuterAccountId
```

The server must derive the actor from the authenticated identity.

The client supplies only the target account.

---

# 27. PUBLIC GRAPH QUERIES

Queries such as:

```text
Followers
Following
Public profile graph information
```

may be public or authenticated depending on the existing account/privacy design.

Inspect the current account API and choose a consistent rule.

Do not silently invent an entirely new privacy architecture.

---

# 28. RESPONSE CONTRACTS

Do not expose EF entities directly if the existing project uses DTOs/contracts.

Follow the existing Part 04 pattern.

Create appropriate response models for:

```text
Account summary
Follower list
Following list
Graph relationship status
```

Avoid returning:

```text
PasswordHash
Email
Internal authentication data
```

through graph endpoints.

---

# 29. API DESIGN

The exact routes must follow the existing project conventions.

A reasonable conceptual API is:

```text
POST   /api/accounts/{id}/follow
DELETE /api/accounts/{id}/follow

GET    /api/accounts/{id}/followers
GET    /api/accounts/{id}/following

POST   /api/accounts/{id}/block
DELETE /api/accounts/{id}/block

POST   /api/accounts/{id}/mute
DELETE /api/accounts/{id}/mute

GET    /api/accounts/{id}/relationship
```

If the current API architecture uses a different naming structure, follow the existing convention instead.

Do not create duplicate route styles.

---

# 30. SERVICE ARCHITECTURE

Keep graph logic out of controllers.

Prefer:

```text
Controller
    ↓
Application Service
    ↓
Persistence
```

For example:

```text
SocialGraphController
        ↓
ISocialGraphService
        ↓
SocialGraphService
        ↓
AppDbContext / UnitOfWork
```

Follow the existing architecture.

Do not create a giant:

```text
AccountService
```

containing every future social-media operation.

Keep graph responsibilities separate.

---

# 31. TRANSACTIONS

Use the existing Unit of Work/transaction infrastructure where multiple database operations must remain consistent.

Example:

```text
Block Bob
 ↓
Remove conflicting follow
 ↓
Create block
 ↓
Commit
```

These should not partially succeed.

If something fails:

```text
Rollback
```

Do not create another transaction abstraction if the existing UnitOfWork already provides what is needed.

Reuse it.

---

# 32. CONCURRENCY

Consider simultaneous requests.

Example:

```text
Request A → Follow Bob
Request B → Follow Bob
```

The database uniqueness constraint must prevent duplicate active follows.

Likewise:

```text
Request A → Mute Bob
Request B → Mute Bob
```

must not create duplicates.

Do not rely exclusively on:

```text
if (!exists)
    insert
```

because concurrent requests can race.

Database constraints are required.

---

# 33. TESTING

Add automated tests for the graph.

At minimum:

## Follow

```text
Authenticated user follows another account → PASS
Duplicate follow → rejected/idempotent according to chosen API behavior
Self-follow → rejected
Unknown target → rejected
Unauthenticated follow → rejected
```

## Unfollow

```text
Existing follow → removed
Non-existing follow → appropriate result
Unauthenticated → rejected
```

## Followers

```text
Correct follower returned
Pagination works
```

## Following

```text
Correct followed account returned
Pagination works
```

## Blocking

```text
Block succeeds
Blocked relationship exists
Conflicting follow handled correctly
Blocked user cannot perform prohibited follow
```

## Unblocking

```text
Block removed
Future allowed graph actions work
```

## Muting

```text
Mute succeeds
Mute does not automatically remove follow
Duplicate mute prevented
```

## Unmuting

```text
Mute removed
```

## Relationship

Test combinations:

```text
No relationship

A follows B

B follows A

Mutual follow

A blocks B

A mutes B
```

The returned state must be correct.

---

# 34. PERSISTENCE TEST

Because persistence is a core project requirement:

```text
Create accounts
 ↓
Create follow/block/mute relationships
 ↓
Stop server
 ↓
Restart server
 ↓
Query graph
```

All relationships must still exist correctly.

Do not keep graph state only in memory.

---

# 35. DATABASE MIGRATION TEST

The database already contains the Part 04 schema.

Do not recreate it from scratch.

Create the appropriate incremental migration/schema change.

Verify:

```text
Existing Accounts survive
Existing Profiles survive
Existing AccountHistory survives
New graph tables exist
Indexes exist
Foreign keys behave correctly
```

---

# 36. ANDROID INTEGRATION

Only perform the minimum client work required to prove Part 05.

Do NOT build the entire social UI.

At minimum, if practical within the current client architecture, allow the client to:

```text
View an account
Follow account
Unfollow account
Block account
Unblock account
Mute account
Unmute account
View followers
View following
```

A very simple test UI is sufficient.

The backend is the important part of this checkpoint.

Do not spend excessive time on styling.

---

# 37. DO NOT BUILD A LOCAL SOCIAL GRAPH CACHE YET

The server is authoritative.

Do not create an elaborate Android-side social graph database.

The client may cache simple results later, but Part 05 does not require a complex offline graph.

Keep the architecture simple.

---

# 38. PERFORMANCE REQUIREMENT

The project eventually targets approximately:

```text
10,000 persistent accounts
```

Therefore:

# Build the graph using database relationships, not in-memory object graphs.

Do NOT do:

```text
Every Account object
    ↓
List<Account> Following
    ↓
List<Account> Followers
```

for the entire world.

The database should own the graph.

Load only what is required.

---

# 39. NO 10,000 ACCOUNT GENERATOR YET

Do NOT populate 10,000 NPCs during this part.

Part 09 handles the population system.

For testing Part 05, create a small number of accounts:

```text
Alice
Bob
Charlie
David
```

and test graph behavior.

Do not prematurely stress the system with the full simulation.

---

# 40. NO NPC LOGIC

The graph is a neutral account-to-account system.

Do not implement:

```text
NPC decides to follow
NPC decides to block
NPC personality
NPC interests
NPC social behavior
```

Those come later.

The graph must be usable by NPC simulation later without embedding NPC decision-making inside it.

---

# 41. NO LLM

Absolutely no:

```text
Ollama
Qwen
LLM prompts
LLM decisions
AI-generated follows
```

The social graph is deterministic application/database functionality.

---

# 42. HISTORY CONSIDERATION

The master specification requires permanent history.

However, Part 05 is focused on the current graph state.

Do not automatically create a massive graph-history subsystem unless needed by the current architecture.

But do design the graph so future history can be added.

Future functionality may need to answer:

```text
When did Alice follow Bob?

When did Alice unfollow Bob?

When did Alice block Bob?

How long was Alice following Bob?
```

Do not make today's schema impossible to extend for that purpose.

---

# 43. NO AUTOMATIC PRUNING

Absolute rule:

# NEVER PRUNE SOCIAL GRAPH HISTORY.

Do not add:

```text
Delete old follows
Delete old blocks
Delete old mutes
Delete inactive relationships
Delete records after X days
```

Performance must come from:

```text
Indexes
Efficient queries
Pagination
Caching
Database optimization
```

NOT deletion.

---

# 44. README UPDATE — REQUIRED

After Part 05 is complete:

# UPDATE README.md

This is now mandatory for every completed part.

Document the ACTUAL implementation.

Include:

```text
Part 05 — Social Graph
```

Document:

```text
Follow system
Unfollow system
Followers
Following
Blocking
Unblocking
Muting
Unmuting
Relationship queries
Pagination
Database tables
Indexes
API endpoints
Service architecture
Authorization
Tests
Persistence verification
```

Also update the project status:

```text
01A COMPLETE
01B COMPLETE
01C COMPLETE
01D COMPLETE
01E COMPLETE
01F COMPLETE
02 COMPLETE
03 COMPLETE
04 COMPLETE
05 COMPLETE
```

Then identify:

```text
NEXT: PART 06 — POSTS & ENGAGEMENT
```

Do not document features that were not actually implemented.

README must describe reality.

---

# 45. GIT CHECKPOINT

Before committing:

```text
Inspect git diff
Inspect git status
Build server
Run automated tests
Run graph API tests
Test persistence after restart
Verify migration
Verify Android integration where applicable
Update README
Review changed files
```

Only commit after everything passes.

Create a clear checkpoint commit.

Suggested message:

```text
Implement social graph
```

The exact message may differ if the project has a preferred convention.

After commit:

```text
Working tree: clean
```

---

# 46. REQUIRED SESSION REPORT

At completion, provide a report structured approximately as:

## PART 05 — COMPLETE

### 1. What Was Inspected

### 2. What Already Existed

### 3. What Changed

### 4. Social Graph Architecture

### 5. Database Changes

### 6. API Endpoints

### 7. Service Changes

### 8. Android Changes

### 9. Tests

Use:

```text
PASS
FAIL
```

for each test.

### 10. Persistence Test

### 11. README

Explicitly state:

```text
Updated: YES
```

### 12. Git

Show:

```text
Commit:
Working tree:
```

### 13. Current Project Status

### 14. Next

```text
NEXT: PART 06 — POSTS & ENGAGEMENT
```

Then:

# STOP

---

# 47. IMPORTANT — DO NOT CHANGE THE MASTER ARCHITECTURE

The following principles remain mandatory:

```text
Server is authoritative.

SQLite is persistent world memory.

Current state and historical state are separate concepts.

Do not automatically delete history.

Do not automatically prune data.

Do not use deletion as a performance strategy.

The Android client is not authoritative.

Authentication identity comes from the server.

Database constraints enforce integrity.

C# controls application/world state.

LLM is not the simulation.
```

---

# 48. WHAT TO PRIORITIZE

Priority order for Part 05:

```text
1. Correct graph semantics
2. Database integrity
3. Authentication/authorization
4. Transaction safety
5. Efficient queries
6. Pagination
7. Automated testing
8. Persistence
9. Clean architecture
10. Android integration
11. README documentation
12. Git checkpoint
```

Fancy UI is low priority.

Advanced optimization is low priority until measurements justify it.

---

# 49. WHAT NOT TO DO

Do NOT:

```text
Rewrite authentication
Rewrite persistence
Rewrite the entire backend
Create a new DbContext
Create a second UnitOfWork
Create duplicate Account classes
Create giant AccountService
Create giant GameManager
Create in-memory global follower lists
Create 10,000 accounts
Create NPC behavior
Use LLM
Implement feeds
Implement posts
Implement recommendations
Implement advanced graph algorithms
Delete old data
Prune history
Skip tests
Skip README
```

---

# 50. FINAL SUCCESS CONDITION

Part 05 is complete only when:

```text
Accounts can follow each other
        ↓
Accounts can unfollow
        ↓
Followers can be queried
        ↓
Following can be queried
        ↓
Blocking works
        ↓
Unblocking works
        ↓
Muting works
        ↓
Unmuting works
        ↓
Mutual relationship state can be queried
        ↓
Authentication is enforced
        ↓
Database constraints protect integrity
        ↓
Pagination works
        ↓
Relationships survive server restart
        ↓
Existing Part 04 data survives
        ↓
Tests pass
        ↓
Server builds
        ↓
README updated
        ↓
Git checkpoint created
        ↓
Working tree clean
```

Then STOP.

Do not begin Part 06 automatically.

Wait for the next instruction.

# START PART 05 NOW.

First inspect the existing project.

Determine what already exists.

Then implement only the missing Social Graph functionality.

Build it.

Test it.

Verify persistence.

Update README.

Commit the working checkpoint.

Then STOP.