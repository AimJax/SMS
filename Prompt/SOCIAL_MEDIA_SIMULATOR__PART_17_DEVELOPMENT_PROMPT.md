# SOCIAL MEDIA SIMULATOR — PART 17 DEVELOPMENT PROMPT
## EVENT SYSTEM

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
15   Communities                    COMPLETE
16   Advanced Feed                  COMPLETE
```

Latest commit:

```text
fc70ceb — Part 16: Advanced Feed - Algorithmic scoring with configurable weights
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 16.

---

# 1. WHY THIS PART, NOW

Parts 01–16 built the core social media infrastructure with an advanced algorithmic feed. The world has accounts, posts, communities, relationships, opinions, and an intelligent feed — but it lacks **emergent narrative events**.

Part 17 introduces the **Event System** — one of the central systems named in the Master Prompt. Events are what transform a collection of posts into a living, breathing social world. Without events, the simulation is passive. With events, the world tells stories.

Events are what make the difference between:

> "I saw some posts from my friends."

and:

> "Kevin and Sarah had a huge public argument, it got news coverage, and now everyone's taking sides."

The Event System is foundational to:
- Event Causality (Part 18) — tracing why things happened
- Offline World Simulation (Part 18) — processing missed events
- Virality (Part 19) — events cause viral spikes
- Topics & Trends (Part 20) — events create and amplify trends
- Rumors (Part 21) — events generate rumors
- News (Part 22) — news accounts cover events
- Social Drama (Part 27) — events drive drama

Without the Event System, the simulation lacks narrative momentum.

---

# 2. THE EXISTING PROJECT

The existing backend already contains, among everything from Parts 01–16:

- Accounts, Profiles, Authentication, JWT
- `SocialGraphService` — Follow/Block/Mute rules
- Posts, Likes, Comments, Reposts
- `FeedService` — algorithmic feed with scoring (Part 16)
- `NotificationService` — notifications (Part 14)
- `CommunityService` — communities (Part 15)
- `NpcSimulationHostedService` — autonomous background tick loop
- `NpcBehaviorService` / `NpcDecisionService` — NPCs with personality-driven reasoning
- `AiContentGeneratorService` + provider-agnostic `IAiTextGenerationService`
- Account interests, personality traits, relationship dimensions
- World clock and simulation tick system (Part 11)

This means the infrastructure for detecting and creating events already exists as raw data:
- Viral posts trigger events
- Major follower changes trigger events
- Celebrity interactions trigger events
- Community milestones trigger events
- NPC behavior generates events through posts, follows, comments

Part 17's job is to **formalize events** as first-class entities with proper lifecycle management, consequences, and persistence.

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

Events are created, managed, and have consequences applied by the server. The server determines when an event occurs and what it causes. The client cannot create arbitrary events directly.

## Layered architecture

```text
API
Application
Domain
Infrastructure
Contracts
```

Event management follows the same layered pattern. Event logic lives in Application/Domain layers, not in controllers.

## Reuse, don't duplicate

Do not create a separate "event post" type. Events are created from existing actions (posts, follows, comments, etc.) and from NPC behavior. Do not duplicate existing entity logic.

## Permanent data rule

Per the project's established permanent-history principle (Part 01B), all events must NOT be automatically deleted/pruned. Events are permanent historical records of what happened in the world.

## Event Bus Pattern

Follow the Event Bus architecture from Section 87 of the Master Prompt:

```text
PostLiked
      │
      ├── Virality
      ├── Notification
      ├── Relationship
      ├── Memory
      ├── Feed
      ├── Metrics
      └── Event History
```

Systems should subscribe to events rather than directly depending on every other system.

---

# PART 17 OBJECTIVE

Implement a core **Event System**:

1. **Event Entity** — First-class event records with type, status, participants, and metadata.
2. **Event Bus** — Internal event dispatcher that notifies subscribers when events occur.
3. **Event Detection** — Mechanisms to detect and create events from existing actions.
4. **Event Lifecycle** — Scheduled → Active → Ended/Cancelled status transitions.
5. **Event Participation** — Track which accounts participate in events.
6. **Event Consequences** — Event-triggered world state changes.
7. **Event Queries** — APIs to browse and filter events.
8. **Event History** — All events remain permanently queryable.

Do NOT implement in this part:

- Event causality chains (Part 18) — causes and consequences are basic here, detailed chains come later
- Offline event processing (Part 18) — events happen in real-time only for now
- Event visualization in Android — backend only for now
- Event recommendations or predictions
- Event severity/impact scoring beyond basic types
- Event templates or scripted events

