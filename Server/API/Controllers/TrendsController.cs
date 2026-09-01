using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.API.Controllers;

[ApiController]
public static class TrendsController
{
    /// <summary>
    /// Maps all trend-related endpoints
    /// </summary>
    public static void MapTrendEndpoints(this WebApplication app)
    {
        // Global trends
        app.MapGet("/api/trends", GetGlobalTrends);
        app.MapGet("/api/trends/{id}", GetTrendById);
        
        // Community trends
        app.MapGet("/api/communities/{id:int}/trends", GetCommunityTrends);
        
        // Personal trends (requires auth)
        app.MapGet("/api/me/trends", GetPersonalTrends);
        
        // Hashtag endpoints
        app.MapGet("/api/hashtags/trending", GetTrendingHashtags);
        app.MapGet("/api/hashtags/{tag}", GetHashtagByTag);
        
        // Topic endpoints
        app.MapGet("/api/topics", GetAllTopics);
        app.MapGet("/api/topics/search", SearchTopics);
        app.MapGet("/api/topics/{slug}", GetTopicBySlug);
        app.MapGet("/api/topics/{slug}/posts", GetTopicPosts);
        
        // Topic subscription
        app.MapPost("/api/topics/{topicId:guid}/subscribe", SubscribeToTopic);
        app.MapDelete("/api/topics/{topicId:guid}/subscribe", UnsubscribeFromTopic);
        
        // Manual trend calculation (admin)
        app.MapPost("/api/trends/calculate", CalculateTrend);
        app.MapPost("/api/trends/process", ProcessTrendsTick);
    }

    /// <summary>
    /// GET /api/trends
    /// Returns global trending topics
    /// </summary>
    private static async Task<IResult> GetGlobalTrends(
        [FromServices] ITrendService trendService,
        [FromQuery] int count = 10)
    {
        if (count < 1 || count > 50)
        {
            count = 10;
        }

        var trends = await trendService.GetGlobalTrendsAsync(count);
        return Results.Ok(trends.Select(t => new
        {
            t.TrendId,
            t.Type,
            t.Query,
            t.DisplayName,
            t.Slug,
            t.Strength,
            t.PostCount,
            t.UniquePosters,
            t.EngagementTotal,
            t.Velocity,
            t.Rank,
            t.Scope,
            t.CalculatedAt,
            TopicName = t.Topic?.DisplayName,
            Hashtag = t.Hashtag?.Tag
        }));
    }

    /// <summary>
    /// GET /api/trends/{id}
    /// Returns a specific trend by ID
    /// </summary>
    private static async Task<IResult> GetTrendById(
        [FromRoute] Guid id,
        [FromServices] ITrendService trendService)
    {
        var trend = await trendService.GetTrendByIdAsync(id);
        
        if (trend == null)
        {
            return Results.NotFound(new { error = "Trend not found" });
        }

        return Results.Ok(new
        {
            trend.TrendId,
            trend.Type,
            trend.Query,
            trend.DisplayName,
            trend.Slug,
            trend.Strength,
            trend.PostCount,
            trend.UniquePosters,
            trend.EngagementTotal,
            trend.Velocity,
            trend.Rank,
            trend.Scope,
            trend.CalculatedAt,
            trend.PeakedAt,
            trend.ExpiresAt,
            trend.IsActive,
            TopicName = trend.Topic?.DisplayName,
            Hashtag = trend.Hashtag?.Tag,
            CommunityId = trend.Community?.Id
        });
    }

    /// <summary>
    /// GET /api/communities/{id}/trends
    /// Returns trending topics for a community
    /// </summary>
    private static async Task<IResult> GetCommunityTrends(
        [FromRoute] int id,
        [FromServices] ITrendService trendService,
        [FromServices] ICommunityService communityService,
        [FromQuery] int count = 10)
    {
        var community = await communityService.GetByIdAsync(id);
        if (community == null)
        {
            return Results.NotFound(new { error = "Community not found" });
        }

        if (count < 1 || count > 50)
        {
            count = 10;
        }

        var trends = await trendService.GetCommunityTrendsAsync(id, count);
        return Results.Ok(trends.Select(t => new
        {
            t.TrendId,
            t.Type,
            t.Query,
            t.DisplayName,
            t.Slug,
            t.Strength,
            t.PostCount,
            t.UniquePosters,
            t.EngagementTotal,
            t.Velocity,
            t.Rank,
            TopicName = t.Topic?.DisplayName
        }));
    }

