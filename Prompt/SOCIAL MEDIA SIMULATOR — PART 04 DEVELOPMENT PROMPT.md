# SOCIAL MEDIA SIMULATOR — PART 04 DEVELOPMENT PROMPT

## CURRENT PROJECT STATE

You are continuing development of the **Social Media Simulator** project.

This is an existing local project.

The project has already successfully completed:

```text
01A — Development Environment        ✅
01B — Repository Foundation          ✅
01C — ASP.NET Core Server             ✅
01D — SQLite Foundation               ✅
01E — Android Client                  ✅
01F — Foundation Checkpoint           ✅
02  — Backend Architecture            ✅
03  — Persistence                     ✅
```

Latest completed Git checkpoint:

```text
Commit: 33badd0
Message: Establish persistence architecture
Working tree: clean
```

The previous Part 03 session confirmed:

```text
AppDbContext                 ✅
Connection configuration     ✅
Dependency injection        ✅
Async persistence            ✅
SQLite                       ✅
WAL mode                     ✅
Transactions                ✅
Unit of Work                ✅
Entity configuration        ✅
Persistence tests            ✅
Restart persistence         ✅
Historical data preserved   ✅
```

The project is currently working.

# IMPORTANT

Do NOT restart the project.

Do NOT recreate the foundation.

Do NOT replace working architecture without a concrete reason.

Do NOT jump ahead to NPCs, LLMs, feeds, virality, events, relationships, or the 10,000-account simulation.

The next task is ONLY:

# PART 04 — ACCOUNTS & AUTHENTICATION

---

# 1. FIRST — INSPECT THE EXISTING PROJECT

Before changing anything:

1. Inspect the entire current project structure.
2. Inspect the existing README.md.
3. Inspect the existing Server architecture.
4. Inspect the existing Shared project if present.
5. Inspect the existing database/persistence implementation.
6. Inspect existing tests.
7. Inspect existing configuration.
8. Inspect the latest Git state.
9. Identify anything already implemented that overlaps with Part 04.

Do not assume the master prompt perfectly describes the current code.

The actual project is authoritative.

Reuse existing infrastructure.

Do not create duplicate systems.

---

# 2. PART 04 OBJECTIVE

The objective of Part 04 is to establish the foundation for:

```text
Accounts
Profiles
Authentication
Authorization
Account Types
Account Status
Account History
```

At the end of this part, a real player should be able to:

```text
Register
   ↓
Receive an account
   ↓
Log in
   ↓
Authenticate requests
   ↓
Access protected server endpoints
   ↓
Retrieve their own account/profile
```

The system must be designed so that NPC accounts can later use the same underlying account architecture.

---

# 3. DO NOT OVERBUILD PART 04

Do NOT implement the entire social network.

Do NOT implement:

```text
Posts
Comments
Likes
Followers
Following
Relationships
NPC simulation
Personality
Memory
Events
Virality
Trends
Rumors
News
LLM
Communities
Advanced feed
Creator economy
Messaging
```

Those belong to later parts.

Part 04 should establish the **identity/account foundation** that those systems will eventually build upon.

---

# 4. ACCOUNT ARCHITECTURE

Design the account system around a persistent account identity.

Conceptually:

```text
Account
│
├── Identity
├── Authentication
├── Profile
├── AccountType
├── AccountStatus
└── AccountHistory
```

Do not prematurely create giant class hierarchies.

Avoid:

```text
PlayerAccount : Account
CelebrityAccount : Account
NPCAccount : Account
NewsAccount : Account
InfluencerAccount : Account
```

unless the existing architecture provides a compelling reason.

Prefer an account with properties/state that can represent different account types.

For example:

```text
Account
+
AccountType
+
AccountStatus
+
Profile
```

This supports future combinations such as:

```text
Celebrity + Gamer + Creator
OrdinaryUser + Creator
OfficialAccount + News
Influencer + CommunityLeader
```

---

# 5. ACCOUNT IDENTITY

Create a stable internal account identifier.

The identifier must:

