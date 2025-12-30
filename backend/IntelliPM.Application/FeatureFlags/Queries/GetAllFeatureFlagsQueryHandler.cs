using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.FeatureFlags.Queries;

/// <summary>
/// Handler for getting all feature flags.
/// </summary>
public class GetAllFeatureFlagsQueryHandler : IRequestHandler<GetAllFeatureFlagsQuery, List<FeatureFlagDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllFeatureFlagsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<List<FeatureFlagDto>> Handle(GetAllFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        var featureFlagRepo = _unitOfWork.Repository<FeatureFlag>();

        var query = featureFlagRepo.Query()
            .AsNoTracking();

        // Filter by organization ID if provided
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(f => f.OrganizationId == request.OrganizationId.Value || f.OrganizationId == null);
        }

        var featureFlags = await query
            .OrderBy(f => f.Name)
            .ThenBy(f => f.OrganizationId)
            .Select(f => new FeatureFlagDto(
                f.Id,
                f.Name,
                f.IsEnabled,
                f.OrganizationId,
                f.Description,
                f.CreatedAt,
                f.UpdatedAt,
                f.IsGlobal,
                f.IsOrganizationSpecific
            ))
            .ToListAsync(cancellationToken);

        return featureFlags;
    }
}

