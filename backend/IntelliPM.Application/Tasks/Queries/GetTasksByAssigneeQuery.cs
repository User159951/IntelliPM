using MediatR;
using IntelliPM.Application.Tasks.Queries;

namespace IntelliPM.Application.Tasks.Queries;

/// <summary>
/// Query to get all tasks assigned to a specific user
/// </summary>
public record GetTasksByAssigneeQuery(int AssigneeId) : IRequest<List<TaskDto>>;

