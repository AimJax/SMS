namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Current status of the NPC simulation background service
/// </summary>
public class SimulationStatus
{
    /// <summary>
    /// Whether the simulation is currently running (ticking)
    /// </summary>
    public bool IsRunning { get; set; }
    
    /// <summary>
    /// Whether the simulation is paused (can be resumed)
    /// </summary>
    public bool IsPaused { get; set; }
    
    /// <summary>
    /// Whether the simulation service is enabled via configuration
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Configured tick interval in seconds
    /// </summary>
    public int TickIntervalSeconds { get; set; }
    
    /// <summary>
    /// Maximum NPCs per tick
    /// </summary>
    public int MaxNpcsPerTick { get; set; }
    
    /// <summary>
    /// Total number of ticks executed since service start
    /// </summary>
    public long TotalTicks { get; set; }
    
    /// <summary>
    /// Total number of NPCs processed since service start
    /// </summary>
    public long TotalNpcsProcessed { get; set; }
    
    /// <summary>
    /// Number of ticks skipped due to overlap prevention
    /// </summary>
    public long TotalTicksSkipped { get; set; }
    
    /// <summary>
    /// Number of ticks that failed
    /// </summary>
    public long TotalTicksFailed { get; set; }
    
    /// <summary>
    /// Timestamp of the last successful tick
    /// </summary>
    public DateTime? LastTickAt { get; set; }
    
    /// <summary>
    /// Duration of the last tick in milliseconds
    /// </summary>
    public double? LastTickDurationMs { get; set; }
    
    /// <summary>
    /// Number of NPCs processed in the last tick
    /// </summary>
    public int LastTickNpcsProcessed { get; set; }
    
    /// <summary>
    /// Timestamp when the service started
    /// </summary>
    public DateTime ServiceStartedAt { get; set; }
    
    /// <summary>
    /// Whether a tick is currently in progress
    /// </summary>
    public bool IsTickInProgress { get; set; }
    
    /// <summary>
    /// Timestamp when the current tick started (if in progress)
    /// </summary>
    public DateTime? CurrentTickStartedAt { get; set; }
    
    /// <summary>
    /// Total NPC-to-NPC follow edges created since service start
    /// </summary>
    public long TotalNpcFollows { get; set; }
    
    /// <summary>
    /// Total NPC-to-NPC unfollows since service start
    /// </summary>
    public long TotalNpcUnfollows { get; set; }
    
    /// <summary>
    /// Number of follows in the last tick
    /// </summary>
    public int LastTickFollows { get; set; }
    
    /// <summary>
    /// Number of unfollows in the last tick
    /// </summary>
    public int LastTickUnfollows { get; set; }
    
    /// <summary>
    /// Total AI generation attempts since service start
    /// </summary>
    public long TotalAiAttempts { get; set; }
    
    /// <summary>
    /// Total successful AI generations since service start
    /// </summary>
    public long TotalAiSuccesses { get; set; }
    
    /// <summary>
    /// Total AI generation fallbacks to template since service start
    /// </summary>
    public long TotalAiFallbacks { get; set; }
    
    /// <summary>
    /// Last AI generation error message (if any), without sensitive data
    /// </summary>
    public string? LastAiError { get; set; }
    
    /// <summary>
    /// Currently configured AI provider name (or null if not configured)
    /// </summary>
    public string? AiProvider { get; set; }
    
    /// <summary>
    /// Currently configured AI model (or null if not configured)
    /// </summary>
    public string? AiModel { get; set; }
    
    /// <summary>
    /// Whether AI generation is currently enabled
    /// </summary>
    public bool IsAiEnabled { get; set; }
}
