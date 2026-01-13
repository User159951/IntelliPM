using MediatR;

namespace IntelliPM.Application.Sprints.Queries;

public record GetSprintByIdQuery(int SprintId) : IRequest<SprintDetailDto>;

public record SprintDetailDto(
    int Id,
    int ProjectId,
    string ProjectName,
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? Goal,
    string Status,
    List<SprintTaskDto> Tasks,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record SprintTaskDto(
    int Id,
    string Title,
    string Status,
    string Priority,
    int? StoryPoints
);

