namespace SocialMediaSimulator.Server.Contracts.Responses;

public record AuthResponse(
    string Token,
    AccountResponse Account
);

public record AccountResponse(
    Guid AccountId,
    string Username,
    string? DisplayName,
    string? Bio,
    string? AvatarUrl,
    string AccountType,
    string Status,
    DateTime CreatedAt
);

public record PublicProfileResponse(
    Guid AccountId,
    string Username,
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    string AccountType
);

public record ErrorResponse(
    string Error,
    int? StatusCode = null
);
