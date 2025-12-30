using MediatR;
using Microsoft.Extensions.Logging;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.FeatureFlags.Queries;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.FeatureFlags.Commands;

/// <summary>
/// Handler for creating a new feature flag.
/// </summary>
public class CreateFeatureFlagCommandHandler : IRequestHandler<CreateFeatureFlagCommand, FeatureFlagDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFeatureFlagCommandHandler> _logger;

    public CreateFeatureFlagCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateFeatureFlagCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FeatureFlagDto> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var featureFlagRepo = _unitOfWork.Repository<FeatureFlag>();

        // Check if feature flag with same name and organization already exists
        var existingFlag = await featureFlagRepo.Query()
            .FirstOrDefaultAsync(f => 
                f.Name == request.Name && 
                f.OrganizationId == request.OrganizationId, 
                cancellationToken);

        if (existingFlag != null)
        {
            var scope = request.OrganizationId.HasValue 
                ? $"organization {request.OrganizationId.Value}" 
                : "global";
            throw new ValidationException($"A feature flag with name '{request.Name}' already exists for {scope}.");
        }

        // Create new feature flag
        var featureFlag = FeatureFlag.Create(
            request.Name,
            request.OrganizationId,
            request.Description,
            request.IsEnabled);

        await featureFlagRepo.AddAsync(featureFlag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Feature flag created: {FeatureFlagName} (Id: {FeatureFlagId}, OrganizationId: {OrganizationId}, Enabled: {IsEnabled})",
            featureFlag.Name,
            featureFlag.Id,
            featureFlag.OrganizationId,
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

