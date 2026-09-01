# SOCIAL MEDIA SIMULATOR — PART 15 DEVELOPMENT PROMPT
## COMMUNITIES

You are continuing development of the **Social Media Simulator** from the existing project.

**DO NOT restart, redesign, or replace the existing architecture.**

You must inspect the current repository first and build directly on everything already implemented.

---

# CURRENT PROJECT CHECKPOINT

Completed:

```text
01A  Development Environment         COMPLETE
01B  Repository Foundation           COMPLETE
01C  ASP.NET Core Server            COMPLETE
01D  SQLite Foundation               COMPLETE
01E  Android Client Foundation       COMPLETE
01F  Foundation Checkpoint           COMPLETE
02   Backend Architecture            COMPLETE
03   Persistence                     COMPLETE
04   Accounts & Authentication       COMPLETE
05   Social Graph                    COMPLETE
06   Posts & Engagement              COMPLETE
07   Feed & Timeline                 COMPLETE
08   NPC Simulator Foundation        COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation         COMPLETE
11   NPC Background Simulation       COMPLETE
12   NPC Social Graph                COMPLETE
13   AI Content Generation           COMPLETE
14   Notifications System            COMPLETE
```

Latest commit:

```text
667183a — Updated project files
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 14.

---

# 1. WHY THIS PART, NOW

Parts 01–14 built the core social media infrastructure: accounts, social graph, posts, feeds, NPCs with AI-generated content, and a notifications system to surface engagement. The world has a growing population of accounts that follow, post, like, and comment on each other.

Part 15 introduces **Communities** — persistent social subcultures that group accounts around shared interests, topics, and identities. Communities are one of the core features named in the Master Prompt's ultimate design goal (Section 37) and are foundational to several future systems:

- Feed personalization and echo chambers (Part 15 Advanced Feed)
- Trend propagation through communities (Part 20)
- News account monitoring of communities (Part 22)
- Player-created communities (Part 29)
- Community influence and reputation

Without communities, the network is essentially a flat list of accounts. Adding communities creates natural clustering, subgroup identity, community-specific trends, and community-based behavior differentiation — all of which make the world feel more like a real internet.

---

# 2. THE EXISTING PROJECT

The existing backend already contains, among everything from Parts 01–14:

- Accounts, Profiles, Authentication, JWT
- `SocialGraphService` — Follow/Block/Mute rules
- Posts, Likes, Comments (soft-deletable)
- `FeedService` — chronological, paginated, block/mute-aware feed
- `NotificationService` — follow/like/comment notifications (Part 14)
- `NpcSimulationHostedService` — autonomous background tick loop (pause/resume/status, overlap prevention, failure isolation)
- `NpcBehaviorService` / `NpcDecisionService` / `NpcSocialGraphService` — NPCs performing actions with personality-driven reasoning
- `AiContentGeneratorService` + provider-agnostic `IAiTextGenerationService` — AI-or-template content generation
- Existing entity conventions: GUID IDs, soft-delete pattern, `CreatedAt`/`UpdatedAt`, owner-ID patterns

This means the infrastructure for adding a new entity type (`Community`) with membership (`CommunityMembership`) and its own feed is already well-established. Part 15 mirrors the patterns used for Accounts/Posts/Notifications.

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

Communities are created, owned, and moderated by the server. The server controls membership, visibility, and content rules. The client requests actions; the server validates and performs them.

## Layered architecture

```text
API
Application
Domain
Infrastructure
Contracts
```

Community management follows the same layered pattern as existing systems.

## Reuse, don't duplicate

Do not create a separate "community account" type. Communities are a distinct entity attached to existing accounts (as members/owners). Reuse existing post infrastructure for community posts. Do not duplicate post/entity logic.

## Permanent data rule

Per the project's established permanent-history principle (Part 01B), community records must NOT be automatically deleted/pruned. When a community is archived/deactivated, it remains in the database; its posts and membership history remain queryable.

## Performance

With potentially hundreds of communities and thousands of members, membership queries and community feed generation must remain efficient. Use indexes, pagination, and appropriate query strategies — not full-table scans.

---

# PART 15 OBJECTIVE

Implement a first, solid Communities system:

1. A `Community` entity capturing discrete communities with identity, ownership, and configuration.
2. A `CommunityMembership` entity tracking which accounts are members of which communities, and their role within the community.
3. APIs for: browsing communities, joining/leaving communities, community discovery (search/recommended), community details, and community feed.
4. Community roles: Owner, Admin, Moderator, Member — with appropriate permission boundaries.
5. NPC awareness of communities: NPCs should be aware of communities relevant to their interests and personality, and should join communities (for Tier 1–2 NPCs). Community membership should influence NPC behavior.
6. Community posts: existing `Post` entity is reused with an optional `CommunityId` reference — a post can belong to a community (visible to members) or be a personal post (visible to followers).
7. Community feed: a paginated, community-scoped feed showing recent posts within a community.
8. Community discovery: APIs to browse and search communities by name, topic, or interest tag.

Do NOT implement in this part:

- Community-specific moderation tools (banning members, deleting content, warnings) — deferred to Part 30 Moderation.
- Community events — deferred to Part 16 Event System.
- Community DMs or group chats — deferred to Part 28 Messaging.
- Community verification or official status — deferred to Part 31 Verification.
- Community feeds in the Android client beyond basic display — defer Android feed rendering to a future Android-focused part.
- Pinned posts, community rules, community descriptions beyond basic fields.
- Community influence or community-specific reputation scores.
- Nested/sub-communities or community hierarchy.
- Auto-generation of initial community set (Part 09 generated accounts; community generation will be a separate population step).

---

# PART 15 — REQUIRED FEATURES

## 1. Community entity

Create a `Community` entity capturing at minimum:

```text
Id                      (GUID, stable identity per project convention)
Name                    (unique display name)
Slug                    (URL-safe unique identifier)
Description            (short description, nullable)
OwnerAccountId         (account that owns this community)
Topic                  (primary topic tag, e.g., "gaming", "music", "tech")
Tags                    (comma-separated or JSON list of related tags)
Visibility              (Public, Private, Hidden — enum)
IsActive                (bool — soft-deactivate rather than delete)
MemberCount             (denormalized for fast display, updated via trigger/service)
PostCount               (denormalized for fast display)
CreatedAt
UpdatedAt
```

Inspect existing entity conventions (soft-delete pattern, GUID usage, timestamp conventions) from Parts 03–06 and follow them.

---

## 2. CommunityMembership entity

Create a `CommunityMembership` entity:

```text
Id                      (GUID)
CommunityId
AccountId
Role                    (Owner, Admin, Moderator, Member — enum)
JoinedAt
IsActive                (bool — leave vs hard-delete)
```

A community's owner is determined by `Community.OwnerAccountId`; membership records track everyone else. Ensure `AccountId + CommunityId` is unique. An account can be a member of many communities.

---

## 3. Post with CommunityId

Extend the existing `Post` entity (or configure the API/Service layer) to support an optional `CommunityId` reference:

```text
CommunityId             (nullable GUID — if set, this is a community post visible to members)
```

A post with `CommunityId = null` is a personal post (existing behavior, visible in personal feed/followers).

A post with `CommunityId = X` is a community post (visible in community feed and to community members' personal feeds if they have community posts enabled).

Do NOT modify the existing post table in a breaking way. If `CommunityId` is already nullable or can be added as a nullable column, add it via migration. If the existing schema requires a different approach, document the decision.

---

## 4. Community Browsing API

```http
GET /api/communities
```

- Public communities only (no auth required).
- Returns paginated list: Name, Slug, Topic, MemberCount, Description, CreatedAt.
- Sort by: member count, name, newest.
- Cursor or offset pagination consistent with Part 07/14 conventions.

```http
GET /api/communities/{slug}
```

- Public and private communities visible to authenticated members.
- Returns community details + membership role (for the authenticated user).

```http
GET /api/communities/{slug}/feed
```

- Returns paginated posts within the community.
- Authenticated user must be a member (or community is Public).
- Reuses existing post response shape; includes community context.

---

## 5. Community Search/Discovery API

```http
GET /api/communities/search?q={query}&topic={topic}&sort={sort}&cursor={cursor}
```

- Search by name, description, or tags.
- Filter by topic (optional).
- Returns same shape as `/api/communities`.
- Topic-based discovery: `GET /api/communities/by-topic/{topic}` — returns communities with matching primary topic.

---

## 6. Community Membership API

```http
POST /api/communities/{slug}/join
```

- Authenticated only.
- Adds membership record (if not already a member and community is Public or User was invited).
- Do NOT implement invites in this part — joining public communities is open.
- Returns membership details.

```http
POST /api/communities/{slug}/leave
```

- Authenticated only.
- Soft-deactivates membership (sets `IsActive = false`).
- Owner cannot leave — must transfer ownership first (document this constraint).

```http
GET /api/communities/{slug}/members
```

- Returns paginated list of active members with their roles.

```http
GET /api/accounts/{id}/communities
```

- Returns communities the authenticated account is a member of.
- Authenticated only.

---

## 7. Community Post Creation

Extend the existing post creation API to accept an optional `CommunityId`:

```http
POST /api/posts
{
  "content": "...",
  "communityId": "..."  // nullable
}
```

- If `communityId` is provided, the authenticated user must be an active member of that community.
- If `communityId` is null, creates a personal post (existing behavior).
- Posts created in a community should also appear in the posting account's personal feed (if the account follows them personally) — check with existing feed logic to confirm whether community posts should flow to personal feeds or remain community-scoped. Document the decision.

---

## 8. NPC Awareness of Communities

NPCs should be aware of communities relevant to their interests and personality:

- When an NPC's interests overlap with community topics/tags, the NPC should have a probability of joining that community.
- Community membership influences NPC behavior: community members are more likely to post in their community, comment on community posts, and interact with other community members.
- NPCs should not join ALL matching communities — use personality-driven selection (e.g., `CommunityParticipation` tendency from Part 10).
- NPCs should not be auto-assigned as community owners (that role is reserved for player-created communities, Part 29). NPCs can be regular members or admins.
- Add `CommunityId` to the NPC decision context when generating post/comment actions — prefer posting in joined communities proportional to `CommunityParticipation` tendency.
- Tier 1–2 NPCs (Part 11 simulation tiers) should be prioritized for community-related behavior. Tier 3 NPCs (background/lurkers) should rarely participate.
- Document the probability thresholds and how community interests are matched to NPC interests.

---

## 9. Initial Community Seed (Data)

Create a data seeding mechanism that generates an initial set of communities (e.g., 50–100) with diverse topics covering the major categories from the Master Prompt (Gaming, Technology, Anime, Music, Sports, Photography, Memes, Fashion, Art, Celebrity fandom, etc.).

This is similar in spirit to the account population in Part 09. The communities should:

- Have generated names, slugs, descriptions, and tags.
- NOT have an owner (or owner is a system/SMS bot account that already exists).
- Be marked as Public visibility.
- Be active.

Do NOT automatically assign all existing NPCs as members. NPCs join communities organically through their behavior system (Section 8 above). At most, seed a few NPCs as initial members for some communities to kickstart activity.

---

## 10. Database Migration

This part requires schema changes (the new `Community` and `CommunityMembership` tables, `CommunityId` on posts, and appropriate indexes). Create migrations through the normal EF Core workflow and verify they apply cleanly on a fresh database and on top of the existing Part 14 database.

Indexes to consider:
- `CommunityMembership(CommunityId, AccountId, IsActive)` — for membership checks and member lists
- `CommunityMembership(AccountId, IsActive)` — for "my communities" queries
- `Community(Slug)` — unique lookup
- `Community(Topic)` — topic browsing
- `Community(MemberCount)` — sorting by popularity
- `Post(CommunityId, CreatedAt)` — community feed query

---

## 11. Tests

Add tests appropriate to this part. At minimum verify:

### Authentication

```text
Unauthenticated community join/leave request → rejected
Authenticated request → succeeds
```

### Community CRUD

```text
Public communities are browsable without auth
Private community details are restricted to members
Hidden communities are not discoverable by non-members
```

### Community membership

```text
Account joins a public community → membership record created
Account leaves a community → membership deactivated (IsActive = false)
Account cannot join the same community twice
Account cannot leave a community they are not a member of
Owner cannot leave their own community (request fails with appropriate error)
```

### Community posts

```text
Member posts in a community → post has CommunityId set, appears in community feed
Non-member posts in a community → request rejected
Post with null CommunityId → personal post, not in community feed
```

### Community feed

```text
Community feed returns only posts within that community
Posts are ordered newest-first with correct pagination
Non-members cannot access private community feed
```

### Community search

```text
Search by name returns matching communities
Search by topic returns communities with that topic
Empty results return empty list, not error
```

### NPC behavior

```text
NPCs with matching interests join relevant communities (probabilistically, not always)
NPCs post in joined communities proportionally to CommunityParticipation tendency
NPCs do not join ALL communities indiscriminately
```

### Permission boundaries

```text
Regular member cannot perform admin actions (document which actions are admin-only)
Owner can perform owner actions
```

### Persistence

```text
Community data persists across server restart
Membership persists across server restart
Community posts persist across server restart
```

### Regression

```text
Existing Parts 01–14 tests still pass
```

---

## 12. Android

Part 15 is primarily a backend task. Do NOT build a full Android community browsing UI or community feed screen.

If the existing Android project requires a minimal adjustment to stay coherent with the new API surface (e.g., a data model for the community response shape matching the pattern established for existing models), make only the necessary change. Do not build community feeds, member lists, or discovery UI in this part.

---

## 13. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

Document:

- Part 15 completion
- Community entity structure and fields
- CommunityMembership entity and roles
- Community visibility levels (Public, Private, Hidden)
- Community scoping on posts (`CommunityId`)
- API endpoints (browse, details, search, by-topic, join, leave, members, account communities, community feed)
- NPC community awareness and behavior integration
- Initial community seed data
- Community vs personal post behavior (document the decision on whether community posts flow to personal feeds)
- Permission boundaries for each role
- Relevant database indexes
- Tests performed and results
- Current project status
- Next planned part

---

## 14. Git

After implementation and verification:

1. Inspect `git status`.
2. Review changed files. Ensure no generated junk or unrelated files are committed.
3. Commit the completed work.

Suggested commit message:

```text
Implement communities system (Part 15)
```

Push to `origin/main`. Verify the push actually reached the remote (fetch and compare) before reporting success.

---

## 15. DO NOT IMPLEMENT YET

Do NOT implement the following in Part 15:

```text
Community moderation (banning, content removal, warnings)
Community events
Community DMs / group chats
Community verification / official status
Nested / hierarchical communities
Community reputation / influence scores
Community feeds in Android UI
Invites / invite-only joining
Community rules / guidelines pages
Pinned posts
Community-specific notifications beyond standard notifications
Auto-generation of community membership for all NPCs
```

Those belong to later parts.

---

## 16. DEVELOPMENT PROCESS

Before changing anything:

1. Confirm `git status`/`git fetch` show a clean, synced state.
2. Inspect `Community` entity — does one already exist? (If already partially implemented in a previous part, extend rather than recreate.)
3. Inspect `CommunityMembership` entity — does one already exist?
4. Inspect `Post` entity for `CommunityId` — is it already present?
5. Inspect `AppDbContext` and existing entity configuration conventions.
6. Inspect `NpcDecisionService` / `NpcBehaviorService` to understand how to integrate community awareness into NPC decisions.
7. Inspect `FeedService` to understand how community-scoped posts should integrate (or diverge) from personal feeds.
8. Inspect the existing account population/seed data from Part 09 to understand the seeding pattern.
9. Inspect the existing authentication/authorization conventions.
10. Inspect the README.

Then implement Part 15. Do not assume a file does not exist merely because this prompt says to create it. Reuse existing functionality wherever appropriate. Do not duplicate business logic. Do not perform unrelated refactoring.

---

## 17. QUALITY REQUIREMENTS

The implementation must be:

- correct
- persistent (no automatic deletion/pruning, per the project's permanent-data rule)
- server-authoritative
- role-permission-aware (Owner, Admin, Moderator, Member)
- database-efficient (indexed, paginated, no full-table loads)
- NPC-aware (communities influence NPC behavior)
- consistent with existing entity conventions
- testable
- maintainable
- compatible with the existing architecture

---

## 18. FINAL VERIFICATION

Before declaring Part 15 complete, verify:

```text
Server builds
Communities can be browsed and searched
Communities can be joined and left
Community posts appear in community feed
Personal posts do not appear in community feed
Non-members cannot access private community feed or post
Owner cannot leave their own community
NPCs join relevant communities (probabilistically)
NPCs post in joined communities
Initial community seed data is created
Pagination correct: no duplicates, no skipped items
Community data persists across restart
Existing Parts 01–14 tests still pass
README updated
Git commit created
Git push succeeds and is verified against origin
Working tree clean
```

---

## 19. FINAL SESSION REPORT

When finished, provide a complete session report in this structure:

```text
# PART 15 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Community Architecture
...

## 5. Community Roles & Permissions
...

## 6. Community Post Scoping (Personal vs Community)
...

## 7. NPC Community Awareness
...

## 8. Database Changes
...

## 9. API Endpoints
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
15  COMPLETE

## 14. Intentionally Not Implemented
- Community moderation (banning, content removal)
- Community events
- Community DMs / group chats
- Community verification
- Nested communities
- Community reputation scores
- Android community UI

## 15. NEXT

NEXT: PART 16 — ...
```

Do not claim completion until the implementation and verification have actually succeeded.

**STOP after completing Part 15 and reporting the session log.**
