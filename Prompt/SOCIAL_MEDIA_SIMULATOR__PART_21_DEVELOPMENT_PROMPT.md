# SOCIAL MEDIA SIMULATOR — PART 21 DEVELOPMENT PROMPT
## RUMORS

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
01D  SQLite Foundation              COMPLETE
01E  Android Client Foundation      COMPLETE
01F  Foundation Checkpoint           COMPLETE
02   Backend Architecture            COMPLETE
03   Persistence                   COMPLETE
04   Accounts & Authentication       COMPLETE
05   Social Graph                   COMPLETE
06   Posts & Engagement             COMPLETE
07   Feed & Timeline               COMPLETE
08   NPC Simulator Foundation        COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation        COMPLETE
11   NPC Background Simulation       COMPLETE
12   NPC Social Graph              COMPLETE
13   AI Content Generation          COMPLETE
14   Notifications System           COMPLETE
15   Communities                   COMPLETE
16   Advanced Feed                 COMPLETE
17   LLM-Driven Event System       COMPLETE
18   Event Causality & Offline Sim  COMPLETE
19   Virality                      COMPLETE
20   Topics & Trends               COMPLETE
```

Latest commit:

```text
6c0f956 — Part 20: Topics and trends system
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 20.

---

# 1. WHY THIS PART, NOW

Parts 01–20 built a social media platform with virality and trends. Content spreads, posts go viral, topics trend — but there's no **uncertain information spreading**. Every post is treated as fact.

