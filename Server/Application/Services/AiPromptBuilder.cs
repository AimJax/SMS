using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Builds prompts for AI text generation based on NPC context.
/// Reuses and extends the context assembly from Part 10's ContentGeneratorService.
/// </summary>
public interface IAiPromptBuilder
{
    /// <summary>
    /// Build a prompt for generating a social media post.
    /// </summary>
    AiGenerationRequest BuildPostPrompt(NpcProfile npc, Random random);
    
    /// <summary>
    /// Build a prompt for generating a comment on a post.
    /// </summary>
    AiGenerationRequest BuildCommentPrompt(NpcProfile npc, Post targetPost, Random random);
}

public class AiPromptBuilder : IAiPromptBuilder
{
    // System prompts for different account types
    private static readonly Dictionary<AccountType, string> AccountTypeSystemPrompts = new()
    {
        [AccountType.OrdinaryUser] = "You are a regular social media user. Your posts are personal, relatable, and casual. You share everyday thoughts, experiences, and moments. Keep your writing natural and conversational.",
        
        [AccountType.Creator] = "You are a content creator on social media. Your posts are engaging, share creative work, and encourage interaction. You often discuss your creative process, ask for feedback, and thank your audience. Keep posts compelling and community-focused.",
        
        [AccountType.Influencer] = "You are a social media influencer. Your posts promote lifestyle, products, or trends. You engage with followers, celebrate milestones, and share aspirational content. Keep posts upbeat, motivational, and engagement-driving.",
        
        [AccountType.Celebrity] = "You are a celebrity or public figure. Your posts express gratitude to fans, announce projects, share life updates, and maintain public image. Keep posts gracious, measured, and professional.",
        
        [AccountType.Official] = "You represent an official organization or entity. Your posts are professional, informative, and authoritative. You share official statements, updates, and announcements. Keep posts clear, factual, and appropriately formal.",
        
        [AccountType.News] = "You represent a news organization or journalist. Your posts report facts, provide context, and share breaking news. You remain neutral and informative. Keep posts clear, accurate, and newsworthy."
    };

