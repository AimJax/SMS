using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Interface for content relevance scoring
/// </summary>
public interface IContentRelevanceService
{
    /// <summary>
    /// Calculate relevance between an NPC's interests and a post
    /// </summary>
    double CalculatePostRelevance(Post post, IEnumerable<NpcInterest> interests);
    
    /// <summary>
    /// Calculate relevance between an NPC's interests and an account
    /// </summary>
    double CalculateAccountRelevance(Account account, IEnumerable<NpcInterest> interests);
    
    /// <summary>
    /// Determine topic category from content
    /// </summary>
    string[] ExtractTopics(string content);
}

/// <summary>
/// Deterministic keyword-based content relevance service.
/// Replaceable with ML-based classifier in future parts.
/// </summary>
public class ContentRelevanceService : IContentRelevanceService
{
    // Keyword mappings for interest categories
    private static readonly Dictionary<string, string[]> CategoryKeywords = new()
    {
        [InterestCategories.Gaming] = new[] { "game", "gaming", "play", "player", "steam", "xbox", "playstation", "nintendo", "esports", "twitch", "fps", "rpg", "moba", "indie", "gamer", "gaming" },
        [InterestCategories.Politics] = new[] { "politics", "government", "election", "vote", "congress", "senate", "parliament", "policy", "law", "democrat", "republican", "liberal", "conservative" },
        [InterestCategories.Sports] = new[] { "sport", "football", "basketball", "soccer", "baseball", "hockey", "tennis", "golf", "nfl", "nba", "mlb", "uefa", "fifa", "olympics", "team", "score", "win", "lose" },
        [InterestCategories.Technology] = new[] { "tech", "software", "programming", "code", "developer", "ai", "machine learning", "startup", "app", "computer", "internet", "web", "data", "cloud" },
        [InterestCategories.Music] = new[] { "music", "song", "album", "artist", "band", "concert", "spotify", "playlist", "rap", "rock", "pop", "jazz", "classical", "singer" },
        [InterestCategories.Movies] = new[] { "movie", "film", "cinema", "hollywood", "actor", "actress", "director", "netflix", "marvel", "dc", "disney", "box office", "premiere" },
        [InterestCategories.Television] = new[] { "tv", "show", "series", "episode", "hbo", "prime", "streaming", "drama", "comedy", "reality", "channel" },
        [InterestCategories.Fashion] = new[] { "fashion", "style", "clothing", "dress", "outfit", "designer", "runway", "trend", "wear", "brand", "luxury" },
        [InterestCategories.Food] = new[] { "food", "recipe", "cooking", "chef", "restaurant", "meal", "eat", "delicious", "tasty", "cuisine", "foodie" },
        [InterestCategories.Travel] = new[] { "travel", "trip", "vacation", "flight", "hotel", "destination", "tourism", "explore", "adventure", "beach", "mountain" },
        [InterestCategories.Science] = new[] { "science", "research", "study", "experiment", "physics", "chemistry", "biology", "space", "nasa", "discovery", "scientist" },
        [InterestCategories.Health] = new[] { "health", "fitness", "workout", "exercise", "diet", "wellness", "mental health", "medical", "doctor", "yoga", "gym" },
        [InterestCategories.Business] = new[] { "business", "company", "startup", "entrepreneur", "ceo", "corporate", "industry", "market", "enterprise", "founder" },
        [InterestCategories.Finance] = new[] { "finance", "money", "investment", "stock", "market", "crypto", "bitcoin", "trading", "bank", "economy", "financial" },
        [InterestCategories.Education] = new[] { "education", "school", "university", "college", "learning", "student", "teacher", "course", "degree", "study", "class" },
        [InterestCategories.LocalNews] = new[] { "local", "city", "town", "community", "neighborhood", "council", "police", "fire", "traffic" },
        [InterestCategories.WorldNews] = new[] { "world", "international", "global", "news", "breaking", "headline", "reporter", "journalist", "media" },
        [InterestCategories.Entertainment] = new[] { "entertainment", "celebrity", "fame", "star", "famous", "viral", "trending", "fandom", "fan" },
        [InterestCategories.GamingNews] = new[] { "gaming news", "game update", "patch", "dlc", "release date", "announcement", "reveal", "trailer" },
        [InterestCategories.SportsNews] = new[] { "sports news", "trade", "draft", "championship", "playoffs", "roster", "injury", "coach", "manager" },
        [InterestCategories.TechNews] = new[] { "tech news", "gadget", "device", "launch", "product", "apple", "google", "microsoft", "amazon", "review" }
    };

    /// <inheritdoc />
    public double CalculatePostRelevance(Post post, IEnumerable<NpcInterest> interests)
    {
        if (post == null || post.Content == null)
            return 0.0;

        var content = post.Content.ToLowerInvariant();
        var topics = ExtractTopics(content);
        
        if (topics.Length == 0)
            return 0.1; // Small default relevance for posts without detected topics

        var interestList = interests.ToList();
        if (interestList.Count == 0)
            return 0.1; // Small default for NPCs with no interests

        double totalRelevance = 0.0;
        int matchCount = 0;

        foreach (var topic in topics)
        {
            foreach (var interest in interestList)
            {
                if (CategoryKeywords.TryGetValue(interest.InterestKey, out var keywords))
                {
                    foreach (var keyword in keywords)
                    {
                        if (content.Contains(keyword.ToLowerInvariant()))
                        {
                            // Weight by interest strength
                            totalRelevance += interest.Strength * 0.2;
                            matchCount++;
                            break;
                        }
                    }
                }
            }
        }

        // Normalize to 0.0 - 1.0 range
        if (topics.Length > 0)
        {
            return Math.Min(1.0, totalRelevance / Math.Max(1, topics.Length));
        }

        return 0.1;
    }

    /// <inheritdoc />
    public double CalculateAccountRelevance(Account account, IEnumerable<NpcInterest> interests)
    {
        var interestList = interests.ToList();
        if (interestList.Count == 0)
            return 0.1;

        // Account type compatibility scoring
        double typeBonus = 0.0;
        
        // Check if any interests align with account type
        var primaryInterests = interestList
            .OrderByDescending(i => i.Strength)
            .Take(3)
            .Select(i => i.InterestKey)
            .ToHashSet();

        // Accounts with same-type posts get boosted
        typeBonus = primaryInterests.Count * 0.1;

        return Math.Min(1.0, 0.1 + typeBonus);
    }

    /// <inheritdoc />
    public string[] ExtractTopics(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Array.Empty<string>();

        var normalized = content.ToLowerInvariant();
        var matchedTopics = new HashSet<string>();

        foreach (var (category, keywords) in CategoryKeywords)
        {
            foreach (var keyword in keywords)
            {
                if (normalized.Contains(keyword.ToLowerInvariant()))
                {
                    matchedTopics.Add(category);
                    break;
                }
            }
        }

        return matchedTopics.ToArray();
    }
}
