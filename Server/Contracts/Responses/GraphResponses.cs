namespace SocialMediaSimulator.Server.Contracts.Responses;

/// <summary>
/// Summary of an account for graph display
/// </summary>
public record AccountSummaryResponse(
    Guid AccountId,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string AccountType,
    DateTime CreatedAt
);

/// <summary>
/// Paginated list of account summaries
/// </summary>
public record PaginatedAccountsResponse(
    IEnumerable<AccountSummaryResponse> Accounts,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Relationship status between two accounts
/// </summary>
public record RelationshipResponse(
    Guid AccountId,
    bool IsFollowing,
    bool IsFollowedBy,
    bool IsMutual,
    bool IsBlocking,
    bool IsBlockedBy,
    bool IsMuting
);

/// <summary>
/// Result of a graph action
/// </summary>
public record GraphActionResponse(
    bool Success,
    string Message
);
