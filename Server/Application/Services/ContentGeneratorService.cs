using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Interface for generating content (posts/comments)
/// This can be replaced with LLM integration in future parts
/// </summary>
public interface IContentGeneratorService
{
    /// <summary>
    /// Generate a post for an NPC
    /// </summary>
    string GeneratePostContent(NpcProfile npc, Random random);
    
    /// <summary>
    /// Generate a comment for an NPC
    /// </summary>
    string GenerateCommentContent(NpcProfile npc, Post targetPost, Random random);
}

/// <summary>
/// Deterministic template-based content generator.
/// This is a placeholder that can be replaced with LLM generation in future parts.
/// </summary>
public class ContentGeneratorService : IContentGeneratorService
{
    // Post templates by account type
    private static readonly Dictionary<AccountType, string[]> PostTemplates = new()
    {
        [AccountType.OrdinaryUser] = new[]
        {
            "Just had an amazing day!",
            "Thinking about life today.",
            "What's everyone up to this weekend?",
            "Finally finished that thing I've been working on!",
            "Sometimes you just need to take a break.",
            "Looking forward to the week ahead.",
            "Quick update: everything is going well!",
            "Anyone have recommendations for something new to try?",
            "Grateful for the little things.",
            "That feeling when everything clicks into place."
        },
        [AccountType.Creator] = new[]
        {
            "New content coming soon! Stay tuned.",
            "Here's what I've been working on lately...",
            "Thanks for all the support!",
            "Behind the scenes of my latest project.",
            "Drop your favorite things in the comments!",
            "Let me know what you think of this!",
            "Big announcement coming tomorrow...",
            "The process is just as important as the result.",
            "Feedback always welcome!",
            "Work in progress - what should I focus on?"
        },
        [AccountType.Influencer] = new[]
        {
            "Check out my latest post! Link in bio.",
            "Thank you for 10K followers! Couldn't do it without you.",
            "Today's look is all about comfort meets style.",
            "New collab coming soon!",
            "Living my best life today.",
            "Who's ready for the weekend?",
            "Tag someone who needs to see this!",
            "Double tap if you agree!",
            "S/O to everyone who supports this journey.",
            "Dream big, work hard."
        },
        [AccountType.Celebrity] = new[]
        {
            "Grateful for all the love and support.",
            "Thank you to my fans - you make everything possible.",
            "Announcing my new project!",
            "Honored to be recognized for this.",
            "Thank you for having me at your event!",
            "Excited to share what's coming next.",
            "The journey continues...",
            "Truly blessed to have such an amazing community.",
            "Making memories that will last a lifetime.",
            "Can't wait to see everyone soon!"
        },
        [AccountType.Official] = new[]
        {
            "Official statement regarding recent developments.",
            "Reminder: Submit your feedback by the deadline.",
            "Public comment period now open.",
            "Join us for the upcoming town hall meeting.",
            "Transparency report now available.",
            "Working to serve our community better.",
            "Important update for stakeholders.",
            "Thank you for your continued engagement.",
            "Our commitment to progress continues.",
            "For more information, visit our official channels."
        },
        [AccountType.News] = new[]
        {
            "BREAKING: Updates on developing story.",
            "Full coverage of today's events.",
            "Expert analysis: What this means for you.",
            "Live updates as the situation develops.",
            "Sources confirm new information about...",
            "In-depth report coming at 6 PM.",
            "Fact-check: Setting the record straight.",
            "Community response to recent news.",
            "What you need to know right now.",
            "Follow for continuous updates."
        }
    };

    // Comment templates
    private static readonly string[] PositiveComments = new[]
    {
        "Great post!",
        "Love this!",
        "This is amazing!",
        "So inspiring!",
        "Couldn't agree more!",
        "Perfect!",
        "This made my day!",
        "Absolutely wonderful!",
        "Keep it up!",
        "This is everything!"
    };

    private static readonly string[] OpinionComments = new[]
    {
        "Interesting perspective!",
        "I've been thinking the same thing.",
        "Not sure I agree, but respect your view.",
        "This is worth discussing.",
        "Food for thought.",
        "Thanks for sharing!",
        "I hadn't considered that before.",
        "Valid point.",
        "Good observation.",
        "Well said!"
    };

    private static readonly string[] QuestionComments = new[]
    {
        "Can you tell us more?",
        "What's your thoughts on this?",
        "Where can I learn more about this?",
        "Have you tried the alternative approach?",
        "How long have you been involved with this?",
        "What's next for this?",
        "Any recommendations?",
        "Would love to hear more details.",
        "What inspired you to do this?",
        "Do you have any tips?"
    };

    private static readonly string[] AgreementComments = new[]
    {
        "Same here!",
        "Exactly what I was thinking!",
        "Preach!",
        "Couldn't have said it better myself!",
        "This is so true!",
        "Facts!",
        "Everyone needs to hear this!",
        "Spreading the word!",
        "This deserves more attention!",
        "Finally someone said it!"
    };

    /// <inheritdoc />
    public string GeneratePostContent(NpcProfile npc, Random random)
    {
        var accountType = npc.Account?.AccountType ?? AccountType.OrdinaryUser;
        
        if (PostTemplates.TryGetValue(accountType, out var templates))
        {
            return templates[random.Next(templates.Length)];
        }
        
        return PostTemplates[AccountType.OrdinaryUser][random.Next(PostTemplates[AccountType.OrdinaryUser].Length)];
    }

    /// <inheritdoc />
    public string GenerateCommentContent(NpcProfile npc, Post targetPost, Random random)
    {
        var personality = npc.Personality;
        var commentType = SelectCommentType(personality ?? new NpcPersonality(), random);
        
        return commentType switch
        {
            CommentType.Positive => PositiveComments[random.Next(PositiveComments.Length)],
            CommentType.Opinion => OpinionComments[random.Next(OpinionComments.Length)],
            CommentType.Question => QuestionComments[random.Next(QuestionComments.Length)],
            CommentType.Agreement => AgreementComments[random.Next(AgreementComments.Length)],
            _ => PositiveComments[random.Next(PositiveComments.Length)]
        };
    }

    private CommentType SelectCommentType(NpcPersonality? personality, Random random)
    {
        // Use personality to influence comment type
        var roll = random.NextDouble();
        
        // Default to neutral distribution if no personality
        var agreeableness = personality?.Agreeableness ?? 0.5;
        
        // Agreeableness affects positivity
        if (agreeableness > 0.6)
        {
            if (roll < 0.5) return CommentType.Positive;
            if (roll < 0.7) return CommentType.Agreement;
            if (roll < 0.85) return CommentType.Opinion;
            return CommentType.Question;
        }
        
        // Neutral personality
        if (roll < 0.3) return CommentType.Positive;
        if (roll < 0.5) return CommentType.Opinion;
        if (roll < 0.75) return CommentType.Question;
        return CommentType.Agreement;
    }

    private enum CommentType
    {
        Positive,
        Opinion,
        Question,
        Agreement
    }
}
