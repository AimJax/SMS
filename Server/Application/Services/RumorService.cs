using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using System.Text.RegularExpressions;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing rumors and misinformation
/// </summary>
public class RumorService : IRumorService
{
    private readonly AppDbContext _context;
    private readonly IPostService _postService;
    private readonly ISocialGraphService _socialGraphService;
    private readonly INotificationService _notificationService;
    private readonly RumorConfig _config;
    private readonly ILogger<RumorService> _logger;
    private readonly IAiTextGenerationService? _aiService;

    public RumorService(
        AppDbContext context,
        IPostService postService,
        ISocialGraphService socialGraphService,
        INotificationService notificationService,
        RumorConfig config,
        ILogger<RumorService> logger,
        IAiTextGenerationService? aiService = null)
    {
        _context = context;
        _postService = postService;
        _socialGraphService = socialGraphService;
        _notificationService = notificationService;
        _config = config;
        _logger = logger;
        _aiService = aiService;
    }

    #region Rumor Management

    public async Task<Rumor?> CreateRumorFromPostAsync(Guid postId, int? accountId = null)
    {
        if (!_config.Enabled) return null;

        var post = await _postService.GetPostByIdAsync(postId);
        if (post == null) return null;

        // Check if this post already has a rumor
        var existingRumor = await _context.Rumors
            .FirstOrDefaultAsync(r => r.SourcePostId == postId);
        if (existingRumor != null) return existingRumor;

        // Extract claim from post content
        var claim = ExtractClaimFromContent(post.Content);
        if (string.IsNullOrWhiteSpace(claim)) return null;

        // Create rumor
        var rumor = new Rumor
        {
            OriginAccountId = accountId ?? post.AuthorAccountId,
            Claim = claim,
            Summary = claim.Length > 200 ? claim.Substring(0, 197) + "..." : claim,
            TruthStatus = RumorTruthStatus.Unverified,
            SpreadType = accountId.HasValue ? RumorSpreadType.Planted : RumorSpreadType.Organic,
            SourcePostId = postId,
            CommunityId = post.CommunityId,
            FirstSeenAt = DateTime.UtcNow,
            IsActive = true,
            IsNotable = post.Likes.Count > _config.MinEngagementForRumor,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rumors.Add(rumor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created rumor {RumorId} from post {PostId}", rumor.RumorId, postId);
        return rumor;
    }

    public async Task<Rumor?> GetRumorByIdAsync(Guid rumorId)
    {
        return await _context.Rumors
            .Include(r => r.OriginAccount)
            .Include(r => r.SourcePost)
            .FirstOrDefaultAsync(r => r.RumorId == rumorId);
    }

    public async Task<List<Rumor>> GetNotableRumorsAsync(int count = 20)
    {
        return await _context.Rumors
            .Where(r => r.IsNotable && r.IsActive)
            .OrderByDescending(r => r.ShareCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Rumor>> GetRumorsByStatusAsync(RumorTruthStatus status, int count = 20)
    {
        return await _context.Rumors
            .Where(r => r.TruthStatus == status)
            .OrderByDescending(r => r.ShareCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Rumor>> GetCommunityRumorsAsync(int communityId, int count = 20)
    {
        return await _context.Rumors
            .Where(r => r.CommunityId == communityId && r.IsActive)
            .OrderByDescending(r => r.FirstSeenAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Rumor?> UpdateRumorStatusAsync(Guid rumorId, RumorTruthStatus status)
    {
        var rumor = await _context.Rumors
            .FirstOrDefaultAsync(r => r.RumorId == rumorId);
        if (rumor == null) return null;

        rumor.TruthStatus = status;
        rumor.ResolvedAt = DateTime.UtcNow;

        // If debunked or confirmed, mark as not active
        if (status == RumorTruthStatus.ConfirmedFalse || status == RumorTruthStatus.Debunked)
        {
            rumor.IsActive = false;
        }

        await _context.SaveChangesAsync();
        return rumor;
    }

    #endregion

    #region Belief Management

    public async Task<AccountBelief?> GetAccountBeliefAsync(int accountId, Guid rumorId)
    {
        return await _context.AccountBeliefs
            .FirstOrDefaultAsync(b => b.AccountId == accountId && b.RumorId == rumorId);
    }

    public async Task<List<AccountBelief>> GetAccountBeliefsAsync(int accountId)
    {
        return await _context.AccountBeliefs
            .Include(b => b.Rumor)
            .Where(b => b.AccountId == accountId)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync();
    }

    public async Task<AccountBelief?> UpdateBeliefAsync(int accountId, Guid rumorId, RumorTruthStatus belief, double confidence = 0.5)
    {
        var existingBelief = await GetAccountBeliefAsync(accountId, rumorId);
        
        if (existingBelief != null)
        {
            existingBelief.Belief = belief;
            existingBelief.Confidence = Math.Clamp(confidence, 0.0, 1.0);
            existingBelief.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existingBelief = new AccountBelief
            {
                AccountId = accountId,
                RumorId = rumorId,
                Belief = belief,
                Confidence = Math.Clamp(confidence, 0.0, 1.0),
                FormedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AccountBeliefs.Add(existingBelief);
        }

        await _context.SaveChangesAsync();
        return existingBelief;
    }

    public async Task<AccountBelief?> FormBeliefFromExposureAsync(int accountId, Guid rumorId, string influenceSource)
    {
        var rumor = await GetRumorByIdAsync(rumorId);
        if (rumor == null) return null;

        // Determine belief based on rumor status and influence
        var belief = rumor.TruthStatus;
        
        // If rumor is unverified, account forms belief based on what they've seen
        if (belief == RumorTruthStatus.Unverified)
        {
            // 60% chance they believe it, 30% they're skeptical, 10% they disbelieve
            var rand = Random.Shared.NextDouble();
            belief = rand < 0.6 ? RumorTruthStatus.Unverified :
                     (rand < 0.9 ? RumorTruthStatus.Unknown : RumorTruthStatus.ConfirmedFalse);
        }

        // Confidence based on influence source
        double confidence = influenceSource.ToLower().Contains("friend") ? 0.7 :
                          influenceSource.ToLower().Contains("news") ? 0.8 :
                          influenceSource.ToLower().Contains("random") ? 0.4 : 0.5;

        return await UpdateBeliefAsync(accountId, rumorId, belief, confidence);
    }

    #endregion

    #region Evidence Management

    public async Task<RumorEvidence?> AddEvidenceAsync(Guid rumorId, int? accountId, string description, bool supportsRumor, string? sourceUrl = null)
    {
        var rumor = await GetRumorByIdAsync(rumorId);
        if (rumor == null) return null;

        var evidence = new RumorEvidence
        {
            RumorId = rumorId,
            AccountId = accountId,
            Description = description,
            SupportsRumor = supportsRumor,
            SourceUrl = sourceUrl,
            CredibilityScore = sourceUrl != null ? 7 : 5,
            CreatedAt = DateTime.UtcNow
        };

        _context.RumorEvidence.Add(evidence);

        // Update rumor stats
        rumor.ShareCount++;

        await _context.SaveChangesAsync();

        // Check if we should evaluate the rumor
        var evidenceCount = await _context.RumorEvidence.CountAsync(e => e.RumorId == rumorId);
        if (evidenceCount >= _config.EvidenceThresholdForResolution)
        {
            _ = Task.Run(() => EvaluateRumorTruthAsync(rumorId));
        }

        return evidence;
    }

    public async Task<List<RumorEvidence>> GetRumorEvidenceAsync(Guid rumorId)
    {
        return await _context.RumorEvidence
            .Where(e => e.RumorId == rumorId)
            .OrderByDescending(e => e.CredibilityScore)
            .ToListAsync();
    }

    public async Task<Rumor?> EvaluateRumorTruthAsync(Guid rumorId)
    {
        var rumor = await GetRumorByIdAsync(rumorId);
        if (rumor == null) return null;

        var evidence = await GetRumorEvidenceAsync(rumorId);
        if (!evidence.Any()) return rumor;

        var supportingEvidence = evidence.Where(e => e.SupportsRumor).ToList();
        var contradictingEvidence = evidence.Where(e => !e.SupportsRumor).ToList();

        var supportingCredibility = supportingEvidence.Sum(e => e.CredibilityScore);
        var contradictingCredibility = contradictingEvidence.Sum(e => e.CredibilityScore);

        // Evaluate based on evidence weight
        if (contradictingCredibility > supportingCredibility * 1.5)
        {
            rumor.TruthStatus = RumorTruthStatus.Debunked;
        }
        else if (supportingCredibility > contradictingCredibility * 1.5)
        {
            rumor.TruthStatus = RumorTruthStatus.ConfirmedTrue;
        }
        else if (evidence.Count >= 5)
        {
            rumor.TruthStatus = RumorTruthStatus.PartiallyTrue;
        }

        rumor.ResolvedAt = DateTime.UtcNow;
        rumor.IsActive = rumor.TruthStatus != RumorTruthStatus.Unverified;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Evaluated rumor {RumorId}: {Status}", rumorId, rumor.TruthStatus);

        return rumor;
    }

    #endregion

    #region Rumor Processing

    public Task<bool> ContainsRumorContentAsync(string content)
    {
        // Check for rumor indicators
        var rumorIndicators = new[]
        {
            "apparently", "reportedly", "sources say", "allegedly",
            "rumor has it", "word is", "heard that", "breaking:",
            "exclusive:", "leaked", "insider", "anonymous source"
        };

        var lowerContent = content.ToLower();
        var containsIndicator = rumorIndicators.Any(indicator => lowerContent.Contains(indicator));
        var containsQuestionable = ContainsQuestionableClaim(content);

        return Task.FromResult(containsIndicator || containsQuestionable);
    }

    public Task<List<string>> ExtractClaimsAsync(string content)
    {
        var claims = new List<string>();

        // Extract sentences that look like claims
        var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (trimmed.Length > 20 && ContainsClaimIndicators(trimmed))
            {
                claims.Add(trimmed);
            }
        }

        return Task.FromResult(claims);
    }

    public async Task ProcessRumorsTickAsync()
    {
        if (!_config.Enabled) return;

        _logger.LogDebug("Processing rumors tick");

        // Process existing rumors
        var activeRumors = await _context.Rumors
            .Where(r => r.IsActive && r.TruthStatus == RumorTruthStatus.Unverified)
            .ToListAsync();

        foreach (var rumor in activeRumors)
        {
            // Update spread velocity
            var windowStart = DateTime.UtcNow.AddHours(-_config.RumorWindowHours);
            var recentShares = await _context.AccountBeliefs
                .CountAsync(b => b.RumorId == rumor.RumorId && b.FormedAt > windowStart);

            var velocity = (float)recentShares / Math.Max(1, _config.RumorWindowHours / 24.0f);
            if (velocity > rumor.PeakVelocity)
            {
                rumor.PeakVelocity = velocity;
                rumor.PeakedAt = DateTime.UtcNow;
            }

            // Check if rumor should be marked as notable
            if (!rumor.IsNotable && rumor.ShareCount > _config.MinEngagementForRumor)
            {
                rumor.IsNotable = true;
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Private Helpers

    private static string ExtractClaimFromContent(string content)
    {
        // Look for claim patterns in content
        var claimPatterns = new[]
        {
            @"(.+?)(?:\s+(?:is|are|was|were)\s+(?:said\s+to\s+|believed\s+to\s+)?)",
            @"(.+?)(?:\s+(?:according\s+to|via|via\s+)[\w\s]+)",
            @"(.+?)(?:\s+(?:breaking|exclusive|just\s:in)\s+)",
            @"(.+?)(?:\s+(?:sources?|report|reveal|discover))",
        };

        foreach (var pattern in claimPatterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups[1].Length > 10)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        // Fallback: return first sentence if it looks like a claim
        var sentences = content.Split('.', '!', '?');
        if (sentences.Length > 0)
        {
            var first = sentences[0].Trim();
            if (first.Length > 20 && ContainsClaimIndicators(first))
            {
                return first;
            }
        }

        return string.Empty;
    }

    private static bool ContainsQuestionableClaim(string content)
    {
        var patterns = new[]
        {
            @"\d+\s*(?:million|billion|thousand)\s+(?:people|users|accounts)",
            @"(?:secret|hidden|they\s+(?:don't|won't)\s+tell)",
            @"(?:everyone|all\s+\w+)\s+(?:is|are)\s+(?:lying|wrong|fake)",
            @"(?:big\s+tech|government|celebrities?)\s+(?:is|are)\s+\w+ing"
        };

        return patterns.Any(p => Regex.IsMatch(content, p, RegexOptions.IgnoreCase));
    }

    private static bool ContainsClaimIndicators(string text)
    {
        var indicators = new[]
        {
            "apparently", "reportedly", "sources", "allegedly",
            "rumor", "leaked", "exclusive", "insider", "anonymous"
        };

        var lower = text.ToLower();
        return indicators.Any(i => lower.Contains(i));
    }

    #endregion
}
