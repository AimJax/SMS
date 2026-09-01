namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Represents a mute relationship between accounts
/// </summary>
public class Mute
{
    public int Id { get; set; }
    
    /// <summary>
    /// The account that muted the other
    /// </summary>
    public int MuterAccountId { get; set; }
    
    /// <summary>
    /// The account that was muted
    /// </summary>
    public int MutedAccountId { get; set; }
    
    /// <summary>
    /// When the mute was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? MuterAccount { get; set; }
    public Account? MutedAccount { get; set; }
}
