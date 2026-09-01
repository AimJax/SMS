namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Configuration for the LLM-driven event system
/// </summary>
public class EventConfig
{
    /// <summary>
    /// Whether the event generation system is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// How often to attempt event generation (in ticks)
    /// e.g., 5 = attempt every 5th tick
    /// </summary>
    public int EventGenerationIntervalTicks { get; set; } = 5;
    
    /// <summary>
    /// Maximum events that can be active at once
    /// </summary>
    public int MaxActiveEvents { get; set; } = 20;
    
    /// <summary>
    /// Time after which an event auto-ends (in hours)
    /// </summary>
    public int EventDurationHours { get; set; } = 24;
    
    /// <summary>
    /// Minimum time between events involving the same account (in hours)
    /// </summary>
    public int AccountEventCooldownHours { get; set; } = 2;
    
    /// <summary>
    /// Whether to automatically approve LLM-generated events
    /// (set to false for human review)
    /// </summary>
    public bool AutoApproveEvents { get; set; } = true;
    
    /// <summary>
    /// Maximum events per hour
    /// </summary>
    public int MaxEventsPerHour { get; set; } = 10;
}
