namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Trend model for API responses
/// </summary>
public class Trend
{
    public int Id { get; set; }
    public Guid TrendId { get; set; }
    public string Query { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PostCount { get; set; }
    public int Velocity { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
