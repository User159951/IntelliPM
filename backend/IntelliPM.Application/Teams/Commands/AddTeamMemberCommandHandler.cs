using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Teams.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Teams.Commands;

public class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand, TeamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddTeamMemberCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<TeamDto> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
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

        // Check if user already exists in team
        if (team.Members.Any(m => m.UserId == request.UserId))
        {
            throw new InvalidOperationException("User is already a member of this team");
        }

        // Verify user exists and belongs to the same organization
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.OrganizationId == organizationId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found or does not belong to your organization");
        }

        // Add member
        team.Members.Add(new TeamMember
        {
            UserId = request.UserId,
            Role = TeamConstants.Roles.Member,
            JoinedAt = DateTimeOffset.UtcNow
        });

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
