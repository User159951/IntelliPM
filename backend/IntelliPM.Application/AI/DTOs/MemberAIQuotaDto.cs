namespace IntelliPM.Application.AI.DTOs;

/// <summary>
/// DTO for member AI quota information with effective quota calculation.
/// </summary>
public record MemberAIQuotaDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string GlobalRole,
    int OrganizationId,
    string OrganizationName,
    // Effective quota (UserOverride ?? OrganizationLimit)
    EffectiveMemberQuotaDto EffectiveQuota,
    // User override (if exists)
    UserQuotaOverrideDto? Override,
    // Organization base quota
    OrganizationQuotaBaseDto OrganizationQuota
);

/// <summary>
/// Effective quota limits for a member (user override or organization default).
/// </summary>
public record EffectiveMemberQuotaDto(
    long MonthlyTokenLimit,
    int? MonthlyRequestLimit,
    bool IsAIEnabled,
    bool HasOverride
);

/// <summary>
/// User quota override information.
/// </summary>
public record UserQuotaOverrideDto(
    long? MonthlyTokenLimitOverride,
    int? MonthlyRequestLimitOverride,
    bool? IsAIEnabledOverride,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

/// <summary>
/// Organization base quota information.
/// </summary>
public record OrganizationQuotaBaseDto(
    long MonthlyTokenLimit,
    int? MonthlyRequestLimit,
    bool IsAIEnabled
);

/// <summary>
/// Request DTO for updating user AI quota override.
/// </summary>
public record UpdateMemberAIQuotaRequest(
    long? MonthlyTokenLimitOverride,
    int? MonthlyRequestLimitOverride,
    bool? IsAIEnabledOverride
);

