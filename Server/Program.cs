using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SocialMediaSimulator.Server.API.Controllers;
using SocialMediaSimulator.Server.API.Middleware;
using SocialMediaSimulator.Server.Extensions;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using SocialMediaSimulator.Server.Application.Services;

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

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SocialMediaSimulator";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SocialMediaSimulator";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

builder.Services.AddAuthorization();

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

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

// Map API endpoints
app.MapPersistenceTestEndpoints();
app.MapAuthEndpoints();
app.MapAccountEndpoints();
app.MapGraphEndpoints();
app.MapPostEndpoints();
app.MapFeedEndpoints();
app.MapSimulationEndpoints();
app.MapAiEndpoints();
app.MapNotificationEndpoints();
app.MapCommunityEndpoints();
app.MapEventEndpoints();
app.MapCausalityEndpoints();
app.MapOfflineEndpoints();
app.MapViralityEndpoints();
app.MapTrendEndpoints();

// Seed topics on startup
using (var scope = app.Services.CreateScope())
{
    var topicSeedService = scope.ServiceProvider.GetRequiredService<ITopicSeedService>();
    if (!await topicSeedService.TopicsExistAsync())
    {
        var result = await topicSeedService.SeedTopicsAsync();
        if (result.Success)
        {
            Console.WriteLine($"Seeded {result.TopicsCreated} topics.");
        }
    }
}

app.Run();
