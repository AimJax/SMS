using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Service for seeding initial community data
/// </summary>
public interface ICommunitySeedService
{
    /// <summary>
    /// Seed initial communities if none exist
    /// </summary>
    Task<CommunitySeedResult> SeedCommunitiesAsync(int count = 50);
    
    /// <summary>
    /// Check if communities have been seeded
    /// </summary>
    Task<bool> CommunitiesExistAsync();
}

public class CommunitySeedResult
{
    public bool Success { get; set; }
    public int CommunitiesCreated { get; set; }
    public string? ErrorMessage { get; set; }
    
    public static CommunitySeedResult SuccessResult(int count) => new()
    {
        Success = true,
        CommunitiesCreated = count
    };
    
    public static CommunitySeedResult FailureResult(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}

/// <summary>
/// Seeds initial community data
/// </summary>
public class CommunitySeedService : ICommunitySeedService
{
    private readonly AppDbContext _context;
    
    // Topic categories with sample communities
    private static readonly Dictionary<string, List<(string Name, string Description, string Tags)>> TopicTemplates = new()
    {
        ["gaming"] = new()
        {
            ("Retro Gaming Hub", "Celebrating classic video games from the 80s, 90s, and early 2000s", "retro,nostalgia,arcade,nintendo,sega"),
            ("Indie Game Dev", "For independent game developers and players who love indie games", "indie,gamedev,unity,unreal,steam"),
            ("PC Master Race", "Desktop gaming enthusiasts sharing builds and benchmarks", "pcgaming,tech,builds,benchmarks,mods"),
            ("Esports Arena", "Competitive gaming news, tournaments, and team discussions", "esports,tournaments,teams,competitions,live"),
            ("Mobile Gaming", "Mobile and tablet gaming community for casual and competitive players", "mobile,gaming,android,ios,casual"),
            ("Board Game Geeks", "Tabletop gaming enthusiasts who love classic and modern board games", "boardgames,tabletop,strategy,party,classic"),
            ("RPG Legends", "Role-playing games from D&D to video game RPGs", "rpg,dnd,fantasy,adventure,story"),
            ("Speedrun Central", "For speedrunners and those who love watching record attempts", "speedrun,records,attempts,challenges,loopholes")
        },
        ["technology"] = new()
        {
            ("Tech Enthusiasts", "All things tech - gadgets, software, hardware, and the future", "tech,gadgets,software,hardware,future"),
            ("AI & Machine Learning", "Discussing artificial intelligence, ML models, and automation", "ai,ml,automation,llm,gpt,neural"),
            ("Programming Hub", "Developers sharing code, tips, and career advice", "programming,code,dev,careers,tutorials"),
            ("Linux Community", "Linux distributions, desktop customization, and open source", "linux,opensource,kernel,distros,desktop"),
            ("Apple Ecosystem", "Apple products, ecosystem integration, and productivity", "apple,macos,ios,ecosystem,productivity"),
            ("Cybersecurity Alerts", "Security news, vulnerability alerts, and best practices", "security,cybersecurity,vulnerabilities,privacy,alerts"),
            ("Cloud Computing", "AWS, Azure, GCP and all things cloud infrastructure", "cloud,aws,azure,gcp,infrastructure,devops"),
            ("Startup Founders", "Entrepreneurs building the next big thing", "startup,entrepreneurs,founder,vc,funding")
        },
        ["anime"] = new()
        {
            ("Anime Central", "General anime discussion - shows, manga, and recommendations", "anime,manga,shows,recommendations,discussion"),
            ("Studio Ghibli Fan Club", "Devoted to Studio Ghibli films and Miyazaki's artistry", "ghibli,miyazaki,animation,fantasy,nostalgia"),
            ("Manga Readers", "For manga enthusiasts - new releases and classic series", "manga,releases,series,chapter,scanlation"),
            ("Anime Art & Cosplay", "Fan art, cosplay photos, and creative expressions", "cosplay,art,fandoms,creative,fanart"),
            ("Seasonal Anime Club", "Discussing currently airing seasonal anime", "seasonalanime,cour,spring,fall,winter,summer"),
            ("Mecha Universe", "For fans of mecha anime and robot-themed series", "mecha,robots,gundam,evangelion,scifi"),
            ("Isekai Adventures", "Other world anime and portal fantasy discussions", "isekai,fantasy,adventure,transmigration,game"),
            ("Anime OST & Music", "Soundtracks, openings, endings, and music discussions", "anime,music,ost,openings,endings,soundtracks")
        },
        ["music"] = new()
        {
            ("Music Production", "For producers, DJs, and music creators sharing tips and tracks", "music,production,dj,beats,electronic"),
            ("Indie Music Discovery", "Finding and sharing independent artists and albums", "indie,music,discovery,albums,artists"),
            ("Hip Hop Culture", "Hip hop music, culture, and community", "hiphop,rap,beats,freestyle,cyphers"),
            ("Rock Legends", "Classic and modern rock music discussions", "rock,guitar,metal,band,sound"),
            ("Electronic Beats", "EDM, house, techno, and electronic music scene", "electronic,edm,house,techno,beats"),
            ("K-Pop Universe", "Korean pop music, groups, and global fan community", "kpop,korea,groups,bts,blackpink,comebacks"),
            ("Vinyl Collectors", "For vinyl enthusiasts and record collectors", "vinyl,records,collectors,audio,audiophile"),
            ("Music Festivals", "Festival announcements, experiences, and lineups", "festivals,coachella,glastonbury,lineups,live")
        },
        ["sports"] = new()
        {
            ("Football Frenzy", "American football - NFL, college, and fantasy football", "football,nfl,college,fantasy,touchdown"),
            ("Soccer World", "Global football/soccer discussions", "soccer,football,premierleague,laliga,worldcup"),
            ("Basketball Court", "NBA and basketball discussions", "basketball,nba,players,teams,dunks"),
            ("Baseball Nation", "MLB and baseball fans", "baseball,mlb,stats,pitching,hitting"),
            ("Extreme Sports", "Skateboarding, BMX, snowboarding, and action sports", "extreme,skateboarding,bmx,snowboarding,adrenaline"),
            ("Fitness & Training", "Workout tips, training programs, and fitness goals", "fitness,workout,training,gym,health"),
            ("Esports Sports", "Competitive video gaming tournaments and teams", "esports,gaming,tournaments,competitive,teams"),
            ("MMA & Boxing", "Combat sports - UFC, boxing, and martial arts", "mma,ufc,boxing,combat,fights")
        },
        ["photography"] = new()
        {
            ("Photography 101", "Learn and improve photography skills", "photography,tips,beginners,composition,lighting"),
            ("Street Photography", "Urban photography and candid shots", "street,urban,candid,city,documentary"),
            ("Portrait Photography", "Portrait photography techniques and inspiration", "portrait,models,lighting,studio,expression"),
            ("Nature & Wildlife", "Nature photography and wildlife shots", "nature,wildlife,animals,birds,landscape"),
            ("Gear Talk", "Camera equipment, lenses, and accessories discussion", "gear,cameras,lenses,equipment,reviews"),
            ("Photo Editing", "Post-processing, Lightroom, and Photoshop tips", "editing,lightroom,photoshop,colorgrading,retouching"),
            ("Astro Photography", "Astrophotography and night sky imaging", "astro,night,stars,astrophotography,galaxies"),
            ("Mobile Photography", "Smartphone photography tips and showcases", "mobile,smartphone,iphone,android,casual")
        },
        ["memes"] = new()
        {
            ("Meme Factory", "Fresh memes and meme culture discussions", "memes,fresh,dank,trending,viral"),
            ("Wholesome Memes", "Positive and uplifting meme content", "wholesome,positive,feelgood,cute,friendly"),
            ("Vintage Memes", "Classic memes from the golden age of internet humor", "vintage,classic,throwback,goldenage,history"),
            ("Meme Reviews", "Hot takes and reviews on trending memes", "reviews,trending,analysis,ratings,ranking"),
            ("OC Memers", "Original meme creators sharing their work", "oc,original,creator,artwork,homemade"),
            ("Meme Templates", "Meme templates and how to use them", "templates,format,caption,editing,captioned"),
            ("Anime Memes", "Anime-related memes and shitposts", "anime,weeb,memes,shitpost,fandom"),
            ("Gaming Memes", "Gaming humor and video game memes", "gaming,memes,video,twitch,steam")
        },
        ["fashion"] = new()
        {
            ("Streetwear Culture", "Urban fashion, sneakers, and street style", "streetwear,urban,sneakers,fashion,style"),
            ("Vintage Fashion", "Thrifted finds and vintage clothing discussions", "vintage,thrift,retro,classic,clothing"),
            ("Sustainable Fashion", "Eco-friendly and sustainable clothing choices", "sustainable,eco,fashion,green,ethical"),
            ("Fashion Photography", "Fashion photography and editorial content", "fashion,fotografia,editorial,models,style"),
            ("Skincare & Beauty", "Skincare routines, beauty tips, and product reviews", "skincare,beauty,routines,products,reviews"),
            ("Haute Couture", "High fashion, runway shows, and designer discussions", "couture,designer,runway,luxury,haute"),
            ("Men's Fashion", "Style tips and fashion discussions for men", "mens,style,fashion,outfits,accessories"),
            ("DIY Fashion", "Do-it-yourself fashion and clothing modifications", "diy,crafts,custom,homemade,modifications")
        },
        ["art"] = new()
        {
            ("Digital Art Studio", "Digital artists sharing techniques and portfolios", "digitalart,illustration,drawing,graphics,tablet"),
            ("Traditional Art", "Painting, sketching, and traditional art forms", "painting,sketching,traditional,oils,watercolor"),
            ("3D Modeling", "3D art, modeling, and rendering discussions", "3d,modeling,blender,rendering,sculpting"),
            ("Art Commissions", "Commission boards and artist connections", "commissions,artists,requests,boards,hiring"),
            ("Art Galleries", "Share and appreciate art from the community", "galleries,exhibits,showcase,appreciation,collection"),
            ("Art Supplies", "Discussion about art materials and tools", "supplies,canvases,brushes,paints,tools"),
            ("Concept Art", "Concept art for games, films, and creative projects", "concept,game,film,design,visualdevelopment"),
            ("Art Challenges", "Community art challenges and collaborations", "challenges,collabs,themes,community,events")
        },
        ["celebrity"] = new()
        {
            ("Hollywood Buzz", "Movies, TV shows, and Hollywood celebrity news", "hollywood,movies,tv,celebrity,gossip"),
            ("Music Stars", "Musicians, singers, and music industry news", "musicians,singers,music,industry,albums"),
            ("Influencer Life", "Influencer culture, drama, and lifestyle", "influencer,lifestyle,socialmedia,brand,collab"),
            ("Reality TV Stars", "Reality TV shows and contestant discussions", "realitytv,shows,contestants,drama,tv"),
            ("Sports Celebrities", "Athletes as celebrities and sports entertainment", "athletes,sports,celebrity,endorsements,teams"),
            ("YouTube Creators", "YouTube personalities and content creators", "youtube,creators,content,streamers,channels"),
            ("TikTok Stars", "TikTok influencers and viral content creators", "tiktok,viral,influencer,trends,content"),
            ("Award Season", "Oscars, Grammys, Emmys and award show discussions", "awards,oscars,grammys,emmys,season")
        },
        ["science"] = new()
        {
            ("Space Exploration", "NASA, SpaceX, astronomy, and space science", "space,nasa,spacex,astronomy,rockets"),
            ("Climate Science", "Climate change research and environmental discussions", "climate,environment,research,globalwarming"),
            ("Physics Mysteries", "Physics breakthroughs and scientific discoveries", "physics,quantum,relativity,discoveries,mysteries"),
            ("Biology & Nature", "Biology, genetics, and natural world discussions", "biology,genetics,nature,evolution,species"),
            ("Science Education", "Explaining science concepts for everyone", "education,science,learning,explained,beginners"),
            ("Medical Research", "Medical breakthroughs and health science", "medical,research,health,breakthroughs,trials"),
            ("Psychology Today", "Psychology, mental health, and human behavior", "psychology,mentalhealth,behavior,cognition,therapy"),
            ("Paleontology", "Dinosaurs, fossils, and ancient life", "dinosaurs,fossils,paleontology,ancient,prehistoric")
        },
        ["food"] = new()
        {
            ("Cooking Tips", "Cooking techniques and kitchen advice", "cooking,tips,techniques,kitchen,advice"),
            ("Recipe Exchange", "Share and discover recipes from around the world", "recipes,cooking,food,dishes,cuisine"),
            ("Food Photography", "Beautiful food photos and plating inspiration", "food,photography,plating,presentation,instagram"),
            ("Vegetarian Kitchen", "Plant-based cooking and vegetarian recipes", "vegetarian,plantbased,vegan,recipes,cooking"),
            ("Baking Paradise", "Breads, pastries, cakes, and all things baked", "baking,bread,pastries,cakes,oven"),
            ("Restaurant Reviews", "Share and read restaurant experiences", "restaurants,reviews,dining,experiences,recommendations"),
            ("Food Science", "The science behind cooking and food", "foodscience,cooking,chemistry,techniques,science"),
            ("Street Food World", "Street food culture and vendor discoveries", "streetfood,vendors,culture,local,discoveries")
        }
    };

