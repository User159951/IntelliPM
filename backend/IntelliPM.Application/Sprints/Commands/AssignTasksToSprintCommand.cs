using MediatR;

namespace IntelliPM.Application.Sprints.Commands;

public record AssignTasksToSprintCommand(
    int SprintId,
    List<int> TaskIds,
    int UpdatedBy
) : IRequest<AssignTasksToSprintResponse>;

public record AssignTasksToSprintResponse(
    int SprintId,
    int AssignedCount
);

