using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
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
            .FirstOrDefaultAsync(d => d.DecisionId == request.DecisionId, ct);

        if (decision == null)
        {
            throw new NotFoundException($"Decision {request.DecisionId} not found");
        }

        // Verify user has access to this organization's decisions
        var organizationId = _currentUserService.GetOrganizationId();
        if (decision.OrganizationId != organizationId)
        {
            throw new UnauthorizedException("You do not have access to this decision");
        }

        if (!decision.RequiresHumanApproval)
        {
            throw new ValidationException("This decision does not require approval");
        }

        if (decision.ApprovedByHuman.HasValue && decision.ApprovedByHuman.Value)
        {
            throw new ValidationException("This decision has already been approved");
        }

        decision.ApproveDecision(currentUserId, request.ApprovalNotes);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AI decision {DecisionId} approved by user {UserId}", request.DecisionId, currentUserId);
    }
}

