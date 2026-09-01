using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// LLM-driven event generation service
/// </summary>
public class EventGenerationService : IEventGenerationService
{
    private readonly AppDbContext _context;
    private readonly IAiTextGenerationService _aiService;
    private readonly ILogger<EventGenerationService> _logger;

    private const string SystemPrompt = @"You are the narrative director of a social media simulation world.
Your job is to create interesting, dramatic, and organic events that emerge naturally from the social landscape.

You analyze:
1. Recent posts and their engagement
2. Relationship tensions and dynamics
3. Community moods and activities
4. Individual NPC personalities and histories
5. Current trends and topics

You decide:
1. What interesting event should happen next
2. Who should be involved
3. Why this event makes sense given the context
4. How the event should unfold narratively

RULES:
- Events must emerge from existing state, not be random
- Consider character personalities when deciding actions
- Drama is good, but meaningless cruelty is not
- Events should have consequences that ripple outward
- Give each event a compelling narrative hook
- All referenced accounts must exist and be active
- Do not create events involving blocked/muted relationships

Event Types:
- Drama: JealousyIncident, PublicArgument, Betrayal, RedemptionArc, ComebackStory, DownfallStory
- Romance: NewRelationship, Breakup, LoveTriangle, SecretRelationship, RelationshipMilestone, Reconciliation
- Social: NewFriendship, FriendshipEnded, Alliance, Rivalry, FanWar, TrollAttack
- Fame: RiseToFame, FallFromGrace, Scandal, Apology, Comeback, Cancellation
- Community: CommunityDriven, CommunitySplit, CommunityMilestone, CommunityDrama
- Content: ViralPost, ViralComment, QuotePostDrama, PollControversy
- Trend: TrendStart, TrendPivot, TrendDeath
- News: NewsCoverage, BreakingNews, NewsDebate

Respond ONLY with valid JSON in this exact format:
{
  ""eventType"": ""EventTypeName"",
  ""title"": ""A compelling dramatic title"",
  ""description"": ""What happens in this event"",
  ""narrativeContext"": ""Why this makes sense given the world state"",
  ""primaryAccountId"": null,
  ""secondaryAccountId"": null,
  ""topic"": ""relevant topic"",
  ""dramaLevel"": 5,
  ""participants"": [
    {""accountId"": 1, ""role"": ""Protagonist"", ""reasoning"": ""Why this account""}
  ],
  ""expectedConsequences"": [
    {""type"": ""RelationshipChange"", ""targetAccountId"": 2, ""relationship"": ""trust"", ""delta"": -10}
  ],
  ""followUpEventProbability"": 0.5,
  ""narrativeArcLength"": 2
}";

    public EventGenerationService(
        AppDbContext context,
        IAiTextGenerationService aiService,
        ILogger<EventGenerationService> logger)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<EventProposal?> ProposeNextEventAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Gather world state for the LLM
            var worldState = await GatherWorldStateAsync(cancellationToken);
            
            var prompt = $@"Based on the current world state, propose ONE compelling event:

{worldState}

Generate a single event that would be interesting, dramatic, and natural given this context.";

            var request = new AiGenerationRequest
            {
                SystemPrompt = SystemPrompt,
                UserPrompt = prompt,
                MaxTokens = 800,
                Temperature = 0.8,
                RequestId = $"event-proposal-{Guid.NewGuid()}"
            };

            var result = await _aiService.GenerateAsync(request, cancellationToken);
            
            if (!result.Success || string.IsNullOrWhiteSpace(result.Text))
            {
                _logger.LogWarning("Event generation failed: {Error}", result.ErrorMessage);
                return null;
            }

            var proposal = ParseEventProposal(result.Text);
            return proposal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proposing next event");
            return null;
        }
    }

    public async Task<EventProposal?> ProposeEventForAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _context.Accounts
                .Include(a => a.Profile)
                .Include(a => a.NpcProfile)
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for event proposal", accountId);
                return null;
            }

            var accountContext = await GatherAccountContextAsync(accountId, cancellationToken);
            
            var prompt = $@"Propose ONE compelling event centered on account {account.Username}:

{accountContext}

