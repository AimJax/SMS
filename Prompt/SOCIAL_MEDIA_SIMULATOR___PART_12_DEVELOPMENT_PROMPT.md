# SOCIAL MEDIA SIMULATOR — PART 12 DEVELOPMENT PROMPT
## NPC SOCIAL GRAPH (NPC-TO-NPC FOLLOWS & RELATIONSHIPS)

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
```

Latest commit:

```text
e6c4c0e — Implement NPC background simulation loop (Part 11)
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

---

# 0. FIRST ACTION — RESOLVE PART 11 PUSH STATE

Part 11's session report recorded:

```text
Push: TIMEOUT (local commit exists, push pending network)
```

Before doing **any** Part 12 work:

1. Run `git status` and `git log --oneline -5` and confirm the local commit `e6c4c0e` (or whatever the actual latest local commit is) exists.
2. Attempt `git push origin main` again.
3. Verify with `git log origin/main --oneline -1` (or equivalent, e.g. `git fetch` then compare) that the remote actually now matches local.
4. Do NOT force-push. Do NOT rewrite history to "fix" this. If the push still fails, STOP, report the exact error, and do not proceed to Part 12 implementation until the repository is confirmed in sync (or you have explicit instruction to proceed anyway).
5. Only once local and remote are confirmed aligned should you continue.

Report this resolution explicitly in the final session report for this part, even though it is not new Part 12 functionality.

---

# 1. THE EXISTING PROJECT

The existing backend already contains:

- ASP.NET Core server, layered architecture, EF Core, SQLite
- Accounts, Profiles, Authentication, JWT
- `SocialGraphService` — Follow relationships, Blocks, Mutes (currently player-account-oriented, used by NPC actions via `NpcBehaviorService` for things like following/unfollowing player-facing accounts)
- Posts, Likes, Comments, soft deletion
- Chronological feed with pagination
- NPC profiles, personalities (Big Five), interests, account types (OrdinaryUser, Creator, Influencer, Celebrity, Official, News)
- `NpcSimulationService`, `NpcBehaviorService`, `NpcDecisionService`, `ContentRelevanceService`, `ContentGeneratorService`
- `NpcSimulationHostedService` — autonomous background tick loop (Part 11), with pause/resume/status admin endpoints
- NPC action history (`NpcAction`)
- 127 passing automated tests

Currently, NPCs can already:

```text
Browse feeds
Like / unlike posts
Comment on posts
Follow / unfollow accounts
Create posts
```

via the existing `Follow` action type and `SocialGraphService`. What is **missing** is any deliberate, personality-and-interest-driven logic for **which** NPCs follow **which other NPCs**, and the resulting shape of the NPC-to-NPC social graph over time (clusters, hubs, mutual follows, etc.).

---

# MASTER ARCHITECTURE PRINCIPLES

Continue following the established master prompt.

## Server authoritative

NPC social connections are simulation state owned entirely by the server. The graph must emerge from the background tick loop, not from any client action.

## Layered architecture

```text
API
Application
Domain
Infrastructure
Contracts
```

Do not put graph-shaping logic in controllers. Do not put it directly inside `NpcSimulationHostedService` — that service's job remains scheduling/lifetime only.

## Reuse, don't duplicate

`SocialGraphService` (Part 05) already enforces follow/block/mute rules and already persists `Follow` relationships as ordinary account-to-account edges. NPCs are accounts. **Do not create a second, parallel relationship system for NPCs.** NPC-to-NPC follows must be the same `Follow` entity/table used for player follows, subject to the same block rules.

## Performance

With hundreds to thousands of NPCs eventually running continuous ticks, the follow-decision logic must not require loading the entire account table or the entire existing follow graph into memory on every tick for every NPC.

---

# PART 12 OBJECTIVE

Give NPCs a genuine reason to follow (and occasionally unfollow) other accounts — especially other NPCs — driven by personality, interests, and account type, so that a believable social graph emerges organically as the background simulation runs over time.

This part is about **decision quality and candidate selection for NPC-to-NPC follows**, built on top of the *existing* follow mechanism. It is NOT about building a new relationship system.

