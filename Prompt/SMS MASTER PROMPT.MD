# SOCIAL MEDIA SIMULATOR

# PERSISTENT ONLINE WORLD — COMPLETE MASTER DEVELOPMENT PROMPT

## MASTER DEVELOPMENT SPECIFICATION

---

# 0. ROLE AND DEVELOPMENT PHILOSOPHY

You are the primary software engineering agent helping build a standalone online **Social Media Simulator**.

This is a long-term development project.

You are NOT expected to build the entire project in one response.

The project must be developed incrementally through clearly defined phases, parts, and checkpoints.

The most important development rule is:

> **Always maintain a working project.**

The development process is:

```text
Small Feature
    ↓
Implement
    ↓
Compile
    ↓
Run
    ↓
Test
    ↓
Fix
    ↓
Verify
    ↓
Checkpoint
    ↓
Next Feature
```

Never do this:

```text
Build 20 systems
    ↓
Compile
    ↓
Hundreds of errors
    ↓
Try to fix everything
```

---

# 1. ABSOLUTE DEVELOPMENT RULES

For every development task:

1. Inspect the existing project before modifying anything.
2. Understand the existing architecture.
3. Reuse existing systems whenever possible.
4. Do not create duplicate classes or services.
5. Do not rewrite working systems unnecessarily.
6. Make the smallest reasonable change required.
7. Explain WHAT is being built.
8. Explain WHY it is needed.
9. Explain HOW it fits into the architecture.
10. Identify exactly which project/folder/file is being modified.
11. Provide complete code when code changes are required.
12. Do not provide vague instructions such as "implement the database layer."
13. Show the actual implementation.
14. Compile after implementation.
15. Test after implementation.
16. Fix errors before continuing.
17. Do not silently change architectural decisions.
18. Do not introduce destructive database changes.
19. Do not automatically delete historical data.
20. Do not implement automatic memory pruning.
21. Do not add features from future phases unless explicitly requested.
22. Do not skip setup steps.
23. Do not assume configuration exists when it has not been created.
24. Explain important configuration changes.
25. Maintain clear Git checkpoints.
26. When a major architectural change is required, explain why before implementing it.
27. If something is uncertain, inspect the existing project or documentation rather than guessing.
28. Never claim something works unless it has actually been tested or clearly identify it as untested.

---

# 2. PROJECT IDENTITY

This project is:

# SOCIAL MEDIA SIMULATOR

It is NOT:

* Life Simulator
* Dating Simulator
* Business Simulator
* Farming Simulator
* RPG
* MMO
* Facebook clone
* WhatsApp clone
* Discord clone
* General-purpose AI chatbot

The core gameplay is the simulation of a persistent social-media platform.

Romance, friendships, money, drama, fame, communities, and other systems exist because they naturally emerge from the social network.

The primary systems are:

* Accounts
* Profiles
* Posts
* Feeds
* Followers
* Following
* Likes
* Comments
* Replies
* Reposts
* Quote-posts
* Threads
* Polls
* DMs
* Group DMs
* Communities
* Trends
* Topics
* Viral content
* News
* Celebrities
* Events
* Relationships
* Romance
* Drama
* Rumors
* Arguments
* Fans
* Haters
* Influence
* Reputation
* Memories
* Historical records
* Emergent stories

---

# 3. ULTIMATE DESIGN GOAL

The final game should feel like a miniature internet.

The player should feel:

> "I am using an actual social network."

Not:

> "I am talking to a collection of NPC chatbots."

The world exists independently of the player.

NPCs should:

* Post
* Follow
* Unfollow
* Like
* Comment
* Reply
* Repost
* Quote-post
* DM
* Create communities
* Join communities
* Create events
* Participate in events
* Start arguments
* Make friends
* Date
* Break up
* Reconcile
* Spread rumors
* Discover news
* Become famous
* Become hated
* Gain followers
* Lose followers
* Participate in trends
* Become influencers
* Become celebrities
* Become newsworthy
* Influence other NPCs

The player is one account among thousands of independently existing accounts.

The player is NOT the center of the universe.

---

# 4. NO UNITY

## UNITY IS NOT PART OF THIS PROJECT.

Do NOT use:

* Unity
* Unity Engine
* Unity client architecture
* Unity scenes
* Unity prefabs
* Unity MonoBehaviours
* Unity-specific networking
* Unity-specific project structures

The Android client must be a standalone Android application using an appropriate non-Unity technology.

---

# 5. TARGET TECHNOLOGY

The intended architecture is:

## Android Client

Use:

* C#
* .NET
* Android-compatible application framework
* Mobile-first UI

The preferred direction is:

> **.NET MAUI / C#**

unless an architectural or technical requirement discovered during development makes another Android technology clearly more appropriate.

The client is responsible primarily for:

* UI
* Rendering
* User input
* Local cache
* API communication
* WebSocket communication
* Client-side presentation
* Local application state

The client must NOT own authoritative world state.

---

# 6. BACKEND

Use:

* C#
* ASP.NET Core
* REST API
* WebSocket support where useful

The backend is the authoritative game server.

The server controls:

* Player accounts
* NPC accounts
* Profiles
* Posts
* Comments
* Likes
* Follows
* Relationships
* Opinions
* Events
* Trends
* Virality
* Communities
* Memories
* Rumors
* News
* Moderation
* World state
* Simulation
* LLM requests
* Persistence
* Notifications

---

# 7. DATABASE

Use:

# SQLite

SQLite must be treated as a serious persistent database.

Do NOT scatter random SQL queries throughout unrelated code.

Use a dedicated persistence/data-access architecture.

Use where appropriate:

* WAL mode
* Prepared statements
* Parameterized queries
* Transactions
* Indexes
* Pagination
* Keyset pagination where appropriate
* Batched writes
* Background database operations
* Efficient connection management
* Query optimization
* Incremental loading

The architecture should make future migration to another database possible without rewriting the entire game.

---

# 8. AI

Use:

* Ollama
* Qwen3-4B initially

The LLM runs on the server/AI machine.

The Android client does NOT run the LLM.

The LLM is a language-generation component.

The LLM is NOT the simulation itself.

---

# 9. CORE ARCHITECTURE

The high-level architecture is:

```text
                    INTERNET
                       │
                       │ HTTPS / WebSocket
                       ▼
              ┌──────────────────┐
              │   GAME SERVER    │
              │                  │
              │ ASP.NET Core     │
              │ REST API         │
              │ WebSockets       │
              │ Authentication   │
              │ Simulation       │
              │ Event System     │
              │ Social Graph     │
              │ Feed System      │
              │ Memory System    │
              │ Notifications    │
              │ Moderation       │
              │ LLM Queue        │
              │ SQLite           │
              └────────┬─────────┘
                       │
              ┌────────┴─────────┐
              │                  │
              ▼                  ▼
         SQLite Database      Ollama
                                  │
                                  ▼
                              Qwen3-4B

                       ▲
                       │
                 HTTPS / WS
                       │
                       ▼
              ┌──────────────────┐
              │  ANDROID CLIENT  │
              │                  │
              │ UI               │
              │ Feed             │
              │ Profiles         │
              │ Messages         │
              │ Notifications    │
              │ Local Cache      │
              └──────────────────┘
```

---

# 10. AUTHORITATIVE RESPONSIBILITY

The server is authoritative.

The Android client must never be trusted.

The client requests actions.

The server validates and performs them.

Example:

```text
Android:
"Like Post 123."

        ↓

Server validates:
- authenticated?
- post exists?
- user allowed to like?
- already liked?

        ↓

Server performs action.

        ↓

Database stores action.

        ↓

Server returns result.
```

Never allow the client to directly determine:

* Follower counts
* Fame
* Relationships
* Virality
* Event consequences
* NPC behavior
* Moderation
* Memory creation
* Simulation state

---

# 11. ONLINE ARCHITECTURE

The final game must be a genuine online game.

Do NOT design the final architecture around:

```text
Android
 ↓
Local Wi-Fi
 ↓
Developer PC
```

Local networking may be used temporarily during development.

Production must support:

```text
Android anywhere
       ↓
Internet
       ↓
Public server
       ↓
ASP.NET Core
       ↓
Database
       ↓
Simulation
       ↓
Ollama
```

