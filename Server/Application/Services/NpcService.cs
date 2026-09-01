using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class NpcService : INpcService
{
    private readonly AppDbContext _context;
    
    // Default simulation interval: 30 seconds
    private const int DefaultSimulationIntervalSeconds = 30;
    
    // Number of interests per NPC
    private const int InterestsPerNpc = 5;

    public NpcService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NpcProfile> CreateNpcAsync(string username, string? displayName, string? bio, AccountType accountType)
    {
        // Generate unique NPC ID
        var npcId = Guid.NewGuid();
        
        // Normalize username
        var normalizedUsername = username.ToUpperInvariant();
        
        // Check if username is available
        if (!await IsUsernameAvailableInternalAsync(normalizedUsername))
        {
            throw new InvalidOperationException("Username is already taken");
        }
        
        // Generate deterministic personality and interests based on NPC ID as seed
        var personality = GeneratePersonality(npcId);
        var interests = GenerateInterests(accountType, npcId).ToList();
        
        // Determine simulation interval based on account type
        var intervalSeconds = GetSimulationInterval(accountType);
        
        // Begin transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Create account (NPC accounts need a password hash even if unused)
            var account = new Account
            {
                AccountId = Guid.NewGuid(),
                Username = username,
                UsernameNormalized = normalizedUsername,
                PasswordHash = GenerateNpcPasswordHash(), // Placeholder hash
                AccountType = accountType,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            
            // Create profile
            var profile = new Profile
            {
                AccountId = account.Id,
                DisplayName = displayName ?? username,
                Bio = bio,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Profiles.Add(profile);
            
            // Create NPC profile
            var npcProfile = new NpcProfile
            {
                NpcId = npcId,
                AccountId = account.Id,
                IsActive = true,
                ActivityState = NpcActivityState.Idle,
                LastSimulatedAt = null,
                NextSimulationAt = DateTime.UtcNow.AddSeconds(intervalSeconds),
                SimulationIntervalSeconds = intervalSeconds,
                SimulationVersion = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.NpcProfiles.Add(npcProfile);
            await _context.SaveChangesAsync();
            
            // Now set the personality and save
            personality.NpcProfileId = npcProfile.Id;
            _context.NpcPersonalities.Add(personality);
            
            // Add interests with proper NPC profile ID
            foreach (var interest in interests)
            {
                interest.NpcProfileId = npcProfile.Id;
                _context.NpcInterests.Add(interest);
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Reload with navigation properties
            return await GetByNpcIdAsync(npcId) 
                ?? throw new InvalidOperationException("Failed to create NPC");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<NpcProfile?> GetByNpcIdAsync(Guid npcId)
    {
        return await _context.NpcProfiles
            .Include(n => n.Account)
                .ThenInclude(a => a!.Profile)
            .Include(n => n.Personality)
            .Include(n => n.Interests)
            .FirstOrDefaultAsync(n => n.NpcId == npcId);
    }

    public async Task<NpcProfile?> GetByAccountIdAsync(int accountId)
    {
        return await _context.NpcProfiles
            .Include(n => n.Account)
                .ThenInclude(a => a!.Profile)
            .Include(n => n.Personality)
            .Include(n => n.Interests)
            .FirstOrDefaultAsync(n => n.AccountId == accountId);
    }

    public async Task<bool> IsNpcAsync(int accountId)
    {
        return await _context.NpcProfiles.AnyAsync(n => n.AccountId == accountId);
    }

    public async Task<bool> IsNpcByAccountIdAsync(Guid accountId)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
        if (account == null) return false;
        return await IsNpcAsync(account.Id);
    }

    public async Task<bool> DeactivateAsync(Guid npcId)
    {
        var npc = await _context.NpcProfiles.FirstOrDefaultAsync(n => n.NpcId == npcId);
        if (npc == null) return false;
        
        npc.IsActive = false;
        npc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateAsync(Guid npcId)
    {
        var npc = await _context.NpcProfiles.FirstOrDefaultAsync(n => n.NpcId == npcId);
        if (npc == null) return false;
        
        npc.IsActive = true;
        npc.NextSimulationAt = DateTime.UtcNow;
        npc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public NpcPersonality GeneratePersonality(Guid seed)
    {
        // Use deterministic random based on seed
        var random = new Random(seed.GetHashCode());
        
        return new NpcPersonality
        {
            Openness = Math.Round(random.NextDouble(), 2),
            Conscientiousness = Math.Round(random.NextDouble(), 2),
            Extraversion = Math.Round(random.NextDouble(), 2),
            Agreeableness = Math.Round(random.NextDouble(), 2),
            Neuroticism = Math.Round(random.NextDouble(), 2),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public IEnumerable<NpcInterest> GenerateInterests(AccountType accountType, Guid seed)
    {
        var random = new Random(seed.GetHashCode());
        
        // Get base interests for account type
        var baseInterests = GetBaseInterestsForAccountType(accountType);
        
        // Add some randomness - select 2-3 from base + fill remaining from all
        var shuffledBase = baseInterests.OrderBy(_ => random.Next()).Take(2).ToList();
        var shuffledAll = InterestCategories.All
            .Where(i => !shuffledBase.Contains(i))
            .OrderBy(_ => random.Next())
            .Take(InterestsPerNpc - shuffledBase.Count)
            .ToList();
        
        var allSelected = shuffledBase.Concat(shuffledAll).ToList();
        
        foreach (var interest in allSelected)
        {
            // Generate strength between 0.3 and 1.0
            var strength = Math.Round(0.3 + random.NextDouble() * 0.7, 2);
            
            yield return new NpcInterest
            {
                InterestKey = interest,
                Strength = strength,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
    
    private static int GetSimulationInterval(AccountType accountType)
    {
        // Different account types have different activity frequencies
        return accountType switch
        {
            AccountType.Celebrity => 15,      // Very active
            AccountType.News => 20,           // News outlets post frequently
            AccountType.Influencer => 25,     // Influencers are active
            AccountType.Creator => 30,        // Creators have regular content
            AccountType.Official => 45,       // Official accounts less frequent
            _ => DefaultSimulationIntervalSeconds
        };
    }
    
    private static string[] GetBaseInterestsForAccountType(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.News => new[] { InterestCategories.WorldNews, InterestCategories.LocalNews, InterestCategories.Politics },
            AccountType.Creator => new[] { InterestCategories.Gaming, InterestCategories.Music, InterestCategories.Movies, InterestCategories.Entertainment },
            AccountType.Influencer => new[] { InterestCategories.Fashion, InterestCategories.Health, InterestCategories.Travel },
            AccountType.Celebrity => new[] { InterestCategories.Entertainment, InterestCategories.Movies, InterestCategories.Music },
            AccountType.Official => new[] { InterestCategories.LocalNews, InterestCategories.Education, InterestCategories.Business },
            _ => new[] { InterestCategories.Technology, InterestCategories.Entertainment }
        };
    }
    
    private async Task<bool> IsUsernameAvailableInternalAsync(string normalizedUsername)
    {
        return !await _context.Accounts.AnyAsync(a => a.UsernameNormalized == normalizedUsername);
    }
    
    private static string GenerateNpcPasswordHash()
    {
        // Generate a placeholder hash for NPC accounts
        // NPCs don't authenticate normally, but we need a valid hash format
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), // Random content
            salt,
            iterations: 100000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
        
        byte[] hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);
        
        return Convert.ToBase64String(hashBytes);
    }
}
