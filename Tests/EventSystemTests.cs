using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;
using Xunit;

namespace SocialMediaSimulator.Tests;

public class EventServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _eventService = new EventService(_context, Mock.Of<ILogger<EventService>>());
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsEvents()
    {
        // Arrange
        var evt = new Event
        {
            Title = "Test Event",
            Description = "Test Description",
            NarrativeContext = "Test Context",
            Type = EventType.Drama,
            Status = EventStatus.Active
        };
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        // Act
        var (events, cursor) = await _eventService.GetEventsAsync();

        // Assert
        Assert.Single(events);
        Assert.Equal("Test Event", events.First().Title);
    }

    [Fact]
    public async Task GetEventsAsync_FiltersbyType()
    {
        // Arrange
        _context.Events.AddRange(
            new Event { Title = "Drama Event", Type = EventType.Drama, Description = "D", NarrativeContext = "C" },
            new Event { Title = "Romance Event", Type = EventType.Romance, Description = "D", NarrativeContext = "C" }
        );
        await _context.SaveChangesAsync();

        // Act
        var (events, _) = await _eventService.GetEventsAsync(type: EventType.Drama);

        // Assert
        Assert.Single(events);
        Assert.Equal("Drama Event", events.First().Title);
    }

    [Fact]
    public async Task GetEventsAsync_FiltersByStatus()
    {
        // Arrange
        _context.Events.AddRange(
            new Event { Title = "Active Event", Status = EventStatus.Active, Description = "D", NarrativeContext = "C" },
            new Event { Title = "Ended Event", Status = EventStatus.Ended, Description = "D", NarrativeContext = "C" }
        );
        await _context.SaveChangesAsync();

        // Act
        var (events, _) = await _eventService.GetEventsAsync(status: EventStatus.Active);

        // Assert
        Assert.Single(events);
        Assert.Equal("Active Event", events.First().Title);
    }

    [Fact]
    public async Task GetEventsAsync_Pagination_ReturnsCursor()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            _context.Events.Add(new Event
            {
                Title = $"Event {i}",
                Description = "D",
                NarrativeContext = "C",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (events, cursor) = await _eventService.GetEventsAsync(pageSize: 10);

        // Assert
        Assert.Equal(10, events.Count());
        Assert.NotNull(cursor);
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsEvent()
    {
        // Arrange
        var evt = new Event
        {
            EventId = Guid.NewGuid(),
            Title = "Specific Event",
            Description = "D",
            NarrativeContext = "C"
        };
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _eventService.GetEventByIdAsync(evt.EventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Specific Event", result.Title);
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsNullForNonExistent()
    {
        // Act
        var result = await _eventService.GetEventByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEventsForAccountAsync_ReturnsAccountEvents()
    {
        // Arrange
        var accountId = 1;
        var evt = new Event
        {
            Title = "Account Event",
            Description = "D",
            NarrativeContext = "C"
        };
        _context.Events.Add(evt);
        
        var participation = new EventParticipation
        {
            EventId = evt.Id,
            AccountId = accountId,
            Role = ParticipantRole.Protagonist
        };
        _context.EventParticipations.Add(participation);
        await _context.SaveChangesAsync();

        // Act
        var events = await _eventService.GetEventsForAccountAsync(accountId);

        // Assert
        Assert.Single(events);
        Assert.Equal("Account Event", events.First().Title);
    }

    [Fact]
    public async Task GetActiveEventsAsync_ReturnsActiveEvents()
    {
        // Arrange
        _context.Events.AddRange(
            new Event { Title = "Active", Status = EventStatus.Active, Description = "D", NarrativeContext = "C" },
            new Event { Title = "Ended", Status = EventStatus.Ended, Description = "D", NarrativeContext = "C" }
        );
        await _context.SaveChangesAsync();

        // Act
        var events = await _eventService.GetActiveEventsAsync();

        // Assert
        Assert.Single(events);
        Assert.Equal("Active", events.First().Title);
    }

    [Fact]
    public async Task GetEventParticipantsAsync_ReturnsParticipants()
    {
        // Arrange
        var account = new Account
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash"
        };
        _context.Accounts.Add(account);
        
        var evt = new Event
        {
            Title = "Event with Participants",
            Description = "D",
            NarrativeContext = "C"
        };
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();
        
        _context.EventParticipations.Add(new EventParticipation
        {
            EventId = evt.Id,
            AccountId = account.Id,
            Role = ParticipantRole.Protagonist,
            LLMReasoning = "Main character"
        });
        await _context.SaveChangesAsync();

        // Act
        var participants = await _eventService.GetEventParticipantsAsync(evt.Id);

        // Assert
        Assert.Single(participants);
        Assert.Equal(ParticipantRole.Protagonist, participants.First().Role);
    }
}

public class EventEntityTests
{
    [Fact]
    public void Event_GetCategory_ReturnsCorrectCategory()
    {
        // Arrange
        var dramaEvent = new Event { Type = EventType.JealousyIncident };
        var romanceEvent = new Event { Type = EventType.NewRelationship };
        var fameEvent = new Event { Type = EventType.RiseToFame };

        // Assert
        Assert.Equal(EventType.Drama, dramaEvent.GetCategory());
        Assert.Equal(EventType.Romance, romanceEvent.GetCategory());
        Assert.Equal(EventType.Fame, fameEvent.GetCategory());
    }

    [Fact]
    public void Event_DefaultValues_AreCorrect()
    {
        // Arrange
        var evt = new Event();

        // Assert
        Assert.Equal(EventStatus.Proposed, evt.Status);
        Assert.Equal(EventVisibility.Public, evt.Visibility);
        Assert.Equal(5, evt.DramaLevel);
        Assert.Equal(0.5, evt.FollowUpProbability);
        Assert.Equal(1, evt.NarrativeArcLength);
        Assert.False(evt.IsDeleted);
    }

    [Fact]
    public void EventParticipation_DefaultValues_AreCorrect()
    {
        // Arrange
        var participation = new EventParticipation();

        // Assert
        Assert.Equal(ParticipationStatus.Active, participation.Status);
        Assert.Equal(0, participation.ContributionScore);
    }
}

public class ValidationResultTests
{
    [Fact]
    public void ValidationResult_Success_ReturnsValid()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_ReturnsInvalid()
    {
        // Act
        var result = ValidationResult.Failure("Error 1", "Error 2");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void ValidationResult_AddError_AddsToErrors()
    {
        // Arrange
        var result = new ValidationResult();

        // Act
        result.AddError("Test Error");
        result.AddWarning("Test Warning");

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Single(result.Warnings);
    }
}

public class EventProposalTests
{
    [Fact]
    public void EventProposal_DefaultValues_AreCorrect()
    {
        // Arrange
        var proposal = new EventProposal();

        // Assert
        Assert.Equal(5, proposal.DramaLevel);
        Assert.Equal(0.5, proposal.FollowUpProbability);
        Assert.Equal(1, proposal.NarrativeArcLength);
        Assert.Empty(proposal.Participants);
        Assert.Empty(proposal.ExpectedConsequences);
    }

    [Fact]
    public void EventParticipantProposal_CanBeCreated()
    {
        // Arrange
        var participant = new EventParticipantProposal
        {
            AccountId = 1,
            Role = ParticipantRole.Protagonist,
            Reasoning = "Main character"
        };

        // Assert
        Assert.Equal(1, participant.AccountId);
        Assert.Equal(ParticipantRole.Protagonist, participant.Role);
    }

    [Fact]
    public void EventConsequenceProposal_CanBeCreated()
    {
        // Arrange
        var consequence = new EventConsequenceProposal
        {
            Type = ConsequenceType.RelationshipChange,
            Parameters = new Dictionary<string, object>
            {
                { "trust", 10 },
                { "targetAccountId", 2 }
            }
        };

        // Assert
        Assert.Equal(ConsequenceType.RelationshipChange, consequence.Type);
        Assert.Equal(2, consequence.Parameters["targetAccountId"]);
    }
}

public class EventTypeTests
{
    [Fact]
    public void EventType_Categories_AreCorrectlySpaced()
    {
        // Verify event type enum values are in correct ranges
        Assert.True((int)EventType.Drama >= 1000 && (int)EventType.Drama < 2000);
        Assert.True((int)EventType.Romance >= 2000 && (int)EventType.Romance < 3000);
        Assert.True((int)EventType.Social >= 3000 && (int)EventType.Social < 4000);
        Assert.True((int)EventType.Fame >= 4000 && (int)EventType.Fame < 5000);
        Assert.True((int)EventType.Community >= 5000 && (int)EventType.Community < 6000);
        Assert.True((int)EventType.Content >= 6000 && (int)EventType.Content < 7000);
        Assert.True((int)EventType.Trend >= 7000 && (int)EventType.Trend < 8000);
        Assert.True((int)EventType.News >= 8000);
    }

    [Theory]
    [InlineData(EventType.JealousyIncident)]
    [InlineData(EventType.PublicArgument)]
    [InlineData(EventType.NewRelationship)]
    [InlineData(EventType.Breakup)]
    [InlineData(EventType.FanWar)]
    [InlineData(EventType.RiseToFame)]
    [InlineData(EventType.ViralPost)]
    public void EventType_AllTypesCanBeCreated(EventType type)
    {
        // Arrange
        var evt = new Event { Type = type, Title = "T", Description = "D", NarrativeContext = "C" };

        // Assert
        Assert.Equal(type, evt.Type);
    }
}
