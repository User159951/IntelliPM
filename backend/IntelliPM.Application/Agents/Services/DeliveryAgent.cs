using IntelliPM.Application.Common.Interfaces;

namespace IntelliPM.Application.Agents.Services;

public record DeliveryAgentOutput(string RiskAssessment, List<string> RecommendedActions, decimal Confidence);

public class DeliveryAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;

    public DeliveryAgent(ILlmClient llmClient, IVectorStore vectorStore)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
    }

    public async Task<DeliveryAgentOutput> RunAsync(int projectId, string sprintProgress, List<decimal> velocityTrend, List<string> activeRisks, CancellationToken ct = default)
    {
        var context = await _vectorStore.RetrieveContextAsync(projectId, "delivery", 5, ct);

        var prompt = $@"You are a delivery manager. Assess the delivery risk based on:
SPRINT PROGRESS:
{sprintProgress}

VELOCITY TREND (last 5 sprints):
{string.Join(", ", velocityTrend)}

ACTIVE RISKS:
{string.Join("\n", activeRisks)}

PROJECT CONTEXT:
{context}

Provide risk assessment and 3-5 actionable recommendations.
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        return new DeliveryAgentOutput(output, new() { "Action 1", "Action 2", "Action 3" }, 0.82m);
    }
}

