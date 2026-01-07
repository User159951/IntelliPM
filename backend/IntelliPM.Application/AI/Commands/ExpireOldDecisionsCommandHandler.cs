using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Handler for expiring old pending AI decisions.
/// </summary>
public class ExpireOldDecisionsCommandHandler : IRequestHandler<ExpireOldDecisionsCommand, ExpireOldDecisionsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpireOldDecisionsCommandHandler> _logger;

    public ExpireOldDecisionsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ExpireOldDecisionsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<ExpireOldDecisionsResult> Handle(ExpireOldDecisionsCommand request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredDecisionIds = new List<Guid>();

        var query = _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .Where(d => d.RequiresHumanApproval 
                && d.Status == AIDecisionStatus.Pending
                && d.ApprovalDeadline.HasValue 
                && d.ApprovalDeadline.Value < now);

        // Apply organization filter if specified
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(d => d.OrganizationId == request.OrganizationId.Value);
        }

        // Limit batch size
        var decisionsToExpire = await query
            .Take(request.BatchSize)
            .ToListAsync(ct);

        foreach (var decision in decisionsToExpire)
        {
            decision.MarkAsExpired();
            expiredDecisionIds.Add(decision.DecisionId);
        }

        if (decisionsToExpire.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Expired {Count} AI decisions that passed their approval deadline. Decision IDs: {DecisionIds}",
                decisionsToExpire.Count,
                string.Join(", ", expiredDecisionIds));
        }

        return new ExpireOldDecisionsResult(decisionsToExpire.Count, expiredDecisionIds);
    }
}

