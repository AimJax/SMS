namespace SocialMediaSimulator.Client.Models;

/// <summary>
/// News article model for API responses
/// </summary>
public class NewsArticle
{
    public int Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid NewsAccountId { get; set; }
    public string? NewsName { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Category { get; set; }
    public int Views { get; set; }
    public int Shares { get; set; }
    public bool IsBreakingNews { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
