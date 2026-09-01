# SOCIAL MEDIA SIMULATOR — PART 17 DEVELOPMENT PROMPT
## LLM-DRIVEN EVENT SYSTEM

You are continuing development of the **Social Media Simulator** from the existing project.

**DO NOT restart, redesign, or replace the existing architecture.**

You must inspect the current repository first and build directly on everything already implemented.

---

# IMPORTANT: LLM-DRIVEN ARCHITECTURE

This part uses **local Ollama** for all event detection, event generation, and narrative logic. The LLM is the brain that decides what events happen, how they unfold, and what consequences they have.

**API Configuration:**
- Endpoint: Your local Ollama instance (e.g., `http://localhost:11434`)
- API Key: `eb83536349244577bc482f76d21bc55f.JFqxh_kUnppKPlpmBsDZBMeG`
- Model: Use your configured model (e.g., qwen3-4b, llama2, mistral, etc.)

**ABSOLUTE RULE:** Do NOT use DeepSeek API or any other external API. Only use local Ollama.

---

# CURRENT PROJECT CHECKPOINT

Completed:

```text
01A  Development Environment         COMPLETE
01B  Repository Foundation           COMPLETE
01C  ASP.NET Core Server            COMPLETE
01D  SQLite Foundation              COMPLETE
01E  Android Client Foundation      COMPLETE
01F  Foundation Checkpoint          COMPLETE
02   Backend Architecture           COMPLETE
03   Persistence                    COMPLETE
04   Accounts & Authentication     COMPLETE
05   Social Graph                   COMPLETE
06   Posts & Engagement             COMPLETE
07   Feed & Timeline                COMPLETE
08   NPC Simulator Foundation       COMPLETE
09   NPC Population Generation      COMPLETE
10   NPC Behavior Simulation        COMPLETE
11   NPC Background Simulation      COMPLETE
12   NPC Social Graph               COMPLETE
13   AI Content Generation          COMPLETE
14   Notifications System           COMPLETE
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

---

# 1. WHY THIS PART, NOW

Parts 01–16 built the core social media infrastructure with an advanced algorithmic feed. The world has accounts, posts, communities, relationships, opinions, and an intelligent feed — but it lacks **emergent narrative events** driven by intelligent LLM reasoning.

Part 17 introduces the **LLM-Driven Event System** — the heart of emergent storytelling. The LLM (via local Ollama) acts as the world narrator, continuously analyzing the social landscape and deciding what interesting events should happen.

Instead of hardcoded rules like "if post has 100 likes, create viral event," the LLM reasons like:

> "Sarah has been dating Kevin for 2 weeks. Kevin just followed Emma, who Sarah is jealous of. Sarah noticed and posted a passive-aggressive comment. The LLM decides: this is the start of a potential jealousy drama event."

This creates organic, unpredictable, and entertaining emergent stories that make the world feel alive.

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
- `AiContentGeneratorService` + `IAiTextGenerationService` — existing Ollama integration (Part 13)
- Account interests, personality traits, relationship dimensions
- World clock and simulation tick system (Part 11)

The `AiContentGeneratorService` from Part 13 already has Ollama integration. Part 17 extends this to create an **Event Generation Service** that uses the same Ollama connection.

---

# 3. OLLAMA CONFIGURATION

## Existing Integration (Part 13)

Inspect the existing `AiContentGeneratorService` and `IAiTextGenerationService` from Part 13. These provide the foundation for LLM communication.

## New Configuration

Add/update configuration for Ollama:

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "ApiKey": "eb83536349244577bc482f76d21bc55f.JFqxh_kUnppKPlpmBsDZBMeG",
    "DefaultModel": "your-configured-model",
    "EventGenerationModel": "your-configured-model",
    "TimeoutSeconds": 60,
    "MaxRetries": 3
  }
}
```

**Important:**
- The API key format `key.model` indicates the model to use. Extract the model name from the key.
- If Ollama doesn't require authentication, use `ApiKey: ""` or your Ollama configuration.
- Ensure the model supports function calling or structured output if needed.

---

# 4. MASTER ARCHITECTURE PRINCIPLES

## Server Authoritative

The LLM proposes events, but the server validates and executes them. The LLM never directly modifies database state. The server always has final authority.

## Layered Architecture

```text
API
Application
Domain
Infrastructure
    └── Ollama (LLM)
```

## Reuse, don't duplicate

Extend the existing `IAiTextGenerationService` for event generation. Do not create a separate LLM client.

## C# Simulation Authority + LLM Narrative

```text
C# controls:
- World state (accounts, relationships, posts)
- Rules (what actions are allowed)
- Consequences (follower changes, fame changes)
- Validation (is this event valid?)

LLM controls:
- Narrative (what story should unfold)
- Event timing (when should this happen)
- Event context (why is this interesting)
- Character motivation (why would NPC X do this?)
```

