namespace IntelliPM.Application.Sprints.Queries;

public record SprintDto(
    int Id,
    int ProjectId,
    string ProjectName,
    int Number,
    string Goal,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string Status,
    int TaskCount,
    DateTimeOffset CreatedAt
);

