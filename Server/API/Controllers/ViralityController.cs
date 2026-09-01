using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.API.Controllers;

/// <summary>
/// API endpoints for virality management
/// </summary>
public static class ViralityEndpoints
{
    public static void MapViralityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Virality");

        // Get viral posts
        group.MapGet("/viral", async (
            int count,
            ViralityState minState,
            string? topic,
            IViralityService viralityService,
            CancellationToken ct) =>
        {
            var posts = await viralityService.GetViralPostsAsync(count, minState, topic, ct);
            return Results.Ok(new
            {
                count = posts.Count,
                posts = posts.Select(p => new
                {
                    p.PostId,
                    p.Content,
                    p.Topic,
                    p.CreatedAt,
                    author = new { p.AuthorAccount?.Username }
                })
            });
        });

        // Get trending posts
        group.MapGet("/trending", async (
            int count,
            string? topic,
            IViralityService viralityService,
            CancellationToken ct) =>
        {
            var posts = await viralityService.GetTrendingPostsAsync(count, topic, ct);
            return Results.Ok(new
            {
                count = posts.Count,
                posts = posts.Select(p => new
                {
                    p.PostId,
                    p.Content,
                    p.Topic,
                    p.CreatedAt,
                    author = new { p.AuthorAccount?.Username }
                })
            });
        });

        // Get post virality details
        group.MapGet("/{postId}/virality", async (
            Guid postId,
            IViralityService viralityService,
            CancellationToken ct) =>
        {
            var virality = await viralityService.GetPostViralityAsync(postId, ct);
            if (virality == null)
            {
                return Results.NotFound(new { message = "Virality data not found for this post" });
            }

            return Results.Ok(new
            {
                postId = virality.PostId,
                state = virality.State.ToString(),
                score = virality.Score,
                totalEngagement = virality.TotalEngagement,
                velocity = virality.Velocity,
                peakVelocity = virality.PeakVelocity,
                reach = virality.Reach,
                shareCount = virality.ShareCount,
                viralAt = virality.ViralAt,
                massivelyViralAt = virality.MassivelyViralAt,
                declinedAt = virality.DeclinedAt,
                controversyLevel = virality.ControversyLevel,
                lastUpdated = virality.LastUpdated
            });
        });

        // Get virality state
        group.MapGet("/{postId}/virality-state", async (
            Guid postId,
            IViralityService viralityService,
            CancellationToken ct) =>
        {
            var state = await viralityService.GetViralityStateAsync(postId, ct);
            return Results.Ok(new { postId, state = state.ToString() });
        });

        // Get transition history
        group.MapGet("/{postId}/virality-history", async (
            Guid postId,
            IViralityService viralityService,
            CancellationToken ct) =>
        {
            var transitions = await viralityService.GetTransitionHistoryAsync(postId, ct);
            return Results.Ok(new
            {
                postId,
                transitions = transitions.Select(t => new
                {
                    t.TransitionId,
                    fromState = t.FromState.ToString(),
                    toState = t.ToState.ToString(),
                    t.ScoreAtTransition,
                    t.EngagementAtTransition,
                    t.VelocityAtTransition,
                    t.TransitionedAt
                })
            });
        });

        // Trigger virality calculation
        group.MapPost("/{postId}/calculate-virality", async (
            Guid postId,
            IViralityService viralityService,
            CancellationToken ct) =>
        {
            try
            {
                var virality = await viralityService.CalculateViralityAsync(postId, ct);
                return Results.Ok(new
                {
                    postId = virality.PostId,
                    state = virality.State.ToString(),
                    score = virality.Score,
                    message = "Virality recalculated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        });

        // Analyze controversy
        group.MapPost("/{postId}/analyze-controversy", async (
            Guid postId,
            IViralityService viralityService,
            CancellationToken ct) =>
        {
            var controversyLevel = await viralityService.AnalyzeControversyAsync(postId, ct);
            return Results.Ok(new
            {
                postId,
                controversyLevel,
                description = controversyLevel switch
                {
                    >= 8 => "Highly controversial",
                    >= 5 => "Moderately controversial",
                    >= 3 => "Slightly controversial",
                    _ => "Not controversial"
                }
            });
        });
    }
}
