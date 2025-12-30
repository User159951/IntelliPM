using MediatR;

namespace IntelliPM.Application.Sprints.Queries;

public record GetSprintsByProjectQuery(int ProjectId) : IRequest<GetSprintsByProjectResponse>;

public record GetSprintsByProjectResponse(List<SprintListDto> Sprints);

public record SprintListDto(
    int Id,
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? Goal,
    string Status,
    int TaskCount,
    DateTimeOffset CreatedAt
);

