namespace IntelliPM.Application.AI.DTOs;

/// <summary>
/// DTO for AI quota template information.
/// </summary>
public record AIQuotaTemplateDto(
    int Id,
    string TierName,
    string? Description,
    bool IsActive,
    bool IsSystemTemplate,
    int MaxTokensPerPeriod,
    int MaxRequestsPerPeriod,
    int MaxDecisionsPerPeriod,
    decimal MaxCostPerPeriod,
    bool AllowOverage,
    decimal OverageRate,
    decimal DefaultAlertThresholdPercentage,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

