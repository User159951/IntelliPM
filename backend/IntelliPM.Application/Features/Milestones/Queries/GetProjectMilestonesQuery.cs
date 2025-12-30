using MediatR;
using IntelliPM.Application.Features.Milestones.DTOs;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Query to retrieve all milestones for a project with optional filtering.
/// </summary>
public record GetProjectMilestonesQuery(
    int ProjectId,
    MilestoneStatus? Status = null,
    bool IncludeCompleted = false
) : IRequest<List<MilestoneDto>>;

