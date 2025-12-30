using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Commands;

/// <summary>
/// Command to assign an entire team to a project.
/// All team members will be added as project members with the specified default role,
/// unless overridden by MemberRoleOverrides.
/// </summary>
public record AssignTeamToProjectCommand : IRequest<AssignTeamToProjectResponse>
{
    /// <summary>
    /// The ID of the project to assign the team to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// The ID of the team to assign to the project.
    /// </summary>
    public int TeamId { get; init; }

    /// <summary>
    /// Default role to assign to team members who don't have a role override.
    /// Defaults to Developer.
    /// </summary>
    public ProjectRole DefaultRole { get; init; } = ProjectRole.Developer;

    /// <summary>
    /// Optional dictionary mapping UserId to ProjectRole for role overrides.
    /// If a team member's UserId is in this dictionary, their role will be set to the specified value
    /// instead of DefaultRole.
    /// </summary>
    public Dictionary<int, ProjectRole>? MemberRoleOverrides { get; init; }
}

/// <summary>
/// Response containing information about the team assignment operation.
/// </summary>
public record AssignTeamToProjectResponse(
    int ProjectId,
    int TeamId,
    List<AssignedMemberDto> AssignedMembers
);

/// <summary>
/// DTO representing a team member that was assigned (or skipped) during team assignment.
/// </summary>
public record AssignedMemberDto(
    int UserId,
    string Username,
    ProjectRole Role,
    bool AlreadyMember
);

