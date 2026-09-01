namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Represents a block relationship between accounts
/// </summary>
public class Block
{
    public int Id { get; set; }
    
    /// <summary>
    /// The account that blocked the other
    /// </summary>
    public int BlockerAccountId { get; set; }
    
    /// <summary>
    /// The account that was blocked
    /// </summary>
    public int BlockedAccountId { get; set; }
    
    /// <summary>
    /// When the block was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? BlockerAccount { get; set; }
    public Account? BlockedAccount { get; set; }
}
