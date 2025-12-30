using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Handler for GetOverdueMilestonesQuery.
/// Retrieves all overdue milestones across all projects for an organization.
/// </summary>
public class GetOverdueMilestonesQueryHandler : IRequestHandler<GetOverdueMilestonesQuery, List<MilestoneDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetOverdueMilestonesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<List<MilestoneDto>> Handle(GetOverdueMilestonesQuery request, CancellationToken cancellationToken)
    {
        var organizationId = request.OrganizationId ?? _currentUserService.GetOrganizationId();
        if (organizationId == 0)
            return new List<MilestoneDto>();

        var now = DateTimeOffset.UtcNow;

        var milestones = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.CreatedBy)
            .Where(m => m.OrganizationId == organizationId)
            .Where(m => m.DueDate < now)
            .Where(m => m.Status == MilestoneStatus.Pending || m.Status == MilestoneStatus.InProgress)
            .OrderBy(m => m.DueDate) // Most overdue first
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return milestones.Select(m => MapToDto(m, now)).ToList();
    }

    private static MilestoneDto MapToDto(Milestone milestone, DateTimeOffset now)
    {
        var daysUntilDue = (milestone.DueDate - now).Days;
        var isOverdue = milestone.DueDate < now 
            && milestone.Status != MilestoneStatus.Completed 
            && milestone.Status != MilestoneStatus.Cancelled;

        return new MilestoneDto
        {
            Id = milestone.Id,
            ProjectId = milestone.ProjectId,
            Name = milestone.Name,
            Description = milestone.Description ?? string.Empty,
            Type = milestone.Type.ToString(),
            Status = milestone.Status.ToString(),
            DueDate = milestone.DueDate,
            CompletedAt = milestone.CompletedAt,
            Progress = milestone.Progress,
            DaysUntilDue = daysUntilDue,
            IsOverdue = isOverdue,
            CreatedAt = milestone.CreatedAt,
            CreatedByName = milestone.CreatedBy?.Username ?? "Unknown"
        };
    }
}

