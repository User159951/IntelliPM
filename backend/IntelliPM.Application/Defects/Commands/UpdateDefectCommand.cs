using MediatR;

namespace IntelliPM.Application.Defects.Commands;

public record UpdateDefectCommand(
    int DefectId,
    int UpdatedBy,
    string? Title = null,
    string? Description = null,
    string? Severity = null,
    string? Status = null,
    int? AssignedToId = null,
    string? FoundInEnvironment = null,
    string? StepsToReproduce = null,
    string? Resolution = null
) : IRequest<UpdateDefectResponse>;

public record UpdateDefectResponse(
    int Id,
    string Title,
    string Severity,
    string Status,
    DateTimeOffset UpdatedAt
);
