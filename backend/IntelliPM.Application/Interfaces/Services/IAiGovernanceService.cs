namespace IntelliPM.Application.Interfaces.Services;

/// <summary>
/// Comprehensive AI governance service for quota enforcement, kill switch management, and decision logging.
/// Provides a single entry point for all AI governance checks and logging.
/// </summary>
public interface IAiGovernanceService
{
    /// <summary>
    /// Validates that AI execution is allowed for the given organization.
    /// Checks global kill switch, organization-level AI enabled flag, and quotas.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="quotaType">Type of quota to check: "Requests", "Tokens", or "Decisions"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="Application.Common.Exceptions.AIDisabledException">Thrown if AI is disabled globally or for the organization</exception>
    /// <exception cref="Application.Common.Exceptions.AIQuotaExceededException">Thrown if quota has been exceeded</exception>
    System.Threading.Tasks.Task ValidateAIExecutionAsync(int organizationId, string quotaType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if AI is enabled globally (system-wide kill switch).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if AI is enabled globally, false otherwise</returns>
    System.Threading.Tasks.Task<bool> IsGlobalAIEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if AI is enabled for a specific organization.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if AI is enabled for the organization, false otherwise</returns>
    System.Threading.Tasks.Task<bool> IsAIEnabledForOrganizationAsync(int organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an AI execution decision with comprehensive context.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="userId">User ID who requested the execution</param>
    /// <param name="decisionType">Type of decision (e.g., "RiskDetection", "SprintPlanning")</param>
    /// <param name="agentType">Type of agent (e.g., "DeliveryAgent", "ProductAgent")</param>
    /// <param name="entityType">Entity type (e.g., "Project", "Sprint", "Task")</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="requestPayload">Request payload (will be sanitized for PII)</param>
    /// <param name="modelName">AI model used</param>
    /// <param name="tokensConsumed">Total tokens consumed</param>
    /// <param name="promptTokens">Prompt tokens</param>
    /// <param name="completionTokens">Completion tokens</param>
    /// <param name="decisionOutcome">Decision outcome/result</param>
    /// <param name="confidenceScore">Confidence score (0.0 to 1.0)</param>
    /// <param name="executionTimeMs">Execution time in milliseconds</param>
    /// <param name="requiresApproval">Whether this decision requires human approval</param>
    /// <param name="correlationId">Request correlation ID for distributed tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ID of the created AIDecisionLog entry, or null if logging failed</returns>
    System.Threading.Tasks.Task<int?> LogAIExecutionAsync(
        int organizationId,
        int userId,
        string decisionType,
        string agentType,
        string entityType,
        int entityId,
        object? requestPayload,
        string modelName,
        int tokensConsumed,
        int promptTokens,
        int completionTokens,
        string decisionOutcome,
        decimal confidenceScore,
        int executionTimeMs,
        bool requiresApproval = false,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current quota status for an organization.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Quota status information</returns>
    System.Threading.Tasks.Task<AIQuotaStatus> GetQuotaStatusAsync(int organizationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the current quota status for an organization.
/// </summary>
public record AIQuotaStatus(
    int OrganizationId,
    bool IsAIEnabled,
    bool IsQuotaEnforced,
    int RequestsUsed,
    int RequestsLimit,
    int TokensUsed,
    int TokensLimit,
    int DecisionsUsed,
    int DecisionsLimit,
    string TierName,
    DateTimeOffset? PeriodEndDate
);
