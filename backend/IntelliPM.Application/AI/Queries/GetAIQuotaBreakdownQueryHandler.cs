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
        try
        {
            // Calculate date range based on period
            var (startDate, endDate) = CalculateDateRange(request.Period, request.StartDate, request.EndDate);

            _logger.LogInformation(
                "Getting AI quota breakdown for OrganizationId: {OrganizationId}, Period: {Period}, StartDate: {StartDate}, EndDate: {EndDate}",
                request.OrganizationId, request.Period, startDate, endDate);

            // Query AIDecisionLog for usage data
            List<AIDecisionLog> decisions;
            try
            {
                var query = _unitOfWork.Repository<AIDecisionLog>()
                    .Query()
                    .AsNoTracking()
                    .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);

                // Filter by organization if provided
                if (request.OrganizationId.HasValue)
                {
                    query = query.Where(d => d.OrganizationId == request.OrganizationId.Value);
                }

                decisions = await query.ToListAsync(ct);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error when querying AI decision logs for breakdown. OrganizationId: {OrganizationId}", request.OrganizationId);
                // Return empty breakdown on database error
                return new AIQuotaBreakdownDto(
                    new Dictionary<string, AgentBreakdownDto>(),
                    new Dictionary<string, DecisionTypeBreakdownDto>(),
                    new PeriodSummaryDto(startDate, endDate, 0, 0, 0, 0)
                );
            }

            if (decisions == null || decisions.Count == 0)
            {
                _logger.LogInformation("No AI decision logs found for the specified criteria");
                return new AIQuotaBreakdownDto(
                    new Dictionary<string, AgentBreakdownDto>(),
                    new Dictionary<string, DecisionTypeBreakdownDto>(),
                    new PeriodSummaryDto(startDate, endDate, 0, 0, 0, 0)
                );
            }

            // Calculate totals for percentages with error handling
            int totalTokens = 0;
            int totalDecisions = 0;
            try
            {
                totalTokens = decisions.Sum(d => d.TokensUsed);
                totalDecisions = decisions.Count(d => d.WasApplied);
            }
            catch (Exception calcEx)
            {
                _logger.LogWarning(calcEx, "Error calculating totals for breakdown. Using default values.");
                totalTokens = 0;
                totalDecisions = 0;
            }

            // Breakdown by agent type with error handling
            Dictionary<string, AgentBreakdownDto> byAgent;
            try
            {
                byAgent = decisions
                    .GroupBy(d => d.AgentType ?? "Unknown")
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
            }
            catch (Exception agentEx)
            {
                _logger.LogWarning(agentEx, "Error calculating agent breakdown. Using empty dictionary.");
                byAgent = new Dictionary<string, AgentBreakdownDto>();
            }

            // Breakdown by decision type with error handling
            Dictionary<string, DecisionTypeBreakdownDto> byDecisionType;
            try
            {
                byDecisionType = decisions
                    .Where(d => d.WasApplied)
                    .GroupBy(d => d.DecisionType ?? "Unknown")
                    .Select(g => new DecisionTypeBreakdownDto(
                        g.Key,
                        g.Count(), // Decisions
                        g.Sum(d => d.TokensUsed), // Tokens
                        g.Sum(d => (decimal)d.TokensUsed * Domain.Constants.AIQuotaConstants.CostPerToken), // Cost
                        totalDecisions > 0 ? (decimal)g.Count() / totalDecisions * 100 : 0 // Percentage
                    ))
                    .OrderByDescending(d => d.Decisions)
                    .ToDictionary(d => d.DecisionType, d => d);
            }
            catch (Exception decisionEx)
            {
                _logger.LogWarning(decisionEx, "Error calculating decision type breakdown. Using empty dictionary.");
                byDecisionType = new Dictionary<string, DecisionTypeBreakdownDto>();
            }

            // Summary with error handling
            PeriodSummaryDto summary;
            try
            {
                summary = new PeriodSummaryDto(
                    startDate,
                    endDate,
                    decisions.Count, // Total requests
                    totalTokens,
                    totalDecisions,
                    decisions.Sum(d => (decimal)d.TokensUsed * Domain.Constants.AIQuotaConstants.CostPerToken) // Total cost
                );
            }
            catch (Exception summaryEx)
            {
                _logger.LogWarning(summaryEx, "Error calculating summary. Using default values.");
                summary = new PeriodSummaryDto(startDate, endDate, 0, 0, 0, 0);
            }

            return new AIQuotaBreakdownDto(
                byAgent,
                byDecisionType,
                summary
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota breakdown for OrganizationId: {OrganizationId}, Period: {Period}", 
                request.OrganizationId, request.Period);
            throw;
        }
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

