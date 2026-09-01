using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;

namespace SocialMediaSimulator.Server.Infrastructure;

/// <summary>
/// Background service that runs the NPC simulation loop.
/// Continuously processes simulation ticks at configured intervals.
/// </summary>
public class NpcSimulationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISimulationStateService _stateService;
    private readonly ILogger<NpcSimulationHostedService> _logger;
    private readonly SimulationConfig _config;

    public NpcSimulationHostedService(
        IServiceProvider serviceProvider,
        ISimulationStateService stateService,
        ILogger<NpcSimulationHostedService> logger,
        SimulationConfig config)
    {
        _serviceProvider = serviceProvider;
        _stateService = stateService;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stateService.Initialize(_config);
        
        if (!_config.Enabled)
        {
            _logger.LogInformation("NPC Simulation background service is disabled via configuration");
            return;
        }

        _logger.LogInformation("NPC Simulation background service starting. Tick interval: {IntervalSeconds}s, Max NPCs per tick: {MaxNpcs}",
            _config.TickIntervalSeconds, _config.MaxNpcsPerTick);

        var tickNumber = 0L;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Check if we can start a tick
                if (!_stateService.CanStartTick())
                {
                    // If paused or disabled, just wait and check again
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                // Record tick start
                tickNumber++;
                _stateService.TickStarted();
                
                _logger.LogDebug("Tick {TickNumber} started at {Time}", tickNumber, DateTime.UtcNow);

                // Create a new scope for this tick
                using (var scope = _serviceProvider.CreateScope())
                {
                    var simulationService = scope.ServiceProvider.GetRequiredService<INpcSimulationService>();
                    
                    var startTime = DateTime.UtcNow;
                    int npcsProcessed = 0;
                    
                    try
                    {
                        // Process the tick
                        var result = await simulationService.ProcessTickAsync(_config.MaxNpcsPerTick);
                        npcsProcessed = result.NpcsProcessed;
                        
                        // Record social graph activity
                        _stateService.RecordSocialGraphActivity(result.FollowsCreated, result.UnfollowsCreated);
                        
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        _stateService.TickCompleted(npcsProcessed, duration);
                        
                        _logger.LogInformation("Tick {TickNumber} completed. NPCs processed: {NpcsProcessed}, Follows: {Follows}, Unfollows: {Unfollows}, Duration: {Duration:F2}ms",
                            tickNumber, npcsProcessed, result.FollowsCreated, result.UnfollowsCreated, duration);
                    }
                    catch (Exception ex)
                    {
                        // Log the failure but don't crash the service
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        _logger.LogError(ex, "Tick {TickNumber} failed after {Duration:F2}ms. Error: {Error}",
                            tickNumber, duration, ex.Message);
                        
                        _stateService.TickFailed();
                    }
                }

                // Wait for the configured interval before next tick
                await Task.Delay(TimeSpan.FromSeconds(_config.TickIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown - exit gracefully
                _logger.LogInformation("NPC Simulation background service stopping (cancellation requested)");
                break;
            }
            catch (Exception ex)
            {
                // Unexpected error - log and continue
                _logger.LogError(ex, "Unexpected error in NPC Simulation background service. Will retry in 10 seconds.");
                
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("NPC Simulation background service stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NPC Simulation background service is shutting down gracefully...");
        
        // If a tick is in progress, let it complete or timeout
        var status = _stateService.GetStatus();
        if (status.IsTickInProgress)
        {
            _logger.LogWarning("A tick is still in progress. Waiting up to 30 seconds for completion...");
            
            // Wait for the tick to complete (with timeout)
            var timeout = DateTime.UtcNow.AddSeconds(30);
            while (_stateService.GetStatus().IsTickInProgress && DateTime.UtcNow < timeout)
            {
                await Task.Delay(500, cancellationToken);
            }
            
            if (_stateService.GetStatus().IsTickInProgress)
            {
                _logger.LogWarning("Tick did not complete in time. Proceeding with shutdown.");
            }
        }
        
        await base.StopAsync(cancellationToken);
        
        _logger.LogInformation("NPC Simulation background service shutdown complete");
    }
}
