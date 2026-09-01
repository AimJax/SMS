using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using Xunit;

namespace SocialMediaSimulator.Tests;

/// <summary>
/// Tests for CausalChain entity and related models
/// </summary>
public class CausalChainEntityTests
{
    [Fact]
    public void CausalChain_DefaultValues()
    {
        // Arrange & Act
        var chain = new CausalChain();

        // Assert
        Assert.NotEqual(Guid.Empty, chain.CausalChainId);
        Assert.Equal(CauseType.Direct, chain.CauseType);
        Assert.Equal(1.0, chain.CauseStrength);
        Assert.Equal("{}", chain.Metadata);
        Assert.Null(chain.AccountId);
    }

    [Fact]
    public void CausalChain_CanSetAllProperties()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var causeEventId = Guid.NewGuid();
        var accountId = 1;

        // Act
        var chain = new CausalChain
        {
            EventId = eventId,
            CauseEventId = causeEventId,
            CauseType = CauseType.Trigger,
            CauseDescription = "Test cause",
            CauseStrength = 0.8,
            AccountId = accountId,
            Metadata = "{\"key\": \"value\"}"
        };

        // Assert
        Assert.Equal(eventId, chain.EventId);
        Assert.Equal(causeEventId, chain.CauseEventId);
        Assert.Equal(CauseType.Trigger, chain.CauseType);
        Assert.Equal("Test cause", chain.CauseDescription);
        Assert.Equal(0.8, chain.CauseStrength);
        Assert.Equal(accountId, chain.AccountId);
    }
}

/// <summary>
/// Tests for CauseType enum
/// </summary>
public class CauseTypeTests
{
    [Theory]
    [InlineData(CauseType.Direct)]
    [InlineData(CauseType.Indirect)]
    [InlineData(CauseType.Contributing)]
    [InlineData(CauseType.Trigger)]
    public void CauseType_AllTypesAreValid(CauseType causeType)
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(CauseType), causeType));
    }

    [Fact]
    public void CauseType_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)CauseType.Direct);
        Assert.Equal(1, (int)CauseType.Indirect);
        Assert.Equal(2, (int)CauseType.Contributing);
        Assert.Equal(3, (int)CauseType.Trigger);
    }
}

/// <summary>
/// Tests for OfflineSimulationResult entity
/// </summary>
public class OfflineSimulationResultTests
{
    [Fact]
    public void OfflineSimulationResult_DefaultValues()
    {
        // Arrange & Act
        var result = new OfflineSimulationResult();

        // Assert
        Assert.NotEqual(Guid.Empty, result.OfflineSimulationResultId);
        Assert.Equal(0, result.PostsCreated);
        Assert.Equal(0, result.FollowersGained);
        Assert.Equal(0, result.FollowersLost);
        Assert.Equal(0, result.EventsCreated);
        Assert.Equal(0, result.NotificationsCreated);
        Assert.Equal("[]", result.EventsSummaryJson);
        Assert.False(result.IsAcknowledged);
    }

    [Fact]
    public void OfflineSimulationResult_CanSetAllProperties()
    {
        // Arrange
        var resultId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(-5);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Act
        var result = new OfflineSimulationResult
        {
            AccountId = 1,
            StartTime = startTime,
            EndTime = endTime,
            Duration = duration,
            TicksSimulated = 50,
            PostsCreated = 10,
            FollowersGained = 25,
            FollowersLost = 3,
            EventsCreated = 2,
            NotificationsCreated = 15,
            CatchupSummary = "Test summary",
            IsAcknowledged = false
        };

        // Assert
        Assert.Equal(1, result.AccountId);
        Assert.Equal(startTime, result.StartTime);
        Assert.Equal(endTime, result.EndTime);
        Assert.True(Math.Abs(result.Duration.TotalSeconds - 18000) < 1); // ~5 hours
        Assert.Equal(50, result.TicksSimulated);
        Assert.Equal(10, result.PostsCreated);
        Assert.Equal(25, result.FollowersGained);
        Assert.Equal(3, result.FollowersLost);
        Assert.Equal(2, result.EventsCreated);
        Assert.Equal(15, result.NotificationsCreated);
        Assert.Equal("Test summary", result.CatchupSummary);
        Assert.False(result.IsAcknowledged);
    }
}

