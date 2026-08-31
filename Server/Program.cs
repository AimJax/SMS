using SocialMediaSimulator.Server.API.Controllers;
using SocialMediaSimulator.Server.API.Middleware;
using SocialMediaSimulator.Server.Extensions;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on a specific port
builder.WebHost.UseUrls("http://0.0.0.0:5225");

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add persistence
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database initialized successfully.");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add exception handling middleware
app.UseExceptionHandling();

// Health endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

// Map API endpoints
app.MapPersistenceTestEndpoints();

app.Run();
