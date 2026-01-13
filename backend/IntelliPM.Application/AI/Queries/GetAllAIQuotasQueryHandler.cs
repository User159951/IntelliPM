using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting all AI quotas (Admin only).
/// </summary>
public class GetAllAIQuotasQueryHandler : IRequestHandler<GetAllAIQuotasQuery, PagedResponse<AIQuotaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllAIQuotasQueryHandler> _logger;

    public GetAllAIQuotasQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllAIQuotasQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<PagedResponse<AIQuotaDto>> Handle(GetAllAIQuotasQuery request, CancellationToken ct)
    {
        try
        {
            IQueryable<AIQuota> query;
            try
            {
                query = _unitOfWork.Repository<AIQuota>()
                    .Query()
                    .AsNoTracking()
                    .Include(q => q.Organization)
                    .AsQueryable();
            }
            catch (Exception queryEx)
            {
                _logger.LogError(queryEx, "Error initializing AI quota query");
                // Return empty result on query initialization error
                return new PagedResponse<AIQuotaDto>(
                    new List<AIQuotaDto>(),
                    Math.Max(1, request.Page),
                    Math.Max(1, Math.Min(request.PageSize, 100)),
                    0
                );
            }

            // Apply filters
            if (!string.IsNullOrEmpty(request.TierName))
            {
                query = query.Where(q => q.TierName == request.TierName);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(q => q.IsActive == request.IsActive.Value);
            }

            // Note: IsExceeded filter is commented out as IsQuotaExceeded might throw exceptions
            // if (request.IsExceeded.HasValue)
            // {
            //     query = query.Where(q => q.IsQuotaExceeded == request.IsExceeded.Value);
            // }

            // Get total count with error handling
            int totalCount = 0;
            try
            {
                totalCount = await query.CountAsync(ct);
            }
            catch (Exception countEx)
            {
                _logger.LogWarning(countEx, "Error counting AI quotas. Using default value 0.");
                totalCount = 0;
            }

            // Apply pagination
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

            // Get quotas with error handling
            List<AIQuota> quotas = new List<AIQuota>();
            try
            {
                quotas = await query
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);
            }
            catch (Exception listEx)
            {
                _logger.LogError(listEx, "Error querying AI quotas. Returning empty list.");
                // Return empty result on query error
                return new PagedResponse<AIQuotaDto>(
                    new List<AIQuotaDto>(),
                    page,
                    pageSize,
                    totalCount
                );
            }

            var quotaDtos = new List<AIQuotaDto>();
            
            foreach (var q in quotas)
        {
            try
            {
                // Ensure Organization is loaded
                if (q.Organization == null)
                {
                    _logger.LogWarning("Quota {QuotaId} has null Organization. OrganizationId: {OrgId}", q.Id, q.OrganizationId);
                }

                // Safely get quota status
                QuotaStatus status;
                try
                {
                    status = q.GetQuotaStatus();
                }
                catch (Exception statusEx)
                {
                    _logger.LogWarning(statusEx, "Error getting quota status for quota {QuotaId}. Using default values.", q.Id);
                    // Use default status values
                    status = new QuotaStatus
                    {
                        TokensUsed = q.TokensUsed,
                        TokensLimit = q.MaxTokensPerPeriod,
                        TokensPercentage = 0,
                        RequestsUsed = q.RequestsUsed,
                        RequestsLimit = q.MaxRequestsPerPeriod,
                        RequestsPercentage = 0,
                        CostAccumulated = q.CostAccumulated,
                        CostLimit = q.MaxCostPerPeriod,
                        CostPercentage = 0,
                        IsExceeded = false,
                        DaysRemaining = 0
                    };
                }

                // Safely get IsQuotaExceeded
                bool isExceeded = false;
                try
                {
                    isExceeded = q.IsQuotaExceeded;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading IsQuotaExceeded for quota {QuotaId}. Defaulting to false.", q.Id);
                }

                quotaDtos.Add(new AIQuotaDto(
                    q.Id,
                    q.OrganizationId,
                    q.Organization?.Name ?? "Unknown Organization",
                    q.TierName ?? "Unknown Tier",
                    q.IsActive,
                    new QuotaUsageDto(
                        status.TokensUsed,
                        status.TokensLimit,
                        status.TokensPercentage,
                        status.RequestsUsed,
                        status.RequestsLimit,
                        status.RequestsPercentage,
                        status.CostAccumulated,
                        status.CostLimit,
                        status.CostPercentage
                    ),
                    q.PeriodEndDate != default(DateTimeOffset) ? q.PeriodEndDate : DateTimeOffset.UtcNow,
                    isExceeded,
                    q.AlertSent
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing quota {QuotaId} for organization {OrgId}. Exception: {ExceptionType}, Message: {Message}. Skipping this quota.", 
                    q.Id, q.OrganizationId, ex.GetType().Name, ex.Message);
                // Skip this quota and continue with others
                continue;
            }
            }

            return new PagedResponse<AIQuotaDto>(quotaDtos, page, pageSize, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI quotas. Page: {Page}, PageSize: {PageSize}, IsActive: {IsActive}", 
                request.Page, request.PageSize, request.IsActive);
            throw;
        }
    }
}