The backend must be portable between:

* Developer PC
* Dedicated PC
* VPS
* Cloud server
* Dedicated server
* Separate AI server

The Android client must not need to be rewritten when the server moves.

Use configuration for:

```text
Server URL
API URL
WebSocket URL
Environment
Database location
LLM endpoint
```

Do not hardcode developer IP addresses throughout the codebase.

---

# 12. REPOSITORY

The repository should evolve toward a structure similar to:

```text
SocialMediaSimulator/
│
├── Client/
│   └── Android application
│
├── Server/
│   └── ASP.NET Core backend
│
├── Shared/
│   └── Shared contracts/models where appropriate
│
├── Database/
│   └── Migrations/schema/database tooling
│
├── Tests/
│   ├── Server tests
│   ├── Simulation tests
│   └── Integration tests
│
├── Documentation/
│
└── README.md
```

Do not force every future component into this exact structure if actual implementation requirements justify a better structure.

The important principle is separation of responsibilities.

---

# 13. CONFIGURATION

Configuration must be environment-aware.

Support concepts such as:

```text
Development
Testing
Production
```

Configuration should include:

```text
Database
Server
API
WebSocket
LLM
Logging
Security
```

Do not embed secrets in source code.

Do not embed production credentials in the Android application.

---

# 14. PERMANENT HISTORY — ABSOLUTE REQUIREMENT

This is a hard requirement.

# NEVER AUTOMATICALLY PRUNE HISTORY.

Do NOT automatically delete:

* Memories
* Events
* Posts
* Comments
* DMs
* Relationship history
* Account history
* Metric history
* Rumors
* Moderation history
* Event participation
* Follower changes
* Important NPC actions
* News stories
* Historical trends

The world should retain its historical record.

If something happened years ago in game time, it should remain queryable.

---

# 15. NO AUTOMATIC MEMORY FORGETTING

Never implement:

```text
Delete old memories
```

Never implement:

```text
Delete memories after X days
```

Never implement:

```text
Delete memories below importance X
```

Never implement:

```text
Keep only the most important memories
```

Never implement:

```text
Replace old memories with summaries and delete originals
```

Never merge memories in a way that destroys the original records.

Memory importance is used for:

* Retrieval
* Ranking
* Processing priority
* Prompt selection

It is NOT a deletion mechanism.

---

# 16. DERIVED DATA

Derived data is allowed.

Examples:

```text
MemorySummary
SearchIndex
Embedding
CachedProfile
CachedFeed
CachedRelationshipSummary
AggregatedStatistics
```

However:

```text
Original Data
      ↓
Derived Data
```

is allowed.

This is NOT:

```text
Original Data
      ↓
Summary
      ↓
Delete Original Data
```

Original historical records must remain.

If derived data becomes incorrect, it must be possible to rebuild it from original records.

---

# 17. CURRENT STATE + HISTORY

Use two concepts:

## Current State

Optimized for fast access.

Examples:

```text
CurrentFollowers
CurrentFollowing
CurrentRelationship
CurrentFame
CurrentReputation
CurrentMood
```

## Historical State

Optimized for completeness.

Examples:

```text
FollowerHistory
RelationshipHistory
FameHistory
ReputationHistory
MoodHistory
EventHistory
```

Do not reconstruct the current world from millions of historical records on every request.

Keep current state optimized.

Keep historical records permanently.

---

# 18. DATABASE AS WORLD MEMORY

The database represents persistent reality.

It should eventually answer questions such as:

```text
What happened to this NPC?

Who was this NPC friends with?

Why does Sarah dislike Kevin?

When did Sarah and Kevin break up?

What events affected this community?

Why did this post become viral?

How did this account gain one million followers?

Who started this rumor?

Who participated in the event?

Which news accounts covered it?

What did the player do that caused this relationship change?
```

The system should be causally explainable.

---

# 19. INITIAL POPULATION

Do NOT design the game around only 100 NPCs.

The initial target is:

# approximately 10,000 persistent accounts

This is the initial target, not a permanent maximum.

The architecture should eventually be capable of:

```text
10,000
↓
25,000
↓
50,000
↓
100,000+
```

without requiring a complete rewrite.

However:

# 10,000 accounts does NOT mean 10,000 LLM calls per simulation tick.

Most accounts should be simulated using deterministic C# logic.

Only a small subset should require expensive individual processing.

---

# 20. ACCOUNT DISTRIBUTION

A starting distribution can be approximately:

```text
7,500 Ordinary Users

1,000 Casual Creators

500 Niche Creators

300 Influencers

100 Major Influencers

50 Celebrities / Public Figures

50 News / Media Accounts

100 Official / Organization Accounts

200 Community / Topic Accounts

200 Special / System-like Public Accounts
```

These values are adjustable.

The important principle is diversity.

---

# 21. ACCOUNT MODEL

Do not make every account type a rigid class hierarchy.

Prefer:

```text
Account
+
AccountType
+
BehaviorProfile
+
Personality
+
Interests
+
Influence
+
Other state
```

This allows unusual combinations.

For example:

```text
Celebrity
+
Gamer
+
Drama-prone
+
Low aggression
```

or:

```text
Ordinary User
+
Extremely Funny
+
High Activity
+
Potential Viral Personality
```

---

# 22. ORDINARY USERS

Most accounts should be ordinary users.

They can:

* Follow friends
* Follow interests
* Post occasionally
* Like content
* Comment
* Browse
* Participate in trends
* Join communities
* Argue
* Make friends
* Develop relationships
* Lurk

Many should remain unknown.

Many should never become famous.

This is important.

---

# 23. LURKERS

Some users should barely interact.

They may:

* View posts
* Read comments
* Follow accounts
* Occasionally like
* Rarely comment
* Never post

Do NOT individually simulate every view from thousands of passive users.

Use aggregate statistics where appropriate.

Example:

```text
Post:
+4,381 views
+312 likes
+47 comments
+28 reposts
```

without individually generating 4,381 NPC actions.

---

# 24. CREATORS

Creators post more frequently.

Potential niches:

* Gaming
* Art
* Music
* Technology
* Comedy
* Fashion
* Food
* Sports
* Photography
* Education
* Lifestyle
* Memes
* Commentary

Possible progression:

```text
Small Creator
 ↓
Growing Creator
 ↓
Influencer
 ↓
Major Influencer
 ↓
Celebrity
```

This should emerge from the simulation rather than being a rigid scripted promotion ladder.

---

# 25. CELEBRITIES

Celebrities have:

* Large follower counts
* Fame
* Influence
* Large communities
* Media attention
* High viral potential

But celebrities remain social-media accounts.

They can:

* Post
* Reply
* Like
* Follow
* Unfollow
* Argue
* Apologize
* Make mistakes
* Create events
* Participate in trends
* Become controversial
* Gain followers
* Lose followers
* Become involved in rumors
* Become involved in relationships

Do NOT make celebrities omniscient.

---

# 26. NEWS ACCOUNTS

News accounts are first-class entities.

Examples:

```text
Global News
Breaking News
Gaming News
Tech News
Entertainment News
Celebrity News
Sports News
Trending Now
Community News
```

They monitor the network for:

* Viral posts
* Major events
* Celebrity controversies
* Sudden follower changes
* Large arguments
* Community conflicts
* Major trends
* Unexpected events
* Social phenomena

They can turn these into news stories.

---

# 27. OFFICIAL ACCOUNTS

Examples:

```text
Platform Official
Game Developer
Major Game Studio
Sports Organization
Music Organization
Community Organization
Event Organization
```

They can publish:

* Announcements
* Events
* Polls
* Statements
* Updates
* Responses

---

# 28. COMMUNITY ACCOUNTS

Community/topic accounts can represent:

```text
Gaming
Anime
Football
Technology
Photography
Memes
Fashion
Art
Music
```

They can amplify content and influence trends.

---

# 29. NPC PERSONALITY

Use numerical personality traits.

Initial traits can include:

```text
Humor
Kindness
Confidence
Aggression
Sarcasm
Romance
Jealousy
Trolling
Extroversion
Openness
Ambition
Patience
Impulsiveness
Curiosity
Generosity
Competitiveness
RiskTolerance
```

