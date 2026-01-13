using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public abstract class BacklogItem : IAggregateRoot, ITenantEntity
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; } = 50; // 0-100
    public string Status { get; set; } = "Backlog";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}

public class Epic : BacklogItem
{
    public ICollection<Feature> Features { get; set; } = new List<Feature>();
}

public class Feature : BacklogItem
{
    public int? EpicId { get; set; }
    public int? StoryPoints { get; set; }
    public string? DomainTag { get; set; }

    public Epic? Epic { get; set; }
    public ICollection<UserStory> Stories { get; set; } = new List<UserStory>();
}

public class UserStory : BacklogItem
{
    public int? FeatureId { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public int? StoryPoints { get; set; }
    public string? DomainTag { get; set; }

    public Feature? Feature { get; set; }
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
    public ICollection<SprintItem> SprintItems { get; set; } = new List<SprintItem>();
}

public class Task
{
    public int Id { get; set; }
    public int UserStoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "TODO";
    public int? AssigneeId { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; }

    public UserStory UserStory { get; set; } = null!;
    public User? Assignee { get; set; }
}

