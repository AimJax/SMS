namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Configuration for the NPC simulation background service
/// </summary>
public class SimulationConfig
{
    /// <summary>
    /// Whether the simulation background service is enabled
    /// Default: true for development, can be disabled for testing
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Interval between simulation ticks in seconds
    /// Default: 10 seconds (balanced for development and responsiveness)
    /// </summary>
    public int TickIntervalSeconds { get; set; } = 10;
    
    /// <summary>
    /// Maximum number of NPCs to process per tick
    /// </summary>
    public int MaxNpcsPerTick { get; set; } = 100;
    
    /// <summary>
    /// Whether to log detailed NPC processing information
    /// Default: false (avoids log spam)
    /// </summary>
    public bool DetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Minimum tick interval in seconds (for safety)
    /// </summary>
    public const int MinTickIntervalSeconds = 1;
    
    /// <summary>
    /// Maximum tick interval in seconds (for safety)
    /// </summary>
    public const int MaxTickIntervalSeconds = 3600;
}
