using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Domain.Entities;

public class Project : IAggregateRoot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = ProjectConstants.Types.Scrum;
    public int SprintDurationDays { get; set; } = 14;
    public int OwnerId { get; set; }
    public int OrganizationId { get; set; }
    public string Status { get; set; } = ProjectConstants.Statuses.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; }
    
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public User Owner { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Epic> Epics { get; set; } = new List<Epic>();
    public ICollection<Feature> Features { get; set; } = new List<Feature>();
    public ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<Risk> Risks { get; set; } = new List<Risk>();
    public ICollection<AIAgentRun> AgentRuns { get; set; } = new List<AIAgentRun>();
    public ICollection<AIDecision> Decisions { get; set; } = new List<AIDecision>();
    public ICollection<DocumentStore> Documents { get; set; } = new List<DocumentStore>();
    public ICollection<ProjectTeam> AssignedTeams { get; set; } = new List<ProjectTeam>();
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public ICollection<Release> Releases { get; set; } = new List<Release>();
}

public class ProjectMember
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public ProjectRole Role { get; set; }
    public int InvitedById { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
    public User InvitedBy { get; set; } = null!;
}

