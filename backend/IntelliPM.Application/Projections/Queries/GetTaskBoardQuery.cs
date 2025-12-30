using MediatR;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Projections.Queries;

/// <summary>
/// Query to retrieve task board read model for a project.
/// Returns pre-grouped tasks by status for optimized Kanban board rendering.
/// </summary>
public record GetTaskBoardQuery : IRequest<TaskBoardReadModelDto?>
{
    /// <summary>
    /// The ID of the project to get the task board for.
    /// </summary>
    public int ProjectId { get; init; }
}

/// <summary>
/// DTO for task board read model.
/// Contains pre-calculated counts and grouped tasks by status.
/// </summary>
public record TaskBoardReadModelDto(
    int ProjectId,
    int TodoCount,
    int InProgressCount,
    int DoneCount,
    int TotalTaskCount,
    int TodoStoryPoints,
    int InProgressStoryPoints,
    int DoneStoryPoints,
    int TotalStoryPoints,
    List<TaskSummaryDto> TodoTasks,
    List<TaskSummaryDto> InProgressTasks,
    List<TaskSummaryDto> DoneTasks,
    DateTimeOffset LastUpdated,
    int Version
);

