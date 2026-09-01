using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Services;
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
