using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.Infrastructure;

/// <summary>
/// Background service for processing news
/// </summary>
public class NewsProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly NewsConfig _config;
    private readonly ILogger<NewsProcessingService> _logger;

    public NewsProcessingService(
        IServiceProvider serviceProvider,
        NewsConfig config,
        ILogger<NewsProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("NewsProcessingService is disabled");
            return;
        }

        _logger.LogInformation("NewsProcessingService starting. Processing interval: {Interval} minutes", 
            _config.ProcessingIntervalMinutes);

        // Wait a bit for other services to start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var newsService = scope.ServiceProvider.GetRequiredService<INewsService>();
                await newsService.ProcessNewsTickAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in news processing tick");
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.ProcessingIntervalMinutes), stoppingToken);
        }
    }
}
