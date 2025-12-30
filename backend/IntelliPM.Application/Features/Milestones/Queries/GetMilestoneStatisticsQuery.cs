using MediatR;
using IntelliPM.Application.Features.Milestones.DTOs;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Query to retrieve milestone statistics for a project.
/// Returns aggregated statistics about milestones.
/// </summary>
public record GetMilestoneStatisticsQuery(int ProjectId) : IRequest<MilestoneStatisticsDto>;