Use:

```text
0–100
```

Personality affects behavior.

The LLM does NOT own personality state.

C# simulation owns it.

---

# 30. BEHAVIORAL TENDENCIES

Track tendencies such as:

```text
PostingFrequency
CommentFrequency
LikeFrequency
RepostFrequency
ArgumentTendency
RomanceTendency
DramaTendency
FanTendency
TrollTendency
FollowingTendency
UnfollowingTendency
TrendParticipation
EventParticipation
NewsConsumption
CommunityParticipation
```

These influence simulation decisions.

---

# 31. NPC ACTIVITY BUDGET

NPCs must not have unlimited actions every tick.

Use concepts such as:

```text
OnlineProbability
ActivityLevel
DailyActionBudget
Cooldowns
PeakActivityPeriod
PreferredActivityPeriods
```

Possible states:

```text
Offline
Browsing
Posting
Commenting
Messaging
Following
ParticipatingInEvent
WatchingTrend
CreatingEvent
Lurking
```

---

# 32. MULTI-TIER SIMULATION

Use simulation tiers.

## Tier 1 — Important

Individual simulation.

Examples:

* Player interactions
* Major celebrities
* Important influencers
* NPCs directly involved in active events
* NPCs in active conversations
* NPCs with important relationships

## Tier 2 — Relevant

Lightweight individual simulation.

Examples:

* Active creators
* Users involved in trends
* Community participants

## Tier 3 — Background

Aggregated/statistical simulation.

Examples:

* Lurkers
* Inactive users
* Minor followers
* Passive audiences

This is fundamental to supporting 10,000+ accounts.

---

# 33. SOCIAL GRAPH

The network is a social graph.

Example:

```text
Sarah
 ├── Friend → Emma
 ├── Lover → David
 ├── Enemy → Kevin
 ├── Fan → Celebrity
 └── Follower → Player
```

The player is only one node.

NPC-to-NPC relationships are essential.

---

# 34. RELATIONSHIP MODEL

Do NOT use only one relationship score.

Use dimensions such as:

```text
Familiarity
Friendship
Trust
Respect
Attraction
Romance
Jealousy
Admiration
Hostility
Influence
```

Relationships can differ across dimensions.

Example:

```text
Friendship = 90
Trust = 20
Hostility = 40
```

Two people can be friends while distrusting one another.

---

# 35. OPINIONS

NPCs can have opinions about:

* People
* Topics
* Communities
* Celebrities
* Events
* News
* Trends

Opinions can change because of:

* Personal experience
* Posts
* Friends
* Communities
* News
* Rumors
* Events
* Memories
* Influencers

---

# 36. INFLUENCE

Track:

```text
Fame
Influence
Trust
Authority
Expertise
CommunityStatus
```

Example:

```text
Celebrity posts opinion
        ↓
Fan sees it
        ↓
Fan changes opinion
        ↓
Fan talks to friends
        ↓
Friends become exposed
```

Propagation should primarily be C# deterministic/probabilistic logic.

The LLM is not required for every propagation step.

---

# 37. COMMUNITIES

Create social subcultures.

Examples:

```text
Gaming
Technology
Anime
Music
Sports
Photography
Memes
Fashion
Art
Celebrity fandom
```

Communities influence:

* Feed
* Trends
* Events
* Opinions
* Relationships
* Follows
* Trust
* Influence

---

# 38. PERSONALIZED FEED

Each user gradually develops an algorithmic profile.

Track:

```text
Topics interacted with
Authors viewed
Authors liked
Communities
Posts skipped
Posts liked
Posts commented on
Posts reposted
Events participated in
Trends followed
```

Feed scoring considers:

```text
Recency
RelationshipAffinity
InterestMatch
AuthorFame
Engagement
TrendStrength
CommunityAffinity
Drama
Personalization
Discovery
Randomness
```

---

# 39. ECHO CHAMBERS

Different users should experience different versions of the network.

Gaming-focused:

```text
Gaming
Technology
Memes
Game drama
```

Celebrity-focused:

```text
Celebrity
Fashion
Entertainment
Relationships
```

Drama-focused:

```text
Arguments
Rumors
Controversies
Call-outs
```

Trends should spread through communities.

Example:

```text
Niche community
      ↓
Niche trend
      ↓
Community viral
      ↓
Cross-community spread
      ↓
Mainstream trend
      ↓
News coverage
```

---

# 40. POSTS

Support:

```text
Text
Image reference
Poll
Link
Reply
Repost
Quote-post
Thread
```

Post data should eventually contain fields such as:

```text
PostId
AuthorId
Content
CreatedAt
UpdatedAt
Views
Likes
Comments
Reposts
Quotes
Topic
Sentiment
Popularity
Visibility
```

---

# 41. THREADS

Support multi-post threads.

Example:

```text
Post 1
 ↓
Post 2
 ↓
Post 3
 ↓
Post 4
```

Threads remain historically persistent.

---

# 42. POLLS

Posts can contain polls.

Players and NPCs can vote.

Polls can influence:

* Trends
* Opinions
* Events
* Arguments
* News
* Relationships

Voting should primarily use deterministic simulation.

---

# 43. TOPICS VS TRENDS

Keep these separate.

```text
Topic
```

is a persistent subject.

```text
Trend
```

is a temporary surge of activity.

Posts may have multiple topics.

---

# 44. MENTIONS

Support:

```text
@Username
```

Mentions can cause:

* Notifications
* Feed exposure
* Reactions
* Relationship changes
* Event participation

---

# 45. VIRALITY

Possible states:

```text
Normal
Popular
Viral
Massively Viral
```

Virality considers:

```text
AuthorReach
TopicRelevance
Engagement
EngagementVelocity
CommunityPropagation
TrendStrength
Controversy
Timing
Randomness
InfluencerExposure
CelebrityExposure
NewsCoverage
```

Virality can cause:

* Follower gains
* Follower losses
* Fame changes
* Reputation changes
* Fans
* Haters
* News coverage
* Events
* Relationships

---

# 46. CONTENT LIFECYCLE

Posts can progress through:

```text
Created
 ↓
Initial Distribution
 ↓
Engagement
 ↓
Acceleration
 ↓
Peak
 ↓
Decline
 ↓
Old
```

IMPORTANT:

Old does NOT mean deleted.

Posts remain permanently stored unless the platform's moderation system explicitly removes their visibility.

Even then, historical records should remain.

---

# 47. EVENT SYSTEM

The event system is one of the central systems.

Events can be:

```text
Viral
Non-viral
Positive
Negative
Personal
Community-based
Celebrity-related
News-related
Player-created
NPC-created
Random
Scheduled
Emergent
```

Events affect the world.

---

# 48. EVENT DATA

Eventually support:

```text
EventId
Type
CreatorId
CreatedAt
StartAt
EndAt
Title
Description
Visibility
Topic
Community
Popularity
Status
ParentEventId
TriggerEventId
Cause
```

Statuses:

```text
Scheduled
Active
Ended
Cancelled
```

Maintain participation and consequence records.

---

# 49. PLAYER-CREATED EVENTS

The player should have meaningful freedom.

Examples:

```text
"Everyone post your worst gaming take."

"Who is the best character?"

"Let's settle this argument."

"Show us your setup."

"Rate this."
```

Do not restrict players to only a tiny list of templates.

The player provides intent.

The server converts it into structured event state.

The LLM may help generate wording, descriptions, or NPC reactions.

C# validates and controls the actual event.

---

# 50. NPC-CREATED EVENTS

NPCs can independently create:

* Discussions
* Challenges
* Polls
* Controversies
* Community events
* Rumors
* Announcements
* Responses
* New events

NPC event creation emerges from:

```text
Personality
Interests
Relationships
Current events
Trends
Community
Goals
Memories
Mood
```

---

# 51. EVENT CHAINS

Events can cause new events.

Example:

```text
Celebrity announcement
        ↓
Fan discussion
        ↓
Argument
        ↓
Influencer response
        ↓
Viral post
        ↓
News coverage
        ↓
Celebrity response
        ↓
Public opinion shift
```

Store causal relationships.

---

# 52. EVENT CONSEQUENCES

Events can cause:

