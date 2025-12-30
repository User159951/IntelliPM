using MediatR;

namespace IntelliPM.Application.Tasks.Queries;

public record GetTasksByProjectQuery(
    int ProjectId,
    string? Status = null,
    int? AssigneeId = null,
    string? Priority = null
) : IRequest<GetTasksByProjectResponse>;

public record GetTasksByProjectResponse(List<TaskDto> Tasks);
