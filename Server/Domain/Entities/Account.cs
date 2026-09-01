namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Account status enumeration
/// </summary>
public enum AccountStatus
{
    Active = 0,
    Disabled = 1,
    Suspended = 2,
    Banned = 3
}

/// <summary>
/// Account type enumeration
/// </summary>
public enum AccountType
{
    OrdinaryUser = 0,
    Creator = 1,
    Influencer = 2,
    Celebrity = 3,
    Official = 4,
    News = 5
}

/// <summary>
/// Core account entity with stable identity
/// </summary>
public class Account
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable internal identifier - never changes even if username changes
    /// </summary>
    public Guid AccountId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Unique username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Normalized username for case-insensitive lookups
    /// </summary>
    public string UsernameNormalized { get; set; } = string.Empty;
    
    /// <summary>
    /// Securely hashed password
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Account email (optional)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Account type
    /// </summary>
    public AccountType AccountType { get; set; } = AccountType.OrdinaryUser;
    
    /// <summary>
    /// Account status
    /// </summary>
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    
    /// <summary>
    /// When the account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the account was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Profile? Profile { get; set; }
    public ICollection<AccountHistory> History { get; set; } = new List<AccountHistory>();
}