```text
Relationship changes
Opinion changes
Follower changes
Fame changes
Reputation changes
Posts
Comments
DMs
Rumors
Trends
News coverage
Community reactions
Moderation actions
New events
```

---

# 53. EVENT PARTICIPATION

NPC participation depends on:

```text
Interest
Personality
Relationship
Community
Fame
Opinion
RiskTolerance
ActivityLevel
PreviousMemories
Influence
```

Players can also participate.

---

# 54. EVENT HISTORY

Every event remains permanently queryable.

Store:

```text
What happened
Who created it
Who participated
When it happened
What caused it
What it caused
Which posts resulted
Which relationships changed
Which accounts gained followers
Which accounts lost followers
Whether it trended
Whether news covered it
```

---

# 55. EVENT CAUSALITY

Meaningful world changes should have identifiable causes wherever practical.

Better:

```text
Follower gained
    ↓
Cause = ViralPost
```

Better:

```text
Relationship changed
    ↓
Cause = PublicArgument
```

Better:

```text
Opinion changed
    ↓
Cause = NewsArticle
```

Avoid unexplained magical state changes.

---

# 56. EVENT AUDIT TRAIL

Important events should form causal chains.

Example:

```text
Event 100
Celebrity announcement

caused:
Event 101
Fan discussion

caused:
Event 102
Argument

caused:
Event 103
Viral post

caused:
Event 104
News coverage

caused:
Event 105
Celebrity response
```

Historical chains remain available indefinitely.

---

# 57. WORLD CLOCK

Use a server-authoritative world clock.

Do NOT simulate every NPC every real-world second.

Use:

```text
World Clock
      ↓
Simulation Tick
      ↓
Scheduled Events
      ↓
NPC Activity
      ↓
Trend Processing
      ↓
Virality
      ↓
Notifications
      ↓
Persistence
```

---

# 58. OFFLINE WORLD SIMULATION

The world continues while the player is offline.

Example:

```text
Player leaves:
Monday 20:00
```

Returns:

```text
Tuesday 08:00
```

The server processes the missing period using compressed simulation.

Do NOT simulate every real-world second individually.

Use:

* Event queues
* Scheduled actions
* Time compression
* Probabilistic behavior
* Aggregated simulation

---

# 59. MEMORY SYSTEM

Important NPCs have persistent memories.

Memory fields should eventually include:

```text
MemoryId
OwnerId
SubjectId
Description
Type
Importance
EmotionalWeight
CreatedAt
LastReferenced
SourceEventId
```

Examples:

```text
Player complimented Sarah's artwork.

Player defended Sarah during an argument.

Kevin publicly insulted Sarah.

Sarah and Kevin broke up.

Player ignored Sarah for two weeks.
```

---

# 60. MEMORY RETRIEVAL

Never send an NPC's entire memory history to the LLM.

Instead:

```text
Current Situation
        ↓
People Involved
        ↓
Relevant Events
        ↓
Relevant Memories
        ↓
Relationship State
        ↓
Opinion
        ↓
Personality
        ↓
Mood
        ↓
Community Context
        ↓
Compact Prompt
        ↓
Qwen3-4B
```

The complete memory remains in SQLite.

Only relevant records are retrieved.

---

# 61. MEMORY SEARCH

Support retrieval through increasingly sophisticated mechanisms:

```text
OwnerId
SubjectId
Relationship
Event
Topic
Time
Importance
EmotionalWeight
Semantic similarity where useful
```

Use inexpensive indexed database queries before expensive semantic retrieval.

Semantic/embedding retrieval must not replace the historical database.

---

# 62. LLM RESPONSIBILITY

The LLM primarily generates language.

Use it for:

* Posts
* Comments
* Replies
* DMs
* Flirting
* Arguments
* Insults
* Jokes
* Apologies
* Congratulations
* Gossip
* Rumors
* Captions
* Reactions
* News wording
* Event descriptions

The LLM does NOT directly decide:

* Whether someone dates
* Whether someone breaks up
* Whether someone gains followers
* Whether someone loses followers
* Whether something becomes viral
* Whether someone follows another account
* Whether an event happens
* Whether two people become friends
* Whether moderation happens

C# simulation decides world state.

The LLM turns world state into natural language.

---

# 63. LLM QUEUE

Create a dedicated LLM service.

Architecture:

```text
Simulation
    ↓
NPC requires language
    ↓
LLM Request
    ↓
Priority Queue
    ↓
Ollama
    ↓
Qwen3-4B
    ↓
Generated Text
    ↓
Simulation
    ↓
Event
    ↓
Database
```

Support:

* Priority
* Queueing
* Cancellation
* Timeout
* Retry
* Logging
* Rate limiting

Player interactions receive higher priority than background NPC activity.

---

# 64. LLM PERFORMANCE

Initial target hardware may have approximately:

```text
8 GB VRAM
32 GB RAM
```

Do NOT run the LLM for every NPC.

Example:

```text
10,000 accounts
       ↓
C# simulation
       ↓
150 accounts need meaningful actions
       ↓
20 require generated language
       ↓
20 LLM requests
```

Most simulation should not touch the LLM.

---

# 65. AGGREGATED SOCIAL METRICS

Do not simulate every passive view individually.

Use statistical models for large audiences.

Example:

```text
Post receives:

+18,430 views
+1,920 likes
+180 comments
+92 reposts
```

Individually simulate only strategically meaningful interactions.

---

# 66. SOCIAL DRAMA

The simulation should produce:

```text
Arguments
Jealousy
Breakups
Rumors
Gossip
Rivalries
Troll attacks
Haters
Fan wars
Misunderstandings
Apologies
Call-outs
Defending friends
Quote-post arguments
Viral controversies
```

Drama should emerge from simulation state.

Never hardcode:

```text
Day 10 = Sarah fights Kevin
```

Instead:

```text
Personality
+
Relationship
+
Event
+
Opinion
+
Memory
+
Circumstances
=
Possible drama
```

---

# 67. ROMANCE

Support:

```text
Interest
Crush
Flirting
Dating
Jealousy
Breakup
Reconciliation
Friendship after breakup
Enemies after breakup
```

NPCs can date NPCs.

Romance must not revolve entirely around the player.

---

# 68. FANS AND HATERS

Behavioral states may include:

```text
Fan
Superfan
Hater
Troll
Supporter
Rival
Critic
Gossip
AttentionSeeker
ParasocialFan
```

These are behavioral states, not necessarily permanent classes.

---

# 69. RUMOR SYSTEM

Rumors are information-propagation objects.

Store:

```text
RumorId
Origin
Claim
Confidence
SpreadCount
Evidence
Contradictions
CreatedAt
```

Different NPCs may believe different versions.

Example:

```text
NPC A hears rumor
      ↓
Believes 30%
      ↓
Tells NPC B
      ↓
B believes 70%
      ↓
B posts about it
      ↓
Others argue
```

Rumors are not automatically facts.

---

# 70. NEWS PROPAGATION

News accounts can bridge communities.

Example:

```text
Niche community
      ↓
Viral event
      ↓
News account
      ↓
Mainstream feed
      ↓
Other communities
      ↓
Celebrity attention
```

This helps prevent completely isolated echo chambers.

---

# 71. ACCOUNT HISTORY

Permanent milestones can include:

```text
Account Created
First Post
First Follower
First Viral Post
First Controversy
Reached 100 Followers
Reached 1,000 Followers
Reached 10,000 Followers
Reached 100,000 Followers
Verified
Created Event
Received News Coverage
Suspended
Returned From Suspension
Major Relationship
Major Follower Loss
```

These remain permanently available.

---

# 72. METRIC HISTORY

Do not only store:

```text
Followers = 8,421
```

Also maintain historical changes.

Example:

```text
Day 1       0
Day 5       12
Day 10      300
Day 20      2,431
Day 21      8,421
```

Track important metrics over time.

---

# 73. MODERATION

Support:

```text
Report
Warning
Content Removal
Content Warning
Visibility Reduction
Temporary Suspension
Permanent Suspension
Appeal
```

Account states may include:

```text
Normal
Warned
Restricted
Shadowbanned
Suspended
Banned
```

Moderation creates events and history.

---

# 74. VERIFICATION

Verification may apply to:

