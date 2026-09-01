using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing rumors and misinformation spreading on the platform
/// </summary>
public interface IRumorService
{
    // === Rumor Management ===
    
    /// <summary>
    /// Create a new rumor from a post
    /// </summary>
    Task<Rumor?> CreateRumorFromPostAsync(Guid postId, int? accountId = null);
    
    /// <summary>
    /// Get a rumor by its stable ID
    /// </summary>
    Task<Rumor?> GetRumorByIdAsync(Guid rumorId);
    
    /// <summary>
    /// Get all notable rumors
    /// </summary>
    Task<List<Rumor>> GetNotableRumorsAsync(int count = 20);
    
    /// <summary>
    /// Get rumors by truth status
    /// </summary>
    Task<List<Rumor>> GetRumorsByStatusAsync(RumorTruthStatus status, int count = 20);
    
    /// <summary>
    /// Get rumors spreading in a community
    /// </summary>
    Task<List<Rumor>> GetCommunityRumorsAsync(int communityId, int count = 20);
    
    /// <summary>
    /// Update rumor status (mark as confirmed, debunked, etc.)
    /// </summary>
    Task<Rumor?> UpdateRumorStatusAsync(Guid rumorId, RumorTruthStatus status);
    
    // === Belief Management ===
    
    /// <summary>
    /// Get an account's belief about a rumor
    /// </summary>
    Task<AccountBelief?> GetAccountBeliefAsync(int accountId, Guid rumorId);
    
    /// <summary>
    /// Get all beliefs for an account
    /// </summary>
    Task<List<AccountBelief>> GetAccountBeliefsAsync(int accountId);
    
    /// <summary>
    /// Update an account's belief about a rumor
    /// </summary>
    Task<AccountBelief?> UpdateBeliefAsync(int accountId, Guid rumorId, RumorTruthStatus belief, double confidence = 0.5);
    
    /// <summary>
    /// Form a belief about a rumor based on exposure
    /// </summary>
    Task<AccountBelief?> FormBeliefFromExposureAsync(int accountId, Guid rumorId, string influenceSource);
    
    // === Evidence Management ===
    
    /// <summary>
    /// Add evidence to a rumor
    /// </summary>
    Task<RumorEvidence?> AddEvidenceAsync(Guid rumorId, int? accountId, string description, bool supportsRumor, string? sourceUrl = null);
    
    /// <summary>
    /// Get evidence for a rumor
    /// </summary>
    Task<List<RumorEvidence>> GetRumorEvidenceAsync(Guid rumorId);
    
    /// <summary>
    /// Evaluate rumor truth based on evidence
    /// </summary>
    Task<Rumor?> EvaluateRumorTruthAsync(Guid rumorId);
    
    // === Rumor Processing ===
    
    /// <summary>
    /// Check if a post contains rumor-like content
    /// </summary>
    Task<bool> ContainsRumorContentAsync(string content);
    
    /// <summary>
    /// Extract claims from content that could be rumors
    /// </summary>
    Task<List<string>> ExtractClaimsAsync(string content);
    
    /// <summary>
    /// Process rumor spread (call periodically)
    /// </summary>
    Task ProcessRumorsTickAsync();
}

/// <summary>
/// Configuration for rumor system
/// </summary>
public class RumorConfig
{
    public bool Enabled { get; set; } = true;
    public int MinEngagementForRumor { get; set; } = 10;
    public int RumorWindowHours { get; set; } = 48;
    public float BaseSpreadProbability { get; set; } = 0.05f;
    public float PlantedSpreadMultiplier { get; set; } = 2.0f;
    public int MaxNotableRumors { get; set; } = 50;
    public int EvidenceThresholdForResolution { get; set; } = 3;
    public int ProcessingIntervalMinutes { get; set; } = 10;
}
