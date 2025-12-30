using MediatR;

namespace IntelliPM.Application.Teams.Queries;

public record GetTeamByIdQuery(int TeamId) : IRequest<TeamDto?>;
