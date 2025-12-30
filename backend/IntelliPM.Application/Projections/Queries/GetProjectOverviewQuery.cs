using MediatR;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Query to retrieve project overview read model.
/// Returns aggregated project metrics, team statistics, and health indicators.
/// </summary>
public record GetProjectOverviewQuery : IRequest<ProjectOverviewReadModelDto?>
{
    /// <summary>
    /// The ID of the project to get the overview for.
    /// </summary>
    public int ProjectId { get; init; }
}

/// <summary>
/// DTO for project overview read model.
/// Contains aggregated metrics, team statistics, and health indicators.
/// </summary>
public record ProjectOverviewReadModelDto(
    int ProjectId,
    string ProjectName,
    string ProjectType,
    string Status,
    int OwnerId,
    string OwnerName,
    int TotalMembers,
    int ActiveMembers,
    List<MemberSummaryDto> TeamMembers,
    int TotalSprints,
    int ActiveSprintsCount,
    int CompletedSprintsCount,
    int? CurrentSprintId,
    string? CurrentSprintName,
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int TodoTasks,
    int BlockedTasks,
    int OverdueTasks,
    int TotalStoryPoints,
    int CompletedStoryPoints,
    int RemainingStoryPoints,
    int TotalDefects,
    int OpenDefects,
    int CriticalDefects,
    decimal AverageVelocity,
    decimal LastSprintVelocity,
    List<VelocityTrendDto> VelocityTrend,
    decimal ProjectHealth,
    string HealthStatus,
    List<string> RiskFactors,
    DateTimeOffset LastActivityAt,
    int ActivitiesLast7Days,
    int ActivitiesLast30Days,
    decimal OverallProgress,
    decimal SprintProgress,
    int DaysUntilNextMilestone,
    DateTimeOffset LastUpdated,
    int Version
);

