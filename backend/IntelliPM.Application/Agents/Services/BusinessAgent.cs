using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Common.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Agents.Services;

public record BusinessAgentOutput(
    string ValueDeliverySummary,
    Dictionary<string, decimal> ValueMetrics,
    List<string> BusinessHighlights,
    List<string> StrategicRecommendations,
    decimal Confidence
);

public class BusinessAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;
    private readonly IAgentOutputParser _parser;
    private readonly ILogger<BusinessAgent> _logger;

    public BusinessAgent(
        ILlmClient llmClient, 
        IVectorStore vectorStore,
        IAgentOutputParser parser,
        ILogger<BusinessAgent> logger)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
        _parser = parser;
        _logger = logger;
    }

    public async Task<BusinessAgentOutput> RunAsync(
        int projectId,
        Dictionary<string, object> kpis,
        List<string> completedFeatures,
        Dictionary<string, decimal> businessMetrics,
        CancellationToken ct = default)
    {
        var context = await _vectorStore.RetrieveContextAsync(projectId, "business", 5, ct);

        var kpisJson = JsonSerializer.Serialize(kpis);
        var metricsJson = JsonSerializer.Serialize(businessMetrics);

        var prompt = $@"You are a business value analyst. Assess the business value delivered by this software project.

KEY PERFORMANCE INDICATORS:
{kpisJson}

COMPLETED FEATURES:
{string.Join("\n", completedFeatures ?? new List<string>())}

BUSINESS METRICS:
{metricsJson}

PROJECT CONTEXT:
{context}

Provide:
1. Business value summary (ROI, customer impact, market advantage)
2. Value delivery assessment
3. Top 3 business highlights this period
4. Strategic recommendations for maximizing business value
5. Alignment with business objectives

Focus on translating technical metrics into business outcomes.

IMPORTANT: Return ONLY valid JSON. Do not include any text before or after the JSON. Use this exact format:

{{
  ""valueDeliverySummary"": ""Business value summary text"",
  ""valueMetrics"": {{
    ""EstimatedROI"": 145.5,
    ""TimeToMarket"": 85.0,
    ""CustomerSatisfaction"": 4.2
  }},
  ""businessHighlights"": [
    ""Highlight 1"",
    ""Highlight 2"",
    ""Highlight 3""
  ],
  ""strategicRecommendations"": [
    ""Recommendation 1"",
    ""Recommendation 2"",
    ""Recommendation 3""
  ],
  ""confidence"": 0.87
}}

Return only the JSON object, no markdown formatting, no explanation text.
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        // Parse JSON output with validation
        if (_parser.TryParse<BusinessAgentOutputDto>(output, out var result, out var errors))
        {
            _logger.LogInformation("Successfully parsed BusinessAgent output with confidence {Confidence}", result!.Confidence);
            return result.ToBusinessAgentOutput();
        }

        // Fallback on parsing failure
        _logger.LogWarning("Failed to parse BusinessAgent output. Errors: {Errors}. Raw output: {Output}", 
            string.Join("; ", errors ?? new List<string>()), output);
        
        return new BusinessAgentOutput(
            ValueDeliverySummary: "Failed to parse agent output. Original response: " + output,
            ValueMetrics: new Dictionary<string, decimal>(),
            BusinessHighlights: new List<string>(),
            StrategicRecommendations: new List<string>(),
            Confidence: 0.0m
        );
    }
}
