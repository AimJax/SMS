using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for tracking causal relationships between events
/// </summary>
public class CausalTrackingService : ICausalTrackingService
{
    private readonly AppDbContext _context;
    private readonly IAiTextGenerationService? _aiService;
    private readonly ILogger<CausalTrackingService> _logger;

    public CausalTrackingService(
        AppDbContext context,
        IAiTextGenerationService? aiService = null,
        ILogger<CausalTrackingService> logger = null!)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<CausalChain> RecordCausalLinkAsync(
        Guid effectEventId,
        Guid causeEventId,
        CauseType causeType,
        string description,
        double causeStrength = 1.0,
        int? accountId = null)
    {
        var chain = new CausalChain
        {
            EventId = effectEventId,
            CauseEventId = causeEventId,
            CauseType = causeType,
            CauseDescription = description,
            CauseStrength = causeStrength,
            AccountId = accountId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CausalChains.Add(chain);
        await _context.SaveChangesAsync();

        _logger?.LogDebug("Recorded causal link: {CauseEventId} -> {EffectEventId} ({CauseType})",
            causeEventId, effectEventId, causeType);

        return chain;
    }

    public async Task<List<CausalChain>> GetCausalChainAsync(Guid eventId)
    {
        var chain = new List<CausalChain>();
        var visited = new HashSet<Guid>();
        var currentEventId = eventId;

        // Walk backwards through the causal chain
        while (true)
        {
            if (visited.Contains(currentEventId))
                break; // Prevent infinite loop

            visited.Add(currentEventId);

            var causalLink = await _context.CausalChains
                .Include(c => c.CauseEvent)
                .Include(c => c.Account)
                .Where(c => c.EventId == currentEventId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (causalLink == null)
                break;

            chain.Add(causalLink);
            currentEventId = causalLink.CauseEventId;
        }

        return chain;
    }

    public async Task<List<Event>> GetEventChainAsync(Guid rootEventId)
    {
        var events = new List<Event>();
        var visited = new HashSet<Guid>();
        var currentEventId = rootEventId;

        // Start from root event
        var rootEvent = await _context.Events
            .Include(e => e.Participations)
            .FirstOrDefaultAsync(e => e.EventId == rootEventId);

        if (rootEvent == null)
            return events;

        events.Add(rootEvent);
        visited.Add(rootEventId);

        // Walk forward through downstream events
        var queue = new Queue<Guid>();
        queue.Enqueue(rootEventId);

        while (queue.Count > 0 && events.Count < 100) // Cap at 100 events
        {
            var parentId = queue.Dequeue();

            var children = await _context.Events
                .Include(e => e.Participations)
                .Where(e => e.ParentEventId == parentId)
                .OrderBy(e => e.ChainDepth)
                .ToListAsync();

            foreach (var child in children)
            {
                if (visited.Contains(child.EventId))
                    continue;

                events.Add(child);
                visited.Add(child.EventId);
                queue.Enqueue(child.EventId);
            }
        }

        return events.OrderBy(e => e.ChainDepth).ThenBy(e => e.CreatedAt).ToList();
    }

    public async Task<Event?> GetRootCauseAsync(Guid eventId)
    {
        var chain = await GetCausalChainAsync(eventId);
        
        if (chain.Count == 0)
        {
            // No causes - this is the root
            return await _context.Events
                .Include(e => e.Participations)
                .FirstOrDefaultAsync(e => e.EventId == eventId);
        }

        // The last item in the chain is the root cause
        var rootCauseEventId = chain.Last().CauseEventId;
        return await _context.Events
            .Include(e => e.Participations)
            .FirstOrDefaultAsync(e => e.EventId == rootCauseEventId);
    }

    public async Task<List<Event>> GetDownstreamEventsAsync(Guid eventId)
    {
        var events = new List<Event>();
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();

        queue.Enqueue(eventId);
        visited.Add(eventId);

        while (queue.Count > 0 && events.Count < 100) // Cap at 100 events
        {
            var currentId = queue.Dequeue();

            var downstream = await _context.Events
                .Include(e => e.Participations)
                .Where(e => e.ParentEventId == currentId || e.TriggerEventId == currentId)
                .ToListAsync();

            foreach (var evt in downstream)
            {
                if (visited.Contains(evt.EventId))
                    continue;

                events.Add(evt);
                visited.Add(evt.EventId);
                queue.Enqueue(evt.EventId);
            }
        }

        return events.OrderBy(e => e.ChainDepth).ThenBy(e => e.CreatedAt).ToList();
    }

    public async Task<string> GenerateCausalNarrativeAsync(Guid eventId)
    {
        var chain = await GetCausalChainAsync(eventId);
        var effectEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (effectEvent == null)
            return "Event not found.";

        if (chain.Count == 0)
            return $"This event ({effectEvent.Title}) has no recorded causes.";

        // Build context for LLM
        var chainContext = new List<string>();
        for (int i = chain.Count - 1; i >= 0; i--)
        {
            var cause = chain[i];
            var causeEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == cause.CauseEventId);
            if (causeEvent != null)
            {
                chainContext.Add($"{i + 1}. {causeEvent.Title}: {cause.CauseDescription}");
            }
        }

        var prompt = $@"You are analyzing a causal chain in a social media simulation.

EVENT: {effectEvent.Title}
Description: {effectEvent.Description}
CAUSAL CHAIN:
{string.Join("\n", chainContext)}

Generate a brief narrative explanation (2-3 sentences) of how these causes led to this event. Be dramatic but factual.";

        if (_aiService != null && _aiService.IsConfigured)
        {
            var result = await _aiService.GenerateAsync(new AiGenerationRequest
            {
                SystemPrompt = "You are a narrative analyst for a social media simulation.",
                UserPrompt = prompt,
                MaxTokens = 200,
                Temperature = 0.7
            });

            if (result.Success && !string.IsNullOrWhiteSpace(result.Text))
                return result.Text;
        }

        // Fallback: simple text summary
        return $"This event was caused by {chain.Count} preceding event(s). {chain.LastOrDefault()?.CauseDescription ?? "The chain of events led to this outcome."}";
    }

    public async Task<Event> LinkToParentEventAsync(Event childEvent, Event parentEvent, CauseType causeType, string description)
    {
        // Set parent-child relationship
        childEvent.ParentEventId = parentEvent.EventId;
        childEvent.TriggerEventId ??= parentEvent.EventId;
        childEvent.EventChainId ??= parentEvent.EventChainId ?? parentEvent.EventId;
        childEvent.ChainDepth = parentEvent.ChainDepth + 1;

        await _context.SaveChangesAsync();

        // Record causal link
        await RecordCausalLinkAsync(
            childEvent.EventId,
            parentEvent.EventId,
            causeType,
            description,
            1.0,
            childEvent.CreatorAccountId);

        _logger?.LogInformation("Linked event {ChildId} to parent {ParentId} with cause: {Description}",
            childEvent.EventId, parentEvent.EventId, description);

        return childEvent;
    }
}
