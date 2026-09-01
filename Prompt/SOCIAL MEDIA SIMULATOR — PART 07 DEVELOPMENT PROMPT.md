# SOCIAL MEDIA SIMULATOR — PART 07 DEVELOPMENT PROMPT
## FEED, TIMELINE & CONTENT DISCOVERY

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
```

Latest commit:

```text
40d23e6 — Implement posts and engagement (Part 06)
```

Remote:

```text
origin/main
```

Working tree should currently be clean.

The existing backend already contains:

- ASP.NET Core server
- layered architecture
- EF Core
- SQLite
- Unit of Work
- Accounts
- Profiles
- Authentication
- JWT
- Follow relationships
- Blocks
- Mutes
- Posts
- Likes
- Comments
- Pagination
- Soft deletion
- Entity configurations
- API controllers
- DTO/contracts
- automated/manual verification
- README documentation

The project is a **standalone online Social Media Simulator**, not a Life Simulator.

The long-term goal is a simulated social platform populated by many different types of NPC accounts and emergent interactions.

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

The server owns:

- social state
- accounts
- relationships
- posts
- engagement
- feed generation
- simulation state
- NPC behavior
- future recommendation systems

The Android client must NOT become authoritative for simulation.

## Layered architecture

Continue using the existing separation between:

```text
API
Application
Domain
Infrastructure
Contracts
```

Do not collapse business logic into controllers.

Do not introduce unnecessary architectural rewrites.

## Stable identity

Continue using GUID-based stable IDs where the existing project does so.

## Persistence

All important state must persist through EF Core + SQLite.

## Performance

The eventual simulator should support a much larger world than the initial prototype.

Do not design the system around only a handful of users.

The long-term simulation should be capable of supporting **hundreds to thousands of accounts**, including:

- ordinary users
- lurkers
- influencers
- celebrities
- news outlets
- official organizations
- businesses
- creators
- public figures
- bots/system accounts where appropriate

Do not implement premature pruning of the simulated world.

Optimize queries and indexes instead.

---

# PART 07 OBJECTIVE

Implement the first real **feed/timeline system**.

The player currently has:

- an account
- a profile
- follows
- blocks
- mutes
- posts
- likes
- comments

Now the player needs a way to retrieve a meaningful timeline composed of content from the social graph.

The initial feed should be **chronological**, not an AI recommendation algorithm yet.

Do NOT jump ahead and implement the full recommendation/algorithmic feed in this part.

That will come later.

Part 07 establishes the foundation that future recommendation systems will build upon.

---

# PART 07 — REQUIRED FEATURES

## 1. Feed endpoint

Create an authenticated feed endpoint.

Example:

```http
GET /api/feed
```

Authentication required.

The endpoint should return posts relevant to the authenticated user's timeline.

At minimum, the initial feed should contain:

- posts created by accounts the user follows
- optionally the user's own posts

Choose the behavior that best matches the existing architecture and document the decision.

---

# 2. Social graph filtering

The feed MUST respect the existing social graph.

Do not show content from:

- blocked accounts
- accounts that have blocked the authenticated user

Muted accounts should also be excluded from the feed.

The existing Follow / Block / Mute behavior from Part 05 must be reused rather than duplicated incorrectly.

---

# 3. Deleted posts

Soft-deleted posts must not appear in the feed.

Reuse the existing `Post` soft-delete behavior.

Do not physically delete posts merely to make feed queries easier.

---

# 4. Pagination

The feed must support pagination.

Prefer a scalable approach suitable for a growing social network.

Do not build pagination that requires loading the entire feed into memory.

A cursor-based approach is preferred if it fits naturally with the existing project.

If the current architecture strongly favors another approach, use the existing convention and document it.

The API should return enough information for the client to request the next page.

---

# 5. Feed response

Create appropriate response DTOs.

The feed response should provide enough information for a social-media client to render a post without immediately requiring another request for every basic field.

Include appropriate information such as:

```text
PostId
AuthorAccountId
AuthorUsername
AuthorDisplayName
AuthorAvatarUrl
Body
CreatedAt
LikeCount
CommentCount
IsLikedByCurrentUser
```

Use the existing domain models and conventions.

Do not introduce fields that require systems which have not been implemented yet.

---

# 6. Engagement counts

The feed should expose basic engagement information.

At minimum:

```text
LikeCount
CommentCount
```

These values should be calculated efficiently.

Do not perform an unnecessary database query for every post if the query can reasonably be optimized.

Avoid an obvious N+1 query pattern.

---

# 7. Current-user like state

The feed should indicate whether the authenticated user has liked each returned post.

Example:

```text
IsLikedByCurrentUser
```

This must be determined from the existing `PostLike` data.

Do not create a second or duplicate like system.

---

# 8. Ordering

Part 07 uses a simple chronological timeline.

Default ordering:

```text
Newest → Oldest
```

Use a deterministic secondary ordering where necessary so pagination does not produce duplicate or missing posts when timestamps are equal.

---

# 9. Author information

The feed should provide sufficient author information for rendering.

Use the existing:

```text
Account
Profile
```

relationships.

Do not duplicate profile data into Post unless there is a strong architectural reason.

---

# 10. Feed service

Create a dedicated feed abstraction.

For example:

```text
IFeedService
FeedService
```

The controller should not contain feed-generation business logic.

The architecture should look approximately like:

```text
FeedController
      ↓
