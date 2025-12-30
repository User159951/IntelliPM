using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Tasks.Queries;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, GetTasksByProjectResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTasksByProjectQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetTasksByProjectResponse> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        
        var query = taskRepo.Query()
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Sprint)
            .Include(t => t.CreatedBy)
            .Include(t => t.UpdatedBy)
            .Where(t => t.ProjectId == request.ProjectId);

        // Apply optional filters
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(t => t.Status == request.Status);
        }

        if (request.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == request.AssigneeId.Value);
        }

        if (!string.IsNullOrEmpty(request.Priority))
        {
            query = query.Where(t => t.Priority == request.Priority);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskDto(
                t.Id,
                t.ProjectId,
                t.Project.Name,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.StoryPoints != null ? t.StoryPoints.Value : null,
                t.AssigneeId,
                t.Assignee != null ? t.Assignee.Username : null,
                t.SprintId,
                t.Sprint != null ? $"Sprint {t.Sprint.Number}" : null,
                t.CreatedById,
                t.CreatedBy.Username,
                t.UpdatedById,
                t.UpdatedBy != null ? t.UpdatedBy.Username : null,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new GetTasksByProjectResponse(tasks);
    }
}
