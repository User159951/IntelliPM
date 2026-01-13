using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

public class Alert : IAggregateRoot, ITenantEntity
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ProjectId { get; set; }
    public string Type { get; set; } = string.Empty; // VelocityDrop | RiskEscalation | DefectSpike | DeadlineMissing
    public string Severity { get; set; } = "Info"; // Info | Warning | Error | Critical
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TriggerData { get; set; } // JSON metadata
    public bool IsRead { get; set; } = false;
    public bool IsResolved { get; set; } = false;
    public int? ResolvedById { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation
    public Project Project { get; set; } = null!;
    public User? ResolvedBy { get; set; }
    public Organization Organization { get; set; } = null!;
}

