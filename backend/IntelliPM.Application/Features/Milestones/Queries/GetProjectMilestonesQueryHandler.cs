using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Handler for GetProjectMilestonesQuery.
/// Retrieves all milestones for a project with optional filtering.
/// </summary>
public class GetProjectMilestonesQueryHandler : IRequestHandler<GetProjectMilestonesQuery, List<MilestoneDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectMilestonesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<List<MilestoneDto>> Handle(GetProjectMilestonesQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
            return new List<MilestoneDto>();

        var now = DateTimeOffset.UtcNow;

        var query = _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.CreatedBy)
            .Where(m => m.ProjectId == request.ProjectId)
            .Where(m => m.OrganizationId == organizationId); // Multi-tenancy

        // Apply status filter if provided
        if (request.Status.HasValue)
        {
            query = query.Where(m => m.Status == request.Status.Value);
        }

        // Exclude completed milestones if IncludeCompleted is false
        if (!request.IncludeCompleted)
        {
            query = query.Where(m => m.Status != MilestoneStatus.Completed);
        }

        var milestones = await query
            .OrderBy(m => m.DueDate)
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