## Permanent data rule

Per the project's permanent-history principle (Part 01B), all events must NOT be automatically deleted/pruned.

---

# PART 17 OBJECTIVE

Implement an **LLM-Driven Event System** where local Ollama is the narrative engine:

1. **Event Entity** — First-class event records with type, status, participants, and narrative context.
2. **Event Generation Service** — LLM-powered service that analyzes world state and proposes events.
3. **Event Execution Pipeline** — Server validates and executes LLM-proposed events safely.
4. **Event Consequences** — C# applies consequences determined by events.
5. **Event Queries** — APIs to browse and filter events.
6. **Event History** — All events remain permanently queryable.

Do NOT implement in this part:

- Event causality chains (Part 18)
- Offline event processing (Part 18)
- Event UI in Android
- Event predictions or recommendations
- Event templates or scripted events
- Any external API besides local Ollama

---

# PART 17 — REQUIRED FEATURES

## 1. Event Entity

Create an `Event` entity:

```text
Id                      (GUID)
Type                    (enum — see Section 2)
Title                   (string — LLM-generated dramatic title)
Description             (string — LLM-generated narrative description)
NarrativeContext        (string — LLM's reasoning for why this event happened)
CreatorAccountId        (GUID — LLM/system, nullable)
CreatedAt               (timestamp)
StartAt                 (timestamp)
EndAt                   (nullable timestamp)
Status                  (enum — Proposed, Approved, Active, Ended, Rejected, Cancelled)
Visibility              (enum — Public, FollowersOnly, CommunityOnly, Private)
Topic                   (string — primary topic tag)
CommunityId             (nullable GUID)
Popularity              (int — current engagement level)
ParticipantCount        (int — denormalized)
MaxParticipants         (nullable int)
Metadata                (JSON — LLM-provided context: involved accounts, relationships, tensions)
IsDeleted               (bool — soft delete)
```

### Event Types (Enum)

```text
Drama
    ├── JealousyIncident
    ├── PublicArgument
    ├── Betrayal
    ├── RedemptionArc
    ├── ComebackStory
    └── DownfallStory
    
Romance
    ├── NewRelationship
    ├── Breakup
    ├── LoveTriangle
    ├── SecretRelationship
    ├── RelationshipMilestone
    └── Reconciliation
    
Social
    ├── NewFriendship
    ├── FriendshipEnded
    ├── Alliance
    ├── Rivalry
    ├── FanWar
    └── TrollAttack

Fame
    ├── RiseToFame
    ├── FallFromGrace
    ├── Scandal
    ├── Apology
    ├── Comeback
    └── Cancellation

Community
    ├── CommunityDriven
    ├── CommunitySplit
    ├── CommunityMilestone
    └── CommunityDrama

Content
    ├── ViralPost
    ├── ViralComment
    ├── QuotePostDrama
    └── PollControversy

Trend
    ├── TrendStart
    ├── TrendPivot
    └── TrendDeath

News
    ├── NewsCoverage
    ├── BreakingNews
    └── NewsDebate
```

---

## 2. EventParticipation Entity

```text
Id                      (GUID)
EventId
AccountId
Role                    (enum — Protagonist, Antagonist, Supporter, Victim, Bystander, Narrator)
JoinedAt                (timestamp)
ContributionScore        (int — how much this account contributed)
Status                  (enum — Active, Withdrew, WasRemoved, Completed)
LLMReasoning            (string — why LLM chose this account)
```

---

## 3. Event Generation Service (LLM-Driven)

### IEventGenerationService

```csharp
public interface IEventGenerationService
{
    Task<Event?> ProposeNextEventAsync();
    Task<Event?> ProposeEventForAccountAsync(Guid accountId);
    Task<Event?> ProposeCommunityEventAsync(Guid communityId);
    Task<bool> ValidateEventProposalAsync(EventProposal proposal);
    Task ApproveAndExecuteEventAsync(EventProposal proposal);
}
```

### LLM Prompt for Event Generation

The core of Part 17 is the LLM prompt that drives event generation:

