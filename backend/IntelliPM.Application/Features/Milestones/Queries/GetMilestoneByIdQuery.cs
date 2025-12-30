using MediatR;
using IntelliPM.Application.Features.Milestones.DTOs;

namespace IntelliPM.Application.Features.Milestones.Queries;

/// <summary>
/// Query to retrieve a milestone by its ID.
/// </summary>
public record GetMilestoneByIdQuery(int Id) : IRequest<MilestoneDto?>;

