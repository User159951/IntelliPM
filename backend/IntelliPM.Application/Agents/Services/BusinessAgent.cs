using IntelliPM.Application.Common.Interfaces;
using System.Text.Json;

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

    public BusinessAgent(ILlmClient llmClient, IVectorStore vectorStore)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
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
{string.Join("\n", completedFeatures)}

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
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        var valueMetrics = new Dictionary<string, decimal>
        {
            { "EstimatedROI", 145.5m },
            { "TimeToMarket", 85.0m },
            { "CustomerSatisfaction", 4.2m }
        };

        var highlights = new List<string>
        {
            "Delivered authentication feature ahead of schedule",
            "Reduced technical debt by 20%",
            "Improved system performance by 35%"
        };

        var recommendations = new List<string>
        {
            "Focus next sprint on high-value customer-facing features",
            "Consider A/B testing for new UI components",
            "Prioritize scalability improvements for expected growth"
        };

        return new BusinessAgentOutput(
            output,
            valueMetrics,
            highlights,
            recommendations,
            0.87m
        );
    }
}

