namespace SocialMediaSimulator.Server.Domain.Entities;

/// <summary>
/// News categories for coverage areas
/// </summary>
public enum NewsCategory
{
    General = 0,
    Technology = 1,
    Sports = 2,
    Entertainment = 3,
    Politics = 4,
    Business = 5,
    Science = 6,
    Lifestyle = 7,
    Gaming = 8,
    Gossip = 9,
    Local = 10,
    Crime = 11
}

/// <summary>
/// Tone/style of news reporting
/// </summary>
public enum NewsTone
{
    Serious = 0,
    Casual = 1,
    Sensational = 2,
    Balanced = 3
}

/// <summary>
/// Article type enumeration
/// </summary>
public enum ArticleType
{
    Breaking = 0,
    Report = 1,
    Investigation = 2,
    FactCheck = 3,
    Opinion = 4,
    Update = 5,
    Roundup = 6
}

/// <summary>
/// Article status
/// </summary>
public enum ArticleStatus
{
    Draft = 0,
    Published = 1,
    Updated = 2,
    Retracted = 3,
    Archived = 4
}

/// <summary>
/// News lead type for detection
/// </summary>
public enum NewsLeadType
{
    TrendCoverage = 0,
    ViralStory = 1,
    FactCheck = 2,
    EventCoverage = 3,
    Investigation = 4
}

/// <summary>
/// Special account for reporting news
/// </summary>
public class NewsAccount
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid NewsAccountId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// The NPC account this news account belongs to
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// News outlet name
    /// </summary>
    public string NewsName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tagline for the outlet
    /// </summary>
    public string NewsTagline { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the news outlet
    /// </summary>
    public string NewsBio { get; set; } = string.Empty;
    
    /// <summary>
    /// Coverage category
    /// </summary>
    public NewsCategory Category { get; set; } = NewsCategory.General;
    
    /// <summary>
    /// Credibility score (0-100)
    /// </summary>
    public int CredibilityScore { get; set; } = 50;
    
    /// <summary>
    /// Number of subscribers
    /// </summary>
    public int SubscriberCount { get; set; }
    
    /// <summary>
    /// Total articles published
    /// </summary>
    public int ArticlesPublished { get; set; }
    
    /// <summary>
    /// Total views across all articles
    /// </summary>
    public int TotalArticleViews { get; set; }
    
    /// <summary>
    /// Accuracy rating (0.0-1.0)
    /// </summary>
    public double AccuracyRating { get; set; } = 0.5;
    
    /// <summary>
    /// Number of breaking news reported
    /// </summary>
    public int BreakingNewsCount { get; set; }
    
    /// <summary>
    /// Tone/style of reporting
    /// </summary>
    public NewsTone Tone { get; set; } = NewsTone.Balanced;
    
    /// <summary>
    /// Target articles per hour
    /// </summary>
    public int ReportFrequency { get; set; } = 2;
    
    /// <summary>
    /// Whether the outlet is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether verified by the platform
    /// </summary>
    public bool IsVerified { get; set; }
    
    /// <summary>
    /// When created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Account? Account { get; set; }
    public ICollection<NewsArticle> Articles { get; set; } = new List<NewsArticle>();
}

/// <summary>
/// A news article written by a news account
/// </summary>
public class NewsArticle
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid ArticleId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// News account that wrote this article
    /// </summary>
    public Guid NewsAccountId { get; set; }
    
    /// <summary>
    /// Article headline
    /// </summary>
    public string Headline { get; set; } = string.Empty;
    
    /// <summary>
    /// Brief summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    /// <summary>
    /// Full article body
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// Tags for the article (JSON serialized list)
    /// </summary>
    public string TagsJson { get; set; } = "[]";
    
    /// <summary>
    /// Category of the article
    /// </summary>
    public NewsCategory Category { get; set; } = NewsCategory.General;
    
    /// <summary>
    /// Type of article
    /// </summary>
    public ArticleType Type { get; set; } = ArticleType.Report;
    
    /// <summary>
    /// Topic being covered
    /// </summary>
    public Guid? CoveredTopicId { get; set; }
    
    /// <summary>
    /// Rumor being reported
    /// </summary>
    public Guid? CoveredRumorId { get; set; }
    
    /// <summary>
    /// Event being covered
    /// </summary>
    public Guid? CoveredEventId { get; set; }
    
    /// <summary>
    /// Related post being covered
    /// </summary>
    public Guid? RelatedPostId { get; set; }
    
    /// <summary>
    /// Account IDs mentioned (JSON serialized list)
    /// </summary>
    public string MentionedAccountsJson { get; set; } = "[]";
    
    /// <summary>
    /// View count
    /// </summary>
    public int Views { get; set; }
    
    /// <summary>
    /// Share count
    /// </summary>
    public int Shares { get; set; }
    
    /// <summary>
    /// Comment count
    /// </summary>
    public int Comments { get; set; }
    
    /// <summary>
    /// Bonus for breaking news
    /// </summary>
    public int BreakingNewsBonus { get; set; }
    
    /// <summary>
    /// Article status
    /// </summary>
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    
    /// <summary>
    /// Whether this is breaking news
    /// </summary>
    public bool IsBreakingNews { get; set; }
    
    /// <summary>
    /// Whether sources are verified
    /// </summary>
    public bool IsVerified { get; set; }
    
    /// <summary>
    /// When published
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// When created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public NewsAccount? NewsAccount { get; set; }
    public Topic? CoveredTopic { get; set; }
    public ICollection<NewsExposure> Exposures { get; set; } = new List<NewsExposure>();
}

/// <summary>
/// Tracks news exposure to communities
/// </summary>
public class NewsExposure
{
    public int Id { get; set; }
    
    /// <summary>
    /// Stable identifier
    /// </summary>
    public Guid ExposureId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Article that was exposed
    /// </summary>
    public Guid ArticleId { get; set; }
    
    /// <summary>
    /// Community that saw the article
    /// </summary>
    public int CommunityId { get; set; }
    
    /// <summary>
    /// Views in this community
    /// </summary>
    public int Views { get; set; }
    
    /// <summary>
    /// When exposed
    /// </summary>
    public DateTime ExposedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public NewsArticle? Article { get; set; }
    public Community? Community { get; set; }
}
