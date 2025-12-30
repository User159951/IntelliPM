using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Sprints.Commands;

public class AssignTasksToSprintCommandHandler : IRequestHandler<AssignTasksToSprintCommand, AssignTasksToSprintResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignTasksToSprintCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AssignTasksToSprintResponse> Handle(AssignTasksToSprintCommand request, CancellationToken cancellationToken)
    {
        // Verify sprint exists
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var sprint = await sprintRepo.GetByIdAsync(request.SprintId, cancellationToken);
        
        if (sprint == null)
            throw new InvalidOperationException($"Sprint with ID {request.SprintId} not found");

        // Cannot add tasks to completed sprint
        if (sprint.Status == "Completed")
            throw new InvalidOperationException($"Cannot add tasks to a completed sprint");

        // Get tasks and assign them to the sprint
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var tasks = await taskRepo.Query()
            .Where(t => request.TaskIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        if (tasks.Count == 0)
            throw new ArgumentException("No valid tasks found to assign");

        int assignedCount = 0;
        var errors = new List<string>();

        foreach (var task in tasks)
        {
            // Verify task belongs to the same project
            if (task.ProjectId != sprint.ProjectId)
            {
                errors.Add($"Task {task.Id} belongs to a different project");
                continue;
            }

            // Verify task is not already in another sprint
            if (task.SprintId.HasValue && task.SprintId.Value != request.SprintId)
            {
                errors.Add($"Task {task.Id} is already assigned to another sprint");
                continue;
            }

            // Assign task to sprint
            task.SprintId = request.SprintId;
            task.UpdatedAt = DateTimeOffset.UtcNow;
            task.UpdatedById = request.UpdatedBy;
            taskRepo.Update(task);
            assignedCount++;
        }

        if (errors.Any() && assignedCount == 0)
            throw new ArgumentException(string.Join("; ", errors));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignTasksToSprintResponse(request.SprintId, assignedCount);
    }
}

