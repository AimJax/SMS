# PART 10 — NPC BEHAVIOR SIMULATION
## Development Instruction for the AI Coding Agent

You are continuing development of the **Social Media Simulator** project.

The project is a standalone social-media simulation game with:
- Unity Android client
- ASP.NET Core backend
- SQLite + EF Core persistence
- JWT authentication
- Social graph
- Posts and engagement
- Server-side feed
- Large NPC population
- Deterministic NPC generation

The current project has completed Parts 01A–09.

Your task is to implement **PART 10 — NPC BEHAVIOR SIMULATION**.

---

# 1. CURRENT PROJECT STATE

Completed:

```text
01A  COMPLETE — Development Environment
01B  COMPLETE — Repository Foundation
01C  COMPLETE — ASP.NET Core Server
01D  COMPLETE — SQLite Foundation
01E  COMPLETE — Android Client Foundation
01F  COMPLETE — Foundation Checkpoint
02   COMPLETE — Backend Architecture
03   COMPLETE — Persistence
04   COMPLETE — Accounts & Authentication
05   COMPLETE — Social Graph
06   COMPLETE — Posts & Engagement
07   COMPLETE — Feed & Timeline
08   COMPLETE — NPC Simulator Foundation
09   COMPLETE — NPC Population Generation
10   IN PROGRESS — NPC Behavior Simulation
```

Existing architecture:

```text
Account
├── Profile
├── AccountHistory
└── NpcProfile
      ├── NpcPersonality
      ├── NpcInterest[]
      └── NpcAction[]

Social Graph:
Account
├── Follow
├── Block
└── Mute

Content:
Post
├── PostLike
└── Comment

Feed:
Account → FeedService → Posts from followed accounts
```

Part 08 established:
- NpcProfile
- NpcPersonality
- NpcInterest
- NpcAction
- INpcService / NpcService
- INpcSimulationService / NpcSimulationService

Part 09 established:
- PopulationConfig
- PopulationResult
- INpcPopulationService / NpcPopulationService
- UsernameGenerator
- ProfileGenerator
- deterministic NPC generation
- configurable account-type distribution
- large population generation
- duplicate prevention

The project supports **1000 NPC population generation** and must remain designed for significantly larger populations.

---

# 2. PRIMARY GOAL

Build the first real **NPC decision-making system**.

NPCs must stop being merely generated records and become simulated social-media users capable of:

- browsing
- following
- unfollowing
- liking
- unliking
- commenting
- posting
- reading/ignoring content
- interacting with accounts
- respecting blocks
- respecting mutes
- behavior influenced by personality
- behavior influenced by interests
- behavior influenced by account type
- behavior influenced by activity state

The system should create **emergent behavior**, not scripted NPC stories.

---

# 3. REQUIRED BEHAVIOR PIPELINE

Build a real pipeline:

```text
NPC becomes due
      ↓
Determine current activity
      ↓
Gather relevant social/content context
      ↓
Evaluate candidate actions
      ↓
Score actions
      ↓
Choose action
      ↓
Validate social/security rules
      ↓
Execute action transactionally
      ↓
Update NPC state
      ↓
Schedule next simulation
```

Keep this modular so future parts can replace or extend the decision engine without rewriting persistence, social graph, posts, or NPC identity.

---

# 4. BEHAVIOR/ACTION MODEL

Use the existing `NpcAction` infrastructure where appropriate.

Support action types such as:

```text
Browse
Follow
Unfollow
Like
Unlike
Comment
Post
Read
Engage
Idle
```

Inspect the existing model before changing it. Do not create redundant systems.

If additional fields are required, use the project's existing conventions. Useful concepts include:

```text
ActionType
TargetAccountId
TargetPostId
TargetCommentId
CreatedAt
ExecutedAt
Success
FailureReason
```

Only add fields that are actually needed.

---

# 5. BEHAVIOR SERVICES

Create a dedicated behavior layer following existing architecture.

Recommended structure:

```text
Application/Services/
├── INpcBehaviorService.cs
├── NpcBehaviorService.cs
├── INpcDecisionService.cs
└── NpcDecisionService.cs
```

Exact organization may follow existing conventions.

`INpcBehaviorService` should coordinate execution of NPC simulation behavior.

`INpcDecisionService` should handle candidate evaluation and decision making.

Do not create abstractions that do not provide meaningful separation.

---

# 6. DECISION-MAKING

NPC decisions MUST be influenced by existing NPC data.

Big Five personality:

```text
Openness
Conscientiousness
Extraversion
Agreeableness
Neuroticism
```

