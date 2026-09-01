using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class RumorsControllerExtensions
{
    public static void MapRumorEndpoints(this WebApplication app)
    {
        // Rumor endpoints
        app.MapGet("/api/rumors", async (IRumorService rumorService, int count = 20) =>
        {
            var rumors = await rumorService.GetNotableRumorsAsync(count);
            return Results.Ok(rumors);
        });

        app.MapGet("/api/rumors/{id:guid}", async (Guid id, IRumorService rumorService) =>
        {
            var rumor = await rumorService.GetRumorByIdAsync(id);
            if (rumor == null) return Results.NotFound();
            return Results.Ok(rumor);
        });

        app.MapGet("/api/rumors/status/{status}", async (string status, IRumorService rumorService, int count = 20) =>
        {
            if (!Enum.TryParse<RumorTruthStatus>(status, true, out var truthStatus))
            {
                return Results.BadRequest($"Invalid status: {status}");
            }
            var rumors = await rumorService.GetRumorsByStatusAsync(truthStatus, count);
            return Results.Ok(rumors);
        });

        app.MapGet("/api/communities/{id:int}/rumors", async (int id, IRumorService rumorService, int count = 20) =>
        {
            var rumors = await rumorService.GetCommunityRumorsAsync(id, count);
            return Results.Ok(rumors);
        });

        app.MapPost("/api/rumors/{id:guid}/status", async (Guid id, [FromBody] UpdateRumorStatusRequest request, IRumorService rumorService) =>
        {
            if (!Enum.TryParse<RumorTruthStatus>(request.Status, true, out var truthStatus))
            {
                return Results.BadRequest($"Invalid status: {request.Status}");
            }
            var rumor = await rumorService.UpdateRumorStatusAsync(id, truthStatus);
            if (rumor == null) return Results.NotFound();
            return Results.Ok(rumor);
        });

        // Belief endpoints
        app.MapGet("/api/me/rumors/beliefs", async (HttpContext context, IRumorService rumorService) =>
        {
            var accountId = GetAccountId(context);
            if (accountId == null) return Results.Unauthorized();
            
            var beliefs = await rumorService.GetAccountBeliefsAsync(accountId.Value);
            return Results.Ok(beliefs);
        });

        app.MapGet("/api/rumors/{id:guid}/belief", async (Guid id, HttpContext context, IRumorService rumorService) =>
        {
            var accountId = GetAccountId(context);
            if (accountId == null) return Results.Unauthorized();
            
            var belief = await rumorService.GetAccountBeliefAsync(accountId.Value, id);
            if (belief == null) return Results.NotFound();
            return Results.Ok(belief);
        });

        app.MapPost("/api/rumors/{id:guid}/believe", async (Guid id, [FromBody] UpdateBeliefRequest request, HttpContext context, IRumorService rumorService) =>
        {
            var accountId = GetAccountId(context);
            if (accountId == null) return Results.Unauthorized();
            
            if (!Enum.TryParse<RumorTruthStatus>(request.Belief, true, out var belief))
            {
                return Results.BadRequest($"Invalid belief: {request.Belief}");
            }
            
            var result = await rumorService.UpdateBeliefAsync(accountId.Value, id, belief, request.Confidence);
            if (result == null) return Results.NotFound();
            return Results.Ok(result);
        });

        // Evidence endpoints
        app.MapGet("/api/rumors/{id:guid}/evidence", async (Guid id, IRumorService rumorService) =>
        {
            var evidence = await rumorService.GetRumorEvidenceAsync(id);
            return Results.Ok(evidence);
        });

        app.MapPost("/api/rumors/{id:guid}/evidence", async (Guid id, [FromBody] AddEvidenceRequest request, HttpContext context, IRumorService rumorService) =>
        {
            var accountId = GetAccountId(context);
            var evidence = await rumorService.AddEvidenceAsync(id, accountId, request.Description, request.SupportsRumor, request.SourceUrl);
            if (evidence == null) return Results.NotFound();
            return Results.Ok(evidence);
        });

        // Processing endpoints
        app.MapPost("/api/rumors/process", async (IRumorService rumorService) =>
        {
            await rumorService.ProcessRumorsTickAsync();
            return Results.Ok(new { status = "processed" });
        });
    }

    private static int? GetAccountId(HttpContext context)
    {
        var claim = context.User.FindFirst("account_id") ?? context.User.FindFirst("sub");
        if (claim != null && int.TryParse(claim.Value, out var accountId))
        {
            return accountId;
        }
        return null;
    }
}

public class UpdateRumorStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateBeliefRequest
{
    public string Belief { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.5;
}

public class AddEvidenceRequest
{
    public string Description { get; set; } = string.Empty;
    public bool SupportsRumor { get; set; } = true;
    public string? SourceUrl { get; set; }
}
