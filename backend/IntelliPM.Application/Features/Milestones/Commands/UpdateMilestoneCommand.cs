using IntelliPM.Application.Features.Milestones.DTOs;
using MediatR;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Command to update an existing milestone.
/// </summary>
public record UpdateMilestoneCommand : IRequest<MilestoneDto>
{
    /// <summary>
    /// ID of the milestone to update.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Updated name of the milestone.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Updated description of the milestone.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Updated due date for the milestone.
    /// </summary>
    public DateTimeOffset DueDate { get; init; }

    /// <summary>
    /// Updated progress percentage (0-100).
    /// </summary>
    public int Progress { get; init; }
}