    /// <inheritdoc />
    public AiGenerationRequest BuildPostPrompt(NpcProfile npc, Random random)
    {
        var accountType = npc.Account?.AccountType ?? AccountType.OrdinaryUser;
        
        // Get base system prompt for account type
        var systemPrompt = AccountTypeSystemPrompts.GetValueOrDefault(accountType, AccountTypeSystemPrompts[AccountType.OrdinaryUser]);
        
        // Add personality context
        systemPrompt = AddPersonalityContext(systemPrompt, npc.Personality);
        
        // Add interests context
        systemPrompt = AddInterestsContext(systemPrompt, npc.Interests.ToList());
        
        // Build user prompt
        var userPrompt = BuildPostUserPrompt(accountType, npc.Personality, npc.Interests.ToList(), random);
        
        return new AiGenerationRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            MaxTokens = 200,
            Temperature = 0.8,
            RequestId = $"post_{npc.AccountId}_{DateTime.UtcNow:yyyyMMddHHmmss}"
        };
    }

    /// <inheritdoc />
    public AiGenerationRequest BuildCommentPrompt(NpcProfile npc, Post targetPost, Random random)
    {
        var accountType = npc.Account?.AccountType ?? AccountType.OrdinaryUser;
        
        // Get base system prompt for account type
        var systemPrompt = AccountTypeSystemPrompts.GetValueOrDefault(accountType, AccountTypeSystemPrompts[AccountType.OrdinaryUser]);
        
        // Add personality context
        systemPrompt = AddPersonalityContext(systemPrompt, npc.Personality);
        
        // Add comment-specific guidance
        systemPrompt += "\nYou are writing a comment, not a post. Keep it brief (1-2 sentences) and conversational. Comments should feel spontaneous and genuine.";
        
        // Build user prompt
        var userPrompt = BuildCommentUserPrompt(accountType, npc.Personality, targetPost, random);
        
        return new AiGenerationRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            MaxTokens = 100,
            Temperature = 0.7,
            RequestId = $"comment_{npc.AccountId}_{targetPost.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}"
        };
    }

    private string AddPersonalityContext(string systemPrompt, NpcPersonality? personality)
    {
        if (personality == null)
            return systemPrompt;

        var additions = new List<string>();
        
        // Extraversion
        if (personality.Extraversion > 0.7)
            additions.Add("You are very outgoing and enthusiastic.");
        else if (personality.Extraversion > 0.5)
            additions.Add("You are moderately social and engaged.");
        else if (personality.Extraversion < 0.3)
            additions.Add("You are more reserved and reflective in your expressions.");

        // Openness
        if (personality.Openness > 0.7)
            additions.Add("You are curious, creative, and open to new ideas.");
        else if (personality.Openness > 0.5)
            additions.Add("You appreciate both familiar and novel things.");
        
        // Agreeableness
        if (personality.Agreeableness > 0.7)
            additions.Add("You are warm, supportive, and considerate in your expressions.");
        else if (personality.Agreeableness < 0.3)
            additions.Add("You are direct and sometimes skeptical in your views.");
        
        // Neuroticism
        if (personality.Neuroticism > 0.6)
            additions.Add("You sometimes express concerns or reflect on challenges.");
        else if (personality.Neuroticism < 0.3)
            additions.Add("You tend to be emotionally stable and optimistic.");
        
        // Conscientiousness
        if (personality.Conscientiousness > 0.7)
            additions.Add("You are thoughtful and organized in your thoughts.");
        else if (personality.Conscientiousness < 0.3)
            additions.Add("You are spontaneous and go-with-the-flow.");

        if (additions.Count == 0)
            return systemPrompt;

        return systemPrompt + "\n\nPersonality: " + string.Join(" ", additions);
    }

    private string AddInterestsContext(string systemPrompt, List<NpcInterest> interests)
    {
        if (interests == null || interests.Count == 0)
            return systemPrompt;

        var topInterests = interests
            .OrderByDescending(i => i.Strength)
            .Take(3)
            .Select(i => i.InterestKey)
            .ToList();

        if (topInterests.Count == 0)
            return systemPrompt;

        return systemPrompt + $"\n\nInterests: {string.Join(", ", topInterests)}";
    }

    private string BuildPostUserPrompt(AccountType accountType, NpcPersonality? personality, List<NpcInterest> interests, Random random)
    {
        var prompt = accountType switch
        {
            AccountType.OrdinaryUser => "Write a short, authentic social media post (1-2 sentences). It should feel personal and genuine, like something a regular person would share with friends. Do not include hashtags or emojis unless they feel completely natural.",

            AccountType.Creator => "Write an engaging social media post for your creative content (1-2 sentences). It should promote interaction, share your creative work or process, and feel authentic to your audience.",

            AccountType.Influencer => "Write an engaging social media post (1-2 sentences). It should be motivational, share lifestyle content, or celebrate a milestone. Include a subtle call-to-action if appropriate.",

            AccountType.Celebrity => "Write a gracious and professional social media post (1-2 sentences). Express gratitude, share an update, or announce something. Maintain your public image while feeling genuine.",

            AccountType.Official => "Write a clear, professional social media post (1-2 sentences) for your organization. Share information, make an announcement, or engage with stakeholders.",

            AccountType.News => "Write a concise, informative social media post (1-2 sentences) reporting news. Be factual, neutral, and newsworthy. If appropriate, hint at more information to come.",

            _ => "Write a short social media post (1-2 sentences). Make it engaging and appropriate for the platform."
        };

        // Add interest-specific flavor for certain types
        if ((accountType == AccountType.OrdinaryUser || accountType == AccountType.Creator) && interests.Count > 0)
        {
            var topInterest = interests.OrderByDescending(i => i.Strength).First().InterestKey;
            prompt += $" Consider the topic of {topInterest} if it feels natural.";
        }

        return prompt;
    }

    private string BuildCommentUserPrompt(AccountType accountType, NpcPersonality? personality, Post targetPost, Random random)
    {
        var targetContent = targetPost.Content?.Length > 200 
            ? targetPost.Content[..200] + "..." 
            : targetPost.Content ?? "[post content]";

        var prompt = "You are commenting on this post:\n\n";
        prompt += $"\"{targetContent}\"\n\n";
        prompt += "Write a brief, natural comment (1-2 sentences) responding to this post. ";

        // Adjust based on personality
        if (personality?.Agreeableness > 0.6)
        {
            prompt += "Be supportive and positive.";
        }
        else if (personality?.Agreeableness < 0.4)
        {
            prompt += "Be honest, perhaps with a critical eye.";
        }
        else
        {
            prompt += "Be genuine and conversational.";
        }

        prompt += " Do not be overly enthusiastic or use excessive exclamation marks.";

        return prompt;
    }
}
