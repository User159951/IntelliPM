using System.Text.Json;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Utilities;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IAiGovernanceService = IntelliPM.Application.Interfaces.Services.IAiGovernanceService;

namespace IntelliPM.Application.Services;

/// <summary>
/// Comprehensive AI governance service implementing quota enforcement, kill switch management, and decision logging.
/// </summary>
public class AiGovernanceService : IntelliPM.Application.Interfaces.Services.IAiGovernanceService
{
    private const string GlobalAIEnabledKey = "AI.Enabled";
    private const int CacheExpirationMinutes = 5;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AiGovernanceService> _logger;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly IAIAvailabilityService _availabilityService;

    public AiGovernanceService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<AiGovernanceService> logger,
        IAIDecisionLogger decisionLogger,
        IAIAvailabilityService availabilityService)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
        _decisionLogger = decisionLogger;
        _availabilityService = availabilityService;
    }

    public async System.Threading.Tasks.Task ValidateAIExecutionAsync(int organizationId, string quotaType, CancellationToken cancellationToken = default)
    {
        // First check global kill switch
        var isGlobalAIEnabled = await IsGlobalAIEnabledAsync(cancellationToken);
        if (!isGlobalAIEnabled)
        {
            _logger.LogWarning(
                "AI execution blocked for organization {OrganizationId} - Global AI kill switch is disabled",
                organizationId);
            throw new AIDisabledException(
                "AI features are currently disabled system-wide. Please contact support.",
                organizationId,
                "Global AI kill switch disabled");
        }

        // Check organization-level AI enabled flag
        var isOrgAIEnabled = await IsAIEnabledForOrganizationAsync(organizationId, cancellationToken);
        if (!isOrgAIEnabled)
        {
            _logger.LogWarning(
                "AI execution blocked for organization {OrganizationId} - Organization AI is disabled",
                organizationId);
            throw new AIDisabledException(
                "AI features are currently disabled for your organization. Please contact an administrator.",
                organizationId,
                "Organization AI disabled");
        }

        // Check quota using existing service
        await _availabilityService.CheckQuotaAsync(organizationId, quotaType, cancellationToken);
    }

    public async System.Threading.Tasks.Task<bool> IsGlobalAIEnabledAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "ai_global_enabled";

        // Check cache first
        if (_cache.TryGetValue<bool>(cacheKey, out var cachedValue))
        {
            return cachedValue;
        }

        try
        {
            var globalSettingRepo = _unitOfWork.Repository<GlobalSetting>();
            var globalSetting = await globalSettingRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(gs => gs.Key == GlobalAIEnabledKey, cancellationToken);

            // Default to enabled if setting doesn't exist (backward compatibility)
            var isEnabled = globalSetting == null || 
                           bool.TryParse(globalSetting.Value, out var parsed) && parsed;

            // Cache result for 5 minutes
            _cache.Set(cacheKey, isEnabled, TimeSpan.FromMinutes(CacheExpirationMinutes));

            return isEnabled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking global AI enabled status, defaulting to enabled");
            // Default to enabled on error to avoid breaking existing functionality
            return true;
        }
    }

    public async System.Threading.Tasks.Task<bool> IsAIEnabledForOrganizationAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        return await _availabilityService.IsAIEnabledForOrganization(organizationId, cancellationToken);
    }

    public async System.Threading.Tasks.Task<int?> LogAIExecutionAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Sanitize request payload for PII
            string? sanitizedInputData = null;
            if (requestPayload != null)
            {
                var sanitized = PiiRedactor.SanitizeObject(requestPayload);
                sanitizedInputData = JsonSerializer.Serialize(sanitized);
            }

            // Determine entity name if possible
            string? entityName = null;
            try
            {
                entityName = await GetEntityNameAsync(entityType, entityId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve entity name for {EntityType} {EntityId}", entityType, entityId);
            }

            // Use the existing AIDecisionLogger
            var decisionLogId = await _decisionLogger.LogDecisionAsync(
                agentType: agentType,
                decisionType: decisionType,
                reasoning: decisionOutcome,
                confidenceScore: confidenceScore,
                metadata: null,
                userId: userId,
                organizationId: organizationId,
                projectId: entityType == "Project" ? entityId : null,
                entityType: entityType,
                entityId: entityId,
                entityName: entityName,
                question: $"{agentType} execution for {entityType} {entityId}",
                decision: decisionOutcome,
                inputData: sanitizedInputData,
                outputData: JsonSerializer.Serialize(new { outcome = decisionOutcome, tokensConsumed, modelName }),
                modelName: modelName,
                tokensUsed: tokensConsumed,
                promptTokens: promptTokens,
                completionTokens: completionTokens,
                executionTimeMs: executionTimeMs,
                isSuccess: true,
                errorMessage: null,
                correlationId: correlationId,
                cancellationToken: cancellationToken);

            if (decisionLogId.HasValue)
            {
                _logger.LogInformation(
                    "AI execution logged: {AgentType} {DecisionType} for Org {OrganizationId}, User {UserId} | DecisionLogId: {DecisionLogId} | CorrelationId: {CorrelationId}",
                    agentType, decisionType, organizationId, userId, decisionLogId.Value, correlationId);
            }

            return decisionLogId;
        }
        catch (Exception ex)
        {
            // Log error but don't throw - logging should not break the agent execution
            _logger.LogError(ex,
                "Failed to log AI execution: AgentType={AgentType}, DecisionType={DecisionType}, OrganizationId={OrganizationId} | CorrelationId: {CorrelationId}",
                agentType, decisionType, organizationId, correlationId);
            return null;
        }
    }

    public async System.Threading.Tasks.Task<AIQuotaStatus> GetQuotaStatusAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get organization AI quota
            var orgQuotaRepo = _unitOfWork.Repository<OrganizationAIQuota>();
            var orgQuota = await orgQuotaRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.OrganizationId == organizationId, cancellationToken);

            // Get active AI quota (period-based)
            var aiQuotaRepo = _unitOfWork.Repository<AIQuota>();
            var aiQuota = await aiQuotaRepo.Query()
                .AsNoTracking()
                .Include(q => q.Template)
                .FirstOrDefaultAsync(q => q.OrganizationId == organizationId && q.IsActive, cancellationToken);

            var isAIEnabled = orgQuota?.IsAIEnabled ?? true;
            var isQuotaEnforced = aiQuota?.EnforceQuota ?? false;

            return new AIQuotaStatus(
                OrganizationId: organizationId,
                IsAIEnabled: isAIEnabled,
                IsQuotaEnforced: isQuotaEnforced,
                RequestsUsed: aiQuota?.RequestsUsed ?? 0,
                RequestsLimit: aiQuota?.MaxRequestsPerPeriod ?? 0,
                TokensUsed: aiQuota?.TokensUsed ?? 0,
                TokensLimit: aiQuota?.MaxTokensPerPeriod ?? 0,
                DecisionsUsed: aiQuota?.DecisionsMade ?? 0,
                DecisionsLimit: aiQuota?.MaxDecisionsPerPeriod ?? 0,
                TierName: aiQuota?.TierName ?? "Unknown",
                PeriodEndDate: aiQuota?.PeriodEndDate
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quota status for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    private async System.Threading.Tasks.Task<string?> GetEntityNameAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        return entityType switch
        {
            "Project" => await GetProjectNameAsync(entityId, cancellationToken),
            "Sprint" => await GetSprintNameAsync(entityId, cancellationToken),
            "Task" => await GetTaskNameAsync(entityId, cancellationToken),
            _ => $"{entityType} {entityId}"
        };
    }

    private async System.Threading.Tasks.Task<string?> GetProjectNameAsync(int projectId, CancellationToken cancellationToken)
    {
        try
        {
            var projectRepo = _unitOfWork.Repository<Project>();
            var project = await projectRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
            return project?.Name;
        }
        catch
        {
            return null;
        }
    }

    private async System.Threading.Tasks.Task<string?> GetSprintNameAsync(int sprintId, CancellationToken cancellationToken)
    {
        try
        {
            var sprintRepo = _unitOfWork.Repository<Sprint>();
            var sprint = await sprintRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);
            return sprint != null ? $"Sprint {sprint.Number}" : null;
        }
        catch
        {
            return null;
        }
    }

    private async System.Threading.Tasks.Task<string?> GetTaskNameAsync(int taskId, CancellationToken cancellationToken)
    {
        try
        {
            var taskRepo = _unitOfWork.Repository<ProjectTask>();
            var task = await taskRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
            return task?.Title;
        }
        catch
        {
            return null;
        }
    }
}
