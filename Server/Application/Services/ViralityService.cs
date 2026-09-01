using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for calculating and managing post virality
/// </summary>
public class ViralityService : IViralityService
{
    private readonly AppDbContext _context;
    private readonly IPostService _postService;
    private readonly IAccountService _accountService;
    private readonly ISocialGraphService _socialGraphService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly IAiTextGenerationService? _aiService;
    private readonly ViralityConfig _config;
    private readonly ILogger<ViralityService> _logger;

    public ViralityService(
        AppDbContext context,
        IPostService postService,
        IAccountService accountService,
        ISocialGraphService socialGraphService,
        IEventService eventService,
        INotificationService notificationService,
        IAiTextGenerationService? aiService,
        ViralityConfig config,
        ILogger<ViralityService> logger)
    {
        _context = context;
        _postService = postService;
        _accountService = accountService;
        _socialGraphService = socialGraphService;
        _eventService = eventService;
        _notificationService = notificationService;
        _aiService = aiService;
        _config = config;
        _logger = logger;
    }

    public async Task<PostVirality> CalculateViralityAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postService.GetPostByIdAsync(postId);
        if (post == null)
            throw new ArgumentException($"Post {postId} not found");

        // Get engagement counts
        var likeCount = await _context.PostLikes.CountAsync(l => l.PostId == post.Id, cancellationToken);
        var commentCount = await _context.Comments.CountAsync(c => c.PostId == post.Id && c.Status != CommentStatus.Deleted, cancellationToken);
        var repostCount = await GetRepostCountAsync(post.Id, cancellationToken);
        
        var totalEngagement = likeCount + commentCount + repostCount;
        var velocity = await CalculateVelocityAsync(post.Id, cancellationToken);
        var reach = await EstimateReachAsync(post, cancellationToken);

        // Calculate virality score
        var authorFollowers = await _socialGraphService.GetFollowerCountAsync(post.AuthorAccountId);
        var score = CalculateViralityScore(totalEngagement, velocity, reach, authorFollowers);
        var state = DetermineState(totalEngagement, velocity);

        // Get or create virality record
        var virality = await GetOrCreateViralityRecordAsync(postId, cancellationToken);
        
        // Update virality record
        virality.TotalEngagement = totalEngagement;
        virality.Velocity = velocity;
        virality.PeakVelocity = Math.Max(virality.PeakVelocity, velocity);
        virality.Reach = reach;
        virality.Score = score;
        virality.ShareCount = repostCount;
        
        var previousState = virality.State;
        virality.State = state;
        virality.LastUpdated = DateTime.UtcNow;

        // Handle state transitions
        if (previousState != state)
        {
            await HandleStateTransitionAsync(post, virality, previousState, state, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return virality;
    }

    public async Task<ViralityState> GetViralityStateAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var virality = await GetPostViralityAsync(postId, cancellationToken);
        return virality?.State ?? ViralityState.Normal;
    }

