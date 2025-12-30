using IntelliPM.Application.Common.Interfaces;
using System.Text.Json;

namespace IntelliPM.Application.Agents.Services;

public record ManagerAgentOutput(string ExecutiveSummary, List<string> KeyDecisionsNeeded, List<string> Highlights, decimal Confidence);

public class ManagerAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;

    public ManagerAgent(ILlmClient llmClient, IVectorStore vectorStore)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
    }

    public async Task<ManagerAgentOutput> RunAsync(int projectId, Dictionary<string, object> kpis, string changes, string highlights, CancellationToken ct = default)
    {
        var context = await _vectorStore.RetrieveContextAsync(projectId, "manager", 5, ct);

        var kpisJson = JsonSerializer.Serialize(kpis);

        var prompt = $@"You are an executive summary generator. Generate a brief weekly executive summary based on:
KPIs:
{kpisJson}

CHANGES THIS WEEK:
{changes}

HIGHLIGHTS:
{highlights}

PROJECT CONTEXT:
{context}

Generate a 3-4 paragraph executive summary with 2-3 key decisions needed.
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        return new ManagerAgentOutput(output, new() { "Decision 1", "Decision 2" }, new() { highlights }, 0.88m);
    }
}

