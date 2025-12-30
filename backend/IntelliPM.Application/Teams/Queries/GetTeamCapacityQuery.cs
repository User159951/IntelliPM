using MediatR;

namespace IntelliPM.Application.Teams.Queries;

public record GetTeamCapacityQuery(int TeamId) : IRequest<TeamCapacityDto>;

public record TeamCapacityDto(
    int TeamId,
    string TeamName,
    int TotalCapacity,
    int AssignedStoryPoints,
    int AvailableCapacity,
    int? ActiveSprintId,
    string? ActiveSprintName
);

