using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for NPC-to-NPC social graph decisions.
/// Handles interest-based candidate selection, reciprocity, and unfollow logic.
/// </summary>
public interface INpcSocialGraphService
{
    /// <summary>
    /// Get candidate accounts for the NPC to potentially follow
    /// </summary>
    Task<IEnumerable<FollowCandidate>> GetFollowCandidatesAsync(NpcProfile npc, int limit);
    
    /// <summary>
    /// Get accounts that the NPC might want to unfollow
    /// </summary>
    Task<IEnumerable<int>> GetUnfollowCandidatesAsync(NpcProfile npc, int limit, int hoursBack);
    
    /// <summary>
    /// Calculate reciprocity score (how likely B is to follow back A)
    /// </summary>
    double CalculateReciprocityScore(NpcProfile follower, NpcProfile followee);
}

/// <summary>
/// Represents a candidate account for following
/// </summary>
public class FollowCandidate
{
    public int AccountId { get; set; }
    public string Username { get; set; } = "";
    public AccountType AccountType { get; set; }
    public double InterestScore { get; set; }
    public double ReciprocityScore { get; set; }
    public double ExplorationScore { get; set; }
    public bool IsFollowedByMe { get; set; }
    public DateTime? FollowedByMeAt { get; set; }
}

/// <summary>
/// NPC social graph decision service
/// </summary>
public class NpcSocialGraphService : INpcSocialGraphService
{
    private readonly AppDbContext _context;
    private readonly IContentRelevanceService _relevanceService;

    public NpcSocialGraphService(AppDbContext context, IContentRelevanceService relevanceService)
    {
        _context = context;
        _relevanceService = relevanceService;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FollowCandidate>> GetFollowCandidatesAsync(NpcProfile npc, int limit)
    {
        var npcAccountId = npc.AccountId;
        var interests = npc.Interests.ToList();
        var personality = npc.Personality ?? new NpcPersonality();
        var accountType = npc.Account?.AccountType ?? AccountType.OrdinaryUser;

        // Get accounts the NPC already follows
        var followingIds = await _context.Follows
            .Where(f => f.FollowerAccountId == npcAccountId)
            .Select(f => f.FollowedAccountId)
            .ToListAsync();

        // Get accounts that block the NPC or that NPC blocks
        var blockedByIds = await _context.Blocks
            .Where(b => b.BlockedAccountId == npcAccountId || b.BlockerAccountId == npcAccountId)
            .Select(b => b.BlockerAccountId == npcAccountId ? b.BlockedAccountId : b.BlockerAccountId)
            .ToListAsync();

        var excludedIds = followingIds.Union(blockedByIds).ToHashSet();
        excludedIds.Add(npcAccountId);

        var candidates = new List<FollowCandidate>();

        // 1. Interest-based candidates: accounts with matching interests
        var interestCandidates = await GetInterestBasedCandidatesAsync(npc, interests, excludedIds, limit / 2);
        candidates.AddRange(interestCandidates);

        // 2. Reciprocity candidates: accounts that follow the NPC but aren't followed back
        var reciprocityCandidates = await GetReciprocityCandidatesAsync(npc, excludedIds, personality, limit / 3);
        candidates.AddRange(reciprocityCandidates);

        // 3. Exploration candidates: random accounts with activity (driven by Openness)
        var explorationCount = Math.Max(1, (int)(limit * personality.Openness * 0.3));
        var explorationCandidates = await GetExplorationCandidatesAsync(npc, excludedIds, explorationCount);
        candidates.AddRange(explorationCandidates);

        // 4. Engagement-based candidates: accounts whose posts the NPC has interacted with
        var engagementCandidates = await GetEngagementBasedCandidatesAsync(npc, excludedIds, limit / 4);
        candidates.AddRange(engagementCandidates);

        // Deduplicate by AccountId and take top candidates
        return candidates
            .GroupBy(c => c.AccountId)
            .Select(g => g.First())
            .OrderByDescending(c => c.InterestScore + c.ReciprocityScore + c.ExplorationScore)
            .Take(limit)
            .ToList();
    }

    private async Task<List<FollowCandidate>> GetInterestBasedCandidatesAsync(
        NpcProfile npc, 
        List<NpcInterest> interests, 
        HashSet<int> excludedIds, 
        int limit)
    {
        var candidates = new List<FollowCandidate>();

        if (interests.Count == 0)
            return candidates;

        // Get interest keywords
        var interestKeys = interests.Select(i => i.InterestKey).ToList();

        // Find accounts that have posted about matching interests
        var recentPosts = await _context.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Active)
            .Where(p => !excludedIds.Contains(p.AuthorAccountId))
            .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit * 3)
            .Select(p => new { p.AuthorAccountId, p.Content })
            .ToListAsync();

