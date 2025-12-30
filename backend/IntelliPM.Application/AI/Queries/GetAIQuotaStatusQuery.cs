using MediatR;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get current AI quota status for an organization.
/// </summary>
public record GetAIQuotaStatusQuery : IRequest<AIQuotaStatusDto>
{
    public int OrganizationId { get; init; }
}

/// <summary>
/// DTO for AI quota status with usage information.
/// </summary>
public record AIQuotaStatusDto(
    int QuotaId,
    string TierName,
    bool IsActive,
    QuotaUsageDto Usage,
    DateTimeOffset PeriodEndDate,
    int DaysRemaining,
    bool IsExceeded,
    bool AlertSent
);

/// <summary>
/// DTO for quota usage breakdown.
/// </summary>
public record QuotaUsageDto(
    int TokensUsed,
    int TokensLimit,
    decimal TokensPercentage,
    int RequestsUsed,
    int RequestsLimit,
    decimal RequestsPercentage,
    decimal CostAccumulated,
    decimal CostLimit,
    decimal CostPercentage
);

