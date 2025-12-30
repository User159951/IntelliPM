using MediatR;

namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// Command to remove tasks from a sprint and return them to the backlog.
/// </summary>
public record RemoveTaskFromSprintCommand : IRequest<RemoveTaskFromSprintResponse>
{
    public int SprintId { get; init; }
    public List<int> TaskIds { get; init; } = new();
}

/// <summary>
/// Response containing sprint details and updated capacity information after removing tasks.
/// </summary>
public record RemoveTaskFromSprintResponse(
    int SprintId,
    string SprintName,
    List<TaskRemovedDto> RemovedTasks,
    SprintCapacityDto UpdatedCapacity
);

/// <summary>
/// DTO for a task that was removed from the sprint.
/// </summary>
public record TaskRemovedDto(
    int TaskId,
    string Title,
    int? StoryPoints,
    bool WasInSprint
);

