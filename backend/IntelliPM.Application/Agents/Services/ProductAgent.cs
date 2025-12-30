using IntelliPM.Application.Common.Interfaces;
using System.Text.Json;

namespace IntelliPM.Application.Agents.Services;

public record ProductAgentOutput(List<PrioritizedItem> PrioritizedItems, decimal Confidence, string Rationale);
public record PrioritizedItem(int ItemId, string Title, int Priority, string Rationale);

public class ProductAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;

    public ProductAgent(ILlmClient llmClient, IVectorStore vectorStore)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
    }

    public async Task<ProductAgentOutput> RunAsync(int projectId, List<string> backlogItems, List<string> recentCompletions, CancellationToken ct = default)
    {
        // Retrieve RAG context
        var context = await _vectorStore.RetrieveContextAsync(projectId, "product", 5, ct);

        var prompt = $@"You are a product strategist. Given the following:
BACKLOG ITEMS:
{string.Join("\n", backlogItems)}

RECENT COMPLETIONS:
{string.Join("\n", recentCompletions)}

PROJECT CONTEXT:
{context}

Suggest a prioritized ranking of the top 5 items, ranked by ROI and risk. Include rationale for each.
Format as JSON: {{ ""items"": [{{ ""itemId"": 1, ""title"": ""..."", ""priority"": 90, ""rationale"": ""..."" }}], ""confidence"": 0.85, ""summary"": ""..."" }}
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        // Parse JSON output (simplified - in production, use proper JSON parsing)
        try
        {
            var json = JsonSerializer.Deserialize<dynamic>(output);
            return new ProductAgentOutput(new(), 0.85m, output);
        }
        catch
        {
            return new ProductAgentOutput(new(), 0.7m, output);
        }
    }
}

