using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Generates display names and bios based on account type
/// </summary>
public class ProfileGenerator
{
    private readonly Random _random;
    
    private static readonly string[] OrdinaryUserFirstNames = new[]
    {
        "Alex", "Jordan", "Sam", "Taylor", "Morgan", "Casey", "Riley", "Avery",
        "Quinn", "Blake", "Drew", "Sage", "River", "Phoenix", "Emery", "Harper",
        "Skyler", "Jamie", "Reese", "Parker", "Logan", "Cameron", "Hayden", "Peyton",
        "Charlie", "Finley", "Rowan", "Ellis", "Gray", "Jesse", "Kai", "Lane"
    };
    
    private static readonly string[] OrdinaryUserBios = new[]
    {
        "Just here to share my thoughts and connect with amazing people!",
        "Living life one day at a time. Coffee enthusiast. 🌟",
        "Exploring the world and sharing my journey along the way.",
        "Good vibes only ✨",
        "Adventure seeker | Nature lover | Coffee addict",
        "Making memories and learning every day",
        "Living my best life!",
        "Creative soul with a passion for authentic connections",
        "Dreamer. Believer. Do-er.",
        "Simple pleasures make me happy",
        "Always curious, always learning",
        "Finding joy in the little things"
    };
    
    private static readonly string[] CreatorBios = new[]
    {
        "🎨 Creating content that inspires | 🎮 Gaming | 🎵 Music",
        "Digital creator | Building in public | Tech enthusiast",
        "Content creator & storyteller | DM for collabs",
        "🎬 Videos every week | Subscribe for adventure!",
        "Digital artist | Design lover | Let's create together ✨",
        "Making content on the internet | Creative Director",
        "Streaming games and making people smile 🎮",
        "Building my brand one post at a time | Hustle culture",
        "Creator economy enthusiast | Newsletter coming soon",
        " storyteller |  photographer | visual artist"
    };
    
    private static readonly string[] InfluencerBios = new[]
    {
        "✨ Lifestyle & Wellness Coach | Helping you become your best self",
        "Travel photographer | 20+ countries | DM for collabs ✈️",
        "Fashion & Beauty | Your daily dose of style inspo 💫",
        "Wellness advocate | Yoga teacher | Mindfulness coach",
        "Lifestyle content | Home decor | DIY projects",
        "Your daily dose of motivation and self-care 💕",
        "Beauty guru | Skincare enthusiast | Product reviews",
        "Fitness & health | Transform your lifestyle with me 💪",
        "Fashion blogger | Style tips | Outfit inspo 👗",
        "Living my best life and sharing it with you ✨"
    };
    
    private static readonly string[] CelebrityBios = new[]
    {
        "Award-winning performer | Actor | Entertainer",
        "Musician | Songwriter | Tour dates in bio 🎵",
        "Professional athlete | Champion | Ambassador",
        "TV Personality | Actor | Producer",
        "International recording artist | Grammy winner 🎤",
        "Film & Television | Professional actor",
        "Author | Speaker | Thought leader",
        "Director | Producer | Storyteller",
        "Entertainment | Music | Films",
        "Professional performer | Live events | Tours worldwide"
    };
    
    private static readonly string[] OfficialBios = new[]
    {
        "Official account | City of Springfield | Serving our community",
        "City Government | Emergency alerts | Community updates",
        "Official Municipal Account | Public Services | Contact us",
        "City of [Name] | News | Events | Community",
        "Official City Account | Public Information | Transparency",
        "Municipal Government | Civic Engagement | Community Services",
        "City Hall | Official Updates | Contact: cityinfo@example.gov",
        "Public Services | Infrastructure | Community Development",
        "Official City Account | Open Government | Public Records",
        "Municipal Services | 311 | Report issues | Get help"
    };
    
    private static readonly string[] NewsBios = new[]
    {
        "Breaking news | In-depth reporting | Facts first 📰",
        "Independent journalism | Covering what matters",
        "24/7 News | Local & World | Your source for truth",
        "Investigative journalism | Holding power accountable",
        "Breaking News Alert | Subscribe for updates",
        "Trusted news source | Politics | Business | Culture",
        "Your daily news digest | Headlines that matter",
        "Award-winning journalism | Public interest reporting",
        "News & Analysis | What you need to know today",
        "Community journalism | Local news that matters"
    };
    
    private static readonly string[] AvatarPlaceholders = new[]
    {
        "https://api.dicebear.com/7.x/avataaars/png?seed={seed}",
        "https://api.dicebear.com/7.x/bottts/png?seed={seed}",
        "https://api.dicebear.com/7.x/thumbs/png?seed={seed}",
        "https://api.dicebear.com/7.x/identicon/png?seed={seed}"
    };
    
    public ProfileGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }
    
    /// <summary>
    /// Generate a display name based on account type
    /// </summary>
    public string GenerateDisplayName(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Celebrity => GenerateCelebrityDisplayName(),
            AccountType.News => GenerateNewsDisplayName(),
            AccountType.Official => GenerateOfficialDisplayName(),
            _ => GenerateStandardDisplayName()
        };
    }
    
    /// <summary>
    /// Generate a bio based on account type
    /// </summary>
    public string GenerateBio(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.OrdinaryUser => GetRandomItem(OrdinaryUserBios),
            AccountType.Creator => GetRandomItem(CreatorBios),
            AccountType.Influencer => GetRandomItem(InfluencerBios),
            AccountType.Celebrity => GetRandomItem(CelebrityBios),
            AccountType.Official => GetRandomItem(OfficialBios),
            AccountType.News => GetRandomItem(NewsBios),
            _ => GetRandomItem(OrdinaryUserBios)
        };
    }
    
    /// <summary>
    /// Generate an avatar URL based on username seed
    /// </summary>
    public string GenerateAvatarUrl(string username)
    {
        var template = GetRandomItem(AvatarPlaceholders);
        return template.Replace("{seed}", Uri.EscapeDataString(username));
    }
    
    private string GenerateStandardDisplayName()
    {
        var firstName = GetRandomItem(OrdinaryUserFirstNames);
        var suffix = _random.NextDouble() < 0.5 ? "" : $" {GetRandomItem(new[] { "Official", "Daily", "Channel" })}";
        return $"{firstName}{suffix}";
    }
    
    private string GenerateCelebrityDisplayName()
    {
        var firstName = GetRandomItem(OrdinaryUserFirstNames);
        var lastName = GetRandomItem(new[] { "Official", "TV", "Music", "Entertainment" });
        return $"{firstName} {lastName}";
    }
    
    private string GenerateNewsDisplayName()
    {
        var prefixes = new[] { "Daily", "City", "Global", "Local", "Metro", "Weekly" };
        var suffixes = new[] { "News", "Times", "Herald", "Tribune", "Gazette", "Post" };
        return $"{GetRandomItem(prefixes)} {GetRandomItem(suffixes)}";
    }
    
    private string GenerateOfficialDisplayName()
    {
        var prefixes = new[] { "City of", "Office of", "Department of", "Municipal", "County" };
        var names = new[] { "Springfield", "Riverside", "Oak Valley", "Metro City", "Central", "United" };
        return $"{GetRandomItem(prefixes)} {GetRandomItem(names)}";
    }
    
    private T GetRandomItem<T>(T[] array)
    {
        return array[_random.Next(array.Length)];
    }
}