```text
SYSTEM PROMPT:
You are the narrative director of a social media simulation world.
Your job is to create interesting, dramatic, and organic events that emerge naturally from the social landscape.

You analyze:
1. Recent posts and their engagement
2. Relationship tensions and dynamics
3. Community moods and activities
4. Individual NPC personalities and histories
5. Current trends and topics

You decide:
1. What interesting event should happen next
2. Who should be involved
3. Why this event makes sense given the context
4. How the event should unfold narratively

RULES:
- Events must emerge from existing state, not be random
- Consider character personalities when deciding actions
- Drama is good, but meaningless cruelty is not
- Events should have consequences that ripple outward
- Give each event a compelling narrative hook

OUTPUT FORMAT:
Respond with a JSON event proposal:
{
  "eventType": "Drama.PublicArgument",
  "title": "The Big Fight",
  "description": "Sarah publicly called out Kevin for stealing her content idea...",
  "narrativeContext": "Sarah and Kevin have been rivals for months. Kevin's recent post was suspiciously similar to Sarah's thread from last week. Sarah finally had enough.",
  "primaryAccountId": "guid-of-sarah",
  "secondaryAccountId": "guid-of-kevin",
  "topic": "content-theft",
  "dramaLevel": 7,
  "participants": [
    {"accountId": "guid-of-sarah", "role": "Protagonist", "reasoning": "She's the wronged party and has high aggression trait"},
    {"accountId": "guid-of-kevin", "role": "Antagonist", "reasoning": "He's defensive and has a history of this behavior"},
    {"accountId": "guid-of-emma", "role": "Supporter", "reasoning": "She's Sarah's best friend and will defend her"}
  ],
  "expectedConsequences": [
    {"type": "RelationshipChange", "accounts": ["sarah", "kevin"], "trust": -20, "hostility": +15},
    {"type": "FollowerChange", "account": "kevin", "delta": -50},
    {"type": "PostCreation", "account": "sarah", "content": "She's going to post about it"}
  ],
  "followUpEventProbability": 0.8,
  "narrativeArcLength": 3
}
```

### Event Generation Loop

The `NpcSimulationHostedService` calls event generation on each tick:

```csharp
// In simulation tick loop
public class NpcSimulationHostedService
{
    private readonly IEventGenerationService _eventService;
    
    // Run event generation periodically (e.g., every 5-10 ticks)
    private async Task SimulationTickAsync()
    {
        // Existing NPC behavior processing...
        
        // LLM-driven event generation
        if (_shouldGenerateEvent)
        {
            var event = await _eventService.ProposeNextEventAsync();
            if (event != null)
            {
                await _eventService.ApproveAndExecuteEventAsync(event);
            }
        }
    }
}
```

### Event Proposal Validation

Before executing, the server validates:

```csharp
// Validate the LLM's proposal is valid
public async Task<bool> ValidateEventProposalAsync(EventProposal proposal)
{
    // 1. All referenced accounts exist and are active
    // 2. Relationship states support the event
    // 3. Account personalities allow their proposed actions
    // 4. Event doesn't contradict recent history
    // 5. Event respects block/mute rules
    // 6. Event doesn't duplicate a very recent similar event
}
```

---

## 4. Event Execution Pipeline

Once approved, events are executed safely:

### ExecuteEventAsync

```csharp
public async Task ExecuteEventAsync(Event approvedEvent)
{
    using var transaction = await _dbContext.BeginTransactionAsync();
    
    try
    {
        // 1. Create Event record
        // 2. Create EventParticipation records
        // 3. Apply consequences (via existing services)
        //    - Relationship changes
        //    - Follower changes
        //    - Fame changes
        //    - Post creations
        // 4. Notify affected accounts
        // 5. Update event status to Active
        
        await transaction.CommitAsync();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Failed to execute event {EventId}", approvedEvent.Id);
        // Mark event as Rejected
    }
}
```

### LLM-Generated Content in Events

The LLM can also generate the actual posts/content that events produce:

```csharp
// When event creates a post
public async Task<Post> CreateEventPostAsync(Guid accountId, Event event, string hint)
{
    var prompt = $"Generate a social media post from {accountId}'s perspective about: {event.Description}. {hint}";
    var content = await _aiContentService.GenerateTextAsync(prompt);
    return await _postService.CreatePostAsync(accountId, content);
}
```

---

## 5. Event Consequences (C#)

The LLM proposes consequences, but C# validates and applies them:

### Consequence Types

```csharp
public enum ConsequenceType
{
    RelationshipChange,
    FollowerChange,
    FameChange,
    ReputationChange,
    PostCreation,
    Notification,
    MemoryCreation,
    OpinionChange,
    CommunityMembershipChange,
    FollowAction,
    UnfollowAction,
    BlockAction,
    PostLike,
    PostComment
}

public class EventConsequence
{
    public ConsequenceType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public bool WasExecuted { get; set; }
    public string FailureReason { get; set; }
}
```

### ApplyConsequencesAsync

```csharp
public async Task ApplyConsequencesAsync(Event evt)
{
    var consequences = ParseLLMConsequences(evt.Metadata["expectedConsequences"]);
    
    foreach (var consequence in consequences)
    {
        try
        {
            switch (consequence.Type)
            {
                case ConsequenceType.RelationshipChange:
                    await _socialGraphService.ChangeRelationshipAsync(...);
                    break;
                case ConsequenceType.FollowerChange:
                    await _accountService.AdjustFollowersAsync(...);
                    break;
                case ConsequenceType.PostCreation:
                    await CreateEventPostAsync(...);
                    break;
                // ... etc
            }
            consequence.WasExecuted = true;
        }
        catch (Exception ex)
        {
            consequence.WasExecuted = false;
            consequence.FailureReason = ex.Message;
        }
    }
    
    // Log all consequences for audit
}
```

