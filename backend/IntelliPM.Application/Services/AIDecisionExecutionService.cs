using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for checking if an AI decision can be executed.
/// Implements blocking mechanism for decisions that require approval.
/// </summary>
public class AIDecisionExecutionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AIDecisionExecutionService> _logger;

    public AIDecisionExecutionService(
        IUnitOfWork unitOfWork,
        ILogger<AIDecisionExecutionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a decision can be executed.
    /// Throws AIDecisionNotApprovedException if decision requires approval but hasn't been approved.
    /// </summary>
    /// <param name="decisionId">The decision ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="AIDecisionNotApprovedException">Thrown if decision requires approval but isn't approved</exception>
    /// <exception cref="NotFoundException">Thrown if decision is not found</exception>
    public async System.Threading.Tasks.Task EnsureDecisionCanBeExecutedAsync(
        Guid decisionId,
        CancellationToken cancellationToken = default)
    {
        var decision = await _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .FirstOrDefaultAsync(d => d.DecisionId == decisionId, cancellationToken);

        if (decision == null)
        {
            throw new NotFoundException($"Decision {decisionId} not found");
        }

        await EnsureDecisionCanBeExecutedAsync(decision, cancellationToken);
    }

    /// <summary>
    /// Checks if a decision can be executed.
    /// Throws AIDecisionNotApprovedException if decision requires approval but hasn't been approved.
    /// </summary>
    /// <param name="decision">The decision to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="AIDecisionNotApprovedException">Thrown if decision requires approval but isn't approved</exception>
    public async System.Threading.Tasks.Task EnsureDecisionCanBeExecutedAsync(
        AIDecisionLog decision,
        CancellationToken cancellationToken = default)
    {
        // If decision doesn't require approval, it can always be executed
        if (!decision.RequiresHumanApproval)
        {
            return;
        }

        // Check if decision has expired
        if (decision.IsExpired())
        {
            // Mark as expired if not already marked
            if (decision.Status != AIDecisionStatus.Expired)
            {
                decision.MarkAsExpired();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            throw new AIDecisionNotApprovedException(
                decision.DecisionId,
                decision.DecisionType,
                decision.Status.ToString(),
                decision.OrganizationId,
                $"Decision {decision.DecisionId} has expired (approval deadline passed)");
        }

        // Check if decision is approved
        if (!decision.CanBeExecuted())
        {
            // Check if there's a blocking policy
            var policy = await GetApprovalPolicyAsync(decision.DecisionType, decision.OrganizationId, cancellationToken);
            var isBlocking = policy?.IsBlockingIfNotApproved ?? true; // Default to blocking if no policy

            if (isBlocking)
            {
                throw new AIDecisionNotApprovedException(
                    decision.DecisionId,
                    decision.DecisionType,
                    decision.Status.ToString(),
                    decision.OrganizationId,
                    $"Decision {decision.DecisionId} requires approval but current status is {decision.Status}. " +
                    $"Required role: {policy?.RequiredRole ?? "Unknown"}");
            }
            else
            {
                // Non-blocking policy - log warning but allow execution
                _logger.LogWarning(
                    "Executing decision {DecisionId} that requires approval but isn't approved (non-blocking policy). Status: {Status}",
                    decision.DecisionId,
                    decision.Status);
            }
        }
    }

    private async Task<AIDecisionApprovalPolicy?> GetApprovalPolicyAsync(
        string decisionType,
        int organizationId,
        CancellationToken cancellationToken)
    {
        // First try organization-specific policy
        var orgPolicy = await _unitOfWork.Repository<AIDecisionApprovalPolicy>()
            .Query()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId
                && p.DecisionType == decisionType
                && p.IsActive, cancellationToken);

        if (orgPolicy != null)
            return orgPolicy;

        // Fall back to global policy (OrganizationId is null)
        return await _unitOfWork.Repository<AIDecisionApprovalPolicy>()
            .Query()
            .FirstOrDefaultAsync(p => p.OrganizationId == null
                && p.DecisionType == decisionType
                && p.IsActive, cancellationToken);
    }
}

