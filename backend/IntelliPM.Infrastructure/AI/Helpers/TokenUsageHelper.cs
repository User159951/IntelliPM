using System.Reflection;

namespace IntelliPM.Infrastructure.AI.Helpers;

/// <summary>
/// Helper class to extract token usage information from Semantic Kernel responses
/// </summary>
public static class TokenUsageHelper
{
    /// <summary>
    /// Extracts token usage from Semantic Kernel ChatMessageContent metadata.
    /// Returns (promptTokens, completionTokens, totalTokens).
    /// If token usage is not available, returns (0, 0, 0).
    /// </summary>
    public static (int promptTokens, int completionTokens, int totalTokens) ExtractTokenUsage(object? response)
    {
        if (response == null)
        {
            return (0, 0, 0);
        }

        // Use reflection to access Metadata property
        var responseType = response.GetType();
        var metadataProp = responseType.GetProperty("Metadata");
        
        if (metadataProp == null)
        {
            return (0, 0, 0);
        }

        var metadata = metadataProp.GetValue(response);
        if (metadata == null)
        {
            return (0, 0, 0);
        }

        // Try to get token usage from metadata dictionary
        var metadataType = metadata.GetType();
        var tryGetValueMethod = metadataType.GetMethod("TryGetValue", new[] { typeof(string), typeof(object).MakeByRefType() });
        
        if (tryGetValueMethod != null)
        {
            // Try common metadata keys for token usage
            object? usageValue = null;
            var parameters = new object?[] { "Usage", usageValue };
            if ((bool)(tryGetValueMethod.Invoke(metadata, parameters) ?? false))
            {
                var usageObj = parameters[1];
                if (usageObj != null)
                {
                    var usageType = usageObj.GetType();
                    var promptProp = usageType.GetProperty("PromptTokens");
                    var completionProp = usageType.GetProperty("CompletionTokens");
                    var totalProp = usageType.GetProperty("TotalTokens");
                    
                    if (promptProp != null && completionProp != null && totalProp != null)
                    {
                        var promptTokens = Convert.ToInt32(promptProp.GetValue(usageObj) ?? 0);
                        var completionTokens = Convert.ToInt32(completionProp.GetValue(usageObj) ?? 0);
                        var totalTokens = Convert.ToInt32(totalProp.GetValue(usageObj) ?? 0);
                        return (promptTokens, completionTokens, totalTokens);
                    }
                }
            }

            // Fallback: try to get from individual metadata keys
            object? promptValue = null;
            parameters = new object?[] { "PromptTokens", promptValue };
            var hasPrompt = (bool)(tryGetValueMethod.Invoke(metadata, parameters) ?? false);
            var promptObj = parameters[1];
            
            object? completionValue = null;
            parameters = new object?[] { "CompletionTokens", completionValue };
            var hasCompletion = (bool)(tryGetValueMethod.Invoke(metadata, parameters) ?? false);
            var completionObj = parameters[1];
            
            if (hasPrompt && hasCompletion)
            {
                var promptTokens = Convert.ToInt32(promptObj ?? 0);
                var completionTokens = Convert.ToInt32(completionObj ?? 0);
                return (promptTokens, completionTokens, promptTokens + completionTokens);
            }
        }

        return (0, 0, 0);
    }

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

