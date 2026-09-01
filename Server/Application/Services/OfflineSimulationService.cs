using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Configuration for offline simulation
/// </summary>
public class OfflineSimulationConfig
{
    public bool Enabled { get; set; } = true;
    public int MinOfflineHoursBeforeSimulation { get; set; } = 1;
    public int TicksPerHour { get; set; } = 10;
    public int MaxTicksPerSession { get; set; } = 1000;
    public int MinTicksToSimulate { get; set; } = 5;
    public double EventProbabilityMultiplier { get; set; } = 0.5;
}

/// <summary>
/// Service for offline world simulation
/// </summary>
public class OfflineSimulationService : IOfflineSimulationService
{
    private readonly AppDbContext _context;
    private readonly IAccountService _accountService;
    private readonly INpcSimulationService _npcService;
    private readonly IEventService _eventService;
    private readonly IEventGenerationService _eventGenerationService;
    private readonly ICausalTrackingService _causalTracking;
    private readonly IAiTextGenerationService? _aiService;
    private readonly OfflineSimulationConfig _config;
    private readonly ILogger<OfflineSimulationService> _logger;

    public OfflineSimulationService(
        AppDbContext context,
        IAccountService accountService,
        INpcSimulationService npcService,
        IEventService eventService,
        IEventGenerationService eventGenerationService,
        ICausalTrackingService causalTracking,
        IAiTextGenerationService? aiService,
        OfflineSimulationConfig config,
        ILogger<OfflineSimulationService> logger)
    {
        _context = context;
        _accountService = accountService;
        _npcService = npcService;
        _eventService = eventService;
        _eventGenerationService = eventGenerationService;
        _causalTracking = causalTracking;
        _aiService = aiService;
        _config = config;
        _logger = logger;
    }

    public async Task<TimeSpan> GetOfflineDurationAsync(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            return TimeSpan.Zero;

        var lastSeen = account.LastSeenAt ?? account.CreatedAt;
        return DateTime.UtcNow - lastSeen;
    }

    public async Task<bool> ShouldRunOfflineSimulationAsync(int accountId)
    {
        if (!_config.Enabled)
            return false;

        var duration = await GetOfflineDurationAsync(accountId);
        return duration.TotalHours >= _config.MinOfflineHoursBeforeSimulation;
    }

