using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for community management
/// </summary>
public interface ICommunityService
{
    // Community CRUD
    Task<Community?> GetBySlugAsync(string slug);
    Task<Community?> GetByIdAsync(Guid communityId);
    Task<(IEnumerable<Community> Items, string? NextCursor)> GetPublicCommunitiesAsync(string? cursor = null, int pageSize = 20, string? sortBy = null);
    Task<(IEnumerable<Community> Items, string? NextCursor)> SearchCommunitiesAsync(string? query, string? topic, string? cursor = null, int pageSize = 20);
    Task<IEnumerable<Community>> GetByTopicAsync(string topic, int limit = 20);
    
    // Community membership
    Task<CommunityMembership?> JoinCommunityAsync(int accountId, string slug);
    Task<bool> LeaveCommunityAsync(int accountId, string slug);
    Task<CommunityMembership?> GetMembershipAsync(int accountId, int communityId);
    Task<(IEnumerable<CommunityMembership> Items, string? NextCursor)> GetMembersAsync(int communityId, string? cursor = null, int pageSize = 20);
    Task<IEnumerable<Community>> GetAccountCommunitiesAsync(int accountId);
    Task<CommunityRole?> GetMemberRoleAsync(int accountId, int communityId);
    
    // Community feed
    Task<(IEnumerable<Post> Items, string? NextCursor)> GetCommunityFeedAsync(int communityId, string? cursor = null, int pageSize = 20);
    
    // Community creation (for seeding/admin)
    Task<Community> CreateCommunityAsync(string name, string topic, int ownerAccountId, string? description = null, string? tags = null, CommunityVisibility visibility = CommunityVisibility.Public);
    
    // Membership queries for NPC behavior
    Task<IEnumerable<Community>> GetRelevantCommunitiesForNpcAsync(IEnumerable<string> interests, int limit = 10);
    Task<bool> IsMemberAsync(int accountId, int communityId);
}
