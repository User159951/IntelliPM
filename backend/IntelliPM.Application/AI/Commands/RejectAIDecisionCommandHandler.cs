using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for rejecting an AI decision.
/// </summary>
public class RejectAIDecisionCommandHandler : IRequestHandler<RejectAIDecisionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RejectAIDecisionCommandHandler> _logger;

    public RejectAIDecisionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<RejectAIDecisionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task Handle(RejectAIDecisionCommand request, CancellationToken ct)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == 0)
        {
            throw new UnauthorizedException("User not authenticated");
        }

        var decision = await _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .Include(d => d.RequestedByUser)
            .FirstOrDefaultAsync(d => d.DecisionId == request.DecisionId, ct);

        if (decision == null)
        {
            throw new NotFoundException($"Decision {request.DecisionId} not found");
        }

        // Verify user has access to this organization's decisions
        // SuperAdmin can reject decisions from any organization
        // Admin can only reject decisions from their own organization
        if (!_currentUserService.IsSuperAdmin())
        {
            var organizationId = _currentUserService.GetOrganizationId();
            if (decision.OrganizationId != organizationId)
            {
                throw new UnauthorizedException("You do not have access to this decision");
            }
        }

        if (!decision.RequiresHumanApproval)
        {
            throw new ValidationException("This decision does not require approval");
        }

        if (decision.Status == AIDecisionStatus.Rejected)
        {
            throw new ValidationException("This decision has already been rejected");
        }

        if (decision.Status == AIDecisionStatus.Applied)
        {
            throw new ValidationException("This decision has already been applied and cannot be rejected");
        }

        // Check approval policy (same role requirements as approval)
        var policy = await GetApprovalPolicyAsync(decision.DecisionType, decision.OrganizationId, ct);
        if (policy != null && policy.IsActive)
        {
            if (!await HasRequiredRoleAsync(policy.RequiredRole, currentUserId, decision, ct))
            {
                throw new UnauthorizedException(
                    $"You do not have the required role '{policy.RequiredRole}' to reject decisions of type '{decision.DecisionType}'");
            }
        }

        decision.RejectDecision(currentUserId, $"{request.RejectionReason}. {request.RejectionNotes}");
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AI decision {DecisionId} rejected by user {UserId}. Reason: {Reason}",
            request.DecisionId, currentUserId, request.RejectionReason);
    }

    private async Task<AIDecisionApprovalPolicy?> GetApprovalPolicyAsync(string decisionType, int organizationId, CancellationToken ct)
    {
        // First try organization-specific policy
        var orgPolicy = await _unitOfWork.Repository<AIDecisionApprovalPolicy>()
            .Query()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId 
                && p.DecisionType == decisionType 
                && p.IsActive, ct);

        if (orgPolicy != null)
            return orgPolicy;

        // Fall back to global policy (OrganizationId is null)
        return await _unitOfWork.Repository<AIDecisionApprovalPolicy>()
            .Query()
            .FirstOrDefaultAsync(p => p.OrganizationId == null 
                && p.DecisionType == decisionType 
                && p.IsActive, ct);
    }

    private async Task<bool> HasRequiredRoleAsync(string requiredRole, int userId, AIDecisionLog decision, CancellationToken ct)
    {
        // SuperAdmin can reject anything
        if (_currentUserService.IsSuperAdmin())
            return true;

        // Check global roles
        var globalRole = _currentUserService.GetGlobalRole();
        
        switch (requiredRole.ToLowerInvariant())
        {
            case "superadmin":
                return globalRole == GlobalRole.SuperAdmin;
            
            case "admin":
                return globalRole == GlobalRole.Admin || globalRole == GlobalRole.SuperAdmin;
            
            case "productowner":
                // Check if user is ProductOwner in the project associated with this decision
                if (decision.EntityType == "Project" && decision.EntityId > 0)
                {
                    var projectMember = await _unitOfWork.Repository<ProjectMember>()
                        .Query()
                        .FirstOrDefaultAsync(pm => pm.ProjectId == decision.EntityId 
                            && pm.UserId == userId, ct);
                    
                    if (projectMember != null && projectMember.Role == Domain.Enums.ProjectRole.ProductOwner)
                        return true;
                }
                // Also allow Admin/SuperAdmin to reject ProductOwner decisions
                return globalRole == GlobalRole.Admin || globalRole == GlobalRole.SuperAdmin;
            
            default:
                // Unknown role - allow Admin/SuperAdmin as fallback
                return globalRole == GlobalRole.Admin || globalRole == GlobalRole.SuperAdmin;
        }
    }
}

