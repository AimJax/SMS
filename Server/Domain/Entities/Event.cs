using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Main event types for the LLM-driven event system
/// </summary>
public enum EventType
{
    // Drama events
    Drama = 1000,
    JealousyIncident = 1001,
    PublicArgument = 1002,
    Betrayal = 1003,
    RedemptionArc = 1004,
    ComebackStory = 1005,
    DownfallStory = 1006,
    
    // Romance events
    Romance = 2000,
    NewRelationship = 2001,
    Breakup = 2002,
    LoveTriangle = 2003,
    SecretRelationship = 2004,
    RelationshipMilestone = 2005,
    Reconciliation = 2006,
    
    // Social events
    Social = 3000,
    NewFriendship = 3001,
    FriendshipEnded = 3002,
    Alliance = 3003,
    Rivalry = 3004,
    FanWar = 3005,
    TrollAttack = 3006,
    
    // Fame events
    Fame = 4000,
    RiseToFame = 4001,
    FallFromGrace = 4002,
    Scandal = 4003,
    Apology = 4004,
    Comeback = 4005,
    Cancellation = 4006,
    
    // Community events
    Community = 5000,
    CommunityDriven = 5001,
    CommunitySplit = 5002,
    CommunityMilestone = 5003,
    CommunityDrama = 5004,
    
    // Content events
    Content = 6000,
    ViralPost = 6001,
    ViralComment = 6002,
    QuotePostDrama = 6003,
    PollControversy = 6004,
    
    // Trend events
    Trend = 7000,
    TrendStart = 7001,
    TrendPivot = 7002,
    TrendDeath = 7003,
    
    // News events
    News = 8000,
    NewsCoverage = 8001,
    BreakingNews = 8002,
    NewsDebate = 8003
}

/// <summary>
/// Event lifecycle status
/// </summary>
public enum EventStatus
{
    Proposed = 0,
    Approved = 1,
    Active = 2,
    Ended = 3,
    Rejected = 4,
    Cancelled = 5
}

/// <summary>
/// Event visibility level
/// </summary>
public enum EventVisibility
{
    Public = 0,
    FollowersOnly = 1,
    CommunityOnly = 2,
    Private = 3
}

/// <summary>
/// Participant role in an event
/// </summary>
public enum ParticipantRole
{
    Protagonist = 0,
    Antagonist = 1,
    Supporter = 2,
    Victim = 3,
    Bystander = 4,
    Narrator = 5
}

/// <summary>
/// Participation status in an event
/// </summary>
public enum ParticipationStatus
{
    Active = 0,
    Withdrew = 1,
    WasRemoved = 2,
    Completed = 3
}

/// <summary>
/// Main event entity for LLM-driven narrative events
/// </summary>
public class Event
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable internal identifier
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Event type from the EventType enum
    /// </summary>
    public EventType Type { get; set; }
    
    /// <summary>
    /// LLM-generated dramatic title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// LLM-generated narrative description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// LLM's reasoning for why this event happened
    /// </summary>
    public string NarrativeContext { get; set; } = string.Empty;
    
    /// <summary>
    /// Account that proposed/initiated this event (null for system events)
    /// </summary>
    public int? CreatorAccountId { get; set; }
    
    /// <summary>
    /// When the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the event officially starts
    /// </summary>
    public DateTime StartAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the event ends (null if still active)
    /// </summary>
    public DateTime? EndAt { get; set; }
    
    /// <summary>
    /// Current event status
    /// </summary>
    public EventStatus Status { get; set; } = EventStatus.Proposed;
    
    /// <summary>
    /// Visibility level
    /// </summary>
    public EventVisibility Visibility { get; set; } = EventVisibility.Public;
    
    /// <summary>
    /// Primary topic tag
    /// </summary>
    [MaxLength(100)]
    public string? Topic { get; set; }
    
    /// <summary>
    /// Associated community (if applicable)
    /// </summary>
    public int? CommunityId { get; set; }
    
    /// <summary>
    /// Current engagement/popularity level
    /// </summary>
    public int Popularity { get; set; }
    
    /// <summary>
    /// Number of participants (denormalized)
    /// </summary>
    public int ParticipantCount { get; set; }
    
    /// <summary>
    /// Maximum allowed participants (null for unlimited)
    /// </summary>
    public int? MaxParticipants { get; set; }
    
    /// <summary>
    /// LLM-provided context JSON: involved accounts, relationships, tensions
    /// </summary>
    public string Metadata { get; set; } = "{}";
    
    /// <summary>
    /// Drama intensity level 1-10
    /// </summary>
    public int DramaLevel { get; set; } = 5;
    
    /// <summary>
    /// Probability of follow-up event
    /// </summary>
    public double FollowUpProbability { get; set; } = 0.5;
    
    /// <summary>
    /// Expected narrative arc length in stages
    /// </summary>
    public int NarrativeArcLength { get; set; } = 1;
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public Account? CreatorAccount { get; set; }
    public Community? Community { get; set; }
    public ICollection<EventParticipation> Participations { get; set; } = new List<EventParticipation>();
    
    /// <summary>
    /// Get the top-level category of this event type
    /// </summary>
    public EventType GetCategory()
    {
        var typeValue = (int)Type;
        if (typeValue < 2000) return EventType.Drama;
        if (typeValue < 3000) return EventType.Romance;
        if (typeValue < 4000) return EventType.Social;
        if (typeValue < 5000) return EventType.Fame;
        if (typeValue < 6000) return EventType.Community;
        if (typeValue < 7000) return EventType.Content;
        if (typeValue < 8000) return EventType.Trend;
        return EventType.News;
    }
}

/// <summary>
/// Event participation record
/// </summary>
public class EventParticipation
{
    public int Id { get; set; }
    
    public Guid EventParticipationId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Associated event
    /// </summary>
    public int EventId { get; set; }
    
    /// <summary>
    /// Participating account
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// Role in the event
    /// </summary>
    public ParticipantRole Role { get; set; }
    
    /// <summary>
    /// When the account joined this event
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// How much this account contributed to the event
    /// </summary>
    public int ContributionScore { get; set; }
    
    /// <summary>
    /// Participation status
    /// </summary>
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Active;
    
    /// <summary>
    /// LLM reasoning for why this account was chosen
    /// </summary>
    public string LLMReasoning { get; set; } = string.Empty;
    
    // Navigation properties
    public Event? Event { get; set; }
    public Account? Account { get; set; }
}

/// <summary>
/// Event consequence types
/// </summary>
public enum ConsequenceType
{
    RelationshipChange = 0,
    FollowerChange = 1,
    FameChange = 2,
    ReputationChange = 3,
    PostCreation = 4,
    Notification = 5,
    MemoryCreation = 6,
    OpinionChange = 7,
    CommunityMembershipChange = 8,
    FollowAction = 9,
    UnfollowAction = 10,
    BlockAction = 11,
    PostLike = 12,
    PostComment = 13
}

/// <summary>
/// Event consequence audit record
/// </summary>
public class EventConsequence
{
    public int Id { get; set; }
    
    public Guid EventConsequenceId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Associated event
    /// </summary>
    public int EventId { get; set; }
    
    /// <summary>
    /// Type of consequence
    /// </summary>
    public ConsequenceType Type { get; set; }
    
    /// <summary>
    /// JSON parameters for the consequence
    /// </summary>
    public string Parameters { get; set; } = "{}";
    
    /// <summary>
    /// Whether the consequence was successfully executed
    /// </summary>
    public bool WasExecuted { get; set; }
    
    /// <summary>
    /// If execution failed, the reason
    /// </summary>
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// When the consequence was processed
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Event? Event { get; set; }
}