    /// <summary>
    /// GET /api/me/trends
    /// Returns personalized trending topics for the authenticated user
    /// </summary>
    private static async Task<IResult> GetPersonalTrends(
        [FromServices] ITrendService trendService,
        [FromServices] IAccountService accountService,
        HttpContext context,
        [FromQuery] int count = 10)
    {
        var accountId = GetAccountId(context);
        if (accountId == null)
        {
            return Results.Unauthorized();
        }

        if (count < 1 || count > 50)
        {
            count = 10;
        }

        var trends = await trendService.GetPersonalTrendsAsync(accountId.Value, count);
        return Results.Ok(trends.Select(t => new
        {
            t.TrendId,
            t.Type,
            t.Query,
            t.DisplayName,
            t.Slug,
            t.Strength,
            t.PostCount,
            t.UniquePosters,
            t.EngagementTotal,
            t.Velocity,
            t.Rank,
            t.Scope,
            TopicName = t.Topic?.DisplayName,
            Hashtag = t.Hashtag?.Tag
        }));
    }

    /// <summary>
    /// GET /api/hashtags/trending
    /// Returns trending hashtags
    /// </summary>
    private static async Task<IResult> GetTrendingHashtags(
        [FromServices] ITrendService trendService,
        [FromQuery] int count = 20)
    {
        if (count < 1 || count > 50)
        {
            count = 20;
        }

        var hashtags = await trendService.GetTrendingHashtagsAsync(count);
        return Results.Ok(hashtags.Select(h => new
        {
            h.HashtagId,
            h.Tag,
            h.NormalizedTag,
            h.UsageCount,
            h.TodayUsageCount,
            h.IsTrending,
            h.TrendingSince,
            h.TrendingRank,
            TopicName = h.Topic?.DisplayName
        }));
    }

    /// <summary>
    /// GET /api/hashtags/{tag}
    /// Returns a hashtag by tag name
    /// </summary>
    private static async Task<IResult> GetHashtagByTag(
        [FromRoute] string tag,
        [FromServices] ITrendService trendService)
    {
        var hashtag = await trendService.GetHashtagByTagAsync(tag);
        
        if (hashtag == null)
        {
            return Results.NotFound(new { error = "Hashtag not found" });
        }

        return Results.Ok(new
        {
            hashtag.HashtagId,
            hashtag.Tag,
            hashtag.NormalizedTag,
            hashtag.UsageCount,
            hashtag.TodayUsageCount,
            hashtag.IsTrending,
            hashtag.TrendingSince,
            hashtag.TrendingRank,
            TopicName = hashtag.Topic?.DisplayName,
            TopicId = hashtag.Topic?.TopicId
        });
    }

    /// <summary>
    /// GET /api/topics
    /// Returns all topics
    /// </summary>
    private static async Task<IResult> GetAllTopics(
        [FromServices] ITrendService trendService)
    {
        var topics = await trendService.GetAllTopicsAsync();
        return Results.Ok(topics.Select(t => new
        {
            t.TopicId,
            t.Name,
            t.DisplayName,
            t.Slug,
            t.Description,
            t.Category,
            t.PostCount,
            t.ActivePostCount,
            t.SubscriberCount,
            t.IsVerified,
            t.CreatedAt
        }));
    }

    /// <summary>
    /// GET /api/topics/search
    /// Search topics by name
    /// </summary>
    private static async Task<IResult> SearchTopics(
        [FromServices] ITrendService trendService,
        [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest(new { error = "Query parameter 'q' is required" });
        }

        var topics = await trendService.GetAllTopicsAsync();
        var filtered = topics.Where(t => 
            t.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            t.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase));

