using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for NPC decision-making and action selection
/// </summary>
public interface INpcDecisionService
{
    /// <summary>
    /// Evaluate candidates and select the best action
    /// </summary>
    NpcActionDecision EvaluateAndSelect(NpcProfile npc, IEnumerable<NpcActionCandidate> candidates, Random random);
    
    /// <summary>
    /// Calculate personality modifier for an action type
    /// </summary>
    double GetPersonalityModifier(NpcPersonality personality, NpcActionType actionType);
    
    /// <summary>
    /// Calculate account type modifier for an action type
    /// </summary>
    double GetAccountTypeModifier(AccountType accountType, NpcActionType actionType);
    
    /// <summary>
    /// Calculate final score for a candidate
    /// </summary>
    double CalculateFinalScore(NpcActionCandidate candidate, NpcPersonality personality, AccountType accountType, double relevance);
}

/// <summary>
/// Deterministic NPC decision-making service
/// </summary>
public class NpcDecisionService : INpcDecisionService
{
    // Base weights for each action type (0.0 - 1.0)
    private static readonly Dictionary<NpcActionType, double> BaseActionWeights = new()
    {
        [NpcActionType.ViewFeed] = 0.5,
        [NpcActionType.ViewPost] = 0.3,
        [NpcActionType.LikePost] = 0.5,
        [NpcActionType.UnlikePost] = 0.1,
        [NpcActionType.Comment] = 0.3,
        [NpcActionType.Follow] = 0.4,
        [NpcActionType.Unfollow] = 0.15,
        [NpcActionType.CreatePost] = 0.35,
        [NpcActionType.Search] = 0.2
    };

    /// <inheritdoc />
    public NpcActionDecision EvaluateAndSelect(NpcProfile npc, IEnumerable<NpcActionCandidate> candidates, Random random)
    {
        var candidateList = candidates.ToList();
        
        if (candidateList.Count == 0)
        {
            return new NpcActionDecision
            {
                HasAction = false,
                IdleReason = "No valid candidates available"
            };
        }

        var personality = npc.Personality ?? new NpcPersonality();
        var accountType = npc.Account?.AccountType ?? AccountType.OrdinaryUser;
        
        // Score all candidates
        var scoredCandidates = new List<(NpcActionCandidate Candidate, double Score)>();
        
        foreach (var candidate in candidateList)
        {
            var score = CalculateFinalScore(candidate, personality, accountType, candidate.BaseScore);
            scoredCandidates.Add((candidate, score));
        }

        // Apply probabilistic selection based on scores
        // Higher score = higher probability of selection
        var totalScore = scoredCandidates.Sum(s => s.Score);
        
        if (totalScore <= 0)
        {
            return new NpcActionDecision
            {
                HasAction = false,
                IdleReason = "All candidates scored below threshold",
                Candidates = candidateList
            };
        }

        // Weighted random selection
        var roll = random.NextDouble() * totalScore;
        double cumulative = 0;
        
        foreach (var (candidate, score) in scoredCandidates)
        {
            cumulative += score;
            if (roll <= cumulative)
            {
                return new NpcActionDecision
                {
                    HasAction = true,
                    SelectedAction = candidate,
                    Candidates = candidateList
                };
            }
        }

        // Fallback to highest scoring
        var best = scoredCandidates.OrderByDescending(s => s.Score).First();
        return new NpcActionDecision
        {
            HasAction = true,
            SelectedAction = best.Candidate,
            Candidates = candidateList
        };
    }

    /// <inheritdoc />
    public double GetPersonalityModifier(NpcPersonality personality, NpcActionType actionType)
    {
        return actionType switch
        {
            // Extraversion affects social actions
            NpcActionType.Follow => 0.1 * personality.Openness + 
                (personality.Extraversion > 0.6 ? 0.2 * personality.Extraversion : 
                 personality.Extraversion < 0.4 ? -0.15 * (1 - personality.Extraversion) : 0),
            
            // Agreeableness affects positive engagement
            NpcActionType.LikePost => 0.15 * personality.Agreeableness - 0.1 * personality.Neuroticism,
            NpcActionType.Comment => 0.2 * personality.Agreeableness + 0.15 * personality.Openness - 0.1 * personality.Neuroticism,
            
            // Conscientiousness affects posting consistency
            NpcActionType.CreatePost => personality.Conscientiousness > 0.6 ? 0.25 * personality.Conscientiousness : 0.1 * personality.Extraversion,
            NpcActionType.ViewFeed => personality.Conscientiousness > 0.6 ? 0.1 * personality.Conscientiousness : 0,
            
            // Openness affects exploration
            NpcActionType.Search => 0.2 * personality.Openness,
            
            // Neuroticism affects unfollowing
            NpcActionType.Unfollow => 0.05 * personality.Neuroticism,
            
            _ => 0.0
        };
    }

