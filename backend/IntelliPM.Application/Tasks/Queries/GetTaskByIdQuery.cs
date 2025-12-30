using MediatR;

namespace IntelliPM.Application.Tasks.Queries;

public record GetTaskByIdQuery(int TaskId) : IRequest<TaskDto?>;
