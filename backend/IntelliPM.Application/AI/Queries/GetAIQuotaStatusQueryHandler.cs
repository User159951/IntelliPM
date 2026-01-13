using System;
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
        try
        {
            if (request.OrganizationId <= 0)
            {
                _logger.LogWarning("Invalid organization ID: {OrganizationId}", request.OrganizationId);
                // Return default status for invalid organization ID
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

            AIQuota? quota = null;
            try
            {
                quota = await _unitOfWork.Repository<AIQuota>()
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId && q.IsActive, ct);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error when querying AI quota for organization {OrganizationId}", request.OrganizationId);
                // Return default status on database error
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

            if (quota == null)
            {
                _logger.LogInformation("No active quota found for organization {OrganizationId}", request.OrganizationId);
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

            // Safely get quota status
            QuotaStatus status;
            try
            {
                status = quota.GetQuotaStatus();
            }
            catch (Exception statusEx)
            {
                _logger.LogWarning(statusEx, "Error getting quota status for quota {QuotaId}. Using default values.", quota.Id);
                // Use default status values based on quota properties
                status = new QuotaStatus
                {
                    TokensUsed = quota.TokensUsed,
                    TokensLimit = quota.MaxTokensPerPeriod,
                    TokensPercentage = quota.MaxTokensPerPeriod > 0 ? (decimal)quota.TokensUsed / quota.MaxTokensPerPeriod * 100 : 0,
                    RequestsUsed = quota.RequestsUsed,
                    RequestsLimit = quota.MaxRequestsPerPeriod,
                    RequestsPercentage = quota.MaxRequestsPerPeriod > 0 ? (decimal)quota.RequestsUsed / quota.MaxRequestsPerPeriod * 100 : 0,
                    CostAccumulated = quota.CostAccumulated,
                    CostLimit = quota.MaxCostPerPeriod,
                    CostPercentage = quota.MaxCostPerPeriod > 0 ? quota.CostAccumulated / quota.MaxCostPerPeriod * 100 : 0,
                    IsExceeded = false,
                    DaysRemaining = 0
                };
            }

            // Safely calculate days remaining
            int daysRemaining = 0;
            try
            {
                if (quota.PeriodEndDate != default(DateTimeOffset))
                {
                    daysRemaining = Math.Max(0, (quota.PeriodEndDate.Date - DateTimeOffset.UtcNow.Date).Days);
                }
            }
            catch (Exception daysEx)
            {
                _logger.LogWarning(daysEx, "Error calculating days remaining for quota {QuotaId}. Defaulting to 0.", quota.Id);
                daysRemaining = 0;
            }

            // Safely get IsQuotaExceeded
            bool isExceeded = false;
            try
            {
                isExceeded = quota.IsQuotaExceeded;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading IsQuotaExceeded for quota {QuotaId}. Defaulting to false.", quota.Id);
            }

            // Safely create the DTO
            try
            {
                return new AIQuotaStatusDto(
                    quota.Id,
                    quota.TierName ?? "Unknown",
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
                    quota.PeriodEndDate != default(DateTimeOffset) ? quota.PeriodEndDate : DateTimeOffset.UtcNow,
                    daysRemaining,
                    isExceeded,
                    quota.AlertSent
                );
            }
            catch (Exception dtoEx)
            {
                _logger.LogError(dtoEx, "Error creating AIQuotaStatusDto for quota {QuotaId}. Returning default DTO.", quota.Id);
                // Return a safe default DTO
                return new AIQuotaStatusDto(
                    0,
                    "Unknown",
                    false,
                    new QuotaUsageDto(0, 0, 0, 0, 0, 0, 0, 0, 0),
                    DateTimeOffset.UtcNow,
                    0,
                    false,
                    false
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota status for organization {OrganizationId}", request.OrganizationId);
            throw;
        }
    }
}

