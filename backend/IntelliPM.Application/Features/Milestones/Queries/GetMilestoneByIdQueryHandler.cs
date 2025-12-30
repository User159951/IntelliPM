using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Handler for GetMilestoneByIdQuery.
/// Retrieves a milestone by its ID, verifying it belongs to the user's organization.
/// </summary>
public class GetMilestoneByIdQueryHandler : IRequestHandler<GetMilestoneByIdQuery, MilestoneDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMilestoneByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<MilestoneDto?> Handle(GetMilestoneByIdQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
            return null;

        var now = DateTimeOffset.UtcNow;

        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.CreatedBy)
            .Where(m => m.Id == request.Id)
            .Where(m => m.OrganizationId == organizationId) // Multi-tenancy
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

