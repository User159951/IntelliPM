using MediatR;

namespace IntelliPM.Application.Projections.Commands;

/// <summary>
/// Command to rebuild read model projections from source data.
/// Admin-only operation for maintaining projection consistency.
/// </summary>
public record RebuildProjectionCommand : IRequest<RebuildProjectionResponse>
{
    /// <summary>
    /// Type of projection to rebuild: "All", "TaskBoard", "SprintSummary", "ProjectOverview"
    /// </summary>
    public string ProjectionType { get; init; } = "All";

    /// <summary>
    /// Optional: Rebuild projections for a specific project only
    /// </summary>
    public int? ProjectId { get; init; }

    /// <summary>
    /// Optional: Rebuild projections for all projects in a specific organization
    /// </summary>
    public int? OrganizationId { get; init; }

    /// <summary>
    /// If true, delete existing read models and rebuild from scratch.
    /// If false, update existing read models or create if missing.
    /// </summary>
    public bool ForceRebuild { get; init; } = false;
}

/// <summary>
/// Response containing rebuild statistics and details.
/// </summary>
public record RebuildProjectionResponse(
    int ProjectionsRebuilt,
    List<string> RebuildDetails,
    TimeSpan Duration,
    bool Success,
    string? Error
);