* Be unique.
* Persist permanently.
* Never depend on username changes.
* Be suitable for database relationships.
* Be safe to expose as an opaque identifier where appropriate.

Do not use username as the primary identity.

Example conceptual model:

```text
AccountId = stable identity

Username = changeable public identifier
```

If usernames are allowed to change later, historical records must continue referring to the same AccountId.

---

# 6. USERNAME

Implement a unique username.

Requirements:

* Unique.
* Validated server-side.
* Case-handling must be clearly defined.
* Cannot collide with another account.
* Cannot contain invalid characters.
* Must have sensible length limits.

Do not rely solely on application-level checks.

The database should enforce uniqueness where appropriate.

Consider race conditions:

```text
Request A → username "Sarah"
Request B → username "Sarah"

Both must NOT succeed.
```

The database must remain authoritative for uniqueness.

---

# 7. EMAIL / LOGIN IDENTITY

Determine whether the current project requires email authentication or another login identifier.

Do not blindly add unnecessary authentication complexity.

If email is used:

* Normalize it appropriately.
* Enforce uniqueness.
* Never expose sensitive authentication information through normal profile endpoints.

Do not return passwords or password hashes to clients.

---

# 8. PASSWORD SECURITY

If implementing password authentication:

NEVER store plaintext passwords.

Do NOT invent a custom hashing algorithm.

Use a well-established password hashing mechanism available in the selected .NET authentication stack.

Passwords should be:

```text
Password
   ↓
Secure password hashing
   ↓
Stored password hash
```

Never:

```text
Password
   ↓
SQLite plaintext
```

Do not log passwords.

Do not return passwords in API responses.

Do not store passwords in Android local storage in plaintext.

---

# 9. AUTHENTICATION

Implement the minimum production-oriented authentication foundation required by the current architecture.

The client should authenticate with the server.

Conceptually:

```text
Android
   ↓
Login/Register
   ↓
ASP.NET Core
   ↓
Authentication
   ↓
Authenticated identity
   ↓
Protected API
```

The exact authentication mechanism should be chosen based on the current project and .NET architecture.

Do not introduce unnecessary authentication frameworks merely because they exist.

Prefer a maintainable standard .NET approach.

---

# 10. AUTHORIZATION

Authentication answers:

```text
Who are you?
```

Authorization answers:

```text
Are you allowed to perform this action?
```

Establish the foundation for authorization.

At minimum, protected account endpoints should verify the authenticated account.

Example:

```text
GET /api/account/me
```

should return the authenticated account.

A player must NOT be able to simply provide another AccountId and access protected private account data.

---

# 11. ACCOUNT PROFILE

Create the initial public profile foundation.

The profile should eventually support:

```text
Username
DisplayName
Bio
Avatar
AccountType
Verification
FollowerCount
FollowingCount
Fame
Reputation
```

However, do NOT implement follower/fame systems yet.

Only create fields that are justified for the current part.

Avoid prematurely adding dozens of unused columns.

The profile architecture should be extendable later.

---

# 12. PUBLIC VS PRIVATE DATA

Clearly separate information that is:

## Public

Potential examples:

```text
AccountId
Username
DisplayName
Bio
Avatar reference
AccountType
Verification state
```

## Private

Potential examples:

```text
Email
Authentication information
Security information
Internal account metadata
```

Do not accidentally serialize the entire database entity into API responses.

Use DTOs/contracts where appropriate.

---

# 13. ACCOUNT STATUS

Create an account status concept.

Initial statuses may include:

```text
Active
Disabled
Suspended
Banned
```

Do not implement the complete moderation system yet.

The purpose of this part is to establish the account-state foundation.

Future moderation systems can extend this.

Do not implement complex suspension workflows now.

---

# 14. ACCOUNT TYPES

Establish the foundation for account types.

The master specification eventually expects:

```text
Ordinary User
Casual Creator
Niche Creator
Influencer
Major Influencer
Celebrity
News / Media
Official / Organization
Community / Topic
Special
```

Do NOT implement all behavioral differences yet.

For Part 04, simply establish a representation that can support these categories later.

The account type should not dictate a completely separate class for every type.

---

