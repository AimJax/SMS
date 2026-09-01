using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

public interface ISocialGraphService
{
    // Follow operations
    Task<Follow?> FollowAsync(int followerAccountId, int followedAccountId);
    Task<bool> UnfollowAsync(int followerAccountId, int followedAccountId);
    Task<bool> IsFollowingAsync(int followerAccountId, int followedAccountId);
    
    // Block operations
    Task<Block?> BlockAsync(int blockerAccountId, int blockedAccountId);
    Task<bool> UnblockAsync(int blockerAccountId, int blockedAccountId);
    Task<bool> IsBlockingAsync(int blockerAccountId, int blockedAccountId);
    
    // Mute operations
    Task<Mute?> MuteAsync(int muterAccountId, int mutedAccountId);
    Task<bool> UnmuteAsync(int muterAccountId, int mutedAccountId);
    Task<bool> IsMutingAsync(int muterAccountId, int mutedAccountId);
    
    // Relationship queries
    Task<(bool IsFollowing, bool IsFollowedBy, bool IsBlocking, bool IsBlockedBy, bool IsMuting)> GetRelationshipAsync(int accountId1, int accountId2);
    
    // Followers/Following queries with pagination
    Task<(IEnumerable<Follow> Items, int TotalCount)> GetFollowersAsync(int accountId, int page = 1, int pageSize = 20);
    Task<(IEnumerable<Follow> Items, int TotalCount)> GetFollowingAsync(int accountId, int page = 1, int pageSize = 20);
    
    // Counts
    Task<int> GetFollowerCountAsync(int accountId);
    Task<int> GetFollowingCountAsync(int accountId);
    Task<int> GetFollowerCountAsync(Guid accountId);
}
