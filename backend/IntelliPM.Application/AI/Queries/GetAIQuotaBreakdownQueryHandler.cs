using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting AI quota breakdown.
/// Provides detailed breakdown by agent type and decision type.
/// </summary>
public class GetAIQuotaBreakdownQueryHandler : IRequestHandler<GetAIQuotaBreakdownQuery, AIQuotaBreakdownDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIQuotaBreakdownQueryHandler> _logger;

    public GetAIQuotaBreakdownQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIQuotaBreakdownQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AIQuotaBreakdownDto> Handle(GetAIQuotaBreakdownQuery request, CancellationToken ct)
    {
        // Calculate date range based on period
        var (startDate, endDate) = CalculateDateRange(request.Period, request.StartDate, request.EndDate);

        _logger.LogInformation(
            "Getting AI quota breakdown for OrganizationId: {OrganizationId}, Period: {Period}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.OrganizationId, request.Period, startDate, endDate);

        // Query AIDecisionLog for usage data
        var query = _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .AsNoTracking()
            .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);

        // Filter by organization if provided
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(d => d.OrganizationId == request.OrganizationId.Value);
        }

        var decisions = await query.ToListAsync(ct);

        // Calculate totals for percentages
        var totalTokens = decisions.Sum(d => d.TokensUsed);
        var totalDecisions = decisions.Count(d => d.WasApplied);

        // Breakdown by agent type
        var byAgent = decisions
            .GroupBy(d => d.AgentType)
            .Select(g => new AgentBreakdownDto(
                g.Key,
                g.Count(), // Requests
                g.Sum(d => d.TokensUsed), // Tokens
                g.Count(d => d.WasApplied), // Decisions
                g.Sum(d => (decimal)d.TokensUsed * Domain.Constants.AIQuotaConstants.CostPerToken), // Cost
                totalTokens > 0 ? (decimal)g.Sum(d => d.TokensUsed) / totalTokens * 100 : 0 // Percentage
            ))
            .OrderByDescending(a => a.Tokens)
            .ToDictionary(a => a.AgentType, a => a);

        // Breakdown by decision type
        var byDecisionType = decisions
            .Where(d => d.WasApplied)
            .GroupBy(d => d.DecisionType)
            .Select(g => new DecisionTypeBreakdownDto(
                g.Key,
                g.Count(), // Decisions
                g.Sum(d => d.TokensUsed), // Tokens
                g.Sum(d => (decimal)d.TokensUsed * Domain.Constants.AIQuotaConstants.CostPerToken), // Cost
                totalDecisions > 0 ? (decimal)g.Count() / totalDecisions * 100 : 0 // Percentage
            ))
            .OrderByDescending(d => d.Decisions)
            .ToDictionary(d => d.DecisionType, d => d);

        // Summary
        var summary = new PeriodSummaryDto(
            startDate,
            endDate,
            decisions.Count, // Total requests
            totalTokens,
            totalDecisions,
            decisions.Sum(d => (decimal)d.TokensUsed * Domain.Constants.AIQuotaConstants.CostPerToken) // Total cost
        );

        return new AIQuotaBreakdownDto(
            byAgent,
            byDecisionType,
            summary
        );
    }

    private (DateTimeOffset startDate, DateTimeOffset endDate) CalculateDateRange(
        string period,
        DateTimeOffset? providedStartDate,
        DateTimeOffset? providedEndDate)
    {
        var endDate = providedEndDate ?? DateTimeOffset.UtcNow;
        var startDate = providedStartDate ?? period.ToLower() switch
        {
            "day" => endDate.AddDays(-1),
            "week" => endDate.AddDays(-7),
            "month" => endDate.AddMonths(-1),
            _ => endDate.AddMonths(-1) // Default to month
        };

        return (startDate, endDate);
    }
}

