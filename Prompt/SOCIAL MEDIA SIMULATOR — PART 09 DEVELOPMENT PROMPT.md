# SOCIAL MEDIA SIMULATOR — PART 09 DEVELOPMENT TASK

## PART 09 — NPC POPULATION GENERATION

You are continuing development of the **Social Media Simulator** project.

This is **Part 09** of the master development plan.

The project has completed Parts **01A–08**. Do NOT restart, redesign, or replace existing architecture unless you discover a concrete bug or architectural violation that must be fixed.

Work directly from the existing project state.

---

# 1. CURRENT PROJECT STATE

The project currently has:

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
09   CURRENT — NPC Population Generation
```

GitHub repository:

```text
Username: AimJax
Branch: main
```

Part 08 ended with:

```text
NEXT: PART 09 — NPC POPULATION GENERATION
```

---

# 2. IMPORTANT EXISTING ARCHITECTURE

Do NOT duplicate systems that already exist.

Part 08 already implemented:

### Account Types

```text
OrdinaryUser
Creator
Influencer
Celebrity
Official
News
```

### Account Status

```text
Active
Disabled
Suspended
Banned
```

### NPC Entities

```text
NpcProfile
NpcPersonality
NpcInterest
NpcAction
```

### NPC Services

```text
INpcService
NpcService

INpcSimulationService
NpcSimulationService
```

### NPC Personality

Big Five:

```text
Openness
Conscientiousness
Extraversion
Agreeableness
Neuroticism
```

Values:

```text
0.0 - 1.0
```

### NPC Interests

21 supported categories:

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

Each NPC currently receives:

```text
5 interests
```

with strength:

```text
0.3 - 1.0
```

### Simulation Intervals

```text
Celebrity      15s
News           20s
Influencer     25s
Creator        30s
Official       45s
OrdinaryUser   30s
```

### Existing NPC Architecture

```text
Account
   │
   └── NpcProfile
           ├── NpcPersonality
           ├── NpcInterest[]
           └── NpcAction[]
```

---

# 3. PART 09 OBJECTIVE

Implement a robust **NPC Population Generation system**.

The goal is to be able to generate a large simulated social-media population from the existing NPC infrastructure.

This system must create realistic diversity across:

- account types
- usernames
- display names
- profiles
- personalities
- interests
- NPC simulation state

The population system must use the existing Account/NPC architecture rather than creating a second NPC system.

---

# 4. POPULATION SIZE

Do NOT treat 100 NPCs as the intended final population.

The simulator needs to support a substantially larger population.

The architecture should be designed so that generating:

```text
1,000 NPCs
10,000 NPCs
100,000 NPCs
```

is structurally possible without requiring a redesign.

For Part 09, however, use a practical test population rather than blindly generating an enormous database during every test.

Recommended default development population:

```text
1,000 NPCs
```

The implementation should support configurable population size.

---

# 5. ACCOUNT TYPE DISTRIBUTION

Do NOT make every NPC identical.

The population generator should support configurable account-type distribution.

The population should contain a meaningful mixture of:

```text
OrdinaryUser
Creator
Influencer
Celebrity
Official
News
```

The exact percentages should be configurable rather than hard-coded throughout the codebase.

The distribution should produce enough ordinary users to represent the majority of the population while maintaining enough creators, influencers, celebrities, official accounts, and news accounts to generate variety in the simulated platform.

Keep the distribution deterministic when a seed is supplied.

---

# 6. POPULATION GENERATOR ARCHITECTURE

Create a dedicated population-generation abstraction.

Use the existing layered architecture.

For example:

```text
Application
├── INpcPopulationService
└── NpcPopulationService
```

Do NOT blindly copy these names if the existing project conventions indicate a better equivalent.

The service should be responsible for population-level generation.

It should NOT duplicate the individual NPC creation logic already provided by:

```text
INpcService
NpcService
```

Instead:

```text
NpcPopulationService
        ↓
NpcService
        ↓
Account + Profile + NpcProfile
             + Personality
             + Interests
