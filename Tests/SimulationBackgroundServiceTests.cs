using Microsoft.Extensions.Logging;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Application.Services;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class SimulationStateServiceTests
{
    [Fact]
    public void Initialize_SetsCorrectInitialState()
    {
        // Arrange
        var service = new SimulationStateService();
        var config = new SimulationConfig
        {
            Enabled = true,
            TickIntervalSeconds = 15,
            MaxNpcsPerTick = 50
        };

        // Act
        service.Initialize(config);
        var status = service.GetStatus();

        // Assert
        Assert.True(status.IsEnabled);
        Assert.Equal(15, status.TickIntervalSeconds);
        Assert.Equal(50, status.MaxNpcsPerTick);
        Assert.False(status.IsPaused);
        // IsRunning = enabled && !paused, so with Enabled=true and Paused=false, IsRunning=true
        Assert.True(status.IsRunning);
        Assert.False(status.IsTickInProgress);
        Assert.Equal(0, status.TotalTicks);
        Assert.Equal(0, status.TotalNpcsProcessed);
    }

    [Fact]
    public void Initialize_DisabledConfig_SetsIsEnabledFalse()
    {
        // Arrange
        var service = new SimulationStateService();
        var config = new SimulationConfig { Enabled = false };

        // Act
        service.Initialize(config);
        var status = service.GetStatus();

        // Assert
        Assert.False(status.IsEnabled);
        Assert.False(status.IsRunning);
    }

    [Fact]
    public void Pause_SetsIsPausedTrue()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act
        service.Pause();
        var status = service.GetStatus();

        // Assert
        Assert.True(status.IsPaused);
        Assert.False(status.IsRunning);
    }

    [Fact]
    public void Resume_ClearsIsPaused()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });
        service.Pause();

        // Act
        service.Resume();
        var status = service.GetStatus();

        // Assert
        Assert.False(status.IsPaused);
        Assert.True(status.IsRunning);
    }

    [Fact]
    public void CanStartTick_WhenPaused_ReturnsFalse()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });
        service.Pause();

        // Act & Assert
        Assert.False(service.CanStartTick());
    }

    [Fact]
    public void CanStartTick_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = false });

        // Act & Assert
        Assert.False(service.CanStartTick());
    }

    [Fact]
    public void CanStartTick_WhenTickInProgress_ReturnsFalse()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });
        service.TickStarted();

        // Act & Assert
        Assert.False(service.CanStartTick());
    }

    [Fact]
    public void CanStartTick_WhenReady_ReturnsTrue()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act & Assert
        Assert.True(service.CanStartTick());
    }

    [Fact]
    public void TickStarted_SetsTickInProgress()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act
        service.TickStarted();
        var status = service.GetStatus();

        // Assert
        Assert.True(status.IsTickInProgress);
        Assert.NotNull(status.CurrentTickStartedAt);
    }

    [Fact]
    public void TickCompleted_UpdatesCountersAndClearsInProgress()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });
        service.TickStarted();

        // Act
        service.TickCompleted(10, 150.5);
        var status = service.GetStatus();

        // Assert
        Assert.False(status.IsTickInProgress);
        Assert.Equal(1, status.TotalTicks);
        Assert.Equal(10, status.TotalNpcsProcessed);
        Assert.Equal(150.5, status.LastTickDurationMs);
        Assert.Equal(10, status.LastTickNpcsProcessed);
        Assert.NotNull(status.LastTickAt);
    }

    [Fact]
    public void TickSkipped_IncrementsSkippedCounter()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act
        service.TickSkipped();
        service.TickSkipped();
        var status = service.GetStatus();

        // Assert
        Assert.Equal(2, status.TotalTicksSkipped);
    }

    [Fact]
    public void TickFailed_ClearsInProgressAndIncrementsFailedCounter()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });
        service.TickStarted();

        // Act
        service.TickFailed();
        var status = service.GetStatus();

        // Assert
        Assert.False(status.IsTickInProgress);
        Assert.Equal(1, status.TotalTicksFailed);
    }

    [Fact]
    public void MultipleTicks_CumulativeStats()
    {
        // Arrange
        var service = new SimulationStateService();
        service.Initialize(new SimulationConfig { Enabled = true });

        // Act - Simulate 3 ticks
        service.TickStarted();
        service.TickCompleted(5, 100);
        service.TickStarted();
        service.TickCompleted(7, 120);
        service.TickStarted();
        service.TickCompleted(3, 80);

        var status = service.GetStatus();

        // Assert
        Assert.Equal(3, status.TotalTicks);
        Assert.Equal(15, status.TotalNpcsProcessed);
        Assert.Equal(3, status.LastTickNpcsProcessed);
    }
}

public class SimulationConfigTests
{
    [Fact]
    public void DefaultValues_AreSensible()
    {
        // Arrange & Act
        var config = new SimulationConfig();

        // Assert
        Assert.True(config.Enabled);
        Assert.Equal(10, config.TickIntervalSeconds);
        Assert.Equal(100, config.MaxNpcsPerTick);
        Assert.False(config.DetailedLogging);
    }

    [Fact]
    public void MinTickInterval_IsOneSecond()
    {
        Assert.Equal(1, SimulationConfig.MinTickIntervalSeconds);
    }

    [Fact]
    public void MaxTickInterval_IsOneHour()
    {
        Assert.Equal(3600, SimulationConfig.MaxTickIntervalSeconds);
    }
}

public class SimulationStatusTests
{
    [Fact]
    public void DefaultValues_AreZerosAndNulls()
    {
        // Arrange & Act
        var status = new SimulationStatus();

        // Assert
        Assert.False(status.IsRunning);
        Assert.False(status.IsPaused);
        Assert.False(status.IsEnabled);
        Assert.Equal(0, status.TickIntervalSeconds);
        Assert.Equal(0, status.TotalTicks);
        Assert.Equal(0, status.TotalNpcsProcessed);
        Assert.Equal(0, status.TotalTicksSkipped);
        Assert.Equal(0, status.TotalTicksFailed);
        Assert.Null(status.LastTickAt);
        Assert.Null(status.LastTickDurationMs);
        Assert.Equal(0, status.LastTickNpcsProcessed);
        Assert.False(status.IsTickInProgress);
        Assert.Null(status.CurrentTickStartedAt);
    }
}
