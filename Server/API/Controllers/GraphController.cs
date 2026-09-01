using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Responses;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class GraphController
{
    public static void MapGraphEndpoints(this WebApplication app)
    {
        // Public endpoints - get followers/following
        app.MapGet("/api/accounts/{accountId:guid}/followers", GetFollowers)
            .WithTags("Graph")
            .WithName("GetFollowers");

        app.MapGet("/api/accounts/{accountId:guid}/following", GetFollowing)
            .WithTags("Graph")
            .WithName("GetFollowing");

        app.MapGet("/api/accounts/{accountId:guid}/relationship", GetRelationship)
            .WithTags("Graph")
            .WithName("GetRelationship")
            .RequireAuthorization();

        // Protected endpoints - graph actions
        app.MapPost("/api/accounts/{accountId:guid}/follow", Follow)
            .WithTags("Graph")
            .WithName("Follow")
            .RequireAuthorization();

        app.MapDelete("/api/accounts/{accountId:guid}/follow", Unfollow)
            .WithTags("Graph")
            .WithName("Unfollow")
            .RequireAuthorization();

        app.MapPost("/api/accounts/{accountId:guid}/block", Block)
            .WithTags("Graph")
            .WithName("Block")
            .RequireAuthorization();

        app.MapDelete("/api/accounts/{accountId:guid}/block", Unblock)
            .WithTags("Graph")
            .WithName("Unblock")
            .RequireAuthorization();

        app.MapPost("/api/accounts/{accountId:guid}/mute", Mute)
            .WithTags("Graph")
            .WithName("Mute")
            .RequireAuthorization();

        app.MapDelete("/api/accounts/{accountId:guid}/mute", Unmute)
            .WithTags("Graph")
            .WithName("Unmute")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetFollowers(
        Guid accountId,
        [FromQuery] int page,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var account = await accountService.GetByAccountIdAsync(accountId);
        if (account == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        page = page < 1 ? 1 : page;
        var pageSize = 20;

        var (items, totalCount) = await graphService.GetFollowersAsync(account.Id, page, pageSize);

        var accounts = items.Select(f => new AccountSummaryResponse(
            f.FollowerAccount?.AccountId ?? Guid.Empty,
            f.FollowerAccount?.Username ?? "Unknown",
            f.FollowerAccount?.Profile?.DisplayName ?? f.FollowerAccount?.Username ?? "Unknown",
            f.FollowerAccount?.Profile?.AvatarUrl,
            f.FollowerAccount?.AccountType.ToString() ?? "OrdinaryUser",
            f.CreatedAt
        ));

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Results.Ok(new PaginatedAccountsResponse(accounts, page, pageSize, totalCount, totalPages));
    }

    private static async Task<IResult> GetFollowing(
        Guid accountId,
        [FromQuery] int page,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var account = await accountService.GetByAccountIdAsync(accountId);
        if (account == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        page = page < 1 ? 1 : page;
        var pageSize = 20;

        var (items, totalCount) = await graphService.GetFollowingAsync(account.Id, page, pageSize);

        var accounts = items.Select(f => new AccountSummaryResponse(
            f.FollowedAccount?.AccountId ?? Guid.Empty,
            f.FollowedAccount?.Username ?? "Unknown",
            f.FollowedAccount?.Profile?.DisplayName ?? f.FollowedAccount?.Username ?? "Unknown",
            f.FollowedAccount?.Profile?.AvatarUrl,
            f.FollowedAccount?.AccountType.ToString() ?? "OrdinaryUser",
            f.CreatedAt
        ));

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Results.Ok(new PaginatedAccountsResponse(accounts, page, pageSize, totalCount, totalPages));
    }

    private static async Task<IResult> GetRelationship(
        Guid accountId,
        ClaimsPrincipal user,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        // Get authenticated user
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var currentAccountId))
        {
            return Results.Unauthorized();
        }

        // Get target account
        var targetAccount = await accountService.GetByAccountIdAsync(accountId);
        if (targetAccount == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        var (isFollowing, isFollowedBy, isBlocking, isBlockedBy, isMuting) = 
            await graphService.GetRelationshipAsync(currentAccountId, targetAccount.Id);

        var isMutual = isFollowing && isFollowedBy;

        return Results.Ok(new RelationshipResponse(
            accountId,
            isFollowing,
            isFollowedBy,
            isMutual,
            isBlocking,
            isBlockedBy,
            isMuting
        ));
    }

    private static async Task<IResult> Follow(
        Guid accountId,
        ClaimsPrincipal user,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var currentAccountId))
        {
            return Results.Unauthorized();
        }

        var targetAccount = await accountService.GetByAccountIdAsync(accountId);
        if (targetAccount == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        try
        {
            var follow = await graphService.FollowAsync(currentAccountId, targetAccount.Id);
            return Results.Ok(new GraphActionResponse(true, "Successfully followed"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> Unfollow(
        Guid accountId,
        ClaimsPrincipal user,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var currentAccountId))
        {
            return Results.Unauthorized();
        }

        var targetAccount = await accountService.GetByAccountIdAsync(accountId);
        if (targetAccount == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        var result = await graphService.UnfollowAsync(currentAccountId, targetAccount.Id);
        
        if (result)
        {
            return Results.Ok(new GraphActionResponse(true, "Successfully unfollowed"));
        }
        
        return Results.Ok(new GraphActionResponse(false, "Not following this account"));
    }

    private static async Task<IResult> Block(
        Guid accountId,
        ClaimsPrincipal user,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var currentAccountId))
        {
            return Results.Unauthorized();
        }

        var targetAccount = await accountService.GetByAccountIdAsync(accountId);
        if (targetAccount == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        try
        {
            var block = await graphService.BlockAsync(currentAccountId, targetAccount.Id);
            return Results.Ok(new GraphActionResponse(true, "Successfully blocked"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> Unblock(
        Guid accountId,
        ClaimsPrincipal user,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var currentAccountId))
        {
            return Results.Unauthorized();
        }

        var targetAccount = await accountService.GetByAccountIdAsync(accountId);
        if (targetAccount == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        var result = await graphService.UnblockAsync(currentAccountId, targetAccount.Id);
        
        if (result)
        {
            return Results.Ok(new GraphActionResponse(true, "Successfully unblocked"));
        }
        
        return Results.Ok(new GraphActionResponse(false, "Not blocking this account"));
    }

    private static async Task<IResult> Mute(
        Guid accountId,
        ClaimsPrincipal user,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var currentAccountId))
        {
            return Results.Unauthorized();
        }

        var targetAccount = await accountService.GetByAccountIdAsync(accountId);
        if (targetAccount == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        try
        {
            var mute = await graphService.MuteAsync(currentAccountId, targetAccount.Id);
            return Results.Ok(new GraphActionResponse(true, "Successfully muted"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> Unmute(
        Guid accountId,
        ClaimsPrincipal user,
        IAccountService accountService,
        ISocialGraphService graphService)
    {
        var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var currentAccountId))
        {
            return Results.Unauthorized();
        }

        var targetAccount = await accountService.GetByAccountIdAsync(accountId);
        if (targetAccount == null)
        {
            return Results.NotFound(new ErrorResponse("Account not found"));
        }

        var result = await graphService.UnmuteAsync(currentAccountId, targetAccount.Id);
        
        if (result)
        {
            return Results.Ok(new GraphActionResponse(true, "Successfully unmuted"));
        }
        
        return Results.Ok(new GraphActionResponse(false, "Not muting this account"));
    }
}
