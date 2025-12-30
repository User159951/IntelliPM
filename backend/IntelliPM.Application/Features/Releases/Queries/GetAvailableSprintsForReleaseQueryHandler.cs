using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Sprints.Queries;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Features.Releases.Queries;

/// <summary>
/// Handler for GetAvailableSprintsForReleaseQuery.
/// Retrieves sprints that can be added to a release.
/// </summary>
public class GetAvailableSprintsForReleaseQueryHandler : IRequestHandler<GetAvailableSprintsForReleaseQuery, List<SprintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAvailableSprintsForReleaseQueryHandler> _logger;

    public GetAvailableSprintsForReleaseQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetAvailableSprintsForReleaseQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<List<SprintDto>> Handle(GetAvailableSprintsForReleaseQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            return new List<SprintDto>();
        }

        // Get all sprints for project where OrganizationId matches
        var query = _unitOfWork.Repository<Sprint>()
            .Query()
            .Include(s => s.Project)
            .Include(s => s.Items)
            .ThenInclude(si => si.UserStory)
            .ThenInclude(us => us.Tasks)
            .Where(s => s.ProjectId == request.ProjectId)
            .Where(s => s.OrganizationId == organizationId);

        // Filter out sprints already in other releases
        if (request.ReleaseId.HasValue)
        {
            // Editing existing release: exclude sprints with ReleaseId != null AND ReleaseId != request.ReleaseId
            query = query.Where(s => s.ReleaseId == null || s.ReleaseId == request.ReleaseId);
        }
        else
        {
            // Creating new release: exclude sprints with ReleaseId != null
            query = query.Where(s => s.ReleaseId == null);
        }

        // Filter by status: Completed or InProgress (not planned/not started)
        query = query.Where(s => s.Status == SprintConstants.Statuses.Completed || s.Status == SprintConstants.Statuses.Active);

        // Order by EndDate descending (most recent first)
        query = query.OrderByDescending(s => s.EndDate);

        var sprints = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to SprintDto with task counts
        var sprintDtos = new List<SprintDto>();

        foreach (var sprint in sprints)
        {
            // Calculate task count from sprint items
            var taskCount = 0;
            foreach (var item in sprint.Items)
            {
                if (item.UserStory?.Tasks != null)
                {
                    taskCount += item.UserStory.Tasks.Count;
                }
            }

            sprintDtos.Add(new SprintDto(
                sprint.Id,
                sprint.ProjectId,
                sprint.Project?.Name ?? string.Empty,
                sprint.Number,
                sprint.Goal,
                sprint.StartDate,
                sprint.EndDate,
                sprint.Status,
                taskCount,
                sprint.CreatedAt
            ));
        }

        return sprintDtos;
    }
}

