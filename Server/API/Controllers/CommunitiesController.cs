using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaSimulator.Server.Application.Services;
using SocialMediaSimulator.Server.Contracts.Responses;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.API.Controllers;

public static class CommunitiesController
{
    public static void MapCommunityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/communities").WithTags("Communities");

        group.MapGet("" , GetCommunities)
            .WithName("GetCommunities")
            .WithSummary("Browse public communities");

        group.MapGet("/search", SearchCommunities)
            .WithName("SearchCommunities")
            .WithSummary("Search communities by name, description, or tags");

        group.MapGet("/by-topic/{topic}", GetByTopic)
            .WithName("GetCommunitiesByTopic")
            .WithSummary("Get communities by topic");

        group.MapGet("/{slug}", GetCommunityBySlug)
            .WithName("GetCommunity")
            .WithSummary("Get community details");

        group.MapGet("/{slug}/feed", GetCommunityFeed)
            .WithName("GetCommunityFeed")
            .WithSummary("Get posts in a community")
            .RequireAuthorization();

        group.MapGet("/{slug}/members", GetCommunityMembers)
            .WithName("GetCommunityMembers")
            .WithSummary("Get community members");

        group.MapPost("/{slug}/join", JoinCommunity)
            .WithName("JoinCommunity")
            .WithSummary("Join a community")
            .RequireAuthorization();

        group.MapPost("/{slug}/leave", LeaveCommunity)
            .WithName("LeaveCommunity")
            .WithSummary("Leave a community")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetCommunities(
        [FromServices] ICommunityService communityService,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null)
    {
        var (communities, nextCursor) = await communityService.GetPublicCommunitiesAsync(cursor, pageSize, sortBy);
        
        var response = new PaginatedCommunitiesResponse
        {
            Communities = communities.Select(MapToSummary),
            NextCursor = nextCursor
        };
        
        return Results.Ok(response);
    }

    private static async Task<IResult> SearchCommunities(
        [FromServices] ICommunityService communityService,
        [FromQuery] string? q = null,
        [FromQuery] string? topic = null,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20)
    {
        var (communities, nextCursor) = await communityService.SearchCommunitiesAsync(q, topic, cursor, pageSize);
        
        var response = new PaginatedCommunitiesResponse
        {
            Communities = communities.Select(MapToSummary),
            NextCursor = nextCursor
        };
        
        return Results.Ok(response);
    }