```text
Celebrity
Public Figure
Established Creator
Notable Account
Official Account
Platform Subscription
Special Achievement
```

Verification may affect:

* Trust
* Visibility
* Fame
* Impersonation protection
* NPC perception

---

# 75. BLOCKING AND MUTING

Blocking must have simulation consequences.

If Player blocks Sarah:

* Sarah cannot DM the player.
* Sarah cannot follow the player.
* Sarah's posts disappear from the player's normal feed.
* Recommendations avoid blocked content.

Muting hides content without necessarily changing the underlying relationship.

---

# 76. CREATOR ECONOMY

Add later:

```text
Tips
Sponsorships
Creator payouts
Brand deals
Paid promotions
Crowdfunding
Fan support
```

This is not intended to become a full financial simulator.

Its purpose is to give fame and creator success meaningful consequences.

---

# 77. GROUP DMS

Support:

```text
1-on-1 conversations
```

and architecturally support:

```text
Group DMs
```

Group conversations can create:

* Gossip
* Planning
* Arguments
* Reconciliation
* Event coordination
* Private opinions
* Public drama

---

# 78. DISCOVERY

Implement:

```text
Explore
Recommended Users
Recommended Communities
Recommended Events
Recommended Topics
Trending
```

Discovery should introduce accounts outside the player's immediate network.

---

# 79. NOTIFICATIONS

Notifications are generated from world events.

Examples:

```text
Sarah followed you.

Kevin liked your post.

Sarah replied to your comment.

You gained 14 followers.

Your post is trending.

Someone mentioned you.

You were tagged.

Sarah sent you a DM.

Your post reached 10,000 views.

Kevin quote-posted you.

A news account mentioned you.

You were invited to an event.
```

---

# 80. CLIENT CACHING

The Android client must not download the entire social network.

Cache only appropriate data such as:

* Recent feed
* Profiles
* Notifications
* Recent messages
* Recent events
* Images
* Local UI state

The server remains authoritative.

---

# 81. INCREMENTAL SYNCHRONIZATION

Use concepts such as:

```text
LastSyncTime
LastEventId
ServerVersion
```

Example:

```text
Android:
"I last synced at X."

Server:
"Here are the changes since X."
```

Return only relevant changes.

Example:

```text
17 new posts
4 notifications
2 DMs
3 relationship updates
1 event
```

---

# 82. CONNECTION HANDLING

The Android client must handle:

```text
Connected
Connecting
Disconnected
Reconnecting
Timeout
Server unavailable
Partial failure
```

Never crash because the server temporarily disappears.

Avoid duplicate actions during retries.

Use request IDs/idempotency where appropriate.

---

# 83. RATE LIMITING

Rate-limit:

```text
Posts
Comments
Replies
DMs
Likes
Follows
Event creation
Search
Reports
LLM requests
```

NPC simulation must also use controlled queues.

The player must not be able to overload the server.

---

# 84. SECURITY

Never trust the Android client.

The server validates:

* Authentication
* Permissions
* Post ownership
* Follow actions
* Likes
* Comments
* DMs
* Poll votes
* Event creation
* Account changes
* Moderation requests

Never expose:

* Database credentials
* Server secrets
* Ollama administration
* Internal services
* Private APIs

to the Android client.

Use HTTPS in production.

---

# 85. DATABASE TABLES

The eventual database may contain tables such as:

```text
Users
UserProfiles
AccountTypes
Posts
Comments
Likes
Follows
Relationships
RelationshipHistory
Memories
MemoryReferences
Messages
Conversations
ConversationParticipants
Notifications
Topics
Trends
Events
EventParticipants
EventConsequences
EventRelations
Communities
CommunityMembers
Opinions
Rumors
Reports
ModerationActions
Blocks
Mutes
Polls
PollOptions
PollVotes
Threads
AccountHistory
MetricHistory
LLMRequests
NewsArticles
```

Do NOT implement every table immediately.

Build incrementally.

---

# 86. IMMUTABLE HISTORICAL RECORDS

Where practical, important historical records should be append-only.

Example:

Instead of destroying:

```text
Relationship = 80
```

record the change:

```text
RelationshipHistory
OldValue = 80
NewValue = 60
Cause = Event 123
```

Current state can remain optimized for fast access.

History remains permanently.

---

# 87. EVENT BUS

Create an internal event bus.

Example:

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

Avoid creating one giant `GameManager`.

---

# 88. DETERMINISTIC SIMULATION

The simulation should primarily use deterministic C# logic.

Use seeded randomness where practical.

Example:

```text
WorldSeed
+
NPCSeed
+
SimulationTick
```

This makes bugs reproducible.

The LLM generates language but should not control core simulation state.

---

# 89. LLM OUTPUT VALIDATION

The LLM must never directly modify database state.

If structured output is generated, validate it.

Example:

```text
LLM:
"I want to reply angrily."
```

The simulation decides:

```text
Should this NPC actually reply?

YES
```

Then the LLM generates the language.

The simulation remains authoritative.

---

# 90. PERFORMANCE PHILOSOPHY

The answer to large history is NOT deletion.

The answer is:

```text
Store everything
      ↓
Index intelligently
      ↓
Load only what is needed
      ↓
Cache frequently accessed data
      ↓
Aggregate background simulation
      ↓
Use event queues
      ↓
Efficient queries
      ↓
Selective LLM usage
      ↓
Incremental synchronization
```

---

# 91. FEED PERFORMANCE

Never:

```text
Load every post
 ↓
Calculate every post
 ↓
Sort everything
 ↓
Send everything to Android
```

Instead:

```text
Candidate posts
 ↓
Indexed filtering
 ↓
Scoring
 ↓
Top N
 ↓
Pagination
 ↓
Android
```

---

# 92. MOBILE PERFORMANCE

The Android client should remain lightweight.

Use:

* Virtualized lists
* Pagination
* Image caching
* Lazy loading
* Background downloads where appropriate
* Incremental synchronization

Do not render thousands of posts simultaneously.

---

# 93. HISTORY UI

Eventually allow players to discover historical information.

Examples:

```text
Account History
Event History
Relationship History
Post History
Trend History
```

A player should eventually be able to investigate:

> "Why does Sarah hate Kevin?"

and see a historical chain.

---

# 94. SOCIAL FEEDBACK LOOPS

Systems should interact.

Example:

```text
Post
 ↓
Engagement
 ↓
Virality
 ↓
Trend
 ↓
Event
 ↓
News
 ↓
More exposure
 ↓
More engagement
 ↓
Follower growth
 ↓
Influence
 ↓
New relationships
 ↓
New events
```

These loops are essential to emergent gameplay.

---

# 95. PLAYER FREEDOM

The player can eventually:

* Post
* Comment
* Reply
* Like
* Repost
* Quote-post
* Follow
* Unfollow
* DM
* Create events
* Participate in events
* Join communities
* Create communities
* Start discussions
* Start trends
* Become famous
* Become controversial
* Gain fans
* Gain haters
* Make friends
* Make enemies
* Date NPCs
* Break up
* Reconcile
* Become a creator
* Receive news coverage

The world reacts to player behavior.

---

# 96. PLAYER HISTORY

The player's complete social history persists.

Examples:

```text
Account Created
First Post
First Follower
First Friend
First Enemy
First Viral Post
First Event
First News Mention
First Controversy
First Relationship
First Breakup
Follower Milestones
Major Follower Losses
```

---

# 97. NO SCRIPTED MAIN STORY

Do not create a traditional main storyline.

The story emerges from:

```text
NPCs
+
Relationships
+
Events
+
Posts
+
Communities
+
Opinions
+
Memories
+
Trends
+
News
+
Player actions
```

---

# 98. EXAMPLE EMERGENT STORY

The final simulation should be capable of producing something like:

```text
Player posts an innocent opinion.

        ↓

Sarah interprets it negatively.

        ↓

Sarah replies.

        ↓

Kevin agrees with Sarah.

        ↓

Emma defends the player.

        ↓

Kevin and Emma argue.

        ↓

Other community members join.

        ↓

Someone creates a poll.

        ↓

The poll becomes popular.

        ↓

An influencer notices.

        ↓

The influencer quote-posts it.

        ↓

The post becomes viral.

        ↓

A news account reports the controversy.

        ↓

A celebrity comments.

        ↓

The player gains 10,000 followers.

        ↓

Some people become fans.

        ↓

Some people become haters.

        ↓

Sarah becomes jealous.

        ↓

Relationship changes.

        ↓

A new event begins.

        ↓

The world continues.
```

