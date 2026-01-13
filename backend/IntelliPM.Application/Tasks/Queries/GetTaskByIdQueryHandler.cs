using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Tasks.Queries;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaskByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        
        var task = await taskRepo.Query()
            .Where(t => t.Id == request.TaskId)
            // Tenant filter automatically applied via global filter
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Sprint)
            .Include(t => t.CreatedBy)
            .Include(t => t.UpdatedBy)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (task == null)
            throw new NotFoundException($"Task not found");

        return task;
    }
}
