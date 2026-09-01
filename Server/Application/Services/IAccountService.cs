using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

public interface IAccountService
{
    /// <summary>
    /// Register a new account
    /// </summary>
    Task<Account> RegisterAsync(string username, string password, string? displayName, string? bio, string? email);

    /// <summary>
    /// Authenticate an account and return it if valid
    /// </summary>
    Task<Account?> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Get account by ID
    /// </summary>
    Task<Account?> GetByIdAsync(int id);

    /// <summary>
    /// Get account by AccountId (stable GUID)
    /// </summary>
    Task<Account?> GetByAccountIdAsync(Guid accountId);

    /// <summary>
    /// Get account by username (case-insensitive)
    /// </summary>
    Task<Account?> GetByUsernameAsync(string username);

    /// <summary>
    /// Check if username is available
    /// </summary>
    Task<bool> IsUsernameAvailableAsync(string username);
    
    /// <summary>
    /// Adjust follower count for an account
    /// </summary>
    Task AdjustFollowerCountAsync(int accountId, int delta, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adjust fame level for an account
    /// </summary>
    Task AdjustFameLevelAsync(int accountId, float delta, CancellationToken cancellationToken = default);
}
