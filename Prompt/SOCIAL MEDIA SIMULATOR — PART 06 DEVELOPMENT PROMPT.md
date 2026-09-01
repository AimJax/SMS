# PART 06 — POSTS & ENGAGEMENT

Continue development of the Social Media Simulator strictly from the existing project state.

## CURRENT CHECKPOINT

Parts 01A–01F, 02, 03, 04, and 05 are COMPLETE.

The latest completed component is:

**PART 05 — SOCIAL GRAPH**

Implemented and verified:

- Account
- Profile
- AccountHistory
- Follow
- Block
- Mute
- JWT authentication
- SQLite + EF Core persistence
- Unit of Work
- SocialGraphService
- GraphController
- Pagination
- Relationship queries
- Blocking/follow conflict handling
- Transactional graph operations

Latest Git commit:

`0104a37 Implement social graph`

The working tree was clean at the end of Part 05.

---

# YOUR TASK

Implement:

# PART 06 — POSTS & ENGAGEMENT

This part establishes the core content system of the social-media platform.

Do NOT jump ahead into the feed algorithm, recommendations, notifications, messaging, moderation, or advanced NPC simulation unless a small supporting abstraction is genuinely required.

Build the foundation cleanly so those later systems can depend on it.

---

# 1. FIRST — INSPECT THE EXISTING PROJECT

Before modifying anything:

1. Inspect the current repository structure.
2. Inspect existing:
   - Account entities
   - Profile entities
   - AccountHistory
   - Social graph entities
   - AppDbContext
   - UnitOfWork
   - Entity configurations
   - Service interfaces
   - Controllers
   - DTO/contracts
   - Authentication/JWT infrastructure
   - Database migrations
   - README
   - Existing tests
3. Confirm the conventions already established in Parts 01–05.
4. Do NOT duplicate existing functionality.
5. Do NOT replace working architecture merely because you prefer another design.
6. Preserve the existing layered architecture and naming conventions.

The existing project is the source of truth.

---

# 2. GOAL

Create a persistent social-media content system supporting:

- Creating posts
- Reading posts
- Deleting posts
- Likes
- Unlike
- Comments
- Comment deletion
- Engagement counts
- Pagination
- Author information
- Persistence across server restarts

The implementation must be suitable for thousands of accounts and large numbers of posts.

Avoid premature optimization, but design database access and indexes correctly from the beginning.

---

# 3. POST ENTITY

Create a persistent `Post` entity.

At minimum it should contain:

- `PostId` — GUID
- `AuthorAccountId` — GUID
- `Content` — string
- `CreatedAt`
- `UpdatedAt`
- `Status` or equivalent lifecycle state if consistent with the existing architecture

Relationships:

```text
Account
   │
   └── creates ──→ Post
                       │
                       ├── Likes
                       └── Comments
```

A post belongs to exactly one account.

Do not store redundant author information inside the Post entity unless there is a clear architectural reason.

---

# 4. POST LIFECYCLE

Implement basic lifecycle behavior.

Required operations:

### Create

Authenticated account creates a post.

Requirements:

- Account must exist.
- Account must be allowed to post according to existing account/status rules.
- Content cannot be empty.
- Apply sensible maximum content length.
- Created timestamp is generated server-side.
- Author is the authenticated account.

### Get

Publicly retrieve a post.

The response should contain useful author information without exposing sensitive account data.

### Delete

Authenticated author can delete their own post.

Do not allow arbitrary users to delete another user's post.

Use the project's existing authorization conventions.

---

# 5. LIKE SYSTEM

Create a `PostLike` entity.

At minimum:

- `PostId`
- `AccountId`
- `CreatedAt`

The combination:

```text
(PostId, AccountId)
```

must be unique.

This prevents one account from liking the same post multiple times.

Required operations:

```text
POST   /api/posts/{id}/like
DELETE /api/posts/{id}/like
```

Behavior:

- Authenticated users only.
- Like must reference an existing post.
- Duplicate likes must be handled safely.
- Unlike must be idempotent or otherwise follow the project's established API conventions.
- Like count must be available when retrieving a post.

---

# 6. COMMENT SYSTEM

Create a `Comment` entity.

At minimum:

