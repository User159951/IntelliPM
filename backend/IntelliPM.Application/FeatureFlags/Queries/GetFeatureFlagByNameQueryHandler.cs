using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.FeatureFlags.Queries;

/// <summary>
/// Handler for getting a feature flag by name.
/// </summary>
public class GetFeatureFlagByNameQueryHandler : IRequestHandler<GetFeatureFlagByNameQuery, FeatureFlagDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFeatureFlagByNameQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<FeatureFlagDto?> Handle(GetFeatureFlagByNameQuery request, CancellationToken cancellationToken)
    {
        var featureFlagRepo = _unitOfWork.Repository<FeatureFlag>();

        FeatureFlag? featureFlag = null;

        if (request.OrganizationId.HasValue)
        {
            // First try to get organization-specific flag
            featureFlag = await featureFlagRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Name == request.Name && f.OrganizationId == request.OrganizationId.Value, cancellationToken);

            // If not found, fall back to global flag
            if (featureFlag == null)
            {
                featureFlag = await featureFlagRepo.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Name == request.Name && f.OrganizationId == null, cancellationToken);
            }
        }
        else
        {
            // Only get global flag
            featureFlag = await featureFlagRepo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Name == request.Name && f.OrganizationId == null, cancellationToken);
        }

        if (featureFlag == null)
        {
            return null;
        }

        return new FeatureFlagDto(
            featureFlag.Id,
            featureFlag.Name,
            featureFlag.IsEnabled,
            featureFlag.OrganizationId,
            featureFlag.Description,
            featureFlag.CreatedAt,
            featureFlag.UpdatedAt,
            featureFlag.OrganizationId == null,
            featureFlag.OrganizationId != null
        );
    }
}

