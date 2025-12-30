using MediatR;
using IntelliPM.Application.Tasks.Queries;

namespace IntelliPM.Application.Tasks.Commands;

public record UpdateTaskCommand(
    int TaskId,
    string? Title = null,
    string? Description = null,
    string? Priority = null,
    int? StoryPoints = null,
    int UpdatedById = 0
) : IRequest<TaskDto>;