---

## 6. NPC Event Awareness

NPCs are aware of events and react to them:

### EventContext for NPC Decisions

When NPCs decide actions, they receive event context:

```csharp
public class NpcDecisionContext
{
    // Existing fields from Part 10-13...
    
    // New: Event awareness
    public Event ActiveEvent { get; set; }        // Event NPCs are currently in
    public List<Event> RecentEvents { get; set; } // Events that just ended
    public double DramaAwareness { get; set; }     // NPC's awareness of drama
}
```

### LLM Prompt for NPC Event Reactions

```text
NPC {npc_name} just received context about: {event_description}

NPC Profile:
- Personality: {personality traits}
- Relationships: {key relationships}
- Current mood: {mood state}
- Drama tendency: {0-100}

Should {npc_name} react to this event? If yes, what should they do?
Options: Post, Comment, Like, Follow, ReactPrivately, Ignore

Generate the content and reasoning.
```

---

## 7. API Endpoints

### Browse Events
```http
GET /api/events
```
Parameters: type, topic, status, cursor, pageSize

### Event Details
```http
GET /api/events/{id}
```

### Event Participants
```http
GET /api/events/{id}/participants
```

### My Events
```http
GET /api/accounts/{id}/events
```

---

## 8. Database Migration

### New Tables
- `Event` — main event records
- `EventParticipation` — participation records
- `EventConsequence` — consequence audit log

### Indexes
- `Event(Status, CreatedAt)`
- `Event(Type, Status)`
- `Event(RelatedAccountId)`
- `Event(RelatedCommunityId)`
- `EventParticipation(EventId, AccountId)`

---

## 9. Tests

### LLM Integration Tests
```text
Event generation returns valid JSON proposal
Invalid proposals are rejected by validator
LLM generates contextually appropriate events
LLM respects account personalities
```

### Event Execution Tests
```text
Approved events execute all consequences
Failed consequences are logged but don't block event
Event status transitions correctly
Notifications sent to participants
```

### NPC Integration Tests
```text
NPCs aware of active events
NPCs react to events based on personality
NPC reactions are LLM-generated
```

### Persistence Tests
```text
Events persist across restart
Event history is queryable
Consequence audit trail is complete
```

### Regression Tests
```text
Existing Parts 01-16 tests still pass
```

---

## 10. Android

Part 17 is backend-only. Minimal model adjustments only.

---

## 11. README — REQUIRED

Document:
- Part 17 completion
- LLM-driven event architecture
- Ollama integration
- Event types supported
- Event generation prompt
- Event execution pipeline
- NPC event awareness
- API endpoints
- Database changes
- Tests performed
- Current status
- Next planned part

---

## 12. Git

After implementation:
1. Inspect `git status`
2. Commit: `Implement LLM-driven event system (Part 17)`
3. Push to `origin/main`
4. Verify against origin

---

## 13. DO NOT IMPLEMENT YET

Do NOT implement:
- Event causality chains (Part 18)
- Offline event processing (Part 18)
- Android event UI
- Event predictions
- Event severity scoring
- Scripted event templates
- DeepSeek API or external APIs (ONLY local Ollama)

---

## 14. QUALITY REQUIREMENTS

- Correct (LLM output parsed correctly)
- Safe (server validates all LLM proposals)
- Performant (event generation doesn't block ticks)
- Testable (mock LLM for tests)
- Configurable (Ollama endpoint/key configurable)

---

## 15. FINAL VERIFICATION

```text
Server builds
Ollama connection works
Event generation produces valid proposals
Invalid proposals rejected
Event execution applies consequences
Events persist across restart
NPCs react to events
Existing tests pass
README updated
Git commit pushed
Working tree clean
```

---

## 16. FINAL SESSION REPORT

```text
# PART 17 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. LLM-Driven Event Architecture
...

## 5. Ollama Configuration
...

## 6. Event Generation Prompt
...

## 7. Event Execution Pipeline
...

## 8. NPC Event Awareness
...

## 9. API Endpoints
...

## 10. Database Changes
...

## 11. Tests
...

## 12. README
Updated: YES
...

## 13. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 14. Current Project Status
01A-17 COMPLETE

## 15. Intentionally Not Implemented
- Event causality chains
- Offline event processing
- Android event UI
- External APIs (Ollama only)

## 16. NEXT
NEXT: PART 18 — Event Causality & Offline Simulation
```

**STOP after completing Part 17 and reporting the session log.**
