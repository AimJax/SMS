using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Responses;

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
    }
}
