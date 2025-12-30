using MediatR;

namespace IntelliPM.Application.Teams.Commands;

public record UpdateTeamCapacityCommand(
    int TeamId,
    int NewCapacity,
    int UpdatedBy
) : IRequest<UpdateTeamCapacityResponse>;

public record UpdateTeamCapacityResponse(
    int Id,
    int Capacity,
    DateTimeOffset UpdatedAt
);