---

# PART 17 — REQUIRED FEATURES

## 1. Event Entity

Create an `Event` entity:

```text
Id                      (GUID)
Type                    (enum — see Section 2)
SubType                 (string — optional subtype for flexibility)
Title                   (string — human-readable event title)
Description             (string — optional longer description)
CreatorAccountId        (GUID — who/what initiated the event; nullable for system events)
CreatedAt               (timestamp)
StartAt                 (timestamp — when event becomes active)
EndAt                   (timestamp — nullable, when event ends)
Status                  (enum — Scheduled, Active, Ended, Cancelled)
Visibility              (enum — Public, FollowersOnly, CommunityOnly, Private)
Topic                   (string — primary topic tag)
CommunityId             (nullable GUID — community this event belongs to)
RelatedPostId           (nullable GUID — primary post that triggered this event)
RelatedAccountId        (nullable GUID — primary account involved)
RelatedCommunityId      (nullable GUID — secondary community)
Popularity              (int — engagement/interest level)
ParticipantCount        (int — denormalized count of participants)
MaxParticipants         (nullable int — optional cap)
Location                (string — nullable, for physical events if supported)
Metadata                (JSON — flexible key-value data for event-specific info)
IsDeleted               (bool — soft delete)
```

### Event Types (Enum)

```text
PostViral              — A post crossed viral threshold
CelebrityPost          — Celebrity made a post
CelebrityFollow        — Celebrity followed someone
CelebrityFight         — Two celebrities argued
Argument               — A public argument between accounts
Breakup                — A relationship ended publicly
NewRelationship        — A new relationship formed publicly
FollowerMilestone      — Account reached follower milestone (100, 1000, 10000, etc.)
CommunityGrowth        — Community reached membership milestone
CommunityDrama         — Drama within a community
TrendStart             — A new trend emerged
NewsCoverage           — A news account covered a story
Scandal                — A scandal or controversy broke out
RumorSpread            — A rumor started spreading
Apology                — Public apology
Announcement           — Major announcement
Challenge              — A challenge or dare
PollResults            — A poll concluded with significant results
MassUnfollow           — Mass unfollowing event
ViralRepost            — Someone's repost caused viral spread
QuotePostDrama         — Quote-post started drama
NewInfluencer          — An account became influential
AccountSuspension      — An account was suspended
ReturnFromSuspension   — An account returned from suspension
```

---

## 2. EventMembership (Participation)

Create an `EventParticipation` entity:

```text
Id                      (GUID)
EventId
AccountId
Role                    (enum — Organizer, Participant, Observer, Victim, Aggressor)
JoinedAt                (timestamp)
ContributionScore        (int — how much this account contributed to the event)
Status                  (enum — Active, Withdrew, WasRemoved, Completed)
Note                    (string — optional note, e.g., "apologized publicly")
```

Track who participated in events and their role. This enables event causality tracing.

---

## 3. Event Bus (Internal)

Create an internal event dispatcher system:

### IEventBus Interface

```csharp
Task PublishAsync<TEvent>(TEvent eventData) where TEvent : class;
void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
```

### Domain Events

Create domain event classes:

```csharp
PostViralEvent
CelebrityPostedEvent
PublicArgumentStartedEvent
RelationshipChangedEvent
FollowerMilestoneReachedEvent
CommunityMilestoneEvent
TrendEmergedEvent
NewsCoverageEvent
// etc.
```

### Event Handlers

Register handlers for each event type:

```csharp
// Notification handler — sends notifications to affected accounts
// Feed handler — surfaces event-related content
// Metrics handler — updates event metrics
// NPC handler — NPCs react to events based on personality
// Memory handler — NPCs remember important events
// Virality handler — events can trigger virality
// News handler — news accounts may cover events
```

### Design Principles

- Events are fire-and-forget from the publisher's perspective
- Handlers run asynchronously where possible
- A handler failure should not prevent other handlers from running
- Log all events for debugging
- Events should be serializable for potential future message queue

---

## 4. Event Detection

Create mechanisms to detect and create events from existing actions:

### Automatic Event Detection

Create an `EventDetectionService` that monitors:

#### Post Virality
```csharp
// When Post.Virality crosses threshold (100 likes, 1000 views, etc.)
// Create Event with Type = PostViral
// RelatedPostId = Post.Id
// RelatedAccountId = Post.AuthorId
```

#### Celebrity Activity
```csharp
// When Celebrity posts/follows/comments
// Create Event with Type = CelebrityPost/CelebrityFollow
// RelatedAccountId = Celebrity.Id
// RelatedAccountId2 = Target.Id (if applicable)
```

