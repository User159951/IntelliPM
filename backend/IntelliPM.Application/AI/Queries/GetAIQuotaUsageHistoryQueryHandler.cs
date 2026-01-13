using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting AI quota usage history.
/// Aggregates usage data from AIDecisionLog by day with pagination support.
/// </summary>
public class GetAIQuotaUsageHistoryQueryHandler : IRequestHandler<GetAIQuotaUsageHistoryQuery, PagedResponse<DailyUsageHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIQuotaUsageHistoryQueryHandler> _logger;

    public GetAIQuotaUsageHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIQuotaUsageHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResponse<DailyUsageHistoryDto>> Handle(GetAIQuotaUsageHistoryQuery request, CancellationToken ct)
    {
        try
        {
            var startDate = request.StartDate ?? DateTimeOffset.UtcNow.AddDays(-30);
            var endDate = request.EndDate ?? DateTimeOffset.UtcNow;
            
            // Validate pagination parameters
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            _logger.LogInformation(
                "Getting AI quota usage history for OrganizationId: {OrganizationId}, StartDate: {StartDate}, EndDate: {EndDate}, Page: {Page}, PageSize: {PageSize}",
                request.OrganizationId, startDate, endDate, page, pageSize);

            // Query AIDecisionLog for usage data with error handling
            List<AIDecisionLog> decisions;
            try
            {
                var baseQuery = _unitOfWork.Repository<AIDecisionLog>()
                    .Query()
                    .AsNoTracking()
                    .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);

                // Filter by organization if provided
                if (request.OrganizationId.HasValue)
                {
                    baseQuery = baseQuery.Where(d => d.OrganizationId == request.OrganizationId.Value);
                }

                decisions = await baseQuery.ToListAsync(ct);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error when querying AI decision logs for usage history. OrganizationId: {OrganizationId}", request.OrganizationId);
                // Return empty paginated response on database error
                return new PagedResponse<DailyUsageHistoryDto>(
                    new List<DailyUsageHistoryDto>(),
                    page,
                    pageSize,
                    0
                );
            }

            if (decisions == null || decisions.Count == 0)
            {
                _logger.LogInformation("No AI decision logs found for the specified criteria");
                return new PagedResponse<DailyUsageHistoryDto>(
                    new List<DailyUsageHistoryDto>(),
                    page,
                    pageSize,
                    0
                );
            }

            // Group by date and aggregate with error handling
            List<DailyUsageHistoryDto> dailyUsage;
            try
            {
                dailyUsage = decisions
                    .GroupBy(d => d.CreatedAt.Date)
                    .Select(g => new DailyUsageHistoryDto(
                        g.Key,
                        g.Count(), // Requests
                        g.Sum(d => d.TokensUsed), // Tokens
                        g.Count(d => d.WasApplied), // Decisions (only applied ones)
                        g.Sum(d => (decimal)d.TokensUsed * Domain.Constants.AIQuotaConstants.CostPerToken) // Cost
                    ))
                    .OrderByDescending(d => d.Date) // Most recent first
                    .ToList();
            }
            catch (Exception aggEx)
            {
                _logger.LogWarning(aggEx, "Error aggregating daily usage data. Returning empty list.");
                dailyUsage = new List<DailyUsageHistoryDto>();
            }

            // Apply pagination
            var totalCount = dailyUsage.Count;
            var paginatedItems = dailyUsage
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResponse<DailyUsageHistoryDto>(
                paginatedItems,
                page,
                pageSize,
                totalCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota usage history for OrganizationId: {OrganizationId}", request.OrganizationId);
            throw;
        }
    }
}

