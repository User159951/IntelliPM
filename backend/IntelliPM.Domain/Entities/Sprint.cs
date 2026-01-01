using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Domain.Entities;

public class Sprint : IAggregateRoot
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int OrganizationId { get; set; }
    public int Number { get; set; }
    public string Goal { get; set; } = string.Empty;
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string Status { get; set; } = SprintConstants.Statuses.NotStarted;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// ID of the release this sprint is included in.
    /// Null if the sprint is not part of any release yet.
    /// </summary>
    public int? ReleaseId { get; set; }
    
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// AI-generated retrospective notes for the sprint (generated when sprint is completed).
    /// </summary>
    public string? RetrospectiveNotes { get; set; }

    public Project Project { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    /// <summary>
    /// Release this sprint is included in.
    /// Null if the sprint is not part of any release yet.
    /// </summary>
    public Release? Release { get; set; }
    public ICollection<SprintItem> Items { get; set; } = new List<SprintItem>();
    public ICollection<KPISnapshot> KPISnapshots { get; set; } = new List<KPISnapshot>();
}

public class SprintItem
{
    public int Id { get; set; }
    public int SprintId { get; set; }
    public int UserStoryId { get; set; }
    public int? SnapshotStoryPoints { get; set; }
    public string Status { get; set; } = "TODO"; // TODO | InProgress | Review | Done

    public Sprint Sprint { get; set; } = null!;
    public UserStory UserStory { get; set; } = null!;
}

public class KPISnapshot
{
    public int Id { get; set; }
    public int SprintId { get; set; }
    public int? VelocityPoints { get; set; }
    public int? CompletedPoints { get; set; }
    public int DefectCount { get; set; } = 0;
    public decimal? LeadTimeDays { get; set; }
    public decimal? CycleTimeDays { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Sprint Sprint { get; set; } = null!;
}

