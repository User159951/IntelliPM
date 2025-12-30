using IntelliPM.Application.Features.Milestones.DTOs;
using MediatR;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Command to create a new milestone for a project.
/// </summary>
public record CreateMilestoneCommand : IRequest<MilestoneDto>
{
    /// <summary>
    /// ID of the project this milestone belongs to.
    /// </summary>
    public int ProjectId { get; init; }

    /// <summary>
    /// Name of the milestone.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional description of the milestone.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Type of milestone: "Release", "Sprint", "Deadline", "Custom".
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Due date for the milestone.
    /// </summary>
    public DateTimeOffset DueDate { get; init; }

    /// <summary>
    /// Progress percentage (0-100). Default: 0.
    /// </summary>
    public int Progress { get; init; } = 0;
}

