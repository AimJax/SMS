using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.API.Controllers;

/// <summary>
/// Admin endpoints for NPC simulation control and monitoring
/// </summary>
public static class SimulationController
{
    public static void MapSimulationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/simulation")
            .WithTags("Admin - Simulation")
            .RequireAuthorization(); // Requires authenticated user

        // Get simulation status
        group.MapGet("/status", ([FromServices] ISimulationStateService stateService) =>
        {
            var status = stateService.GetStatus();
            return Results.Ok(status);
        });

        // Pause simulation
        group.MapPost("/pause", ([FromServices] ISimulationStateService stateService, ILogger<Program> logger) =>
        {
            stateService.Pause();
            logger.LogInformation("Simulation paused via admin endpoint");
            return Results.Ok(new { message = "Simulation paused" });
        });

        // Resume simulation
        group.MapPost("/resume", ([FromServices] ISimulationStateService stateService, ILogger<Program> logger) =>
        {
            stateService.Resume();
            logger.LogInformation("Simulation resumed via admin endpoint");
            return Results.Ok(new { message = "Simulation resumed" });
        });
    }
}
