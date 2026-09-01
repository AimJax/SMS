using SocialMediaSimulator.Server.Application.Models;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for generating NPC populations
/// </summary>
public interface INpcPopulationService
{
    /// <summary>
    /// Generate a population of NPCs
    /// </summary>
    /// <param name="config">Population configuration</param>
    /// <returns>Result of generation operation</returns>
    Task<PopulationResult> GeneratePopulationAsync(PopulationConfig config);
    
    /// <summary>
    /// Generate a population using default configuration
    /// </summary>
    /// <param name="populationSize">Number of NPCs to generate</param>
    /// <param name="seed">Random seed for deterministic generation</param>
    /// <returns>Result of generation operation</returns>
    Task<PopulationResult> GeneratePopulationAsync(int populationSize, int? seed = null);
    
    /// <summary>
    /// Get the count of existing NPCs in the database
    /// </summary>
    Task<int> GetExistingNpcCountAsync();
    
    /// <summary>
    /// Check if population already exists
    /// </summary>
    Task<bool> PopulationExistsAsync();
    
    /// <summary>
    /// Validate population configuration
    /// </summary>
    bool ValidateConfig(PopulationConfig config, out string errorMessage);
}
