using MediatR;

namespace IntelliPM.Application.Sprints.Queries;

/// <summary>
/// Query to calculate sprint velocity based on completed story points.
/// </summary>
public record GetSprintVelocityQuery : IRequest<SprintVelocityResponse>
{
    public int ProjectId { get; init; }
    public int? SprintId { get; init; } // If null, return velocity for all sprints
    public int? LastNSprints { get; init; } = 5; // Get last N sprints for trend
}

/// <summary>
/// Response containing sprint velocity data for one or more sprints.
/// </summary>
public record SprintVelocityResponse(
    int ProjectId,
    List<SprintVelocityDto> Sprints,
    decimal AverageVelocity,
    int TotalCompletedStoryPoints
);

/// <summary>
/// DTO for sprint velocity information.
/// </summary>
public record SprintVelocityDto(
    int SprintId,
    string SprintName,
    DateTime StartDate,
    DateTime? EndDate,
    int CompletedStoryPoints,
    int PlannedStoryPoints,
    int TotalTasks,
    int CompletedTasks,
    decimal CompletionRate
);