IFeedService
      ↓
EF Core / AppDbContext
      ↓
SQLite
```

Use the project's existing conventions rather than blindly copying these exact names if equivalent abstractions already exist.

---

# 11. Feed controller

Create the appropriate API controller.

For example:

```text
FeedController
```

Expose:

```http
GET /api/feed
```

The endpoint must require authentication.

Use the authenticated user's AccountId from the JWT claims.

Do NOT accept the AccountId from the client as the source of truth.

The server determines whose feed is being requested.

---

# 12. Feed query performance

Design the query for a growing dataset.

Inspect the existing indexes and add only indexes that are actually justified.

Consider the primary access pattern:

```text
followed accounts
        ↓
their posts
        ↓
newest first
        ↓
pagination
```

The implementation must avoid:

```text
load every followed account
load every post
sort everything in application memory
```

Prefer database-side filtering, joining, ordering, and pagination.

---

# 13. Block and mute behavior

The existing social graph rules must remain authoritative.

Example:

```text
Alice follows Bob
Bob creates post
Alice sees Bob's post
```

If Alice mutes Bob:

```text
Bob's new posts do not appear in Alice's feed
```

If Alice blocks Bob:

```text
Bob's posts do not appear in Alice's feed
```

If Bob blocks Alice:

```text
Bob's posts do not appear in Alice's feed
```

Do not modify Part 05's established semantics unless inspection reveals an actual bug.

If a change is required, explain why before making it.

---

# 14. Empty feed

An account with no relevant followed accounts/posts should receive a valid empty response.

Do not treat an empty feed as an error.

Example concept:

```json
{
  "items": [],
  "nextCursor": null
}
```

Use the project's established response conventions.

---

# 15. Tests

Add comprehensive tests for Part 07.

At minimum verify:

### Authentication

```text
Unauthenticated feed request → rejected
Authenticated feed request → succeeds
```

### Basic feed

```text
User follows Bob
Bob creates post
User requests feed
Bob's post appears
```

### Own posts

If the chosen design includes the user's own posts:

```text
User creates post
User requests feed
Own post appears
```

Document the behavior.

### Non-followed account

```text
User does not follow Bob
Bob creates post
Bob's post does not appear
```

### Mute

```text
User follows Bob
Bob creates post
User mutes Bob
Feed excludes Bob's post
```

### Block

```text
User follows Bob
Bob creates post
User blocks Bob
Feed excludes Bob's post
```

### Reverse block

```text
Bob blocks User
Bob creates post
Feed excludes Bob's post
```

### Deleted post

```text
Followed account creates post
Post is soft-deleted
Feed excludes post
```

### Like state

```text
User likes Post A
Feed returns Post A
IsLikedByCurrentUser = true
```

And:

```text
User has not liked Post B
IsLikedByCurrentUser = false
```

### Counts

Verify:

```text
LikeCount
CommentCount
```

are correctly returned.

### Ordering

Create multiple posts with different timestamps and verify:

```text
newest → oldest
```

### Pagination

Verify:

```text
Page 1
↓
next cursor
↓
Page 2
```

and verify there are:

- no duplicates
- no skipped posts
- correct ordering

### Persistence

Restart the server/database process and verify the feed still works using persisted data.

---

# 16. Database migration

Only create a migration if the implementation actually requires schema changes.

Do NOT create meaningless migrations.

If new indexes are justified, add them through the normal EF Core migration workflow.

Verify the migration applies successfully.

---

# 17. Android

Part 07 is primarily a backend foundation task.

Do NOT build a large Android feed UI yet.

If the existing Android project requires a minimal API integration adjustment to keep the architecture coherent, make only the necessary change.

The primary goal is establishing a reliable backend feed API.

---

# 18. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

This is mandatory.

Document:

- Part 07 completion
- Feed architecture
- Feed endpoint
- Authentication requirements
- Chronological ordering
- Pagination strategy
- Block behavior
- Mute behavior
- Deleted-post behavior
- Feed response format
- Like/comment counts
- Current-user like state
- Relevant database indexes
- Tests performed
- verification results
- current project status
- next planned part

Do not leave the README describing an older architecture.

---

# 19. Git

After implementation and verification:

1. Inspect git status.
2. Review changed files.
3. Ensure no generated junk or unrelated files are committed.
4. Commit the completed work.

Use a clear commit message such as:

```text
Implement feed and timeline (Part 07)
```

Push to:

```text
origin/main
```

Only report success if the command actually succeeds.

---

# 20. DO NOT IMPLEMENT YET

Do NOT implement the following in Part 07:

- AI recommendation algorithm
- machine-learning ranking
- echo chambers
- trending algorithm
- hashtags
- mentions
- notifications
- NPC simulation
- content generation by Ollama
- viral mechanics
- advanced discovery
- infinite-scroll client UI
- communities
- DMs
- groups
- creator economy
- moderation system

Those belong to later parts.

Part 07 should establish a **clean chronological feed foundation** that later systems can replace or augment.

---

# 21. DEVELOPMENT PROCESS

Before changing anything:

1. Inspect the repository.
2. Inspect existing entities.
3. Inspect `PostService`.
4. Inspect `SocialGraphService`.
5. Inspect `AppDbContext`.
6. Inspect entity configurations.
7. Inspect authentication/JWT claims.
8. Inspect existing controllers and DTO conventions.
9. Inspect existing tests/manual verification.
10. Inspect the README.

Then implement Part 07.

Do not assume a file does not exist merely because this prompt says to create it.

Reuse existing functionality wherever appropriate.

Do not duplicate business logic.

Do not perform unrelated refactoring.

---

# 22. QUALITY REQUIREMENTS

The implementation must be:

- correct
- persistent
- server-authoritative
- asynchronous
- database-efficient
- paginated
- testable
- maintainable
- compatible with the existing architecture

Avoid premature optimization, but do not knowingly introduce obvious N+1 queries or in-memory full-feed processing.

---

# 23. FINAL VERIFICATION

Before declaring Part 07 complete, verify:

```text
Server builds
Feed endpoint works
Authentication works
Followed posts appear
Non-followed posts do not appear
Muted posts do not appear
Blocked posts do not appear
Reverse-blocked posts do not appear
Deleted posts do not appear
Like state works
Like counts work
Comment counts work
Ordering works
Pagination works
Persistence works
README updated
Git commit created
Git push succeeds
Working tree clean
```

---

# 24. FINAL SESSION REPORT

When finished, provide a complete session report in this structure:

```text
# PART 07 — COMPLETE

## 1. What Was Inspected

...

## 2. What Already Existed

...

## 3. What Changed

...

## 4. Feed Architecture

...

## 5. API Endpoints

...

## 6. Database Changes

...

## 7. Tests

...

## 8. README

Updated: YES

...

## 9. Git

Commit: ...

Push: ...

Working tree: ...

## 10. Current Project Status

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

## 11. NEXT

NEXT: PART 08 — ...
```

Do not claim completion until the implementation and verification have actually succeeded.

**STOP after completing Part 07 and reporting the session log.**