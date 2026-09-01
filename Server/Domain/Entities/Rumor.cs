namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Truth status of a rumor
/// </summary>
public enum RumorTruthStatus
{
    /// <summary>
    /// Unverified - spreading without confirmation
    /// </summary>
    Unverified = 0,
    
    /// <summary>
    /// Confirmed true by evidence
    /// </summary>
    ConfirmedTrue = 1,
    
    /// <summary>
    /// Confirmed false by evidence
    /// </summary>
    ConfirmedFalse = 2,
    
    /// <summary>
    /// Partially true - some aspects verified
    /// </summary>
    PartiallyTrue = 3,
    
    /// <summary>
    /// Debunked as misinformation
    /// </summary>
    Debunked = 4,
    
    /// <summary>
    /// Status unknown - still under investigation
    /// </summary>
    Unknown = 5
}

/// <summary>
/// How a rumor spreads
/// </summary>
public enum RumorSpreadType
{
    /// <summary>
    /// Organic spread through engagement
    /// </summary>
    Organic = 0,
    
    /// <summary>
    /// Deliberately planted by account
    /// </summary>
    Planted = 1,
    
    /// <summary>
    /// From news source
    /// </summary>
    NewsSource = 2,
    
    /// <summary>
    /// From satirical source
    /// </summary>
    Satire = 3
}

/// <summary>
/// Represents a rumor - unverified or disputed information spreading on the platform
/// </summary>
public class Rumor
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid RumorId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Account that originated the rumor (nullable for system rumors)
    /// </summary>
    public int? OriginAccountId { get; set; }
    
    /// <summary>
    /// The claim/assertion being spread
    /// </summary>
    public string Claim { get; set; } = string.Empty;
    
    /// <summary>
    /// Summary of the rumor for display
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    /// <summary>
    /// Current truth status
    /// </summary>
    public RumorTruthStatus TruthStatus { get; set; } = RumorTruthStatus.Unverified;
    
    /// <summary>
    /// How the rumor spread initially
    /// </summary>
    public RumorSpreadType SpreadType { get; set; } = RumorSpreadType.Organic;
    
    /// <summary>
    /// Post that first spread this rumor
    /// </summary>
    public Guid? SourcePostId { get; set; }
    
    /// <summary>
    /// Number of times this rumor has been shared
    /// </summary>
    public int ShareCount { get; set; }
    
    /// <summary>
    /// Number of unique accounts that spread this rumor
    /// </summary>
    public int SpreadByCount { get; set; }
    
    /// <summary>
    /// Peak engagement velocity (shares per hour)
    /// </summary>
    public float PeakVelocity { get; set; }
    
    /// <summary>
    /// When the rumor first appeared
    /// </summary>
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the rumor peaked (most active spread)
    /// </summary>
    public DateTime? PeakedAt { get; set; }
    
    /// <summary>
    /// When the truth status was last updated
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
    
    /// <summary>
    /// LLM-generated analysis of the rumor
    /// </summary>
    public string Analysis { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the rumor is currently active (still spreading)
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether this is a notable/discoverable rumor
    /// </summary>
    public bool IsNotable { get; set; }
    
    /// <summary>
    /// Community this rumor originated in (nullable)
    /// </summary>
    public int? CommunityId { get; set; }
    
    /// <summary>
    /// When the rumor record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? OriginAccount { get; set; }
    public Post? SourcePost { get; set; }
    public Community? Community { get; set; }
    public ICollection<AccountBelief> Beliefs { get; set; } = new List<AccountBelief>();
    public ICollection<RumorEvidence> Evidence { get; set; } = new List<RumorEvidence>();
}

/// <summary>
/// An account's belief about a specific rumor
/// </summary>
public class AccountBelief
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid AccountBeliefId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Account that holds this belief
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// The rumor this belief is about
    /// </summary>
    public Guid RumorId { get; set; }
    
    /// <summary>
    /// What the account believes: true, false, or unknown
    /// </summary>
    public RumorTruthStatus Belief { get; set; } = RumorTruthStatus.Unverified;
    
    /// <summary>
    /// How strongly the account believes this (0.0 - 1.0)
    /// </summary>
    public double Confidence { get; set; } = 0.5;
    
    /// <summary>
    /// When the account formed this belief
    /// </summary>
    public DateTime FormedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the belief was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Source that influenced this belief
    /// </summary>
    public string? InfluenceSource { get; set; }
    
    // Navigation properties
    public Account? Account { get; set; }
    public Rumor? Rumor { get; set; }
}

/// <summary>
/// Evidence supporting or contradicting a rumor
/// </summary>
public class RumorEvidence
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid EvidenceId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The rumor this evidence relates to
    /// </summary>
    public Guid RumorId { get; set; }
    
    /// <summary>
    /// Account that provided this evidence
    /// </summary>
    public int? AccountId { get; set; }
    
    /// <summary>
    /// Description of the evidence
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// URL or reference to the evidence
    /// </summary>
    public string? SourceUrl { get; set; }
    
    /// <summary>
    /// Whether this evidence supports or contradicts the rumor
    /// </summary>
    public bool SupportsRumor { get; set; }
    
    /// <summary>
    /// Credibility rating of this evidence (0-10)
    /// </summary>
    public int CredibilityScore { get; set; } = 5;
    
    /// <summary>
    /// When this evidence was added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? Account { get; set; }
    public Rumor? Rumor { get; set; }
}
