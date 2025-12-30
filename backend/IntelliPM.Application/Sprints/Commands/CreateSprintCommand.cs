using MediatR;
using IntelliPM.Application.Sprints.Queries;

namespace IntelliPM.Application.Sprints.Commands;

public record CreateSprintCommand(
    string Name,
    int ProjectId,
    int CurrentUserId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Capacity,
    string? Goal
) : IRequest<SprintDto>;
