using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.API.Controllers;

/// <summary>
/// Admin endpoints for AI provider configuration and testing.
/// </summary>
public static class AiController
{
    public static void MapAiEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/ai")
            .WithTags("Admin - AI")
            .RequireAuthorization(); // Requires authenticated user

        // Get current AI configuration
        group.MapGet("/config", async ([FromServices] IAiProviderService aiService, ILogger<Program> logger) =>
        {
            var config = await aiService.GetConfigAsync();
            logger.LogDebug("AI config requested");
            return Results.Ok(config);
        });

        // Update AI configuration
        group.MapPut("/config", async (
            [FromServices] IAiProviderService aiService,
            [FromBody] UpdateAiConfigRequest request,
            ILogger<Program> logger) =>
        {
            try
            {
                var config = await aiService.UpdateConfigAsync(request);
                logger.LogInformation("AI configuration updated: Provider={Provider}, Model={Model}, IsEnabled={IsEnabled}",
                    config.Provider, config.Model, config.IsEnabled);
                return Results.Ok(config);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning("AI configuration update rejected: {Error}", ex.Message);
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // Test AI connection
        group.MapPost("/test", async (
            [FromServices] IAiProviderService aiService,
            [FromBody] AiTestRequest? testRequest,
            ILogger<Program> logger) =>
        {
            var prompt = testRequest?.Prompt ?? "Say 'Hello, this is a test!' and nothing else.";
            logger.LogInformation("AI connection test requested");
            
            var result = await aiService.TestConnectionAsync(prompt);
            
            if (result.Success)
            {
                logger.LogInformation("AI connection test succeeded in {DurationMs}ms", result.DurationMs);
            }
            else
            {
                logger.LogWarning("AI connection test failed: {Error}", result.Message);
            }
            
            return Results.Ok(result);
        });
    }
}

/// <summary>
/// Request body for AI test endpoint.
/// </summary>
public class AiTestRequest
{
    /// <summary>
    /// Optional custom prompt to test with.
    /// </summary>
    public string? Prompt { get; set; }
}