/// <summary>
/// Tests for CatchupSummary model
/// </summary>
public class CatchupSummaryTests
{
    [Fact]
    public void CatchupSummary_DefaultValues()
    {
        // Arrange & Act
        var summary = new CatchupSummary();

        // Assert
        Assert.Empty(summary.MajorEvents);
        Assert.Equal(TimeSpan.Zero, summary.Duration);
        Assert.False(summary.IsAcknowledged);
    }

    [Fact]
    public void CatchupSummary_CanSetAllProperties()
    {
        // Arrange
        var offlineSince = DateTime.UtcNow.AddHours(-5);
        var offlineUntil = DateTime.UtcNow;

        // Act
        var summary = new CatchupSummary
        {
            OfflineSimulationResultId = Guid.NewGuid(),
            Duration = TimeSpan.FromHours(5),
            OfflineSince = offlineSince,
            OfflineUntil = offlineUntil,
            NewFollowers = 25,
            LostFollowers = 3,
            NotificationsCreated = 15,
            PostsCreated = 10,
            MajorEvents = new List<EventSummary>
            {
                new EventSummary
                {
                    EventId = Guid.NewGuid(),
                    Type = "Drama",
                    Title = "Test Event",
                    DramaLevel = 7,
                    ParticipantCount = 5
                }
            },
            Summary = "Test catchup summary",
            IsAcknowledged = false
        };

        // Assert
        Assert.Equal(TimeSpan.FromHours(5), summary.Duration);
        Assert.Equal(25, summary.NewFollowers);
        Assert.Equal(3, summary.LostFollowers);
        Assert.Single(summary.MajorEvents);
        Assert.Equal("Test Event", summary.MajorEvents[0].Title);
    }
}

/// <summary>
/// Tests for EventSummary model
/// </summary>
public class EventSummaryTests
{
    [Fact]
    public void EventSummary_CanSetAllProperties()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var summary = new EventSummary
        {
            EventId = eventId,
            Type = "Drama",
            Title = "Test Event",
            DramaLevel = 8,
            ParticipantCount = 10
        };

        // Assert
        Assert.Equal(eventId, summary.EventId);
        Assert.Equal("Drama", summary.Type);
        Assert.Equal("Test Event", summary.Title);
        Assert.Equal(8, summary.DramaLevel);
        Assert.Equal(10, summary.ParticipantCount);
    }
}

/// <summary>
/// Tests for Event chain properties
/// </summary>
public class EventChainTests
{
    [Fact]
    public void Event_ChainProperties_DefaultToNull()
    {
        // Arrange
        var evt = new Event();

        // Assert
        Assert.Null(evt.ParentEventId);
        Assert.Null(evt.TriggerEventId);
        Assert.Null(evt.EventChainId);
        Assert.Equal(0, evt.ChainDepth);
    }

    [Fact]
    public void Event_CanSetChainProperties()
    {
        // Arrange
        var chainId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();

        // Act
        var evt = new Event
        {
            EventChainId = chainId,
            ParentEventId = parentId,
            TriggerEventId = triggerId,
            ChainDepth = 3
        };

        // Assert
        Assert.Equal(chainId, evt.EventChainId);
        Assert.Equal(parentId, evt.ParentEventId);
        Assert.Equal(triggerId, evt.TriggerEventId);
        Assert.Equal(3, evt.ChainDepth);
    }

