using IntelliPM.Application.Features.Milestones.DTOs;
using MediatR;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Command to mark a milestone as completed.
/// </summary>
public record CompleteMilestoneCommand : IRequest<MilestoneDto>
{
    /// <summary>
    /// ID of the milestone to complete.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Optional completion date. If not provided, uses current UTC time.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }
}