    private static async Task<IResult> GetByTopic(
        [FromServices] ICommunityService communityService,
        [FromRoute] string topic,
        [FromQuery] int limit = 20)
    {
        var communities = await communityService.GetByTopicAsync(topic, limit);
        
        var response = new PaginatedCommunitiesResponse
        {
            Communities = communities.Select(MapToSummary)
        };
        
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCommunityBySlug(
        [FromServices] ICommunityService communityService,
        [FromRoute] string slug,
        HttpContext httpContext)
    {
        var community = await communityService.GetBySlugAsync(slug);
        
        if (community == null)
        {
            return Results.NotFound(new { message = "Community not found" });
        }

        if (community.Visibility != CommunityVisibility.Public)
        {
            var accountId = GetAccountId(httpContext);
            if (!accountId.HasValue)
            {
                return Results.Forbid();
            }
            
            var role = await communityService.GetMemberRoleAsync(accountId.Value, community.Id);
            if (role == null)
            {
                return Results.Forbid();
            }
        }

        var response = MapToDetail(community);
        
        var currentAccountId = GetAccountId(httpContext);
        if (currentAccountId.HasValue)
        {
            var currentRole = await communityService.GetMemberRoleAsync(currentAccountId.Value, community.Id);
            if (currentRole.HasValue)
            {
                response.CurrentUserRole = new MemberRoleResponse
                {
                    Role = currentRole.Value.ToString(),
                    JoinedAt = DateTime.UtcNow
                };
            }
        }
        
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCommunityFeed(
        [FromServices] ICommunityService communityService,
        [FromRoute] string slug,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20,
        HttpContext httpContext = null!)
    {
        var accountId = GetAccountId(httpContext);
        if (!accountId.HasValue)
        {
            return Results.Unauthorized();
        }

        var community = await communityService.GetBySlugAsync(slug);
        if (community == null)
        {
            return Results.NotFound(new { message = "Community not found" });
        }

        if (community.Visibility != CommunityVisibility.Public)
        {
            var isMember = await communityService.IsMemberAsync(accountId.Value, community.Id);
            if (!isMember)
            {
                return Results.Forbid();
            }
        }

        var (posts, nextCursor) = await communityService.GetCommunityFeedAsync(community.Id, cursor, pageSize);
        
        var response = new
        {
            Community = MapToSummary(community),
            Posts = posts.Select(p => new
            {
                p.PostId,
                p.AuthorAccountId,
                AuthorUsername = p.AuthorAccount?.Username,
                AuthorDisplayName = p.AuthorAccount?.Profile?.DisplayName,
                AuthorAvatarUrl = p.AuthorAccount?.Profile?.AvatarUrl,
                p.Content,
                p.CreatedAt,
                LikeCount = p.Likes?.Count ?? 0,
                CommentCount = p.Comments?.Count ?? 0,
                CommunityId = p.CommunityId
            }),
            NextCursor = nextCursor
        };
        
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCommunityMembers(
        [FromServices] ICommunityService communityService,
        [FromRoute] string slug,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 20)
    {
        var community = await communityService.GetBySlugAsync(slug);
        if (community == null)
        {
            return Results.NotFound(new { message = "Community not found" });
        }

        var (members, nextCursor) = await communityService.GetMembersAsync(community.Id, cursor, pageSize);
        
        var response = new PaginatedMembersResponse
        {
            Members = members.Select(m => new CommunityMemberResponse
            {
                MembershipId = m.MembershipId,
                AccountId = m.AccountId,
                Username = m.Account?.Username ?? string.Empty,
                DisplayName = m.Account?.Profile?.DisplayName ?? string.Empty,
                AvatarUrl = m.Account?.Profile?.AvatarUrl,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            }),
            NextCursor = nextCursor
        };
        
        return Results.Ok(response);
    }

    private static async Task<IResult> JoinCommunity(
        [FromServices] ICommunityService communityService,
        [FromRoute] string slug,
        HttpContext httpContext)
    {
        var accountId = GetAccountId(httpContext);
        if (!accountId.HasValue)
        {
            return Results.Unauthorized();
        }

        var membership = await communityService.JoinCommunityAsync(accountId.Value, slug);
        
        if (membership == null)
        {
            return Results.BadRequest(new { message = "Failed to join community." });
        }
        
        return Results.Ok(new MembershipActionResponse
        {
            Success = true,
            Message = "Successfully joined community",
            Membership = new CommunityMemberResponse
            {
                MembershipId = membership.MembershipId,
                AccountId = membership.AccountId,
                Role = membership.Role.ToString(),
                JoinedAt = membership.JoinedAt
            }
        });
    }

    private static async Task<IResult> LeaveCommunity(
        [FromServices] ICommunityService communityService,
        [FromRoute] string slug,
        HttpContext httpContext)
    {
        var accountId = GetAccountId(httpContext);
        if (!accountId.HasValue)
        {
            return Results.Unauthorized();
        }

        var success = await communityService.LeaveCommunityAsync(accountId.Value, slug);
        
        if (!success)
        {
            return Results.BadRequest(new { message = "Failed to leave community." });
        }
        
        return Results.Ok(new MembershipActionResponse
        {
            Success = true,
            Message = "Successfully left community"
        });
    }

    private static int? GetAccountId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("accountId")?.Value;
        
        if (int.TryParse(userIdClaim, out var accountId))
        {
            return accountId;
        }
        
        return null;
    }

    private static CommunitySummaryResponse MapToSummary(Community community)
    {
        return new CommunitySummaryResponse
        {
            CommunityId = community.CommunityId,
            Name = community.Name,
            Slug = community.Slug,
            Description = community.Description,
            Topic = community.Topic,
            Tags = community.Tags,
            MemberCount = community.MemberCount,
            PostCount = community.PostCount,
            Visibility = community.Visibility.ToString(),
            CreatedAt = community.CreatedAt,
            Owner = community.OwnerAccount != null ? new CommunityOwnerInfo
            {
                AccountId = community.OwnerAccount.Id,
                Username = community.OwnerAccount.Username,
                DisplayName = community.OwnerAccount.Profile?.DisplayName ?? string.Empty,
                AvatarUrl = community.OwnerAccount.Profile?.AvatarUrl
            } : null
        };
    }

    private static CommunityDetailResponse MapToDetail(Community community)
    {
        return new CommunityDetailResponse
        {
            CommunityId = community.CommunityId,
            Name = community.Name,
            Slug = community.Slug,
            Description = community.Description,
            Topic = community.Topic,
            Tags = community.Tags,
            MemberCount = community.MemberCount,
            PostCount = community.PostCount,
            Visibility = community.Visibility.ToString(),
            CreatedAt = community.CreatedAt,
            IsActive = community.IsActive,
            UpdatedAt = community.UpdatedAt,
            Owner = community.OwnerAccount != null ? new CommunityOwnerInfo
            {
                AccountId = community.OwnerAccount.Id,
                Username = community.OwnerAccount.Username,
                DisplayName = community.OwnerAccount.Profile?.DisplayName ?? string.Empty,
                AvatarUrl = community.OwnerAccount.Profile?.AvatarUrl
            } : null
        };
    }
}
