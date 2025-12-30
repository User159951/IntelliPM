using MediatR;

namespace IntelliPM.Application.Defects.Queries;

public record GetDefectByIdQuery(int DefectId) : IRequest<DefectDetailDto>;

public record DefectDetailDto(
    int Id,
    string Title,
    string Description,
    string Severity,
    string Status,
    int ProjectId,
    int? UserStoryId,
    string? UserStoryTitle,
    int? SprintId,
    string? SprintName,
    int? ReportedById,
    string? ReportedByName,
    int? AssignedToId,
    string? AssignedToName,
    string? FoundInEnvironment,
    string? StepsToReproduce,
    string? Resolution,
    DateTimeOffset ReportedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset UpdatedAt
);