Examples:

- Higher Extraversion → more social interaction
- Higher Openness → broader exploration
- Higher Agreeableness → greater positive engagement tendency
- Higher Conscientiousness → more consistent/predictable activity
- Higher Neuroticism → potentially more cautious/avoidant behavior

Use continuous probability/weight effects, not rigid rules.

Bad:

```text
if Extraversion > 0.8 then always comment
```

Prefer continuous scoring.

---

# 7. INTEREST-DRIVEN BEHAVIOR

Use existing `NpcInterest` values.

Interest categories include:

```text
Gaming
Politics
Sports
Technology
Music
Movies
Television
Fashion
Food
Travel
Science
Health
Business
Finance
Education
LocalNews
WorldNews
Entertainment
GamingNews
SportsNews
TechNews
```

NPCs should be more likely to:

- read relevant posts
- like relevant posts
- comment on relevant posts
- follow relevant accounts
- create posts related to strong interests

Interest strength must matter.

A strength of `0.95` should produce meaningfully stronger relevance than `0.30`.

Do not make NPCs behave identically.

---

# 8. ACCOUNT-TYPE BEHAVIOR

Existing account types:

```text
OrdinaryUser
Creator
Influencer
Celebrity
Official
News
```

Account type must influence behavioral tendencies.

Examples:

### OrdinaryUser
- mostly browse
- follow accounts
- occasional likes/comments
- occasional posts

### Creator
- more frequent posting
- audience engagement
- relevant follows

### Influencer
- frequent posting
- follower engagement
- trend/relevant-account exploration

### Celebrity
- selective following
- high visibility
- selective engagement

### Official
- information-oriented posting
- controlled engagement

### News
- frequent posting
- news-related behavior
- relevant-topic engagement

These are tendencies, NOT rigid scripts.

---

# 9. SOCIAL GRAPH RULES

NPC behavior MUST respect Part 05.

Before executing an action, validate the relevant graph rules.

Follow:
- cannot follow itself
- cannot follow an account that blocks it
- cannot follow an account it blocks

Like/comment:
- do not interact with content when blocked in either direction

Feed/discovery:
- continue respecting blocks and mutes

Unfollow:
- only operate on an existing relationship

Reuse existing `SocialGraphService` rules rather than duplicating them whenever possible.

---

# 10. CANDIDATE ACTIONS

Possible actions:

```text
Idle
Browse
Follow
Unfollow
Like
Unlike
Comment
Post
```

Filter impossible actions before scoring.

Examples:

```text
Already following → cannot Follow
Not following → cannot Unfollow
Already liked → cannot Like
Not liked → cannot Unlike
No valid post → cannot Comment
```

---

# 11. ACTION SCORING

Use a deterministic and testable scoring system.

Conceptually:

```text
BaseActionWeight
+ PersonalityModifier
+ InterestModifier
+ AccountTypeModifier
+ SocialRelationshipModifier
+ ContentRelevanceModifier
+ ActivityModifier
+ RandomVariation
```

Exact mathematics is up to the implementation.

Requirements:
- understandable scoring
- normalized values where practical
- no arbitrary giant constants
- injectable/seedable randomness
- no hidden global state
- no LLM dependency

---

# 12. CONTENT RELEVANCE

NPCs need a lightweight content-relevance mechanism.

Do NOT introduce heavyweight ML/embeddings yet.

Use available project information:
- post text/category
- NPC interests
- author account type
- relationship
- engagement
- recency

If explicit content categorization does not exist, implement a simple deterministic keyword/category mechanism.

Keep it replaceable so a future LLM or richer classifier can replace it without rewriting the behavior engine.

---

# 13. POSTING

NPCs must be able to decide to post.

Posting should depend on:
- account type
- personality
- interests
- activity state
- recent behavior
- cooldowns

Do NOT integrate the LLM yet.

For Part 10, content may be deterministic/template-based, derived from NPC interests/account type.

Prefer an abstraction such as:

```text
GeneratePostContent(...)
```

with a deterministic local implementation that can later be replaced by LLM generation.

---

# 14. COMMENTING

NPCs must be able to decide whether to comment.

Influences:
- personality
- interest relevance
- relationship with author
- account type
- post relevance
- existing engagement
- activity state

Comments should not all be identical.

Use deterministic templates/variations based on:
- personality
- account type
- interests
- post context

Possible categories:

```text
positive reaction
agreement
short opinion
question
interest-specific reaction
```

---

# 15. FOLLOWING

NPCs should discover and follow accounts using:

