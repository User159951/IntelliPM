using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting pending AI decision approvals.
/// </summary>
public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, PagedResponse<PendingApprovalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPendingApprovalsQueryHandler> _logger;

    public GetPendingApprovalsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetPendingApprovalsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<PagedResponse<PendingApprovalDto>> Handle(GetPendingApprovalsQuery request, CancellationToken ct)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == 0)
        {
            throw new Application.Common.Exceptions.UnauthorizedException("User not authenticated");
        }

        // Determine organization scope
        int? organizationId = request.OrganizationId;
        if (!_currentUserService.IsSuperAdmin())
        {
            // Non-SuperAdmin users can only see their own organization's pending approvals
            organizationId = _currentUserService.GetOrganizationId();
        }

        var query = _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .AsNoTracking()
            .Include(d => d.RequestedByUser)
            .Include(d => d.RequestedByUser.Organization)
            .Where(d => d.RequiresHumanApproval 
                && d.Status == AIDecisionStatus.Pending);

        // Apply organization filter
        if (organizationId.HasValue)
        {
            query = query.Where(d => d.OrganizationId == organizationId.Value);
        }

        // Apply decision type filter
        if (!string.IsNullOrEmpty(request.DecisionType))
        {
            query = query.Where(d => d.DecisionType == request.DecisionType);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var now = DateTimeOffset.UtcNow;
        var decisions = await query
            .OrderBy(d => d.ApprovalDeadline ?? d.CreatedAt.AddHours(48)) // Order by deadline, oldest first
            .ThenByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new PendingApprovalDto(
                d.DecisionId,
                d.DecisionType,
                d.AgentType,
                d.EntityType,
                d.EntityId,
                d.EntityName,
                d.Question,
                d.Decision,
                d.ConfidenceScore,
                d.Status.ToString(),
                d.CreatedAt,
                d.ApprovalDeadline,
                d.ApprovalDeadline.HasValue && now > d.ApprovalDeadline.Value,
                d.RequestedByUserId,
                d.RequestedByUser.Username,
                d.OrganizationId,
                d.RequestedByUser.Organization.Name,
                d.TokensUsed,
                d.CostAccumulated
            ))
            .ToListAsync(ct);

        return new PagedResponse<PendingApprovalDto>(decisions, page, pageSize, totalCount);
    }
}

