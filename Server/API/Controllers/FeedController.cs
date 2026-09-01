using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Responses;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class FeedController
{
    public static void MapFeedEndpoints(this WebApplication app)
    {
        // Get feed (authenticated)
        app.MapGet("/api/feed", GetFeed)
            .WithTags("Feed")
            .WithName("GetFeed")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetFeed(
        ClaimsPrincipal user,
        IFeedService feedService,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeDiscovery = true,
        [FromQuery] double? echoStrength = null)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        // Validate page size
        pageSize = Math.Clamp(pageSize, 1, 50);
        
        // Validate echo strength
        if (echoStrength.HasValue)
        {
            echoStrength = Math.Clamp(echoStrength.Value, 0.0, 1.0);
        }

        // Use advanced feed
        var response = await feedService.GetAdvancedFeedAsync(
            accountId, 
            cursor, 
            pageSize,
            includeDiscovery,
            echoStrength);

        // Map to response format
        var feedResponse = new FeedResponse(
            response.Items.Select(item => new FeedItemResponse(
                item.PostId,
                item.AuthorAccountId,
                item.AuthorUsername,
                item.AuthorDisplayName,
                item.AuthorAvatarUrl,
                item.Content,
                item.CreatedAt,
                item.LikeCount,
                item.CommentCount,
                item.IsLikedByCurrentUser
            )),
            response.NextCursor,
            response.PageSize);

        return Results.Ok(feedResponse);
    }
}
