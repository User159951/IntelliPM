using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Domain.Entities;

public class Team : IAggregateRoot, ITenantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; } // Story points per sprint
    public int OrganizationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<ProjectTeam> AssignedProjects { get; set; } = new List<ProjectTeam>();
}

public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = TeamConstants.Roles.Member;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}

