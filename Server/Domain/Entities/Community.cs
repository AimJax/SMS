namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Community visibility levels
/// </summary>
public enum CommunityVisibility
{
    Public = 0,
    Private = 1,
    Hidden = 2
}

/// <summary>
/// Represents a community - a social group centered around shared interests or topics
/// </summary>
public class Community
{
    public int Id { get; set; }
    public Guid CommunityId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerAccountId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public CommunityVisibility Visibility { get; set; } = CommunityVisibility.Public;
    public bool IsActive { get; set; } = true;
    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public Account? OwnerAccount { get; set; }
    public ICollection<CommunityMembership> Memberships { get; set; } = new List<CommunityMembership>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
