using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Handler for GetMilestoneStatisticsQuery.
/// Calculates and returns aggregated statistics about milestones for a project.
/// </summary>
public class GetMilestoneStatisticsQueryHandler : IRequestHandler<GetMilestoneStatisticsQuery, MilestoneStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMilestoneStatisticsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<MilestoneStatisticsDto> Handle(GetMilestoneStatisticsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            return new MilestoneStatisticsDto();
        }

        var now = DateTimeOffset.UtcNow;

        var milestones = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Where(m => m.ProjectId == request.ProjectId)
            .Where(m => m.OrganizationId == organizationId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var total = milestones.Count;
        var completed = milestones.Count(m => m.Status == MilestoneStatus.Completed);
        var missed = milestones.Count(m => m.Status == MilestoneStatus.Missed);
        var pending = milestones.Count(m => m.Status == MilestoneStatus.Pending);
        var inProgress = milestones.Count(m => m.Status == MilestoneStatus.InProgress);
        var upcoming = milestones.Count(m => 
            (m.Status == MilestoneStatus.Pending || m.Status == MilestoneStatus.InProgress) 
            && m.DueDate >= now);

        var completionRate = total > 0 ? (double)completed / total * 100 : 0;

        return new MilestoneStatisticsDto
        {
            TotalMilestones = total,
            CompletedMilestones = completed,
            MissedMilestones = missed,
            UpcomingMilestones = upcoming,
            PendingMilestones = pending,
            InProgressMilestones = inProgress,
            CompletionRate = Math.Round(completionRate, 2)
        };
    }
}

