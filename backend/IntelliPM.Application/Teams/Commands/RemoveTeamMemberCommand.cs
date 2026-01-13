using MediatR;
using IntelliPM.Application.Teams.Queries;

namespace IntelliPM.Application.Teams.Commands;

public record RemoveTeamMemberCommand(
    int TeamId,
    int UserId,
    int RequestedByUserId
) : IRequest<TeamDto>;
