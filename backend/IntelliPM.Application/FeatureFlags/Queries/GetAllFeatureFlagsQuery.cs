using MediatR;

namespace IntelliPM.Application.FeatureFlags.Queries;

/// <summary>
/// Query to get all feature flags, optionally filtered by organization ID.
/// </summary>
public record GetAllFeatureFlagsQuery(int? OrganizationId = null) : IRequest<List<FeatureFlagDto>>;

/// <summary>
/// Data transfer object for feature flag information.
/// </summary>
public record FeatureFlagDto(
    Guid Id,
    string Name,
    bool IsEnabled,
    int? OrganizationId,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsGlobal,
    bool IsOrganizationSpecific
);

