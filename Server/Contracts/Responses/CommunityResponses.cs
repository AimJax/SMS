using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Contracts.Responses;

public class CommunitySummaryResponse
{
    public Guid CommunityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public CommunityOwnerInfo? Owner { get; set; }
}

public class CommunityOwnerInfo
{
    public int AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class CommunityDetailResponse : CommunitySummaryResponse
{
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public MemberRoleResponse? CurrentUserRole { get; set; }
}

public class MemberRoleResponse
{
    public string Role { get; set; } = string.Empty;
    public DateTime? JoinedAt { get; set; }
}

public class CommunityMemberResponse
{
    public Guid MembershipId { get; set; }
    public int AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class PaginatedCommunitiesResponse
{
    public IEnumerable<CommunitySummaryResponse> Communities { get; set; } = new List<CommunitySummaryResponse>();
    public string? NextCursor { get; set; }
}

public class PaginatedMembersResponse
{
    public IEnumerable<CommunityMemberResponse> Members { get; set; } = new List<CommunityMemberResponse>();
    public string? NextCursor { get; set; }
}

public class MembershipActionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public CommunityMemberResponse? Membership { get; set; }
}

public class AccountCommunitiesResponse
{
    public IEnumerable<CommunitySummaryResponse> Communities { get; set; } = new List<CommunitySummaryResponse>();
}
