using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Sprints.Queries;

/// <summary>
/// Handler for calculating sprint velocity based on completed story points.
/// </summary>
public class GetSprintVelocityQueryHandler : IRequestHandler<GetSprintVelocityQuery, SprintVelocityResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetSprintVelocityQueryHandler> _logger;

    public GetSprintVelocityQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetSprintVelocityQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SprintVelocityResponse> Handle(GetSprintVelocityQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var taskRepo = _unitOfWork.Repository<ProjectTask>();

        // Build query for sprints
        IQueryable<Sprint> sprintQuery = sprintRepo.Query()
            .AsNoTracking()
            .Where(s => s.ProjectId == request.ProjectId)
            .Where(s => s.OrganizationId == organizationId); // Multi-tenancy check

        // If SprintId provided: fetch single sprint
        if (request.SprintId.HasValue)
        {
            sprintQuery = sprintQuery.Where(s => s.Id == request.SprintId.Value);
        }
        else
        {
            // If SprintId not provided: fetch last N sprints for project
            // Order by StartDate descending, then take LastNSprints
            var lastN = request.LastNSprints ?? 5;
            sprintQuery = sprintQuery
                .OrderByDescending(s => s.StartDate ?? s.CreatedAt)
                .Take(lastN);
        }

        // Include related tasks for efficient querying
        // Note: If SprintId is provided, we already have the single sprint
        // If not provided, we already ordered and took LastNSprints above
        var sprints = await sprintQuery.ToListAsync(cancellationToken);
        
        // Ensure sprints are ordered by StartDate descending for consistent output
        sprints = sprints.OrderByDescending(s => s.StartDate ?? s.CreatedAt).ToList();

        if (!sprints.Any())
        {
            _logger.LogInformation("No sprints found for project {ProjectId}", request.ProjectId);
            return new SprintVelocityResponse(
                request.ProjectId,
                new List<SprintVelocityDto>(),
                0,
                0
            );
        }

        var sprintIds = sprints.Select(s => s.Id).ToList();

        // Get all tasks for these sprints in one query
        var allTasks = await taskRepo.Query()
            .AsNoTracking()
            .Where(t => sprintIds.Contains(t.SprintId ?? 0))
            .ToListAsync(cancellationToken);

        // Group tasks by sprint for efficient processing
        var tasksBySprint = allTasks
            .Where(t => t.SprintId.HasValue)
            .GroupBy(t => t.SprintId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var sprintVelocities = new List<SprintVelocityDto>();
        var totalCompletedStoryPoints = 0;

        // For each sprint, calculate velocity metrics
        foreach (var sprint in sprints)
        {
            var sprintTasks = tasksBySprint.GetValueOrDefault(sprint.Id, new List<ProjectTask>());

            // Filter out sprints with no tasks
            if (!sprintTasks.Any())
            {
                _logger.LogDebug("Sprint {SprintId} has no tasks, skipping", sprint.Id);
                continue;
            }

            // Calculate CompletedStoryPoints: sum of StoryPoints where Status == "Done"
            var completedStoryPoints = sprintTasks
                .Where(t => t.Status == TaskConstants.Statuses.Done)
                .Sum(t => t.StoryPoints?.Value ?? 0);

            // Calculate PlannedStoryPoints: sum of all StoryPoints in sprint
            var plannedStoryPoints = sprintTasks
                .Sum(t => t.StoryPoints?.Value ?? 0);

            // Count TotalTasks and CompletedTasks
            var totalTasks = sprintTasks.Count;
            var completedTasks = sprintTasks.Count(t => t.Status == TaskConstants.Statuses.Done);

            // Calculate CompletionRate: (CompletedTasks / TotalTasks) * 100
            var completionRate = totalTasks > 0
                ? Math.Round((decimal)completedTasks / totalTasks * 100, 2)
                : 0;

            sprintVelocities.Add(new SprintVelocityDto(
                sprint.Id,
                $"Sprint {sprint.Number}",
                sprint.StartDate?.DateTime ?? sprint.CreatedAt.DateTime,
                sprint.EndDate?.DateTime,
                completedStoryPoints,
                plannedStoryPoints,
                totalTasks,
                completedTasks,
                completionRate
            ));

            totalCompletedStoryPoints += completedStoryPoints;

            _logger.LogDebug(
                "Sprint {SprintId} ({SprintName}): Completed={CompletedSP}, Planned={PlannedSP}, Tasks={TotalTasks}/{CompletedTasks}, Rate={CompletionRate}%",
                sprint.Id,
                $"Sprint {sprint.Number}",
                completedStoryPoints,
                plannedStoryPoints,
                totalTasks,
                completedTasks,
                completionRate);
        }

        // Calculate AverageVelocity: average of CompletedStoryPoints across all fetched sprints
        var averageVelocity = sprintVelocities.Any()
            ? Math.Round((decimal)totalCompletedStoryPoints / sprintVelocities.Count, 2)
            : 0;

        _logger.LogInformation(
            "Calculated velocity for {SprintCount} sprint(s) in project {ProjectId}. Average velocity: {AverageVelocity}, Total completed: {TotalCompleted}",
            sprintVelocities.Count,
            request.ProjectId,
            averageVelocity,
            totalCompletedStoryPoints);

        return new SprintVelocityResponse(
            request.ProjectId,
            sprintVelocities,
            averageVelocity,
            totalCompletedStoryPoints
        );
    }
}