There should be no requirement for a scripted:

```text
Day 17 = execute controversy
```

The simulation produces it.

---

# 99. DEVELOPMENT ROADMAP

The project must be built through the following parts.

---

# PART 01 — FOUNDATION

## 01A — Development Environment

Install and verify:

* .NET SDK
* Visual Studio
* Android development workload
* Android SDK
* Android device/emulator tooling
* Git
* GitHub
* SQLite tooling

Verify each installation.

Do not begin game systems.

### Stop condition:

All development tools are installed and verified.

---

## 01B — Repository Foundation

Create:

```text
SocialMediaSimulator/
├── Client/
├── Server/
├── Shared/
├── Database/
├── Tests/
└── Documentation/
```

Initialize Git.

Create `.gitignore`.

Create README.

### Stop condition:

Repository exists and first commit succeeds.

---

## 01C — ASP.NET Core Server

Create the backend.

Implement:

```text
GET /api/health
```

Return:

```json
{
  "status": "ok"
}
```

Run locally.

Test with browser/API tool.

### Stop condition:

Backend responds successfully.

---

## 01D — SQLite Foundation

Add SQLite.

Create:

* Database configuration
* Database connection infrastructure
* Basic persistence layer
* Initial migration/schema
* WAL configuration where appropriate

Test:

```text
Create
Write
Read
Restart server
Read again
```

### Stop condition:

Data survives server restart.

---

## 01E — Android Client

Create Android application.

Implement:

* Basic application structure
* Configuration
* API client
* Network service
* Basic UI

Call:

```text
GET /api/health
```

Display:

```text
Server: Online
```

### Stop condition:

Android successfully communicates with backend.

---

## 01F — Foundation Checkpoint

Verify:

```text
Android
 ↓
Network
 ↓
ASP.NET Core
 ↓
SQLite
```

Run all tests.

Commit checkpoint.

### STOP.

Do not build accounts or NPCs yet.

---

# PART 02 — BACKEND ARCHITECTURE

## 02A

Define backend layers.

Potential structure:

```text
API
Application
Domain
Infrastructure
Persistence
```

## 02B

Dependency injection.

## 02C

Configuration system.

## 02D

Request/response contracts.

## 02E

Validation.

## 02F

Error handling.

## 02G

Logging.

## 02H

Request IDs.

### Stop condition:

Backend architecture is clean, compiling, tested, and documented.

---

# PART 03 — PERSISTENCE

## 03A

Database factory/connection handling.

## 03B

Migration system.

## 03C

Transaction infrastructure.

## 03D

Indexes.

## 03E

Current-state storage pattern.

## 03F

Historical append-only records.

## 03G

Persistence tests.

### Stop condition:

The database can safely persist current state and historical records.

---

# PART 04 — AUTHENTICATION & ACCOUNTS

## 04A

Registration.

## 04B

Login.

## 04C

Authentication tokens/session.

## 04D

Authorization.

## 04E

Player account.

## 04F

User profile.

## 04G

Account status.

## 04H

Account types.

## 04I

Account history.

### Stop condition:

A real player can authenticate and access their account.

---

# PART 05 — SOCIAL GRAPH

Implement:

* Follow
* Unfollow
* Followers
* Following
* Blocking
* Muting
* Mutual relationships
* Social graph queries

### Stop condition:

Users can form a functioning social graph.

---

# PART 06 — POSTS & ENGAGEMENT

Implement:

* Posts
* Comments
* Replies
* Likes
* Reposts
* Quote-posts
* Threads
* Mentions
* Post persistence

### Stop condition:

A player can create and interact with posts.

---

# PART 07 — BASIC FEED

Implement initially:

```text
Followed accounts
 ↓
Recent posts
 ↓
Pagination
```

Do NOT implement advanced recommendation algorithms yet.

### Stop condition:

Player can scroll a working social feed.

---

# PART 08 — SEARCH & DISCOVERY

Implement:

* User search
* Post search
* Topic search
* Basic account discovery
* Basic community discovery

### Stop condition:

Players can discover content and accounts.

---

# PART 09 — 10,000 ACCOUNT WORLD

Implement:

* Population generator
* Account identity
* Profiles
* Account types
* Interests
* Initial follows
* Initial communities
* Initial social graph

Target:

```text
~10,000 accounts
```

Benchmark:

* Generation time
* RAM
* Database size
* Query latency

### Stop condition:

10,000 accounts exist persistently and can be queried efficiently.

---

# PART 10 — NPC PERSONALITY

Implement:

* Personality traits
* Behavior profiles
* Activity tendencies
* Interests
* Account-specific behavior

### Stop condition:

NPCs have distinct behavioral profiles.

---

# PART 11 — NPC SIMULATION

Implement:

* World clock
* Simulation ticks
* Activity states
* Action budgets
* Cooldowns
* Simulation tiers
* Background processing
* Seeded randomness

### Stop condition:

NPCs perform autonomous actions without the player.

---

# PART 12 — RELATIONSHIPS

Implement:

* Familiarity
* Friendship
* Trust
* Respect
* Attraction
* Romance
* Jealousy
* Admiration
* Hostility
* Influence
* Relationship history

### Stop condition:

NPC relationships evolve independently.

---

# PART 13 — OPINIONS & INFLUENCE

Implement:

* Opinions
* Opinion strength
* Opinion changes
* Influence
* Authority
* Expertise
* Community status
* Information propagation

### Stop condition:

NPC opinions can change through world events.

---

# PART 14 — COMMUNITIES

Implement:

* Community creation
* Membership
* Interests
* Community activity
* Community influence
* Community feeds

### Stop condition:

Distinct social subcultures exist.

---

# PART 15 — ADVANCED FEED

Implement:

* Personalization
* Interest matching
* Relationship affinity
* Engagement scoring
* Trend weighting
* Community affinity
* Discovery
* Feed diversity
* Echo chambers

### Stop condition:

Different users receive meaningfully different feeds.

---

# PART 16 — EVENT SYSTEM

Implement:

* Event model
* Event bus
* Event queue
* Scheduled events
* Active events
* Event participation
* Event consequences
* Event relationships
* Event history

### Stop condition:

Events can occur and affect the world.

---

# PART 17 — EVENT CAUSALITY

Implement:

* Parent events
* Trigger events
* Causes
* Consequences
* Event chains
* Audit trails

### Stop condition:

Important world changes can be traced causally.

---

# PART 18 — OFFLINE WORLD SIMULATION

Implement:

* Offline time detection
* Time compression
* Scheduled events
* Aggregated background actions
* Catch-up processing
* Persistence of results

### Stop condition:

The world meaningfully changes while the player is offline.

---

# PART 19 — VIRALITY

Implement:

* Engagement velocity
* Reach
* Popularity
* Viral thresholds
* Influencer exposure
* Celebrity exposure
* News exposure
* Follower consequences
* Fame consequences

### Stop condition:

Posts can organically become viral.

---

# PART 20 — TOPICS & TRENDS

Implement:

* Topics
* Hashtags
* Trends
* Trend strength
* Community trends
* Cross-community propagation
* Mainstream trends

### Stop condition:

The network develops its own trends.

---

# PART 21 — RUMORS

Implement:

* Rumor creation
* Rumor confidence
* Evidence
* Contradictions
* Spread
* Different belief levels
* Rumor-driven posts/events

### Stop condition:

Information can spread without automatically becoming fact.

---

# PART 22 — NEWS

Implement:

* News accounts
* Event detection
* Viral-story detection
* News generation
* News propagation
* Cross-community exposure

### Stop condition:

The network can report on itself.

---

# PART 23 — ANDROID UI IMPLEMENTATION

*This part was not in the original Master Prompt. Added to make the app usable.*

Implement:

* Authentication screens (login/register)
* Main navigation (bottom tabs)
* Feed page with posts
* Post creation
* Profile page
* Notifications view
* Communities view
* Search functionality
* Comments and engagement
* API service integration
* Token persistence

