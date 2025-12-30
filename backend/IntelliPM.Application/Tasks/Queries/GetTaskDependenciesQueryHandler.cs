using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Tasks.DTOs;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Tasks.Queries;

/// <summary>
/// Handler for retrieving all dependencies for a specific task.
/// Returns dependencies where the task is either the source or the dependent task.
/// </summary>
public class GetTaskDependenciesQueryHandler : IRequestHandler<GetTaskDependenciesQuery, List<TaskDependencyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTaskDependenciesQueryHandler> _logger;

    public GetTaskDependenciesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetTaskDependenciesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<TaskDependencyDto>> Handle(GetTaskDependenciesQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        if (organizationId == 0)
        {
            throw new Application.Common.Exceptions.UnauthorizedException("Organization ID not found");
        }

        // Get all dependencies where TaskId is either SourceTaskId or DependentTaskId
        var dependencies = await _unitOfWork.Repository<TaskDependency>()
            .Query()
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId &&
                       (d.SourceTaskId == request.TaskId || d.DependentTaskId == request.TaskId))
            .Include(d => d.SourceTask)
            .Include(d => d.DependentTask)
            .Include(d => d.CreatedBy)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = dependencies.Select(d =>
        {
            var createdByName = $"{d.CreatedBy.FirstName} {d.CreatedBy.LastName}".Trim();
            if (string.IsNullOrEmpty(createdByName))
            {
                createdByName = d.CreatedBy.Username;
            }

            return new TaskDependencyDto(
                d.Id,
                d.SourceTaskId,
                d.SourceTask.Title,
                d.DependentTaskId,
                d.DependentTask.Title,
                d.DependencyType.ToString(),
                d.CreatedAt,
                createdByName
            );
        }).ToList();

        _logger.LogInformation(
            "Retrieved {Count} dependencies for task {TaskId} in organization {OrganizationId}",
            result.Count, request.TaskId, organizationId);

        return result;
    }
}

