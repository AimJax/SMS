using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Background service that processes virality updates periodically
/// </summary>
public class ViralityProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ViralityConfig _config;
    private readonly ILogger<ViralityProcessingService> _logger;

    public ViralityProcessingService(
        IServiceProvider serviceProvider,
        ViralityConfig config,
        ILogger<ViralityProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("ViralityProcessingService is disabled");
            return;
        }

        _logger.LogInformation("ViralityProcessingService starting. Processing interval: {Interval} minutes",
            _config.ProcessingIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessViralityTickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing virality tick");
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.ProcessingIntervalMinutes), stoppingToken);
        }
    }

    public async Task ProcessViralityTickAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var viralityService = scope.ServiceProvider.GetRequiredService<IViralityService>();

        var startTime = DateTime.UtcNow;
        _logger.LogDebug("Starting virality processing tick at {Time}", startTime);

        // Get posts that need virality updates
        // - Posts created in last N days (ActivePostDays)
        // - Posts that are not deleted
        var cutoffTime = DateTime.UtcNow.AddDays(-_config.ActivePostDays);
        
        var candidatePosts = await context.Posts
            .Where(p => p.Status != PostStatus.Deleted && p.CreatedAt > cutoffTime)
            .Select(p => p.PostId)
            .ToListAsync(cancellationToken);

        var totalPosts = candidatePosts.Count;
        _logger.LogDebug("Found {Count} posts to process for virality", totalPosts);

        // Process in batches
        var processedCount = 0;
        var errorCount = 0;
        var batchSize = _config.MaxPostsPerTick;

        for (int i = 0; i < candidatePosts.Count; i += batchSize)
        {
            var batch = candidatePosts.Skip(i).Take(batchSize).ToList();
            
            foreach (var postId in batch)
            {
                try
                {
                    await viralityService.CheckThresholdsAsync(postId, cancellationToken);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogWarning(ex, "Error processing virality for post {PostId}", postId);
                }
            }
        }

        // Mark declining posts
        await MarkDecliningPostsAsync(context, cancellationToken);

        var elapsed = DateTime.UtcNow - startTime;
        _logger.LogInformation(
            "Virality tick completed: {Processed}/{Total} posts processed, {Errors} errors, {Elapsed}ms",
            processedCount, totalPosts, errorCount, elapsed.TotalMilliseconds);
    }

    private async Task MarkDecliningPostsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        // Find posts that were viral but now have declining velocity
        var viralPosts = await context.PostVirality
            .Where(v => v.State == ViralityState.Viral || 
                       v.State == ViralityState.MassivelyViral ||
                       v.State == ViralityState.Popular)
            .Where(v => v.LastUpdated < DateTime.UtcNow.AddHours(-_config.ViralWindowHours))
            .ToListAsync(cancellationToken);

        foreach (var virality in viralPosts)
        {
            // Check if velocity has dropped significantly
            var velocityDropPercent = virality.PeakVelocity > 0
                ? (virality.PeakVelocity - virality.Velocity) / virality.PeakVelocity
                : 0;

            if (velocityDropPercent >= _config.DeclineVelocityDropPercent)
            {
                var previousState = virality.State;
                virality.State = ViralityState.Declining;
                virality.DeclinedAt ??= DateTime.UtcNow;
                
                // Log the transition
                context.ViralityTransitions.Add(new ViralityTransition
                {
                    PostId = virality.PostId,
                    FromState = previousState,
                    ToState = ViralityState.Declining,
                    ScoreAtTransition = virality.Score,
                    EngagementAtTransition = virality.TotalEngagement,
                    VelocityAtTransition = virality.Velocity,
                    Metadata = $"{{\"velocityDropPercent\":{velocityDropPercent:F2},\"peakVelocity\":{virality.PeakVelocity:F2}}}"
                });

                _logger.LogInformation(
                    "Post {PostId} declining from {FromState} to Declining. Velocity dropped {DropPercent:F0}%",
                    virality.PostId, previousState, velocityDropPercent * 100);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
