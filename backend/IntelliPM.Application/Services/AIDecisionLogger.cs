using System.Text.Json;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for logging AI decisions to AIDecisionLog for audit trail and governance.
/// </summary>
public class AIDecisionLogger : IAIDecisionLogger
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AIDecisionLogger> _logger;
    private readonly IAIPricingService _pricingService;
    private readonly ICorrelationIdService _correlationIdService;

    public AIDecisionLogger(
        IUnitOfWork unitOfWork,
        ILogger<AIDecisionLogger> logger,
        IAIPricingService pricingService,
        ICorrelationIdService correlationIdService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _pricingService = pricingService;
        _correlationIdService = correlationIdService;
    }

    public async System.Threading.Tasks.Task<int?> LogDecisionAsync(
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
        int promptTokens = 0,
        int completionTokens = 0,
        int executionTimeMs = 0,
        bool isSuccess = true,
        string? errorMessage = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get correlation ID if not provided
            var finalCorrelationId = correlationId ?? _correlationIdService.GetCorrelationId() ?? Guid.NewGuid().ToString();
            
            // Use projectId as entityId if not specified
            var finalEntityId = entityId ?? projectId ?? 0;
            
            // Get project name if entityName is not provided and we have a projectId
            string? finalEntityName = entityName;
            if (string.IsNullOrEmpty(finalEntityName) && projectId.HasValue)
            {
                try
                {
                    var projectRepo = _unitOfWork.Repository<Project>();
                    var project = await projectRepo.GetByIdAsync(projectId.Value, cancellationToken);
                    finalEntityName = project?.Name ?? $"Project {projectId.Value}";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve project name for project {ProjectId}, using default", projectId);
                    finalEntityName = $"Project {projectId.Value}";
                }
            }
            else if (string.IsNullOrEmpty(finalEntityName))
            {
                finalEntityName = $"{entityType} {finalEntityId}";
            }

            // Default question if not provided
            var finalQuestion = question ?? $"{agentType} decision for {entityType} {finalEntityId}";

            // Serialize metadata if provided
            var metadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : "{}";

            // Determine if approval is required (can be enhanced with policy checks)
            var requiresApproval = confidenceScore < AIDecisionConstants.MinConfidenceScore;

            // Create decision log entry
            var decisionLog = new AIDecisionLog
            {
                OrganizationId = organizationId,
                DecisionType = decisionType,
                AgentType = agentType,
                EntityType = entityType,
                EntityId = finalEntityId,
                EntityName = finalEntityName ?? string.Empty,
                Question = finalQuestion,
                Decision = decision ?? reasoning, // Use reasoning as decision if decision not provided
                Reasoning = reasoning,
                ConfidenceScore = confidenceScore,
                ModelName = modelName,
                ModelVersion = string.Empty, // Can be enhanced later
                TokensUsed = tokensUsed > 0 ? tokensUsed : (promptTokens + completionTokens), // Use total if provided, otherwise sum
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                InputData = inputData ?? metadataJson,
                OutputData = outputData ?? reasoning,
                RequestedByUserId = userId,
                RequiresHumanApproval = requiresApproval,
                Status = isSuccess && !requiresApproval 
                    ? AIDecisionStatus.Applied 
                    : AIDecisionStatus.Pending, // Failed executions or approvals required remain pending
                WasApplied = isSuccess && !requiresApproval, // Only mark as applied if successful and no approval needed
                AppliedAt = isSuccess && !requiresApproval ? DateTimeOffset.UtcNow : null,
                CreatedAt = DateTimeOffset.UtcNow,
                ExecutionTimeMs = executionTimeMs,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                CorrelationId = finalCorrelationId
            };

            // Set approval deadline if approval is required
            if (requiresApproval)
            {
                decisionLog.SetApprovalDeadline();
            }

            // Add to repository
            var decisionLogRepo = _unitOfWork.Repository<AIDecisionLog>();
            await decisionLogRepo.AddAsync(decisionLog, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "AI Decision logged: {AgentType} {DecisionType} for Org {OrganizationId} | CorrelationId: {CorrelationId} | DecisionLogId: {DecisionLogId}",
                agentType, decisionType, organizationId, finalCorrelationId, decisionLog.Id);

            return decisionLog.Id;
        }
        catch (Exception ex)
        {
            // Log error but don't throw - logging should not break the agent execution
            var errorCorrelationId = correlationId ?? _correlationIdService.GetCorrelationId() ?? Guid.NewGuid().ToString();
            _logger.LogError(ex,
                "Failed to log AI decision: AgentType={AgentType}, DecisionType={DecisionType}, ProjectId={ProjectId} | CorrelationId: {CorrelationId}",
                agentType, decisionType, projectId, errorCorrelationId);
            return null;
        }
    }
}

