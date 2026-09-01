using System.Text.Json;

namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Types of causal relationships between events
/// </summary>
public enum CauseType
{
    /// <summary>
    /// This event directly caused the next
    /// </summary>
    Direct = 0,
    
    /// <summary>
    /// This event contributed indirectly
    /// </summary>
    Indirect = 1,
    
    /// <summary>
    /// One of multiple contributing factors
    /// </summary>
    Contributing = 2,
    
    /// <summary>
    /// Final trigger that broke the camel's back
    /// </summary>
    Trigger = 3
}

/// <summary>
/// Records a causal link between events
/// </summary>
public class CausalChain
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable internal identifier
    /// </summary>
    public Guid CausalChainId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The event that was caused (effect)
    /// </summary>
    public Guid EventId { get; set; }
    
    /// <summary>
    /// The event that caused this (cause)
    /// </summary>
    public Guid CauseEventId { get; set; }
    
    /// <summary>
    /// Type of causal relationship
    /// </summary>
    public CauseType CauseType { get; set; }
    
    /// <summary>
    /// Human-readable explanation of the causal relationship
    /// </summary>
    public string CauseDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// How much this cause contributed (0.0-1.0)
    /// </summary>
    public double CauseStrength { get; set; } = 1.0;
    
    /// <summary>
    /// Account whose action caused this (nullable)
    /// </summary>
    public int? AccountId { get; set; }
    
    /// <summary>
    /// When this causal link was recorded
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional context as JSON
    /// </summary>
    public string Metadata { get; set; } = "{}";
    
    // Navigation properties
    public Event? Event { get; set; }
    public Event? CauseEvent { get; set; }
    public Account? Account { get; set; }
}

/// <summary>
/// Result of offline simulation for an account
/// </summary>
public class OfflineSimulationResult
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable internal identifier
    /// </summary>
    public Guid OfflineSimulationResultId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Account this simulation was run for
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// When the offline period started
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// When the offline period ended
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Duration of offline period
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Number of compressed ticks simulated
    /// </summary>
    public int TicksSimulated { get; set; }
    
    /// <summary>
    /// Number of posts created during simulation
    /// </summary>
    public int PostsCreated { get; set; }
    
    /// <summary>
    /// Number of followers gained
    /// </summary>
    public int FollowersGained { get; set; }
    
    /// <summary>
    /// Number of followers lost
    /// </summary>
    public int FollowersLost { get; set; }
    
    /// <summary>
    /// Number of events created during simulation
    /// </summary>
    public int EventsCreated { get; set; }
    
    /// <summary>
    /// Number of notifications created
    /// </summary>
    public int NotificationsCreated { get; set; }
    
    /// <summary>
    /// Major events summary JSON
    /// </summary>
    public string EventsSummaryJson { get; set; } = "[]";
    
    /// <summary>
    /// LLM-generated catchup summary
    /// </summary>
    public string CatchupSummary { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the catchup has been acknowledged by the user
    /// </summary>
    public bool IsAcknowledged { get; set; }
    
    /// <summary>
    /// When this simulation result was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Account? Account { get; set; }
}

/// <summary>
/// Summary of a catchup period for display
/// </summary>
public class CatchupSummary
{
    public Guid OfflineSimulationResultId { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime OfflineSince { get; set; }
    public DateTime OfflineUntil { get; set; }
    public int NewFollowers { get; set; }
    public int LostFollowers { get; set; }
    public int NotificationsCreated { get; set; }
    public int PostsCreated { get; set; }
    public List<EventSummary> MajorEvents { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
}

/// <summary>
/// Summary of an event for catchup display
/// </summary>
public class EventSummary
{
    public Guid EventId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int DramaLevel { get; set; }
    public int ParticipantCount { get; set; }
}