#### Public Arguments
```csharp
// When two accounts with mutual followers argue (comment chains with hostility keywords)
// Create Event with Type = Argument
// RelatedAccountId = Account1.Id
// RelatedAccountId2 = Account2.Id
```

#### Relationship Changes
```csharp
// When Relationship.Status changes significantly
// Create Event with Type = Breakup/NewRelationship
// RelatedAccountId = Account1.Id
// RelatedAccountId2 = Account2.Id
```

#### Milestones
```csharp
// When Account.FollowerCount crosses thresholds
// Create Event with Type = FollowerMilestone
// RelatedAccountId = Account.Id
// Metadata["FollowerCount"] = newCount
```

#### Community Events
```csharp
// When Community membership crosses thresholds
// Create Event with Type = CommunityGrowth
// RelatedCommunityId = Community.Id
```

### NPC-Initiated Events

NPCs can initiate events through their behavior:

```csharp
// When NPC creates a controversial post
// NPC decision system may trigger Event detection

// When NPC publicly attacks another account
// EventDetectionService detects argument

// When NPC creates a challenge/poll
// Event with Type = Challenge/PollResults
```

---

## 5. Event Lifecycle

Manage event status transitions:

```text
Scheduled
    ↓ (StartAt reached)
Active
    ↓ (EndAt reached OR automatic detection)
Ended
    ↓ (never, permanent)
```

### Scheduled → Active
- Triggered when `Event.StartAt <= Now`
- Publishes `EventActivatedEvent` domain event
- Notifications sent to participants
- Event becomes visible in event queries

### Active → Ended
- Triggered when `Event.EndAt <= Now` (if set)
- Or when automatic detection determines event is over (e.g., argument cooled down)
- Publishes `EventEndedEvent` domain event
- Participation records finalized
- Event remains queryable but marked as Ended

### Active → Cancelled
- Triggered by moderator action or system rules
- Publishes `EventCancelledEvent` domain event
- Participation records marked as WasRemoved

---

## 6. Event Consequences

When events occur, they can trigger consequences:

### Consequence Types

```csharp
// RelationshipChange(consequences: Account1, Account2, change: -20 Trust)
// FollowerChange(consequences: Account, delta: +500)
// FameChange(consequences: Account, delta: +10)
// PostCreation(triggered_by: Event, content: "...")
// Notification(consequences: Account, type: ..., message: "...")
// MemoryCreation(subjects: [Account1, Account2], description: "...", importance: 80)
// OpinionChange(subject: Account, target: Account/Topic, delta: -15)
// CommunityMembershipChange(community: Community, delta: +100)
// TrendCreation(topic: string, strength: 80)
```

### Consequence Execution

Events can queue consequences that execute asynchronously:

```csharp
// Event consequence is recorded immediately
// Actual state changes happen through normal services
// Consequence is logged for audit trail
```

---

## 7. Event Queries

### API Endpoints

#### Browse Events
```http
GET /api/events
```

Parameters:
- `type` — filter by event type
- `topic` — filter by topic
- `status` — Scheduled, Active, Ended (default: Active + Ended)
- `communityId` — events in a specific community
- `accountId` — events involving an account
- `cursor` — pagination cursor
- `pageSize` — items per page

Returns paginated list of events.

#### Event Details
```http
GET /api/events/{id}
```

Returns full event details with participant list.

#### Event Participants
```http
GET /api/events/{id}/participants
```

Returns paginated list of participants with their roles.

#### My Events
```http
GET /api/accounts/{id}/events
```

Returns events the authenticated account is participating in.

---

## 8. Event Notifications

When events occur, affected accounts should be notified:

- **Event Participation** — "You've been invited to join [Event Name]"
- **Event Activation** — "[Event Name] has started"
- **Event Mention** — "You were mentioned in [Event Name]"
- **Event End** — "[Event Name] has ended"

Use the existing `NotificationService` (Part 14) to send these.

---

## 9. NPC Event Awareness

NPCs should be aware of and react to events:

### Event Detection for NPCs
- NPCs are aware of events relevant to their interests
- NPCs monitor trending events in their communities
- NPCs may participate in events based on personality (DramaTendency, EventParticipation tendency)

### NPC Event Participation
```csharp
// When event is created
// Determine which NPCs might be interested
// Roll probability based on personality + event relevance
// Interested NPCs may join as Participant or Observer
```

