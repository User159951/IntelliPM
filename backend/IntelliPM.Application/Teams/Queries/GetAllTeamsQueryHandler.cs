using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Teams.Queries;

public class GetAllTeamsQueryHandler : IRequestHandler<GetAllTeamsQuery, GetAllTeamsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTeamsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetAllTeamsResponse> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
    {
        var teamRepo = _unitOfWork.Repository<Team>();
        
        // Get all teams where the user is a member
        var teams = await teamRepo.Query()
            .AsNoTracking()
            .Where(t => t.Members.Any(m => m.UserId == request.UserId))
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Capacity,
                MemberCount = t.Members.Count,
                t.CreatedAt
            })
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var teamDtos = teams.Select(t => new TeamSummaryDto(
            t.Id,
            t.Name,
            t.Capacity,
            t.MemberCount,
            t.CreatedAt
        )).ToList();

        return new GetAllTeamsResponse(teamDtos);
    }
}