# 15. ACCOUNT HISTORY

The master specification requires permanent history.

Establish the initial account-history foundation.

Examples of future history:

```text
Account Created
Username Changed
Display Name Changed
Profile Changed
Account Type Changed
Verification Granted
Verification Removed
Suspended
Unsuspended
Banned
Unbanned
```

For this part, implement only the history events actually justified by the functionality being built.

Do NOT invent dozens of unused history records.

History must be persistent.

---

# 16. PERMANENT HISTORY RULE

This is an absolute requirement.

NEVER implement automatic deletion of account history.

Do NOT create:

```text
Delete old account history
Delete history after X days
Keep only recent history
Archive then delete
```

Historical data must remain queryable.

Performance problems must be solved through:

```text
Indexes
Pagination
Efficient queries
Caching
Proper schema design
```

NOT deletion.

---

# 17. DATABASE SAFETY

You are modifying an existing SQLite database.

Be extremely careful.

Before schema changes:

1. Inspect current schema.
2. Inspect current migration/schema strategy.
3. Understand existing tables.
4. Determine how the project currently handles migrations.
5. Preserve existing data.

Do NOT casually execute:

```sql
DROP TABLE
```

Do NOT destroy the existing persistence test data.

Do NOT recreate the database simply because a schema change is easier.

If a migration system already exists, use it.

If one does not exist, determine the safest approach consistent with the existing architecture before proceeding.

---

# 18. API DESIGN

Create only the minimum endpoints needed for Part 04.

Potential endpoints:

```text
POST /api/auth/register
POST /api/auth/login

GET /api/account/me
GET /api/accounts/{id}
```

The exact routes should follow the existing project's conventions.

Do not blindly use these routes if the current architecture has an established convention.

Public account lookup and private authenticated account retrieval must remain conceptually separate.

---

# 19. REGISTRATION

Registration should:

1. Validate input.
2. Validate username.
3. Validate login identifier if applicable.
4. Securely hash password if password authentication is used.
5. Create the account.
6. Create its profile.
7. Create initial account history.
8. Persist using the existing persistence architecture.
9. Use a transaction where multiple records must succeed together.
10. Return an appropriate response.

If account creation fails partway through, do not leave half-created accounts.

---

# 20. LOGIN

Login should:

1. Validate credentials.
2. Find the account.
3. Verify the password securely.
4. Check account status.
5. Establish authentication.
6. Return the required authentication result.
7. Avoid leaking whether a specific account exists unnecessarily.

Do not log credentials.

Do not return sensitive database fields.

---

# 21. PROTECTED ACCOUNT ENDPOINT

Implement something equivalent to:

```text
GET /api/account/me
```

It should require authentication.

Example:

```text
Unauthenticated request
        ↓
401 Unauthorized
```

Authenticated:

```text
Authenticated request
        ↓
Account identity
        ↓
Return current account/profile
```

This endpoint is important because it proves that authentication is actually connected to account identity.

---

# 22. ANDROID INTEGRATION

Only perform the minimum Android work necessary to prove the account/authentication pipeline.

Do not build a complete social-media UI.

The client should eventually be able to:

```text
Register
Login
Store authentication state appropriately
Call authenticated endpoint
Display current account
```

Keep UI extremely simple for this checkpoint.

Example:

```text
Social Media Simulator

Username: ______
Password: ______

[ Register ]
[ Login ]

Status:
Logged in as @username
```

The exact UI should follow the current client architecture.

---

# 23. CLIENT SECURITY

Do not store sensitive authentication information casually.

Do not:

```text
Hardcode credentials
Hardcode tokens
Commit tokens
Commit passwords
Log tokens
Display tokens in UI
```

Use an appropriate secure Android storage mechanism if persistent authentication storage is implemented.

If persistent token storage is outside the current scope, keep the authentication session minimal rather than introducing unnecessary complexity.

---

# 24. ERROR HANDLING

Implement sensible API responses.

