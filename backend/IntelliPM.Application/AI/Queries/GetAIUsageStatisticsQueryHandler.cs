using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting AI usage statistics for an organization.
/// </summary>
public class GetAIUsageStatisticsQueryHandler : IRequestHandler<GetAIUsageStatisticsQuery, AIUsageStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIUsageStatisticsQueryHandler> _logger;

    public GetAIUsageStatisticsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIUsageStatisticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<AIUsageStatisticsDto> Handle(GetAIUsageStatisticsQuery request, CancellationToken ct)
    {
        // Get decision logs in date range
        var decisions = await _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .AsNoTracking()
            .Where(d => d.OrganizationId == request.OrganizationId &&
                       d.CreatedAt >= request.StartDate &&
                       d.CreatedAt <= request.EndDate)
            .ToListAsync(ct);

        // Calculate totals
        var totalTokensUsed = decisions.Sum(d => d.TokensUsed);
        var totalRequests = decisions.Count;
        var totalDecisions = decisions.Count(d => d.WasApplied);
        var totalCost = decisions.Sum(d => d.CostAccumulated);

        // Usage by agent
        var usageByAgent = decisions
            .GroupBy(d => d.AgentType)
            .ToDictionary(
                g => g.Key,
                g => new AgentUsageStatsDto(
                    g.Sum(d => d.TokensUsed),
                    g.Count()
                )
            );

        // Usage by decision type
        var usageByDecisionType = decisions
            .GroupBy(d => d.DecisionType)
            .ToDictionary(
                g => g.Key,
                g => new DecisionTypeStatsDto(
                    g.Sum(d => d.TokensUsed),
                    g.Count(d => d.WasApplied)
                )
            );

        // Daily usage
        var dailyUsage = decisions
            .GroupBy(d => d.CreatedAt.Date)
            .Select(g => new DailyUsageDto(
                g.Key,
                g.Sum(d => d.TokensUsed),
                g.Count(),
                g.Count(d => d.WasApplied)
            ))
            .OrderBy(d => d.Date)
            .ToList();

        return new AIUsageStatisticsDto(
            totalTokensUsed,
            totalRequests,
            totalDecisions,
            totalCost,
            usageByAgent,
            usageByDecisionType,
            dailyUsage
        );
    }
}

