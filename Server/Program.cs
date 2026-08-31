using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Data;
using SocialMediaSimulator.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on a specific port
builder.WebHost.UseUrls("http://0.0.0.0:5225");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure SQLite database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register application services
builder.Services.AddScoped<PersistenceTestService>();

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database initialized successfully.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

// Persistence test endpoints
app.MapPost("/api/persistence-test", async (PersistenceTestService service, CreatePersistenceTestRequest request) =>
{
    var result = await service.CreateAsync(request.Value);
    return Results.Created($"/api/persistence-test/{result.Id}", result);
});

app.MapGet("/api/persistence-test/{id:int}", async (PersistenceTestService service, int id) =>
{
    var result = await service.GetByIdAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapGet("/api/persistence-test", async (PersistenceTestService service) =>
{
    var results = await service.GetAllAsync();
    return Results.Ok(results);
});

app.Run();
