using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for calculating AI costs based on model and token usage.
/// Supports per-model pricing configuration with separate prompt and completion rates.
/// </summary>
public interface IAIPricingService
{
    /// <summary>
    /// Calculates the cost for a given model and token usage.
    /// </summary>
    /// <param name="modelName">Name of the AI model (e.g., "llama3.2:3b", "gpt-4")</param>
    /// <param name="promptTokens">Number of prompt tokens used</param>
    /// <param name="completionTokens">Number of completion tokens used</param>
    /// <returns>Total cost in USD</returns>
    decimal CalculateCost(string modelName, int promptTokens, int completionTokens);

    /// <summary>
    /// Gets the pricing configuration for a specific model.
    /// </summary>
    /// <param name="modelName">Name of the AI model</param>
    /// <returns>Model pricing configuration or default if not found</returns>
    ModelPricing GetModelPricing(string modelName);
}

/// <summary>
/// Pricing configuration for an AI model.
/// </summary>
public class ModelPricing
{
    public string Provider { get; set; } = "Local";
    public decimal PromptCostPer1000Tokens { get; set; } = 0m;
    public decimal CompletionCostPer1000Tokens { get; set; } = 0m;
}

/// <summary>
/// Service implementation for calculating AI costs.
/// </summary>
public class AIPricingService : IAIPricingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIPricingService> _logger;
    private readonly Dictionary<string, ModelPricing> _modelPricingCache;

    public AIPricingService(IConfiguration configuration, ILogger<AIPricingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _modelPricingCache = new Dictionary<string, ModelPricing>();
        LoadPricingConfiguration();
    }

    private void LoadPricingConfiguration()
    {
        var aiPricingSection = _configuration.GetSection("AIPricing");
        var modelsSection = aiPricingSection.GetSection("Models");

        foreach (var modelSection in modelsSection.GetChildren())
        {
            var modelName = modelSection.Key;
            var pricing = new ModelPricing
            {
                Provider = modelSection["Provider"] ?? "Local",
                PromptCostPer1000Tokens = decimal.Parse(modelSection["PromptCostPer1000Tokens"] ?? "0"),
                CompletionCostPer1000Tokens = decimal.Parse(modelSection["CompletionCostPer1000Tokens"] ?? "0")
            };

            _modelPricingCache[modelName] = pricing;
            _logger.LogDebug("Loaded pricing for model {ModelName}: Prompt=${PromptCost}/1k, Completion=${CompletionCost}/1k",
                modelName, pricing.PromptCostPer1000Tokens, pricing.CompletionCostPer1000Tokens);
        }

        _logger.LogInformation("Loaded pricing configuration for {ModelCount} models", _modelPricingCache.Count);
    }

    public decimal CalculateCost(string modelName, int promptTokens, int completionTokens)
    {
        var pricing = GetModelPricing(modelName);

        // Calculate cost: (promptTokens * promptRate + completionTokens * completionRate) / 1000
        var promptCost = (promptTokens * pricing.PromptCostPer1000Tokens) / 1000m;
        var completionCost = (completionTokens * pricing.CompletionCostPer1000Tokens) / 1000m;
        var totalCost = promptCost + completionCost;

        _logger.LogDebug(
            "Calculated cost for model {ModelName}: {PromptTokens} prompt tokens (${PromptCost}) + {CompletionTokens} completion tokens (${CompletionCost}) = ${TotalCost}",
            modelName, promptTokens, promptCost, completionTokens, completionCost, totalCost);

        return totalCost;
    }

    public ModelPricing GetModelPricing(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            modelName = _configuration["AIPricing:DefaultModel"] ?? "llama3.2:3b";
        }

        if (_modelPricingCache.TryGetValue(modelName, out var pricing))
        {
            return pricing;
        }

        // Return default pricing if model not found
        var defaultPricing = new ModelPricing
        {
            Provider = _configuration["AIPricing:DefaultModel"]?.StartsWith("llama") == true ? "Local" : "Cloud",
            PromptCostPer1000Tokens = decimal.Parse(_configuration["AIPricing:DefaultPromptCostPer1000Tokens"] ?? "0"),
            CompletionCostPer1000Tokens = decimal.Parse(_configuration["AIPricing:DefaultCompletionCostPer1000Tokens"] ?? "0")
        };

        _logger.LogWarning("Model {ModelName} not found in pricing configuration, using default pricing", modelName);
        return defaultPricing;
    }
}

