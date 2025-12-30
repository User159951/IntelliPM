using MediatR;
using IntelliPM.Application.FeatureFlags.Queries;

namespace IntelliPM.Application.FeatureFlags.Commands;

/// <summary>
/// Command to update an existing feature flag.
/// </summary>
public record UpdateFeatureFlagCommand(
    Guid Id,
    bool? IsEnabled,
    string? Description
) : IRequest<FeatureFlagDto>;

