namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Community membership roles
/// </summary>
public enum CommunityRole
{
    Member = 0,
    Moderator = 1,
    Admin = 2,
    Owner = 3
}

/// <summary>
/// Represents membership of an account in a community
/// </summary>
public class CommunityMembership
{
    public int Id { get; set; }
    public Guid MembershipId { get; set; } = Guid.NewGuid();
    public int CommunityId { get; set; }
    public int AccountId { get; set; }
    public CommunityRole Role { get; set; } = CommunityRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    public Community? Community { get; set; }
    public Account? Account { get; set; }
}