    /// <inheritdoc />
    public double GetAccountTypeModifier(AccountType accountType, NpcActionType actionType)
    {
        return (accountType, actionType) switch
        {
            // OrdinaryUser modifiers
            (AccountType.OrdinaryUser, NpcActionType.ViewFeed) => 0.3,
            (AccountType.OrdinaryUser, NpcActionType.ViewPost) => 0.2,
            (AccountType.OrdinaryUser, NpcActionType.LikePost) => 0.2,
            (AccountType.OrdinaryUser, NpcActionType.Comment) => 0.15,
            (AccountType.OrdinaryUser, NpcActionType.Follow) => 0.25,
            (AccountType.OrdinaryUser, NpcActionType.CreatePost) => 0.15,
            
            // Creator modifiers
            (AccountType.Creator, NpcActionType.CreatePost) => 0.5,
            (AccountType.Creator, NpcActionType.ViewFeed) => 0.15,
            (AccountType.Creator, NpcActionType.LikePost) => 0.2,
            (AccountType.Creator, NpcActionType.Comment) => 0.25,
            (AccountType.Creator, NpcActionType.Follow) => 0.2,
            
            // Influencer modifiers
            (AccountType.Influencer, NpcActionType.CreatePost) => 0.5,
            (AccountType.Influencer, NpcActionType.LikePost) => 0.25,
            (AccountType.Influencer, NpcActionType.Comment) => 0.3,
            (AccountType.Influencer, NpcActionType.Follow) => 0.15,
            (AccountType.Influencer, NpcActionType.ViewFeed) => 0.1,
            
            // Celebrity modifiers
            (AccountType.Celebrity, NpcActionType.CreatePost) => 0.4,
            (AccountType.Celebrity, NpcActionType.LikePost) => -0.1,
            (AccountType.Celebrity, NpcActionType.Comment) => 0.1,
            (AccountType.Celebrity, NpcActionType.Follow) => -0.15,
            (AccountType.Celebrity, NpcActionType.Unfollow) => 0.1,
            
            // Official modifiers
            (AccountType.Official, NpcActionType.CreatePost) => 0.45,
            (AccountType.Official, NpcActionType.ViewFeed) => 0.1,
            (AccountType.Official, NpcActionType.LikePost) => -0.1,
            (AccountType.Official, NpcActionType.Comment) => 0.1,
            (AccountType.Official, NpcActionType.Follow) => 0.1,
            
            // News modifiers
            (AccountType.News, NpcActionType.CreatePost) => 0.6,
            (AccountType.News, NpcActionType.ViewFeed) => 0.15,
            (AccountType.News, NpcActionType.Follow) => 0.2,
            (AccountType.News, NpcActionType.LikePost) => 0.05,
            (AccountType.News, NpcActionType.Comment) => 0.1,
            
            _ => 0.0
        };
    }

    /// <inheritdoc />
    public double CalculateFinalScore(NpcActionCandidate candidate, NpcPersonality personality, AccountType accountType, double relevance)
    {
        // Base weight for action type
        var baseWeight = BaseActionWeights.GetValueOrDefault(candidate.ActionType, 0.3);
        
        // Personality modifier
        var personalityMod = GetPersonalityModifier(personality, candidate.ActionType);
        
        // Account type modifier
        var accountTypeMod = GetAccountTypeModifier(accountType, candidate.ActionType);
        
        // Content relevance
        var relevanceMod = relevance * 0.3;
        
        // Combine scores
        var finalScore = baseWeight + personalityMod + accountTypeMod + relevanceMod;
        
        // Clamp to 0.0 - 1.0 range
        return Math.Max(0.0, Math.Min(1.0, finalScore));
    }
}
