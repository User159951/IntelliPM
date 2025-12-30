namespace IntelliPM.Application.Tasks.Queries;

public record TaskDto(
    int Id,
    int ProjectId,
    string ProjectName,
    string Title,
    string Description,
    string Status,
    string Priority,
    int? StoryPoints,
    int? AssigneeId,
    string? AssigneeName,
    int? SprintId,
    string? SprintName,
    int CreatedById,
    string CreatedByName,
    int? UpdatedById,
    string? UpdatedByName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

