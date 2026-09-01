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
        
        // Load feed scoring configuration from appsettings
        var feedScoringConfig = new FeedScoringConfig();
        configuration.GetSection("FeedScoring").Bind(feedScoringConfig);
        services.AddSingleton(feedScoringConfig);
        services.AddSingleton<IFeedScoringService, FeedScoringService>();
        services.AddScoped<IFeedCacheService, FeedCacheService>();
        
        // Register notification service
        services.AddScoped<INotificationService, NotificationService>();
        
        // Register community service
        services.AddScoped<ICommunityService, CommunityService>();
        services.AddScoped<ICommunitySeedService, CommunitySeedService>();
        services.AddScoped<ITopicSeedService, TopicSeedService>();
        services.AddScoped<AiConfigSeederService>();
        
        // Register event services
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IEventGenerationService, EventGenerationService>();
        
        // Register causal tracking service
        services.AddScoped<ICausalTrackingService, CausalTrackingService>();
        
        // Register virality configuration
        var viralityConfig = new ViralityConfig
        {
            Enabled = configuration.GetValue<bool>("Virality:Enabled", true),
            TrendingThreshold = configuration.GetValue<int>("Virality:TrendingThreshold", 50),
            PopularThreshold = configuration.GetValue<int>("Virality:PopularThreshold", 200),
            ViralThreshold = configuration.GetValue<int>("Virality:ViralThreshold", 1000),
            MassivelyViralThreshold = configuration.GetValue<int>("Virality:MassivelyViralThreshold", 10000),
            ViralVelocityMin = configuration.GetValue<float>("Virality:ViralVelocityMin", 10),
            ViralWindowHours = configuration.GetValue<int>("Virality:ViralWindowHours", 24),
            ProcessingIntervalMinutes = configuration.GetValue<int>("Virality:ProcessingIntervalMinutes", 5),
            MaxPostsPerTick = configuration.GetValue<int>("Virality:MaxPostsPerTick", 100),
            ActivePostDays = configuration.GetValue<int>("Virality:ActivePostDays", 7),
            DeclineVelocityDropPercent = configuration.GetValue<float>("Virality:DeclineVelocityDropPercent", 0.7f),
            BaseFollowerGainOnViral = configuration.GetValue<int>("Virality:BaseFollowerGainOnViral", 10),
            BaseFameGainOnViral = configuration.GetValue<float>("Virality:BaseFameGainOnViral", 5.0f)
        };
        services.AddSingleton(viralityConfig);
        services.AddScoped<IViralityService, ViralityService>();
        
        // Register virality background processing service
        services.AddHostedService<ViralityProcessingService>();
        
        // Register trend configuration
        var trendConfig = new TrendConfig
        {
            Enabled = configuration.GetValue<bool>("Trends:Enabled", true),
            ProcessingIntervalMinutes = configuration.GetValue<int>("Trends:ProcessingIntervalMinutes", 15),
            TrendWindowHours = configuration.GetValue<int>("Trends:TrendWindowHours", 24),
            MinPostsForTrend = configuration.GetValue<int>("Trends:MinPostsForTrend", 10),
            MaxTrendingHashtags = configuration.GetValue<int>("Trends:MaxTrendingHashtags", 20),
            TrendDurationHours = configuration.GetValue<int>("Trends:TrendDurationHours", 24),
            PropagationMultiplier = configuration.GetValue<double>("Trends:PropagationMultiplier", 1.0),
            TopicPostCountDays = configuration.GetValue<int>("Trends:TopicPostCountDays", 7)
        };
        services.AddSingleton(trendConfig);
        services.AddScoped<ITrendService, TrendService>();
        
        // Register trend background processing service
        services.AddHostedService<TrendProcessingService>();
        
        // Register offline simulation configuration
        var offlineConfig = new OfflineSimulationConfig
        {
            Enabled = configuration.GetValue<bool>("OfflineSimulation:Enabled", true),
            MinOfflineHoursBeforeSimulation = configuration.GetValue<int>("OfflineSimulation:MinOfflineHoursBeforeSimulation", 1),
            TicksPerHour = configuration.GetValue<int>("OfflineSimulation:TicksPerHour", 10),
            MaxTicksPerSession = configuration.GetValue<int>("OfflineSimulation:MaxTicksPerSession", 1000),
            MinTicksToSimulate = configuration.GetValue<int>("OfflineSimulation:MinTicksToSimulate", 5),
            EventProbabilityMultiplier = configuration.GetValue<double>("OfflineSimulation:EventProbabilityMultiplier", 0.5)
        };
        services.AddSingleton(offlineConfig);
        services.AddScoped<IOfflineSimulationService, OfflineSimulationService>();
        
        // Register NPC services
        services.AddScoped<INpcService, NpcService>();
        services.AddScoped<INpcSimulationService, NpcSimulationService>();
        services.AddScoped<INpcPopulationService, NpcPopulationService>();
        
        // Register NPC behavior services
        services.AddSingleton<IContentRelevanceService, ContentRelevanceService>();
        services.AddSingleton<ContentGeneratorService>(); // Template fallback
        services.AddSingleton<AiPromptBuilder>();
        services.AddSingleton<INpcDecisionService, NpcDecisionService>();
        services.AddScoped<INpcBehaviorService, NpcBehaviorService>();
        services.AddScoped<INpcSocialGraphService, NpcSocialGraphService>();
        
        // Register AI services
        services.AddHttpClient("AIProvider"); // Named HTTP client for AI providers
        services.AddScoped<IAiProviderService, AiProviderService>();
        services.AddSingleton<IContentGeneratorService>(sp =>
        {
            var aiService = sp.GetRequiredService<IAiProviderService>();
            var templateGenerator = sp.GetRequiredService<ContentGeneratorService>();
            var promptBuilder = sp.GetRequiredService<AiPromptBuilder>();
            var logger = sp.GetRequiredService<ILogger<AiContentGeneratorService>>();
            var simulationState = sp.GetService<ISimulationStateService>();
            return new AiContentGeneratorService(aiService, templateGenerator, promptBuilder, logger, simulationState);
        });
        
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
            ExplorationRate = 0.3,
            EnableCommunityBehavior = true,
            MaxCommunityJoinsPerTick = 1,
            MaxRelevantCommunities = 10
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
        
        // Register event configuration
        var eventConfig = new EventConfig
        {
            Enabled = configuration.GetValue<bool>("EventSystem:Enabled", true),
            EventGenerationIntervalTicks = configuration.GetValue<int>("EventSystem:EventGenerationIntervalTicks", 5),
            MaxActiveEvents = configuration.GetValue<int>("EventSystem:MaxActiveEvents", 20),
            EventDurationHours = configuration.GetValue<int>("EventSystem:EventDurationHours", 24),
            AccountEventCooldownHours = configuration.GetValue<int>("EventSystem:AccountEventCooldownHours", 2),
            AutoApproveEvents = configuration.GetValue<bool>("EventSystem:AutoApproveEvents", true),
            MaxEventsPerHour = configuration.GetValue<int>("EventSystem:MaxEventsPerHour", 10)
        };
        services.AddSingleton(eventConfig);

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
