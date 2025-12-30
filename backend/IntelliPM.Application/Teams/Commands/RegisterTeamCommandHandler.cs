using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Teams.Queries;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Teams.Commands;

public class RegisterTeamCommandHandler : IRequestHandler<RegisterTeamCommand, TeamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RegisterTeamCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<TeamDto> Handle(RegisterTeamCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Team name is required");

        if (request.MemberIds == null || !request.MemberIds.Any())
            throw new ArgumentException("Team must have at least one member");

        if (request.TotalCapacity <= 0)
            throw new ArgumentException("Total capacity must be greater than 0");

        // Get organization ID for multi-tenancy
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedAccessException("Organization ID not found");
        }

        // Verify all users exist and belong to the same organization
        var userRepo = _unitOfWork.Repository<User>();
        var existingUsers = await userRepo.Query()
            .Where(u => request.MemberIds.Contains(u.Id) && u.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        if (existingUsers.Count != request.MemberIds.Count)
        {
            var missingIds = request.MemberIds.Except(existingUsers.Select(u => u.Id)).ToList();
            throw new InvalidOperationException($"Users with IDs {string.Join(", ", missingIds)} not found or do not belong to your organization");
        }

        // Create team
        var team = new Team
        {
            Name = request.Name,
            Capacity = request.TotalCapacity,
            OrganizationId = organizationId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Add members
        foreach (var userId in request.MemberIds)
        {
            team.Members.Add(new TeamMember
            {
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTimeOffset.UtcNow
            });
        }

        var teamRepo = _unitOfWork.Repository<Team>();
        await teamRepo.AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload team with members to return DTO
        var createdTeam = await teamRepo.Query()
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstAsync(t => t.Id == team.Id, cancellationToken);

        return new TeamDto(
            createdTeam.Id,
            createdTeam.Name,
            createdTeam.Capacity,
            createdTeam.Members.Select(m => new TeamMemberDto(
                m.UserId,
                m.User.Username,
                m.User.Email,
                m.User.FirstName,
                m.User.LastName
            )).ToList(),
            createdTeam.CreatedAt,
            createdTeam.UpdatedAt
        );
    }
}

