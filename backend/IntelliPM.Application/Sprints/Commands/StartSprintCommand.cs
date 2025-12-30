using MediatR;

namespace IntelliPM.Application.Sprints.Commands;

public record StartSprintCommand(
    int SprintId,
    int UpdatedBy
) : IRequest<StartSprintResponse>;

public record StartSprintResponse(
    int Id,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset UpdatedAt
);

