using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context;

    public AccountService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Account> RegisterAsync(string username, string password, string? displayName, string? bio, string? email)
    {
        // Normalize username
        var normalizedUsername = username.ToUpperInvariant();

        // Check if username is available
        if (await IsUsernameAvailableAsync(normalizedUsername) == false)
        {
            throw new InvalidOperationException("Username is already taken");
        }

        // Create account
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Username = username,
            UsernameNormalized = normalizedUsername,
            PasswordHash = HashPassword(password),
            Email = email,
            AccountType = AccountType.OrdinaryUser,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create profile
        var profile = new Profile
        {
            DisplayName = displayName ?? username,
            Bio = bio,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create initial history
        var history = new AccountHistory
        {
            EventType = AccountHistoryEventType.Created,
            Details = $"Account created with username: {username}",
            CreatedAt = DateTime.UtcNow
        };

        // Begin transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Set up relationships
            profile.AccountId = account.Id;
            history.AccountId = account.Id;

            _context.Profiles.Add(profile);
            _context.AccountHistory.Add(history);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // Load profile for return
            account.Profile = profile;
            return account;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Account?> AuthenticateAsync(string username, string password)
    {
        var normalizedUsername = username.ToUpperInvariant();

        var account = await _context.Accounts
            .Include(a => a.Profile)
            .FirstOrDefaultAsync(a => a.UsernameNormalized == normalizedUsername);

        if (account == null)
        {
            return null;
        }

        // Verify password
        if (!VerifyPassword(password, account.PasswordHash))
        {
            return null;
        }

        // Check account status
        if (account.Status != AccountStatus.Active)
        {
            return null;
        }

        return account;
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .Include(a => a.Profile)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Accounts
            .Include(a => a.Profile)
            .FirstOrDefaultAsync(a => a.AccountId == accountId);
    }

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        var normalizedUsername = username.ToUpperInvariant();

        return await _context.Accounts
            .Include(a => a.Profile)
            .FirstOrDefaultAsync(a => a.UsernameNormalized == normalizedUsername);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        var normalizedUsername = username.ToUpperInvariant();

        return !await _context.Accounts
            .AnyAsync(a => a.UsernameNormalized == normalizedUsername);
    }

    public async Task AdjustFollowerCountAsync(int accountId, int delta, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .Include(a => a.Profile)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        
        if (account?.Profile != null)
        {
            account.Profile.FollowerCount = Math.Max(0, account.Profile.FollowerCount + delta);
            account.Profile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AdjustFameLevelAsync(int accountId, float delta, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .Include(a => a.Profile)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        
        if (account?.Profile != null)
        {
            account.Profile.FameLevel = Math.Max(0, account.Profile.FameLevel + delta);
            account.Profile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string HashPassword(string password)
    {
        // Using PBKDF2 with SHA256
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        // Combine salt and hash
        byte[] hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        byte[] hashBytes = Convert.FromBase64String(storedHash);

        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != hash[i])
            {
                return false;
            }
        }

        return true;
    }
}
