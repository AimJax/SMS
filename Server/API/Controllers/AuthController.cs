using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Requests;
using SocialMediaSimulator.Server.Contracts.Responses;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class AuthController
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        // Register
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IAccountService accountService,
            IJwtService jwtService) =>
        {
            // Validate username availability
            if (!await accountService.IsUsernameAvailableAsync(request.Username))
            {
                return Results.BadRequest(new ErrorResponse("Username is already taken"));
            }

            try
            {
                var account = await accountService.RegisterAsync(
                    request.Username,
                    request.Password,
                    request.DisplayName,
                    request.Bio,
                    request.Email);

                var token = jwtService.GenerateToken(account);

                return Results.Created($"/api/accounts/{account.AccountId}", new AuthResponse(
                    token,
                    new AccountResponse(
                        account.AccountId,
                        account.Username,
                        account.Profile?.DisplayName,
                        account.Profile?.Bio,
                        account.Profile?.AvatarUrl,
                        account.AccountType.ToString(),
                        account.Status.ToString(),
                        account.CreatedAt
                    )
                ));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ErrorResponse(ex.Message));
            }
        });

        // Login
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IAccountService accountService,
            IJwtService jwtService) =>
        {
            var account = await accountService.AuthenticateAsync(request.Username, request.Password);

            if (account == null)
            {
                return Results.Unauthorized();
            }

            var token = jwtService.GenerateToken(account);

            return Results.Ok(new AuthResponse(
                token,
                new AccountResponse(
                    account.AccountId,
                    account.Username,
                    account.Profile?.DisplayName,
                    account.Profile?.Bio,
                    account.Profile?.AvatarUrl,
                    account.AccountType.ToString(),
                    account.Status.ToString(),
                    account.CreatedAt
                )
            ));
        });
    }
}
