using IntelliPM.Application.Agents.DTOs;
using IntelliPM.Application.Common.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Agents.Services;

public record QAAgentOutput(
    string DefectAnalysis,
    List<DefectPattern> Patterns,
    List<string> Recommendations,
    decimal Confidence
);

public record DefectPattern(string Pattern, int Frequency, string Severity, string Suggestion);

public class QAAgent
{
    private readonly ILlmClient _llmClient;
    private readonly IVectorStore _vectorStore;
    private readonly IAgentOutputParser _parser;
    private readonly ILogger<QAAgent> _logger;

    public QAAgent(
        ILlmClient llmClient, 
        IVectorStore vectorStore,
        IAgentOutputParser parser,
        ILogger<QAAgent> logger)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
        _parser = parser;
        _logger = logger;
    }

    public async Task<QAAgentOutput> RunAsync(
        int projectId,
        List<string> recentDefects,
        Dictionary<string, int> defectStats,
        CancellationToken ct = default)
    {
        var context = await _vectorStore.RetrieveContextAsync(projectId, "qa", 5, ct);

        var statsJson = JsonSerializer.Serialize(defectStats);

        var prompt = $@"You are a QA analyst. Analyze defect patterns and quality trends.

RECENT DEFECTS:
{string.Join("\n", recentDefects)}

DEFECT STATISTICS:
{statsJson}

PROJECT CONTEXT:
{context}

Provide:
1. Overall quality assessment
2. Defect patterns (if any recurring issues)
3. Root cause analysis suggestions
4. 3-5 actionable quality improvement recommendations

Focus on preventing defects rather than just fixing them.

IMPORTANT: Return ONLY valid JSON. Do not include any text before or after the JSON. Use this exact format:

{{
  ""defectAnalysis"": ""Overall quality assessment text"",
  ""patterns"": [
    {{
      ""pattern"": ""Pattern name"",
      ""frequency"": 3,
      ""severity"": ""High"",
      ""suggestion"": ""Suggestion text""
    }}
  ],
  ""recommendations"": [
    ""Recommendation 1"",
    ""Recommendation 2"",
    ""Recommendation 3""
  ],
  ""confidence"": 0.84
}}

Severity must be one of: Critical, High, Medium, Low
Return only the JSON object, no markdown formatting, no explanation text.
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        // Parse JSON output with validation
        if (_parser.TryParse<QAAgentOutputDto>(output, out var result, out var errors))
        {
            _logger.LogInformation("Successfully parsed QAAgent output with confidence {Confidence}", result!.Confidence);
            return result.ToQAAgentOutput();
        }

        // Fallback on parsing failure
        _logger.LogWarning("Failed to parse QAAgent output. Errors: {Errors}. Raw output: {Output}", 
            string.Join("; ", errors), output);
        
        return new QAAgentOutput(
            DefectAnalysis: "Failed to parse agent output. Original response: " + output,
            Patterns: new List<DefectPattern>(),
            Recommendations: new List<string>(),
            Confidence: 0.0m
        );
    }
}
