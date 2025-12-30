using MediatR;

namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// Command to add tasks to a sprint with automatic velocity calculation.
/// </summary>
public record AddTaskToSprintCommand : IRequest<AddTaskToSprintResponse>
{
    public int SprintId { get; init; }
    public List<int> TaskIds { get; init; } = new();
    public bool IgnoreCapacityWarning { get; init; } = false;
}

/// <summary>
/// Response containing sprint details and capacity information after adding tasks.
/// </summary>
public record AddTaskToSprintResponse(
    int SprintId,
    string SprintName,
    List<TaskAddedDto> AddedTasks,
    SprintCapacityDto Capacity,
    bool IsOverCapacity,
    string? CapacityWarning
);

/// <summary>
/// DTO for a task that was added to the sprint.
/// </summary>
public record TaskAddedDto(
    int TaskId,
    string Title,
    int? StoryPoints,
    bool WasAlreadyInSprint
);


