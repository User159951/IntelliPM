using MediatR;
using IntelliPM.Application.Teams.Queries;

namespace IntelliPM.Application.Teams.Commands;

public record RegisterTeamCommand(
    string Name,
    List<int> MemberIds,
    int TotalCapacity
) : IRequest<TeamDto>;

