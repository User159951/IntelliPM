using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Organizations.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Handler for GetMyOrganizationPermissionPolicyQuery.
/// Returns the current user's organization permission policy (Admin only).
/// If no policy exists, returns default values (all permissions allowed).
/// </summary>
public class GetMyOrganizationPermissionPolicyQueryHandler : IRequestHandler<GetMyOrganizationPermissionPolicyQuery, OrganizationPermissionPolicyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyOrganizationPermissionPolicyQueryHandler> _logger;

    public GetMyOrganizationPermissionPolicyQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetMyOrganizationPermissionPolicyQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrganizationPermissionPolicyDto> Handle(GetMyOrganizationPermissionPolicyQuery request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can view organization permission policy");
        }

        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated or organization not found");
        }

        // Verify organization exists
        var organization = await _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId, ct);

        if (organization == null)
        {
            throw new NotFoundException($"Organization {organizationId} not found");
        }

        // Get existing policy or return default
        var policy = await _unitOfWork.Repository<OrganizationPermissionPolicy>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId, ct);

        if (policy == null)
        {
            // Return default values (all permissions allowed)
            return new OrganizationPermissionPolicyDto(
                0, // Not persisted yet
                organization.Id,
                organization.Name,
                organization.Code,
                new List<string>(), // Empty list = all permissions allowed
                true, // Active by default
                DateTimeOffset.UtcNow,
                null
            );
        }

        return new OrganizationPermissionPolicyDto(
            policy.Id,
            policy.OrganizationId,
            organization.Name,
            organization.Code,
            policy.GetAllowedPermissions(),
            policy.IsActive,
            policy.CreatedAt,
            policy.UpdatedAt
        );
    }
}

