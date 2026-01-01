using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Common.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Agents.Services;

public record ManagerAgentOutput(string ExecutiveSummary, List<string> KeyDecisionsNeeded, List<string> Highlights, decimal Confidence);

public class ManagerAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;
    private readonly IAgentOutputParser _parser;
    private readonly ILogger<ManagerAgent> _logger;

    public ManagerAgent(
        ILlmClient llmClient, 
        IVectorStore vectorStore,
        IAgentOutputParser parser,
        ILogger<ManagerAgent> logger)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
        _parser = parser;
        _logger = logger;
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

IMPORTANT: Return ONLY valid JSON. Do not include any text before or after the JSON. Use this exact format:

{{
  ""executiveSummary"": ""Executive summary text (3-4 paragraphs)"",
  ""keyDecisions"": [
    ""Decision 1"",
    ""Decision 2""
  ],
  ""highlights"": [
    ""Highlight 1"",
    ""Highlight 2""
  ],
  ""confidence"": 0.88
}}

Return only the JSON object, no markdown formatting, no explanation text.
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        // Parse JSON output with validation
        if (_parser.TryParse<ManagerAgentOutputDto>(output, out var result, out var errors))
        {
            _logger.LogInformation("Successfully parsed ManagerAgent output with confidence {Confidence}", result!.Confidence);
            return result.ToManagerAgentOutput();
        }

        // Fallback on parsing failure
        _logger.LogWarning("Failed to parse ManagerAgent output. Errors: {Errors}. Raw output: {Output}", 
            string.Join("; ", errors ?? new List<string>()), output);
        
        return new ManagerAgentOutput(
            ExecutiveSummary: "Failed to parse agent output. Original response: " + output,
            KeyDecisionsNeeded: new List<string>(),
            Highlights: new List<string> { highlights },
            Confidence: 0.0m
        );
    }
}
