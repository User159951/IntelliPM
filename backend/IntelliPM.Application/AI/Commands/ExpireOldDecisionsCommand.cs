using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to expire old pending AI decisions that have passed their approval deadline.
/// This is typically called by a background service.
/// </summary>
public record ExpireOldDecisionsCommand : IRequest<ExpireOldDecisionsResult>
{
    /// <summary>
    /// Optional organization ID to limit expiration to a specific organization.
    /// If null, expires decisions for all organizations.
    /// </summary>
    public int? OrganizationId { get; init; }
    
    /// <summary>
    /// Maximum number of decisions to expire in one batch.
    /// </summary>
    public int BatchSize { get; init; } = 100;
}

/// <summary>
/// Result of expiring old decisions.
/// </summary>
public record ExpireOldDecisionsResult(
    int ExpiredCount,
    List<Guid> ExpiredDecisionIds
);

