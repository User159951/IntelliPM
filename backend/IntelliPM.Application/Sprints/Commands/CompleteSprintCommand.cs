using MediatR;

namespace IntelliPM.Application.Sprints.Commands;

public record CompleteSprintCommand(
    int SprintId,
    int UpdatedBy,
    string? IncompleteTasksAction = null // "next_sprint" | "backlog" | "keep"
) : IRequest<CompleteSprintResponse>;

public record CompleteSprintResponse(
    int Id,
    string Status,
    DateTimeOffset EndDate,
    DateTimeOffset UpdatedAt,
    int CompletedTasksCount,
    int TotalTasksCount,
    int Velocity,
    decimal CompletionRate
);

