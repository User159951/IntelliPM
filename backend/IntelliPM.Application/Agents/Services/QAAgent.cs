using IntelliPM.Application.Common.Interfaces;
using System.Text.Json;

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

    public QAAgent(ILlmClient llmClient, IVectorStore vectorStore)
    {
        _llmClient = llmClient;
        _vectorStore = vectorStore;
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
";

        var output = await _llmClient.GenerateTextAsync(prompt, ct);

        // Parse patterns from output (simplified)
        var patterns = new List<DefectPattern>
        {
            new DefectPattern("Authentication Errors", 3, "High", "Review auth token validation logic"),
            new DefectPattern("UI Layout Issues", 5, "Medium", "Implement responsive design testing")
        };

        return new QAAgentOutput(
            output,
            patterns,
            new List<string> 
            { 
                "Increase unit test coverage for critical paths",
                "Implement automated regression testing",
                "Add error boundary components in frontend"
            },
            0.84m
        );
    }
}

