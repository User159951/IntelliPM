using MediatR;

namespace IntelliPM.Application.Projects.Commands;

public record UpdateProjectCommand(
    int ProjectId,
    int CurrentUserId,
    string? Name = null,
    string? Description = null,
    string? Status = null,
    string? Type = null,
    int? SprintDurationDays = null
) : IRequest<UpdateProjectResponse>;

public record UpdateProjectResponse(
    int Id,
    string Name,
    string Description,
    string Type,
    string Status,
    int SprintDurationDays,
    DateTimeOffset UpdatedAt
);

