using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Handler for assigning an entire team to a project.
/// All team members are added as project members with appropriate roles.
/// </summary>
public class AssignTeamToProjectCommandHandler : IRequestHandler<AssignTeamToProjectCommand, AssignTeamToProjectResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly ILogger<AssignTeamToProjectCommandHandler> _logger;

    public AssignTeamToProjectCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMediator mediator,
        ILogger<AssignTeamToProjectCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AssignTeamToProjectResponse> Handle(AssignTeamToProjectCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        _logger.LogInformation(
            "User {UserId} attempting to assign team {TeamId} to project {ProjectId}",
            currentUserId,
            request.TeamId,
            request.ProjectId);

        // Verify project exists
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.Query()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");
        }

        // Verify team exists
        var teamRepo = _unitOfWork.Repository<Team>();
        var team = await teamRepo.Query()
            .Include(t => t.Members)
                .ThenInclude(tm => tm.User)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

        if (team == null)
        {
            _logger.LogWarning("Team {TeamId} not found", request.TeamId);
            throw new NotFoundException($"Team with ID {request.TeamId} not found");
        }

        // Verify project and team belong to same organization
        if (project.OrganizationId != team.OrganizationId)
        {
            _logger.LogWarning(
                "Project {ProjectId} (OrganizationId: {ProjectOrgId}) and Team {TeamId} (OrganizationId: {TeamOrgId}) belong to different organizations",
                request.ProjectId,
                project.OrganizationId,
                request.TeamId,
                team.OrganizationId);
            throw new ValidationException("Project and team must belong to the same organization");
        }

        // Verify current user's organization matches
        if (project.OrganizationId != organizationId)
        {
            _logger.LogWarning(
                "User {UserId} (OrganizationId: {UserOrgId}) attempted to assign team to project in different organization (OrganizationId: {ProjectOrgId})",
                currentUserId,
                organizationId,
                project.OrganizationId);
            throw new UnauthorizedException("You don't have access to this project");
        }

        // Get current user's role in the project
        var userRole = await _mediator.Send(
            new GetUserRoleInProjectQuery(request.ProjectId, currentUserId),
            cancellationToken);

        if (userRole == null)
        {
            _logger.LogWarning(
                "User {UserId} is not a member of project {ProjectId}",
                currentUserId,
                request.ProjectId);
            throw new UnauthorizedException("You are not a member of this project");
        }

        // Check if current user has permission to manage project members
        if (!ProjectPermissions.CanInviteMembers(userRole.Value))
        {
            _logger.LogWarning(
                "User {UserId} with role {Role} does not have permission to invite members to project {ProjectId}",
                currentUserId,
                userRole.Value,
                request.ProjectId);
            throw new UnauthorizedException("You don't have permission to assign teams to this project. Only Product Owners and Scrum Masters can assign teams.");
        }

        // Check if team is already assigned to project
        var projectTeamRepo = _unitOfWork.Repository<ProjectTeam>();
        var existingProjectTeam = await projectTeamRepo.Query()
            .FirstOrDefaultAsync(pt => pt.ProjectId == request.ProjectId && pt.TeamId == request.TeamId, cancellationToken);

        ProjectTeam? projectTeam = null;
        if (existingProjectTeam != null)
        {
            // Team already assigned - reactivate if inactive
            if (!existingProjectTeam.IsActive)
            {
                existingProjectTeam.IsActive = true;
                existingProjectTeam.UnassignedAt = null;
                existingProjectTeam.AssignedAt = DateTimeOffset.UtcNow; // Update assignment time
                existingProjectTeam.AssignedById = currentUserId;
                projectTeamRepo.Update(existingProjectTeam);
                projectTeam = existingProjectTeam;
                _logger.LogInformation(
                    "Reactivated existing ProjectTeam assignment for team {TeamId} to project {ProjectId}",
                    request.TeamId,
                    request.ProjectId);
            }
            else
            {
                projectTeam = existingProjectTeam;
                _logger.LogInformation(
                    "Team {TeamId} is already assigned to project {ProjectId}",
                    request.TeamId,
                    request.ProjectId);
            }
        }
        else
        {
            // Create new ProjectTeam entry
            projectTeam = new ProjectTeam
            {
                ProjectId = request.ProjectId,
                TeamId = request.TeamId,
                OrganizationId = project.OrganizationId,
                AssignedAt = DateTimeOffset.UtcNow,
                AssignedById = currentUserId,
                IsActive = true
            };

            await projectTeamRepo.AddAsync(projectTeam, cancellationToken);
            _logger.LogInformation(
                "Created ProjectTeam entry for team {TeamId} to project {ProjectId}",
                request.TeamId,
                request.ProjectId);
        }

        // Get existing project members for duplicate checking
        var existingMemberUserIds = project.Members.Select(m => m.UserId).ToHashSet();

        // Process each team member
        var assignedMembers = new List<AssignedMemberDto>();
        var projectMemberRepo = _unitOfWork.Repository<ProjectMember>();

        foreach (var teamMember in team.Members)
        {
            // Skip if user is already a project member
            if (existingMemberUserIds.Contains(teamMember.UserId))
            {
                // Get existing member's role
                var existingMember = project.Members.First(m => m.UserId == teamMember.UserId);
                assignedMembers.Add(new AssignedMemberDto(
                    teamMember.UserId,
                    teamMember.User.Username,
                    existingMember.Role,
                    AlreadyMember: true
                ));
                _logger.LogInformation(
                    "User {UserId} ({Username}) is already a member of project {ProjectId}, skipping",
                    teamMember.UserId,
                    teamMember.User.Username,
                    request.ProjectId);
                continue;
            }

            // Determine role: use override if exists, otherwise use default
            var role = request.MemberRoleOverrides?.ContainsKey(teamMember.UserId) == true
                ? request.MemberRoleOverrides[teamMember.UserId]
                : request.DefaultRole;

            // Create ProjectMember entry
            var projectMember = new ProjectMember
            {
                ProjectId = request.ProjectId,
                UserId = teamMember.UserId,
                Role = role,
                InvitedById = currentUserId,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = DateTimeOffset.UtcNow
            };

            await projectMemberRepo.AddAsync(projectMember, cancellationToken);
            existingMemberUserIds.Add(teamMember.UserId); // Track to avoid duplicates in same batch

            assignedMembers.Add(new AssignedMemberDto(
                teamMember.UserId,
                teamMember.User.Username,
                role,
                AlreadyMember: false
            ));

            _logger.LogInformation(
                "Added user {UserId} ({Username}) to project {ProjectId} with role {Role}",
                teamMember.UserId,
                teamMember.User.Username,
                request.ProjectId,
                role);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully assigned team {TeamId} to project {ProjectId}. {AssignedCount} members assigned, {SkippedCount} already members",
            request.TeamId,
            request.ProjectId,
            assignedMembers.Count(m => !m.AlreadyMember),
            assignedMembers.Count(m => m.AlreadyMember));

        return new AssignTeamToProjectResponse(
            request.ProjectId,
            request.TeamId,
            assignedMembers
        );
    }
}