    public CommunitySeedService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CommunitiesExistAsync()
    {
        return await _context.Communities.AnyAsync();
    }

    public async Task<CommunitySeedResult> SeedCommunitiesAsync(int count = 50)
    {
        try
        {
            // Check if communities already exist
            if (await CommunitiesExistAsync())
            {
                return CommunitySeedResult.FailureResult("Communities already exist");
            }

            var communities = new List<Community>();
            var random = new Random(42); // Fixed seed for reproducibility
            var topics = TopicTemplates.Keys.ToList();
            var created = 0;

            for (int i = 0; i < count && i < GetTotalTemplateCount(); i++)
            {
                // Select random topic
                var topic = topics[random.Next(topics.Count)];
                var templates = TopicTemplates[topic];
                var template = templates[random.Next(templates.Count)];

                // Generate unique slug
                var baseSlug = Slugify(template.Name);
                var slug = baseSlug;
                var suffix = 1;
                while (communities.Any(c => c.Slug == slug))
                {
                    slug = $"{baseSlug}-{suffix}";
                    suffix++;
                }

                // Create community
                var community = new Community
                {
                    Name = template.Name,
                    Slug = slug,
                    Description = template.Description,
                    Topic = topic,
                    Tags = template.Tags,
                    Visibility = CommunityVisibility.Public,
                    IsActive = true,
                    MemberCount = random.Next(10, 1000), // Random initial members for realism
                    PostCount = random.Next(5, 500), // Random initial posts
                    OwnerAccountId = 1, // Will be updated if system account exists
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                    UpdatedAt = DateTime.UtcNow
                };

                communities.Add(community);
                created++;
            }

            // Try to find a system/bot account to own communities
            var systemAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == "sms_bot" || a.Username == "system");
            
            if (systemAccount != null)
            {
                // Assign communities to system account
                foreach (var community in communities)
                {
                    community.OwnerAccountId = systemAccount.Id;
                }
            }

            _context.Communities.AddRange(communities);
            await _context.SaveChangesAsync();

            return CommunitySeedResult.SuccessResult(created);
        }
        catch (Exception ex)
        {
            return CommunitySeedResult.FailureResult($"Failed to seed communities: {ex.Message}");
        }
    }

    private static int GetTotalTemplateCount()
    {
        return TopicTemplates.Values.Sum(t => t.Count);
    }

    private static string Slugify(string text)
    {
        // Convert to lowercase
        var slug = text.ToLowerInvariant();
        
        // Replace spaces and special characters with hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9]+", "-");
        
        // Remove leading/trailing hyphens
        slug = slug.Trim('-');
        
        // Limit length
        if (slug.Length > 50)
        {
            slug = slug.Substring(0, 50);
        }
        
        return slug;
    }
}