Examples:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
500 Internal Server Error
```

Do not expose stack traces or internal database details to clients in production-style responses.

Development logging can contain diagnostic information where appropriate.

---

# 25. VALIDATION

Server-side validation is mandatory.

Never trust Android input.

Validate:

```text
Username
Email/login identifier if applicable
Password
DisplayName
Bio
Account identifiers
```

Client validation may improve UX.

It does NOT replace server validation.

---

# 26. TESTING

Create automated tests where appropriate.

At minimum test:

### Registration

```text
Valid registration → succeeds
Duplicate username → fails
Invalid username → fails
Invalid input → fails
```

### Authentication

```text
Valid credentials → succeeds
Invalid credentials → fails
Disabled account → rejected
Suspended/banned account → rejected as appropriate
```

### Authorization

```text
Unauthenticated /me → 401
Authenticated /me → succeeds
```

### Persistence

```text
Register
 ↓
Restart server
 ↓
Account still exists
```

### Profile

```text
Account created
 ↓
Profile exists
 ↓
Profile retrievable
```

### History

```text
Account created
 ↓
History record exists
 ↓
History survives restart
```

Do not stop at compilation.

---

# 27. TEST THE DATABASE AFTER RESTART

This project requires persistence.

Therefore:

```text
Register account
      ↓
Verify account
      ↓
Stop server
      ↓
Start server
      ↓
Login
      ↓
Verify account
```

must work.

The account must not exist only in memory.

---

# 28. DO NOT IMPLEMENT SOCIAL FEATURES YET

Even though accounts are now available, STOP before implementing:

```text
Follow
Followers
Following
Posts
Comments
Likes
Replies
Reposts
Quote-posts
Threads
Feed
Search
NPCs
10,000 accounts
Personality
Relationships
Events
Virality
Trends
Rumors
News
Memory
Ollama
Qwen3-4B
LLM queue
```

Those are later phases.

---

# 29. ARCHITECTURAL QUALITY

While implementing Part 04:

Prefer clear separation such as:

```text
API
 ↓
Application
 ↓
Domain
 ↓
Persistence
```

but follow the architecture already established in the project.

Do not restructure the entire backend simply because another architecture might look cleaner.

The existing architecture is working.

Make incremental changes.

Avoid:

```text
God AccountService
God GameManager
God DatabaseManager
```

Keep responsibilities focused.

---

# 30. DO NOT OVER-ABSTRACT

Do not create interfaces for every class simply to "make it scalable."

Create abstractions where they provide actual value.

Avoid unnecessary:

```text
IAccountFactory
IProfileFactory
IUsernameValidatorFactory
IAccountMapperFactory
IAccountServiceFactory
```

unless there is a real architectural reason.

The goal is maintainable code, not maximum abstraction.

---

# 31. DO NOT OVERENGINEER THE DATABASE

Do not create the eventual 30+ social-media tables now.

Part 04 should create only the tables/entities needed for:

```text
Accounts
Profiles
Authentication
Account status
Account type
Account history
```

Later parts can add:

```text
Posts
Follows
Relationships
Events
Memories
```

etc.

Incremental schema evolution is intentional.

---

# 32. README REQUIREMENT

This is now a mandatory rule for the entire project.

After completing Part 04:

# UPDATE README.md

The README must reflect the actual state of the project.

Document:

```text
Part 04 completion
Account architecture
Authentication architecture
Authorization
Account/profile model
Account types
Account status
Account history
New API endpoints
Database changes
Testing performed
Current project status
Next development phase
```

Do NOT merely write:

```text
Part 04 complete
```

The README should be useful to another developer opening the project later.

Do not document functionality that was not actually implemented.

The README must describe reality.

---

# 33. GIT CHECKPOINT

At the end:

```text
Inspect git diff
Inspect git status
Review changed files
Run tests
Build server
Build Android if applicable
Verify database
Verify authentication
Verify registration
Verify restart persistence
Update README
```

Then create a Git checkpoint.

Commit message should clearly describe the work.

Example:

```text
Implement accounts and authentication
```

Do not commit until the project is working.

Working tree should be clean after the commit.

---

# 34. REQUIRED DEVELOPMENT RESPONSE

During this task, report progress using:

## 1. What We Are Building

## 2. What Already Exists

## 3. What Needs to Change

## 4. Architecture

## 5. Files To Create

## 6. Files To Modify

## 7. Implementation

Provide complete files when code changes are required.

Do NOT give incomplete snippets when a complete file is needed.

## 8. Database Changes

Explain schema/migration changes.

## 9. API Changes

List endpoints and behavior.

## 10. Android Changes

Explain client changes.

## 11. Tests

Show exactly what was tested.

## 12. Results

Clearly state PASS/FAIL.

## 13. README Update

Confirm README was updated with the actual completed functionality.

## 14. Git Checkpoint

Show commit hash/message when completed.

## 15. STOP

Stop after Part 04 is verified.

---

# 35. ERROR RULE

If anything breaks:

```text
STOP
 ↓
