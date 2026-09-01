namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Interest categories for NPC content preferences
/// </summary>
public static class InterestCategories
{
    public const string Gaming = "Gaming";
    public const string Politics = "Politics";
    public const string Sports = "Sports";
    public const string Technology = "Technology";
    public const string Music = "Music";
    public const string Movies = "Movies";
    public const string Television = "Television";
    public const string Fashion = "Fashion";
    public const string Food = "Food";
    public const string Travel = "Travel";
    public const string Science = "Science";
    public const string Health = "Health";
    public const string Business = "Business";
    public const string Finance = "Finance";
    public const string Education = "Education";
    public const string LocalNews = "LocalNews";
    public const string WorldNews = "WorldNews";
    public const string Entertainment = "Entertainment";
    public const string GamingNews = "GamingNews";
    public const string SportsNews = "SportsNews";
    public const string TechNews = "TechNews";
    
    /// <summary>
    /// All available interest categories
    /// </summary>
    public static readonly string[] All = new[]
    {
        Gaming, Politics, Sports, Technology, Music, Movies, Television,
        Fashion, Food, Travel, Science, Health, Business, Finance,
        Education, LocalNews, WorldNews, Entertainment, GamingNews,
        SportsNews, TechNews
    };
}

/// <summary>
/// Represents an NPC's interest in a particular category
/// </summary>
public class NpcInterest
{
    public int Id { get; set; }
    
    /// <summary>
    /// Reference to the associated NPC profile
    /// </summary>
    public int NpcProfileId { get; set; }
    
    /// <summary>
    /// Interest category key
    /// </summary>
    public string InterestKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Interest strength (0.0 - 1.0)
    /// </summary>
    public double Strength { get; set; }
    
    /// <summary>
    /// When interest was assigned
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public NpcProfile? NpcProfile { get; set; }
}
