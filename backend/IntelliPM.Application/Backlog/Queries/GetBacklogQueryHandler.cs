using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Backlog.Queries;

/// <summary>
/// Handler for retrieving backlog tasks (unassigned tasks) sorted by priority.
/// </summary>
public class GetBacklogQueryHandler : IRequestHandler<GetBacklogQuery, PagedResponse<BacklogTaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetBacklogQueryHandler> _logger;

    public GetBacklogQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetBacklogQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResponse<BacklogTaskDto>> Handle(GetBacklogQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        var taskRepo = _unitOfWork.Repository<ProjectTask>();

        // Build IQueryable<ProjectTask> with filters
        var query = taskRepo.Query()
            .AsNoTracking()
            .Include(t => t.Assignee)
            .Where(t => t.ProjectId == request.ProjectId)
            .Where(t => t.SprintId == null) // Backlog tasks only
            .Where(t => t.OrganizationId == organizationId); // Multi-tenancy check

        // Apply optional filters
        if (!string.IsNullOrEmpty(request.Priority))
        {
            query = query.Where(t => t.Priority == request.Priority);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(t => t.Status == request.Status);
        }

        // Apply search term filter (case-insensitive)
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(searchTermLower) ||
                (t.Description != null && t.Description.ToLower().Contains(searchTermLower)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Order by priority using conditional logic to map priority to sort order
        // Critical=1, High=2, Medium=3, Low=4, Default=5
        // Then order by CreatedAt ascending (oldest first)
        var orderedQuery = query
            .OrderBy(t => t.Priority == TaskConstants.Priorities.Critical ? 1 :
                          t.Priority == TaskConstants.Priorities.High ? 2 :
                          t.Priority == TaskConstants.Priorities.Medium ? 3 :
                          t.Priority == TaskConstants.Priorities.Low ? 4 : 5)
            .ThenBy(t => t.CreatedAt);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100)); // Limit page size to 100

        var tasks = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new BacklogTaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.Status,
                t.StoryPoints != null ? t.StoryPoints.Value : null,
                t.AssigneeId,
                t.Assignee != null ? $"{t.Assignee.FirstName} {t.Assignee.LastName}".Trim() : null,
                t.CreatedAt,
                t.Priority == TaskConstants.Priorities.Critical ? 1 :
                t.Priority == TaskConstants.Priorities.High ? 2 :
                t.Priority == TaskConstants.Priorities.Medium ? 3 :
                t.Priority == TaskConstants.Priorities.Low ? 4 : 5
            ))
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} backlog tasks for project {ProjectId} (Page {Page}, PageSize {PageSize}, Total: {TotalCount})",
            tasks.Count,
            request.ProjectId,
            page,
            pageSize,
            totalCount);

        return new PagedResponse<BacklogTaskDto>(tasks, page, pageSize, totalCount);
    }
}

