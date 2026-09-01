# SOCIAL MEDIA SIMULATOR — PART 14 DEVELOPMENT PROMPT
## NOTIFICATIONS SYSTEM

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
11   NPC Background Simulation     COMPLETE
12   NPC Social Graph              COMPLETE
13   AI Content Generation         COMPLETE
```

Latest commit:

```text
583de7a — Implement provider-agnostic AI content generation (Part 13)
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. The Part 13 push was confirmed successful and verified against origin, so no push-resolution step is required at the start of this part — but still run `git status` and `git fetch` as your first action to confirm nothing has drifted since.

---

# 1. WHY THIS PART, NOW

Every part since 08 has poured effort into making NPCs act like real people: they browse feeds, like posts, comment, follow and unfollow each other with personality-driven reasoning (Part 10, Part 12), and now generate real AI-written text (Part 13). All of that activity is currently invisible to the player unless they manually reload their own feed and notice new followers/likes/comments by comparison.

Part 14 closes that loop: it surfaces the engagement the simulation is already generating — from both NPCs and other players — directly to the account it happened to, as a proper **Notifications system**. This is the most direct way to make the last six parts of NPC work actually *felt* by whoever is using the account.

---

# 2. THE EXISTING PROJECT

The existing backend already contains, among everything from Parts 01–13:

- Accounts, Profiles, Authentication, JWT
- `SocialGraphService` — Follow/Block/Mute rules
- Posts, Likes, Comments (soft-deletable)
- `FeedService` — chronological, paginated, block/mute-aware feed
- `NpcSimulationHostedService` — autonomous background tick loop (pause/resume/status, overlap prevention, failure isolation)
- `NpcBehaviorService` / `NpcDecisionService` / `NpcSocialGraphService` — NPCs performing Follow, Unfollow, LikePost, Comment, CreatePost actions against real accounts, including player accounts
- `AiContentGeneratorService` + provider-agnostic `IAiTextGenerationService` — AI-or-template content generation
- `NpcAction` — action history already recording who did what to whom

This means most of the **event source data** for notifications already exists in the form of `Follow`, `PostLike`, and `Comment` records (and `NpcAction` history for NPC-attributed actions). Part 14 is primarily about **surfacing** these events, not inventing new ones.

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

Notifications are generated and owned by the server, triggered by real state changes (a follow, a like, a comment) — never fabricated or requested by the client.

## Layered architecture

```text
API
Application
Domain
Infrastructure
Contracts
```

Notification generation must not be duplicated logic bolted onto `SocialGraphService`/`PostService`/`NpcBehaviorService` independently — use one consistent mechanism (see Section 3) so every code path that creates a follow/like/comment reliably produces a notification without needing to remember to do so in five different places.

## Reuse, don't duplicate

Do not create a second "who follows whom" or "who liked what" store. Notifications reference existing entities (`Follow`, `PostLike`, `Comment`) by ID; they do not copy/duplicate that data.

## Permanent data rule

Per the project's established permanent-history principle (Part 01B), notifications must NOT be automatically deleted/pruned/expired. "Read" is a status flag, not a deletion. Performance at scale is solved with pagination/indexes, not deletion.

## Performance

With hundreds to thousands of accounts and an active NPC background loop, notification writes will happen frequently. This must not become a bottleneck for the tick loop (Part 11) or for normal API requests (posting, liking, commenting, following).

---

# PART 14 OBJECTIVE

Implement a first, solid notifications system:

