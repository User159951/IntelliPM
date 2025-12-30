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
        var query = _unitOfWork.Repository<AIQuota>()
            .Query()
            .AsNoTracking()
            .Include(q => q.Organization)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.TierName))
        {
            query = query.Where(q => q.TierName == request.TierName);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(q => q.IsActive == request.IsActive.Value);
        }

        if (request.IsExceeded.HasValue)
        {
            query = query.Where(q => q.IsQuotaExceeded == request.IsExceeded.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var quotas = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var quotaDtos = quotas.Select(q =>
        {
            var status = q.GetQuotaStatus();
            return new AIQuotaDto(
                q.Id,
                q.OrganizationId,
                q.Organization.Name,
                q.TierName,
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
                q.PeriodEndDate,
                q.IsQuotaExceeded,
                q.AlertSent
            );
        }).ToList();

        return new PagedResponse<AIQuotaDto>(quotaDtos, page, pageSize, totalCount);
    }
}

