namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Background service that processes trends periodically
/// </summary>
public class TrendProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TrendConfig _config;
    private readonly ILogger<TrendProcessingService> _logger;

    public TrendProcessingService(
        IServiceProvider serviceProvider,
        TrendConfig config,
        ILogger<TrendProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("TrendProcessingService is disabled");
            return;
        }

        _logger.LogInformation("TrendProcessingService starting. Processing interval: {Interval} minutes",
            _config.ProcessingIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTrendsTickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing trends tick");
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.ProcessingIntervalMinutes), stoppingToken);
        }
    }

    public async Task ProcessTrendsTickAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var trendService = scope.ServiceProvider.GetRequiredService<ITrendService>();

        var startTime = DateTime.UtcNow;
        _logger.LogDebug("Starting trend processing tick at {Time}", startTime);

        await trendService.ProcessTrendsTickAsync();

        var elapsed = DateTime.UtcNow - startTime;
        _logger.LogInformation("Trend tick completed in {Elapsed}ms", elapsed.TotalMilliseconds);
    }
}
