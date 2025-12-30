using MediatR;

namespace IntelliPM.Application.Projects.Commands;

public record DeleteProjectCommand(
    int ProjectId,
    int CurrentUserId
) : IRequest;
