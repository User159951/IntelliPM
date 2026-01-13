namespace IntelliPM.Domain.Interfaces;

/// <summary>
/// Interface for entities that belong to a tenant (organization).
/// Used to identify entities that require tenant isolation.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// The ID of the organization (tenant) that owns this entity.
    /// </summary>
    int OrganizationId { get; set; }
}
