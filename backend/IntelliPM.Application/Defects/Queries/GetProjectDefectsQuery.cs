using MediatR;

namespace IntelliPM.Application.Defects.Queries;

public record GetProjectDefectsQuery(
    int ProjectId, 
    string? Status = null,
    string? Severity = null,
    int? AssignedToId = null
) : IRequest<GetProjectDefectsResponse>;

public record GetProjectDefectsResponse(List<DefectDto> Defects, int Total);

public record DefectDto(
    int Id,
    string Title,
    string Description,
    string Severity,
    string Status,
    int? UserStoryId,
    string? UserStoryTitle,
    int? AssignedToId,
    string? AssignedToName,
    int? ReportedById,
    string? ReportedByName,
    string? FoundInEnvironment,
    DateTimeOffset ReportedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset UpdatedAt
);

