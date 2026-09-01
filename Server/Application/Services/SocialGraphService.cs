using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class SocialGraphService : ISocialGraphService
{
    private readonly AppDbContext _context;
    private readonly INotificationService? _notificationService;

    public SocialGraphService(AppDbContext context, INotificationService? notificationService = null)
    {
        _context = context;
        _notificationService = notificationService;
    }

    #region Follow Operations

    public async Task<Follow?> FollowAsync(int followerAccountId, int followedAccountId)
    {
        // Cannot follow yourself
        if (followerAccountId == followedAccountId)
        {
            throw new InvalidOperationException("Cannot follow yourself");
        }

        // Check if target account exists and is active
        var targetAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == followedAccountId);

        if (targetAccount == null)
        {
            throw new InvalidOperationException("Target account not found");
        }

        if (targetAccount.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException("Cannot follow an inactive account");
        }

        // Check if blocker has blocked the follower
        var blockedByTarget = await _context.Blocks
            .AnyAsync(b => b.BlockerAccountId == followedAccountId && b.BlockedAccountId == followerAccountId);

        if (blockedByTarget)
        {
            throw new InvalidOperationException("Cannot follow this account");
        }

        // Check if already following
        var existingFollow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerAccountId == followerAccountId && f.FollowedAccountId == followedAccountId);

        if (existingFollow != null)
        {
            throw new InvalidOperationException("Already following this account");
        }

        // Check if follower has blocked target (clean up block if exists)
        var existingBlock = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerAccountId == followerAccountId && b.BlockedAccountId == followedAccountId);

        if (existingBlock != null)
        {
            throw new InvalidOperationException("Cannot follow an account you have blocked");
        }

        // Create follow relationship
        var follow = new Follow
        {
            FollowerAccountId = followerAccountId,
            FollowedAccountId = followedAccountId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        // Create notification (fire-and-forget pattern for non-blocking notification creation)
        if (_notificationService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.NotifyFollowAsync(follow.Id, followerAccountId, followedAccountId);
                }
                catch
                {
                    // Notification service already logs failures internally; swallow exception
                }
            });
        }

        return follow;
    }

    public async Task<bool> UnfollowAsync(int followerAccountId, int followedAccountId)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerAccountId == followerAccountId && f.FollowedAccountId == followedAccountId);

        if (follow == null)
        {
            return false;
        }

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsFollowingAsync(int followerAccountId, int followedAccountId)
    {
        return await _context.Follows
            .AnyAsync(f => f.FollowerAccountId == followerAccountId && f.FollowedAccountId == followedAccountId);
    }

    #endregion

    #region Block Operations

    public async Task<Block?> BlockAsync(int blockerAccountId, int blockedAccountId)
    {
        // Cannot block yourself
        if (blockerAccountId == blockedAccountId)
        {
            throw new InvalidOperationException("Cannot block yourself");
        }

        // Check if target account exists
        var targetAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == blockedAccountId);

        if (targetAccount == null)
        {
            throw new InvalidOperationException("Target account not found");
        }

        // Check if already blocking
        var existingBlock = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerAccountId == blockerAccountId && b.BlockedAccountId == blockedAccountId);

        if (existingBlock != null)
        {
            throw new InvalidOperationException("Already blocking this account");
        }

        // Begin transaction to ensure consistency
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Remove any existing follow relationships (bidirectional)
            var followFromBlocker = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerAccountId == blockerAccountId && f.FollowedAccountId == blockedAccountId);

            if (followFromBlocker != null)
            {
                _context.Follows.Remove(followFromBlocker);
            }

            var followFromBlocked = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerAccountId == blockedAccountId && f.FollowedAccountId == blockerAccountId);

            if (followFromBlocked != null)
            {
                _context.Follows.Remove(followFromBlocked);
            }

            // Create block relationship
            var block = new Block
            {
                BlockerAccountId = blockerAccountId,
                BlockedAccountId = blockedAccountId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Blocks.Add(block);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return block;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UnblockAsync(int blockerAccountId, int blockedAccountId)
    {
        var block = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerAccountId == blockerAccountId && b.BlockedAccountId == blockedAccountId);

        if (block == null)
        {
            return false;
        }

        _context.Blocks.Remove(block);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsBlockingAsync(int blockerAccountId, int blockedAccountId)
    {
        return await _context.Blocks
            .AnyAsync(b => b.BlockerAccountId == blockerAccountId && b.BlockedAccountId == blockedAccountId);
    }

    #endregion

    #region Mute Operations

    public async Task<Mute?> MuteAsync(int muterAccountId, int mutedAccountId)
    {
        // Cannot mute yourself
        if (muterAccountId == mutedAccountId)
        {
            throw new InvalidOperationException("Cannot mute yourself");
        }

        // Check if target account exists
        var targetAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == mutedAccountId);

        if (targetAccount == null)
        {
            throw new InvalidOperationException("Target account not found");
        }

        // Check if already muting
        var existingMute = await _context.Mutes
            .FirstOrDefaultAsync(m => m.MuterAccountId == muterAccountId && m.MutedAccountId == mutedAccountId);

        if (existingMute != null)
        {
            throw new InvalidOperationException("Already muting this account");
        }

        // Create mute relationship
        var mute = new Mute
        {
            MuterAccountId = muterAccountId,
            MutedAccountId = mutedAccountId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Mutes.Add(mute);
        await _context.SaveChangesAsync();

        return mute;
    }

    public async Task<bool> UnmuteAsync(int muterAccountId, int mutedAccountId)
    {
        var mute = await _context.Mutes
            .FirstOrDefaultAsync(m => m.MuterAccountId == muterAccountId && m.MutedAccountId == mutedAccountId);

        if (mute == null)
        {
            return false;
        }

        _context.Mutes.Remove(mute);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsMutingAsync(int muterAccountId, int mutedAccountId)
    {
        return await _context.Mutes
            .AnyAsync(m => m.MuterAccountId == muterAccountId && m.MutedAccountId == mutedAccountId);
    }

    #endregion

    #region Relationship Queries

    public async Task<(bool IsFollowing, bool IsFollowedBy, bool IsBlocking, bool IsBlockedBy, bool IsMuting)> GetRelationshipAsync(int accountId1, int accountId2)
    {
        var isFollowing = await IsFollowingAsync(accountId1, accountId2);
        var isFollowedBy = await IsFollowingAsync(accountId2, accountId1);
        var isBlocking = await IsBlockingAsync(accountId1, accountId2);
        var isBlockedBy = await IsBlockingAsync(accountId2, accountId1);
        var isMuting = await IsMutingAsync(accountId1, accountId2);

        return (isFollowing, isFollowedBy, isBlocking, isBlockedBy, isMuting);
    }

    #endregion

    #region Followers/Following Queries

    public async Task<(IEnumerable<Follow> Items, int TotalCount)> GetFollowersAsync(int accountId, int page = 1, int pageSize = 20)
    {
        var query = _context.Follows
            .Include(f => f.FollowerAccount)
                .ThenInclude(a => a.Profile)
            .Where(f => f.FollowedAccountId == accountId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Follow> Items, int TotalCount)> GetFollowingAsync(int accountId, int page = 1, int pageSize = 20)
    {
        var query = _context.Follows
            .Include(f => f.FollowedAccount)
                .ThenInclude(a => a.Profile)
            .Where(f => f.FollowerAccountId == accountId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region Counts

    public async Task<int> GetFollowerCountAsync(int accountId)
    {
        return await _context.Follows.CountAsync(f => f.FollowedAccountId == accountId);
    }

    public async Task<int> GetFollowingCountAsync(int accountId)
    {
        return await _context.Follows.CountAsync(f => f.FollowerAccountId == accountId);
    }

    public async Task<int> GetFollowerCountAsync(Guid accountId)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
        if (account == null) return 0;
        return await GetFollowerCountAsync(account.Id);
    }

    #endregion
}
