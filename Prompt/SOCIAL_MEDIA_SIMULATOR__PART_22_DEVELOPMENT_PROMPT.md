# SOCIAL MEDIA SIMULATOR — PART 22 DEVELOPMENT PROMPT
## RUMORS & MISINFORMATION

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
02   Backend Architecture           COMPLETE
03   Persistence                     COMPLETE
04   Accounts & Authentication       COMPLETE
05   Social Graph                    COMPLETE
06   Posts & Engagement              COMPLETE
07   Feed & Timeline                 COMPLETE
08   NPC Simulator Foundation       COMPLETE
09   NPC Population Generation       COMPLETE
10   NPC Behavior Simulation         COMPLETE
11   NPC Background Simulation       COMPLETE
12   NPC Social Graph                COMPLETE
13   AI Content Generation           COMPLETE
14   Notifications System            COMPLETE
15   Communities                     COMPLETE
16   Advanced Feed                   COMPLETE
17   LLM-Driven Event System        COMPLETE
18   Event Causality & Offline Sim   COMPLETE
19   Virality                        COMPLETE
20   Topics & Trends                 COMPLETE
21   Deployment & Testing            COMPLETE
```

Latest commit:

```text
3499a09 — Part 21: Deployment & Testing - Make it runnable
```

Remote:

```text
origin/main
```

Repository:

```text
https://github.com/AimJax/SMS.git
```

Working tree should currently be clean. Run `git status` and `git fetch` as your first action to confirm nothing has drifted since Part 21.

---

# 1. WHY THIS PART, NOW

Parts 01–21 built a social media platform with trends, virality, communities, events, and news accounts. The platform has organic content spreading, trending topics, and even news coverage — but there's no **uncertain information spreading**. Every post is treated as fact. Every rumor is believed instantly. No one questions anything.

Part 22 introduces **Rumors & Misinformation** — the critical social phenomenon where information spreads with uncertain truth value. This adds the messiness and drama of real social media:

- NPCs can gossip and spread unverified claims
- Accounts can have beliefs that don't match reality
- Contradicting evidence can emerge
- Some accounts spread rumors deliberately
- Rumors can be confirmed or debunked
- The truth eventually emerges (or doesn't)

Without rumors, the platform feels sterile and unrealistic. With rumors:
- Social drama emerges naturally
- NPCs have motivations beyond generic posting
- Information ecosystems form
- Echo chambers strengthen around rumors
- News accounts have stories to cover

Rumors are foundational to:
- News coverage (Part 22+) — news reports on rumors
- Reputation damage — rumors affect accounts
- Social dynamics — rumors create drama
- Echo chambers — rumors reinforce beliefs

---

# 2. THE EXISTING PROJECT

The existing backend contains from Parts 01–21:

- Everything from Part 21 and earlier
- **News Accounts (Part 22):** News outlets covering topics
- **Trends (Part 20):** Trending topics tracked
- **Virality (Part 19):** Posts can go viral
- **Events (Part 17):** World events detected
- **Communities (Part 15):** Grouped interests
- **NPCs (Parts 10-13):** NPCs with personalities including GossipTendency, DramaTendency
- **Posts, Comments, Engagement (Parts 06-07):** Content system

The infrastructure exists:
- Posts and comments ready to carry rumor content
- NPC personalities designed for gossip behavior
- Trends that rumors can attach to
- Communities where rumors spread
- News accounts ready to fact-check

Part 22 adds the rumor concept: information with uncertain truth value that spreads through the network.

---

# 3. MASTER ARCHITECTURE PRINCIPLES

## Server Authoritative

Rumors are managed by the server. The server tracks what information exists, how it spreads, and what people believe. The server never "auto-corrects" misinformation — it only provides mechanisms for truth to emerge.

## C# + LLM Hybrid

- C# manages rumor state, spread mechanics, and belief calculations
- LLM generates rumor content and assesses evidence plausibility
- Server validates all rumor-related actions

## Permanent Data Rule

All rumors, beliefs, evidence, and contradictions must NOT be automatically deleted/pruned. Even debunked rumors remain in history.

## Core Concept: Information ≠ Fact

```
Information exists
        ↓
