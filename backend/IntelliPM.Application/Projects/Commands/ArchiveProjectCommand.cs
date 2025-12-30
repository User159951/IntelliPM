using MediatR;

namespace IntelliPM.Application.Projects.Commands;

public record ArchiveProjectCommand(
    int ProjectId,
    int CurrentUserId
) : IRequest<ArchiveProjectResponse>;

public record ArchiveProjectResponse(
    int Id,
    string Name,
    string Status,
    DateTimeOffset ArchivedAt
);

