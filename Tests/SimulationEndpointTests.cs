using Microsoft.Extensions.DependencyInjection;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using Xunit;

namespace SocialMediaSimulator.Tests;

/// <summary>
/// Unit tests for SimulationController endpoints logic.
/// Full integration tests would require the actual server to be running.
/// </summary>
public class SimulationEndpointTests
{
    [Fact]
    public void SimulationStateService_ProvidesStatusData()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig
        {
            Enabled = true,
            TickIntervalSeconds = 10,
            MaxNpcsPerTick = 100
        });

        // Act - Simulate some activity
        service.TickStarted();
        service.TickCompleted(5, 100.5);
        service.Pause();

        var status = service.GetStatus();

        // Assert
        Assert.True(status.IsEnabled);
        Assert.True(status.IsPaused);
        Assert.False(status.IsRunning);
        Assert.Equal(10, status.TickIntervalSeconds);
        Assert.Equal(100, status.MaxNpcsPerTick);
        Assert.Equal(1, status.TotalTicks);
        Assert.Equal(5, status.TotalNpcsProcessed);
        Assert.Equal(100.5, status.LastTickDurationMs);
    }

    [Fact]
    public void PauseResume_CorrectlyTogglesState()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act & Assert - Initial state
        Assert.False(service.IsPaused());
        Assert.True(service.GetStatus().IsRunning);

        // Pause
        service.Pause();
        Assert.True(service.IsPaused());
        Assert.False(service.GetStatus().IsRunning);
        Assert.False(service.CanStartTick());

        // Resume
        service.Resume();
        Assert.False(service.IsPaused());
        Assert.True(service.GetStatus().IsRunning);
        Assert.True(service.CanStartTick());
    }

    [Fact]
    public void TickLifecycle_TracksProgress()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act - Simulate tick lifecycle
        Assert.True(service.CanStartTick());
        
        service.TickStarted();
        Assert.False(service.CanStartTick()); // Can't start while running
        Assert.True(service.GetStatus().IsTickInProgress);

        service.TickCompleted(10, 250.0);
        
        // After completion
        Assert.False(service.GetStatus().IsTickInProgress);
        Assert.True(service.CanStartTick());
        Assert.Equal(1, service.GetStatus().TotalTicks);
        Assert.Equal(10, service.GetStatus().LastTickNpcsProcessed);
    }

    [Fact]
    public void TickSkipped_IncrementsCounter()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act - Skip a tick (simulate overlap prevention)
        service.TickStarted();
        service.TickSkipped(); // Called when a tick is skipped
        service.TickCompleted(0, 0); // Complete with 0 processed

        // Assert
        Assert.Equal(1, service.GetStatus().TotalTicksSkipped);
    }

    [Fact]
    public void TickFailed_RecordsErrorAndContinues()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act - Simulate a failed tick
        service.TickStarted();
        service.TickFailed();

        // Assert - Should be able to continue
        Assert.Equal(1, service.GetStatus().TotalTicksFailed);
        Assert.False(service.GetStatus().IsTickInProgress);
        Assert.True(service.CanStartTick()); // Should be able to start next tick
    }

    [Fact]
    public void DisabledSimulation_CannotStartTicks()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = false });

        // Act & Assert
        Assert.False(service.CanStartTick());
        Assert.False(service.GetStatus().IsRunning);
        Assert.False(service.GetStatus().IsEnabled);
    }

    [Fact]
    public void OverlapPrevention_BlocksConcurrentTicks()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });
        service.TickStarted();

        // Act & Assert - Multiple attempts to start while one is running
        Assert.False(service.CanStartTick()); // First tick in progress
        
        // Simulate multiple rapid calls
        var canStartResults = new List<bool>();
        for (int i = 0; i < 5; i++)
        {
            canStartResults.Add(service.CanStartTick());
        }

        // Assert - All should be false
        Assert.All(canStartResults, r => Assert.False(r));
        
        // Complete the tick
        service.TickCompleted(5, 100);
        Assert.True(service.CanStartTick());
    }
}
