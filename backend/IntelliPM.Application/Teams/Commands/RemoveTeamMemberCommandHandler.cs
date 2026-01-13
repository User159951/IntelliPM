using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Teams.Queries;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Teams.Commands;

public class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, TeamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveTeamMemberCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<TeamDto> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedAccessException("Organization ID not found");
        }

        // Get team
        var teamRepo = _unitOfWork.Repository<Team>();
        var team = await teamRepo.Query()
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId && t.OrganizationId == organizationId, cancellationToken);

        if (team == null)
        {
            throw new InvalidOperationException($"Team with ID {request.TeamId} not found");
        }

        // Find member to remove
        var memberToRemove = team.Members.FirstOrDefault(m => m.UserId == request.UserId);
        if (memberToRemove == null)
        {
            throw new InvalidOperationException("User is not a member of this team");
        }

        // Prevent removing the last member
        if (team.Members.Count <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last member from a team");
        }

        // Remove member from team's collection (EF will track the deletion)
        team.Members.Remove(memberToRemove);
        team.UpdatedAt = DateTimeOffset.UtcNow;
        teamRepo.Update(team);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload team with members to return DTO
        var updatedTeam = await teamRepo.Query()
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstAsync(t => t.Id == team.Id, cancellationToken);

        return new TeamDto(
            updatedTeam.Id,
            updatedTeam.Name,
            updatedTeam.Capacity,
            updatedTeam.Members.Select(m => new TeamMemberDto(
                m.UserId,
                m.User.Username,
                m.User.Email,
                m.User.FirstName,
                m.User.LastName
            )).ToList(),
            updatedTeam.CreatedAt,
            updatedTeam.UpdatedAt
        );
    }
}