        return Results.Ok(filtered.Select(t => new
        {
            t.TopicId,
            t.Name,
            t.DisplayName,
            t.Slug,
            t.Category,
            t.PostCount,
            t.IsVerified
        }));
    }

    /// <summary>
    /// GET /api/topics/{slug}
    /// Returns topic details by slug
    /// </summary>
    private static async Task<IResult> GetTopicBySlug(
        [FromRoute] string slug,
        [FromServices] ITrendService trendService)
    {
        var topic = await trendService.GetTopicBySlugAsync(slug);
        
        if (topic == null)
        {
            return Results.NotFound(new { error = "Topic not found" });
        }

        return Results.Ok(new
        {
            topic.TopicId,
            topic.Name,
            topic.DisplayName,
            topic.Slug,
            topic.Description,
            topic.Category,
            topic.PostCount,
            topic.ActivePostCount,
            topic.SubscriberCount,
            topic.IsVerified,
            topic.IsActive,
            topic.CreatedAt,
            topic.UpdatedAt,
            Hashtags = topic.Hashtags?.Select(h => new { h.HashtagId, h.Tag, h.UsageCount })
        });
    }

    /// <summary>
    /// GET /api/topics/{slug}/posts
    /// Returns posts for a topic
    /// </summary>
    private static async Task<IResult> GetTopicPosts(
        [FromRoute] string slug,
        [FromServices] ITrendService trendService,
        [FromServices] IPostService postService,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20)
    {
        var topic = await trendService.GetTopicBySlugAsync(slug);
        if (topic == null)
        {
            return Results.NotFound(new { error = "Topic not found" });
        }

        if (pageSize < 1 || pageSize > 50)
        {
            pageSize = 20;
        }

        var posts = await postService.GetPostsByTopicAsync(topic.Name, cursor, pageSize);
        return Results.Ok(posts);
    }

    /// <summary>
    /// POST /api/topics/{topicId}/subscribe
    /// Subscribe to a topic
    /// </summary>
    private static async Task<IResult> SubscribeToTopic(
        [FromRoute] Guid topicId,
        [FromServices] ITrendService trendService,
        [FromServices] IAccountService accountService,
        HttpContext context)
    {
        var accountId = GetAccountId(context);
        if (accountId == null)
        {
            return Results.Unauthorized();
        }

        var topic = await trendService.GetTopicByIdAsync(topicId);
        if (topic == null)
        {
            return Results.NotFound(new { error = "Topic not found" });
        }

        var isSubscribed = await trendService.IsSubscribedToTopicAsync(accountId.Value, topicId);
        if (isSubscribed)
        {
            return Results.Ok(new { message = "Already subscribed to this topic" });
        }

        await trendService.SubscribeToTopicAsync(accountId.Value, topicId);
        return Results.Ok(new { message = "Subscribed to topic", topicId, topicName = topic.DisplayName });
    }

    /// <summary>
    /// DELETE /api/topics/{topicId}/subscribe
    /// Unsubscribe from a topic
    /// </summary>
    private static async Task<IResult> UnsubscribeFromTopic(
        [FromRoute] Guid topicId,
        [FromServices] ITrendService trendService,
        HttpContext context)
    {
        var accountId = GetAccountId(context);
        if (accountId == null)
        {
            return Results.Unauthorized();
        }

        var isSubscribed = await trendService.IsSubscribedToTopicAsync(accountId.Value, topicId);
        if (!isSubscribed)
        {
            return Results.Ok(new { message = "Not subscribed to this topic" });
        }

        await trendService.UnsubscribeFromTopicAsync(accountId.Value, topicId);
        return Results.Ok(new { message = "Unsubscribed from topic", topicId });
    }

    /// <summary>
    /// POST /api/trends/calculate
    /// Manually calculate a trend
    /// </summary>
    private static async Task<IResult> CalculateTrend(
        [FromServices] ITrendService trendService,
        [FromBody] CalculateTrendRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return Results.BadRequest(new { error = "Query is required" });
        }

        var trend = await trendService.CalculateTrendAsync(request.Query, request.Scope, request.CommunityId);
        
        return Results.Ok(new
        {
            trend.TrendId,
            trend.Type,
            trend.Query,
            trend.DisplayName,
            trend.Strength,
            trend.PostCount,
            trend.UniquePosters,
            trend.EngagementTotal,
            trend.Velocity,
            trend.IsActive
        });
    }

    /// <summary>
    /// POST /api/trends/process
    /// Manually trigger trend processing
    /// </summary>
    private static async Task<IResult> ProcessTrendsTick(
        [FromServices] TrendProcessingService trendProcessingService)
    {
        await trendProcessingService.ProcessTrendsTickAsync();
        return Results.Ok(new { message = "Trend processing completed" });
    }

    private static int? GetAccountId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("accountId")?.Value;
        
        if (int.TryParse(userIdClaim, out var accountId))
        {
            return accountId;
        }
        
        return null;
    }
}

/// <summary>
/// Request model for calculating a trend
/// </summary>
public class CalculateTrendRequest
{
    public string Query { get; set; } = string.Empty;
    public TrendScope Scope { get; set; } = TrendScope.Global;
    public int? CommunityId { get; set; }
}
