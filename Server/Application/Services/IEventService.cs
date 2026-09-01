using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing events and their lifecycle
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Get all events with optional filtering
    /// </summary>
    Task<(IEnumerable<Event> Items, string? NextCursor)> GetEventsAsync(
        EventType? type = null,
        string? topic = null,
        EventStatus? status = null,
        string? cursor = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a single event by ID
    /// </summary>
    Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get events for a specific account
    /// </summary>
    Task<IEnumerable<Event>> GetEventsForAccountAsync(int accountId, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get participants for an event
    /// </summary>
    Task<IEnumerable<EventParticipation>> GetEventParticipantsAsync(int eventId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get active events (for NPC awareness)
    /// </summary>
    Task<IEnumerable<Event>> GetActiveEventsAsync(int limit = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get recent events that just ended
    /// </summary>
    Task<IEnumerable<Event>> GetRecentEndedEventsAsync(int limit = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for generating events via LLM
/// </summary>
public interface IEventGenerationService
{
    /// <summary>
    /// Propose the next event based on world state analysis
    /// </summary>
    Task<EventProposal?> ProposeNextEventAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Propose an event focused on a specific account
    /// </summary>
    Task<EventProposal?> ProposeEventForAccountAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Propose a community-focused event
    /// </summary>
    Task<EventProposal?> ProposeCommunityEventAsync(int communityId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate an event proposal
    /// </summary>
    Task<ValidationResult> ValidateProposalAsync(EventProposal proposal, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute an approved event
    /// </summary>
    Task<Event> ExecuteEventAsync(EventProposal proposal, CancellationToken cancellationToken = default);
}

/// <summary>
/// Event proposal from LLM
/// </summary>
public class EventProposal
{
    public EventType EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NarrativeContext { get; set; } = string.Empty;
    public int? PrimaryAccountId { get; set; }
    public int? SecondaryAccountId { get; set; }
    public string? Topic { get; set; }
    public int DramaLevel { get; set; } = 5;
    public List<EventParticipantProposal> Participants { get; set; } = new();
    public List<EventConsequenceProposal> ExpectedConsequences { get; set; } = new();
    public double FollowUpProbability { get; set; } = 0.5;
    public int NarrativeArcLength { get; set; } = 1;
}

/// <summary>
/// Proposed participant in an event
/// </summary>
public class EventParticipantProposal
{
    public int AccountId { get; set; }
    public ParticipantRole Role { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// Proposed consequence of an event
/// </summary>
public class EventConsequenceProposal
{
    public ConsequenceType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Validation result for event proposals
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
}