Generate an event where this account plays a central role.";

            var request = new AiGenerationRequest
            {
                SystemPrompt = SystemPrompt,
                UserPrompt = prompt,
                MaxTokens = 800,
                Temperature = 0.8,
                RequestId = $"event-proposal-{Guid.NewGuid()}"
            };

            var result = await _aiService.GenerateAsync(request, cancellationToken);
            
            if (!result.Success || string.IsNullOrWhiteSpace(result.Text))
            {
                _logger.LogWarning("Event generation for account {AccountId} failed: {Error}", accountId, result.ErrorMessage);
                return null;
            }

            var proposal = ParseEventProposal(result.Text);
            if (proposal != null)
            {
                proposal.PrimaryAccountId ??= accountId;
            }
            return proposal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proposing event for account {AccountId}", accountId);
            return null;
        }
    }

    public async Task<EventProposal?> ProposeCommunityEventAsync(int communityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var community = await _context.Communities
                .Include(c => c.Posts)
                    .ThenInclude(p => p.AuthorAccount)
                .FirstOrDefaultAsync(c => c.Id == communityId, cancellationToken);

            if (community == null)
            {
                _logger.LogWarning("Community {CommunityId} not found for event proposal", communityId);
                return null;
            }

            var communityContext = await GatherCommunityContextAsync(communityId, cancellationToken);
            
            var prompt = $@"Propose ONE compelling event for community ""{community.Name}"":

{communityContext}

Generate a community-focused event.";

            var request = new AiGenerationRequest
            {
                SystemPrompt = SystemPrompt,
                UserPrompt = prompt,
                MaxTokens = 800,
                Temperature = 0.8,
                RequestId = $"event-proposal-{Guid.NewGuid()}"
            };

            var result = await _aiService.GenerateAsync(request, cancellationToken);
            
            if (!result.Success || string.IsNullOrWhiteSpace(result.Text))
            {
                _logger.LogWarning("Event generation for community {CommunityId} failed: {Error}", communityId, result.ErrorMessage);
                return null;
            }

            var proposal = ParseEventProposal(result.Text);
            return proposal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proposing event for community {CommunityId}", communityId);
            return null;
        }
    }

    public async Task<ValidationResult> ValidateProposalAsync(EventProposal proposal, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(proposal.Title))
        {
            result.AddError("Event title is required");
        }

        if (string.IsNullOrWhiteSpace(proposal.Description))
        {
            result.AddError("Event description is required");
        }

        if (!Enum.IsDefined(typeof(EventType), proposal.EventType))
        {
            result.AddError($"Invalid event type: {proposal.EventType}");
        }

        // Validate all referenced accounts exist
        var accountIds = proposal.Participants.Select(p => p.AccountId).ToList();
        if (proposal.PrimaryAccountId.HasValue && !accountIds.Contains(proposal.PrimaryAccountId.Value))
        {
            accountIds.Add(proposal.PrimaryAccountId.Value);
        }
        if (proposal.SecondaryAccountId.HasValue && !accountIds.Contains(proposal.SecondaryAccountId.Value))
        {
            accountIds.Add(proposal.SecondaryAccountId.Value);
        }

        if (accountIds.Any())
        {
            var existingAccounts = await _context.Accounts
                .Where(a => accountIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            var missingAccounts = accountIds.Except(existingAccounts).ToList();
            if (missingAccounts.Any())
            {
                result.AddError($"Accounts not found: {string.Join(", ", missingAccounts)}");
            }

            // Check for blocked relationships between participants
            foreach (var participant in proposal.Participants)
            {
                var blockedBy = await _context.Blocks
                    .AnyAsync(b => b.BlockedAccountId == participant.AccountId && accountIds.Contains(b.BlockerAccountId), cancellationToken);
                
                if (blockedBy)
                {
                    result.AddWarning($"Participant {participant.AccountId} may have blocked relationships with other participants");
                }
            }
        }

        // Validate drama level
        if (proposal.DramaLevel < 1 || proposal.DramaLevel > 10)
        {
            result.AddWarning($"Drama level {proposal.DramaLevel} is outside normal range 1-10");
        }

        // Check for duplicate recent events
        var recentEvent = await _context.Events
            .Where(e => !e.IsDeleted && e.CreatedAt > DateTime.UtcNow.AddHours(-6))
            .Where(e => e.Title.ToLower() == proposal.Title.ToLower() || 
                        (proposal.PrimaryAccountId.HasValue && e.Participations.Any(p => p.AccountId == proposal.PrimaryAccountId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (recentEvent != null)
        {
            result.AddWarning($"Similar event occurred recently: {recentEvent.Title}");
        }

        // Validate consequence targets
        var allValidatedAccounts = accountIds.ToHashSet();
        foreach (var consequence in proposal.ExpectedConsequences)
        {
            if (consequence.Parameters.TryGetValue("targetAccountId", out var targetId) && targetId is int targetAccountId)
            {
                if (!allValidatedAccounts.Contains(targetAccountId))
                {
                    result.AddError($"Consequence targets non-existent account: {targetAccountId}");
                }
            }
        }

        return result;
    }

    public async Task<Event> ExecuteEventAsync(EventProposal proposal, CancellationToken cancellationToken = default)
    {
        // First validate
        var validation = await ValidateProposalAsync(proposal, cancellationToken);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Event proposal validation failed: {string.Join(", ", validation.Errors)}");
        }

        // Create the event
        var evt = new Event
        {
            Type = proposal.EventType,
            Title = proposal.Title,
            Description = proposal.Description,
            NarrativeContext = proposal.NarrativeContext,
            CreatorAccountId = proposal.PrimaryAccountId,
            Status = EventStatus.Active,
            Topic = proposal.Topic,
            DramaLevel = proposal.DramaLevel,
            FollowUpProbability = proposal.FollowUpProbability,
            NarrativeArcLength = proposal.NarrativeArcLength,
            Metadata = JsonSerializer.Serialize(new
            {
                participants = proposal.Participants,
                consequences = proposal.ExpectedConsequences
            })
        };

        _context.Events.Add(evt);
        await _context.SaveChangesAsync(cancellationToken);

        // Create participation records
        foreach (var participant in proposal.Participants)
        {
            var participation = new EventParticipation
            {
                EventId = evt.Id,
                AccountId = participant.AccountId,
                Role = participant.Role,
                LLMReasoning = participant.Reasoning
            };
            _context.EventParticipations.Add(participation);
        }

        evt.ParticipantCount = proposal.Participants.Count;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Event {EventId} '{Title}' created with {ParticipantCount} participants",
            evt.EventId, evt.Title, evt.ParticipantCount);

        return evt;
    }

    private async Task<string> GatherWorldStateAsync(CancellationToken cancellationToken)
    {
        var recentPosts = await _context.Posts
            .Include(p => p.AuthorAccount)
            .Where(p => p.CreatedAt > DateTime.UtcNow.AddHours(24))
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(p => new
            {
                p.Content,
                Author = p.AuthorAccount != null ? p.AuthorAccount.Username : "Unknown",
                p.CreatedAt,
                Likes = p.Likes.Count,
                Comments = p.Comments.Count
            })
            .ToListAsync(cancellationToken);

        var recentEvents = await _context.Events
            .Where(e => !e.IsDeleted && e.CreatedAt > DateTime.UtcNow.AddDays(7))
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .Select(e => new
            {
                e.Title,
                e.Type,
                e.Status,
                Participants = e.Participations.Count
            })
            .ToListAsync(cancellationToken);

        var topAccounts = await _context.Accounts
            .Where(a => a.Status == Domain.Entities.AccountStatus.Active)
            .OrderByDescending(a => a.AccountType)
            .Take(5)
            .Select(a => a.Username)
            .ToListAsync(cancellationToken);

        return $@"Current World State:
Recent Posts ({recentPosts.Count}):
{string.Join("\n", recentPosts.Select(p => $"- {p.Author}: {p.Content.Substring(0, Math.Min(100, p.Content.Length))}... ({p.Likes} likes, {p.Comments} comments)"))}

Recent Events ({recentEvents.Count}):
{string.Join("\n", recentEvents.Select(e => $"- {e.Title} ({e.Type}, {e.Status})"))}

Active Accounts: {string.Join(", ", topAccounts)}";
    }

    private async Task<string> GatherAccountContextAsync(int accountId, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Include(a => a.Profile)
            .Include(a => a.NpcProfile)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null) return "Account not found";

        var followers = await _context.Follows.CountAsync(f => f.FollowedAccountId == accountId, cancellationToken);
        var following = await _context.Follows.CountAsync(f => f.FollowerAccountId == accountId, cancellationToken);

        var recentPosts = await _context.Posts
            .Where(p => p.AuthorAccountId == accountId && p.Status == Domain.Entities.PostStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .Select(p => p.Content)
            .ToListAsync(cancellationToken);

        var personality = account.NpcProfile?.Personality != null 
            ? $"Personality: {account.NpcProfile.Personality.Openness}, {account.NpcProfile.Personality.Conscientiousness}"
            : "Human account";

        return $@"Account: {account.Username}
Followers: {followers}, Following: {following}
{personality}
Recent Posts: {string.Join(" | ", recentPosts)}";
    }

    private async Task<string> GatherCommunityContextAsync(int communityId, CancellationToken cancellationToken)
    {
        var community = await _context.Communities
            .Include(c => c.Posts)
                .ThenInclude(p => p.AuthorAccount)
            .Include(c => c.Memberships)
            .FirstOrDefaultAsync(c => c.Id == communityId, cancellationToken);

        if (community == null) return "Community not found";

        var recentPosts = community.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => $"{p.AuthorAccount?.Username}: {p.Content.Substring(0, Math.Min(80, p.Content.Length))}")
            .ToList();

        return $@"Community: {community.Name}
Topic: {community.Topic}
Members: {community.MemberCount}
Recent Activity: {string.Join(" | ", recentPosts)}";
    }

    private EventProposal? ParseEventProposal(string jsonText)
    {
        try
        {
            // Clean up the JSON - remove markdown code blocks if present
            jsonText = jsonText.Trim();
            if (jsonText.StartsWith("```json"))
            {
                jsonText = jsonText.Substring(7);
            }
            else if (jsonText.StartsWith("```"))
            {
                jsonText = jsonText.Substring(3);
            }
            if (jsonText.EndsWith("```"))
            {
                jsonText = jsonText.Substring(0, jsonText.Length - 3);
            }
            jsonText = jsonText.Trim();

            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            var proposal = new EventProposal
            {
                Title = GetStringProperty(root, "title"),
                Description = GetStringProperty(root, "description"),
                NarrativeContext = GetStringProperty(root, "narrativeContext"),
                Topic = GetStringProperty(root, "topic"),
                DramaLevel = GetIntProperty(root, "dramaLevel", 5),
                FollowUpProbability = GetDoubleProperty(root, "followUpEventProbability", 0.5),
                NarrativeArcLength = GetIntProperty(root, "narrativeArcLength", 1)
            };

            // Parse event type
            var eventTypeStr = GetStringProperty(root, "eventType");
            if (!string.IsNullOrEmpty(eventTypeStr) && Enum.TryParse<EventType>(eventTypeStr, true, out var eventType))
            {
                proposal.EventType = eventType;
            }

            // Parse primary/secondary accounts
            proposal.PrimaryAccountId = GetNullableIntProperty(root, "primaryAccountId");
            proposal.SecondaryAccountId = GetNullableIntProperty(root, "secondaryAccountId");

            // Parse participants
            if (root.TryGetProperty("participants", out var participants) && participants.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in participants.EnumerateArray())
                {
                    var accountId = GetIntProperty(p, "accountId", 0);
                    if (accountId > 0)
                    {
                        var roleStr = GetStringProperty(p, "role");
                        Enum.TryParse<ParticipantRole>(roleStr, true, out var role);

                        proposal.Participants.Add(new EventParticipantProposal
                        {
                            AccountId = accountId,
                            Role = role,
                            Reasoning = GetStringProperty(p, "reasoning")
                        });
                    }
                }
            }

            // Parse consequences
            if (root.TryGetProperty("expectedConsequences", out var consequences) && consequences.ValueKind == JsonValueKind.Array)
            {
                foreach (var c in consequences.EnumerateArray())
                {
                    var typeStr = GetStringProperty(c, "type");
                    Enum.TryParse<ConsequenceType>(typeStr, true, out var type);

                    var params2 = new Dictionary<string, object>();
                    foreach (var prop in c.EnumerateObject())
                    {
                        params2[prop.Name] = prop.Value.ValueKind switch
                        {
                            JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? i : prop.Value.GetDouble(),
                            JsonValueKind.String => prop.Value.GetString() ?? "",
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            _ => prop.Value.ToString()
                        };
                    }

                    proposal.ExpectedConsequences.Add(new EventConsequenceProposal
                    {
                        Type = type,
                        Parameters = params2
                    });
                }
            }

            return proposal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse event proposal JSON");
            return null;
        }
    }

    private static string GetStringProperty(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? "";
        }
        return "";
    }

    private static int GetIntProperty(JsonElement element, string property, int defaultValue)
    {
        if (element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            return prop.GetInt32();
        }
        return defaultValue;
    }

    private static int? GetNullableIntProperty(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
            {
                return prop.GetInt32();
            }
        }
        return null;
    }

    private static double GetDoubleProperty(JsonElement element, string property, double defaultValue)
    {
        if (element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            return prop.GetDouble();
        }
        return defaultValue;
    }
}