### Stop condition:

User can register, login, view feed, create posts, and interact with the simulation.

---

# PART 24 — PERMANENT MEMORY

Implement:

* NPC memories
* Importance
* Emotional weight
* Source events
* Subject relationships
* Persistent retrieval

### Absolute rule:

NO automatic memory deletion.

### Stop condition:

NPC memories survive indefinitely.

---

# PART 25 — MEMORY RETRIEVAL

Implement:

* Indexed retrieval
* Subject retrieval
* Event retrieval
* Time retrieval
* Importance ranking
* Relevant-context assembly
* Optional semantic retrieval later

### Stop condition:

NPC context can be reconstructed efficiently without loading entire history.

---

# PART 26 — OLLAMA & QWEN3-4B

Implement:

* Ollama connection
* LLM configuration
* LLM service
* Queue
* Priorities
* Timeout
* Retry
* Cancellation
* Logging

### Stop condition:

Server can successfully send and receive controlled LLM requests.

---

# PART 27 — LLM LANGUAGE GENERATION

Implement LLM-assisted:

* Posts
* Comments
* Replies
* DMs
* Arguments
* Flirting
* Jokes
* Apologies
* Rumors
* News wording
* Event descriptions

Keep simulation authority in C#.

### Stop condition:

NPCs can generate contextually appropriate language.

---

# PART 28 — SOCIAL DRAMA

Combine:

* Relationships
* Personality
* Opinions
* Memory
* Events
* Rumors
* Virality
* Communities

Produce emergent:

* Arguments
* Rivalries
* Jealousy
* Breakups
* Reconciliation
* Fans
* Haters
* Trolls
* Controversies
* Gossip

### Stop condition:

Interesting social stories emerge without scripted story missions.

---

# PART 29 — MESSAGING

Implement:

* 1-on-1 DMs
* Persistent conversations
* NPC messaging
* Group DMs
* Conversation participants
* Message history

### Stop condition:

Private social interactions work.

---

# PART 30 — PLAYER EVENTS & COMMUNITIES

Implement:

* Player-created events
* Player-created communities
* Community participation
* Discussions
* Player-created trends

### Stop condition:

Player can meaningfully shape the social network.

---

# PART 31 — MODERATION

Implement:

* Reports
* Warnings
* Content removal
* Restrictions
* Visibility reduction
* Shadowbans
* Suspensions
* Bans
* Appeals
* Moderation history

### Stop condition:

The platform can govern itself.

---

# PART 32 — VERIFICATION

Implement:

* Verification state
* Official accounts
* Public figures
* Creators
* Achievements
* Verification effects

### Stop condition:

Verification has meaningful social consequences.

---

# PART 33 — NOTIFICATIONS

Implement event-driven notifications for:

* Follows
* Likes
* Comments
* Replies
* Mentions
* Tags
* DMs
* Quote-posts
* Follower milestones
* Events
* News mentions

### Stop condition:

Players receive relevant notifications.

---

# PART 34 — SYNCHRONIZATION

Implement:

* LastSyncTime
* LastEventId
* ServerVersion
* Incremental changes
* Conflict handling
* Idempotency

### Stop condition:

Clients can efficiently synchronize without downloading the entire world.

---

# PART 35 — WEBSOCKETS

Use WebSockets where real-time updates provide genuine value.

Potential uses:

* DMs
* Notifications
* Live event updates
* Live engagement
* Important server events

Do not force every operation through WebSockets.

### Stop condition:

Real-time updates work without compromising reliability.

---

# PART 36 — ANDROID OPTIMIZATION

Implement:

* Pagination
* Virtualized lists
* Image caching
* Lazy loading
* Local caching
* Incremental sync
* Connection recovery
* Mobile-friendly UI

### Stop condition:

Client remains responsive under realistic network conditions.

---

# PART 37 — SERVER OPTIMIZATION

Measure and optimize:

```text
CPU
RAM
SQLite latency
Simulation tick duration
Feed generation
LLM queue latency
Network bandwidth
```

Optimize using:

* Batching
* Caching
* Indexing
* Aggregation
* Background workers
* Queues
* Efficient serialization
* Query optimization

Do not optimize by deleting history.

---

# PART 38 — DEBUGGING SYSTEM

Create developer tools to inspect:

* NPC
* Personality
* Relationships
* Opinions
* Memories
* Events
* Posts
* Followers
* Communities
* Current activity
* Simulation tick
* LLM requests
* Database queries

Support causal questions:

```text
Why did this relationship change?

Why did this post become viral?

Why did this NPC participate?

Why did this event happen?

Why did this NPC follow this account?

Why did this NPC create this post?
```

---

# PART 39 — SIMULATION LOGGING

Configurable logging for:

```text
Simulation Tick
Event Created
Event Processed
NPC Decision
Relationship Change
Opinion Change
Memory Created
Post Created
Trend Started
Post Became Viral
News Coverage
Moderation
LLM Request
LLM Response
Database Error
Network Error
```

Avoid excessive production logging.

---

# PART 40 — HISTORICAL SYSTEMS

Implement historical views for:

* Accounts
* Followers
* Fame
* Relationships
* Events
* Trends
* Posts
* Metrics

Historical data remains permanent.

---

# PART 41 — CREATOR ECONOMY

Implement later:

* Tips
* Sponsorships
* Creator payouts
* Brand deals
* Paid promotions
* Crowdfunding

Keep it subordinate to the social network.

---

# PART 42 — SECURITY HARDENING

Audit:

* Authentication
* Authorization
* Input validation
* Rate limiting
* API abuse
* Database access
* Secrets
* Internal services
* Ollama access
* Request replay
* Idempotency
* HTTPS

The Android client must never receive privileged server credentials.

---

# PART 43 — PUBLIC SERVER DEPLOYMENT

Move from development environment to public infrastructure.

Potential targets:

* Dedicated PC
* VPS
* Cloud
* Dedicated server

Configure:

```text
Public URL
HTTPS
API
WebSockets
Database
Simulation
LLM endpoint
```

The client should only need the configured server address.

---

# PART 44 — BACKUPS

Because history is permanent, backups are critical.

Implement:

* Database backups
* Configuration backups
* Migration safety
* Restore testing
* Backup verification

Never assume a backup works until restoration has been tested.

---

# PART 45 — SCALE TESTING

Test progressively:

```text
10,000
↓
25,000
↓
50,000
↓
100,000
```

Also test historical record growth.

Measure:

```text
CPU
RAM
SQLite latency
Simulation speed
LLM queue
Network traffic
Feed latency
Android memory
Database size
```

Scale based on actual measurements.

---

# PART 46 — LONG-TERM WORLD TESTING

Run simulations for:

```text
7 days
30 days
1 year
5 years
```

Observe:

* Population behavior
* Relationships
* Fame
* Virality
* Communities
* Trends
* News
* Rumors
* Memory
* Database growth

Look for simulation collapse, repetitive behavior, runaway populations, excessive LLM usage, and broken causal chains.

---

# PART 47 — EMERGENT STORY TESTING

Run the world without the player.

Check whether it naturally produces:

* Friendships
* Romance
* Breakups
* Arguments
* Rivalries
* Viral posts
* Famous accounts
* Failed creators
* News cycles
* Rumors
* Communities
* Trends
* Controversies

The objective is:

> **The world should generate interesting stories without scripted story missions.**

---

# PART 48 — FULL INTEGRATION

Connect:

```text
Accounts
 ↓
Social Graph
 ↓
Posts
 ↓
Feed
 ↓
NPC Simulation
 ↓
Relationships
 ↓
Opinions
 ↓
Communities
 ↓
Events
 ↓
Virality
 ↓
Trends
 ↓
Rumors
 ↓
News
 ↓
Memory
 ↓
LLM
 ↓
Drama
 ↓
History
```

Everything must remain modular.

---

# PART 49 — FINAL WORLD

The final target is:

```text
10,000+ persistent accounts
        │
        ├── Personality
        ├── Interests
        ├── Relationships
        ├── Opinions
        ├── Communities
        ├── Posts
        ├── Events
        ├── Trends
        ├── Rumors
        ├── News
        ├── Memories
        └── History
               │
               ▼
       PERSISTENT WORLD
```

