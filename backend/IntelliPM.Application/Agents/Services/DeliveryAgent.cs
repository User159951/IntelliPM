using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Agents.Services;

public record DeliveryAgentOutput(string RiskAssessment, List<string> RecommendedActions, decimal Confidence);

public class DeliveryAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;
    private readonly IAgentOutputParser _parser;
    private readonly ILogger<DeliveryAgent> _logger;

    public DeliveryAgent(
        ILlmClient llmClient, 
        IVectorStore vectorStore,
        IAgentOutputParser parser,
        ILogger<DeliveryAgent> logger)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
        _parser = parser;
        _logger = logger;
    }

    public async Task<DeliveryAgentOutput> RunAsync(int projectId, string sprintProgress, List<decimal> velocityTrend, List<string> activeRisks, CancellationToken ct = default)
    {
        var context = await _vectorStore.RetrieveContextAsync(projectId, "delivery", 5, ct);

        var prompt = $@"You are a delivery manager. Assess the delivery risk based on:
SPRINT PROGRESS:
{sprintProgress}

VELOCITY TREND (last 5 sprints):
{string.Join(", ", velocityTrend ?? new List<decimal>())}

ACTIVE RISKS:
{string.Join("\n", activeRisks ?? new List<string>())}

PROJECT CONTEXT:
{context}

Provide risk assessment and 3-5 actionable recommendations.

IMPORTANT: Return ONLY valid JSON. Do not include any text before or after the JSON. Use this exact format:

{{
  ""riskAssessment"": ""Detailed risk assessment text"",
  ""recommendedActions"": [
    ""Action 1"",
    ""Action 2"",
    ""Action 3""
  ],
  ""highlights"": [
    ""Highlight 1"",
    ""Highlight 2""
  ],
  ""confidence"": 0.82
}}

Return only the JSON object, no markdown formatting, no explanation text.
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        // Parse JSON output with validation
        if (_parser.TryParse<DeliveryAgentOutputDto>(output, out var result, out var errors))
        {
            _logger.LogInformation("Successfully parsed DeliveryAgent output with confidence {Confidence}", result!.Confidence);
            return result.ToDeliveryAgentOutput();
        }

        // Fallback on parsing failure
        _logger.LogWarning("Failed to parse DeliveryAgent output. Errors: {Errors}. Raw output: {Output}", 
            string.Join("; ", errors ?? new List<string>()), output);
        
        return new DeliveryAgentOutput(
            RiskAssessment: "Failed to parse agent output. Original response: " + output,
            RecommendedActions: new List<string>(),
            Confidence: 0.0m
        );
    }
}
