namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Temporary entity for persistence foundation verification.
/// This is NOT part of the final game schema.
/// </summary>
public class PersistenceTest
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
