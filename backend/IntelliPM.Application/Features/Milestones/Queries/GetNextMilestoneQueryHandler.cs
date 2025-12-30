using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Handler for GetNextMilestoneQuery.
/// Retrieves the next upcoming milestone for a project based on due date.
/// </summary>
public class GetNextMilestoneQueryHandler : IRequestHandler<GetNextMilestoneQuery, MilestoneDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetNextMilestoneQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<MilestoneDto?> Handle(GetNextMilestoneQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
            return null;

        var now = DateTimeOffset.UtcNow;

        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.CreatedBy)
            .Where(m => m.ProjectId == request.ProjectId)
            .Where(m => m.OrganizationId == organizationId) // Multi-tenancy
            .Where(m => m.Status == MilestoneStatus.Pending || m.Status == MilestoneStatus.InProgress)
            .Where(m => m.DueDate >= now)
            .OrderBy(m => m.DueDate)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (milestone == null)
            return null;

        return MapToDto(milestone, now);
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

