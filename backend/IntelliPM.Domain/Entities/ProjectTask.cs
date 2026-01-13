using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.ValueObjects;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Simplified Task entity for direct project task management
/// (separate from Epic/Feature/UserStory hierarchy)
/// </summary>
public class ProjectTask : IAggregateRoot, ITenantEntity
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int OrganizationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = TaskConstants.Statuses.Todo;
    public string Priority { get; set; } = TaskConstants.Priorities.Medium;
    public StoryPoints? StoryPoints { get; set; }
    public int? AssigneeId { get; set; }
    public int? SprintId { get; set; }
    public int CreatedById { get; set; }
    public int? UpdatedById { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Project Project { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public User? Assignee { get; set; }
    public Sprint? Sprint { get; set; }
    public User CreatedBy { get; set; } = null!;
    public User? UpdatedBy { get; set; }
}