```

The population layer orchestrates large-scale creation.

---

# 7. CONFIGURATION

Create a clear configuration model for population generation.

It should support at minimum:

```text
PopulationSize
RandomSeed
AccountTypeDistribution
```

It should be possible to change population size without modifying business logic.

A supplied seed must produce reproducible generation.

Example concept:

```text
Seed 12345
Population 1000
```

should produce the same generated population structure when run against an equivalent empty database.

Do not use uncontrolled randomness everywhere.

---

# 8. USERNAME GENERATION

NPC usernames must be varied and deterministic when seeded.

Avoid generating usernames such as:

```text
NPC1
NPC2
NPC3
NPC4
```

as the primary strategy.

Generate usernames that resemble actual social-media accounts.

Examples of style:

```text
pixelwanderer
nightowl92
techwithalex
dailyfootball
citynews24
gamevault
mariatravels
officialcityupdates
```

The exact generated names should be algorithmically produced.

Requirements:

- unique
- deterministic with seed
- scalable
- efficient
- no database query for every candidate if avoidable
- collision handling must exist

Do not generate massive in-memory structures unnecessarily.

---

# 9. PROFILE GENERATION

Generated NPC accounts should receive appropriate profile information through the existing Profile/NPC architecture.

At minimum:

```text
Username
DisplayName
Bio
AvatarUrl
```

Profiles should have variation based on account type.

For example:

### OrdinaryUser

Personal/social descriptions.

### Creator

Content-oriented descriptions.

### Influencer

Lifestyle/media-oriented descriptions.

### Celebrity

Public-personality-oriented descriptions.

### Official

Institutional/public-service descriptions.

### News

News/media-oriented descriptions.

Do not create real-world identities or impersonate actual celebrities, organizations, or news companies.

Use fictional generated identities.

---

# 10. PERSONALITY GENERATION

Reuse the existing deterministic personality generation from Part 08.

Do NOT implement a second personality algorithm unless required.

Population generation should invoke the existing NPC personality generation system.

Personality should remain:

```text
0.0 - 1.0
```

and retain the existing Big Five structure.

The population should naturally contain personality variation.

---

# 11. INTEREST GENERATION

Reuse the existing interest-generation system.

Do NOT create duplicate interest logic.

The population generator should ensure every generated NPC receives valid interests according to the existing rules.

Interests should remain influenced by account type.

---

# 12. ACCOUNT-TYPE VARIETY

The population must not simply be:

```text
1000 OrdinaryUser
```

unless explicitly configured that way.

The generator should produce a varied ecosystem.

Example development distribution could be approximately:

```text
OrdinaryUser    70%
Creator         12%
Influencer       7%
News             5%
Official         4%
Celebrity        2%
```

These values are only a reasonable starting configuration.

Make them configurable.

Do not scatter percentages across the code.

---

# 13. DATABASE / PERFORMANCE

Population generation must be designed for scale.

Avoid:

```text
SaveChangesAsync()
SaveChangesAsync()
SaveChangesAsync()
```

for every individual NPC.

Use batching where appropriate.

The system should minimize:

- unnecessary database round trips
- excessive tracking overhead
- repeated queries
- unnecessary object allocation

Use the existing EF Core persistence architecture.

Do not bypass the established persistence layer without a strong reason.

The generator should work correctly with SQLite.

---

# 14. TRANSACTION / FAILURE HANDLING

Population generation should not leave the database in an obviously corrupted or half-generated state.

Use the existing Unit of Work / transaction infrastructure where appropriate.

Consider what should happen if:

- generation fails midway
- a username collision occurs
- invalid configuration is supplied
- population size is zero
- a duplicate generation is attempted

Do not silently swallow errors.

Return meaningful results or throw appropriate exceptions according to existing project conventions.

---

# 15. DUPLICATE GENERATION

Think carefully about what happens if the population generator is run more than once.

Do NOT accidentally create duplicate populations unless that is explicitly intended.

The system should have a clear behavior for repeated generation.

Possible approaches include:

- preventing duplicate generation
- allowing multiple generated populations with explicit batch identifiers
- generating only until a target population is reached

Choose the approach that best fits the existing architecture.

Document the decision in the README.

Do not introduce unnecessary complexity if the current project does not require population batches yet.

---

# 16. NPC IDENTITY

Every generated NPC must retain stable identity.

Continue using:

```text
AccountId = GUID
NpcId = GUID
```

Do not derive persistent identity from array indexes.

This is important because later systems will reference NPCs through:

- follows
- posts
- comments
- likes
- relationships
- simulation state

---

# 17. FUTURE COMPATIBILITY

Part 09 should prepare the project for future systems without prematurely implementing them.

The population generated here will eventually be used by:

```text
NPC behavior
NPC following
NPC posting
NPC liking
NPC commenting
NPC browsing
NPC relationships
LLM content generation
feed interaction
emergent drama
```

Do NOT implement those systems in Part 09.

Only make sure the population architecture does not block them.

---

# 18. API / ADMIN ACCESS

Part 08 intentionally did not create public NPC APIs.

Do not expose NPC population generation to normal users.

If an administrative/internal mechanism is needed for testing, keep it clearly separated from the public social-media API.

Do not introduce insecure public endpoints merely to make testing easier.

Prefer service-level integration tests where practical.

---

# 19. TESTING REQUIREMENTS

Create comprehensive tests for Part 09.

At minimum test:

### Configuration

- default configuration is valid
- custom population size works
- invalid population size is rejected
- deterministic seed works
- account-type distribution is respected

### Generation

- generate 1 NPC
- generate 10 NPCs
- generate 100 NPCs
- generate 1,000 NPCs
- all generated accounts have NPC profiles
- all generated NPCs have personalities
- all generated NPCs have interests
- all usernames are unique
- all Account IDs are unique
- all NPC IDs are unique

### Account Types

Verify that configured account-type distribution is actually reflected in generated data.

### Persistence

- generated population survives restart
- generated population can be queried afterward
- existing accounts are not accidentally corrupted

### Failure Handling

Test appropriate behavior for:

- zero population
- negative population
- invalid distribution
- duplicate generation
- username collision

### Determinism

Given the same:

```text
seed
configuration
empty database
```

the generated population structure should be reproducible.

Do not require GUID values themselves to be identical if the existing architecture intentionally generates GUIDs independently; test deterministic generated attributes and distribution instead.

---

# 20. PERFORMANCE TEST

Part 09 must include a population-generation performance test.

Measure at least:

```text
1,000 NPC generation
```

Record:

```text
NPC count
elapsed time
successful creation
database persistence
```

Do not optimize blindly before measuring.

The goal is to establish a baseline for future scaling work.

Do NOT claim that 100,000 NPCs are supported unless the implementation has actually been tested at that scale.

---

# 21. README REQUIREMENT — MANDATORY

Update:

```text
D:\SMS\README.md
```

as part of Part 09.

This is REQUIRED.

The README must accurately reflect everything actually implemented.

Add/update:

```text
Part 09 — NPC Population Generation
```

Document:

- population generation architecture
- configuration
- population size
- account-type distribution
- deterministic seeding
- username generation
- profile generation
- persistence strategy
- duplicate-generation behavior
- performance results
- test results
- limitations
- future scalability considerations

Also update:

```text
Current Project Status
```

so Part 09 is marked:

```text
COMPLETE
```

ONLY after all required work and tests pass.

Do not claim features that were not actually implemented.

---

# 22. GIT REQUIREMENT

After implementation and verification:

```text
git status
git diff
git log
```

Inspect the changes.

Make sure there are no accidental files, generated artifacts, databases, build output, secrets, or unrelated modifications committed.

Then commit Part 09.

Use a clear commit message such as:

```text
Implement NPC population generation (Part 09)
```

Push to:

```text
origin/main
```

Confirm the push succeeded.

The working tree should be clean afterward.

---

# 23. FINAL VERIFICATION

Before declaring Part 09 complete:

```text
Server build → PASS
All existing tests → PASS
Part 09 tests → PASS
Database persistence → PASS
Population generation → PASS
README updated → YES
Git commit → SUCCESS
Git push → SUCCESS
Working tree → CLEAN
```

Do not mark Part 09 complete if critical tests fail.

Do not hide failures.

---

# 24. DO NOT IMPLEMENT YET

Do NOT implement:

```text
LLM/Ollama integration
Qwen NPC content generation
NPC posting behavior
NPC following behavior
NPC liking behavior
NPC commenting behavior
NPC browsing decisions
advanced NPC decision making
recommendation algorithm
trending algorithm
emergent drama system
NPC background hosted simulation loop
Android NPC UI
```

Those belong to later parts.

Part 09 is specifically:

```text
NPC POPULATION GENERATION
```

Stay within scope.

---

# 25. DEVELOPMENT RULES

Before changing code:

1. Inspect the existing implementation.
2. Understand existing conventions.
3. Reuse existing services and infrastructure.
4. Do not duplicate functionality.
5. Keep the layered architecture intact.
6. Keep business logic out of controllers.
7. Keep persistence logic in the persistence layer.
8. Use async APIs where appropriate.
9. Keep IDs stable.
10. Keep generation deterministic when seeded.
11. Optimize for scalability without premature overengineering.
12. Write tests for every important behavior.
13. Run the complete test suite.
14. Update README.md.
15. Review Git changes.
16. Commit and push.

Do not rewrite working Parts 01–08 simply for stylistic reasons.

---

# 26. COMPLETION REPORT

When finished, provide a clean session log containing:

```text
# PART 09 — COMPLETE

## 1. What Was Inspected

...

## 2. What Already Existed

...

## 3. What Changed

...

## 4. Population Architecture

...

## 5. Configuration

...

## 6. Account-Type Distribution

...

## 7. Generation

...

## 8. Database / Performance

...

## 9. Tests

...

## 10. README

Updated: YES

## 11. Git

Commit: <hash>
Push: SUCCESS
Working tree: CLEAN

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
09  COMPLETE

## 13. Intentionally Not Implemented

...

## 14. NEXT

NEXT: PART 10 — <exact next part from the master plan>
```

Do not invent the Part 10 title if the master development plan already defines it. Use the exact title from the master plan.

---

# 27. STOP CONDITION

Once Part 09 is fully implemented, tested, documented, committed, and pushed:

**STOP.**

Do not automatically begin Part 10.

Wait for the next instruction.

# START PART 09 NOW.