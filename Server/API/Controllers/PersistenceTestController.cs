using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Requests;
using SocialMediaSimulator.Server.Contracts.Responses;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class PersistenceTestController
{
    public static void MapPersistenceTestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/persistence-test").WithTags("PersistenceTest");

        group.MapPost("", async (IPersistenceTestService service, CreatePersistenceTestRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Value))
            {
                return Results.BadRequest(new { error = "Value is required" });
            }

            var result = await service.CreateAsync(request.Value);
            return Results.Created(
                $"/api/persistence-test/{result.Id}",
                new PersistenceTestResponse(result.Id, result.Value, result.CreatedAt));
        });

        group.MapGet("/{id:int}", async (IPersistenceTestService service, int id) =>
        {
            var result = await service.GetByIdAsync(id);
            return result is not null
                ? Results.Ok(new PersistenceTestResponse(result.Id, result.Value, result.CreatedAt))
                : Results.NotFound(new { error = $"Record with id {id} not found" });
        });

        group.MapGet("", async (IPersistenceTestService service) =>
        {
            var results = await service.GetAllAsync();
            return Results.Ok(results.Select(r => new PersistenceTestResponse(r.Id, r.Value, r.CreatedAt)));
        });
    }
}
