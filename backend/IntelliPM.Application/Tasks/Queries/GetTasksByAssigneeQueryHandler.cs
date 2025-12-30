using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Tasks.Queries;

public class GetTasksByAssigneeQueryHandler : IRequestHandler<GetTasksByAssigneeQuery, List<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTasksByAssigneeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TaskDto>> Handle(GetTasksByAssigneeQuery request, CancellationToken cancellationToken)
    {
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        
        var tasks = await taskRepo.Query()
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Sprint)
            .Include(t => t.CreatedBy)
            .Include(t => t.UpdatedBy)
            .Where(t => t.AssigneeId == request.AssigneeId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tasks.Select(t => new TaskDto(
            t.Id,
            t.ProjectId,
            t.Project.Name,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.StoryPoints?.Value,
            t.AssigneeId,
            t.Assignee != null ? $"{t.Assignee.FirstName} {t.Assignee.LastName}" : null,
            t.SprintId,
            t.Sprint != null ? $"Sprint {t.Sprint.Number}" : null,
            t.CreatedById,
            $"{t.CreatedBy.FirstName} {t.CreatedBy.LastName}",
            t.UpdatedById,
            t.UpdatedBy != null ? $"{t.UpdatedBy.FirstName} {t.UpdatedBy.LastName}" : null,
            t.CreatedAt,
            t.UpdatedAt
        )).ToList();
    }
}

