using MediatR;

namespace IntelliPM.Application.Teams.Queries;

public record GetAllTeamsQuery(int UserId) : IRequest<GetAllTeamsResponse>;

public record GetAllTeamsResponse(List<TeamSummaryDto> Teams);

public record TeamSummaryDto(
    int Id,
    string Name,
    int Capacity,
    int MemberCount,
    DateTimeOffset CreatedAt
);