        var scoredAccounts = new Dictionary<int, (int Score, string Username, AccountType Type)>();

        foreach (var post in recentPosts)
        {
            if (scoredAccounts.ContainsKey(post.AuthorAccountId))
                continue;

            var topics = _relevanceService.ExtractTopics(post.Content);
            var score = topics.Count(t => interestKeys.Contains(t));

            if (score > 0)
            {
                var account = await _context.Accounts
                    .AsNoTracking()
                    .Where(a => a.Id == post.AuthorAccountId)
                    .Select(a => new { a.Username, a.AccountType })
                    .FirstOrDefaultAsync();

                if (account != null)
                {
                    scoredAccounts[post.AuthorAccountId] = (score, account.Username, account.AccountType);
                }
            }
        }

        foreach (var (accountId, (score, username, type)) in scoredAccounts.Take(limit))
        {
            candidates.Add(new FollowCandidate
            {
                AccountId = accountId,
                Username = username,
                AccountType = type,
                InterestScore = Math.Min(1.0, score * 0.3)
            });
        }

        return candidates;
    }

    private async Task<List<FollowCandidate>> GetReciprocityCandidatesAsync(
        NpcProfile npc,
        HashSet<int> excludedIds,
        NpcPersonality personality,
        int limit)
    {
        var candidates = new List<FollowCandidate>();

        // Find accounts that follow the NPC but the NPC doesn't follow back
        var followers = await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowedAccountId == npc.AccountId)
            .Where(f => !excludedIds.Contains(f.FollowerAccountId))
            .OrderByDescending(f => f.CreatedAt)
            .Take(limit)
            .Select(f => new { f.FollowerAccountId, f.CreatedAt })
            .ToListAsync();

        foreach (var follower in followers)
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.Id == follower.FollowerAccountId)
                .Select(a => new { a.Username, a.AccountType })
                .FirstOrDefaultAsync();

            if (account != null)
            {
                // Agreeableness increases reciprocity
                var reciprocityScore = 0.3 + (personality.Agreeableness * 0.4);

                candidates.Add(new FollowCandidate
                {
                    AccountId = follower.FollowerAccountId,
                    Username = account.Username,
                    AccountType = account.AccountType,
                    ReciprocityScore = reciprocityScore,
                    IsFollowedByMe = false,
                    FollowedByMeAt = null
                });
            }
        }

        return candidates;
    }

    private async Task<List<FollowCandidate>> GetExplorationCandidatesAsync(
        NpcProfile npc,
        HashSet<int> excludedIds,
        int limit)
    {
        var candidates = new List<FollowCandidate>();

        // Get random active accounts not in excluded list
        // Using order by newid() is not ideal but acceptable for small limits
        var accounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.Status == AccountStatus.Active)
            .Where(a => !excludedIds.Contains(a.Id))
            .Where(a => a.AccountType != AccountType.Celebrity) // Celebrities rarely want exploration follows
            .OrderByDescending(a => _context.Posts.Count(p => p.AuthorAccountId == a.Id && p.Status == PostStatus.Active))
            .Take(limit * 2)
            .Select(a => new { a.Id, a.Username, a.AccountType })
            .ToListAsync();

        var random = new Random();
        var shuffled = accounts.OrderBy(_ => random.Next()).Take(limit).ToList();

        foreach (var account in shuffled)
        {
            candidates.Add(new FollowCandidate
            {
                AccountId = account.Id,
                Username = account.Username,
                AccountType = account.AccountType,
                ExplorationScore = 0.1 // Base exploration score
            });
        }

        return candidates;
    }

    private async Task<List<FollowCandidate>> GetEngagementBasedCandidatesAsync(
        NpcProfile npc,
        HashSet<int> excludedIds,
        int limit)
    {
        var candidates = new List<FollowCandidate>();

        // Find posts the NPC has liked or commented on recently
        var interactedPostIds = await _context.PostLikes
            .Where(l => l.AccountId == npc.AccountId)
            .Where(l => l.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .Select(l => l.PostId)
            .ToListAsync();

        var commentedPostIds = await _context.Comments
            .Where(c => c.AuthorAccountId == npc.AccountId)
            .Where(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .Select(c => c.PostId)
            .ToListAsync();

        var allInteractedPostIds = interactedPostIds.Union(commentedPostIds).Distinct().ToList();

        if (allInteractedPostIds.Count == 0)
            return candidates;

        // Get authors of interacted posts
        var authorIds = await _context.Posts
            .AsNoTracking()
            .Where(p => allInteractedPostIds.Contains(p.Id))
            .Where(p => !excludedIds.Contains(p.AuthorAccountId))
            .Select(p => p.AuthorAccountId)
            .Distinct()
            .Take(limit)
            .ToListAsync();

        foreach (var authorId in authorIds)
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.Id == authorId)
                .Select(a => new { a.Username, a.AccountType })
                .FirstOrDefaultAsync();

            if (account != null)
            {
                candidates.Add(new FollowCandidate
                {
                    AccountId = authorId,
                    Username = account.Username,
                    AccountType = account.AccountType,
                    InterestScore = 0.4 // Boosted score for engaged accounts
                });
            }
        }

        return candidates;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<int>> GetUnfollowCandidatesAsync(NpcProfile npc, int limit, int hoursBack)
    {
        var npcAccountId = npc.AccountId;
        var cutoff = DateTime.UtcNow.AddHours(-hoursBack);
        var personality = npc.Personality ?? new NpcPersonality();

        // Get accounts the NPC follows
        var following = await _context.Follows
            .Where(f => f.FollowerAccountId == npcAccountId)
            .Select(f => new { f.FollowedAccountId, f.CreatedAt })
            .ToListAsync();

        if (following.Count == 0)
            return Enumerable.Empty<int>();

        var candidates = new List<(int AccountId, double Score, DateTime FollowedAt)>();

        foreach (var follow in following)
        {
            // Check if followed account has posted recently
            var recentPosts = await _context.Posts
                .AsNoTracking()
                .CountAsync(p => p.AuthorAccountId == follow.FollowedAccountId 
                    && p.Status == PostStatus.Active 
                    && p.CreatedAt >= cutoff);

            // Calculate unfollow score
            // Higher score = more likely to unfollow
            double score = 0.0;

            // Stale follows (no recent content) increase unfollow likelihood
            if (recentPosts == 0)
            {
                score += 0.3;
            }
            else if (recentPosts <= 2)
            {
                score += 0.1;
            }

            // Neuroticism increases unfollow tendency
            score += personality.Neuroticism * 0.2;

            // Conscientiousness decreases unfollow tendency (more deliberate)
            score -= personality.Conscientiousness * 0.1;

            // Followed for a long time without interaction - slight increase
            var followAge = DateTime.UtcNow - follow.CreatedAt;
            if (followAge.TotalDays > 90)
            {
                score += 0.1;
            }

            if (score > 0.1) // Only suggest unfollows with minimum threshold
            {
                candidates.Add((follow.FollowedAccountId, score, follow.CreatedAt));
            }
        }

        // Return top candidates sorted by score descending
        return candidates
            .OrderByDescending(c => c.Score)
            .Take(limit)
            .Select(c => c.AccountId)
            .ToList();
    }

    /// <inheritdoc />
    public double CalculateReciprocityScore(NpcProfile follower, NpcProfile followee)
    {
        var personality = followee.Personality ?? new NpcPersonality();
        var accountType = followee.Account?.AccountType ?? AccountType.OrdinaryUser;

        // Base reciprocity
        double score = 0.2;

        // Agreeableness increases reciprocity
        score += personality.Agreeableness * 0.4;

        // Extraversion increases reciprocity
        score += personality.Extraversion * 0.2;

        // Celebrity/Influencer types are less likely to reciprocate
        if (accountType == AccountType.Celebrity)
            score -= 0.3;
        else if (accountType == AccountType.Influencer)
            score -= 0.2;

        // Ordinary users more likely to reciprocate
        if (accountType == AccountType.OrdinaryUser)
            score += 0.1;

        // Openness slightly increases reciprocity
        score += personality.Openness * 0.1;

        // Clamp to 0-1
        return Math.Max(0.0, Math.Min(1.0, score));
    }
}
