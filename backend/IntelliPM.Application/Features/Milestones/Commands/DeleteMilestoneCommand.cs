using MediatR;

namespace IntelliPM.Application.Features.Milestones.Commands;

/// <summary>
/// Command to delete a milestone.
/// </summary>
public record DeleteMilestoneCommand(int Id) : IRequest<Unit>;

