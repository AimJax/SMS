namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Persistent personality traits for an NPC
/// Based on Big Five personality model (normalized 0.0 - 1.0)
/// </summary>
public class NpcPersonality
{
    public int Id { get; set; }
    
    /// <summary>
    /// Reference to the associated NPC profile
    /// </summary>
    public int NpcProfileId { get; set; }
    
    /// <summary>
    /// Openness: curiosity, creativity, openness to new experiences
    /// </summary>
    public double Openness { get; set; }
    
    /// <summary>
    /// Conscientiousness: self-discipline, organization, dependability
    /// </summary>
    public double Conscientiousness { get; set; }
    
    /// <summary>
    /// Extraversion: sociability, energy, assertiveness
    /// </summary>
    public double Extraversion { get; set; }
    
    /// <summary>
    /// Agreeableness: trust, altruism, cooperation
    /// </summary>
    public double Agreeableness { get; set; }
    
    /// <summary>
    /// Neuroticism: emotional instability, tendency to experience negative emotions
    /// </summary>
    public double Neuroticism { get; set; }
    
    /// <summary>
    /// When personality was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public NpcProfile? NpcProfile { get; set; }
}
