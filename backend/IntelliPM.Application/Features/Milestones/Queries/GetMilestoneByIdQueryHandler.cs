using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
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

    public GetMilestoneByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<MilestoneDto> Handle(GetMilestoneByIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Where(m => m.Id == request.Id)
            // Tenant filter automatically applied via global filter
            .Include(m => m.CreatedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (milestone == null)
            throw new NotFoundException($"Milestone not found");

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