### NPC Event Creation
```csharp
// NPCs can create events through behavior:
// - Controversial posts trigger arguments
// - Challenges trigger Challenge events
// - Polls trigger PollResults events
```

---

## 10. Database Migration

This part requires schema changes:

### New Tables
- `Event` — main event records
- `EventParticipation` — participation records
- `EventConsequence` — consequence audit log

### Indexes
- `Event(Type, Status, CreatedAt)` — browse by type
- `Event(RelatedPostId)` — find event by post
- `Event(RelatedAccountId)` — events involving account
- `Event(RelatedCommunityId)` — events in community
- `Event(Status, StartAt)` — active/scheduled events
- `EventParticipation(EventId, AccountId)` — unique membership
- `EventParticipation(AccountId, Status)` — my events

---

## 11. Tests

### Unit Tests
```text
Event entity validation
Event type enum completeness
Status transitions (Scheduled → Active → Ended)
Consequence creation and logging
Event bus publish/subscribe
Event detection triggers correctly
```

### Integration Tests
```text
Post virality creates PostViral event
Celebrity activity creates CelebrityPost event
Argument in comments creates Argument event
Milestone threshold creates FollowerMilestone event
Event query returns filtered results
Pagination works correctly
Participation records correctly
```

### Event Bus Tests
```text
Event publishes to all subscribers
Handler failure doesn't crash bus
Multiple handlers receive event
Async handlers execute properly
```

### NPC Tests
```text
NPC detects relevant events
NPC probability of participation based on personality
NPC event creation triggers detection
```

### Persistence Tests
```text
Events persist across restart
Participation persists across restart
Event history is queryable
```

### Regression Tests
```text
Existing Parts 01-16 tests still pass
```

---

## 12. Android

Part 17 is primarily a backend task. Do NOT build Android UI for events.

If the existing Android project requires a minimal adjustment (e.g., a data model for the event response shape), make only the necessary change.

---

## 13. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

Document:
- Part 17 completion
- Event entity structure
- Event types supported
- Event bus architecture
- Event detection mechanisms
- Event lifecycle management
- Event participation tracking
- Event consequences (basic)
- API endpoints
- NPC event awareness
- Database changes
- Tests performed and results
- Current project status
- Next planned part

---

## 14. Git

After implementation and verification:

1. Inspect `git status`
2. Review changed files
3. Commit with message: `Implement event system (Part 17)`
4. Push to `origin/main`
5. Verify against origin

---

## 15. DO NOT IMPLEMENT YET

Do NOT implement in Part 17:
```text
Event causality chains (causes/consequences in detail)
Offline event processing
Event UI in Android
Event recommendations
Event severity scoring
Event templates/scripting
Event predictions
Event analytics dashboard
Event moderation tools
```

---

## 16. DEVELOPMENT PROCESS

Before changing anything:

1. Confirm `git status`/`git fetch` show clean, synced state
2. Inspect existing entity conventions
3. Inspect NPC behavior service for event creation integration
4. Inspect notification service for event notifications
5. Inspect feed service for event integration
6. Inspect world clock and tick system
7. Inspect the README

Then implement Part 17. Reuse existing functionality. Do not duplicate logic.

---

## 17. QUALITY REQUIREMENTS

The implementation must be:
- Correct
- Permanent (events never auto-deleted)
- Server-authoritative
- Extensible (easy to add new event types)
- Performant (event queries use indexes)
- Testable
- Maintainable

---

## 18. FINAL VERIFICATION

Before declaring Part 17 complete, verify:

```text
Server builds
Event entity and tables created
Event bus dispatches correctly
Events detected from posts, relationships, milestones
Event lifecycle transitions work
Participation tracked correctly
Event queries return correct results
Notifications sent for event updates
NPCs aware of relevant events
Events persist across restart
Existing Parts 01-16 tests still pass
README updated
Git commit created and pushed
Working tree clean
```

---

## 19. FINAL SESSION REPORT

```text
# PART 17 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Event Architecture
...

## 5. Event Types Supported
...

## 6. Event Bus Implementation
...

## 7. Event Detection Mechanisms
...

## 8. Event Lifecycle
...

## 9. NPC Event Awareness
...

## 10. API Endpoints
...

## 11. Database Changes
...

## 12. Tests
...

## 13. README
Updated: YES
...

## 14. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 15. Current Project Status
01A-17 COMPLETE

## 16. Intentionally Not Implemented
- Event causality chains
- Offline event processing
- Android event UI
- Event recommendations

## 17. NEXT
NEXT: PART 18 — Event Causality & Offline Simulation
```

**STOP after completing Part 17 and reporting the session log.**
