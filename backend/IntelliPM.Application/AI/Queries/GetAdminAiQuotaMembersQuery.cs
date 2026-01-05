using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get paginated list of organization members with their AI quota information (Admin only).
/// </summary>
public record GetAdminAiQuotaMembersQuery : IRequest<PagedResponse<AdminAiQuotaMemberDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; } // Search by email or name
}

/// <summary>
/// DTO for admin AI quota member information.
/// </summary>
public record AdminAiQuotaMemberDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string UserRole, // GlobalRole as string
    int OrganizationId,
    string OrganizationName,
    EffectiveQuotaDto EffectiveQuota,
    QuotaOverrideDto? Override,
    UserUsageDto Usage,
    PeriodInfoDto Period
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

/// <summary>
/// User usage information.
/// </summary>
public record UserUsageDto(
    int TokensUsed,
    int RequestsUsed,
    int DecisionsMade,
    decimal CostAccumulated,
    decimal TokensPercentage,
    decimal RequestsPercentage,
    decimal DecisionsPercentage,
    decimal CostPercentage
);

/// <summary>
/// Period information.
/// </summary>
public record PeriodInfoDto(
    DateTimeOffset PeriodStartDate,
    DateTimeOffset PeriodEndDate,
    int DaysRemaining
);

