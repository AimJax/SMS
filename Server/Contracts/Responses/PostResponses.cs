namespace SocialMediaSimulator.Server.Contracts.Responses;

/// <summary>
/// Response for a post with engagement data
/// </summary>
public record PostResponse(
    Guid PostId,
    Guid AuthorAccountId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int LikeCount,
    int CommentCount,
    bool IsLikedByCurrentUser
);

/// <summary>
/// Response for a comment
/// </summary>
public record CommentResponse(
    Guid CommentId,
    Guid PostId,
    Guid AuthorAccountId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// Paginated list of posts
/// </summary>
public record PaginatedPostsResponse(
    IEnumerable<PostResponse> Posts,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Paginated list of comments
/// </summary>
public record PaginatedCommentsResponse(
    IEnumerable<CommentResponse> Comments,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Result of an engagement action (like/unlike)
/// </summary>
public record EngagementActionResponse(
    bool Success,
    string Message
);
