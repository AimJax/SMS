using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for seeding AI provider configuration on startup
/// </summary>
public class AiConfigSeederService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiConfigSeederService> _logger;

    public AiConfigSeederService(
        AppDbContext context, 
        IConfiguration configuration,
        ILogger<AiConfigSeederService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedIfNeededAsync()
    {
        try
        {
            var exists = await _context.AiProviderConfigs.AnyAsync();
            if (exists)
            {
                _logger.LogDebug("AI provider config already exists, skipping seed");
                return;
            }

            var provider = _configuration["AiProvider:Provider"] ?? "Generic";
            var baseUrl = _configuration["AiProvider:BaseUrl"] ?? "http://localhost:11434";
            var model = _configuration["AiProvider:Model"] ?? "qwen3-4b";
            var apiKey = _configuration["AiProvider:ApiKey"] ?? "";
            var isEnabled = _configuration.GetValue<bool>("AiProvider:IsEnabled", true);
            var timeoutSeconds = _configuration.GetValue<int>("AiProvider:TimeoutSeconds", 120);

            var config = new Domain.Entities.AiProviderConfig
            {
                Provider = provider,
                BaseUrl = baseUrl,
                Model = model,
                ApiKey = apiKey,
                IsEnabled = isEnabled,
                TimeoutSeconds = timeoutSeconds,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AiProviderConfigs.Add(config);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("AI provider seeded: {Provider} / {Model}", provider, model);
            Console.WriteLine($"AI provider seeded: {provider} / {model}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed AI provider config");
            Console.WriteLine($"Warning: Failed to seed AI provider config: {ex.Message}");
        }
    }
}
