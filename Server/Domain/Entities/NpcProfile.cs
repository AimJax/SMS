namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// NPC activity state enumeration
/// </summary>
public enum NpcActivityState
{
    Idle = 0,
    Browsing = 1,
    Posting = 2,
    Reading = 3,
    Engaging = 4,
    Offline = 5
}

/// <summary>
/// NPC profile containing simulation metadata and state
/// </summary>
public class NpcProfile
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable internal identifier
    /// </summary>
    public Guid NpcId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Reference to the associated account (links to Account.Id)
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// Whether this NPC is currently active in simulation
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Current activity state
    /// </summary>
    public NpcActivityState ActivityState { get; set; } = NpcActivityState.Idle;
    
    /// <summary>
    /// When the NPC was last simulated
    /// </summary>
    public DateTime? LastSimulatedAt { get; set; }
    
    /// <summary>
    /// When the NPC should be simulated next
    /// </summary>
    public DateTime NextSimulationAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Simulation interval in seconds (how often to simulate this NPC)
    /// </summary>
    public int SimulationIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// Simulation version for tracking state changes
    /// </summary>
    public int SimulationVersion { get; set; } = 1;
    
    /// <summary>
    /// When the NPC was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the NPC was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? Account { get; set; }
    public NpcPersonality? Personality { get; set; }
    public ICollection<NpcInterest> Interests { get; set; } = new List<NpcInterest>();
}
