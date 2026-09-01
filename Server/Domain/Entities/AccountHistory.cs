namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Account history event types
/// </summary>
public enum AccountHistoryEventType
{
    Created = 0,
    UsernameChanged = 1,
    DisplayNameChanged = 2,
    EmailChanged = 3,
    PasswordChanged = 4,
    ProfileUpdated = 5,
    StatusChanged = 6,
    TypeChanged = 7
}

/// <summary>
/// Permanent account history record
/// </summary>
public class AccountHistory
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to Account
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// Type of history event
    /// </summary>
    public AccountHistoryEventType EventType { get; set; }
    
    /// <summary>
    /// Additional details about the event (JSON or text)
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Account? Account { get; set; }
}
