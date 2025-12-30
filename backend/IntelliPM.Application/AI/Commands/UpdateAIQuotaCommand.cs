using MediatR;
using IntelliPM.Domain.Constants;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to update AI quota limits for an organization.
/// Supports tier upgrades/downgrades, custom limits, and billing integration.
/// </summary>
public record UpdateAIQuotaCommand : IRequest<UpdateAIQuotaResponse>
{
    public int OrganizationId { get; init; }
    public string TierName { get; init; } = string.Empty;
    public int? MaxTokensPerPeriod { get; init; }
    public int? MaxRequestsPerPeriod { get; init; }
    public int? MaxDecisionsPerPeriod { get; init; }
    public decimal? MaxCostPerPeriod { get; init; }
    public bool? AllowOverage { get; init; }
    public decimal? OverageRate { get; init; }
    public bool? EnforceQuota { get; init; }
    public bool ApplyImmediately { get; init; } = true;
    public DateTimeOffset? ScheduledDate { get; init; }
    public string? Reason { get; init; } // Audit trail
}

/// <summary>
/// Response containing updated quota information and billing details.
/// </summary>
public record UpdateAIQuotaResponse(
    int QuotaId,
    int OrganizationId,
    string TierName,
    QuotaLimitsDto Limits,
    QuotaStatus CurrentStatus,
    bool WasBillingTriggered,
    string? BillingReferenceId
);

/// <summary>
/// DTO for quota limits.
/// </summary>
public record QuotaLimitsDto(
    int MaxTokensPerPeriod,
    int MaxRequestsPerPeriod,
    int MaxDecisionsPerPeriod,
    decimal MaxCostPerPeriod,
    bool AllowOverage,
    decimal OverageRate
);

/// <summary>
/// Current quota status with usage percentages.
/// </summary>
public record QuotaStatus(
    int TokensUsed,
    int TokensLimit,
    decimal TokensPercentage,
    int RequestsUsed,
    int RequestsLimit,
    decimal RequestsPercentage,
    decimal CostAccumulated,
    decimal CostLimit,
    decimal CostPercentage,
    bool IsExceeded,
    int DaysRemaining
);

