using MediatR;

namespace IntelliPM.Application.Projects.Queries;

/// <summary>
/// Query to get all teams assigned to a project.
/// Returns teams that are currently active on the project.
/// </summary>
public record GetProjectAssignedTeamsQuery : IRequest<List<ProjectAssignedTeamDto>>
{
    /// <summary>
    /// The ID of the project to get assigned teams for.
    /// </summary>
    public int ProjectId { get; init; }
}

/// <summary>
/// DTO representing a team assigned to a project.
/// </summary>
public record ProjectAssignedTeamDto(
    int TeamId,
    string TeamName,
    string? TeamDescription,
    DateTimeOffset AssignedAt,
    int? AssignedById,
    string? AssignedByName,
    bool IsActive
);