- shared interests
- account type compatibility
- social relationships
- content relevance
- activity
- controlled random exploration

Avoid every NPC following the same popular accounts.

The graph should grow naturally.

---

# 16. UNFOLLOWING

NPCs should occasionally reconsider existing follows.

Potential signals:
- low relevance
- prolonged inactivity
- excessive following
- exploration
- changing interests

Do not make unfollowing overly aggressive.

Relationships should persist.

---

# 17. ACTIVITY STATES

Reuse existing states:

```text
Idle
Browsing
Posting
Reading
Engaging
Offline
```

Example transitions:

```text
Idle → Browsing → Reading → Engaging → Idle
```

or:

```text
Idle → Posting → Idle
```

Do not break the existing account-type simulation intervals from Part 08.

---

# 18. SIMULATION INTEGRATION

Integrate with:

```text
INpcSimulationService
NpcSimulationService
```

Do NOT create a second scheduler.

Conceptually:

```text
GetDueNpcs
    ↓
Process NPC simulation
    ↓
Process NPC behavior
    ↓
Update simulation state
    ↓
Set next simulation time
```

The existing scheduling mechanism remains authoritative.

Unless the master plan explicitly requires it here, do not create a permanently running hosted worker yet.

---

# 19. PERFORMANCE

The simulator must scale beyond 1000 NPCs.

Do NOT implement:

```text
every NPC × every Account × every Post
```

Avoid catastrophic O(N²) or O(N×Posts) behavior.

Use:
- recent posts
- bounded candidate sets
- indexed queries
- targeted account selection
- `AsNoTracking()` for read-only queries where appropriate
- async EF Core operations
- batching where appropriate
- minimal database round trips

**Do not add pruning/deletion systems to make performance pass.**

Keep simulated entities/history intact.

---

# 20. TRANSACTIONS & DATA INTEGRITY

Use the existing Unit of Work when one behavior action changes multiple pieces of state.

Examples:

```text
Follow + NpcAction
Like + NpcAction
Comment + NpcAction
Post + NpcAction
```

Do not record successful actions when the underlying operation failed.

Handle failures cleanly.

---

# 21. NPC ACTION HISTORY

`NpcAction` should represent meaningful simulation history.

Record what NPCs actually attempted/executed.

Useful information:

```text
NPC
Action type
Target
Timestamp
Success/failure
Failure reason
```

Actual graph/content tables remain authoritative.

---

# 22. TESTING

Add comprehensive tests.

### Decision tests
- deterministic decision with fixed seed
- personality affects weighting
- interests affect relevance
- account type affects behavior
- invalid candidates are rejected
- candidate filtering works

### Follow
- valid follow works
- self-follow rejected
- blocked account rejected
- duplicate follow prevented
- action recorded

### Like
- valid like works
- duplicate like prevented
- blocked content excluded
- action recorded

### Comment
- valid comment works
- blocked content excluded
- comment persisted
- action recorded

### Post
- valid post works
- cooldown/limits respected
- action recorded

### Unfollow
- valid unfollow works
- invalid unfollow safely rejected

### Simulation
- due NPC processed
- inactive NPC does not act
- disabled/suspended/banned account does not act
- activity state updates
- next simulation time updates
- simulation version increments

### Persistence
- behavior survives restart
- follows persist
- likes persist
- comments persist
- posts persist
- action history persists

---

# 23. PERFORMANCE TESTS

Benchmark:

```text
100 NPCs
1,000 NPCs
5,000 NPCs
10,000 NPCs
```

Measure:

```text
total processing time
average time per NPC
database operations where practical
```

Do not delete/prune NPCs to improve results.

If a benchmark is slow, report the real result and identify bottlenecks.

---

# 24. NO LLM YET

Do NOT integrate:

```text
Ollama
Qwen
OpenAI
cloud LLM APIs
embeddings
vector databases
```

A future-facing interface is acceptable if useful, but Part 10 itself should use deterministic/local behavior.

---

# 25. NO ADVANCED FEED ALGORITHM YET

Do not replace Part 07's chronological/social feed with:
- recommendation ML
- engagement ranking
- trending
- advanced personalization

Future feed work belongs to later parts.

---

# 26. NO PUBLIC NPC API

NPC behavior is internal backend infrastructure.

Do not expose NPC control to ordinary users.

If a diagnostic/admin endpoint is absolutely required for testing, keep it minimal and secured.

---

# 27. DATABASE

Only make database changes that are genuinely required.

Possible changes:
- extending `NpcAction`
- indexes needed for behavior queries
- cooldown/state fields if missing