Inspect error
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

Do NOT continue adding features while the project is broken.

Do NOT hide errors.

Do NOT claim success without testing.

---

# 36. WHAT IS IMPORTANT

Highest priority:

```text
Existing architecture
Correctness
Data safety
Authentication security
Persistent account identity
Database integrity
Server authority
Testing
Maintainability
README accuracy
Working checkpoint
```

---

# 37. WHAT IS NOT IMPORTANT RIGHT NOW

Do NOT spend time on:

```text
Fancy UI
Animations
Advanced styling
Advanced feed algorithms
NPC intelligence
LLM prompting
Virality
Social drama
10,000 account generation
Complex moderation
Creator economy
```

Those come later.

A simple working account system is better than a beautiful unfinished architecture.

---

# 38. CORE PRINCIPLE

The project is being built as:

```text
FOUNDATION
    ↓
ACCOUNTS
    ↓
SOCIAL GRAPH
    ↓
CONTENT
    ↓
NPC SIMULATION
    ↓
SOCIAL SYSTEMS
    ↓
EMERGENT WORLD
```

Do not skip layers.

Part 04 is the identity layer.

Build it correctly.

---

# 39. FINAL STOP CONDITION

Part 04 is complete ONLY when:

```text
Account registration works
        ↓
Account persists in SQLite
        ↓
Profile persists
        ↓
Authentication works
        ↓
Protected endpoint works
        ↓
Authorization works
        ↓
Account status exists
        ↓
Account type exists
        ↓
Account history exists
        ↓
Tests pass
        ↓
Server builds
        ↓
Android integration works as appropriate
        ↓
README updated
        ↓
Git checkpoint created
        ↓
Working tree clean
```

Then:

# STOP.

Do NOT automatically begin Part 05.

Wait for the next instruction.

---

# 40. REMEMBER THE MASTER PRINCIPLES

Always preserve these:

```text
SERVER IS AUTHORITATIVE.

DATABASE IS PERSISTENT WORLD MEMORY.

HISTORY IS NEVER AUTOMATICALLY PRUNED.

MEMORY IS NEVER AUTOMATICALLY PRUNED.

DERIVED DATA NEVER REPLACES ORIGINAL DATA.

CURRENT STATE AND HISTORY ARE SEPARATE.

C# CONTROLS SIMULATION STATE.

LLM GENERATES LANGUAGE, NOT WORLD STATE.

MOST NPCs WILL NOT NEED LLM CALLS.

10,000 ACCOUNTS ≠ 10,000 LLM CALLS PER TICK.

SIMULATION MUST SCALE THROUGH EFFICIENCY, NOT DELETION.

IMPORTANT WORLD CHANGES SHOULD HAVE CAUSES.

BUILD IN SMALL WORKING CHECKPOINTS.

NEVER STACK FEATURES ON BROKEN CODE.

UPDATE README AFTER EVERY COMPLETED PART.

DO NOT JUMP AHEAD.

DO NOT RUSH.

KEEP THE PROJECT WORKING.
```

# START PART 04 NOW.

First inspect the existing project and determine exactly what is already present for accounts/authentication.

Do not assume.

Do not rewrite working infrastructure.

Implement only the missing Part 04 functionality.

Test everything.

Update README.

Create the Git checkpoint.

Then STOP.