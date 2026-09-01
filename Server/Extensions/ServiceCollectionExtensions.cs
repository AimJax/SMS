using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Infrastructure;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register application services
        services.AddScoped<IPersistenceTestService, PersistenceTestService>();
        
        // Register account services
        services.AddScoped<IAccountService, AccountService>();
        
        // Register JWT service
        services.AddSingleton<IJwtService, JwtService>();
        
        // Register social graph service
        services.AddScoped<ISocialGraphService, SocialGraphService>();
        
        // Register post service
        services.AddScoped<IPostService, PostService>();
        
        // Register feed service
        services.AddScoped<IFeedService, FeedService>();
        
        // Register NPC services
        services.AddScoped<INpcService, NpcService>();
        services.AddScoped<INpcSimulationService, NpcSimulationService>();
        services.AddScoped<INpcPopulationService, NpcPopulationService>();
        
        // Register NPC behavior services
        services.AddSingleton<IContentRelevanceService, ContentRelevanceService>();
        services.AddSingleton<IContentGeneratorService, ContentGeneratorService>();
        services.AddSingleton<INpcDecisionService, NpcDecisionService>();
        services.AddScoped<INpcBehaviorService, NpcBehaviorService>();
        services.AddScoped<INpcSocialGraphService, NpcSocialGraphService>();
        
        // Register behavior configuration
        services.AddSingleton<NpcBehaviorConfig>(sp => new NpcBehaviorConfig
        {
            MaxCandidateAccounts = 50,
            MaxCandidatePosts = 30,
            BaseActionProbability = 0.7,
            PostCooldownSeconds = 300,
            MaxFollowsPerTick = 2,
            MaxLikesPerTick = 5,
            MaxCommentsPerTick = 3,
            MaxUnfollowsPerTick = 1,
            RecentPostsHours = 24,
            MaxFollowingBeforeUnfollow = 200,
            EnableExploration = true,
            ExplorationRate = 0.3
        });

        // Register simulation configuration
        var simulationEnabled = configuration.GetValue<bool>("Simulation:Enabled", true);
        var tickInterval = configuration.GetValue<int>("Simulation:TickIntervalSeconds", 10);
        var maxNpcsPerTick = configuration.GetValue<int>("Simulation:MaxNpcsPerTick", 100);
        
        // Validate tick interval
        if (tickInterval < SimulationConfig.MinTickIntervalSeconds)
            tickInterval = SimulationConfig.MinTickIntervalSeconds;
        if (tickInterval > SimulationConfig.MaxTickIntervalSeconds)
            tickInterval = SimulationConfig.MaxTickIntervalSeconds;
        
        services.AddSingleton(new SimulationConfig
        {
            Enabled = simulationEnabled,
            TickIntervalSeconds = tickInterval,
            MaxNpcsPerTick = maxNpcsPerTick,
            DetailedLogging = configuration.GetValue<bool>("Simulation:DetailedLogging", false)
        });

        // Register simulation state service (singleton for state sharing)
        services.AddSingleton<ISimulationStateService, SimulationStateService>();

        // Register hosted background service
        services.AddHostedService<NpcSimulationHostedService>();

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(30);
            });
        });

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