    public async Task<CatchupSummary> RunOfflineSimulationAsync(int accountId)
    {
        _logger?.LogInformation("Starting offline simulation for account {AccountId}", accountId);

        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            throw new InvalidOperationException($"Account {accountId} not found");

        var lastSeen = account.LastSeenAt ?? account.CreatedAt;
        var now = DateTime.UtcNow;
        var duration = now - lastSeen;

        // Calculate compressed ticks
        var totalTicks = Math.Max(
            _config.MinTicksToSimulate,
            Math.Min(
                _config.MaxTicksPerSession,
                (int)(duration.TotalHours * _config.TicksPerHour)));

        // Track stats
        var postsCreated = 0;
        var followersGained = 0;
        var followersLost = 0;
        var eventsCreated = new List<Event>();

        // Simulate NPC activity
        _logger?.LogDebug("Simulating {Ticks} ticks for offline period {Duration}", totalTicks, duration);

        // Get active NPCs
        var activeNpcs = await _context.Accounts
            .Where(a => a.AccountType != AccountType.OrdinaryUser || a.NpcProfile != null) // NPCs or non-users
            .Where(a => a.LastSeenAt > lastSeen.AddHours(-24) || a.CreatedAt > lastSeen.AddHours(-24))
            .Take(100) // Limit for performance
            .ToListAsync();

        // Predict and execute NPC actions
        var random = new Random((int)(accountId ^ lastSeen.Ticks)); // Deterministic seed
        foreach (var npc in activeNpcs)
        {
            var npcPosts = PredictNpcPosts(npc, totalTicks, random);
            postsCreated += npcPosts.Count;
            
            // Execute posts
            foreach (var post in npcPosts)
            {
                _context.Posts.Add(post);
            }

            // Predict follower changes
            var followerChanges = PredictFollowerChanges(npc, totalTicks, random);
            followersGained += followerChanges.Gained;
            followersLost += followerChanges.Lost;
        }

        await _context.SaveChangesAsync();

        // Generate events for offline period
        var eventsToGenerate = Math.Max(1, (int)(totalTicks / _config.TicksPerHour * _config.EventProbabilityMultiplier));
        for (int i = 0; i < eventsToGenerate; i++)
        {
            try
            {
                var eventProposal = await _eventGenerationService.ProposeNextEventAsync(CancellationToken.None);
                if (eventProposal != null)
                {
                    var validationResult = await _eventGenerationService.ValidateProposalAsync(eventProposal);
                    if (validationResult.IsValid)
                    {
                        var evt = await _eventGenerationService.ExecuteEventAsync(eventProposal, CancellationToken.None);
                        if (evt != null)
                        {
                            eventsCreated.Add(evt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to generate event during offline simulation");
            }
        }

        // Create simulation result record
        var simulationResult = new OfflineSimulationResult
        {
            AccountId = accountId,
            StartTime = lastSeen,
            EndTime = now,
            Duration = duration,
            TicksSimulated = totalTicks,
            PostsCreated = postsCreated,
            FollowersGained = followersGained,
            FollowersLost = followersLost,
            EventsCreated = eventsCreated.Count,
            EventsSummaryJson = JsonSerializer.Serialize(eventsCreated.Select(e => new EventSummary
            {
                EventId = e.EventId,
                Type = e.Type.ToString(),
                Title = e.Title,
                DramaLevel = e.DramaLevel,
                ParticipantCount = e.ParticipantCount
            }))
        };

        // Generate LLM summary if available
        if (_aiService != null && _aiService.IsConfigured && eventsCreated.Count > 0)
        {
            simulationResult.CatchupSummary = await GenerateCatchupNarrativeAsync(account, duration, eventsCreated, followersGained, followersLost);
        }
        else
        {
            simulationResult.CatchupSummary = GenerateDefaultSummary(duration, eventsCreated.Count, followersGained, followersLost);
        }

        _context.OfflineSimulationResults.Add(simulationResult);
        
        // Update account's last seen
        account.LastSeenAt = now;
        
        await _context.SaveChangesAsync();

        _logger?.LogInformation("Completed offline simulation for account {AccountId}: {Posts} posts, {Events} events",
            accountId, postsCreated, eventsCreated.Count);

        return new CatchupSummary
        {
            OfflineSimulationResultId = simulationResult.OfflineSimulationResultId,
            Duration = duration,
            OfflineSince = lastSeen,
            OfflineUntil = now,
            NewFollowers = followersGained,
            LostFollowers = followersLost,
            NotificationsCreated = eventsCreated.Count * 2, // Estimate
            PostsCreated = postsCreated,
            MajorEvents = eventsCreated.Select(e => new EventSummary
            {
                EventId = e.EventId,
                Type = e.Type.ToString(),
                Title = e.Title,
                DramaLevel = e.DramaLevel,
                ParticipantCount = e.ParticipantCount
            }).ToList(),
            Summary = simulationResult.CatchupSummary,
            IsAcknowledged = false
        };
    }

    public async Task<CatchupSummary?> GetCatchupSummaryAsync(int accountId)
    {
        var latestResult = await _context.OfflineSimulationResults
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestResult == null)
            return null;

        var eventSummaries = string.IsNullOrEmpty(latestResult.EventsSummaryJson)
            ? new List<EventSummary>()
            : JsonSerializer.Deserialize<List<EventSummary>>(latestResult.EventsSummaryJson) ?? new List<EventSummary>();

        return new CatchupSummary
        {
            OfflineSimulationResultId = latestResult.OfflineSimulationResultId,
            Duration = latestResult.Duration,
            OfflineSince = latestResult.StartTime,
            OfflineUntil = latestResult.EndTime,
            NewFollowers = latestResult.FollowersGained,
            LostFollowers = latestResult.FollowersLost,
            NotificationsCreated = latestResult.NotificationsCreated,
            PostsCreated = latestResult.PostsCreated,
            MajorEvents = eventSummaries,
            Summary = latestResult.CatchupSummary,
            IsAcknowledged = latestResult.IsAcknowledged
        };
    }

    public async Task AcknowledgeCatchupAsync(int accountId)
    {
        var latestResult = await _context.OfflineSimulationResults
            .Where(r => r.AccountId == accountId && !r.IsAcknowledged)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestResult != null)
        {
            latestResult.IsAcknowledged = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasUnreadCatchupAsync(int accountId)
    {
        return await _context.OfflineSimulationResults
            .AnyAsync(r => r.AccountId == accountId && !r.IsAcknowledged);
    }

    private List<Post> PredictNpcPosts(Account npc, int ticks, Random random)
    {
        var posts = new List<Post>();
        
        // Calculate expected posts based on account type
        var postProbability = npc.AccountType switch
        {
            AccountType.Celebrity => 0.3,
            AccountType.Influencer => 0.25,
            AccountType.Creator => 0.2,
            AccountType.News => 0.35,
            AccountType.Official => 0.15,
            _ => 0.1
        };

        var expectedPosts = ticks * postProbability * 0.1;
        var postCount = (int)(expectedPosts + random.NextDouble() * 2);

        for (int i = 0; i < Math.Min(postCount, 20); i++) // Cap at 20 posts per NPC
        {
            if (random.NextDouble() < postProbability)
            {
                posts.Add(new Post
                {
                    AuthorAccountId = npc.Id,
                    Content = $"[Simulated post from offline simulation]",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-random.NextDouble() * ticks * 6)
                });
            }
        }

        return posts;
    }

    private (int Gained, int Lost) PredictFollowerChanges(Account npc, int ticks, Random random)
    {
        // Base follower change rates per tick
        var growthRate = npc.AccountType switch
        {
            AccountType.Celebrity => 0.8,
            AccountType.Influencer => 0.5,
            AccountType.Creator => 0.3,
            AccountType.News => 0.4,
            AccountType.Official => 0.2,
            _ => 0.1
        };

        var gained = (int)(ticks * growthRate * (0.5 + random.NextDouble()));
        var lost = (int)(gained * 0.1 * random.NextDouble()); // 10% of gains typically lost

        return (Math.Max(0, gained), Math.Max(0, lost));
    }

    private async Task<string> GenerateCatchupNarrativeAsync(
        Account account,
        TimeSpan duration,
        List<Event> events,
        int followersGained,
        int followersLost)
    {
        var eventDescriptions = string.Join("; ", events.Take(5).Select(e => e.Title));

        var prompt = $@"Generate a brief 'you missed this' summary for a social media user.

OFFLINE PERIOD: {duration.TotalHours:F1} hours
WHAT HAPPENED:
- Follower changes: +{followersGained} gained, {followersLost} lost
- Major events: {eventDescriptions}

Generate a 3-4 sentence summary. Keep it engaging but factual. First person from the user's perspective.";

        var result = await _aiService!.GenerateAsync(new AiGenerationRequest
        {
            SystemPrompt = "You are writing brief social media catchup summaries.",
            UserPrompt = prompt,
            MaxTokens = 200,
            Temperature = 0.7
        });

        return result.Success && !string.IsNullOrWhiteSpace(result.Text)
            ? result.Text
            : GenerateDefaultSummary(duration, events.Count, followersGained, followersLost);
    }

    private string GenerateDefaultSummary(TimeSpan duration, int eventCount, int followersGained, int followersLost)
    {
        return $"You were offline for {duration.TotalHours:F1} hours. " +
               $"{eventCount} major event(s) occurred in your feed. " +
               $"You gained {followersGained} follower(s) and lost {followersLost}. " +
               $"Check your notifications for details.";
    }
}