    [Fact]
    public void Event_ChainDepth_MonotonicallyIncreases()
    {
        // Arrange
        var parentEvent = new Event
        {
            Title = "Parent",
            ChainDepth = 0
        };
        
        var childEvent = new Event
        {
            Title = "Child",
            ParentEventId = parentEvent.EventId,
            ChainDepth = parentEvent.ChainDepth + 1
        };
        
        var grandchildEvent = new Event
        {
            Title = "Grandchild",
            ParentEventId = childEvent.EventId,
            ChainDepth = childEvent.ChainDepth + 1
        };

        // Assert
        Assert.Equal(0, parentEvent.ChainDepth);
        Assert.Equal(1, childEvent.ChainDepth);
        Assert.Equal(2, grandchildEvent.ChainDepth);
    }
}

/// <summary>
/// Tests for OfflineSimulationConfig
/// </summary>
public class OfflineSimulationConfigTests
{
    [Fact]
    public void OfflineSimulationConfig_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new OfflineSimulationConfig();

        // Assert
        Assert.True(config.Enabled);
        Assert.Equal(1, config.MinOfflineHoursBeforeSimulation);
        Assert.Equal(10, config.TicksPerHour);
        Assert.Equal(1000, config.MaxTicksPerSession);
        Assert.Equal(5, config.MinTicksToSimulate);
        Assert.Equal(0.5, config.EventProbabilityMultiplier);
    }

    [Fact]
    public void OfflineSimulationConfig_CanBeCustomized()
    {
        // Arrange & Act
        var config = new OfflineSimulationConfig
        {
            Enabled = false,
            MinOfflineHoursBeforeSimulation = 2,
            TicksPerHour = 20,
            MaxTicksPerSession = 500,
            MinTicksToSimulate = 10,
            EventProbabilityMultiplier = 0.3
        };

        // Assert
        Assert.False(config.Enabled);
        Assert.Equal(2, config.MinOfflineHoursBeforeSimulation);
        Assert.Equal(20, config.TicksPerHour);
        Assert.Equal(500, config.MaxTicksPerSession);
        Assert.Equal(10, config.MinTicksToSimulate);
        Assert.Equal(0.3, config.EventProbabilityMultiplier);
    }
}

/// <summary>
/// Tests for ICausalTrackingService interface
/// </summary>
public class ICausalTrackingServiceTests
{
    [Fact]
    public void ICausalTrackingService_InterfaceDefinesRequiredMethods()
    {
        // Verify interface defines expected methods
        var interfaceType = typeof(ICausalTrackingService);
        
        Assert.True(interfaceType.GetMethod("RecordCausalLinkAsync") != null);
        Assert.True(interfaceType.GetMethod("GetCausalChainAsync") != null);
        Assert.True(interfaceType.GetMethod("GetEventChainAsync") != null);
        Assert.True(interfaceType.GetMethod("GetRootCauseAsync") != null);
        Assert.True(interfaceType.GetMethod("GetDownstreamEventsAsync") != null);
        Assert.True(interfaceType.GetMethod("GenerateCausalNarrativeAsync") != null);
        Assert.True(interfaceType.GetMethod("LinkToParentEventAsync") != null);
    }
}

/// <summary>
/// Tests for IOfflineSimulationService interface
/// </summary>
public class IOfflineSimulationServiceTests
{
    [Fact]
    public void IOfflineSimulationService_InterfaceDefinesRequiredMethods()
    {
        // Verify interface defines expected methods
        var interfaceType = typeof(IOfflineSimulationService);
        
        Assert.True(interfaceType.GetMethod("GetOfflineDurationAsync") != null);
        Assert.True(interfaceType.GetMethod("ShouldRunOfflineSimulationAsync") != null);
        Assert.True(interfaceType.GetMethod("RunOfflineSimulationAsync") != null);
        Assert.True(interfaceType.GetMethod("GetCatchupSummaryAsync") != null);
        Assert.True(interfaceType.GetMethod("AcknowledgeCatchupAsync") != null);
        Assert.True(interfaceType.GetMethod("HasUnreadCatchupAsync") != null);
    }
}
