using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class Risk : IAggregateRoot, ITenantEntity
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Probability { get; set; } // 1-5
    public int Impact { get; set; } // 1-5
    public string MitigationPlan { get; set; } = string.Empty;
    public int? OwnerId { get; set; }
    public string Status { get; set; } = "Open"; // Open | Mitigated | Closed
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public User? Owner { get; set; }
    public Organization Organization { get; set; } = null!;
}

