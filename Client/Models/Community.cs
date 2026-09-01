namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// Community model for API responses
/// </summary>
public class Community
{
    public int Id { get; set; }
    public Guid CommunityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public int ActiveMemberCount { get; set; }
    public bool IsJoined { get; set; }
    public List<string> Topics { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
