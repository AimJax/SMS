using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.API.Controllers;

/// <summary>
/// API endpoints for event causality and offline simulation
/// </summary>
public static class CausalityEndpoints
{
    public static void MapCausalityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events/{eventId}").WithTags("Causality");

        // Get causal chain for an event
        group.MapGet("/chain", async (
            Guid eventId,
            ICausalTrackingService causalService,
            CancellationToken ct) =>
        {
            var chain = await causalService.GetCausalChainAsync(eventId);
            return Results.Ok(new
            {
                eventId,
                chainLength = chain.Count,
                chain = chain.Select(c => new
                {
                    c.CauseEventId,
                    c.CauseType,
                    c.CauseDescription,
                    c.CauseStrength,
                    c.AccountId,
                    c.CreatedAt
                })
            });
        });

        // Get event chain (all related events)
        group.MapGet("/event-chain", async (
            Guid eventId,
            ICausalTrackingService causalService,
            CancellationToken ct) =>
        {
            var events = await causalService.GetEventChainAsync(eventId);
            return Results.Ok(new
            {
                rootEventId = eventId,
                eventCount = events.Count,
                events = events.Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Type,
                    e.Status,
                    e.ChainDepth,
                    e.CreatedAt
                })
            });
        });

        // Get root cause of an event
        group.MapGet("/root-cause", async (
            Guid eventId,
            ICausalTrackingService causalService,
            CancellationToken ct) =>
        {
            var rootCause = await causalService.GetRootCauseAsync(eventId);
            if (rootCause == null)
                return Results.NotFound();

            return Results.Ok(new
            {
                rootCause.EventId,
                rootCause.Title,
                rootCause.Type,
                rootCause.Description,
                rootCause.CreatedAt
            });
        });

        // Get downstream events caused by this event
        group.MapGet("/downstream", async (
            Guid eventId,
            ICausalTrackingService causalService,
            CancellationToken ct) =>
        {
            var downstream = await causalService.GetDownstreamEventsAsync(eventId);
            return Results.Ok(new
            {
                sourceEventId = eventId,
                downstreamCount = downstream.Count,
                events = downstream.Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Type,
                    e.Status,
                    e.ChainDepth,
                    e.CreatedAt
                })
            });
        });

        // Generate causal narrative
        group.MapGet("/narrative", async (
            Guid eventId,
            ICausalTrackingService causalService,
            CancellationToken ct) =>
        {
            var narrative = await causalService.GenerateCausalNarrativeAsync(eventId);
            return Results.Ok(new { eventId, narrative });
        });
    }
}

/// <summary>
/// API endpoints for offline simulation
/// </summary>
public static class OfflineEndpoints
{
    public static void MapOfflineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/catchup").WithTags("Offline Simulation");

        // Get catchup summary for authenticated user
        group.MapGet("", async (
            int accountId,
            IOfflineSimulationService offlineService,
            CancellationToken ct) =>
        {
            // Check if we should run offline simulation first
            if (await offlineService.ShouldRunOfflineSimulationAsync(accountId))
            {
                var duration = await offlineService.GetOfflineDurationAsync(accountId);
                if (duration.TotalHours >= 1)
                {
                    await offlineService.RunOfflineSimulationAsync(accountId);
                }
            }

            var summary = await offlineService.GetCatchupSummaryAsync(accountId);
            if (summary == null)
            {
                var offlineDuration = await offlineService.GetOfflineDurationAsync(accountId);
                return Results.Ok(new
                {
                    hasCatchup = false,
                    offlineDuration = offlineDuration.ToString(),
                    message = "No catchup data available."
                });
            }

            return Results.Ok(new
            {
                hasCatchup = true,
                summary.OfflineSimulationResultId,
                duration = summary.Duration.ToString(),
                summary.OfflineSince,
                summary.OfflineUntil,
                stats = new
                {
                    summary.NewFollowers,
                    summary.LostFollowers,
                    summary.NotificationsCreated,
                    summary.PostsCreated
                },
                summary.MajorEvents,
                summary.Summary,
                summary.IsAcknowledged
            });
        }).RequireAuthorization();

        // Acknowledge (mark as seen) catchup summary
        group.MapPost("/acknowledge", async (
            int accountId,
            IOfflineSimulationService offlineService,
            CancellationToken ct) =>
        {
            await offlineService.AcknowledgeCatchupAsync(accountId);
            return Results.Ok(new { acknowledged = true });
        }).RequireAuthorization();

        // Check if user has unread catchup
        group.MapGet("/has-unread", async (
            int accountId,
            IOfflineSimulationService offlineService,
            CancellationToken ct) =>
        {
            var hasUnread = await offlineService.HasUnreadCatchupAsync(accountId);
            return Results.Ok(new { hasUnread });
        }).RequireAuthorization();

        // Get offline duration for an account
        group.MapGet("/duration", async (
            int accountId,
            IOfflineSimulationService offlineService,
            CancellationToken ct) =>
        {
            var duration = await offlineService.GetOfflineDurationAsync(accountId);
            return Results.Ok(new
            {
                accountId,
                duration = duration.ToString(),
                hours = duration.TotalHours,
                shouldSimulate = duration.TotalHours >= 1
            });
        });
    }
}
