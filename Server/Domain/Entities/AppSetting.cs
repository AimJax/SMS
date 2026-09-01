namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// App settings entity for tracking misc settings
/// </summary>
public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTime? ValueDate { get; set; }
}
