using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Responses;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class AccountController
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/account").WithTags("Account").RequireAuthorization();

        // Get current authenticated account
        group.MapGet("/me", async (ClaimsPrincipal user, IAccountService accountService) =>
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
            {
                return Results.Unauthorized();
            }

            var account = await accountService.GetByIdAsync(accountId);

            if (account == null)
            {
                return Results.NotFound(new ErrorResponse("Account not found"));
            }

            return Results.Ok(new AccountResponse(
                account.AccountId,
                account.Username,
                account.Profile?.DisplayName,
                account.Profile?.Bio,
                account.Profile?.AvatarUrl,
                account.AccountType.ToString(),
                account.Status.ToString(),
                account.CreatedAt
            ));
        });

        // Get public profile by account ID
        group.MapGet("/{accountId:guid}", async (Guid accountId, IAccountService accountService) =>
        {
            var account = await accountService.GetByAccountIdAsync(accountId);

            if (account == null)
            {
                return Results.NotFound(new ErrorResponse("Account not found"));
            }

            return Results.Ok(new PublicProfileResponse(
                account.AccountId,
                account.Username,
                account.Profile?.DisplayName ?? account.Username,
                account.Profile?.Bio,
                account.Profile?.AvatarUrl,
                account.AccountType.ToString()
            ));
        });

        // Get communities for the current authenticated account
        group.MapGet("/communities", async (ClaimsPrincipal user, ICommunityService communityService) =>
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out var accountId))
            {
                return Results.Unauthorized();
            }

            var communities = await communityService.GetAccountCommunitiesAsync(accountId);
            
            var response = new AccountCommunitiesResponse
            {
                Communities = communities.Select(c => new CommunitySummaryResponse
                {
                    CommunityId = c.CommunityId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    Topic = c.Topic,
                    Tags = c.Tags,
                    MemberCount = c.MemberCount,
                    PostCount = c.PostCount,
                    Visibility = c.Visibility.ToString(),
                    CreatedAt = c.CreatedAt,
                    Owner = c.OwnerAccount != null ? new CommunityOwnerInfo
                    {
                        AccountId = c.OwnerAccount.Id,
                        Username = c.OwnerAccount.Username,
                        DisplayName = c.OwnerAccount.Profile?.DisplayName ?? string.Empty,
                        AvatarUrl = c.OwnerAccount.Profile?.AvatarUrl
                    } : null
                })
            };
            
            return Results.Ok(response);
        });
    }
}