- `CommentId`
- `PostId`
- `AuthorAccountId`
- `Content`
- `CreatedAt`
- `UpdatedAt`
- lifecycle/status field if consistent with the project

A comment belongs to:

```text
Post
   └── Comments
          └── Author Account
```

Required operations:

```text
POST   /api/posts/{id}/comments
GET    /api/posts/{id}/comments
DELETE /api/comments/{id}
```

Rules:

- Creating comments requires authentication.
- Comment content cannot be empty.
- Comment must reference an existing post.
- Users may delete their own comments.
- Do not allow arbitrary deletion of other users' comments.
- Support pagination.
- Return author information safely.

---

# 7. ENGAGEMENT RESPONSE

Post responses should expose useful engagement information.

At minimum:

```text
PostId
Author
Content
CreatedAt
UpdatedAt
LikeCount
CommentCount
IsLikedByCurrentUser
```

`IsLikedByCurrentUser` should only be calculated when an authenticated user is available.

Do not require authentication merely to view public posts.

Avoid loading every Like and Comment entity just to calculate counts if efficient database-side counting can be used.

---

# 8. PAGINATION

Posts and comments must support pagination.

Use the existing project's pagination conventions if they already exist.

If no convention exists, establish one consistent format.

Example:

```text
page
pageSize
totalCount
items
```

Do not load an entire user's post history into memory.

Use database-side ordering and pagination.

Posts should normally be ordered newest-first.

Comments should have a deterministic ordering.

---

# 9. DATABASE DESIGN

Create appropriate EF Core configurations.

Expected tables:

```text
Posts
PostLikes
Comments
```

Expected important indexes:

### Posts

```text
AuthorAccountId
CreatedAt
(AuthorAccountId, CreatedAt)
```

### PostLikes

```text
(PostId, AccountId) UNIQUE
PostId
AccountId
CreatedAt
```

### Comments

```text
PostId
AuthorAccountId
CreatedAt
(PostId, CreatedAt)
```

Use foreign keys appropriately.

Respect the project's existing SQLite/EF Core conventions.

---

# 10. SERVICES

Create service interfaces and implementations following the architecture already established.

Expected conceptual services:

```text
IPostService
PostService
```

If separating engagement logic improves the existing architecture:

```text
IEngagementService
EngagementService
```

However, do not unnecessarily create abstractions.

The important requirement is that controllers remain thin and business logic remains in the application/service layer.

---

# 11. CONTROLLERS

Create the appropriate controller(s).

Expected API surface:

```text
POST   /api/posts

GET    /api/posts/{id}

DELETE /api/posts/{id}

POST   /api/posts/{id}/like

DELETE /api/posts/{id}/like

POST   /api/posts/{id}/comments

GET    /api/posts/{id}/comments

DELETE /api/comments/{id}
```

Follow existing routing and response conventions.

Do not expose EF Core entities directly if the project already uses DTOs/contracts.

---

# 12. AUTHORIZATION

Use the existing JWT authentication system.

Rules:

### Public

```text
GET /api/posts/{id}
GET /api/posts/{id}/comments
```

### Authenticated

```text
POST /api/posts
DELETE /api/posts/{id}   [owner only]

POST /api/posts/{id}/like
DELETE /api/posts/{id}/like

POST /api/posts/{id}/comments
DELETE /api/comments/{id} [owner only]
```

Do not create a second authentication system.

Do not duplicate JWT logic.

---

# 13. SOCIAL GRAPH INTERACTION

Part 05 already established:

- Follow
- Block
- Mute

Do not implement the feed algorithm yet.

However, ensure the post system can later support graph-aware behavior.

For example, the architecture should make it possible for a future FeedService to query:

```text
posts from followed accounts
posts from public accounts
posts excluded by blocks
posts excluded by mutes
```

Do NOT implement the actual personalized feed in Part 06.

---

# 14. DATA INTEGRITY

Pay particular attention to:

- Foreign keys
- Unique like constraints
- Ownership checks
- Invalid post IDs
- Invalid account IDs
- Duplicate likes
- Missing posts
- Missing accounts
- Empty comments
- Empty posts
- Pagination bounds
- Concurrent like requests
- Transaction consistency where necessary

Do not rely only on controller validation.

Important business rules belong in the service/application layer.

---

# 15. TESTING

Build the server after implementation.

Create or extend tests for at least:

