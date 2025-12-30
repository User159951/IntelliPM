using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting AI quota status for an organization.
/// </summary>
public class GetAIQuotaStatusQueryHandler : IRequestHandler<GetAIQuotaStatusQuery, AIQuotaStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIQuotaStatusQueryHandler> _logger;

    public GetAIQuotaStatusQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIQuotaStatusQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<AIQuotaStatusDto> Handle(GetAIQuotaStatusQuery request, CancellationToken ct)
    {
        var quota = await _unitOfWork.Repository<AIQuota>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId && q.IsActive, ct);

        if (quota == null)
        {
            // Return default status if no quota exists
            return new AIQuotaStatusDto(
                0,
                "None",
                false,
                new QuotaUsageDto(0, 0, 0, 0, 0, 0, 0, 0, 0),
                DateTimeOffset.UtcNow,
                0,
                false,
                false
            );
        }

        var status = quota.GetQuotaStatus();
        var daysRemaining = (quota.PeriodEndDate.Date - DateTimeOffset.UtcNow.Date).Days;

        return new AIQuotaStatusDto(
            quota.Id,
            quota.TierName,
            quota.IsActive,
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
            quota.PeriodEndDate,
            daysRemaining,
            quota.IsQuotaExceeded,
            quota.AlertSent
        );
    }
}

