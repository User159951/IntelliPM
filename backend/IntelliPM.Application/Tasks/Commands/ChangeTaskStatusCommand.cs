using MediatR;

namespace IntelliPM.Application.Tasks.Commands;

public record ChangeTaskStatusCommand(
    int TaskId,
    string NewStatus,
    int UpdatedBy
) : IRequest<ChangeTaskStatusResponse>;

public record ChangeTaskStatusResponse(
    int Id,
    string Status,
    DateTimeOffset UpdatedAt
);

