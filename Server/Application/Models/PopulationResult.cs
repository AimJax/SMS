using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Result of a population generation operation
/// </summary>
public class PopulationResult
{
    /// <summary>
    /// Whether generation succeeded
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Number of NPCs successfully created
    /// </summary>
    public int NpcsCreated { get; set; }
    
    /// <summary>
    /// Number of NPCs that failed to create
    /// </summary>
    public int NpcsFailed { get; set; }
    
    /// <summary>
    /// Time taken for generation
    /// </summary>
    public TimeSpan Elapsed { get; set; }
    
    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Distribution statistics of generated NPCs
    /// </summary>
    public Dictionary<AccountType, int> Distribution { get; set; } = new();
    
    /// <summary>
    /// Seed used for generation
    /// </summary>
    public int? SeedUsed { get; set; }
    
    /// <summary>
    /// Batch identifier for this generation
    /// </summary>
    public string? BatchId { get; set; }
    
    public static PopulationResult SuccessResult(int npcsCreated, TimeSpan elapsed, Dictionary<AccountType, int> distribution, int? seed, string? batchId)
    {
        return new PopulationResult
        {
            Success = true,
            NpcsCreated = npcsCreated,
            NpcsFailed = 0,
            Elapsed = elapsed,
            Distribution = distribution,
            SeedUsed = seed,
            BatchId = batchId
        };
    }
    
    public static PopulationResult FailureResult(string errorMessage)
    {
        return new PopulationResult
        {
            Success = false,
            NpcsCreated = 0,
            NpcsFailed = 0,
            ErrorMessage = errorMessage
        };
    }
}
