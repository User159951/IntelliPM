using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskEntity = IntelliPM.Domain.Entities.Task;

namespace IntelliPM.Application.Projections;

/// <summary>
/// Projection handler that updates TaskBoardReadModel when task-related domain events occur.
/// Implements eventual consistency pattern - handlers are idempotent and safe to retry.
/// </summary>
public class TaskBoardProjectionHandler :
    INotificationHandler<TaskCreatedEvent>,
    INotificationHandler<TaskUpdatedEvent>,
    INotificationHandler<TaskDeletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskBoardProjectionHandler> _logger;

    public TaskBoardProjectionHandler(
        IUnitOfWork unitOfWork,
        ILogger<TaskBoardProjectionHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task Handle(TaskCreatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating TaskBoardReadModel for task created: {TaskId}", notification.TaskId);

            // Get or create read model
            var readModel = await GetOrCreateTaskBoardReadModelAsync(notification.ProjectId, ct);

            // Fetch task with assignee
            var task = await _unitOfWork.Repository<ProjectTask>()
                .Query()
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == notification.TaskId, ct);

            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found when processing TaskCreatedEvent", notification.TaskId);
                return;
            }

            // Create task summary
            var taskSummary = new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                Priority = task.Priority,
                StoryPoints = task.StoryPoints?.Value,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee != null
                    ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim()
                    : null,
                AssigneeAvatar = null, // Avatar not stored in User entity currently
                DueDate = null, // DueDate not in ProjectTask entity currently
                DisplayOrder = 0 // Will be recalculated if needed
            };

            // Add to appropriate list based on status
            // Note: TaskBoardReadModel only tracks Todo, InProgress, Done
            // Other statuses (Review, Blocked) are not included in the board
            var status = task.Status;
            if (status == "Todo" || status == "Review" || status == "Blocked")
            {
                var todoTasks = readModel.GetTodoTasks();
                todoTasks.Add(taskSummary);
                readModel.SetTodoTasks(todoTasks);
            }
            else if (status == "InProgress")
            {
                var inProgressTasks = readModel.GetInProgressTasks();
                inProgressTasks.Add(taskSummary);
                readModel.SetInProgressTasks(inProgressTasks);
            }
            else if (status == "Done")
            {
                var doneTasks = readModel.GetDoneTasks();
                doneTasks.Add(taskSummary);
                readModel.SetDoneTasks(doneTasks);
            }

            readModel.UpdateCounts();
            readModel.Version++;
            readModel.LastUpdated = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("TaskBoardReadModel updated successfully for task: {TaskId}", notification.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TaskBoardReadModel for task created: {TaskId}", notification.TaskId);
            // Don't throw - eventual consistency allows retry via OutboxProcessor
        }
    }

    public async System.Threading.Tasks.Task Handle(TaskUpdatedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating TaskBoardReadModel for task updated: {TaskId}", notification.TaskId);

            var readModel = await GetOrCreateTaskBoardReadModelAsync(notification.ProjectId, ct);

            // Fetch updated task
            var task = await _unitOfWork.Repository<ProjectTask>()
                .Query()
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == notification.TaskId, ct);

            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found when processing TaskUpdatedEvent", notification.TaskId);
                return;
            }

            // If status changed, move task between lists
            if (!string.IsNullOrEmpty(notification.OldStatus) && 
                !string.IsNullOrEmpty(notification.NewStatus) &&
                notification.OldStatus != notification.NewStatus)
            {
                // Remove from old status list
                RemoveTaskFromStatus(readModel, notification.TaskId, notification.OldStatus);

                // Add to new status list
                var taskSummary = CreateTaskSummary(task);
                AddTaskToStatus(readModel, taskSummary, notification.NewStatus);
            }
            else
            {
                // Update task in current list (status didn't change, but other properties might have)
                UpdateTaskInStatus(readModel, task, task.Status);
            }

            readModel.UpdateCounts();
            readModel.Version++;
            readModel.LastUpdated = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("TaskBoardReadModel updated successfully for task: {TaskId}", notification.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TaskBoardReadModel for task updated: {TaskId}", notification.TaskId);
            // Don't throw - eventual consistency allows retry
        }
    }

    public async System.Threading.Tasks.Task Handle(TaskDeletedEvent notification, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating TaskBoardReadModel for task deleted: {TaskId}", notification.TaskId);

            var readModel = await _unitOfWork.Repository<TaskBoardReadModel>()
                .Query()
                .FirstOrDefaultAsync(r => r.ProjectId == notification.ProjectId, ct);

            if (readModel == null)
            {
                _logger.LogWarning("TaskBoardReadModel for project {ProjectId} not found when processing TaskDeletedEvent", notification.ProjectId);
                return;
            }

            // Remove from all lists (idempotent - safe to call multiple times)
            RemoveTaskFromAllLists(readModel, notification.TaskId);

            readModel.UpdateCounts();
            readModel.Version++;
            readModel.LastUpdated = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("TaskBoardReadModel updated successfully for deleted task: {TaskId}", notification.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TaskBoardReadModel for task deleted: {TaskId}", notification.TaskId);
            // Don't throw - eventual consistency allows retry
        }
    }

    private async System.Threading.Tasks.Task<TaskBoardReadModel> GetOrCreateTaskBoardReadModelAsync(int projectId, CancellationToken ct)
    {
        var readModel = await _unitOfWork.Repository<TaskBoardReadModel>()
            .Query()
            .FirstOrDefaultAsync(r => r.ProjectId == projectId, ct);

        if (readModel == null)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetByIdAsync(projectId, ct);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found");
            }

            readModel = new TaskBoardReadModel
            {
                ProjectId = projectId,
                OrganizationId = project.OrganizationId,
                LastUpdated = DateTimeOffset.UtcNow,
                Version = 1
            };

            await _unitOfWork.Repository<TaskBoardReadModel>().AddAsync(readModel, ct);
        }

        return readModel;
    }

    private void RemoveTaskFromStatus(TaskBoardReadModel readModel, int taskId, string? status)
    {
        switch (status)
        {
            case "Todo":
            case "Review":
            case "Blocked":
                var todoTasks = readModel.GetTodoTasks();
                todoTasks.RemoveAll(t => t.Id == taskId);
                readModel.SetTodoTasks(todoTasks);
                break;
            case "InProgress":
                var inProgressTasks = readModel.GetInProgressTasks();
                inProgressTasks.RemoveAll(t => t.Id == taskId);
                readModel.SetInProgressTasks(inProgressTasks);
                break;
            case "Done":
                var doneTasks = readModel.GetDoneTasks();
                doneTasks.RemoveAll(t => t.Id == taskId);
                readModel.SetDoneTasks(doneTasks);
                break;
        }
    }

    private void AddTaskToStatus(TaskBoardReadModel readModel, TaskSummaryDto taskSummary, string? status)
    {
        switch (status)
        {
            case "Todo":
            case "Review":
            case "Blocked":
                var todoTasks = readModel.GetTodoTasks();
                todoTasks.Add(taskSummary);
                readModel.SetTodoTasks(todoTasks);
                break;
            case "InProgress":
                var inProgressTasks = readModel.GetInProgressTasks();
                inProgressTasks.Add(taskSummary);
                readModel.SetInProgressTasks(inProgressTasks);
                break;
            case "Done":
                var doneTasks = readModel.GetDoneTasks();
                doneTasks.Add(taskSummary);
                readModel.SetDoneTasks(doneTasks);
                break;
        }
    }

    private void RemoveTaskFromAllLists(TaskBoardReadModel readModel, int taskId)
    {
        // Remove from all possible status lists (idempotent)
        RemoveTaskFromStatus(readModel, taskId, "Todo");
        RemoveTaskFromStatus(readModel, taskId, "InProgress");
        RemoveTaskFromStatus(readModel, taskId, "Done");
    }

    private TaskSummaryDto CreateTaskSummary(ProjectTask task)
    {
        return new TaskSummaryDto
        {
            Id = task.Id,
            Title = task.Title,
            Priority = task.Priority,
            StoryPoints = task.StoryPoints?.Value,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee != null
                ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim()
                : null,
            AssigneeAvatar = null, // Avatar not stored in User entity currently
            DueDate = null, // DueDate not in ProjectTask entity currently
            DisplayOrder = 0
        };
    }

    private void UpdateTaskInStatus(TaskBoardReadModel readModel, ProjectTask task, string? status)
    {
        var tasks = status switch
        {
            "Todo" or "Review" or "Blocked" => readModel.GetTodoTasks(),
            "InProgress" => readModel.GetInProgressTasks(),
            "Done" => readModel.GetDoneTasks(),
            _ => new List<TaskSummaryDto>()
        };

        var existingTask = tasks.FirstOrDefault(t => t.Id == task.Id);
        if (existingTask != null)
        {
            tasks.Remove(existingTask);
            tasks.Add(CreateTaskSummary(task));

            switch (status)
            {
                case "Todo":
                case "Review":
                case "Blocked":
                    readModel.SetTodoTasks(tasks);
                    break;
                case "InProgress":
                    readModel.SetInProgressTasks(tasks);
                    break;
                case "Done":
                    readModel.SetDoneTasks(tasks);
                    break;
            }
        }
    }
}