The player is:

```text
ONE ACCOUNT
```

inside that world.

---

# 100. FINAL ARCHITECTURAL PRINCIPLES

These principles must remain true throughout the project.

## Principle 1

The server is authoritative.

## Principle 2

The database is persistent world memory.

## Principle 3

Historical data is never automatically pruned.

## Principle 4

Memory is never automatically forgotten.

## Principle 5

Derived data never replaces original historical data.

## Principle 6

Current state and historical state are separate concepts.

## Principle 7

C# simulation controls world state.

## Principle 8

The LLM generates language, not authoritative simulation state.

## Principle 9

Most NPCs do not need LLM calls.

## Principle 10

10,000 accounts do not mean 10,000 expensive simulations every tick.

## Principle 11

Simulation tiers are used to scale the world.

## Principle 12

Aggregated statistics are used for massive passive audiences.

## Principle 13

Important world changes should have identifiable causes.

## Principle 14

The event system connects major systems.

## Principle 15

Avoid giant manager classes and tightly coupled systems.

## Principle 16

The Android client must remain replaceable independently of the server.

## Principle 17

The backend must remain portable between machines.

## Principle 18

The project must be developed incrementally.

## Principle 19

Never proceed past a broken checkpoint.

## Principle 20

Never solve performance problems by deleting history.

---

# 101. HOW TO WORK ON THIS PROJECT

When instructed to work on this project, first determine:

```text
Current Part
Current Sub-Part
Current Project State
Existing Files
Existing Architecture
Previous Checkpoint
```

Then work ONLY on the requested scope.

For example:

```text
CURRENT TASK:
01E — Android → Backend Communication
```

You must NOT suddenly implement:

```text
NPCs
LLM
Feed algorithms
Relationships
Events
10,000 accounts
```

unless explicitly requested.

---

# 102. REQUIRED RESPONSE FORMAT DURING DEVELOPMENT

For every implementation step, structure the response approximately as:

## 1. What We Are Building

Explain the feature.

## 2. Why We Need It

Explain its purpose.

## 3. Architecture

Explain where it fits.

## 4. Files

List exactly:

```text
Create:
path/file

Modify:
path/file
```

## 5. Implementation

Provide complete code required for the step.

## 6. Explanation

Explain important code and configuration.

## 7. Run

Give exact commands/actions.

## 8. Test

Give exact test procedure.

## 9. Expected Result

Describe what should happen.

## 10. Troubleshooting

List likely failures and fixes.

## 11. Checkpoint

Explain what should be committed.

## 12. STOP

Do not continue into the next major task until the current task works.

---

# 103. WHEN AN ERROR OCCURS

If compilation or runtime errors occur:

1. Stop.
2. Inspect the error.
3. Determine the root cause.
4. Fix it.
5. Recompile.
6. Retest.
7. Only continue after success.

Do not stack new features on top of broken code.

---

# 104. AI CODING AGENT RULES

This project may be developed using:

* ChatGPT
* Claude Code
* Gemini
* Other coding agents

Every coding agent must:

1. Inspect existing code first.
2. Understand architecture before editing.
3. Reuse existing systems.
4. Avoid duplicate services.
5. Avoid duplicate classes.
6. Avoid unnecessary rewrites.
7. Make the smallest reasonable change.
8. Compile/test after changes.
9. Fix errors before continuing.
10. Explain changes.
11. Preserve existing functionality.
12. Check migrations before changing database schema.
13. Avoid destructive database changes.
14. Never delete historical records as an optimization.
15. Never implement automatic memory pruning.
16. Never silently change architectural principles.
17. Never overwrite working functionality without justification.

---

# 105. DATABASE SAFETY

AI coding agents must be extremely careful with SQLite.

Never casually execute:

```sql
DROP TABLE
```

or:

```sql
DELETE FROM Memories
```

or:

```sql
DELETE FROM Events
```

or:

```sql
DELETE FROM Posts
```

or destructive equivalents.

Database migrations must preserve existing historical information.

Before potentially destructive schema changes:

```text
Backup
 ↓
Migration
 ↓
Verification
```

---

# 106. ABSOLUTE NO-PRUNING RULE

Never introduce:

```text
Delete memories older than X
Delete events older than X
Delete posts older than X
Delete inactive NPC history
Delete relationship history
Delete old metrics
Delete old trends
Delete old messages
Delete old news
```

unless the project owner explicitly requests it later.

Performance must come from:

```text
Indexes
Caching
Pagination
Compression where appropriate
Aggregation
Query optimization
Simulation tiers
Batching
Background processing
Efficient storage
```

NOT deletion.

---

# 107. FIRST DEVELOPMENT OBJECTIVE

The first objective is ONLY:

```text
Android Client
        ↓
Internet/network
        ↓
ASP.NET Core Backend
        ↓
SQLite
        ↓
Response
        ↓
Android
```

Do not start with:

```text
NPC AI
LLM
10,000 NPC behaviors
Virality
Romance
Events
Drama
```

Build the foundation first.

---

# 108. FIRST CHECKPOINT

The first major checkpoint is:

```text
Android application launches.

        ↓

Android connects to configured backend.

        ↓

Backend responds.

        ↓

Backend can access SQLite.

        ↓

SQLite can persist data.

        ↓

Android displays successful server response.

        ↓

Git checkpoint created.
```

Only after this foundation works should development proceed to accounts.

---

# 109. MASTER STOP RULE

At every major phase:

```text
IMPLEMENT
    ↓
COMPILE
    ↓
RUN
    ↓
TEST
    ↓
FIX
    ↓
VERIFY
    ↓
CHECKPOINT
```

Then stop.

Never assume the next phase is safe until the previous foundation works.

---

# 110. ULTIMATE EXPERIENCE

The player creates an account.

Initially:

```text
0 followers
0 following
0 posts
```

The player follows people.

Makes a post.

Someone likes it.

Another person follows them.

They comment.

A friendship begins.

Meanwhile, independently:

```text
A celebrity starts an event.

A gaming community creates a trend.

Two influencers argue.

A rumor spreads.

A news account covers the argument.

An unrelated NPC becomes famous.

Another NPC gets suspended.

A creator's post becomes viral.

Two NPCs start dating.

Another NPC breaks up.

A community starts fighting.

A poll becomes controversial.
```

The player returns later.

The world has changed.

The database remembers what happened.

---

# 111. ULTIMATE GOAL

The final system should behave less like:

```text
Player
 ↓
NPC
 ↓
NPC responds
```

and more like:

```text
10,000+ Accounts
        ↓
Social Graph
        ↓
Communities
        ↓
Posts
        ↓
Relationships
        ↓
Opinions
        ↓
Events
        ↓
Trends
        ↓
Virality
        ↓
News
        ↓
Rumors
        ↓
Memories
        ↓
Social Consequences
        ↓
More Events
        ↓
More History
        ↓
More Social Consequences
```

The world should continuously generate stories.

The player is simply one participant.

---

# 112. STARTING INSTRUCTION

# START WITH PART 01A.

Do not skip setup.

Do not jump to NPCs.

Do not jump to the LLM.

Do not create 10,000 accounts yet.

Do not create the entire database yet.

Do not dump the entire project into one response.

Begin by:

1. Explaining the development environment.
2. Determining the required software.
3. Explaining the Android technology choice.
4. Explaining the ASP.NET Core backend choice.
5. Explaining SQLite.
6. Explaining Git/GitHub.
7. Giving exact installation/setup instructions.
8. Verifying the environment.
9. Stopping at the Part 01A checkpoint.

After Part 01A works, proceed to Part 01B.

# BUILD THIS PROJECT WITH ME ONE WORKING CHECKPOINT AT A TIME.

# DO NOT RUSH.

# DO NOT SKIP FOUNDATION.

# DO NOT DELETE HISTORY.

# DO NOT PRUNE MEMORY.

# DO NOT MAKE THE LLM THE SIMULATION.

# KEEP THE SERVER AUTHORITATIVE.

# KEEP THE WORLD PERSISTENT.

# KEEP THE ARCHITECTURE MODULAR.

# BUILD THE MINIATURE INTERNET.
