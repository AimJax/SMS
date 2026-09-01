using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Application.Models;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class NpcPopulationService : INpcPopulationService
{
    private readonly AppDbContext _context;
    private readonly INpcService _npcService;
    
    // Default batch size for database operations
    private const int DefaultBatchSize = 50;
    
    // Maximum retries for username collision
    private const int MaxUsernameRetries = 100;

    public NpcPopulationService(AppDbContext context, INpcService npcService)
    {
        _context = context;
        _npcService = npcService;
    }

    public async Task<PopulationResult> GeneratePopulationAsync(PopulationConfig config)
    {
        // Validate configuration
        if (!ValidateConfig(config, out var errorMessage))
        {
            return PopulationResult.FailureResult(errorMessage);
        }
        
        // Check if population already exists
        var existingCount = await GetExistingNpcCountAsync();
        if (existingCount > 0)
        {
            return PopulationResult.FailureResult(
                $"Population already exists with {existingCount} NPCs. Clear existing population before generating a new one.");
        }
        
        return await GenerateInternalAsync(config);
    }

    public async Task<PopulationResult> GeneratePopulationAsync(int populationSize, int? seed = null)
    {
        var config = new PopulationConfig
        {
            PopulationSize = populationSize,
            RandomSeed = seed
        };
        
        return await GeneratePopulationAsync(config);
    }

    public async Task<int> GetExistingNpcCountAsync()
    {
        return await _context.NpcProfiles.CountAsync();
    }

    public async Task<bool> PopulationExistsAsync()
    {
        return await _context.NpcProfiles.AnyAsync();
    }

    public bool ValidateConfig(PopulationConfig config, out string errorMessage)
    {
        if (config.PopulationSize <= 0)
        {
            errorMessage = "Population size must be greater than 0";
            return false;
        }
        
        if (config.PopulationSize > 100000)
        {
            errorMessage = "Population size cannot exceed 100,000";
            return false;
        }
        
        if (!config.Distribution.IsValid(out errorMessage))
        {
            return false;
        }
        
        errorMessage = string.Empty;
        return true;
    }

    private async Task<PopulationResult> GenerateInternalAsync(PopulationConfig config)
    {
        var stopwatch = Stopwatch.StartNew();
        var distribution = new Dictionary<AccountType, int>();
        var successful = 0;
        var failed = 0;
        
        // Initialize distribution counters
        foreach (AccountType type in Enum.GetValues<AccountType>())
        {
            distribution[type] = 0;
        }
        
        // Create generators with seed
        var seed = config.RandomSeed ?? Environment.TickCount;
        var usernameGenerator = new UsernameGenerator(seed);
        var profileGenerator = new ProfileGenerator(seed);
        var random = new Random(seed);
        
        // Calculate account type distribution
        var accountTypes = CalculateAccountTypeDistribution(config.Distribution, config.PopulationSize, random);
        
        try
        {
            // Process in batches
            for (int i = 0; i < config.PopulationSize; i++)
            {
                var accountType = accountTypes[i];
                
                try
                {
                    // Generate unique username
                    var username = await GenerateUniqueUsernameAsync(usernameGenerator, random);
                    usernameGenerator.Register(username);
                    
                    // Generate profile data
                    var displayName = profileGenerator.GenerateDisplayName(accountType);
                    var bio = profileGenerator.GenerateBio(accountType);
                    var avatarUrl = profileGenerator.GenerateAvatarUrl(username);
                    
                    // Create NPC (single NPC in transaction)
                    var npc = await _npcService.CreateNpcAsync(username, displayName, bio, accountType);
                    
                    // Update profile with avatar
                    await UpdateProfileAvatarAsync(npc.AccountId, avatarUrl);
                    
                    distribution[accountType]++;
                    successful++;
                    
                    // Progress logging every 100 NPCs
                    if (successful % 100 == 0)
                    {
                        Console.WriteLine($"Generated {successful}/{config.PopulationSize} NPCs...");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"Failed to create NPC at index {i}: {ex.Message}");
                    
                    if (failed > config.PopulationSize * 0.1) // Stop if more than 10% fail
                    {
                        throw new InvalidOperationException(
                            $"Too many failures ({failed}) during population generation. Stopping.", ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new PopulationResult
            {
                Success = false,
                NpcsCreated = successful,
                NpcsFailed = failed,
                Elapsed = stopwatch.Elapsed,
                ErrorMessage = ex.Message,
                Distribution = distribution,
                SeedUsed = seed,
                BatchId = config.BatchId
            };
        }
        
        stopwatch.Stop();
        
        Console.WriteLine($"Population generation complete: {successful} NPCs in {stopwatch.Elapsed.TotalSeconds:F2}s");
        
        return PopulationResult.SuccessResult(
            successful, 
            stopwatch.Elapsed, 
            distribution, 
            seed, 
            config.BatchId);
    }

    private async Task<string> GenerateUniqueUsernameAsync(UsernameGenerator generator, Random random)
    {
        for (int retry = 0; retry < MaxUsernameRetries; retry++)
        {
            var username = generator.Generate();
            
            // Check if username is available in database
            var isAvailable = await _context.Accounts
                .AnyAsync(a => a.UsernameNormalized == username.ToUpperInvariant());
            
            if (!isAvailable)
            {
                return username;
            }
        }
        
        // Fallback with timestamp-based username
        return $"npc_{DateTime.UtcNow:yyyyMMddHHmmss}_{random.Next(1000, 9999)}";
    }

    private async Task UpdateProfileAvatarAsync(int accountId, string avatarUrl)
    {
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.AccountId == accountId);
        if (profile != null)
        {
            profile.AvatarUrl = avatarUrl;
            profile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static AccountType[] CalculateAccountTypeDistribution(
        AccountTypeDistribution distribution, 
        int populationSize, 
        Random random)
    {
        var types = new List<AccountType>();
        
        // Calculate counts for each type
        var celebrityCount = (int)Math.Round(populationSize * distribution.Celebrity / 100.0);
        var officialCount = (int)Math.Round(populationSize * distribution.Official / 100.0);
        var newsCount = (int)Math.Round(populationSize * distribution.News / 100.0);
        var influencerCount = (int)Math.Round(populationSize * distribution.Influencer / 100.0);
        var creatorCount = (int)Math.Round(populationSize * distribution.Creator / 100.0);
        var ordinaryCount = populationSize - celebrityCount - officialCount - newsCount - influencerCount - creatorCount;
        
        // Add celebrities
        for (int i = 0; i < celebrityCount; i++) types.Add(AccountType.Celebrity);
        
        // Add officials
        for (int i = 0; i < officialCount; i++) types.Add(AccountType.Official);
        
        // Add news
        for (int i = 0; i < newsCount; i++) types.Add(AccountType.News);
        
        // Add influencers
        for (int i = 0; i < influencerCount; i++) types.Add(AccountType.Influencer);
        
        // Add creators
        for (int i = 0; i < creatorCount; i++) types.Add(AccountType.Creator);
        
        // Add ordinary users (remaining)
        for (int i = 0; i < ordinaryCount; i++) types.Add(AccountType.OrdinaryUser);
        
        // Shuffle using Fisher-Yates
        for (int i = types.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (types[i], types[j]) = (types[j], types[i]);
        }
        
        return types.ToArray();
    }
}
