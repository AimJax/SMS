using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

/// <summary>
/// Content generator that uses AI when configured and falls back to templates otherwise.
/// This is the main content generator used by NpcBehaviorService.
/// </summary>
public interface IAiContentGeneratorService : IContentGeneratorService
{
    /// <summary>
    /// Whether AI generation is currently enabled and configured.
    /// </summary>
    bool IsAiEnabled { get; }
}

public class AiContentGeneratorService : IAiContentGeneratorService
{
    private readonly IAiProviderService _aiProviderService;
    private readonly ContentGeneratorService _templateGenerator;
    private readonly AiPromptBuilder _promptBuilder;
    private readonly ILogger<AiContentGeneratorService> _logger;
    private readonly ISimulationStateService? _simulationState;

    public AiContentGeneratorService(
        IAiProviderService aiProviderService,
        ContentGeneratorService templateGenerator,
        AiPromptBuilder promptBuilder,
        ILogger<AiContentGeneratorService> logger,
        ISimulationStateService? simulationState = null)
    {
        _aiProviderService = aiProviderService;
        _templateGenerator = templateGenerator;
        _promptBuilder = promptBuilder;
        _logger = logger;
        _simulationState = simulationState;
    }

    public bool IsAiEnabled => _aiProviderService.IsEnabled;

    public string GeneratePostContent(NpcProfile npc, Random random)
    {
        // If AI is not enabled, use templates directly
        if (!_aiProviderService.IsEnabled)
        {
            return _templateGenerator.GeneratePostContent(npc, random);
        }

        return GeneratePostWithAiFallbackAsync(npc, random).GetAwaiter().GetResult();
    }

    public string GenerateCommentContent(NpcProfile npc, Post targetPost, Random random)
    {
        // If AI is not enabled, use templates directly
        if (!_aiProviderService.IsEnabled)
        {
            return _templateGenerator.GenerateCommentContent(npc, targetPost, random);
        }

        return GenerateCommentWithAiFallbackAsync(npc, targetPost, random).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Async version for post generation with AI + fallback.
    /// </summary>
    public async Task<string> GeneratePostWithAiFallbackAsync(NpcProfile npc, Random random)
    {
        if (!_aiProviderService.IsEnabled)
        {
            return _templateGenerator.GeneratePostContent(npc, random);
        }

        try
        {
            var request = _promptBuilder.BuildPostPrompt(npc, random);
            
            // Add a timeout for AI generation (10 seconds for content generation)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            var service = _aiProviderService.GetTextGenerationService();
            var result = await service.GenerateAsync(request, cts.Token);

            // Record metrics
            _simulationState?.RecordAiGenerationAttempt(result.Success, result.ErrorMessage);

            if (result.Success && !string.IsNullOrWhiteSpace(result.Text))
            {
                _logger.LogDebug("AI generated post for NPC {NpcId}: {Preview}", 
                    npc.AccountId, result.Text.TruncateSafe(50));
                return result.Text;
            }

            _logger.LogInformation("AI generation failed for post (NPC {NpcId}): {Error}. Using template fallback.",
                npc.AccountId, result.ErrorMessage ?? "Unknown error");
        }
        catch (OperationCanceledException)
        {
            _simulationState?.RecordAiGenerationAttempt(false, "Request timed out");
            _logger.LogInformation("AI generation timed out for post (NPC {NpcId}). Using template fallback.", npc.AccountId);
        }
        catch (Exception ex)
        {
            _simulationState?.RecordAiGenerationAttempt(false, ex.Message);
            _logger.LogWarning(ex, "AI generation error for post (NPC {NpcId}). Using template fallback.", npc.AccountId);
        }

        // Fallback to template generation
        return _templateGenerator.GeneratePostContent(npc, random);
    }

    /// <summary>
    /// Async version for comment generation with AI + fallback.
    /// </summary>
    public async Task<string> GenerateCommentWithAiFallbackAsync(NpcProfile npc, Post targetPost, Random random)
    {
        if (!_aiProviderService.IsEnabled)
        {
            return _templateGenerator.GenerateCommentContent(npc, targetPost, random);
        }

        try
        {
            var request = _promptBuilder.BuildCommentPrompt(npc, targetPost, random);
            
            // Add a timeout for AI generation (10 seconds for content generation)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            var service = _aiProviderService.GetTextGenerationService();
            var result = await service.GenerateAsync(request, cts.Token);

            // Record metrics
            _simulationState?.RecordAiGenerationAttempt(result.Success, result.ErrorMessage);

            if (result.Success && !string.IsNullOrWhiteSpace(result.Text))
            {
                _logger.LogDebug("AI generated comment for NPC {NpcId} on post {PostId}: {Preview}",
                    npc.AccountId, targetPost.Id, result.Text.TruncateSafe(50));
                return result.Text;
            }

            _logger.LogInformation("AI generation failed for comment (NPC {NpcId}): {Error}. Using template fallback.",
                npc.AccountId, result.ErrorMessage ?? "Unknown error");
        }
        catch (OperationCanceledException)
        {
            _simulationState?.RecordAiGenerationAttempt(false, "Request timed out");
            _logger.LogInformation("AI generation timed out for comment (NPC {NpcId}). Using template fallback.", npc.AccountId);
        }
        catch (Exception ex)
        {
            _simulationState?.RecordAiGenerationAttempt(false, ex.Message);
            _logger.LogWarning(ex, "AI generation error for comment (NPC {NpcId}). Using template fallback.", npc.AccountId);
        }

        // Fallback to template generation
        return _templateGenerator.GenerateCommentContent(npc, targetPost, random);
    }
}

/// <summary>
/// Extension method for safe string truncation.
/// </summary>
internal static class StringTruncateExtensions
{
    public static string TruncateSafe(this string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }
}
