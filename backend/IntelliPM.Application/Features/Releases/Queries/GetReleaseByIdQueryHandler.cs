using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Handler for GetReleaseByIdQuery.
/// Retrieves a release by its ID with all related data.
/// </summary>
public class GetReleaseByIdQueryHandler : IRequestHandler<GetReleaseByIdQuery, ReleaseDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetReleaseByIdQueryHandler> _logger;

    public GetReleaseByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetReleaseByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReleaseDto?> Handle(GetReleaseByIdQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            return null;
        }

        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .Include(r => r.CreatedBy)
            .Include(r => r.ReleasedBy)
            .Include(r => r.Sprints)
                .ThenInclude(s => s.Items)
                    .ThenInclude(si => si.UserStory)
                        .ThenInclude(us => us.Tasks)
            .Include(r => r.QualityGates)
                .ThenInclude(qg => qg.CheckedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.OrganizationId == organizationId, cancellationToken);

        if (release == null)
        {
            return null;
        }

        // Calculate task counts from sprints
        var totalTasks = 0;
        var completedTasks = 0;

        foreach (var sprint in release.Sprints)
        {
            foreach (var item in sprint.Items)
            {
                if (item.UserStory?.Tasks != null)
                {
                    var itemTasks = item.UserStory.Tasks;
                    totalTasks += itemTasks.Count;
                    completedTasks += itemTasks.Count(t => t.Status == "DONE" || t.Status == "Done");
                }
            }
        }

        // Map quality gates
        var qualityGates = release.QualityGates.Select(qg => new QualityGateDto
        {
            Id = qg.Id,
            ReleaseId = qg.ReleaseId,
            Type = qg.Type.ToString(),
            Status = qg.Status.ToString(),
            IsRequired = qg.IsRequired,
            Threshold = qg.Threshold,
            ActualValue = qg.ActualValue,
            Message = qg.Message,
            Details = qg.Details,
            CheckedAt = qg.CheckedAt,
            CheckedByName = qg.CheckedByUser?.Username
        }).ToList();

        // Map sprints
        var sprintDtos = release.Sprints.Select(s =>
        {
            var sprintTotalTasks = 0;
            var sprintCompletedTasks = 0;

            foreach (var item in s.Items)
            {
                if (item.UserStory?.Tasks != null)
                {
                    var itemTasks = item.UserStory.Tasks;
                    sprintTotalTasks += itemTasks.Count;
                    sprintCompletedTasks += itemTasks.Count(t => t.Status == "DONE" || t.Status == "Done");
                }
            }

            var completionPercentage = sprintTotalTasks > 0 
                ? (int)Math.Round((double)sprintCompletedTasks / sprintTotalTasks * 100) 
                : 0;

            return new ReleaseSprintDto
            {
                Id = s.Id,
                Name = $"Sprint {s.Number}",
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Status = s.Status,
                CompletedTasksCount = sprintCompletedTasks,
                TotalTasksCount = sprintTotalTasks,
                CompletionPercentage = completionPercentage
            };
        }).ToList();

        var overallQualityStatus = release.GetOverallQualityStatus();

        return new ReleaseDto
        {
            Id = release.Id,
            ProjectId = release.ProjectId,
            Name = release.Name,
            Version = release.Version,
            Description = release.Description,
            Type = release.Type.ToString(),
            Status = release.Status.ToString(),
            PlannedDate = release.PlannedDate,
            ActualReleaseDate = release.ActualReleaseDate,
            ReleaseNotes = release.ReleaseNotes,
            ChangeLog = release.ChangeLog,
            IsPreRelease = release.IsPreRelease,
            TagName = release.TagName,
            SprintCount = release.Sprints.Count,
            CompletedTasksCount = completedTasks,
            TotalTasksCount = totalTasks,
            OverallQualityStatus = overallQualityStatus.ToString(),
            QualityGates = qualityGates,
            CreatedAt = release.CreatedAt,
            CreatedByName = release.CreatedBy?.Username ?? "Unknown",
            ReleasedByName = release.ReleasedBy?.Username,
            Sprints = sprintDtos
        };
    }
}