### Posts

- Create authenticated post
- Reject unauthenticated creation
- Reject empty post
- Retrieve post
- Retrieve post after restart
- Delete own post
- Reject deletion by another account
- Reject deletion of nonexistent post

### Likes

- Like post
- Unlike post
- Duplicate like rejected/handled safely
- Like count correct
- Like persists after restart
- `IsLikedByCurrentUser` correct
- Unauthenticated retrieval still works

### Comments

- Create comment
- Reject unauthenticated comment
- Reject empty comment
- Retrieve comments
- Comment pagination
- Delete own comment
- Reject deletion by another account
- Comment persists after restart

### Database

- Foreign keys work
- Unique like constraint works
- Indexes exist
- Existing Part 04/05 data still works
- Existing accounts remain intact
- Existing social graph data remains intact

---

# 16. PERSISTENCE VERIFICATION

The existing project already has proven SQLite persistence.

Verify that:

```text
Account
Profile
Follow
Block
Mute
Post
Like
Comment
```

all survive a server restart.

Do not recreate or wipe the database during normal development/testing.

Existing data must remain intact.

---

# 17. README — REQUIRED

IMPORTANT:

**UPDATE `README.md` AS PART OF THIS PART.**

The README must accurately reflect everything completed in Part 06.

Add/update documentation for:

### Posts

- Post entity
- Post lifecycle
- API endpoints
- Authentication requirements
- Ownership rules

### Likes

- Like model
- Unique constraint
- Like/unlike endpoints
- Engagement counts

### Comments

- Comment model
- Comment endpoints
- Ownership rules
- Pagination

### Database

Document:

```text
Posts
PostLikes
Comments
```

including important relationships and indexes.

### Verification

Document the tests actually performed.

Do NOT claim tests passed if they were not actually executed.

The README must represent the real current state of the project.

---

# 18. GIT

When implementation and verification are complete:

1. Inspect `git status`.
2. Inspect the diff.
3. Make sure no unrelated files or generated junk are committed.
4. Commit the completed Part 06 implementation.

Suggested commit message:

```text
Implement posts and engagement
```

Do not modify unrelated historical commits.

---

# 19. IMPORTANT DEVELOPMENT RULES

Throughout this part:

- Preserve existing architecture.
- Do not rewrite working systems.
- Do not add unnecessary dependencies.
- Do not introduce premature microservices.
- Do not implement the feed algorithm yet.
- Do not implement notifications yet.
- Do not implement DMs yet.
- Do not implement advanced moderation yet.
- Do not implement NPC AI yet.
- Do not build Android UI for this part unless an existing contract absolutely requires it.
- Keep server authoritative.
- Keep database persistence reliable.
- Keep controllers thin.
- Keep business logic in services.
- Use async database operations.
- Avoid N+1 queries.
- Use database-side pagination.
- Use DTOs/contracts consistently.
- Follow the project's existing naming and folder conventions.

---

# 20. FINAL VERIFICATION

Before declaring Part 06 complete:

```text
Server Build              PASS
Post Creation             PASS
Post Retrieval            PASS
Post Deletion             PASS
Like                      PASS
Unlike                    PASS
Duplicate Like Handling   PASS
Like Count                PASS
Comment Creation          PASS
Comment Retrieval         PASS
Comment Pagination        PASS
Comment Deletion          PASS
Authorization             PASS
Persistence After Restart PASS
Existing Data Preserved   PASS
README Updated            PASS
Git Working Tree Clean    PASS
```

Only mark an item PASS if it was actually verified.

---

# 21. FINAL SESSION REPORT

When finished, provide a concise but complete session log using this structure:

# PART 06 — COMPLETE

## 1. What Was Inspected

## 2. What Already Existed

## 3. What Changed

## 4. Posts Architecture

## 5. Database Changes

## 6. API Endpoints

## 7. Service Changes

## 8. Tests

## 9. Persistence Test

## 10. README

State explicitly:

`Updated: YES`

and summarize what was documented.

## 11. Git

Report:

- Commit hash
- Commit message
- Working tree status

## 12. Current Project Status

Show:

```text
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
```

## 13. NEXT

End with:

```text
NEXT: PART 07 — FEED SYSTEM
```

Then STOP.

Do not begin Part 07 automatically.