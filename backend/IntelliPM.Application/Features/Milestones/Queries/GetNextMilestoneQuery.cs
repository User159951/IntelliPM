using MediatR;
using IntelliPM.Application.Features.Milestones.DTOs;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Query to retrieve the next upcoming milestone for a project.
/// Returns the milestone with the earliest due date that is pending or in progress.
/// </summary>
public record GetNextMilestoneQuery(int ProjectId) : IRequest<MilestoneDto?>;