Do NOT implement:

- A new "friendship" or "relationship strength" entity (existing `Follow` + `NpcAction` history is sufficient for now).
- Mutual-follow-only "friend" concept.
- Direct messages or private interactions.
- Any UI for visualizing the graph (that's a later/optional part).

---

# PART 12 — REQUIRED FEATURES

## 1. Inspect existing follow-decision behavior first

Before writing anything new, inspect exactly how `NpcDecisionService` currently scores/selects the `Follow` action and how a target account is currently chosen (if it is chosen at all — it may currently be naive/random or limited to non-NPC accounts). Document what you find. Do not assume; verify by reading the code and, if needed, existing tests.

---

## 2. Candidate selection for follow targets

Implement a bounded, query-efficient way to produce a small set of **candidate accounts** an NPC might follow on a given tick. Candidates should reasonably include:

- Accounts (NPC or player) whose declared/derived interests overlap with the NPC's own interests (reuse `NpcInterest` / `ContentRelevanceService` concepts where they already fit — do not build a second interest-matching system from scratch).
- Accounts the NPC has recently interacted with positively (e.g., liked or commented on their posts) per existing `NpcAction` history, since real users often follow people whose content they've already engaged with.
- A small amount of "discovery" — some randomness/exploration so the graph doesn't become purely deterministic/cliquish, modulated by the NPC's Openness trait.

Do NOT scan the entire accounts table per NPC per tick. Use targeted, indexed queries (e.g., "accounts posting about interest X", "accounts I recently liked/commented on") bounded to a small candidate count (document your chosen bound, e.g. top 10–20 candidates).

---

## 3. Personality-influenced follow decisions

Extend (not replace) the existing `NpcDecisionService` scoring so that whether/whom an NPC follows is influenced by the established Big Five traits already used elsewhere in the project:

```text
Extraversion   → more frequent following, more outgoing connections
Openness       → more willingness to follow outside existing interest clusters
Agreeableness  → more likely to follow back / reciprocate
Neuroticism    → more cautious/slower to follow unfamiliar accounts
Conscientiousness → more deliberate, interest-aligned follow choices vs. impulsive
```

Reuse the same modifier pattern already established in Part 10 for other actions. Do not invent a second personality-influence mechanism.

---

## 4. Account-type influenced follow behavior

Extend existing account-type modifiers (already used for posting frequency etc. in Part 10) to also shape following behavior, for example:

```text
Celebrities / Influencers → followed disproportionately often by others, rarely follow back
Ordinary users / Lurkers  → follow more, are followed less
News / Official accounts  → followed for relevance/topicality rather than personality fit
Creators                  → follow accounts within their content niche
```

Document the exact modifiers chosen. Keep them consistent with the account-type modifier table already established in Part 10's README section — extend it, don't contradict it.

---

## 5. Reciprocity (follow-back) as a distinct, lightweight behavior

When account A follows account B, give B's *future* ticks a modestly increased chance of following A back, weighted by B's Agreeableness and account type (see Section 4). This should be expressed as an input into the existing candidate/scoring pipeline (Section 2–3) — do NOT build a separate "pending follow-back queue" subsystem. A simple, efficient query (e.g., "accounts that follow me but I don't follow back, ordered by recency, small bound") feeding into candidate selection is sufficient.

---

## 6. Occasional unfollow

NPCs should occasionally unfollow accounts, reusing the existing `Unfollow` action type already defined in `NpcAction`/`NpcActionType`. At minimum, support unfollowing when:

- The NPC hasn't seen relevant content from that account in a long time (i.e., low ongoing interest relevance), or
- A simple probabilistic "churn" factor modulated by Neuroticism/Conscientiousness.

Keep this simple. Do not build an elaborate relationship-decay scoring system. Document the exact rule(s) used.

---

## 7. Respect all existing social graph rules

NPC-to-NPC (and NPC-to-player) follow behavior must go through the existing `SocialGraphService` and respect every rule already established in Part 05:

```text
Cannot follow self
Cannot follow if blocked in either direction
Cannot follow if already following
Mutes/blocks are respected the same way for NPCs as for any account
```

Do not special-case NPCs to bypass these rules. If inspection reveals `SocialGraphService` needs a small addition (e.g., a bulk "am I already following any of these candidate IDs" check for efficiency), extend it — do not fork its logic.

---

## 8. Query performance

The candidate-selection and reciprocity queries (Sections 2 and 5) must be database-side, indexed, and bounded — no loading the full follow graph or full account list into memory per NPC per tick. Inspect existing indexes from Parts 05–07 before adding new ones; only add an index if profiling/inspection actually justifies it, and document the justification.

---

## 9. Interaction with the background loop

This logic plugs into the existing `NpcBehaviorService` / `NpcDecisionService` pipeline invoked each tick by `NpcSimulationHostedService` (Part 11). Do not create a second, separate loop or timer for social-graph updates. Reuse `MaxNpcsPerTick` and the existing overlap-prevention/failure-isolation guarantees from Part 11 — a bug in follow-target selection for one NPC must not crash the tick or the loop.

---

## 10. Observability

Extend the existing `GET /api/admin/simulation/status` endpoint (Part 11) — or add one small, clearly-scoped additional read-only admin endpoint if that fits better — with basic NPC social graph metrics useful for verifying this part works, for example:

```text
Total NPC-to-NPC follow edges
Follows created in the last tick / recent period
Unfollows created in the last tick / recent period
```

Do not build a graph visualization. Do not build full analytics. Keep this to simple counts derived from existing data (`Follow` table + `NpcAction` history).

---

## 11. Tests

Add tests appropriate to this part. At minimum verify:

### Candidate selection

```text
NPC with interest X is more likely to have accounts posting about X in its candidate set
than unrelated accounts
```

### Personality influence

```text
High-Extraversion NPC follows more often than low-Extraversion NPC, all else equal
High-Openness NPC follows outside its interest cluster more often than low-Openness NPC
```

### Reciprocity

```text
Account A follows Account B
Given enough ticks, B (with reasonable Agreeableness) has an increased chance of following A back
compared to a baseline unrelated account
```

### Unfollow

```text
NPC unfollow behavior is exercised and reduces its active follow count appropriately
Unfollow respects the same SocialGraphService rules as follow
```

### Social graph rule compliance

```text
NPC never ends up following itself
NPC never follows an account that has blocked it (or that it has blocked)
NPC never double-follows an account it already follows
```

### Failure isolation regression

```text
A forced exception in follow-target selection for one NPC does not crash the tick
or stop the background loop (reuse Part 11's failure-isolation guarantee)
```

### Persistence

```text
NPC follows created during simulated ticks persist across a server restart
```

### Regression

```text
Existing feed, posts, accounts, player-facing social graph, and background-service tests
(Parts 05-11) still pass
```

---

## 12. Database migration

Only create a migration if this part requires schema changes. Reusing the existing `Follow`/`NpcAction` entities should mean no new tables are needed. If you do add anything (e.g., a nullable column, a new index), document exactly why the existing schema was insufficient.

---

## 13. Android

Part 12 is backend-only. Do NOT build any Android UI for this part.

---

## 14. README — REQUIRED

At the end of this part, **UPDATE `README.md`**.

Document:

- Part 12 completion
- Part 11 push-state resolution (Section 0)
- NPC social graph decision architecture (how it plugs into existing `NpcDecisionService`/`NpcBehaviorService`)
- Candidate selection strategy and bounds
- Personality → follow-behavior mapping (extend, don't duplicate, the Part 10 table)
- Account-type → follow-behavior mapping (extend, don't duplicate, the Part 10 table)
- Reciprocity (follow-back) rule
- Unfollow rule
- Confirmation that existing `SocialGraphService` rules are reused unmodified (or document any justified extension)
- New/extended observability metrics
- Tests performed and results
- Current project status
- Next planned part

---

## 15. Git

After implementation and verification:

1. Inspect `git status`.
2. Review changed files.
3. Ensure no generated junk, logs, or unrelated files are committed.
4. Commit the completed work.

Suggested commit message:

```text
Implement NPC-to-NPC social graph behavior (Part 12)
```

Push to `origin/main`. Verify the push actually reached the remote (see Section 0's verification approach) before reporting success — do not report success based on a command exiting without a visible error alone if a timeout was possible.

---

## 16. DO NOT IMPLEMENT YET

Do NOT implement the following in Part 12:

```text
LLM / Ollama / Qwen content generation
"Friendship strength" or relationship-weight entities beyond existing Follow/NpcAction
Direct messages
Notifications
Graph visualization / admin dashboard UI
Trending/virality mechanics based on the social graph
Android UI
Multi-tier simulation (active vs dormant NPC pools)
```

Those belong to later parts.

---

## 17. DEVELOPMENT PROCESS

Before changing anything:

1. Resolve the Part 11 push state (Section 0).
2. Inspect `SocialGraphService` and its existing rules/tests.
3. Inspect `NpcDecisionService`, `NpcBehaviorService`, `ContentRelevanceService`.
4. Inspect `NpcAction` / `NpcActionType` for existing `Follow`/`Unfollow` handling.
5. Inspect `Follow` entity and existing indexes.
6. Inspect `NpcSimulationHostedService` and `SimulationStateService` (Part 11) to understand how a tick is invoked and how failures are isolated.
7. Inspect the existing `/api/admin/simulation/status` response shape.
8. Inspect existing tests for decision-service and behavior-service patterns.
9. Inspect the README.

Then implement Part 12. Do not assume a file does not exist merely because this prompt says to create it. Reuse existing functionality wherever appropriate. Do not duplicate business logic. Do not perform unrelated refactoring.

---

## 18. QUALITY REQUIREMENTS

The implementation must be:

- correct
- persistent
- server-authoritative
- consistent with existing social graph rules (no bypasses)
- database-efficient (bounded, indexed queries — no full-table scans per NPC per tick)
- resilient to individual NPC failures (reuses Part 11 isolation)
- testable
- maintainable
- compatible with the existing architecture

---

## 19. FINAL VERIFICATION

Before declaring Part 12 complete, verify:

```text
Part 11 commit confirmed pushed to origin/main
Server builds
Background loop still starts/stops/pauses/resumes correctly (Part 11 regression)
NPC follow-candidate selection is interest- and history-aware, not naive/random-only
Personality modifiers measurably affect follow behavior
Account-type modifiers measurably affect follow behavior
Reciprocity (follow-back tendency) behaves as designed
Unfollow behavior works and respects the same rules as follow
No self-follows, no bypassed blocks/mutes, no duplicate follows
A forced per-NPC failure does not crash the tick loop
Follows/unfollows persist across a server restart
Observability metrics reflect real data
Existing endpoints and Parts 05-11 tests still pass
README updated
Git commit created
Git push succeeds and is verified against origin
Working tree clean
```

---

## 20. FINAL SESSION REPORT

When finished, provide a complete session report in this structure:

```text
# PART 12 — COMPLETE

## 1. Part 11 Push Resolution
...

## 2. What Was Inspected
...

## 3. What Already Existed
...

## 4. What Changed
...

## 5. NPC Social Graph Decision Architecture
...

## 6. Candidate Selection Strategy
...

## 7. Personality & Account-Type Influence
...

## 8. Reciprocity & Unfollow Rules
...

## 9. Social Graph Rule Compliance
...

## 10. Observability
...

## 11. Tests
...

## 12. README
Updated: YES
...

## 13. Git
Commit: ...
Push: ...
Verified against origin: ...
Working tree: ...

## 14. Current Project Status

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

## 15. Intentionally Not Implemented
- LLM / Ollama / Qwen integration
- Direct messages
- Graph visualization / admin dashboard
- Android UI

## 16. NEXT

NEXT: PART 13 — ...
```

Do not claim completion until the implementation and verification have actually succeeded.

**STOP after completing Part 12 and reporting the session log.**
