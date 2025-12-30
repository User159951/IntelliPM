using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Handler for GetReleaseSprintsQuery.
/// Retrieves all sprints linked to a release with task completion metrics.
/// </summary>
public class GetReleaseSprintsQueryHandler : IRequestHandler<GetReleaseSprintsQuery, List<ReleaseSprintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetReleaseSprintsQueryHandler> _logger;

    public GetReleaseSprintsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetReleaseSprintsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<List<ReleaseSprintDto>> Handle(GetReleaseSprintsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            return new List<ReleaseSprintDto>();
        }

        // Get release and verify ownership
        var release = await _unitOfWork.Repository<Release>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReleaseId && r.OrganizationId == organizationId, cancellationToken);

        if (release == null)
        {
            throw new NotFoundException($"Release with ID {request.ReleaseId} not found");
        }

        // Get all sprints linked to this release
        var sprints = await _unitOfWork.Repository<Sprint>()
            .Query()
            .Include(s => s.Items)
            .ThenInclude(si => si.UserStory)
            .ThenInclude(us => us.Tasks)
            .Where(s => s.ReleaseId == request.ReleaseId)
            .Where(s => s.OrganizationId == organizationId)
            .OrderBy(s => s.StartDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to DTOs with task counts
        var sprintDtos = new List<ReleaseSprintDto>();

        foreach (var sprint in sprints)
        {
            // Calculate task counts from sprint items
            var totalTasks = 0;
            var completedTasks = 0;

            foreach (var item in sprint.Items)
            {
                if (item.UserStory?.Tasks != null)
                {
                    var itemTasks = item.UserStory.Tasks;
                    totalTasks += itemTasks.Count;
                    completedTasks += itemTasks.Count(t => t.Status == "DONE" || t.Status == "Done");
                }
            }

            var completionPercentage = totalTasks > 0 ? (int)Math.Round((double)completedTasks / totalTasks * 100) : 0;

            sprintDtos.Add(new ReleaseSprintDto
            {
                Id = sprint.Id,
                Name = $"Sprint {sprint.Number}",
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Status = sprint.Status,
                CompletedTasksCount = completedTasks,
                TotalTasksCount = totalTasks,
                CompletionPercentage = completionPercentage
            });
        }

        return sprintDtos;
    }
}

