using SocialMediaSimulator.Server.Application.Models;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for managing simulation state
/// </summary>
public interface ISimulationStateService
{
    /// <summary>
    /// Get the current simulation status
    /// </summary>
    SimulationStatus GetStatus();
    
    /// <summary>
    /// Pause the simulation
    /// </summary>
    void Pause();
    
    /// <summary>
    /// Resume the simulation
    /// </summary>
    void Resume();
    
    /// <summary>
    /// Check if simulation is paused
    /// </summary>
    bool IsPaused();
    
    /// <summary>
    /// Check if a tick can start (not paused and no overlap)
    /// </summary>
    bool CanStartTick();
    
    /// <summary>
    /// Mark that a tick has started
    /// </summary>
    void TickStarted();
    
    /// <summary>
    /// Mark that a tick has completed
    /// </summary>
    void TickCompleted(int npcsProcessed, double durationMs);
    
    /// <summary>
    /// Mark that a tick was skipped due to overlap
    /// </summary>
    void TickSkipped();
    
    /// <summary>
    /// Mark that a tick failed
    /// </summary>
    void TickFailed();
    
    /// <summary>
    /// Initialize with configuration
    /// </summary>
    void Initialize(SimulationConfig config);
    
    /// <summary>
    /// Record follow/unfollow counts from a tick
    /// </summary>
    void RecordSocialGraphActivity(int follows, int unfollows);
}

/// <summary>
/// In-memory simulation state service.
/// Note: State resets on server restart (documented trade-off).
/// For persistent state tracking, a database-backed implementation would be needed.
/// </summary>
public class SimulationStateService : ISimulationStateService
{
    private readonly object _lock = new();
    private SimulationStatus _status = new();
    private bool _isPaused;
    private bool _tickInProgress;
    private DateTime? _currentTickStartTime;

    public void Initialize(SimulationConfig config)
    {
        lock (_lock)
        {
            _status = new SimulationStatus
            {
                IsEnabled = config.Enabled,
                IsRunning = false,
                IsPaused = false,
                TickIntervalSeconds = config.TickIntervalSeconds,
                MaxNpcsPerTick = config.MaxNpcsPerTick,
                TotalTicks = 0,
                TotalNpcsProcessed = 0,
                TotalTicksSkipped = 0,
                TotalTicksFailed = 0,
                LastTickAt = null,
                LastTickDurationMs = null,
                LastTickNpcsProcessed = 0,
                ServiceStartedAt = DateTime.UtcNow,
                IsTickInProgress = false,
                CurrentTickStartedAt = null,
                TotalNpcFollows = 0,
                TotalNpcUnfollows = 0,
                LastTickFollows = 0,
                LastTickUnfollows = 0
            };
            _isPaused = false;
            _tickInProgress = false;
            _currentTickStartTime = null;
        }
    }

    public SimulationStatus GetStatus()
    {
        lock (_lock)
        {
            return new SimulationStatus
            {
                IsRunning = _status.IsEnabled && !_isPaused,
                IsPaused = _isPaused,
                IsEnabled = _status.IsEnabled,
                TickIntervalSeconds = _status.TickIntervalSeconds,
                MaxNpcsPerTick = _status.MaxNpcsPerTick,
                TotalTicks = _status.TotalTicks,
                TotalNpcsProcessed = _status.TotalNpcsProcessed,
                TotalTicksSkipped = _status.TotalTicksSkipped,
                TotalTicksFailed = _status.TotalTicksFailed,
                LastTickAt = _status.LastTickAt,
                LastTickDurationMs = _status.LastTickDurationMs,
                LastTickNpcsProcessed = _status.LastTickNpcsProcessed,
                ServiceStartedAt = _status.ServiceStartedAt,
                IsTickInProgress = _tickInProgress,
                CurrentTickStartedAt = _currentTickStartTime
            };
        }
    }

    public void Pause()
    {
        lock (_lock)
        {
            _isPaused = true;
        }
    }

    public void Resume()
    {
        lock (_lock)
        {
            _isPaused = false;
        }
    }

    public bool IsPaused()
    {
        lock (_lock)
        {
            return _isPaused;
        }
    }

    public bool CanStartTick()
    {
        lock (_lock)
        {
            // Cannot start if paused
            if (_isPaused)
                return false;
            
            // Cannot start if already running
            if (_tickInProgress)
                return false;
            
            // Cannot start if disabled
            if (!_status.IsEnabled)
                return false;
            
            return true;
        }
    }

    public void TickStarted()
    {
        lock (_lock)
        {
            _tickInProgress = true;
            _currentTickStartTime = DateTime.UtcNow;
            _status.IsTickInProgress = true;
            _status.CurrentTickStartedAt = _currentTickStartTime;
        }
    }

    public void TickCompleted(int npcsProcessed, double durationMs)
    {
        lock (_lock)
        {
            _tickInProgress = false;
            _currentTickStartTime = null;
            
            _status.TotalTicks++;
            _status.TotalNpcsProcessed += npcsProcessed;
            _status.LastTickAt = DateTime.UtcNow;
            _status.LastTickDurationMs = durationMs;
            _status.LastTickNpcsProcessed = npcsProcessed;
            _status.IsTickInProgress = false;
            _status.CurrentTickStartedAt = null;
        }
    }

    public void TickSkipped()
    {
        lock (_lock)
        {
            _status.TotalTicksSkipped++;
        }
    }

    public void TickFailed()
    {
        lock (_lock)
        {
            _tickInProgress = false;
            _currentTickStartTime = null;
            _status.TotalTicksFailed++;
            _status.IsTickInProgress = false;
            _status.CurrentTickStartedAt = null;
        }
    }

    public void RecordSocialGraphActivity(int follows, int unfollows)
    {
        lock (_lock)
        {
            _status.TotalNpcFollows += follows;
            _status.TotalNpcUnfollows += unfollows;
            _status.LastTickFollows = follows;
            _status.LastTickUnfollows = unfollows;
        }
    }
}
