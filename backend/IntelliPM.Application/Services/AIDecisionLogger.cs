using System.Text.Json;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
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

    public AIDecisionLogger(
        IUnitOfWork unitOfWork,
        ILogger<AIDecisionLogger> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task LogDecisionAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
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
                TokensUsed = tokensUsed,
                PromptTokens = 0, // Can be enhanced later
                CompletionTokens = 0, // Can be enhanced later
                InputData = inputData ?? metadataJson,
                OutputData = outputData ?? reasoning,
                RequestedByUserId = userId,
                RequiresHumanApproval = false,
                Status = AIDecisionConstants.Statuses.Applied, // Default status for RAG agents
                WasApplied = true,
                AppliedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ExecutionTimeMs = executionTimeMs,
                IsSuccess = true
            };

            // Add to repository
            var decisionLogRepo = _unitOfWork.Repository<AIDecisionLog>();
            await decisionLogRepo.AddAsync(decisionLog, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Logged AI decision: AgentType={AgentType}, DecisionType={DecisionType}, EntityId={EntityId}, Confidence={Confidence}",
                agentType, decisionType, finalEntityId, confidenceScore);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - logging should not break the agent execution
            _logger.LogError(ex,
                "Failed to log AI decision: AgentType={AgentType}, DecisionType={DecisionType}, ProjectId={ProjectId}",
                agentType, decisionType, projectId);
        }
    }
}

