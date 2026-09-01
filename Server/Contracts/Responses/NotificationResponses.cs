namespace SocialMediaSimulator.Server.Contracts.Responses;

/// <summary>
/// A single notification item for API responses.
/// Includes enough information for the client to render without extra calls.
/// </summary>
public record NotificationResponse(
    Guid NotificationId,
    string Type,
    Guid ActorAccountId,
    string ActorUsername,
    string ActorDisplayName,
    string? ActorAvatarUrl,
    Guid? RelatedPostId,
    string? RelatedPostSnippet,
    bool IsPostDeleted,
    DateTime CreatedAt,
    bool IsRead,
    DateTime? ReadAt
);

/// <summary>
/// Paginated list of notifications
/// </summary>
public record PaginatedNotificationsResponse(
    IEnumerable<NotificationResponse> Notifications,
    string? NextCursor,
    bool HasMore
);

/// <summary>
/// Unread notification count
/// </summary>
public record UnreadCountResponse(
    int UnreadCount
);

/// <summary>
/// Mark notification as read result
/// </summary>
public record MarkReadResponse(
    bool Success,
    string Message
);

/// <summary>
/// Mark all notifications as read result
/// </summary>
public record MarkAllReadResponse(
    int MarkedAsReadCount
);
