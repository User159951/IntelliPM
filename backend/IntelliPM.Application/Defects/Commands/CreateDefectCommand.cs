using MediatR;

namespace IntelliPM.Application.Defects.Commands;

public record CreateDefectCommand(
    int ProjectId,
    int? UserStoryId,
    int? SprintId,
    string Title,
    string Description,
    string Severity,
    int ReportedById,
    string? FoundInEnvironment,
    string? StepsToReproduce,
    int? AssignedToId
) : IRequest<CreateDefectResponse>;

public record CreateDefectResponse(int Id, string Title, string Severity, string Status);