Part 21 introduces **Rumors** — information that spreads without automatically becoming fact. Rumors add a critical layer of realism: not everything you read is true, people believe things for reasons, evidence accumulates, contradictions emerge, and the truth eventually comes out (or doesn't).

Without rumors, the platform is too clean and trustworthy. With rumors:
- NPCs can spread unverified gossip
- Accounts can have beliefs that don't match reality
- Evidence can contradict rumors
- Rumors can be confirmed or debunked
- Drama emerges naturally from misinformation

Rumors are foundational to:
- News (Part 22) — news accounts cover rumors
- Social Drama (Part 27) — rumors cause drama
- Reputation — rumors affect reputation
- NPC Memory (Part 23) — NPCs remember rumors

---

# 2. THE EXISTING PROJECT

The existing backend contains from Parts 01–20:

- Everything from Part 20 and earlier
- **Topics & Trends (Part 20):** Trends track topic activity
- **Virality (Part 19):** Posts can go viral
- **Event System (Part 17):** Events can be created
- **NPC Behavior (Parts 10-12):** NPCs have personalities that drive behavior
- **Social Graph (Part 05):** Follow/relationship system
- Posts, Comments with engagement

The infrastructure exists:
- Posts and comments that can carry rumor content
- Events that can be rumor-driven
- NPC personalities with traits like GossipTendency, DramaTendency
- Trends that can amplify rumors

Part 21 adds the rumor concept: information with uncertain truth value that spreads through the network.

---

# 3. MASTER ARCHITECTURE PRINCIPLES

## Server Authoritative

Rumors are managed by the server. The server tracks what information exists, how it spreads, and what people believe. Rumors can be confirmed or debunked, but the server never "auto-corrects" misinformation without process.

## C# + LLM Hybrid

- C# manages rumor state, spread mechanics, and belief calculations
- LLM generates rumor content and assesses evidence
- Server validates all rumor-related actions

## Permanent Data Rule

All rumors, beliefs, evidence, and contradictions must NOT be automatically deleted/pruned. Even debunked rumors remain in history.

## Core Concept: Information != Fact

```
Information → Spreads → Becomes Belief → Evidence Accumulates → Truth Emerges
                                    ↓
                              Some believe
                              Some don't
                                    ↓
                              Rumor persists
                              or dies
```

---

# PART 21 OBJECTIVE

Implement a **Rumors System** where information spreads with uncertain truth value:

1. **Rumor Entity** — Information with truth status
2. **Belief Tracking** — What each account believes
3. **Evidence System** — Supporting and contradicting evidence
4. **Rumor Spread** — How rumors propagate
5. **Truth Emergence** — How rumors get confirmed or debunked
6. **Rumor Events** — Events driven by rumors
7. **Rumor API** — Endpoints for rumor management

Do NOT implement in this part:
- News accounts covering rumors (Part 22)
- Full fact-checking systems
- Moderation tools
- Deep investigation mechanics

---

# PART 21 — REQUIRED FEATURES

## 1. Rumor Entity

Create a `Rumor` entity:

```csharp
public class Rumor
{
    public Guid Id { get; set; }
    
    // Content
    public string Subject { get; set; }              // "Kevin is dating Sarah"
    public string Description { get; set; }           // Longer description
    public Guid SubjectAccountId { get; set; }       // Who the rumor is about
    
    // Truth Status
    public RumorTruthStatus TruthStatus { get; set; } // See enum
    public float TruthConfidence { get; set; }       // 0.0 - 1.0 (system's estimate)
    
    // Origin
    public Guid OriginAccountId { get; set; }        // Who started the rumor
    public DateTime OriginDate { get; set; }
    public Guid? OriginPostId { get; set; }          // Post that started it
    
    // Rumor Type
    public RumorType Type { get; set; }              // See enum
    public string Topic { get; set; }                // Related topic
    
    // Metrics
    public int BelieverCount { get; set; }           // How many believe it
    public int DoubterCount { get; set; }           // How many doubt it
    public int TotalMentions { get; set; }           // How many times mentioned
    public int PostCount { get; set; }              // Posts about this rumor
    
    // Evidence
    public int SupportingEvidenceCount { get; set; }
    public int ContradictingEvidenceCount { get; set; }
    
    // Lifecycle
    public RumorStatus Status { get; set; }          // See enum
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? DebunkedAt { get; set; }
    public DateTime? DiedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum RumorTruthStatus
{
    Unknown = 0,       // Truth value unknown
    LikelyTrue = 1,    // Evidence suggests true
    LikelyFalse = 2,   // Evidence suggests false
    ConfirmedTrue = 3, // Officially confirmed true
    ConfirmedFalse = 4,// Officially confirmed false
    Unverifiable = 5   // Can never be verified
}

public enum RumorType
{
    Gossip,            // Social gossip ("dating", "friend breakup")
    Scandal,           // Negative rumor ("cheating", "lying")
    Achievement,       // Positive rumor ("got a job", "won award")
    Relationship,      // Relationship changes
    Professional,      // Career-related
    Personal,          // Personal life
    Conspiracy,        // Complex/layered rumor
    Hoax               // Deliberate fake
}

public enum RumorStatus
{
    Active = 0,        // Spreading
    Stalling = 1,      // Interest declining
    Confirmed = 2,     // Proven true
    Debunked = 3,      // Proven false
    Died = 4           // Faded away
}
```

---

## 2. Account Belief Entity

Track what each account believes:

```csharp
public class AccountBelief
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid RumorId { get; set; }
    
    // Belief state
    public BeliefLevel Belief { get; set; }         // See enum
    public float Confidence { get; set; }              // 0.0 - 1.0
    
    // How they formed this belief
    public BeliefSource Source { get; set; }           // See enum
    public Guid? SourceAccountId { get; set; }        // Who told them
    public Guid? SourcePostId { get; set; }           // Post they saw
    
    // History
    public bool ChangedMind { get; set; }              // Did they change their belief?
    public DateTime? PreviousBeliefDate { get; set; }
    public BeliefLevel? PreviousBelief { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum BeliefLevel
{
    StronglyBelieves = 5,   // 100% sure true
    Believes = 4,          // Probably true
    Uncertain = 3,         // Could go either way
    Doubts = 2,            // Probably false
    StronglyDoubts = 1,    // 100% sure false
    Unaware = 0            // Doesn't know about rumor
}

public enum BeliefSource
{
    DirectWitness,         // Saw it happen
    TrustedFriend,         // Heard from trusted friend
    CasualContact,         // Heard from casual contact
    Post,                  // Saw a post
    Comment,               // Read a comment
    Event,                 // Saw it in an event
    Trend,                 // Saw it trending
    News                   // Saw it from news account
}
```

---

## 3. Evidence Entity

Track evidence for and against rumors:

```csharp
public class RumorEvidence
{
    public Guid Id { get; set; }
    public Guid RumorId { get; set; }
    public Guid AccountId { get; set; }               // Who provided evidence
    
    // Evidence type
    public EvidenceType Type { get; set; }            // See enum
    public bool SupportsRumor { get; set; }           // True = supports, False = contradicts
    
    // Content
    public string Description { get; set; }
    public Guid? RelatedPostId { get; set; }         // Post with evidence
    public Guid? RelatedEventId { get; set; }         // Event as evidence
    
    // Credibility
    public float Credibility { get; set; }             // 0.0 - 1.0
    public EvidenceStrength Strength { get; set; }     // See enum
    
    // Verification
    public bool IsVerified { get; set; }               // Has it been checked?
    public Guid? VerifiedByAccountId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public enum EvidenceType
{
    Post,                  // A post contains evidence
    Comment,               // A comment provides evidence
    Photo,                 // Photo evidence
    Screenshot,            // Screenshot evidence
    Testimony,             // Someone's statement
    Event,                 // An event proves/disproves
    Contradiction,         // Contradicts other evidence
    Verification           // Official verification
}

public enum EvidenceStrength
{
    Weak = 1,              // Circumstantial
    Moderate = 2,          // Suggestive
    Strong = 3,            // Compelling
    Definitive = 4        // Proves/disproves
}
```

---

## 4. Rumor Creation

### How Rumors Start

Rumors can originate from:

1. **NPC-initiated** — NPCs gossip based on personality
2. **Event-driven** — Events trigger rumors
3. **Player-initiated** — Players can spread rumors
4. **LLM-generated** — LLM can propose rumors based on context

### LLM Rumor Generation Prompt

```text
SYSTEM: You are analyzing the social landscape to create plausible rumors.

WORLD CONTEXT:
- {accounts and their relationships}
- {recent events}
- {existing tensions and drama}
- {NPC personalities}

Identify if there's potential for a new rumor to emerge. A rumor should:
- Be plausible but uncertain
- Involve interesting social dynamics
- Have potential for drama
- Be grounded in existing relationships/events

If a rumor should emerge:
{
  "shouldCreateRumor": true/false,
  "subject": "Kevin and Sarah",
  "description": "Kevin has been seen with Sarah's ex-boyfriend...",
  "type": "Relationship",
  "topic": "celebrity",
  "initialBelievers": ["account_id_1", "account_id_2"],
  "initialDoubters": ["account_id_3"],
  "originAccountId": "account_id_1",
  "truthStatus": "Unknown",
  "isActuallyTrue": true/false (keep this secret, don't expose to game)
}
```

### Rumor Creation Service

```csharp
public class RumorService
{
    public async Task<Rumor> CreateRumorAsync(RumorCreationRequest request)
    {
        var rumor = new Rumor
        {
            Subject = request.Subject,
            Description = request.Description,
            SubjectAccountId = request.SubjectAccountId,
            Type = request.Type,
            Topic = request.Topic,
            TruthStatus = RumorTruthStatus.Unknown,
            TruthConfidence = 0.5f, // Unknown
            OriginAccountId = request.OriginAccountId,
            OriginDate = DateTime.UtcNow,
            Status = RumorStatus.Active
        };
        
        await _rumorRepo.CreateAsync(rumor);
        
        // Create initial beliefs
        foreach (var believerId in request.InitialBelievers)
        {
            await CreateBeliefAsync(believerId, rumor.Id, BeliefLevel.Believes);
        }
        
        foreach (var doubterId in request.InitialDoubters)
        {
            await CreateBeliefAsync(doubterId, rumor.Id, BeliefLevel.Doubts);
        }
        
        return rumor;
    }
}
```

---

## 5. Belief System

### How Beliefs Form

Accounts form beliefs based on:

1. **Source Credibility** — Who told them
2. **Network Position** — Friends of friends vs strangers
3. **Prior Beliefs** — Consistent with existing beliefs
4. **Evidence** — Supporting or contradicting evidence
5. **Rumor Characteristics** — Plausible vs outlandish

### Belief Calculation

```csharp
public float CalculateBeliefChange(
    Account account, 
    Rumor rumor, 
    BeliefSource source,
    Guid sourceAccountId)
{
    var baseBelief = 0.5f; // Start neutral
    
    // Source credibility modifier
    var sourceCredibility = GetSourceCredibility(source);
    baseBelief += sourceCredibility;
    
    // Relationship with source
    var sourceRelationship = await _socialGraphService.GetRelationshipAsync(
        account.Id, sourceAccountId);
    var trustModifier = sourceRelationship?.Trust / 200.0f; // -0.5 to +0.5
    baseBelief += trustModifier;
    
    // Account personality
    if (account.Personality.Credulity > 0.7) baseBelief += 0.1f; // Gullible
    if (account.Personality.Skepticism > 0.7) baseBelief -= 0.1f; // Skeptic
    
    // Clamp to 0-1
    return Math.Clamp(baseBelief, 0f, 1f);
}

public BeliefLevel CalculateBeliefLevel(float beliefValue)
{
    return beliefValue switch
    {
        > 0.85f => BeliefLevel.StronglyBelieves,
        > 0.65f => BeliefLevel.Believes,
        > 0.35f => BeliefLevel.Uncertain,
        > 0.15f => BeliefLevel.Doubts,
        _ => BeliefLevel.StronglyDoubts
    };
}
```

---

## 6. Rumor Spread Mechanics

### How Rumors Spread

```csharp
public async Task ProcessRumorSpreadAsync(Guid rumorId)
{
    var rumor = await _rumorRepo.GetAsync(rumorId);
    if (rumor.Status != RumorStatus.Active) return;
    
    // Get accounts who are aware but not yet believers
    var potentialBelievers = await GetAccountsToSpreadToAsync(rumorId);
    
    foreach (var accountId in potentialBelievers)
    {
        var spreadProb = CalculateSpreadProbabilityAsync(accountId, rumor);
        
        if (Random.NextDouble() < spreadProb)
        {
            // Account becomes aware and forms belief
            var beliefValue = CalculateBeliefChange(accountId, rumor);
            var beliefLevel = CalculateBeliefLevel(beliefValue);
            
            await CreateBeliefAsync(accountId, rumorId, beliefLevel);
            
            // Rumor metrics updated
            rumor.TotalMentions++;
            if (beliefLevel >= BeliefLevel.Believes)
                rumor.BelieverCount++;
            else
                rumor.DoubterCount++;
        }
    }
}

public async Task<double> CalculateSpreadProbabilityAsync(Guid accountId, Rumor rumor)
{
    var account = await _accountService.GetAsync(accountId);
    
    // Base probability
    var baseProb = 0.05; // 5% base
    
    // Gossip tendency - higher = more likely to spread
    var gossipBoost = account.Personality.GossipTendency / 200.0; // Up to +0.5
    
    // Drama tendency - dramatic rumors spread more
    var dramaBoost = account.Personality.DramaTendency / 400.0;
    
    // Already knows someone involved - more likely to care
    var connectionBoost = await _socialGraphService.IsConnectedToAsync(
        accountId, rumor.SubjectAccountId) ? 0.1 : 0;
    
    // Topic interest
    var topicBoost = account.Interests.Contains(rumor.Topic) ? 0.05 : 0;
    
    // Trending boost
    var trendBoost = await _trendService.IsTrendingAsync(rumor.Topic) ? 0.1 : 0;
    
    return Math.Min(0.9, baseProb + gossipBoost + dramaBoost + connectionBoost + topicBoost + trendBoost);
}
```

---

## 7. Evidence Accumulation

### Evidence Changes Belief

```csharp
public async Task AddEvidenceAsync(Guid rumorId, RumorEvidence evidence)
{
    await _evidenceRepo.CreateAsync(evidence);
    
    var rumor = await _rumorRepo.GetAsync(rumorId);
    
    // Update rumor evidence counts
    if (evidence.SupportsRumor)
        rumor.SupportingEvidenceCount++;
    else
        rumor.ContradictingEvidenceCount++;
    
    // Recalculate truth confidence
    await UpdateTruthConfidenceAsync(rumor);
    
    // Update beliefs of all who believe this rumor
    await UpdateBeliefsAfterEvidenceAsync(rumorId, evidence);
}

public async Task UpdateTruthConfidenceAsync(Rumor rumor)
{
    // Simple evidence-based confidence
    var supporting = rumor.SupportingEvidenceCount;
    var contradicting = rumor.ContradictingEvidenceCount;
    var total = supporting + contradicting;
    
    if (total == 0)
    {
        rumor.TruthConfidence = 0.5f; // Unknown
        rumor.TruthStatus = RumorTruthStatus.Unknown;
    }
    else
    {
        var ratio = (float)supporting / total;
        
        if (ratio > 0.7f)
        {
            rumor.TruthConfidence = ratio;
            rumor.TruthStatus = RumorTruthStatus.LikelyTrue;
        }
        else if (ratio < 0.3f)
        {
            rumor.TruthConfidence = 1 - ratio;
            rumor.TruthStatus = RumorTruthStatus.LikelyFalse;
        }
        else
        {
            rumor.TruthConfidence = 0.5f;
            rumor.TruthStatus = RumorTruthStatus.Uncertain;
        }
    }
    
    await _rumorRepo.UpdateAsync(rumor);
}
```

---

## 8. Truth Emergence

### How Rumors Get Confirmed or Debunked

Rumors can become confirmed or debunked through:

1. **Official Event** — An event proves/disproves the rumor
2. **Direct Action** — Subject's actions prove/disprove
3. **News Coverage** — News account verifies
4. **Player Confession** — Someone admits truth/fabrication

```csharp
public async Task ConfirmRumorAsync(Guid rumorId, Guid confirmedBy, string evidence)
{
    var rumor = await _rumorRepo.GetAsync(rumorId);
    
    rumor.TruthStatus = RumorTruthStatus.ConfirmedTrue;
    rumor.TruthConfidence = 1.0f;
    rumor.Status = RumorStatus.Confirmed;
    rumor.ConfirmedAt = DateTime.UtcNow;
    
    await _rumorRepo.UpdateAsync(rumor);
    
    // Create evidence of confirmation
    await AddEvidenceAsync(rumorId, new RumorEvidence
    {
        AccountId = confirmedBy,
        Type = EvidenceType.Verification,
        SupportsRumor = true,
        Description = evidence,
        Strength = EvidenceStrength.Definitive,
        IsVerified = true,
        VerifiedByAccountId = confirmedBy,
        VerifiedAt = DateTime.UtcNow
    });
    
    // Create event
    await _eventService.CreateEventAsync(new Event
    {
        Type = "News.RumorConfirmed",
        Title = $"Rumor confirmed: {rumor.Subject}",
        Description = evidence,
        RelatedAccountId = rumor.SubjectAccountId
    });
}
```

### Rumor Lifecycle

```csharp
public async Task ProcessRumorLifecycleAsync(Guid rumorId)
{
    var rumor = await _rumorRepo.GetAsync(rumorId);
    
    // Check if rumor should die (no engagement)
    if (rumor.Status == RumorStatus.Active)
    {
        var daysActive = (DateTime.UtcNow - rumor.OriginDate).Days;
        var mentionsPerDay = rumor.TotalMentions / Math.Max(1, daysActive);
        
        if (mentionsPerDay < 0.5 && daysActive > 7)
        {
            rumor.Status = RumorStatus.Died;
            rumor.DiedAt = DateTime.UtcNow;
            await _rumorRepo.UpdateAsync(rumor);
        }
    }
    
    // Check if rumor is stalling
    if (rumor.Status == RumorStatus.Active)
    {
        var recentMentions = await _rumorRepo.GetMentionsSinceAsync(
            rumorId, DateTime.UtcNow.AddDays(-1));
        
        if (recentMentions < 3)
        {
            rumor.Status = RumorStatus.Stalling;
            await _rumorRepo.UpdateAsync(rumor);
        }
    }
}
```

---

## 9. Rumor-Driven Posts

Posts can be about rumors:

```csharp
public async Task<Post> CreateRumorPostAsync(
    Guid accountId, 
    string content, 
    Guid? rumorId)
{
    var post = await _postService.CreatePostAsync(accountId, content);
    
    if (rumorId.HasValue)
    {
        post.RumorId = rumorId;
        await _postService.UpdateAsync(post);
        
        // Update rumor metrics
        var rumor = await _rumorRepo.GetAsync(rumorId.Value);
        rumor.PostCount++;
        rumor.TotalMentions++;
        await _rumorRepo.UpdateAsync(rumor);
        
        // Spread to followers
        await ProcessRumorSpreadAsync(rumorId.Value);
    }
    
    return post;
}
```

---

## 10. LLM Rumor Events

Use LLM to detect when rumors should emerge from events:

```text
SYSTEM: Analyze this event for potential rumors.

EVENT: {event description}
PARTICIPANTS: {list of accounts}
RELATIONSHIPS: {existing relationships}
RECENT POSTS: {recent activity}

Should this event generate a rumor? What would the rumor be?

{
  "generateRumor": true/false,
  "rumor": {
    "subject": "...",
    "description": "...",
    "type": "Relationship/Scandal/Gossip/etc",
    "originAccountId": "who would start it",
    "initialBelievers": ["account_ids"],
    "isActuallyTrue": true/false (keep secret)
  }
}
```

---

## 11. Rumor API Endpoints

### Get Active Rumors
```http
GET /api/rumors?status=Active&cursor={cursor}&pageSize={size}
```

### Get Rumor Details
```http
GET /api/rumors/{id}
```
Returns rumor with beliefs, evidence, and related posts.

### Get My Beliefs
```http
GET /api/accounts/{id}/beliefs
```
Returns what the account believes about various rumors.

### Get Rumor Evidence
```http
GET /api/rumors/{id}/evidence
```

### Get Rumors About Account
```http
GET /api/accounts/{id}/rumors
```
Returns active rumors about this account.

### Add Evidence
```http
POST /api/rumors/{id}/evidence
{
  "type": "Post",
  "supportsRumor": true,
  "description": "...",
  "relatedPostId": "..."
}
```

### Spread Rumor (implicit via post/repost)

---

## 12. Database Migration

### Rumors Table
```sql
CREATE TABLE Rumors (
    Id TEXT PRIMARY KEY,
    Subject TEXT NOT NULL,
    Description TEXT,
    SubjectAccountId TEXT NOT NULL,
    TruthStatus INTEGER NOT NULL DEFAULT 0,
    TruthConfidence REAL NOT NULL DEFAULT 0.5,
    OriginAccountId TEXT NOT NULL,
    OriginDate TEXT NOT NULL,
    OriginPostId TEXT,
    Type INTEGER NOT NULL,
    Topic TEXT,
    BelieverCount INTEGER NOT NULL DEFAULT 0,
    DoubterCount INTEGER NOT NULL DEFAULT 0,
    TotalMentions INTEGER NOT NULL DEFAULT 0,
    PostCount INTEGER NOT NULL DEFAULT 0,
    SupportingEvidenceCount INTEGER NOT NULL DEFAULT 0,
    ContradictingEvidenceCount INTEGER NOT NULL DEFAULT 0,
    Status INTEGER NOT NULL DEFAULT 0,
    ConfirmedAt TEXT,
    DebunkedAt TEXT,
    DiedAt TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE INDEX IX_Rumors_Status ON Rumors(Status);
CREATE INDEX IX_Rumors_SubjectAccountId ON Rumors(SubjectAccountId);
CREATE INDEX IX_Rumors_Topic ON Rumors(Topic);
CREATE INDEX IX_Rumors_TruthStatus ON Rumors(TruthStatus);
```

### AccountBeliefs Table
```sql
CREATE TABLE AccountBeliefs (
    Id TEXT PRIMARY KEY,
    AccountId TEXT NOT NULL,
    RumorId TEXT NOT NULL,
    Belief INTEGER NOT NULL,
    Confidence REAL NOT NULL DEFAULT 0.5,
    Source INTEGER NOT NULL,
    SourceAccountId TEXT,
    SourcePostId TEXT,
    ChangedMind INTEGER NOT NULL DEFAULT 0,
    PreviousBeliefDate TEXT,
    PreviousBelief INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    UNIQUE(AccountId, RumorId)
);

CREATE INDEX IX_AccountBeliefs_AccountId ON AccountBeliefs(AccountId);
CREATE INDEX IX_AccountBeliefs_RumorId ON AccountBeliefs(RumorId);
```

### RumorEvidence Table
```sql
CREATE TABLE RumorEvidence (
    Id TEXT PRIMARY KEY,
    RumorId TEXT NOT NULL,
    AccountId TEXT NOT NULL,
    Type INTEGER NOT NULL,
    SupportsRumor INTEGER NOT NULL,
    Description TEXT,
    RelatedPostId TEXT,
    RelatedEventId TEXT,
    Credibility REAL NOT NULL DEFAULT 0.5,
    Strength INTEGER NOT NULL DEFAULT 1,
    IsVerified INTEGER NOT NULL DEFAULT 0,
    VerifiedByAccountId TEXT,
    VerifiedAt TEXT,
    CreatedAt TEXT NOT NULL
);

CREATE INDEX IX_RumorEvidence_RumorId ON RumorEvidence(RumorId);
CREATE INDEX IX_RumorEvidence_SupportsRumor ON RumorEvidence(SupportsRumor);
```

---

## 13. Tests

### Rumor Tests
```text
Rumors can be created
Rumor truth status updates with evidence
Rumor dies when engagement drops
Rumor confirms/debunks correctly
```

### Belief Tests
```text
Beliefs form correctly from spread
Source credibility affects belief
Relationship affects belief
Personality affects belief
Beliefs update with new evidence
```

### Spread Tests
```text
Rumors spread to connected accounts
Gossip tendency increases spread
Topic interest increases spread
Trended rumors spread faster
Rumors don't spread to everyone
```

### Evidence Tests
```text
Evidence updates truth confidence
Evidence updates beliefs
Strong evidence has more impact
Verified evidence has more impact
```

### API Tests
```text
Rumor endpoints return correct data
Belief endpoints work
Evidence endpoints work
```

### Regression Tests
```text
Existing Parts 01-20 tests still pass
```

---

## 14. Android

Part 21 is backend-only. Minimal model adjustments only.

---

## 15. README — REQUIRED

Document:
- Part 21 completion
- Rumor entity structure
- Belief system
- Evidence system
- Rumor spread mechanics
- Truth emergence
- Rumor lifecycle
- API endpoints
- Database changes
- Tests performed
- Current status
- Next planned part

---

## 16. Git

After implementation:
1. Inspect `git status`
2. Commit: `Implement rumors system (Part 21)`
3. Push to `origin/main`
4. Verify against origin

---

## 17. DO NOT IMPLEMENT YET

- News accounts covering rumors (Part 22)
- Fact-checking tools
- Moderation of rumors
- Deep investigation
- Paid disinformation

---

## 18. QUALITY REQUIREMENTS

- Correct (beliefs calculate accurately)
- Performant (batch rumor processing)
- Testable
- Permanent (all records persist)
- Realistic (rumors behave like real gossip)

---

## 19. FINAL VERIFICATION

```text
Server builds
Rumors created from events
Beliefs form correctly
Rumors spread through network
Evidence updates truth
Rumors can confirm/debunk
Rumor lifecycle works
Rumor API returns data
Database migrations applied
Existing tests pass
README updated
Git commit pushed
Working tree clean
```

---

## 20. FINAL SESSION REPORT

```text
# PART 21 — COMPLETE

## 1. What Was Inspected
...

## 2. What Already Existed
...

## 3. What Changed
...

## 4. Rumor Architecture
...

## 5. Belief System
...

## 6. Evidence System
...

## 7. Rumor Spread Mechanics
...

## 8. Truth Emergence
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
01A-21 COMPLETE

## 15. Intentionally Not Implemented
- News accounts (Part 22)
- Fact-checking
- Moderation tools

## 16. NEXT
NEXT: PART 22 — News
```

**STOP after completing Part 21 and reporting the session log.**
