namespace IntelliPM.Application.AI.DTOs;

/// <summary>
/// DTO for organization AI quota information.
/// </summary>
public record OrganizationAIQuotaDto(
    int Id,
    int OrganizationId,
    string OrganizationName,
    string OrganizationCode,
    long MonthlyTokenLimit,
    int? MonthlyRequestLimit,
    int? ResetDayOfMonth,
    bool IsAIEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

/// <summary>
/// Request DTO for updating organization AI quota.
/// </summary>
public record UpdateOrganizationAIQuotaRequest(
    long MonthlyTokenLimit,
    int? MonthlyRequestLimit,
    int? ResetDayOfMonth,
    bool? IsAIEnabled
);

