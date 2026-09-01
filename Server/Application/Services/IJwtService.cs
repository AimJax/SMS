using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

public interface IJwtService
{
    /// <summary>
    /// Generate a JWT token for an authenticated account
    /// </summary>
    string GenerateToken(Account account);

    /// <summary>
    /// Get account ID from token claims
    /// </summary>
    int? GetAccountIdFromToken(string token);
}
