using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Organizations.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Handler for UpsertOrganizationPermissionPolicyCommand.
/// Creates or updates organization permission policy (SuperAdmin only).
/// </summary>
public class UpsertOrganizationPermissionPolicyCommandHandler : IRequestHandler<UpsertOrganizationPermissionPolicyCommand, OrganizationPermissionPolicyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpsertOrganizationPermissionPolicyCommandHandler> _logger;

    public UpsertOrganizationPermissionPolicyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpsertOrganizationPermissionPolicyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrganizationPermissionPolicyDto> Handle(UpsertOrganizationPermissionPolicyCommand request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can manage organization permission policies");
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

        // Validate that all permissions exist in the system
        var allPermissions = await _unitOfWork.Repository<Permission>()
            .Query()
            .AsNoTracking()
            .Select(p => p.Name)
            .ToListAsync(ct);

        var invalidPermissions = request.AllowedPermissions
            .Except(allPermissions, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (invalidPermissions.Any())
        {
            throw new ValidationException($"Invalid permissions: {string.Join(", ", invalidPermissions)}")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "AllowedPermissions", new[] { $"The following permissions do not exist: {string.Join(", ", invalidPermissions)}" } }
                }
            };
        }

        var policyRepo = _unitOfWork.Repository<OrganizationPermissionPolicy>();
        var existingPolicy = await policyRepo
            .Query()
            .FirstOrDefaultAsync(p => p.OrganizationId == request.OrganizationId, ct);

        OrganizationPermissionPolicy policy;

        if (existingPolicy != null)
        {
            // Update existing policy
            existingPolicy.SetAllowedPermissions(request.AllowedPermissions);
            if (request.IsActive.HasValue)
            {
                existingPolicy.IsActive = request.IsActive.Value;
            }
            existingPolicy.UpdatedAt = DateTimeOffset.UtcNow;

            policyRepo.Update(existingPolicy);
            policy = existingPolicy;

            _logger.LogInformation(
                "Organization permission policy updated for organization {OrganizationId} by SuperAdmin {UserId}",
                request.OrganizationId, _currentUserService.GetUserId());
        }
        else
        {
            // Create new policy
            policy = new OrganizationPermissionPolicy
            {
                OrganizationId = request.OrganizationId,
                IsActive = request.IsActive ?? true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            };
            policy.SetAllowedPermissions(request.AllowedPermissions);

            await policyRepo.AddAsync(policy, ct);

            _logger.LogInformation(
                "Organization permission policy created for organization {OrganizationId} by SuperAdmin {UserId}",
                request.OrganizationId, _currentUserService.GetUserId());
        }

        await _unitOfWork.SaveChangesAsync(ct);

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

