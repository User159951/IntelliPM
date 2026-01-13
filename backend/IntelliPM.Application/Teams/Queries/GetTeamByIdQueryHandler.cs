using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Teams.Queries;

public class GetTeamByIdQueryHandler : IRequestHandler<GetTeamByIdQuery, TeamDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeamByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TeamDto> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var teamRepo = _unitOfWork.Repository<Team>();
        
        var team = await teamRepo.Query()
            .Where(t => t.Id == request.TeamId)
            // Tenant filter automatically applied via global filter
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Select(t => new TeamDto(
                t.Id,
                t.Name,
                t.Capacity,
                t.Members.Select(m => new TeamMemberDto(
                    m.UserId,
                    m.User.Username,
                    m.User.Email,
                    m.User.FirstName,
                    m.User.LastName
                )).ToList(),
                t.CreatedAt,
                t.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (team == null)
            throw new NotFoundException($"Team not found");

        return team;
    }
}