Inspect the existing schema first.

If schema changes are required:
- create an EF Core migration
- verify it
- preserve existing data

Do not manually edit SQLite instead of using migrations.

---

# 28. CODE QUALITY

Follow existing project conventions.

Requirements:
- nullable reference types respected
- async APIs
- cancellation tokens where appropriate
- dependency injection
- meaningful interfaces
- no god classes
- no duplicated business rules
- centralized configuration for important behavior constants
- meaningful logging
- clean error handling
- no dead code
- no unnecessary packages

Do not rewrite working Parts 01–09 merely for style.

---

# 29. SECURITY

NPC behavior must obey the same business/domain invariants as normal users wherever possible.

Do not bypass:
- ownership
- blocks
- mutes
- uniqueness
- database integrity
- authorization assumptions

Server-controlled NPCs must not bypass domain rules.

---

# 30. README — MANDATORY

At the end of Part 10, **UPDATE `D:\SMS\README.md` based on exactly what was actually implemented.**

Do not forget this.

Document:
- Part 10 status
- behavior architecture
- behavior pipeline
- action types
- decision system
- personality influence
- interest influence
- account-type influence
- social graph restrictions
- activity states
- NPC action history
- posting/commenting behavior
- performance results
- test results
- database changes
- intentionally deferred functionality

Update project status to:

```text
10   COMPLETE — NPC Behavior Simulation
```

Keep Parts 01A–09 documentation intact.

**Never claim an implementation or test result that was not actually verified.**

---

# 31. GIT — MANDATORY

After implementation:

1. Run relevant tests.
2. Run the full test suite.
3. Build the server.
4. Verify migrations/database.
5. Inspect git diff.
6. Ensure no accidental files are staged.
7. Update README.
8. Commit Part 10.
9. Push to `origin/main`.
10. Verify working tree is clean.

Recommended commit:

```text
Implement NPC behavior simulation (Part 10)
```

Do not commit with failing tests.

Do not claim success without verification.

---

# 32. FINAL VERIFICATION CHECKLIST

```text
[ ] Existing Parts 01–09 still build
[ ] Full test suite passes
[ ] NPC behavior service exists
[ ] Decision service exists where justified
[ ] Personality affects behavior
[ ] Interests affect behavior
[ ] Account type affects behavior
[ ] Follow behavior works
[ ] Unfollow behavior works
[ ] Like behavior works
[ ] Comment behavior works
[ ] Post behavior works
[ ] Invalid actions are rejected
[ ] Block rules are respected
[ ] Mute rules are respected where applicable
[ ] Activity states work
[ ] NPC action history is persisted
[ ] Simulation scheduler integration works
[ ] 100 NPC performance test completed
[ ] 1,000 NPC performance test completed
[ ] 5,000 NPC performance test completed
[ ] 10,000 NPC performance test completed
[ ] No pruning/deletion added for performance
[ ] No LLM integration added
[ ] No advanced feed algorithm added
[ ] README updated
[ ] Git commit created
[ ] Pushed to origin/main
[ ] Working tree clean
```

---

# 33. IMPLEMENT, DON'T JUST DESCRIBE

Do not respond with architecture alone.

Inspect the actual repository and implement Part 10.

Before changing files:
- inspect existing implementations
- reuse existing services
- reuse existing domain rules
- follow naming conventions
- avoid duplicate functionality

After changing files:
- compile
- test
- fix failures
- run performance tests
- update README
- commit
- push

If this specification conflicts with an established project convention, preserve the existing architecture after inspecting the actual implementation.

Do not silently remove existing functionality.

---

# 34. REQUIRED COMPLETION REPORT

When finished, provide:

```text
# PART 10 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. NPC Behavior Architecture
...

## 5. Decision System
...

## 6. Supported Actions
...

## 7. Social Graph Rules
...

## 8. Database Changes
...

## 9. Tests
...

## 10. Performance
...

## 11. README
Updated: YES

## 12. Git
Commit: <actual hash>
Push: SUCCESS
Working tree: CLEAN

## 13. Current Project Status
01A COMPLETE
...
10 COMPLETE

## 14. Intentionally Not Implemented
...

## 15. NEXT
NEXT: PART 11 — <next part from the master plan>
```

Only report actual verified results.

---

# 35. STOP CONDITION

When Part 10 is fully implemented, tested, documented, committed, and pushed:

```text
STOP — awaiting next instruction.
```

Do not automatically begin Part 11.

# END OF PART 10 INSTRUCTION
