namespace SocialMediaSimulator.Server.Application.Models;

/// <summary>
/// Configuration for NPC population generation
/// </summary>
public class PopulationConfig
{
    /// <summary>
    /// Target number of NPCs to generate
    /// </summary>
    public int PopulationSize { get; set; } = 1000;
    
    /// <summary>
    /// Random seed for deterministic generation (null for random)
    /// </summary>
    public int? RandomSeed { get; set; }
    
    /// <summary>
    /// Account type distribution percentages (must sum to 100)
    /// </summary>
    public AccountTypeDistribution Distribution { get; set; } = AccountTypeDistribution.Default;
    
    /// <summary>
    /// Batch identifier for this generation (optional)
    /// </summary>
    public string? BatchId { get; set; }
}

/// <summary>
/// Account type distribution configuration
/// </summary>
public class AccountTypeDistribution
{
    /// <summary>
    /// Percentage of OrdinaryUser accounts (0-100)
    /// </summary>
    public double OrdinaryUser { get; set; } = 70;
    
    /// <summary>
    /// Percentage of Creator accounts (0-100)
    /// </summary>
    public double Creator { get; set; } = 12;
    
    /// <summary>
    /// Percentage of Influencer accounts (0-100)
    /// </summary>
    public double Influencer { get; set; } = 7;
    
    /// <summary>
    /// Percentage of News accounts (0-100)
    /// </summary>
    public double News { get; set; } = 5;
    
    /// <summary>
    /// Percentage of Official accounts (0-100)
    /// </summary>
    public double Official { get; set; } = 4;
    
    /// <summary>
    /// Percentage of Celebrity accounts (0-100)
    /// </summary>
    public double Celebrity { get; set; } = 2;
    
    /// <summary>
    /// Default distribution: 70% OrdinaryUser, 12% Creator, 7% Influencer, 5% News, 4% Official, 2% Celebrity
    /// </summary>
    public static AccountTypeDistribution Default => new()
    {
        OrdinaryUser = 70,
        Creator = 12,
        Influencer = 7,
        News = 5,
        Official = 4,
        Celebrity = 2
    };
    
    /// <summary>
    /// Validate that distribution percentages sum to approximately 100
    /// </summary>
    public bool IsValid(out string errorMessage)
    {
        var total = OrdinaryUser + Creator + Influencer + News + Official + Celebrity;
        if (Math.Abs(total - 100.0) > 0.01)
        {
            errorMessage = $"Distribution percentages must sum to 100, but got {total}";
            return false;
        }
        
        if (OrdinaryUser < 0 || Creator < 0 || Influencer < 0 || News < 0 || Official < 0 || Celebrity < 0)
        {
            errorMessage = "All distribution percentages must be non-negative";
            return false;
        }
        
        errorMessage = string.Empty;
        return true;
    }
    
    /// <summary>
    /// Get distribution for a specific account type
    /// </summary>
    public double GetPercentage(Domain.Entities.AccountType accountType)
    {
        return accountType switch
        {
            Domain.Entities.AccountType.OrdinaryUser => OrdinaryUser,
            Domain.Entities.AccountType.Creator => Creator,
            Domain.Entities.AccountType.Influencer => Influencer,
            Domain.Entities.AccountType.News => News,
            Domain.Entities.AccountType.Official => Official,
            Domain.Entities.AccountType.Celebrity => Celebrity,
            _ => 0
        };
    }
}
