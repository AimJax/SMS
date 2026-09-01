using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing events and their lifecycle
/// </summary>
public class EventService : IEventService
{
    private readonly AppDbContext _context;
    private readonly ILogger<EventService> _logger;

    public EventService(AppDbContext context, ILogger<EventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Event> Items, string? NextCursor)> GetEventsAsync(
        EventType? type = null,
        string? topic = null,
        EventStatus? status = null,
        string? cursor = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        
        var query = _context.Events
            .Include(e => e.CreatorAccount)
            .Include(e => e.Community)
            .Where(e => !e.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (type.HasValue)
        {
            query = query.Where(e => e.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            query = query.Where(e => e.Topic == topic);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        // Apply cursor pagination
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            // Cursor format: {eventId}_{createdAt:o}
            var parts = cursor.Split('_');
            if (parts.Length >= 2)
            {
                if (Guid.TryParse(parts[0], out var cursorEventId))
                {
                    if (DateTime.TryParse(string.Join("_", parts.Skip(1)), out var cursorDate))
                    {
                        query = query.Where(e => e.CreatedAt < cursorDate || 
                            (e.CreatedAt == cursorDate && string.Compare(e.EventId.ToString(), cursorEventId.ToString(), StringComparison.Ordinal) > 0));
                    }
                }
            }
        }

        // Order by newest first
        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .ThenByDescending(e => e.EventId)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (events.Count > pageSize)
        {
            events.RemoveAt(events.Count - 1);
            var lastEvent = events.Last();
            nextCursor = $"{lastEvent.EventId}_{lastEvent.CreatedAt:O}";
        }

        return (events, nextCursor);
    }

    public async Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.CreatorAccount)
            .Include(e => e.Community)
            .Include(e => e.Participations)
                .ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(e => e.EventId == eventId && !e.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetEventsForAccountAsync(int accountId, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        
        // Get events where the account is a participant or creator
        var participantEventIds = await _context.EventParticipations
            .Where(p => p.AccountId == accountId)
            .Select(p => p.EventId)
            .ToListAsync(cancellationToken);

        return await _context.Events
            .Include(e => e.CreatorAccount)
            .Include(e => e.Community)
            .Where(e => !e.IsDeleted && 
                (e.CreatorAccountId == accountId || participantEventIds.Contains(e.Id)))
            .OrderByDescending(e => e.CreatedAt)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventParticipation>> GetEventParticipantsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return await _context.EventParticipations
            .Include(p => p.Account)
            .Where(p => p.EventId == eventId)
            .OrderByDescending(p => p.ContributionScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetActiveEventsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Participations)
            .Where(e => !e.IsDeleted && e.Status == EventStatus.Active)
            .OrderByDescending(e => e.Popularity)
            .ThenByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetRecentEndedEventsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Participations)
            .Where(e => !e.IsDeleted && e.Status == EventStatus.Ended)
            .OrderByDescending(e => e.EndAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
