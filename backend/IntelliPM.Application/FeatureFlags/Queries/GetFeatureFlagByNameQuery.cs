using MediatR;

namespace IntelliPM.Application.FeatureFlags.Queries;

/// <summary>
/// Query to get a feature flag by name, optionally filtered by organization ID.
/// </summary>
public record GetFeatureFlagByNameQuery(string Name, int? OrganizationId = null) : IRequest<FeatureFlagDto?>;

