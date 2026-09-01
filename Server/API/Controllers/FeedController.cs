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
        [FromQuery] int pageSize = 20)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Results.Unauthorized();
        }

        // Validate page size
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var (items, nextCursor) = await feedService.GetFeedAsync(accountId, cursor, pageSize);

        var response = items.Select(item => new FeedItemResponse(
            item.Post.PostId,
            item.AuthorAccount.AccountId,
            item.AuthorAccount.Username,
            item.AuthorProfile?.DisplayName ?? item.AuthorAccount.Username,
            item.AuthorProfile?.AvatarUrl,
            item.Post.Content,
            item.Post.CreatedAt,
            item.LikeCount,
            item.CommentCount,
            item.IsLikedByCurrentUser
        ));

        return Results.Ok(new FeedResponse(response, nextCursor, pageSize));
    }
}
