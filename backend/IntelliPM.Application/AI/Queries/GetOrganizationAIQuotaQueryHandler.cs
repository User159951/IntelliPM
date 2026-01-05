using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for GetOrganizationAIQuotaQuery.
/// Returns organization AI quota (SuperAdmin only).
/// </summary>
public class GetOrganizationAIQuotaQueryHandler : IRequestHandler<GetOrganizationAIQuotaQuery, OrganizationAIQuotaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetOrganizationAIQuotaQueryHandler> _logger;

    public GetOrganizationAIQuotaQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetOrganizationAIQuotaQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrganizationAIQuotaDto> Handle(GetOrganizationAIQuotaQuery request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can view organization AI quotas");
        }

        // Verify organization exists
        var organization = await _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, ct);

        if (organization == null)
        {
            throw new NotFoundException($"Organization {request.OrganizationId} not found");
        }

        // Get or create default quota
        var quota = await _unitOfWork.Repository<OrganizationAIQuota>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.OrganizationId == request.OrganizationId, ct);

        if (quota == null)
        {
            // Return default values if quota doesn't exist yet
            // This allows the frontend to show defaults before creating the quota
            var defaultLimits = Domain.Constants.AIQuotaConstants.DefaultLimits[Domain.Constants.AIQuotaConstants.Tiers.Free];
            return new OrganizationAIQuotaDto(
                0, // Not persisted yet
                organization.Id,
                organization.Name,
                organization.Code,
                defaultLimits.MaxTokensPerPeriod,
                defaultLimits.MaxRequestsPerPeriod,
                null, // Reset on first day of month
                true, // AI enabled by default
                DateTimeOffset.UtcNow,
                null
            );
        }

        return new OrganizationAIQuotaDto(
            quota.Id,
            quota.OrganizationId,
            organization.Name,
            organization.Code,
            quota.MonthlyTokenLimit,
            quota.MonthlyRequestLimit,
            quota.ResetDayOfMonth,
            quota.IsAIEnabled,
            quota.CreatedAt,
            quota.UpdatedAt
        );
    }
}