1. A `Notification` entity/table capturing discrete events relevant to an account.
2. A single, consistent, reusable mechanism for creating notifications whenever a relevant event occurs (follow, like, comment), regardless of whether the actor is a player or an NPC.
3. An authenticated, paginated API for a player to retrieve their own notifications.
4. Read/unread state tracking, including an unread count and a mark-as-read action.
5. Correct integration with existing block/mute rules — a blocked/muted relationship must not generate notifications either direction, consistent with how it already suppresses feed visibility (Part 07) and NPC targeting (Part 12).
6. No negative impact on tick-loop performance or throughput (Part 11's guarantees must still hold).

Do NOT implement in this part:

- Push notifications (mobile OS-level push/FCM/APNs) — this is server-side, pull-based (`GET` polling) only for now.
- Real-time delivery (WebSockets/SignalR) — the client polls or refreshes; live push delivery is a future part.
- Notification preferences/settings (e.g., "mute notifications from X" beyond existing mute/block) — reuse existing mute/block only.
- Email/SMS notifications.
- Grouped/bundled notifications ("Alice and 12 others liked your post") — one notification per event is fine for this part; bundling is a future refinement.
- Mentions/hashtags-based notifications — those systems don't exist yet (deferred from Part 07); only Follow/Like/Comment trigger notifications in this part.
- Any Android UI beyond the minimal integration described in Section 13.

---

# PART 14 — REQUIRED FEATURES

## 1. Notification entity

Create a `Notification` entity capturing at minimum:

```text
Id                  (GUID, stable identity per project convention)
RecipientAccountId  (who receives this notification)
ActorAccountId      (who caused it — may be an NPC or player account; accounts already
                     represent both uniformly per existing conventions)
Type                (Follow, Like, Comment — extensible enum)
RelatedEntityId     (the Follow / PostLike / Comment ID this notification is about)
RelatedPostId       (nullable — the post involved, for Like/Comment types, so the client
                     can deep-link without a second lookup)
CreatedAt
IsRead              (bool)
ReadAt              (nullable timestamp)
```

Inspect existing entity conventions (soft-delete pattern, GUID usage, timestamp conventions) from Parts 03–06 and follow them. Do not soft-delete notifications for this part — deletion isn't in scope; only read/unread state is.

---

## 2. Single, consistent notification-creation mechanism

Create a dedicated abstraction, for example:

```text
INotificationService
{
    Task NotifyFollowAsync(Guid followId, ...);
    Task NotifyLikeAsync(Guid postLikeId, ...);
    Task NotifyCommentAsync(Guid commentId, ...);
}
```

Wire this into the **existing** code paths that already create `Follow`, `PostLike`, and `Comment` records — `SocialGraphService` and `PostService` (or their current equivalents; inspect first) — rather than creating parallel logic in `NpcBehaviorService`. Since NPC actions already go through the same `SocialGraphService`/`PostService` methods as player actions (per Part 05/06/12's "reuse, don't duplicate" principle), wiring notification creation into those shared services means both player-caused and NPC-caused events are covered by one code path, automatically.

Verify this explicitly: an NPC liking a player's post and a player liking another player's post must both reliably produce a notification through the same mechanism, not two different ones.

---

## 3. Self-notification suppression

Do not generate a notification when the actor and recipient are the same account (e.g., liking your own post, if that's even possible under existing rules — inspect and confirm). Document the rule.

---

## 4. Block/mute suppression

If the recipient has blocked the actor, or the actor has blocked the recipient, no notification should be created — consistent with existing feed suppression rules (Part 07) and NPC targeting suppression (Part 12). Reuse the existing block-check mechanism from `SocialGraphService`; do not reimplement block logic inside `INotificationService`.

Decide and document whether **mutes** also suppress notifications (recommended: yes, for consistency with the feed, since a muted account's content is already hidden from your feed) or whether notifications intentionally differ from feed visibility. Whichever you choose, document the reasoning — do not leave it undecided/inconsistent.

---

## 5. Deleted-content handling

If the underlying post is soft-deleted after a notification was already created, the notification API should handle this gracefully (e.g., omit or clearly flag content that's no longer available) rather than erroring or returning broken data to the client. Do not physically delete the notification merely because the related post was deleted — the notification itself is historical record.

---

## 6. Notification feed endpoint

Create an authenticated endpoint:

```http
GET /api/notifications
```

- Authentication required; recipient is always the authenticated user from JWT claims — never accepted from the client.
- Returns the authenticated user's notifications, newest first, with deterministic secondary ordering (same convention as Part 07's feed).
- Cursor-based pagination, consistent with the approach chosen in Part 07 — reuse that pattern/convention rather than inventing a new one.

Response should include enough for the client to render without an extra round-trip per notification, for example:

```text
NotificationId
Type
ActorAccountId
ActorUsername
ActorDisplayName
ActorAvatarUrl
RelatedPostId          (nullable)
RelatedPostSnippet     (short excerpt, nullable — omit or mark unavailable if the post was deleted)
CreatedAt
IsRead
```

---

## 7. Unread count endpoint

```http
GET /api/notifications/unread-count
```

Returns a simple count, efficiently computed (a `COUNT` query with an index-friendly filter, not "load all notifications and count them in memory").

---

## 8. Mark-as-read

Support marking notifications as read. At minimum:

```http
POST /api/notifications/{id}/read      -- mark one notification read
POST /api/notifications/read-all       -- mark all of the authenticated user's notifications read
```

Both must operate only on the authenticated user's own notifications (never allow marking someone else's notifications as read via a guessed ID — verify ownership server-side).

---

## 9. Query performance

Add indexes only where inspection/justification supports them — the obvious candidates are `(RecipientAccountId, CreatedAt)` for the feed query and `(RecipientAccountId, IsRead)` for the unread-count query. Document exactly which indexes were added and why. Avoid:

```text
load every notification for a user into memory to filter/sort/count
```

Prefer database-side filtering, ordering, and pagination, consistent with Part 07's feed query approach.

---

## 10. Tick-loop / write-throughput impact

Because NPC actions (Part 10–12) run continuously via the background loop (Part 11), notification writes will happen at whatever rate NPCs like/comment/follow. Confirm:

- Notification writes happen as part of the same transaction/unit-of-work as the triggering action where practical, so a notification is never silently lost due to a crash between "create the Follow" and "create the Notification."
- A failure in notification creation must not prevent the underlying Follow/Like/Comment from succeeding, and must not crash the tick (reuse Part 11's failure-isolation guarantee) — document the exact failure-handling decision (e.g., log and continue vs. fail the whole action) and why.
- The background loop's per-tick timing (Part 11 observability) is not measurably degraded by notification writes. Note the before/after tick-duration comparison in your verification.

---

## 11. Tests

Add tests appropriate to this part. At minimum verify:

### Authentication

```text
Unauthenticated notification request → rejected
Authenticated request → succeeds
```

### Follow notification

```text
Bob follows Alice
Alice's notifications include a Follow notification from Bob
```

### Like notification

```text
Bob likes Alice's post
Alice's notifications include a Like notification from Bob, referencing the post
```

### Comment notification

```text
Bob comments on Alice's post
Alice's notifications include a Comment notification from Bob, referencing the post
```

### NPC-attributed notifications

```text
An NPC (via the background tick loop / NpcBehaviorService) likes a player's post
The player's notifications include that Like notification
(proves the single-mechanism wiring from Section 2 actually covers NPC-caused events)
```

### Self-notification suppression

```text
Actions that could only target oneself (if any exist under current rules) do not
generate a notification to oneself
```

### Block suppression

```text
Alice blocks Bob
Bob follows/likes/comments where technically possible
Alice receives no notification from Bob
```

### Mute suppression

```text
Verify the documented decision from Section 4 (muted → suppressed, or not) with a test
that matches the documented behavior
```

### Deleted post handling

```text
Bob comments on Alice's post
Post is soft-deleted
Alice's notification for that comment is still retrievable and does not error,
with the deleted state handled gracefully per Section 5
```

### Unread count

```text
New notifications increase the unread count
Marking one as read decreases it by exactly one
Marking all as read brings it to zero
```

### Mark-as-read ownership

```text
User cannot mark another user's notification as read
(attempting with someone else's notification ID fails/is rejected)
```

### Pagination and ordering

```text
Multiple notifications with different timestamps return newest → oldest
Page 1 → next cursor → Page 2 has no duplicates, no skipped items
```

### Failure isolation regression

```text
A forced notification-creation failure does not prevent the underlying Follow/Like/Comment
from being created, and does not crash the tick loop (Part 11 guarantee still holds)
```

### Performance

```text
Tick duration with notification writes enabled is not significantly worse than
the Part 11/12/13 baseline (document the comparison)
```

### Persistence

```text
Notifications and their read/unread state persist across a server restart
```

### Regression

```text
Existing Parts 05-13 tests still pass
```

---

## 12. Database migration

This part requires schema changes (the new `Notification` table and its indexes). Create the migration through the normal EF Core workflow and verify it applies cleanly on a fresh database and on top of the existing Part 13 database.

---

## 13. Android

Part 14 is primarily a backend task. Do NOT build a full Android notifications UI/inbox screen.

If the existing Android project requires a minimal adjustment to stay coherent with the new API surface (e.g., a data model for the notification response shape, matching the pattern established for feed integration if one already exists), make only the necessary change. Do not build push notifications, badges, or real-time UI updates.

---

## 14. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

Document:

- Part 14 completion
- Notification architecture and the single-mechanism creation approach
- Notification types supported (Follow, Like, Comment)
- Block/mute suppression decision and reasoning
- Self-notification suppression rule
- Deleted-post handling behavior
- API endpoints (feed, unread-count, mark-read, mark-all-read)
- Pagination strategy (consistent with Part 07)
- Relevant database indexes
- Failure-isolation decision for notification writes
- Tick-loop performance impact (before/after comparison)
- Tests performed and results
- Current project status
- Next planned part

---

## 15. Git

After implementation and verification:

1. Inspect `git status`.
2. Review changed files. Ensure no generated junk or unrelated files are committed.
3. Commit the completed work.

Suggested commit message:

```text
Implement notifications system (Part 14)
```

Push to `origin/main`. Verify the push actually reached the remote (fetch and compare, per the practice established in recent parts) before reporting success.

---

## 16. DO NOT IMPLEMENT YET

Do NOT implement the following in Part 14:

```text
Push notifications (FCM/APNs)
Real-time delivery (WebSockets/SignalR)
Notification preferences/settings beyond existing mute/block
Email/SMS notifications
Grouped/bundled notifications
Mentions/hashtags
Trending/virality mechanics
Direct messages
Communities/groups
Creator economy
Moderation system
Full Android notifications UI
```

Those belong to later parts.

---

## 17. DEVELOPMENT PROCESS

Before changing anything:

1. Confirm `git status`/`git fetch` show a clean, synced state.
2. Inspect `SocialGraphService` (Follow creation path).
3. Inspect `PostService` (Like and Comment creation paths).
4. Inspect `NpcBehaviorService` to confirm it calls the same shared services rather than its own data-access paths.
5. Inspect `FeedService`'s pagination/cursor convention (Part 07) to reuse it consistently.
6. Inspect `AppDbContext` and existing entity configuration conventions.
7. Inspect `NpcSimulationHostedService` / `SimulationStateService` for how per-tick timing is currently measured, to produce the before/after comparison required in Section 10.
8. Inspect existing authentication/authorization conventions.
9. Inspect the README.

Then implement Part 14. Do not assume a file does not exist merely because this prompt says to create it. Reuse existing functionality wherever appropriate. Do not duplicate business logic. Do not perform unrelated refactoring.

---

## 18. QUALITY REQUIREMENTS

The implementation must be:

- correct
- persistent (no automatic deletion/pruning, per the project's permanent-data rule)
- server-authoritative
- consistent with existing block/mute rules
- database-efficient (indexed, paginated, no full-table loads)
- resilient to individual write failures (reuses Part 11 isolation)
- testable
- maintainable
- compatible with the existing architecture

---

## 19. FINAL VERIFICATION

Before declaring Part 14 complete, verify:

```text
Server builds
Notifications generated for Follow, Like, Comment — from both player and NPC actors
Self-notifications suppressed
Blocked-relationship notifications suppressed
Mute suppression behaves per the documented decision
Deleted-post notifications handled gracefully
Unread count accurate and efficient
Mark-as-read (single and all) works and enforces ownership
Pagination correct: no duplicates, no skipped items, newest-first ordering
A forced notification-write failure does not block the underlying action or crash the tick loop
Tick-loop performance not measurably degraded
Notifications and read state persist across restart
Existing Parts 05-13 tests still pass
README updated
Git commit created
Git push succeeds and is verified against origin
Working tree clean
```

---

## 20. FINAL SESSION REPORT

When finished, provide a complete session report in this structure:

```text
# PART 14 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Notification Architecture
...

## 5. API Endpoints
...

## 6. Suppression Rules (Self / Block / Mute)
...

## 7. Deleted-Content Handling
...

## 8. Database Changes
...

## 9. Failure Isolation & Tick-Loop Impact
...

## 10. Tests
...

## 11. README
Updated: YES
...

## 12. Git
Commit: ...
Push: ...
Verified against origin: ...
Working tree: ...

## 13. Current Project Status

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
12  COMPLETE
13  COMPLETE
14  COMPLETE

## 14. Intentionally Not Implemented
- Push notifications (FCM/APNs)
- Real-time delivery (WebSockets/SignalR)
- Grouped/bundled notifications
- Mentions/hashtags
- Full Android notifications UI

## 15. NEXT

NEXT: PART 15 — ...
```

Do not claim completion until the implementation and verification have actually succeeded.

**STOP after completing Part 14 and reporting the session log.**