    public async Task<PostVirality?> GetPostViralityAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.PostVirality
            .FirstOrDefaultAsync(v => v.PostId == postId, cancellationToken);
    }

    public async Task<List<Post>> GetViralPostsAsync(
        int count = 10, 
        ViralityState minState = ViralityState.Trending, 
        string? topic = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Posts
            .Include(p => p.AuthorAccount)
            .Include(p => p.Community)
            .Where(p => p.Status != PostStatus.Deleted);

        if (!string.IsNullOrEmpty(topic))
        {
            query = query.Where(p => p.Topic == topic);
        }

        // Join with virality to filter by state
        var viralPosts = await query
            .Join(
                _context.PostVirality.Where(v => v.State >= minState),
                p => p.PostId,
                v => v.PostId,
                (p, v) => new { Post = p, Virality = v })
            .OrderByDescending(x => x.Virality.Score)
            .Take(count)
            .Select(x => x.Post)
            .ToListAsync(cancellationToken);

        return viralPosts;
    }

    public async Task<List<Post>> GetTrendingPostsAsync(
        int count = 10,
        string? topic = null,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-_config.ViralWindowHours);
        
        var query = _context.Posts
            .Include(p => p.AuthorAccount)
            .Include(p => p.Community)
            .Where(p => p.Status != PostStatus.Deleted && p.CreatedAt > cutoffTime);

        if (!string.IsNullOrEmpty(topic))
        {
            query = query.Where(p => p.Topic == topic);
        }

        // Join with virality and order by score with recency boost
        var trendingPosts = await query
            .Join(
                _context.PostVirality,
                p => p.PostId,
                v => v.PostId,
                (p, v) => new { Post = p, Virality = v })
            .OrderByDescending(x => x.Virality.Score + (DateTime.UtcNow - x.Post.CreatedAt).TotalHours * -0.1)
            .Take(count)
            .Select(x => x.Post)
            .ToListAsync(cancellationToken);

        return trendingPosts;
    }

    public async Task<List<ViralityTransition>> GetTransitionHistoryAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.ViralityTransitions
            .Where(t => t.PostId == postId)
            .OrderByDescending(t => t.TransitionedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PostVirality> TrackEngagementAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        // Quick update for engagement tracking - just recalculate velocity and score
        var virality = await GetOrCreateViralityRecordAsync(postId, cancellationToken);
        var post = await _postService.GetPostByIdAsync(postId);
        
        if (post != null)
        {
            var likeCount = await _context.PostLikes.CountAsync(l => l.PostId == post.Id, cancellationToken);
            var commentCount = await _context.Comments.CountAsync(c => c.PostId == post.Id && c.Status != CommentStatus.Deleted, cancellationToken);
            var repostCount = await GetRepostCountAsync(post.Id, cancellationToken);
            
            virality.TotalEngagement = likeCount + commentCount + repostCount;
            virality.Velocity = await CalculateVelocityAsync(post.Id, cancellationToken);
            virality.PeakVelocity = Math.Max(virality.PeakVelocity, virality.Velocity);
            
            var authorFollowers = await _socialGraphService.GetFollowerCountAsync(post.AuthorAccountId);
            virality.Score = CalculateViralityScore(virality.TotalEngagement, virality.Velocity, virality.Reach, authorFollowers);
            
            virality.State = DetermineState(virality.TotalEngagement, virality.Velocity);
            virality.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        return virality;
    }

    public async Task CheckThresholdsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        await CalculateViralityAsync(postId, cancellationToken);
    }

    public async Task ProcessShareCascadeAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postService.GetPostByIdAsync(postId);
        if (post == null) return;

        var virality = await GetPostViralityAsync(postId, cancellationToken);
        if (virality == null) return;

        // Calculate share probability based on virality
        var shareProbability = CalculateShareProbability(virality);
        
        // Estimate how many more shares we might get
        var potentialShares = (int)(virality.Reach * shareProbability * 0.01); // 1% of reach shares
        
        // Update share count estimate
        virality.ShareCount += potentialShares;
        
        // Update reach based on estimated shares
        if (potentialShares > 0)
        {
            var avgFollowers = await GetAverageFollowerCountAsync(cancellationToken);
            virality.Reach += potentialShares * avgFollowers / 10; // Conservative estimate
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AnalyzeControversyAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postService.GetPostByIdAsync(postId);
        if (post == null) return 0;

        var virality = await GetOrCreateViralityRecordAsync(postId, cancellationToken);
        
        if (virality.HasControversyAnalysis)
            return virality.ControversyLevel;

        if (_aiService != null && _aiService.IsConfigured)
        {
            var author = post.AuthorAccount;
            var followerCount = author != null ? await _socialGraphService.GetFollowerCountAsync(post.AuthorAccountId) : 0;
            
            var prompt = $@"Analyze this social media post for controversy potential.

POST: {post.Content}
AUTHOR: @{(author?.Username ?? "unknown")} ({followerCount} followers)

Respond with a JSON object:
{{
  ""controversyLevel"": 0-10,
  ""reasons"": [""reason 1"", ""reason 2""],
  ""likelyToSpread"": true/false,
  ""reasoning"": ""brief explanation""
}}";

            var result = await _aiService.GenerateAsync(new AiGenerationRequest
            {
                SystemPrompt = "You are a controversy analyzer for social media posts. Return valid JSON only.",
                UserPrompt = prompt,
                MaxTokens = 200,
                Temperature = 0.3
            });

            if (result.Success && !string.IsNullOrWhiteSpace(result.Text))
            {
                try
                {
                    var json = JsonDocument.Parse(result.Text);
                    if (json.RootElement.TryGetProperty("controversyLevel", out var level))
                    {
                        virality.ControversyLevel = level.GetInt32();
                        virality.HasControversyAnalysis = true;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
                catch
                {
                    // Parse failed, use default
                }
            }
        }

        return virality.ControversyLevel;
    }

    private async Task<PostVirality> GetOrCreateViralityRecordAsync(Guid postId, CancellationToken cancellationToken)
    {
        var virality = await _context.PostVirality
            .FirstOrDefaultAsync(v => v.PostId == postId, cancellationToken);

        if (virality == null)
        {
            virality = new PostVirality
            {
                PostId = postId,
                State = ViralityState.Normal,
                CreatedAt = DateTime.UtcNow
            };
            _context.PostVirality.Add(virality);
        }

        return virality;
    }

    private async Task<float> CalculateVelocityAsync(int postId, CancellationToken cancellationToken)
    {
        var windowStart = DateTime.UtcNow.AddHours(-_config.ViralWindowHours);
        
        // Get posts created after window start
        var post = await _context.Posts.FindAsync(new object[] { postId }, cancellationToken);
        if (post == null) return 0;
        
        // Count engagement since post creation, capped at window
        var effectiveStart = post.CreatedAt > windowStart ? post.CreatedAt : windowStart;
        var windowHours = (float)(DateTime.UtcNow - effectiveStart).TotalHours;
        if (windowHours < 0.1) windowHours = 0.1f; // Minimum window
        
        var likeCount = await _context.PostLikes
            .CountAsync(l => l.PostId == postId && l.CreatedAt >= effectiveStart, cancellationToken);
        var commentCount = await _context.Comments
            .CountAsync(c => c.PostId == postId && c.CreatedAt >= effectiveStart && c.Status != CommentStatus.Deleted, cancellationToken);
        var repostCount = await GetRecentRepostCountAsync(postId, effectiveStart, cancellationToken);
        
        var totalEngagement = likeCount + commentCount + repostCount;
        return totalEngagement / windowHours;
    }

    private async Task<int> EstimateReachAsync(Post post, CancellationToken cancellationToken)
    {
        var authorFollowers = await _socialGraphService.GetFollowerCountAsync(post.AuthorAccountId);
        
        // Base reach = author's followers
        var baseReach = authorFollowers;
        
        // Get engagement multiplier
        var likeCount = await _context.PostLikes.CountAsync(l => l.PostId == post.Id, cancellationToken);
        var engagementMultiplier = 1 + (likeCount / 100.0);
        
        // Viral multiplier
        var virality = await GetPostViralityAsync(post.PostId, cancellationToken);
        var viralMultiplier = virality != null && virality.Velocity >= _config.ViralVelocityMin ? 2.0 : 1.0;
        
        return (int)(baseReach * engagementMultiplier * viralMultiplier);
    }

    private float CalculateViralityScore(int totalEngagement, float velocity, int reach, int authorFollowers)
    {
        // Engagement score (0-30)
        var engagementScore = totalEngagement > 0 
            ? Math.Min(30, (float)Math.Log10(totalEngagement + 1) * 10)
            : 0;
        
        // Velocity score (0-30) - recent growth matters more
        var velocityScore = Math.Min(30, velocity * 3);
        
        // Reach score (0-20) - how many people saw it
        var reachScore = reach > 0 
            ? Math.Min(20, (float)Math.Log10(reach + 1) * 5)
            : 0;
        
        // Relative engagement (0-20) - engagement relative to author's reach
        var relativeEngagement = authorFollowers > 0 && totalEngagement > 0
            ? (float)totalEngagement / authorFollowers
            : 0;
        var relativeScore = Math.Min(20, relativeEngagement * 100);
        
        return Math.Min(100, engagementScore + velocityScore + reachScore + relativeScore);
    }

    private ViralityState DetermineState(int totalEngagement, float velocity)
    {
        // Check from highest to lowest
        if (totalEngagement >= _config.MassivelyViralThreshold)
            return ViralityState.MassivelyViral;
        
        if (totalEngagement >= _config.ViralThreshold && velocity >= _config.ViralVelocityMin)
            return ViralityState.Viral;
        
        if (totalEngagement >= _config.PopularThreshold)
            return ViralityState.Popular;
        
        if (totalEngagement >= _config.TrendingThreshold)
            return ViralityState.Trending;
        
        return ViralityState.Normal;
    }

    private async Task HandleStateTransitionAsync(
        Post post, 
        PostVirality virality, 
        ViralityState fromState, 
        ViralityState toState,
        CancellationToken cancellationToken)
    {
        // Log the transition
        var transition = new ViralityTransition
        {
            PostId = post.PostId,
            FromState = fromState,
            ToState = toState,
            ScoreAtTransition = virality.Score,
            EngagementAtTransition = virality.TotalEngagement,
            VelocityAtTransition = virality.Velocity
        };
        _context.ViralityTransitions.Add(transition);

        _logger?.LogInformation("Post {PostId} transitioned from {From} to {To}", 
            post.PostId, fromState, toState);

        // Handle specific transitions
        switch (toState)
        {
            case ViralityState.Viral:
                virality.ViralAt ??= DateTime.UtcNow;
                virality.FirstViralThresholdCrossed ??= ViralityState.Viral;
                await OnPostBecomesViralAsync(post, virality, cancellationToken);
                break;
                
            case ViralityState.MassivelyViral:
                virality.MassivelyViralAt = DateTime.UtcNow;
                virality.FirstViralThresholdCrossed ??= ViralityState.MassivelyViral;
                await OnPostBecomesMassivelyViralAsync(post, virality, cancellationToken);
                break;
                
            case ViralityState.Declining:
                virality.DeclinedAt = DateTime.UtcNow;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task OnPostBecomesViralAsync(Post post, PostVirality virality, CancellationToken cancellationToken)
    {
        var author = post.AuthorAccount;
        if (author == null) return;

        // Calculate and apply follower gain
        var followerGain = CalculateFollowerGain(virality, author);
        await _accountService.AdjustFollowerCountAsync(author.Id, followerGain, cancellationToken);
        
        // Calculate and apply fame gain
        var fameGain = CalculateFameGain(virality);
        await _accountService.AdjustFameLevelAsync(author.Id, fameGain, cancellationToken);

        // Create viral post event
        await CreateViralEventAsync(post, virality, "ViralPost", cancellationToken);

        // Create notification
        try
        {
            var notification = new Notification
            {
                RecipientAccountId = author.Id,
                Type = NotificationType.ViralPost,
                Title = "Your post went viral!",
                Content = $"Your post reached viral status with {virality.TotalEngagement} engagements!",
                RelatedPostGuid = post.PostId,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationService.CreateNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create viral notification for post {PostId}", post.PostId);
        }

        _logger?.LogInformation("Post {PostId} went viral. Author {AuthorId} gained {Followers} followers",
            post.PostId, author.Id, followerGain);
    }

    private async Task OnPostBecomesMassivelyViralAsync(Post post, PostVirality virality, CancellationToken cancellationToken)
    {
        var author = post.AuthorAccount;
        if (author == null) return;

        // Extra follower gain for massively viral
        var followerGain = CalculateFollowerGain(virality, author) * 2;
        await _accountService.AdjustFollowerCountAsync(author.Id, followerGain, cancellationToken);
        
        // Extra fame gain
        var fameGain = CalculateFameGain(virality) * 2;
        await _accountService.AdjustFameLevelAsync(author.Id, fameGain, cancellationToken);

        // Create massively viral event
        await CreateViralEventAsync(post, virality, "ViralPost", cancellationToken);

        // Major notification
        try
        {
            var notification = new Notification
            {
                RecipientAccountId = author.Id,
                Type = NotificationType.ViralPost,
                Title = "Your post went MASSIVELY VIRAL!",
                Content = $"Your post went MASSIVELY VIRAL with {virality.TotalEngagement} engagements!",
                RelatedPostGuid = post.PostId,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationService.CreateNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create massively viral notification for post {PostId}", post.PostId);
        }

        _logger?.LogInformation("Post {PostId} went massively viral. Author {AuthorId} gained {Followers} followers",
            post.PostId, author.Id, followerGain);
    }

    private async Task CreateViralEventAsync(Post post, PostVirality virality, string eventType, CancellationToken cancellationToken)
    {
        var author = post.AuthorAccount;
        
        // Create viral post event
        var evt = new Event
        {
            Type = Domain.Entities.EventType.ViralPost,
            Title = $"Post by @{author?.Username ?? "unknown"} went {eventType}",
            Description = $"A post {(post.Topic != null ? $"about {post.Topic}" : "")} crossed the {virality.State} threshold with {virality.TotalEngagement} engagements, {virality.Velocity:F1}/hour velocity, and {virality.Reach} reach.",
            NarrativeContext = $"The post achieved {virality.Score:F1} virality score with peak velocity of {virality.PeakVelocity:F1}/hour.",
            CreatorAccountId = post.AuthorAccountId,
            Status = Domain.Entities.EventStatus.Active,
            Topic = post.Topic,
            DramaLevel = virality.State switch
            {
                ViralityState.Viral => 4,
                ViralityState.MassivelyViral => 8,
                _ => 2
            },
            StartAt = DateTime.UtcNow,
            Popularity = virality.TotalEngagement
        };
        
        _context.Events.Add(evt);
        await _context.SaveChangesAsync(cancellationToken);

        // Add author as participant
        if (post.AuthorAccountId > 0)
        {
            var participation = new EventParticipation
            {
                EventId = evt.Id,
                AccountId = post.AuthorAccountId,
                Role = ParticipantRole.Protagonist,
                LLMReasoning = "Post author whose content went viral"
            };
            _context.EventParticipations.Add(participation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private int CalculateFollowerGain(PostVirality virality, Account author)
    {
        var baseGain = _config.BaseFollowerGainOnViral;
        
        // Engagement bonus
        var engagementBonus = virality.TotalEngagement / 50;
        
        // Fame penalty (already famous = less relative gain)
        var famePenalty = 0.0;
        if (author.Profile?.Bio != null && author.Profile.Bio.Contains("famous", StringComparison.OrdinalIgnoreCase))
            famePenalty = 0.3;
        
        // Viral multiplier
        var viralMultiplier = virality.State == ViralityState.MassivelyViral ? 5.0 : 
                             virality.State == ViralityState.Viral ? 2.0 : 1.0;
        
        return (int)((baseGain + engagementBonus) * (1 - famePenalty) * viralMultiplier);
    }

    private float CalculateFameGain(PostVirality virality)
    {
        var baseGain = _config.BaseFameGainOnViral;
        
        // Engagement bonus
        var engagementBonus = virality.TotalEngagement / 200.0f;
        
        // Viral multiplier
        var viralMultiplier = virality.State == ViralityState.MassivelyViral ? 3.0f : 1.0f;
        
        return (baseGain + engagementBonus) * viralMultiplier;
    }

    private float CalculateShareProbability(PostVirality virality)
    {
        // Base probability increases with virality score
        var baseProbability = virality.Score / 1000.0f; // Max 10% base
        
        // Velocity bonus
        var velocityBonus = virality.Velocity / 1000.0f;
        
        return Math.Min(0.2f, baseProbability + velocityBonus); // Cap at 20%
    }

    private async Task<int> GetRepostCountAsync(int postId, CancellationToken cancellationToken)
    {
        // For now, return 0 - actual repost counting would require tracking reply-to relationships
        // or a separate Repost table
        return await Task.FromResult(0);
    }

    private async Task<int> GetRecentRepostCountAsync(int postId, DateTime since, CancellationToken cancellationToken)
    {
        return await Task.FromResult(0);
    }

    private async Task<int> GetAverageFollowerCountAsync(CancellationToken cancellationToken)
    {
        var avg = await _context.Follows
            .GroupBy(f => f.FollowedAccountId)
            .Select(g => g.Count())
            .DefaultIfEmpty(0)
            .AverageAsync(cancellationToken);
        
        return (int)avg;
    }
}
