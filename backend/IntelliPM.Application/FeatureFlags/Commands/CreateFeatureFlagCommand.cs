using MediatR;
using IntelliPM.Application.FeatureFlags.Queries;

namespace IntelliPM.Application.FeatureFlags.Commands;

/// <summary>
/// Command to create a new feature flag.
/// </summary>
public record CreateFeatureFlagCommand(
    string Name,
    string? Description,
    bool IsEnabled,
    int? OrganizationId
) : IRequest<FeatureFlagDto>;

