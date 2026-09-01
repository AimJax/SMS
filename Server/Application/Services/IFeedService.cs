using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for retrieving personalized feeds
/// </summary>
public interface IFeedService
{
    /// <summary>
    /// Get feed for an account, including posts from followed accounts
    /// </summary>
    /// <param name="accountId">The account ID requesting the feed</param>
    /// <param name="cursor">Cursor for pagination (null for first page)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple of feed items and next cursor</returns>
    Task<(IEnumerable<FeedItem> Items, string? NextCursor)> GetFeedAsync(int accountId, string? cursor = null, int pageSize = 20);
    
    /// <summary>
    /// Get advanced algorithmic feed with scoring
    /// </summary>
    /// <param name="accountId">The account ID requesting the feed</param>
    /// <param name="cursor">Cursor for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="includeDiscovery">Whether to include discovery content</param>
    /// <param name="echoStrength">Echo chamber strength override (0.0-1.0)</param>
    /// <returns>Advanced feed response with scored items</returns>
    Task<AdvancedFeedResponse> GetAdvancedFeedAsync(
        int accountId, 
        string? cursor = null, 
        int pageSize = 20,
        bool includeDiscovery = true,
        double? echoStrength = null);
}

/// <summary>
/// Represents a single item in the feed with all necessary data
/// </summary>
public class FeedItem
{
    public Post Post { get; set; } = null!;
    public Account AuthorAccount { get; set; } = null!;
    public Profile? AuthorProfile { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}
