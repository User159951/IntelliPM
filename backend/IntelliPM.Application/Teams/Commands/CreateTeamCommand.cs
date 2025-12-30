using MediatR;

namespace IntelliPM.Application.Teams.Commands;

public record CreateTeamCommand(
    string Name,
    int Capacity,
    int CreatedBy
) : IRequest<CreateTeamResponse>;

public record CreateTeamResponse(
    int Id,
    string Name,
    int Capacity,
    DateTimeOffset CreatedAt
);

