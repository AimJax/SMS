using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Implementation of community management service
/// </summary>
public class CommunityService : ICommunityService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CommunityService> _logger;
    
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public CommunityService(AppDbContext context, ILogger<CommunityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Community?> GetBySlugAsync(string slug)
    {
        return await _context.Communities
            .Include(c => c.OwnerAccount)
                .ThenInclude(a => a!.Profile)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
    }

    public async Task<Community?> GetByIdAsync(Guid communityId)
    {
        return await _context.Communities
            .Include(c => c.OwnerAccount)
                .ThenInclude(a => a!.Profile)
            .FirstOrDefaultAsync(c => c.CommunityId == communityId && c.IsActive);
    }

    public async Task<(IEnumerable<Community> Items, string? NextCursor)> GetPublicCommunitiesAsync(
        string? cursor = null, 
        int pageSize = DefaultPageSize,
        string? sortBy = null)
    {
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        
        int? cursorId = null;
        if (!string.IsNullOrEmpty(cursor) && int.TryParse(cursor, out var id))
        {
            cursorId = id;
        }

        var query = _context.Communities
            .Include(c => c.OwnerAccount)
                .ThenInclude(a => a!.Profile)
            .Where(c => c.IsActive && c.Visibility == CommunityVisibility.Public);

        if (cursorId.HasValue)
        {
            query = query.Where(c => c.Id < cursorId.Value);
        }

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "name" => query.OrderBy(c => c.Name),
            "newest" => query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.MemberCount)
        };

        var communities = await query.Take(pageSize + 1).ToListAsync();

        string? nextCursor = null;
        if (communities.Count > pageSize)
        {
            communities = communities.Take(pageSize).ToList();
            nextCursor = communities.Last().Id.ToString();
        }

        return (communities, nextCursor);
    }

    public async Task<(IEnumerable<Community> Items, string? NextCursor)> SearchCommunitiesAsync(
        string? query,
        string? topic,
        string? cursor = null,
        int pageSize = DefaultPageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        
        int? cursorId = null;
        if (!string.IsNullOrEmpty(cursor) && int.TryParse(cursor, out var id))
        {
            cursorId = id;
        }

        var communitiesQuery = _context.Communities
            .Include(c => c.OwnerAccount)
                .ThenInclude(a => a!.Profile)
            .Where(c => c.IsActive);

        if (!string.IsNullOrEmpty(topic))
        {
            communitiesQuery = communitiesQuery.Where(c => c.Topic == topic);
        }

        if (!string.IsNullOrEmpty(query))
        {
            var searchTerm = $"%{query}%";
            communitiesQuery = communitiesQuery.Where(c =>
                EF.Functions.Like(c.Name, searchTerm) ||
                (c.Description != null && EF.Functions.Like(c.Description, searchTerm)) ||
                (c.Tags != null && EF.Functions.Like(c.Tags, searchTerm)));
        }

        if (cursorId.HasValue)
        {
            communitiesQuery = communitiesQuery.Where(c => c.Id < cursorId.Value);
        }

        var communities = await communitiesQuery
            .OrderByDescending(c => c.MemberCount)
            .ThenByDescending(c => c.Id)
            .Take(pageSize + 1)
            .ToListAsync();

        string? nextCursor = null;
        if (communities.Count > pageSize)
        {
            communities = communities.Take(pageSize).ToList();
            nextCursor = communities.Last().Id.ToString();
        }

        return (communities, nextCursor);
    }

    public async Task<IEnumerable<Community>> GetByTopicAsync(string topic, int limit = 20)
    {
        return await _context.Communities
            .Where(c => c.IsActive && c.Topic == topic)
            .OrderByDescending(c => c.MemberCount)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<CommunityMembership?> JoinCommunityAsync(int accountId, string slug)
    {
        var community = await _context.Communities
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

        if (community == null)
        {
            _logger.LogWarning("JoinCommunity failed: community {Slug} not found", slug);
            return null;
        }

        if (community.Visibility != CommunityVisibility.Public)
        {
            _logger.LogWarning("JoinCommunity failed: community {Slug} is not public", slug);
            return null;
        }

        var existingMembership = await _context.CommunityMemberships
            .FirstOrDefaultAsync(m => 
                m.CommunityId == community.Id && 
                m.AccountId == accountId && 
                m.IsActive);

        if (existingMembership != null)
        {
            _logger.LogWarning("JoinCommunity failed: already a member of {Slug}", slug);
            return null;
        }

        var membership = new CommunityMembership
        {
            CommunityId = community.Id,
            AccountId = accountId,
            Role = CommunityRole.Member,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.CommunityMemberships.Add(membership);
        community.MemberCount++;
        community.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Account {AccountId} joined community {Slug}", accountId, slug);
        return membership;
    }

    public async Task<bool> LeaveCommunityAsync(int accountId, string slug)
    {
        var community = await _context.Communities
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

        if (community == null)
        {
            return false;
        }

        if (community.OwnerAccountId == accountId)
        {
            _logger.LogWarning("LeaveCommunity failed: owner cannot leave community {Slug}", slug);
            return false;
        }

        var membership = await _context.CommunityMemberships
            .FirstOrDefaultAsync(m => 
                m.CommunityId == community.Id && 
                m.AccountId == accountId && 
                m.IsActive);

        if (membership == null)
        {
            return false;
        }

        membership.IsActive = false;
        community.MemberCount = Math.Max(0, community.MemberCount - 1);
        community.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Account {AccountId} left community {Slug}", accountId, slug);
        return true;
    }

    public async Task<CommunityMembership?> GetMembershipAsync(int accountId, int communityId)
    {
        return await _context.CommunityMemberships
            .Include(m => m.Community)
            .Include(m => m.Account)
                .ThenInclude(a => a!.Profile)
            .FirstOrDefaultAsync(m => 
                m.CommunityId == communityId && 
                m.AccountId == accountId && 
                m.IsActive);
    }

    public async Task<(IEnumerable<CommunityMembership> Items, string? NextCursor)> GetMembersAsync(
        int communityId, 
        string? cursor = null, 
        int pageSize = DefaultPageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        
        int? cursorId = null;
        if (!string.IsNullOrEmpty(cursor) && int.TryParse(cursor, out var id))
        {
            cursorId = id;
        }

        var query = _context.CommunityMemberships
            .Include(m => m.Account)
                .ThenInclude(a => a!.Profile)
            .Where(m => m.CommunityId == communityId && m.IsActive);

        if (cursorId.HasValue)
        {
            query = query.Where(m => m.Id < cursorId.Value);
        }

        var members = await query
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.JoinedAt)
            .Take(pageSize + 1)
            .ToListAsync();

        string? nextCursor = null;
        if (members.Count > pageSize)
        {
            members = members.Take(pageSize).ToList();
            nextCursor = members.Last().Id.ToString();
        }

        return (members, nextCursor);
    }

    public async Task<IEnumerable<Community>> GetAccountCommunitiesAsync(int accountId)
    {
        return await _context.CommunityMemberships
            .Where(m => m.AccountId == accountId && m.IsActive)
            .Include(m => m.Community)
            .Select(m => m.Community!)
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.MemberCount)
            .ToListAsync();
    }

    public async Task<CommunityRole?> GetMemberRoleAsync(int accountId, int communityId)
    {
        var community = await _context.Communities.FindAsync(communityId);
        
        if (community?.OwnerAccountId == accountId)
        {
            return CommunityRole.Owner;
        }

        var membership = await _context.CommunityMemberships
            .FirstOrDefaultAsync(m => 
                m.CommunityId == communityId && 
                m.AccountId == accountId && 
                m.IsActive);

        return membership?.Role;
    }

    public async Task<(IEnumerable<Post> Items, string? NextCursor)> GetCommunityFeedAsync(
        int communityId,
        string? cursor = null,
        int pageSize = DefaultPageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        
        DateTime? cursorTimestamp = null;
        int? cursorPostId = null;
        
        if (!string.IsNullOrEmpty(cursor))
        {
            var parts = cursor.Split('_');
            if (parts.Length == 2)
            {
                if (DateTime.TryParse(parts[0], out var ts))
                {
                    cursorTimestamp = ts;
                }
                if (int.TryParse(parts[1], out var pid))
                {
                    cursorPostId = pid;
                }
            }
        }

        var query = _context.Posts
            .Include(p => p.AuthorAccount)
                .ThenInclude(a => a!.Profile)
            .Where(p => p.CommunityId == communityId && p.Status == PostStatus.Active);

        if (cursorTimestamp.HasValue && cursorPostId.HasValue)
        {
            query = query.Where(p => 
                p.CreatedAt < cursorTimestamp.Value ||
                (p.CreatedAt == cursorTimestamp.Value && p.Id < cursorPostId.Value));
        }

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(pageSize + 1)
            .ToListAsync();

        string? nextCursor = null;
        if (posts.Count > pageSize)
        {
            var lastPost = posts[pageSize - 1];
            nextCursor = $"{lastPost.CreatedAt:O}_{lastPost.Id}";
            posts = posts.Take(pageSize).ToList();
        }

        return (posts, nextCursor);
    }

    public async Task<Community> CreateCommunityAsync(
        string name,
        string topic,
        int ownerAccountId,
        string? description = null,
        string? tags = null,
        CommunityVisibility visibility = CommunityVisibility.Public)
    {
        var slug = GenerateSlug(name);

        var community = new Community
        {
            Name = name,
            Slug = slug,
            Description = description,
            Topic = topic,
            Tags = tags,
            OwnerAccountId = ownerAccountId,
            Visibility = visibility,
            IsActive = true,
            MemberCount = 1,
            PostCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Communities.Add(community);
        
        var ownerMembership = new CommunityMembership
        {
            CommunityId = community.Id,
            AccountId = ownerAccountId,
            Role = CommunityRole.Owner,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.CommunityMemberships.Add(ownerMembership);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created community {Name} ({Slug}) by account {AccountId}", name, slug, ownerAccountId);
        return community;
    }

    public async Task<IEnumerable<Community>> GetRelevantCommunitiesForNpcAsync(IEnumerable<string> interests, int limit = 10)
    {
        var interestList = interests.Select(i => i.ToLowerInvariant()).ToList();
        
        if (!interestList.Any())
        {
            return Enumerable.Empty<Community>();
        }

        // Get all public active communities first, then filter in memory
        var communities = await _context.Communities
            .Where(c => c.IsActive && c.Visibility == CommunityVisibility.Public)
            .OrderByDescending(c => c.MemberCount)
            .ToListAsync();

        // Filter by matching interests
        return communities
            .Where(c => interestList.Contains(c.Topic.ToLowerInvariant()) ||
                       (c.Tags != null && interestList.Any(tag => 
                           c.Tags.ToLowerInvariant().Contains(tag))))
            .Take(limit);
    }

    public async Task<bool> IsMemberAsync(int accountId, int communityId)
    {
        var community = await _context.Communities.FindAsync(communityId);
        
        if (community == null)
        {
            return false;
        }

        if (community.OwnerAccountId == accountId)
        {
            return true;
        }

        return await _context.CommunityMemberships
            .AnyAsync(m => 
                m.CommunityId == communityId && 
                m.AccountId == accountId && 
                m.IsActive);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        slug = slug.Trim('-');
        if (slug.Length > 50)
        {
            slug = slug.Substring(0, 50);
        }
        return slug;
    }
}