Spreads through network
        ↓
Becomes belief (different levels)
        ↓
Evidence accumulates
        ↓
Truth emerges (or doesn't)
        ↓
Some believe, some doubt
        ↓
Rumor persists or dies
```

---

# PART 22 OBJECTIVE

Implement a **Rumors & Misinformation System**:

1. **Rumor Entity** — Information with truth status
2. **AccountBelief Entity** — What each account believes
3. **RumorEvidence Entity** — Supporting and contradicting evidence
4. **Rumor Creation** — How rumors originate
5. **Belief System** — How beliefs form and change
6. **Rumor Spread** — How rumors propagate
7. **Truth Emergence** — How rumors get confirmed or debunked
8. **Rumor-Driven Posts** — Posts about rumors
9. **NPC Rumor Behavior** — NPCs gossip intentionally
10. **News-Rumor Integration** — News covers rumors
11. **Rumor API** — Endpoints for rumor management

Do NOT implement in this part:
- Deep investigation mechanics
- Paid disinformation campaigns
- Moderation or removal of rumors
- Platform-wide fact-checks

---

# PART 22 — REQUIRED FEATURES

## 1. Rumor Entity

Create a `Rumor` entity:

```csharp
public class Rumor
{
    public Guid Id { get; set; }
    
    // Content
    public string Subject { get; set; }                  // "Kevin is dating Sarah"
    public string Description { get; set; }              // Longer description
    public Guid SubjectAccountId { get; set; }           // Who the rumor is about
    
    // Truth Status
    public RumorTruthStatus TruthStatus { get; set; }    // See enum
    public float TruthConfidence { get; set; }           // 0.0 - 1.0 (system's estimate)
    
    // Origin
    public Guid OriginAccountId { get; set; }            // Who started the rumor
    public DateTime OriginDate { get; set; }
    public Guid? OriginPostId { get; set; }             // Post that started it
    
    // Rumor Type
    public RumorType Type { get; set; }                  // See enum
    public string Topic { get; set; }                    // Related topic
    
    // Metrics
    public int BelieverCount { get; set; }              // How many believe it
    public int DoubterCount { get; set; }               // How many doubt it
    public int TotalMentions { get; set; }              // How many times mentioned
    public int PostCount { get; set; }                  // Posts about this rumor
    
    // Evidence
    public int SupportingEvidenceCount { get; set; }
    public int ContradictingEvidenceCount { get; set; }
    
    // Lifecycle
    public RumorStatus Status { get; set; }              // See enum
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? DebunkedAt { get; set; }
    public DateTime? DiedAt { get; set; }
    
    // Intentional vs Organic
    public bool IsPlant { get; set; }                   // Deliberately spread?
    public Guid? PlantingAccountId { get; set; }        // Who planted it
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum RumorTruthStatus
{
    Unknown = 0,           // Truth value unknown
    LikelyTrue = 1,       // Evidence suggests true
    LikelyFalse = 2,      // Evidence suggests false
    ConfirmedTrue = 3,     // Officially confirmed true
    ConfirmedFalse = 4,    // Officially confirmed false
    Unverifiable = 5      // Can never be verified
}

public enum RumorType
{
    Gossip,                // Social gossip ("dating", "friend breakup")
    Scandal,              // Negative rumor ("cheating", "lying")
    Achievement,           // Positive rumor ("got a job", "won award")
    Relationship,          // Relationship changes
    Professional,         // Career-related
    Personal,             // Personal life
    Conspiracy,           // Complex/layered rumor
    Hoax                  // Deliberate fake
}

public enum RumorStatus
{
    Active = 0,            // Spreading
    Stalling = 1,          // Interest declining
    Confirmed = 2,         // Proven true
    Debunked = 3,          // Proven false
    Died = 4               // Faded away
}
```

---

## 2. AccountBelief Entity

Track what each account believes about rumors:

```csharp
public class AccountBelief
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid RumorId { get; set; }
    
    // Belief state
    public BeliefLevel Belief { get; set; }             // See enum
    public float Confidence { get; set; }                // 0.0 - 1.0
    
    // How they formed this belief
    public BeliefSource Source { get; set; }             // See enum
    public Guid? SourceAccountId { get; set; }           // Who told them
    public Guid? SourcePostId { get; set; }              // Post they saw
    
    // History
    public bool ChangedMind { get; set; }               // Did they change their belief?
    public DateTime? PreviousBeliefDate { get; set; }
    public BeliefLevel? PreviousBelief { get; set; }
    
    // Engagement
    public int TimesShared { get; set; }
    public int TimesCommented { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum BeliefLevel
{
    StronglyBelieves = 5,  // 100% sure true
    Believes = 4,         // Probably true
    Uncertain = 3,         // Could go either way
    Doubts = 2,           // Probably false
    StronglyDoubts = 1,   // 100% sure false
    Unaware = 0           // Doesn't know about rumor
}

public enum BeliefSource
{
    DirectWitness,        // Saw it happen
    TrustedFriend,        // Heard from trusted friend
    CasualContact,        // Heard from casual contact
    Post,                // Saw a post
    Comment,             // Read a comment
    Event,               // Saw it in an event
    Trend,               // Saw it trending
    News,                // Saw it from news account
    FactCheck            // Official fact-check
}
```

---

## 3. RumorEvidence Entity

Track evidence for and against rumors:

```csharp
public class RumorEvidence
{
    public Guid Id { get; set; }
    public Guid RumorId { get; set; }
    public Guid AccountId { get; set; }                  // Who provided evidence
    
    // Evidence type
    public EvidenceType Type { get; set; }               // See enum
    public bool SupportsRumor { get; set; }              // True = supports, False = contradicts
    
    // Content
    public string Description { get; set; }
    public Guid? RelatedPostId { get; set; }             // Post with evidence
    public Guid? RelatedEventId { get; set; }            // Event as evidence
    public Guid? RelatedAccountId { get; set; }          // Account testimony
    
    // Credibility
    public float Credibility { get; set; }               // 0.0 - 1.0
    public EvidenceStrength Strength { get; set; }       // See enum
    
    // Verification
    public bool IsVerified { get; set; }                 // Has it been checked?
    public Guid? VerifiedByAccountId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public enum EvidenceType
{
    Post,                 // A post contains evidence
    Comment,              // A comment provides evidence
    Photo,                // Photo evidence
    Screenshot,           // Screenshot evidence
    Testimony,            // Someone's statement
    Event,                // An event proves/disproves
    Contradiction,        // Contradicts other evidence
    Verification          // Official verification
}

public enum EvidenceStrength
{
    Weak = 1,             // Circumstantial
    Moderate = 2,         // Suggestive
    Strong = 3,           // Compelling
    Definitive = 4       // Proves/disproves
}
```

---

## 4. Rumor Service

### IRumorService

```csharp
public interface IRumorService
{
    // Creation
    Task<Rumor> CreateRumorAsync(RumorCreationRequest request);
    Task<Rumor> CreateRumorFromPostAsync(Guid postId, Guid spreadingAccountId);
    
    // Queries
    Task<Rumor?> GetRumorAsync(Guid rumorId);
    Task<List<Rumor>> GetActiveRumorsAsync(int count = 20);
    Task<List<Rumor>> GetRumorsByTopicAsync(string topic, int count = 20);
    Task<List<Rumor>> GetRumorsAboutAccountAsync(Guid accountId);
    
    // Beliefs
    Task<AccountBelief?> GetBeliefAsync(Guid accountId, Guid rumorId);
    Task<List<AccountBelief>> GetAccountBeliefsAsync(Guid accountId);
    Task<AccountBelief> UpdateBeliefAsync(Guid accountId, Guid rumorId, BeliefLevel belief);
    
    // Evidence
    Task<RumorEvidence> AddEvidenceAsync(Guid rumorId, AddEvidenceRequest request);
    Task<List<RumorEvidence>> GetEvidenceAsync(Guid rumorId);
    
    // Truth
    Task ConfirmRumorAsync(Guid rumorId, Guid confirmedBy, string evidence);
    Task DebunkRumorAsync(Guid rumorId, Guid debunkedBy, string evidence);
    
    // Processing
    Task ProcessRumorsTickAsync();
}
```

---

## 5. Rumor Creation

### How Rumors Start

Rumors can originate from:

1. **NPC-initiated** — NPCs with high GossipTendency spread rumors
2. **Event-driven** — Events trigger rumors about participants
3. **Post-based** — A viral/engaging post becomes rumor content
4. **Deliberate plants** — NPCs deliberately spread false information
5. **LLM-generated** — LLM proposes rumors based on social context

### Rumor Creation from NPC Behavior

```csharp
public async Task<Rumor?> TryCreateRumorAsync(Guid npcId)
{
    var npc = await _npcService.GetAsync(npcId);
    
    // Only gossipy NPCs spread rumors
    if (npc.Personality.GossipTendency < 0.3) return null;
    
    // Probability based on gossip tendency
    var createProb = npc.Personality.GossipTendency * 0.01; // Max 1% per tick
    if (Random.NextDouble() > createProb) return null;
    
    // Find a subject (other account)
    var potentialSubjects = await _accountService.GetAccountsForGossipAsync(npcId);
    if (!potentialSubjects.Any()) return null;
    
    var subject = potentialSubjects.RandomElement();
    
    // Generate rumor content with LLM
    var rumorContent = await GenerateRumorContentAsync(npc, subject);
    
    var rumor = new Rumor
    {
        Subject = rumorContent.Subject,
        Description = rumorContent.Description,
        SubjectAccountId = subject.Id,
        Type = rumorContent.Type,
        Topic = rumorContent.Topic,
        TruthStatus = RumorTruthStatus.Unknown,
        TruthConfidence = 0.5f,
        OriginAccountId = npcId,
        OriginDate = DateTime.UtcNow,
        IsPlant = false,
        Status = RumorStatus.Active,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    await _rumorRepo.CreateAsync(rumor);
    
    return rumor;
}

public async Task<RumorContent> GenerateRumorContentAsync(Npc npc, Account subject)
{
    var prompt = $@"You are generating a rumor that NPC '{npc.DisplayName}' might spread about '{subject.DisplayName}'.

NPC Personality: {npc.Personality}
NPC Interests: {string.Join(", ", npc.Interests.Select(i => i.Name))}
Subject Profile: {subject.DisplayName} - {subject.Bio}

Generate a plausible rumor that:
- Involves the subject account
- Is something a gossipy person might share
- Could be true or false (keep actual truth secret)
- Is dramatic enough to spread

Return as JSON:
{{
  "subject": "What the rumor is about",
  "description": "The rumor content",
  "type": "Gossip/Scandal/Relationship/etc",
  "topic": "Related topic"
}}";

    var result = await _aiService.GenerateTextAsync(prompt);
    
    if (!result.Success) return GenerateTemplateRumor(npc, subject);
    
    return ParseRumorContent(result.Text);
}
```

---

## 6. Belief System

### How Beliefs Form

Accounts form beliefs based on:

1. **Source Credibility** — Who told them
2. **Network Position** — Friends vs strangers
3. **Prior Beliefs** — Consistent with existing views
4. **Evidence** — Supporting or contradicting evidence
5. **Account Traits** — Credulity vs skepticism

### Belief Calculation

```csharp
public float CalculateInitialBelief(Guid accountId, Rumor rumor, BeliefSource source, Guid sourceAccountId)
{
    var baseBelief = 0.5f; // Start neutral
    
    // Source credibility modifier
    var sourceCredibility = GetSourceCredibility(source);
    baseBelief += sourceCredibility;
    
    // Relationship with source (trust)
    var relationship = _socialGraphService.GetRelationship(accountId, sourceAccountId);
    var trustModifier = (relationship?.Trust ?? 50) / 200.0; // -0.25 to +0.25
    baseBelief += trustModifier;
    
    // Account personality
    var account = _accountService.Get(accountId);
    if (account.NpcProfile?.Personality.Credulity > 0.7) baseBelief += 0.1f;
    if (account.NpcProfile?.Personality.Skepticism > 0.7) baseBelief -= 0.1f;
    
    // Topic relevance (care more about things they care about)
    if (account.Interests.Contains(rumor.Topic)) baseBelief += 0.05f;
    
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

public double GetSourceCredibility(BeliefSource source)
{
    return source switch
    {
        BeliefSource.DirectWitness => 0.3,     // +30% if you saw it
        BeliefSource.TrustedFriend => 0.2,     // +20% from friend
        BeliefSource.News => 0.15,            // +15% from news
        BeliefSource.Post => 0.0,             // Neutral from post
        BeliefSource.Trend => -0.05,          // -5% from trend
        _ => 0.0
    };
}
```

---

## 7. Rumor Spread Mechanics

### How Rumors Propagate

```csharp
public async Task ProcessRumorSpreadAsync(Guid rumorId)
{
    var rumor = await _rumorRepo.GetAsync(rumorId);
    if (rumor.Status != RumorStatus.Active) return;
    
    // Get accounts who might spread this rumor
    var potentialSpreaders = await GetPotentialSpreadersAsync(rumorId);
    
    foreach (var accountId in potentialSpreaders)
    {
        var spreadProb = CalculateSpreadProbabilityAsync(accountId, rumor);
        
        if (Random.NextDouble() < spreadProb)
        {
            // Account becomes aware and forms belief
            var beliefValue = CalculateInitialBelief(accountId, rumor, BeliefSource.Post, rumor.OriginAccountId);
            var beliefLevel = CalculateBeliefLevel(beliefValue);
            
            await CreateBeliefAsync(accountId, rumorId, beliefLevel, BeliefSource.Post);
            
            // Update rumor metrics
            rumor.TotalMentions++;
            if (beliefLevel >= BeliefLevel.Believes)
                rumor.BelieverCount++;
            else if (beliefLevel <= BeliefLevel.Doubts)
                rumor.DoubterCount++;
            
            // Account might reshare
            if (beliefLevel >= BeliefLevel.Believes && Random.NextDouble() < 0.3)
            {
                await _postService.CreateRumorPostAsync(accountId, rumor);
            }
        }
    }
    
    await _rumorRepo.UpdateAsync(rumor);
}

public async Task<double> CalculateSpreadProbabilityAsync(Guid accountId, Rumor rumor)
{
    var account = await _accountService.GetAsync(accountId);
    
    // Base probability
    var baseProb = 0.02; // 2% base
    
    // Gossip tendency - higher = more likely to spread
    var gossipBoost = (account.NpcProfile?.Personality.GossipTendency ?? 0) / 100.0;
    
    // Drama tendency - dramatic rumors spread more
    var dramaBoost = (account.NpcProfile?.Personality.DramaTendency ?? 0) / 200.0;
    
    // Connected to subject - more likely to care
    var connectedBoost = await _socialGraphService.IsConnectedToAsync(accountId, rumor.SubjectAccountId) ? 0.05 : 0;
    
    // Topic interest
    var topicBoost = account.Interests.Contains(rumor.Topic) ? 0.03 : 0;
    
    // Trending boost
    var trendBoost = await _trendService.IsTrendingAsync(rumor.Topic) ? 0.1 : 0;
    
    // Already believes - more likely to spread
    var belief = await GetBeliefAsync(accountId, rumor.Id);
    var beliefBoost = belief?.Belief switch
    {
        BeliefLevel.StronglyBelieves => 0.2,
        BeliefLevel.Believes => 0.1,
        BeliefLevel.Uncertain => 0.0,
        _ => -0.1
    };
    
    return Math.Min(0.9, baseProb + gossipBoost + dramaBoost + connectedBoost + topicBoost + trendBoost + beliefBoost);
}
```

---

## 8. Evidence and Truth Emergence

### Adding Evidence

```csharp
public async Task<RumorEvidence> AddEvidenceAsync(Guid rumorId, AddEvidenceRequest request)
{
    var evidence = new RumorEvidence
    {
        RumorId = rumorId,
        AccountId = request.AccountId,
        Type = request.Type,
        SupportsRumor = request.SupportsRumor,
        Description = request.Description,
        RelatedPostId = request.RelatedPostId,
        RelatedEventId = request.RelatedEventId,
        RelatedAccountId = request.RelatedAccountId,
        Strength = request.Strength,
        Credibility = CalculateEvidenceCredibility(request),
        CreatedAt = DateTime.UtcNow
    };
    
    await _evidenceRepo.CreateAsync(evidence);
    
    // Update rumor
    var rumor = await _rumorRepo.GetAsync(rumorId);
    if (request.SupportsRumor)
        rumor.SupportingEvidenceCount++;
    else
        rumor.ContradictingEvidenceCount++;
    
    await UpdateTruthConfidenceAsync(rumor);
    await UpdateBeliefsAfterEvidenceAsync(rumorId, evidence);
    await _rumorRepo.UpdateAsync(rumor);
    
    return evidence;
}
```

### Truth Confidence Calculation

```csharp
public async Task UpdateTruthConfidenceAsync(Rumor rumor)
{
    var supporting = rumor.SupportingEvidenceCount;
    var contradicting = rumor.ContradictingEvidenceCount;
    var total = supporting + contradicting;
    
    if (total == 0)
    {
        rumor.TruthConfidence = 0.5f;
        rumor.TruthStatus = RumorTruthStatus.Unknown;
        return;
    }
    
    // Weight by evidence strength (simplified)
    var supportingWeight = supporting * 1.0;
    var contradictingWeight = contradicting * 1.0;
    var ratio = supportingWeight / (supportingWeight + contradictingWeight);
    
    if (ratio > 0.7f && total >= 3)
    {
        rumor.TruthConfidence = (ratio + 0.5f) / 2;
        rumor.TruthStatus = RumorTruthStatus.LikelyTrue;
    }
    else if (ratio < 0.3f && total >= 3)
    {
        rumor.TruthConfidence = (1 - ratio + 0.5f) / 2;
        rumor.TruthStatus = RumorTruthStatus.LikelyFalse;
    }
    else if (total >= 5)
    {
        rumor.TruthStatus = ratio > 0.5 ? RumorTruthStatus.LikelyTrue : RumorTruthStatus.LikelyFalse;
        rumor.TruthConfidence = 0.6f;
    }
    else
    {
        rumor.TruthConfidence = 0.5f;
        rumor.TruthStatus = RumorTruthStatus.Unknown;
    }
    
    await _rumorRepo.UpdateAsync(rumor);
}
```

---

## 9. Rumor Lifecycle Processing

### Tick Processing

```csharp
public async Task ProcessRumorsTickAsync()
{
    var activeRumors = await _rumorRepo.GetActiveAsync();
    
    foreach (var rumor in activeRumors)
    {
        // 1. Spread to new accounts
        await ProcessRumorSpreadAsync(rumor.Id);
        
        // 2. Check for lifecycle transitions
        await ProcessRumorLifecycleAsync(rumor);
        
        // 3. Update trends (rumors can trend)
        await UpdateRumorTrendsAsync(rumor);
    }
}
```

### Lifecycle Transitions

```csharp
public async Task ProcessRumorLifecycleAsync(Rumor rumor)
{
    // Stalling check
    if (rumor.Status == RumorStatus.Active)
    {
        var recentMentions = await _rumorRepo.GetMentionsSinceAsync(rumor.Id, DateTime.UtcNow.AddDays(-1));
        
        if (recentMentions < 3)
        {
            rumor.Status = RumorStatus.Stalling;
            await _rumorRepo.UpdateAsync(rumor);
        }
    }
    
    // Death check
    if (rumor.Status == RumorStatus.Stalling)
    {
        var recentMentions = await _rumorRepo.GetMentionsSinceAsync(rumor.Id, DateTime.UtcNow.AddDays(-2));
        
        if (recentMentions < 2)
        {
            rumor.Status = RumorStatus.Died;
            rumor.DiedAt = DateTime.UtcNow;
            await _rumorRepo.UpdateAsync(rumor);
        }
    }
}
```

---

## 10. NPC Deliberate Rumor Plants

Some NPCs deliberately spread false information:

```csharp
public async Task ProcessRumorPlantsAsync()
{
    var manipulators = await _npcService.GetByTraitAsync("ManipulationTendency", min: 0.6);
    
    foreach (var npc in manipulators)
    {
        // High manipulation NPCs occasionally plant rumors
        if (Random.NextDouble() < npc.Personality.ManipulationTendency * 0.005)
        {
            await PlantRumorAsync(npc);
        }
    }
}

public async Task PlantRumorAsync(Guid npcId)
{
    var npc = await _npcService.GetAsync(npcId);
    
    // Find a target (rival, enemy, or random)
    var target = await _accountService.GetRumorTargetAsync(npcId);
    if (target == null) return;
    
    // Create deliberately false rumor
    var rumor = new Rumor
    {
        Subject = $"{target.DisplayName} did something bad",
        Description = "A potentially false accusation",
        SubjectAccountId = target.Id,
        TruthStatus = RumorTruthStatus.Unknown,
        TruthConfidence = 0.0f, // Deliberately false
        OriginAccountId = npcId,
        IsPlant = true,
        PlantingAccountId = npcId,
        Status = RumorStatus.Active,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    await _rumorRepo.CreateAsync(rumor);
    
    // Immediately spread to a few accounts
    var initialSpreaders = await _accountService.GetConnectedAccountsAsync(npcId, count: 5);
    foreach (var spreader in initialSpreaders)
    {
        await CreateBeliefAsync(spreader.Id, rumor.Id, BeliefLevel.Believes, BeliefSource.CasualContact);
    }
}
```

---

## 11. News-Rumor Integration

News accounts can cover rumors:

```csharp
public async Task ProcessNewsRumorCoverageAsync()
{
    // Find rumors that need coverage
    var hotRumors = await _rumorRepo.GetActiveAsync()
        .Where(r => r.BelieverCount >= 10 && r.PostCount >= 5)
        .OrderByDescending(r => r.BelieverCount)
        .Take(5)
        .ToListAsync();
    
    foreach (var rumor in hotRumors)
    {
        // Check if already covered
        if (await _newsService.HasFactCheckAsync(rumor.Id)) continue;
        
        // Assign to appropriate news account
        var newsAccount = await _newsService.GetBestMatchAsync(rumor.Topic);
        if (newsAccount == null) continue;
        
        // Generate fact-check article
        await _newsService.GenerateFactCheckAsync(newsAccount.Id, rumor);
    }
}
```

---

## 12. Rumor API Endpoints

### Get Active Rumors
```http
GET /api/rumors?cursor={cursor}&pageSize={size}
```

### Get Rumor Details
```http
GET /api/rumors/{id}
```
Returns rumor with beliefs, evidence, and related posts.

### Get My Beliefs
```http
GET /api/me/beliefs
```
Returns current account's beliefs about rumors.

### Get Beliefs for Account
```http
GET /api/accounts/{id}/beliefs
```

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
  "accountId": "...",
  "type": "Post",
  "supportsRumor": true,
  "description": "...",
  "relatedPostId": "..."
}
```

### Confirm/Debunk Rumor
```http
POST /api/rumors/{id}/confirm
{
  "confirmedBy": "...",
  "evidence": "..."
}

POST /api/rumors/{id}/debunk
{
  "debunkedBy": "...",
  "evidence": "..."
}
```

---

## 13. Database Migration

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
    IsPlant INTEGER NOT NULL DEFAULT 0,
    PlantingAccountId TEXT,
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
CREATE INDEX IX_Rumors_IsPlant ON Rumors(IsPlant);
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
    TimesShared INTEGER NOT NULL DEFAULT 0,
    TimesCommented INTEGER NOT NULL DEFAULT 0,
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
    RelatedAccountId TEXT,
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

## 14. Tests

### Rumor Tests
```text
Rumors can be created
Rumor truth status updates with evidence
Rumor dies when engagement drops
Rumor confirms/debunks correctly
NPC plants are created correctly
```

### Belief Tests
```text
Beliefs form correctly from spread
Source credibility affects belief
Relationship affects belief
Personality affects belief
Beliefs update with new evidence
Beliefs persist correctly
```

### Spread Tests
```text
Rumors spread to connected accounts
Gossip tendency increases spread
Topic interest increases spread
Trended rumors spread faster
Plant rumors spread to initial targets
```

### Evidence Tests
```text
Evidence updates truth confidence
Evidence updates beliefs
Strong evidence has more impact
Verified evidence has more impact
```

### Lifecycle Tests
```text
Active rumors become stalling
Stalling rumors die
Confirmed rumors update status
Debunked rumors update status
```

### API Tests
```text
Rumor endpoints return correct data
Belief endpoints work
Evidence endpoints work
```

### Regression Tests
```text
Existing Parts 01-21 tests still pass
```

---

## 15. Android

Part 22 is backend-only. Minimal model adjustments only if needed.

---

## 16. README — REQUIRED

Document:
- Part 22 completion
- Rumor entity structure
- Belief system
- Evidence system
- Rumor spread mechanics
- Truth emergence
- Rumor lifecycle
- NPC rumor plants
- News-rumor integration
- API endpoints
- Database changes
- Tests performed
- Current status
- Next planned part

---

## 17. Git

After implementation:
1. Inspect `git status`
2. Commit: `Implement rumors and misinformation system (Part 22)`
3. Push to `origin/main`
4. Verify against origin

---

## 18. DO NOT IMPLEMENT YET

- Deep investigation mechanics
- Paid disinformation campaigns
- Moderation or removal of rumors
- Platform-wide fact-checks
- Rumor reporting flags

---

## 19. QUALITY REQUIREMENTS

- Correct (beliefs calculate accurately)
- Performant (batch rumor processing)
- Testable
- Permanent (all records persist)
- Realistic (rumors behave like real gossip)

---

## 20. FINAL VERIFICATION

```text
Server builds
Rumors created from events and NPC behavior
Beliefs form correctly
Rumors spread through network
Evidence updates truth
Rumors can confirm/debunk
Rumor lifecycle works
NPC rumor plants work
News-rumor integration works
Rumor API returns data
Database migrations applied
Existing tests pass
README updated
Git commit pushed
Working tree clean
```

---

## 21. FINAL SESSION REPORT

```text
# PART 22 — COMPLETE

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

## 9. NPC Rumor Plants
...

## 10. News-Rumor Integration
...

## 11. API Endpoints
...

## 12. Database Changes
...

## 13. Tests
...

## 14. README
Updated: YES
...

## 15. Git
Commit: ...
Push: ...
Verified: YES
Working tree: clean

## 16. Current Project Status
01A-22 COMPLETE

## 17. Intentionally Not Implemented
- Deep investigation
- Paid disinformation
- Moderation
- Platform fact-checks

## 18. NEXT
NEXT: PART 23 — Permanent Memory
```

**STOP after completing Part 22 and reporting the session log.**
