using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Tasks.DTOs;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Tasks.Queries;

/// <summary>
/// Handler for retrieving the complete dependency graph for a project.
/// Builds a graph structure with nodes (tasks) and edges (dependencies) for visualization.
/// </summary>
public class GetProjectDependencyGraphQueryHandler : IRequestHandler<GetProjectDependencyGraphQuery, DependencyGraphDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetProjectDependencyGraphQueryHandler> _logger;

    public GetProjectDependencyGraphQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetProjectDependencyGraphQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DependencyGraphDto> Handle(GetProjectDependencyGraphQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new Application.Common.Exceptions.UnauthorizedException("Organization ID not found");
        }

        // Get all tasks in the project
        var tasks = await _unitOfWork.Repository<ProjectTask>()
            .Query()
            .AsNoTracking()
            .Where(t => t.ProjectId == request.ProjectId && t.OrganizationId == organizationId)
            .Include(t => t.Assignee)
            .ToListAsync(cancellationToken);

        if (!tasks.Any())
        {
            _logger.LogInformation("No tasks found for project {ProjectId}", request.ProjectId);
            return new DependencyGraphDto(new List<DependencyGraphNodeDto>(), new List<DependencyGraphEdgeDto>());
        }

        var taskIds = tasks.Select(t => t.Id).ToList();

        // Get all dependencies for tasks in this project
        var dependencies = await _unitOfWork.Repository<TaskDependency>()
            .Query()
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId &&
                       taskIds.Contains(d.SourceTaskId) &&
                       taskIds.Contains(d.DependentTaskId))
            .ToListAsync(cancellationToken);

        // Build nodes (tasks)
        var nodes = tasks.Select(t =>
        {
            var assigneeName = t.Assignee != null
                ? $"{t.Assignee.FirstName} {t.Assignee.LastName}".Trim()
                : null;
            if (string.IsNullOrEmpty(assigneeName) && t.Assignee != null)
            {
                assigneeName = t.Assignee.Username;
            }

            return new DependencyGraphNodeDto(
                t.Id,
                t.Title,
                t.Status,
                t.AssigneeId,
                assigneeName
            );
        }).ToList();

        // Build edges (dependencies)
        var edges = dependencies.Select(d =>
        {
            // Create short label for dependency type
            var label = d.DependencyType.ToString() switch
            {
                "FinishToStart" => "FS",
                "StartToStart" => "SS",
                "FinishToFinish" => "FF",
                "StartToFinish" => "SF",
                _ => d.DependencyType.ToString()
            };

            return new DependencyGraphEdgeDto(
                d.Id,
                d.SourceTaskId,
                d.DependentTaskId,
                d.DependencyType.ToString(),
                label
            );
        }).ToList();

        _logger.LogInformation(
            "Retrieved dependency graph for project {ProjectId}: {NodeCount} nodes, {EdgeCount} edges",
            request.ProjectId, nodes.Count, edges.Count);

        return new DependencyGraphDto(nodes, edges);
    }
}

