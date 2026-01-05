using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to update or create a user AI quota override (Admin only).
/// </summary>
public record UpdateUserAIQuotaOverrideCommand : IRequest<UpdateUserAIQuotaOverrideResponse>
{
    public int UserId { get; init; }
    public int? MaxTokensPerPeriod { get; init; }
    public int? MaxRequestsPerPeriod { get; init; }
    public int? MaxDecisionsPerPeriod { get; init; }
    public decimal? MaxCostPerPeriod { get; init; }
    public string? Reason { get; init; } // Audit trail
}

/// <summary>
/// Response for updating user AI quota override.
/// </summary>
public record UpdateUserAIQuotaOverrideResponse(
    int OverrideId,
    int UserId,
    EffectiveQuotaDto EffectiveQuota,
    QuotaOverrideDto Override
);

/// <summary>
/// Effective quota limits (override + org default).
/// </summary>
public record EffectiveQuotaDto(
    int MaxTokensPerPeriod,
    int MaxRequestsPerPeriod,
    int MaxDecisionsPerPeriod,
    decimal MaxCostPerPeriod,
    bool HasOverride
);

/// <summary>
/// Quota override information.
/// </summary>
public record QuotaOverrideDto(
    int? MaxTokensPerPeriod,
    int? MaxRequestsPerPeriod,
    int? MaxDecisionsPerPeriod,
    decimal? MaxCostPerPeriod,
    DateTimeOffset CreatedAt,
    string? Reason
);

