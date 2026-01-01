namespace IntelliPM.Application.Interfaces;

/// <summary>
/// Service for logging AI decisions to AIDecisionLog for audit trail and governance.
/// </summary>
public interface IAIDecisionLogger
{
    /// <summary>
    /// Logs an AI decision to the AIDecisionLog table.
    /// </summary>
    /// <param name="agentType">Type of agent (e.g., "ProductAgent", "DeliveryAgent")</param>
    /// <param name="decisionType">Type of decision (e.g., "BacklogPrioritization", "DeliveryAnalysis")</param>
    /// <param name="reasoning">Reasoning or rationale for the decision</param>
    /// <param name="confidenceScore">Confidence score between 0.0 and 1.0</param>
    /// <param name="metadata">Additional metadata dictionary</param>
    /// <param name="userId">User ID who requested the decision</param>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="projectId">Project ID (nullable)</param>
    /// <param name="entityType">Entity type (default: "Project")</param>
    /// <param name="entityId">Entity ID (default: projectId)</param>
    /// <param name="entityName">Entity name (e.g., project name)</param>
    /// <param name="question">Question that was asked</param>
    /// <param name="decision">Decision that was made (JSON or text)</param>
    /// <param name="inputData">Input data (JSON)</param>
    /// <param name="outputData">Output data (JSON)</param>
    /// <param name="modelName">AI model name (e.g., "llama3.2:3b")</param>
    /// <param name="tokensUsed">Number of tokens used</param>
    /// <param name="executionTimeMs">Execution time in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    System.Threading.Tasks.Task LogDecisionAsync(
        string agentType,
        string decisionType,
        string reasoning,
        decimal confidenceScore,
        Dictionary<string, object>? metadata,
        int userId,
        int organizationId,
        int? projectId = null,
        string entityType = "Project",
        int? entityId = null,
        string? entityName = null,
        string? question = null,
        string? decision = null,
        string? inputData = null,
        string? outputData = null,
        string modelName = "llama3.2:3b",
        int tokensUsed = 0,
        int executionTimeMs = 0,
        CancellationToken cancellationToken = default);
}

