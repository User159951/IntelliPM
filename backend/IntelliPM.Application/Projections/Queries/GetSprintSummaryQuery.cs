using MediatR;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Query to retrieve sprint summary read model.
/// Returns pre-calculated sprint metrics and burndown data.
/// </summary>
public record GetSprintSummaryQuery : IRequest<SprintSummaryReadModelDto?>
{
    /// <summary>
    /// The ID of the sprint to get the summary for.
    /// </summary>
    public int SprintId { get; init; }
}

/// <summary>
/// DTO for sprint summary read model.
/// Contains pre-calculated metrics, burndown data, and velocity information.
/// </summary>
public record SprintSummaryReadModelDto(
    int SprintId,
    string SprintName,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int? PlannedCapacity,
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int TodoTasks,
    int TotalStoryPoints,
    int CompletedStoryPoints,
    int InProgressStoryPoints,
    int RemainingStoryPoints,
    decimal CompletionPercentage,
    decimal VelocityPercentage,
    decimal CapacityUtilization,
    int EstimatedDaysRemaining,
    bool IsOnTrack,
    decimal AverageVelocity,
    List<BurndownPointDto> BurndownData,
    DateTimeOffset LastUpdated,
    int Version
);

