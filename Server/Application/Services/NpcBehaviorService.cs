using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class NpcBehaviorService : INpcBehaviorService
{
    private readonly AppDbContext _context;
    private readonly IContentRelevanceService _relevanceService;
    private readonly IContentGeneratorService _contentGenerator;
    private readonly ISocialGraphService _socialGraph;
    private readonly INpcSocialGraphService _npcSocialGraph;

    public NpcBehaviorService(
        AppDbContext context,
        IContentRelevanceService relevanceService,
        IContentGeneratorService contentGenerator,
        ISocialGraphService socialGraph,
        INpcSocialGraphService npcSocialGraph)
    {
        _context = context;
        _relevanceService = relevanceService;
        _contentGenerator = contentGenerator;
        _socialGraph = socialGraph;
        _npcSocialGraph = npcSocialGraph;
    }

    /// <inheritdoc />
    public async Task<NpcActionResult?> ProcessBehaviorAsync(NpcProfile npc, NpcBehaviorConfig? config = null)
    {
        config ??= new NpcBehaviorConfig();
        
        var npcAccountId = npc.AccountId;
        var accountType = npc.Account?.AccountType ?? AccountType.OrdinaryUser;
        
        // Get random with optional seed for determinism
        var random = config.RandomSeed.HasValue
            ? new Random(config.RandomSeed.Value + npcAccountId)
            : new Random();

        // Generate candidates
        var candidates = (await GenerateCandidatesAsync(npc, config)).ToList();
        
        if (candidates.Count == 0)
        {
            return new NpcActionResult
            {
                Success = true,
                ActionType = NpcActionType.ViewFeed,
                WasSkipped = true
            };
        }

        // Decide whether to act (base probability check)
        if (random.NextDouble() > config.BaseActionProbability)
        {
            return new NpcActionResult
            {
                Success = true,
                ActionType = NpcActionType.ViewFeed,
                WasSkipped = true
            };
        }

        // Score and select best action
        var scoredCandidates = candidates
            .Select(c => new
            {
                Candidate = c,
                Score = CalculateScore(c, npc, accountType, random)
            })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToList();

        if (scoredCandidates.Count == 0)
        {
            return new NpcActionResult
            {
                Success = true,
                ActionType = NpcActionType.ViewFeed,
                WasSkipped = true
            };
        }

        // Probabilistic selection from top candidates
        var selected = scoredCandidates[random.Next(Math.Min(2, scoredCandidates.Count))].Candidate;

        // Execute the action
        return await ExecuteActionAsync(npc, selected, random);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NpcActionCandidate>> GenerateCandidatesAsync(NpcProfile npc, NpcBehaviorConfig config)
    {
        var candidates = new List<NpcActionCandidate>();
        var npcAccountId = npc.AccountId;
        var interests = npc.Interests.ToList();
        var accountType = npc.Account?.AccountType ?? AccountType.OrdinaryUser;

        // Always add browse/viewfeed as idle option
        candidates.Add(new NpcActionCandidate
        {
            ActionType = NpcActionType.ViewFeed,
            Reason = "Browse content"
        });

        // Generate follow candidates using the social graph service
        var followCandidates = await _npcSocialGraph.GetFollowCandidatesAsync(npc, config.MaxCandidateAccounts);
        foreach (var candidate in followCandidates)
        {
            // Calculate base score combining all factors
            var interestScore = candidate.InterestScore;
            var reciprocityScore = candidate.ReciprocityScore;
            var explorationScore = candidate.ExplorationScore;
            
            // Weight: interests most important, then reciprocity, then exploration
            var baseScore = (interestScore * 0.5) + (reciprocityScore * 0.3) + (explorationScore * 0.2);
            
            candidates.Add(new NpcActionCandidate
            {
                ActionType = NpcActionType.Follow,
                TargetAccountId = candidate.AccountId,
                BaseScore = baseScore,
                Reason = $"Follow: interest={interestScore:F2}, reciprocity={reciprocityScore:F2}, exploration={explorationScore:F2}"
            });
        }

        // Generate like/comment candidates from recent posts
        var recentPosts = await GetRecentPostsAsync(npc, config.MaxCandidatePosts, config.RecentPostsHours);
        foreach (var post in recentPosts)
        {
            var relevance = _relevanceService.CalculatePostRelevance(post, interests);
            
            // Check if already liked
            var isLiked = await _context.PostLikes
                .AnyAsync(l => l.PostId == post.Id && l.AccountId == npcAccountId);
            
            if (!isLiked)
            {
                candidates.Add(new NpcActionCandidate
                {
                    ActionType = NpcActionType.LikePost,
                    TargetPostId = post.Id,
                    BaseScore = relevance,
                    Reason = $"Like relevant post"
                });
            }

            // Comment candidates (only for high-relevance posts)
            if (relevance > 0.3)
            {
                candidates.Add(new NpcActionCandidate
                {
                    ActionType = NpcActionType.Comment,
                    TargetPostId = post.Id,
                    BaseScore = relevance * 0.8,
                    Reason = $"Comment on relevant post"
                });
            }
        }

        // Generate unfollow candidates using the social graph service
        // Unfollow if: following too many OR stale engagement OR personality-driven churn
        var followingCount = await _context.Follows
            .CountAsync(f => f.FollowerAccountId == npcAccountId);
        
        var maxUnfollowCandidates = Math.Max(config.MaxUnfollowsPerTick * 2, 5);
        var unfollowCandidates = await _npcSocialGraph.GetUnfollowCandidatesAsync(
            npc, maxUnfollowCandidates, config.RecentPostsHours * 2);
        
        foreach (var followedId in unfollowCandidates)
        {
            candidates.Add(new NpcActionCandidate
            {
                ActionType = NpcActionType.Unfollow,
                TargetAccountId = followedId,
                BaseScore = 0.15,
                Reason = "Unfollow stale/low-engagement account"
            });
        }
        
        // Also add unfollow candidates if following count is too high (fallback)
        if (followingCount > config.MaxFollowingBeforeUnfollow)
        {
            var staleFollows = await _context.Follows
                .Where(f => f.FollowerAccountId == npcAccountId)
                .OrderBy(f => f.CreatedAt)
                .Take(config.MaxUnfollowsPerTick)
                .Select(f => f.FollowedAccountId)
                .ToListAsync();

            foreach (var followedId in staleFollows)
            {
                // Skip if already added from social graph service
                if (unfollowCandidates.Contains(followedId))
                    continue;
                    
                candidates.Add(new NpcActionCandidate
                {
                    ActionType = NpcActionType.Unfollow,
                    TargetAccountId = followedId,
                    BaseScore = 0.1,
                    Reason = "Reduce following count"
                });
            }
        }

        // Generate unlike candidates (small chance)
        var recentLikes = await _context.PostLikes
            .Where(l => l.AccountId == npcAccountId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(l => l.PostId)
            .ToListAsync();

        if (recentLikes.Count > 2 && accountType != AccountType.Celebrity)
        {
            var unlikeTarget = recentLikes.Last();
            candidates.Add(new NpcActionCandidate
            {
                ActionType = NpcActionType.UnlikePost,
                TargetPostId = unlikeTarget,
                BaseScore = 0.05,
                Reason = "Unlike old post"
            });
        }

        // Generate post candidate based on account type
        var canPost = await CanPostAsync(npc, config);
        if (canPost)
        {
            candidates.Add(new NpcActionCandidate
            {
                ActionType = NpcActionType.CreatePost,
                BaseScore = GetPostingProbability(accountType),
                Reason = "Create new post",
                GeneratedContent = _contentGenerator.GeneratePostContent(npc, new Random())
            });
        }

        return candidates;
    }

    /// <inheritdoc />
    public async Task<bool> CanFollowAsync(int npcAccountId, int targetAccountId)
    {
        // Cannot follow self
        if (npcAccountId == targetAccountId)
            return false;

        // Check if blocked
        var blocked = await _context.Blocks
            .AnyAsync(b => 
                (b.BlockerAccountId == targetAccountId && b.BlockedAccountId == npcAccountId) ||
                (b.BlockerAccountId == npcAccountId && b.BlockedAccountId == targetAccountId));
        
        if (blocked)
            return false;

        // Check if already following
        var existing = await _context.Follows
            .AnyAsync(f => f.FollowerAccountId == npcAccountId && f.FollowedAccountId == targetAccountId);
        
        if (existing)
            return false;

        // Check if target account is active
        var targetAccount = await _context.Accounts.FindAsync(targetAccountId);
        if (targetAccount?.Status != AccountStatus.Active)
            return false;

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CanLikeAsync(int npcAccountId, int postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null || post.Status != PostStatus.Active)
            return false;

        // Check if already liked
        var existing = await _context.PostLikes
            .AnyAsync(l => l.PostId == postId && l.AccountId == npcAccountId);
        
        if (existing)
            return false;

        // Check block status in either direction
        var blocked = await _context.Blocks
            .AnyAsync(b => 
                (b.BlockerAccountId == post.AuthorAccountId && b.BlockedAccountId == npcAccountId) ||
                (b.BlockerAccountId == npcAccountId && b.BlockedAccountId == post.AuthorAccountId));
        
        return !blocked;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Post>> GetRecentPostsAsync(NpcProfile npc, int limit, int hoursBack)
    {
        var npcAccountId = npc.AccountId;
        var cutoff = DateTime.UtcNow.AddHours(-hoursBack);

        // Get posts from non-blocked accounts that NPC doesn't follow (for exploration)
        // or posts from followed accounts (for engagement)
        var followedIds = await _context.Follows
            .Where(f => f.FollowerAccountId == npcAccountId)
            .Select(f => f.FollowedAccountId)
            .ToListAsync();

        var blockedByIds = await _context.Blocks
            .Where(b => b.BlockedAccountId == npcAccountId)
            .Select(b => b.BlockerAccountId)
            .ToListAsync();

        var blockedIds = await _context.Blocks
            .Where(b => b.BlockerAccountId == npcAccountId)
            .Select(b => b.BlockedAccountId)
            .ToListAsync();

        var excludedIds = blockedByIds.Union(blockedIds).ToHashSet();
        excludedIds.Add(npcAccountId);

        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Active)
            .Where(p => p.CreatedAt >= cutoff)
            .Where(p => !excludedIds.Contains(p.AuthorAccountId))
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Account>> GetCandidateAccountsAsync(NpcProfile npc, int limit)
    {
        var npcAccountId = npc.AccountId;
        var interests = npc.Interests.ToList();

        // Get accounts that NPC is not already following
        var followingIds = await _context.Follows
            .Where(f => f.FollowerAccountId == npcAccountId)
            .Select(f => f.FollowedAccountId)
            .ToListAsync();

        var blockedByIds = await _context.Blocks
            .Where(b => b.BlockedAccountId == npcAccountId)
            .Select(b => b.BlockerAccountId)
            .ToListAsync();

        var blockedIds = await _context.Blocks
            .Where(b => b.BlockerAccountId == npcAccountId)
            .Select(b => b.BlockedAccountId)
            .ToListAsync();

        var excludedIds = followingIds.Union(blockedByIds).Union(blockedIds).ToHashSet();
        excludedIds.Add(npcAccountId);

        // Prefer accounts with posts (more active)
        var activeAccounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.Status == AccountStatus.Active)
            .Where(a => !excludedIds.Contains(a.Id))
            .Where(a => _context.Posts.Any(p => p.AuthorAccountId == a.Id && p.Status == PostStatus.Active))
            .OrderByDescending(a => _context.Posts.Count(p => p.AuthorAccountId == a.Id))
            .Take(limit)
            .ToListAsync();

        // If not enough active accounts, include any valid accounts
        if (activeAccounts.Count < limit)
        {
            var additionalAccounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.Status == AccountStatus.Active)
                .Where(a => !excludedIds.Contains(a.Id))
                .Where(a => !activeAccounts.Select(x => x.Id).Contains(a.Id))
                .Take(limit - activeAccounts.Count)
                .ToListAsync();

            activeAccounts.AddRange(additionalAccounts);
        }

        return activeAccounts;
    }

    private double CalculateScore(NpcActionCandidate candidate, NpcProfile npc, AccountType accountType, Random random)
    {
        var personality = npc.Personality ?? new NpcPersonality();
        
        // Base score from candidate
        var score = candidate.BaseScore;

        // Personality modifiers
        score += GetPersonalityModifier(personality, candidate.ActionType);

        // Account type modifiers
        score += GetAccountTypeModifier(accountType, candidate.ActionType);

        // Random variation (0.9 - 1.1)
        score *= 0.9 + (random.NextDouble() * 0.2);

        return Math.Max(0, Math.Min(1, score));
    }

    private double GetPersonalityModifier(NpcPersonality personality, NpcActionType actionType)
    {
        return actionType switch
        {
            // Follow: Extraversion, Openness, Agreeableness influence following behavior
            // Higher Extraversion = more outgoing
            // Higher Openness = more exploration
            // Higher Agreeableness = more reciprocation
            // Higher Neuroticism = more cautious
            NpcActionType.Follow => 
                0.1 * personality.Extraversion + 
                0.1 * personality.Openness - 
                0.1 * personality.Neuroticism +
                (personality.Extraversion > 0.6 ? 0.1 : 0) +
                (personality.Agreeableness > 0.6 ? 0.05 : 0),
            
            // LikePost: Agreeableness drives positive engagement
            NpcActionType.LikePost => 0.15 * personality.Agreeableness - 0.05 * personality.Neuroticism,
            
            // Comment: Agreeableness drives meaningful engagement
            NpcActionType.Comment => 0.2 * personality.Agreeableness + 0.1 * personality.Openness - 0.1 * personality.Neuroticism,
            
            // CreatePost: Conscientiousness for deliberate posting
            NpcActionType.CreatePost => 0.2 * personality.Conscientiousness + 0.1 * personality.Extraversion,
            
            // Unfollow: Neuroticism for cautious behavior, Conscientiousness for deliberate pruning
            NpcActionType.Unfollow => 
                0.05 * personality.Neuroticism + 
                0.02 * personality.Conscientiousness, // Conscientious NPCs prune stale follows
            
            // Search/ViewFeed: Openness drives exploration
            NpcActionType.Search => 0.15 * personality.Openness,
            NpcActionType.ViewFeed => 0.05 * personality.Openness,
            
            _ => 0.0
        };
    }

    private double GetAccountTypeModifier(AccountType accountType, NpcActionType actionType)
    {
        return (accountType, actionType) switch
        {
            // OrdinaryUser: Follow more, create less
            (AccountType.OrdinaryUser, NpcActionType.Follow) => 0.25,
            (AccountType.OrdinaryUser, NpcActionType.LikePost) => 0.15,
            (AccountType.OrdinaryUser, NpcActionType.Comment) => 0.12,
            (AccountType.OrdinaryUser, NpcActionType.CreatePost) => 0.12,
            (AccountType.OrdinaryUser, NpcActionType.ViewFeed) => 0.1,
            
            // Creator: Follow within niche, post frequently
            (AccountType.Creator, NpcActionType.CreatePost) => 0.4,
            (AccountType.Creator, NpcActionType.Follow) => 0.2, // Follow within niche
            (AccountType.Creator, NpcActionType.Comment) => 0.22,
            (AccountType.Creator, NpcActionType.LikePost) => 0.15,
            
            // Influencer: Post heavily, follow less, engage highly
            (AccountType.Influencer, NpcActionType.CreatePost) => 0.45,
            (AccountType.Influencer, NpcActionType.Follow) => 0.12, // Lower follow rate
            (AccountType.Influencer, NpcActionType.Comment) => 0.28,
            (AccountType.Influencer, NpcActionType.LikePost) => 0.22,
            (AccountType.Influencer, NpcActionType.ViewFeed) => 0.05,
            
            // Celebrity: Follow rarely, post moderately, low engagement
            (AccountType.Celebrity, NpcActionType.CreatePost) => 0.35,
            (AccountType.Celebrity, NpcActionType.Follow) => -0.2, // Very low follow rate
            (AccountType.Celebrity, NpcActionType.Unfollow) => 0.15,
            (AccountType.Celebrity, NpcActionType.LikePost) => -0.1,
            (AccountType.Celebrity, NpcActionType.Comment) => 0.08,
            
            // Official: Post for information, follow relevant accounts
            (AccountType.Official, NpcActionType.CreatePost) => 0.42,
            (AccountType.Official, NpcActionType.Follow) => 0.15, // Follow for topical relevance
            (AccountType.Official, NpcActionType.LikePost) => -0.08,
            (AccountType.Official, NpcActionType.Comment) => 0.08,
            
            // News: Post frequently, follow for relevance
            (AccountType.News, NpcActionType.CreatePost) => 0.5,
            (AccountType.News, NpcActionType.Follow) => 0.2, // Follow relevant accounts for sourcing
            (AccountType.News, NpcActionType.LikePost) => 0.05,
            (AccountType.News, NpcActionType.Comment) => 0.1,
            
            _ => 0.0
        };
    }

    private double GetPostingProbability(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.OrdinaryUser => 0.15,
            AccountType.Creator => 0.4,
            AccountType.Influencer => 0.45,
            AccountType.Celebrity => 0.35,
            AccountType.Official => 0.4,
            AccountType.News => 0.5,
            _ => 0.2
        };
    }

    private async Task<bool> CanPostAsync(NpcProfile npc, NpcBehaviorConfig config)
    {
        var npcAccountId = npc.AccountId;
        
        // Check cooldown
        var recentPost = await _context.Posts
            .Where(p => p.AuthorAccountId == npcAccountId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (recentPost != null)
        {
            var cooldownEnd = recentPost.CreatedAt.AddSeconds(config.PostCooldownSeconds);
            if (DateTime.UtcNow < cooldownEnd)
                return false;
        }

        // Check account status
        return npc.Account?.Status == AccountStatus.Active;
    }

    private async Task<NpcActionResult> ExecuteActionAsync(NpcProfile npc, NpcActionCandidate candidate, Random random)
    {
        var npcAccountId = npc.AccountId;
        var npcProfileId = npc.Id;
        var now = DateTime.UtcNow;

        try
        {
            int? actionId = null;

            switch (candidate.ActionType)
            {
                case NpcActionType.Follow:
                    if (candidate.TargetAccountId.HasValue)
                    {
                        var follow = await _socialGraph.FollowAsync(npcAccountId, candidate.TargetAccountId.Value);
                        actionId = await RecordActionAsync(npcProfileId, NpcActionType.Follow, 
                            targetAccountId: candidate.TargetAccountId.Value, executed: true);
                    }
                    break;

                case NpcActionType.Unfollow:
                    if (candidate.TargetAccountId.HasValue)
                    {
                        await _socialGraph.UnfollowAsync(npcAccountId, candidate.TargetAccountId.Value);
                        actionId = await RecordActionAsync(npcProfileId, NpcActionType.Unfollow,
                            targetAccountId: candidate.TargetAccountId.Value, executed: true);
                    }
                    break;

                case NpcActionType.LikePost:
                    if (candidate.TargetPostId.HasValue)
                    {
                        var post = await _context.Posts.FindAsync(candidate.TargetPostId.Value);
                        if (post != null)
                        {
                            await _context.PostLikes.AddAsync(new PostLike
                            {
                                PostId = post.Id,
                                AccountId = npcAccountId,
                                CreatedAt = now
                            });
                            await _context.SaveChangesAsync();
                            actionId = await RecordActionAsync(npcProfileId, NpcActionType.LikePost,
                                targetPostId: candidate.TargetPostId.Value, executed: true);
                        }
                    }
                    break;

                case NpcActionType.UnlikePost:
                    if (candidate.TargetPostId.HasValue)
                    {
                        var like = await _context.PostLikes
                            .FirstOrDefaultAsync(l => l.PostId == candidate.TargetPostId.Value && l.AccountId == npcAccountId);
                        if (like != null)
                        {
                            _context.PostLikes.Remove(like);
                            await _context.SaveChangesAsync();
                            actionId = await RecordActionAsync(npcProfileId, NpcActionType.UnlikePost,
                                targetPostId: candidate.TargetPostId.Value, executed: true);
                        }
                    }
                    break;

                case NpcActionType.Comment:
                    if (candidate.TargetPostId.HasValue && !string.IsNullOrEmpty(candidate.GeneratedContent))
                    {
                        var post = await _context.Posts.FindAsync(candidate.TargetPostId.Value);
                        if (post != null)
                        {
                            await _context.Comments.AddAsync(new Comment
                            {
                                PostId = post.Id,
                                AuthorAccountId = npcAccountId,
                                Content = candidate.GeneratedContent,
                                CreatedAt = now
                            });
                            await _context.SaveChangesAsync();
                            actionId = await RecordActionAsync(npcProfileId, NpcActionType.Comment,
                                targetPostId: candidate.TargetPostId.Value, content: candidate.GeneratedContent, executed: true);
                        }
                    }
                    break;

                case NpcActionType.CreatePost:
                    if (!string.IsNullOrEmpty(candidate.GeneratedContent))
                    {
                        await _context.Posts.AddAsync(new Post
                        {
                            AuthorAccountId = npcAccountId,
                            Content = candidate.GeneratedContent,
                            Status = PostStatus.Active,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                        await _context.SaveChangesAsync();
                        actionId = await RecordActionAsync(npcProfileId, NpcActionType.CreatePost,
                            content: candidate.GeneratedContent, executed: true);
                    }
                    break;

                case NpcActionType.ViewFeed:
                case NpcActionType.ViewPost:
                case NpcActionType.Search:
                    // These are informational actions - just record them
                    actionId = await RecordActionAsync(npcProfileId, candidate.ActionType,
                        targetAccountId: candidate.TargetAccountId,
                        targetPostId: candidate.TargetPostId,
                        executed: true);
                    break;
            }

            return new NpcActionResult
            {
                Success = true,
                ActionType = candidate.ActionType,
                NpcActionId = actionId
            };
        }
        catch (Exception ex)
        {
            // Record failed action
            await RecordActionAsync(npcProfileId, candidate.ActionType,
                targetAccountId: candidate.TargetAccountId,
                targetPostId: candidate.TargetPostId,
                content: candidate.GeneratedContent,
                executed: false);

            return new NpcActionResult
            {
                Success = false,
                ActionType = candidate.ActionType,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<int> RecordActionAsync(
        int npcProfileId,
        NpcActionType actionType,
        int? targetAccountId = null,
        int? targetPostId = null,
        string? content = null,
        bool executed = false)
    {
        var npcAction = new NpcAction
        {
            NpcProfileId = npcProfileId,
            ActionType = actionType,
            TargetAccountId = targetAccountId?.ToString(),
            TargetPostId = targetPostId?.ToString(),
            Content = content,
            Executed = executed,
            ScheduledAt = DateTime.UtcNow,
            ExecutedAt = executed ? DateTime.UtcNow : null
        };

        _context.NpcActions.Add(npcAction);
        await _context.SaveChangesAsync();
        
        return npcAction.Id;
    }
}
