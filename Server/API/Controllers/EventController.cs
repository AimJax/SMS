using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class EventController
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        // Browse events (public)
        app.MapGet("/api/events", GetEvents)
            .WithTags("Events")
            .WithName("GetEvents")
            .AllowAnonymous();

        // Event details (public)
        app.MapGet("/api/events/{eventId:guid}", GetEventById)
            .WithTags("Events")
            .WithName("GetEventById")
            .AllowAnonymous();

        // Event participants (public)
        app.MapGet("/api/events/{eventId:guid}/participants", GetEventParticipants)
            .WithTags("Events")
            .WithName("GetEventParticipants")
            .AllowAnonymous();

        // Get events for an account (authenticated)
        app.MapGet("/api/accounts/{accountId:int}/events", GetAccountEvents)
            .WithTags("Events")
            .WithName("GetAccountEvents")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetEvents(
        IEventService eventService,
        [FromQuery] EventType? type,
        [FromQuery] string? topic,
        [FromQuery] EventStatus? status,
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (events, nextCursor) = await eventService.GetEventsAsync(type, topic, status, cursor, pageSize);

        var response = events.Select(e => new EventResponse(
            e.EventId,
            e.Type.ToString(),
            e.Title,
            e.Description,
            e.NarrativeContext,
            e.CreatorAccountId,
            e.CreatorAccount?.Username,
            e.CreatedAt,
            e.StartAt,
            e.EndAt,
            e.Status.ToString(),
            e.Visibility.ToString(),
            e.Topic,
            e.CommunityId,
            e.Community?.Name,
            e.Popularity,
            e.ParticipantCount,
            e.DramaLevel
        ));

        return Results.Ok(new EventListResponse(response, nextCursor, pageSize));
    }

    private static async Task<IResult> GetEventById(
        Guid eventId,
        IEventService eventService)
    {
        var evt = await eventService.GetEventByIdAsync(eventId);

        if (evt == null)
        {
            return Results.NotFound(new { message = "Event not found" });
        }

        var response = new EventDetailResponse(
            evt.EventId,
            evt.Type.ToString(),
            evt.Title,
            evt.Description,
            evt.NarrativeContext,
            evt.CreatorAccountId,
            evt.CreatorAccount?.Username,
            evt.CreatedAt,
            evt.StartAt,
            evt.EndAt,
            evt.Status.ToString(),
            evt.Visibility.ToString(),
            evt.Topic,
            evt.CommunityId,
            evt.Community?.Name,
            evt.Popularity,
            evt.ParticipantCount,
            evt.DramaLevel,
            evt.FollowUpProbability,
            evt.NarrativeArcLength,
            evt.Participations.Select(p => new EventParticipantResponse(
                p.EventParticipationId,
                p.AccountId,
                p.Account?.Username,
                p.Role.ToString(),
                p.JoinedAt,
                p.ContributionScore,
                p.Status.ToString(),
                p.LLMReasoning
            ))
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> GetEventParticipants(
        Guid eventId,
        IEventService eventService)
    {
        var evt = await eventService.GetEventByIdAsync(eventId);

        if (evt == null)
        {
            return Results.NotFound(new { message = "Event not found" });
        }

        var participants = await eventService.GetEventParticipantsAsync(evt.Id);

        var response = participants.Select(p => new EventParticipantResponse(
            p.EventParticipationId,
            p.AccountId,
            p.Account?.Username,
            p.Role.ToString(),
            p.JoinedAt,
            p.ContributionScore,
            p.Status.ToString(),
            p.LLMReasoning
        ));

        return Results.Ok(new { eventId, participants = response });
    }

    private static async Task<IResult> GetAccountEvents(
        int accountId,
        IEventService eventService,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var events = await eventService.GetEventsForAccountAsync(accountId, pageSize);

        var response = events.Select(e => new EventResponse(
            e.EventId,
            e.Type.ToString(),
            e.Title,
            e.Description,
            e.NarrativeContext,
            e.CreatorAccountId,
            e.CreatorAccount?.Username,
            e.CreatedAt,
            e.StartAt,
            e.EndAt,
            e.Status.ToString(),
            e.Visibility.ToString(),
            e.Topic,
            e.CommunityId,
            e.Community?.Name,
            e.Popularity,
            e.ParticipantCount,
            e.DramaLevel
        ));

        return Results.Ok(new { accountId, events = response });
    }
}

// Response DTOs
public record EventResponse(
    Guid EventId,
    string Type,
    string Title,
    string Description,
    string NarrativeContext,
    int? CreatorAccountId,
    string? CreatorUsername,
    DateTime CreatedAt,
    DateTime StartAt,
    DateTime? EndAt,
    string Status,
    string Visibility,
    string? Topic,
    int? CommunityId,
    string? CommunityName,
    int Popularity,
    int ParticipantCount,
    int DramaLevel
);

public record EventDetailResponse(
    Guid EventId,
    string Type,
    string Title,
    string Description,
    string NarrativeContext,
    int? CreatorAccountId,
    string? CreatorUsername,
    DateTime CreatedAt,
    DateTime StartAt,
    DateTime? EndAt,
    string Status,
    string Visibility,
    string? Topic,
    int? CommunityId,
    string? CommunityName,
    int Popularity,
    int ParticipantCount,
    int DramaLevel,
    double FollowUpProbability,
    int NarrativeArcLength,
    IEnumerable<EventParticipantResponse> Participants
);

public record EventParticipantResponse(
    Guid EventParticipationId,
    int AccountId,
    string? Username,
    string Role,
    DateTime JoinedAt,
    int ContributionScore,
    string Status,
    string LLMReasoning
);

public record EventListResponse(
    IEnumerable<EventResponse> Items,
    string? NextCursor,
    int PageSize
);
