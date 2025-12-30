using MediatR;
using IntelliPM.Application.Tasks.Queries;

namespace IntelliPM.Application.Tasks.Commands;

public record CreateTaskCommand(
    string Title,
    string Description,
    int ProjectId,
    string Priority,
    int? StoryPoints,
    int? AssigneeId,
    int CreatedById
) : IRequest<TaskDto>;
