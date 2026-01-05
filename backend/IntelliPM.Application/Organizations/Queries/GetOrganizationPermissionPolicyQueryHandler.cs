using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Organizations.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Handler for GetOrganizationPermissionPolicyQuery.
/// Returns organization permission policy (SuperAdmin only).
/// If no policy exists, returns default values (all permissions allowed).
/// </summary>
public class GetOrganizationPermissionPolicyQueryHandler : IRequestHandler<GetOrganizationPermissionPolicyQuery, OrganizationPermissionPolicyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetOrganizationPermissionPolicyQueryHandler> _logger;

    public GetOrganizationPermissionPolicyQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetOrganizationPermissionPolicyQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrganizationPermissionPolicyDto> Handle(GetOrganizationPermissionPolicyQuery request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can view organization permission policies");
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

        // Get existing policy or return default
        var policy = await _unitOfWork.Repository<OrganizationPermissionPolicy>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == request.OrganizationId, ct);

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

