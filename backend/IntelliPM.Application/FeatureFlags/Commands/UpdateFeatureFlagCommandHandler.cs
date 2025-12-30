using MediatR;
using Microsoft.Extensions.Logging;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.FeatureFlags.Queries;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.FeatureFlags.Commands;

/// <summary>
/// Handler for updating an existing feature flag.
/// </summary>
public class UpdateFeatureFlagCommandHandler : IRequestHandler<UpdateFeatureFlagCommand, FeatureFlagDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFeatureFlagCommandHandler> _logger;

    public UpdateFeatureFlagCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateFeatureFlagCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FeatureFlagDto> Handle(UpdateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var featureFlagRepo = _unitOfWork.Repository<FeatureFlag>();

        var featureFlag = await featureFlagRepo.Query()
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (featureFlag == null)
        {
            throw new NotFoundException($"Feature flag with ID {request.Id} not found.");
        }

        // Update IsEnabled if provided
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value)
            {
                featureFlag.Enable();
            }
            else
            {
                featureFlag.Disable();
            }
        }

        // Update Description if provided
        if (request.Description != null)
        {
            featureFlag.UpdateDescription(request.Description);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Feature flag updated: {FeatureFlagName} (Id: {FeatureFlagId}, Enabled: {IsEnabled})",
            featureFlag.Name,
            featureFlag.Id,
            featureFlag.IsEnabled);

        return new FeatureFlagDto(
            featureFlag.Id,
            featureFlag.Name,
            featureFlag.IsEnabled,
            featureFlag.OrganizationId,
            featureFlag.Description,
            featureFlag.CreatedAt,
            featureFlag.UpdatedAt,
            featureFlag.IsGlobal,
            featureFlag.IsOrganizationSpecific
        );
    }
}

