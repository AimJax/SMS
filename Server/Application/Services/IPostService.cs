using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

public interface IPostService
{
    // Post operations
    Task<Post?> CreatePostAsync(int authorAccountId, string content);
    Task<Post?> GetPostByIdAsync(Guid postId);
    Task<Post?> GetPostByIdAsync(int id);
    Task<bool> DeletePostAsync(int accountId, Guid postId);
    
    // Like operations
    Task<PostLike?> LikePostAsync(int accountId, Guid postId);
    Task<bool> UnlikePostAsync(int accountId, Guid postId);
    Task<bool> IsPostLikedByAccountAsync(int accountId, Guid postId);
    
    // Comment operations
    Task<Comment?> CreateCommentAsync(int authorAccountId, Guid postId, string content);
    Task<Comment?> GetCommentByIdAsync(Guid commentId);
    Task<bool> DeleteCommentAsync(int accountId, Guid commentId);
    Task<(IEnumerable<Comment> Items, int TotalCount)> GetCommentsAsync(Guid postId, int page = 1, int pageSize = 20);
    
    // Engagement counts
    Task<int> GetLikeCountAsync(Guid postId);
    Task<int> GetCommentCountAsync(Guid postId);
    
    // Topic-based post queries
    Task<(IEnumerable<Post> Items, string? NextCursor)> GetPostsByTopicAsync(string topic, string? cursor = null, int pageSize = 20);
    Task<IEnumerable<Post>> GetRecentPostsAsync(DateTime since, int limit = 100);
}
