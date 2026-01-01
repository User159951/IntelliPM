using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Agents.Services;

public record ProductAgentOutput(List<PrioritizedItem> PrioritizedItems, decimal Confidence, string Rationale);
public record PrioritizedItem(int ItemId, string Title, int Priority, string Rationale);

public class ProductAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;
    private readonly IAgentOutputParser _parser;
    private readonly ILogger<ProductAgent> _logger;

    public ProductAgent(
        ILlmClient llmClient, 
        IVectorStore vectorStore,
        IAgentOutputParser parser,
        ILogger<ProductAgent> logger)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
        _parser = parser;
        _logger = logger;
    }

    public async Task<ProductAgentOutput> RunAsync(int projectId, List<string> backlogItems, List<string> recentCompletions, CancellationToken ct = default)
    {
        // Retrieve RAG context
        var context = await _vectorStore.RetrieveContextAsync(projectId, "product", 5, ct);

        var prompt = $@"You are a product strategist. Given the following:
BACKLOG ITEMS:
{string.Join("\n", backlogItems ?? new List<string>())}

RECENT COMPLETIONS:
{string.Join("\n", recentCompletions ?? new List<string>())}

PROJECT CONTEXT:
{context}

Suggest a prioritized ranking of the top 5 items, ranked by ROI and risk. Include rationale for each.

IMPORTANT: Return ONLY valid JSON. Do not include any text before or after the JSON. Use this exact format:

{{
  ""items"": [
    {{
      ""itemId"": 1,
      ""title"": ""Item title"",
      ""priority"": 90,
      ""rationale"": ""Explanation for this priority""
    }}
  ],
  ""confidence"": 0.85,
  ""summary"": ""Overall summary of prioritization""
}}

Return only the JSON object, no markdown formatting, no explanation text.
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        // Parse JSON output with validation
        if (_parser.TryParse<ProductAgentOutputDto>(output, out var result, out var errors))
        {
            _logger.LogInformation("Successfully parsed ProductAgent output with confidence {Confidence}", result!.Confidence);
            return result.ToProductAgentOutput();
        }

        // Fallback on parsing failure
        _logger.LogWarning("Failed to parse ProductAgent output. Errors: {Errors}. Raw output: {Output}", 
            string.Join("; ", errors ?? new List<string>()), output);
        
        return new ProductAgentOutput(
            PrioritizedItems: new List<PrioritizedItem>(),
            Confidence: 0.0m,
            Rationale: "Failed to parse agent output. Original response: " + output
        );
    }
}
