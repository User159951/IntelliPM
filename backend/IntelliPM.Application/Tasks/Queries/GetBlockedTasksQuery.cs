using MediatR;

namespace IntelliPM.Application.Tasks.Queries;

public record GetBlockedTasksQuery(int ProjectId) : IRequest<GetBlockedTasksResponse>;

public record GetBlockedTasksResponse(List<TaskDto> Tasks);

