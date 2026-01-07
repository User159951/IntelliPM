using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for approving an AI decision.
/// </summary>
public class ApproveAIDecisionCommandHandler : IRequestHandler<ApproveAIDecisionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ApproveAIDecisionCommandHandler> _logger;

    public ApproveAIDecisionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ApproveAIDecisionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task Handle(ApproveAIDecisionCommand request, CancellationToken ct)
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
        // SuperAdmin can approve decisions from any organization
        // Admin can only approve decisions from their own organization
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

        if (decision.Status == AIDecisionStatus.Approved)
        {
            throw new ValidationException("This decision has already been approved");
        }

        if (decision.Status == AIDecisionStatus.Rejected)
        {
            throw new ValidationException("This decision has been rejected and cannot be approved");
        }

        if (decision.Status == AIDecisionStatus.Expired)
        {
            throw new ValidationException("This decision has expired and cannot be approved");
        }

        // Check approval policy
        var policy = await GetApprovalPolicyAsync(decision.DecisionType, decision.OrganizationId, ct);
        if (policy != null && policy.IsActive)
        {
            if (!await HasRequiredRoleAsync(policy.RequiredRole, currentUserId, decision, ct))
            {
                throw new UnauthorizedException(
                    $"You do not have the required role '{policy.RequiredRole}' to approve decisions of type '{decision.DecisionType}'");
            }
        }

        // Set approval deadline if not already set
        if (!decision.ApprovalDeadline.HasValue && decision.RequiresHumanApproval)
        {
            decision.SetApprovalDeadline();
        }

        decision.ApproveDecision(currentUserId, request.ApprovalNotes);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AI decision {DecisionId} approved by user {UserId}", request.DecisionId, currentUserId);
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
        // SuperAdmin can approve anything
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
                // Also allow Admin/SuperAdmin to approve ProductOwner decisions
                return globalRole == GlobalRole.Admin || globalRole == GlobalRole.SuperAdmin;
            
            default:
                // Unknown role - allow Admin/SuperAdmin as fallback
                return globalRole == GlobalRole.Admin || globalRole == GlobalRole.SuperAdmin;
        }
    }
}

