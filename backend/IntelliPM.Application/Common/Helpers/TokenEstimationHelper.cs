namespace IntelliPM.Application.Common.Helpers;

/// <summary>
/// Helper class to estimate token usage for AI operations.
/// Uses approximation: ~4 characters per token for English text.
/// </summary>
public static class TokenEstimationHelper
{
    /// <summary>
    /// Estimates token count based on text length.
    /// Uses approximation: ~4 characters per token for English text.
    /// This is a fallback when actual token counts are not available.
    /// </summary>
    public static int EstimateTokens(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        // Rough estimation: ~4 characters per token for English
        // This is approximate and may vary by language and content
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    /// <summary>
    /// Estimates token usage for prompt and completion text.
    /// Returns (promptTokens, completionTokens, totalTokens).
    /// </summary>
    public static (int promptTokens, int completionTokens, int totalTokens) EstimateTokenUsage(string? promptText, string? completionText)
    {
        var promptTokens = EstimateTokens(promptText);
        var completionTokens = EstimateTokens(completionText);
        return (promptTokens, completionTokens, promptTokens + completionTokens);
    }
}

