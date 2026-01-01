using MediatR;
using IntelliPM.Application.AI.DTOs;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get AI overview statistics aggregated across all organizations (Admin only).
/// </summary>
public record GetAIOverviewStatsQuery : IRequest<AIOverviewStatsDto>;

