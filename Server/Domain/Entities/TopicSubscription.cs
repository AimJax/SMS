namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// Topic subscription entity
/// </summary>
public class TopicSubscription
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Guid TopicId { get; set; }
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
}
