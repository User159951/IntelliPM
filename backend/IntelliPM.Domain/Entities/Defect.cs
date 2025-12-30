using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class Defect : IAggregateRoot
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int OrganizationId { get; set; }
    public int? UserStoryId { get; set; }
    public int? SprintId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium"; // Low | Medium | High | Critical
    public string Status { get; set; } = "Open"; // Open | InProgress | Resolved | Closed | Reopened
    public int? ReportedById { get; set; }
    public int? AssignedToId { get; set; }
    public string? FoundInEnvironment { get; set; }
    public string? StepsToReproduce { get; set; }
    public string? Resolution { get; set; }
    public DateTimeOffset ReportedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Navigation
    public Project Project { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public UserStory? UserStory { get; set; }
    public Sprint? Sprint { get; set; }
    public User? ReportedBy { get; set; }
    public User? AssignedTo { get; set; }
}

