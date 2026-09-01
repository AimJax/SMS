namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Deterministic username generator for NPC population
/// </summary>
public class UsernameGenerator
{
    private readonly Random _random;
    private readonly HashSet<string> _generatedUsernames = new(StringComparer.OrdinalIgnoreCase);
    private int _counter;
    
    // Word components for username generation
    private static readonly string[] Adjectives = new[]
    {
        "pixel", "night", "digital", "cosmic", "swift", "bright", "dark", "silent",
        "wild", "urban", "blue", "green", "red", "golden", "silver", "crystal",
        "neon", "retro", "cyber", "quantum", "hyper", "mega", "super", "ultra",
        "cosmic", "stellar", "lunar", "solar", "astral", "nova", "astro", "galaxy",
        "magic", "mystic", "epic", "legend", "hero", "warrior", "shadow", "light",
        "electric", "vintage", "modern", "classic", "prime", "elite", "pro", "master"
    };
    
    private static readonly string[] Nouns = new[]
    {
        "wanderer", "explorer", "traveler", "nomad", "hiker", "rider", "driver",
        "coder", "dev", "tech", "geek", "nerd", "creator", "artist", "maker",
        "owl", "wolf", "hawk", "lion", "dragon", "phoenix", "tiger", "bear",
        "dreamer", "thinker", "writer", "reader", "viewer", "fan", "gamer", "streamer",
        "chef", "cook", "baker", "foodie", "traveler", "photographer", "artist",
        "musician", "singer", "dancer", "actor", "model", "designer", "builder",
        "gamer", "player", "champion", "master", "legend", "hero", "warrior",
        "news", "daily", "times", "chronicle", "herald", "tribune", "post", "gazette"
    };
    
    private static readonly string[] Suffixes = new[]
    {
        "92", "99", "01", "007", "24", "360", "101", "404", "777", "88",
        "x", "z", "q", "k", "y", "fy", "ly", "er", "ist", "oid",
        "hq", "lab", "hub", "zone", "net", "web", "io", "dev", "app", "co",
        "_x", "_z", "_99", "_01", "_2023", "_live", "_tv", "_hd", "_4k", "_pro"
    };
    
    private static readonly string[] NamePrefixes = new[]
    {
        "tech", "game", "city", "world", "daily", "night", "morning", "crypto",
        "pixel", "stream", "vlog", "art", "music", "food", "travel", "sport",
        "news", "gossip", "media", "social", "digital", "cloud", "data", "cyber"
    };
    
    private static readonly string[] NameSuffixes = new[]
    {
        "with", "by", "of", "from", "the", "just", "real", "official", "daily",
        "updates", "news", "insider", "reports", "central", "network", "channel"
    };
    
    private static readonly string[] FirstNames = new[]
    {
        "Alex", "Jordan", "Taylor", "Morgan", "Casey", "Riley", "Avery", "Quinn",
        "Sage", "River", "Phoenix", "Blake", "Dakota", "Emery", "Finley", "Harley",
        "Jamie", "Kendall", "Logan", "Micah", "Nico", "Parker", "Reese", "Skyler",
        "Charlie", "Drew", "Ellis", "Frankie", "Gray", "Harper", "Indigo", "Jesse",
        "Kai", "Lane", "Marley", "Nicky", "Oakley", "Peyton", "Remy", "Spencer"
    };
    
    private static readonly string[] LastNames = new[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
        "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
        "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"
    };
    
    public UsernameGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _counter = 0;
    }
    
    /// <summary>
    /// Generate a unique username
    /// </summary>
    public string Generate()
    {
        // Keep trying until we find a unique username
        for (int attempt = 0; attempt < 10000; attempt++)
        {
            _counter++;
            
            string username;
            
            // Choose a random generation strategy
            switch (_random.Next(5))
            {
                case 0:
                    username = GenerateAdjectiveNoun();
                    break;
                case 1:
                    username = GeneratePrefixNoun();
                    break;
                case 2:
                    username = GenerateNameStyle();
                    break;
                case 3:
                    username = GenerateNumberedName();
                    break;
                default:
                    username = GenerateFallbackName();
                    break;
            }
            
            if (IsUnique(username))
            {
                return username;
            }
        }
        
        // Ultimate fallback with guaranteed unique name
        return $"npc_{Guid.NewGuid():N}";
    }
    
    /// <summary>
    /// Generate multiple unique usernames
    /// </summary>
    public IEnumerable<string> GenerateBatch(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return Generate();
        }
    }
    
    /// <summary>
    /// Check if a username is in the generated set (for collision detection)
    /// </summary>
    public bool IsUnique(string username)
    {
        return !_generatedUsernames.Contains(username);
    }
    
    /// <summary>
    /// Register a username as used
    /// </summary>
    public void Register(string username)
    {
        _generatedUsernames.Add(username);
    }
    
    private string GenerateAdjectiveNoun()
    {
        var adj = Adjectives[_random.Next(Adjectives.Length)];
        var noun = Nouns[_random.Next(Nouns.Length)];
        return $"{adj}{noun}";
    }
    
    private string GeneratePrefixNoun()
    {
        var prefix = NamePrefixes[_random.Next(NamePrefixes.Length)];
        var noun = Nouns[_random.Next(Nouns.Length)];
        
        // 30% chance to add suffix
        if (_random.NextDouble() < 0.3)
        {
            var suffix = Suffixes[_random.Next(Suffixes.Length)];
            return $"{prefix}{noun}{suffix}";
        }
        
        return $"{prefix}{noun}";
    }
    
    private string GenerateNameStyle()
    {
        var firstName = FirstNames[_random.Next(FirstNames.Length)];
        
        // 40% chance to have two names
        if (_random.NextDouble() < 0.4)
        {
            var lastName = LastNames[_random.Next(LastNames.Length)];
            
            // 50% chance to add suffix
            if (_random.NextDouble() < 0.5)
            {
                var suffix = NameSuffixes[_random.Next(NameSuffixes.Length)];
                return $"{firstName}{suffix}{lastName}";
            }
            
            return $"{firstName}{lastName}";
        }
        
        // 30% chance to add number
        if (_random.NextDouble() < 0.3)
        {
            var num = _random.Next(1, 999);
            return $"{firstName}{num}";
        }
        
        return firstName.ToLowerInvariant();
    }
    
    private string GenerateNumberedName()
    {
        var noun = Nouns[_random.Next(Nouns.Length)];
        var num = _random.Next(10, 999);
        return $"{noun}{num}";
    }
    
    private string GenerateFallbackName()
    {
        return $"user{_counter}_{_random.Next(1000, 9999)}";
    }
}
