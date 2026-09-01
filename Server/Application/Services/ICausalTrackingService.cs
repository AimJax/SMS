using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for tracking causal relationships between events
/// </summary>
public interface ICausalTrackingService
{
    /// <summary>
    /// Record a causal link between two events
    /// </summary>
    Task<CausalChain> RecordCausalLinkAsync(
        Guid effectEventId,
        Guid causeEventId,
        CauseType causeType,
        string description,
        double causeStrength = 1.0,
        int? accountId = null);
    
    /// <summary>
    /// Get the full causal chain leading to an event
    /// </summary>
    Task<List<CausalChain>> GetCausalChainAsync(Guid eventId);
    
    /// <summary>
    /// Get all events in a chain from root to current
    /// </summary>
    Task<List<Event>> GetEventChainAsync(Guid rootEventId);
    
    /// <summary>
    /// Get the root cause of an event
    /// </summary>
    Task<Event?> GetRootCauseAsync(Guid eventId);
    
    /// <summary>
    /// Get all events caused by this event
    /// </summary>
    Task<List<Event>> GetDownstreamEventsAsync(Guid eventId);
    
    /// <summary>
    /// Generate a human-readable narrative of the causal chain
    /// </summary>
    Task<string> GenerateCausalNarrativeAsync(Guid eventId);
    
    /// <summary>
    /// Link a new event to its parent (for event chains)
    /// </summary>
    Task<Event> LinkToParentEventAsync(Event childEvent, Event parentEvent, CauseType causeType, string description);
}

/// <summary>
/// Service for offline world simulation
/// </summary>
public interface IOfflineSimulationService
{
    /// <summary>
    /// Get how long an account has been offline
    /// </summary>
    Task<TimeSpan> GetOfflineDurationAsync(int accountId);
    
    /// <summary>
    /// Check if offline simulation should run for an account
    /// </summary>
    Task<bool> ShouldRunOfflineSimulationAsync(int accountId);
    
    /// <summary>
    /// Run offline simulation and return catchup summary
    /// </summary>
    Task<CatchupSummary> RunOfflineSimulationAsync(int accountId);
    
    /// <summary>
    /// Get the latest catchup summary for an account
    /// </summary>
    Task<CatchupSummary?> GetCatchupSummaryAsync(int accountId);
    
    /// <summary>
    /// Acknowledge (mark as seen) a catchup summary
    /// </summary>
    Task AcknowledgeCatchupAsync(int accountId);
    
    /// <summary>
    /// Check if an account has unread catchup
    /// </summary>
    Task<bool> HasUnreadCatchupAsync(int accountId);
}
