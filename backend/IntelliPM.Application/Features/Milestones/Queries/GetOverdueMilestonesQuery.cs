using MediatR;
using IntelliPM.Application.Features.Milestones.DTOs;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Query to retrieve all overdue milestones.
/// Returns milestones that are past their due date and not completed or cancelled.
/// </summary>
public record GetOverdueMilestonesQuery(int? OrganizationId = null) : IRequest<List<MilestoneDto>>;

