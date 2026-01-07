using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace IntelliPM.Application.Services;

/// <summary>
/// Validates workflow transitions based on role-based rules and required conditions.
/// </summary>
public class WorkflowTransitionValidator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkflowTransitionValidator> _logger;

    public WorkflowTransitionValidator(
        IUnitOfWork unitOfWork,
        ILogger<WorkflowTransitionValidator> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Validates if a user with the given role can transition an entity from one status to another.
    /// </summary>
    /// <param name="entityType">Type of entity (Task, Sprint, Release)</param>
    /// <param name="fromStatus">Current status</param>
    /// <param name="toStatus">Target status</param>
    /// <param name="userRole">Role of the user attempting the transition</param>
    /// <param name="entityId">ID of the entity being transitioned (for audit logging)</param>
    /// <param name="userId">ID of the user attempting the transition (for audit logging)</param>
    /// <param name="projectId">Optional project ID (for audit logging)</param>
    /// <param name="checkConditions">Optional function to check required conditions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>WorkflowValidationResult indicating if transition is allowed and any denial reason</returns>
    public async Task<WorkflowValidationResult> ValidateTransitionAsync(
        string entityType,
        string fromStatus,
        string toStatus,
        ProjectRole userRole,
        int entityId,
        int userId,
        int? projectId = null,
        Func<List<string>, Task<bool>>? checkConditions = null,
        CancellationToken cancellationToken = default)
    {
        var roleString = userRole.ToString();

        // Log attempt
        var auditLog = new WorkflowTransitionAuditLog
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            UserRole = roleString,
            ProjectId = projectId,
            AttemptedAt = DateTimeOffset.UtcNow
        };

        try
        {
            // Find applicable rule
            var ruleRepo = _unitOfWork.Repository<WorkflowTransitionRule>();
            var rule = await ruleRepo.Query()
                .FirstOrDefaultAsync(r =>
                    r.EntityType == entityType &&
                    r.FromStatus == fromStatus &&
                    r.ToStatus == toStatus &&
                    r.IsActive, cancellationToken);

            if (rule == null)
            {
                // No rule found - allow transition by default (backward compatibility)
                // But log it for visibility
                _logger.LogWarning(
                    "No workflow rule found for {EntityType} transition {FromStatus} -> {ToStatus}. Allowing by default.",
                    entityType, fromStatus, toStatus);

                auditLog.WasAllowed = true;
                auditLog.DenialReason = null;
                await LogTransitionAttemptAsync(auditLog);

                return new WorkflowValidationResult
                {
                    IsAllowed = true,
                    Reason = "No rule defined - allowed by default"
                };
            }

            // Check if user's role is allowed
            var allowedRoles = rule.GetAllowedRoles();
            if (!allowedRoles.Contains(roleString))
            {
                var reason = $"Role '{roleString}' is not allowed for this transition. Allowed roles: {string.Join(", ", allowedRoles)}";
                
                auditLog.WasAllowed = false;
                auditLog.DenialReason = reason;
                await LogTransitionAttemptAsync(auditLog);

                return new WorkflowValidationResult
                {
                    IsAllowed = false,
                    Reason = reason
                };
            }

            // Check required conditions if provided
            var requiredConditions = rule.GetRequiredConditions();
            if (requiredConditions.Any() && checkConditions != null)
            {
                var conditionsMet = await checkConditions(requiredConditions);
                if (!conditionsMet)
                {
                    var reason = $"Required conditions not met: {string.Join(", ", requiredConditions)}";
                    
                    auditLog.WasAllowed = false;
                    auditLog.DenialReason = reason;
                    await LogTransitionAttemptAsync(auditLog);

                    return new WorkflowValidationResult
                    {
                        IsAllowed = false,
                        Reason = reason
                    };
                }
            }

            // Transition is allowed
            auditLog.WasAllowed = true;
            auditLog.DenialReason = null;
            await LogTransitionAttemptAsync(auditLog);

            return new WorkflowValidationResult
            {
                IsAllowed = true,
                Reason = "Transition allowed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating workflow transition");
            
            auditLog.WasAllowed = false;
            auditLog.DenialReason = $"Validation error: {ex.Message}";
            await LogTransitionAttemptAsync(auditLog);

            // On error, deny transition for safety
            return new WorkflowValidationResult
            {
                IsAllowed = false,
                Reason = $"Validation error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Logs the transition attempt to the audit log.
    /// </summary>
    private async Task LogTransitionAttemptAsync(WorkflowTransitionAuditLog auditLog)
    {
        try
        {
            var auditRepo = _unitOfWork.Repository<WorkflowTransitionAuditLog>();
            await auditRepo.AddAsync(auditLog);
            // Note: SaveChanges should be called by the calling handler
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log workflow transition audit");
            // Don't throw - audit logging failure shouldn't break the workflow
        }
    }
}

/// <summary>
/// Result of a workflow transition validation.
/// </summary>
public class WorkflowValidationResult
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
}

