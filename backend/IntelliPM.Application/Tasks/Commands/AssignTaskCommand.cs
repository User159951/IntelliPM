using MediatR;

namespace IntelliPM.Application.Tasks.Commands;

public record AssignTaskCommand(
    int TaskId,
    int? AssigneeId,
    int UpdatedBy
) : IRequest<AssignTaskResponse>;

public record AssignTaskResponse(
    int Id,
    int? AssigneeId,
    DateTimeOffset UpdatedAt
);

