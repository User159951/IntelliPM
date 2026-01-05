using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Common.Services;
using IntelliPM.Application.Permissions.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Permissions.Queries;

/// <summary>
/// Helper class for role permission information.
/// </summary>
internal class RolePermissionInfo
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
}

/// <summary>
/// Handler for GetMemberPermissionsQuery.
/// Returns a paginated list of organization members with their permissions (Admin only - own organization).
/// </summary>
public class GetMemberPermissionsQueryHandler : IRequestHandler<GetMemberPermissionsQuery, PagedResponse<MemberPermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<GetMemberPermissionsQueryHandler> _logger;

    public GetMemberPermissionsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        IPermissionService permissionService,
        ILogger<GetMemberPermissionsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<PagedResponse<MemberPermissionDto>> Handle(GetMemberPermissionsQuery request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can view member permissions");
        }

        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated or organization not found");
        }

        // Query users in the organization (with scoping)
        var userQuery = _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking();
        
        // Apply organization scoping (Admin sees only their org)
        userQuery = _scopingService.ApplyOrganizationScope(userQuery);
        
        // Include after scoping
        IQueryable<User> userQueryWithInclude = userQuery.Include(u => u.Organization);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            userQueryWithInclude = userQueryWithInclude.Where(u =>
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Username.ToLower().Contains(searchTerm));
        }

        // Get total count
        var totalCount = await userQueryWithInclude.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var users = await userQueryWithInclude
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Get all role-permission mappings for the roles present
        var roles = users.Select(u => u.GlobalRole).Distinct().ToList();
        var rolePermissions = await _unitOfWork.Repository<RolePermission>()
            .Query()
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => roles.Contains(rp.Role))
            .ToListAsync(ct);

        // Group permissions by role - use a concrete type to avoid anonymous type issues
        var permissionsByRole = rolePermissions
            .GroupBy(rp => rp.Role)
            .ToDictionary(
                g => g.Key,
                g => g.Select(rp => new RolePermissionInfo { PermissionId = rp.PermissionId, PermissionName = rp.Permission.Name }).ToList()
            );

        // Build DTOs
        var memberDtos = new List<MemberPermissionDto>();

        foreach (var user in users)
        {
            if (!permissionsByRole.TryGetValue(user.GlobalRole, out var rolePerms))
            {
                rolePerms = new List<RolePermissionInfo>();
            }
            
            var permissionNames = rolePerms.Select(p => p.PermissionName).ToList();
            var permissionIds = rolePerms.Select(p => p.PermissionId).ToList();

            var memberDto = new MemberPermissionDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                $"{user.FirstName} {user.LastName}",
                user.GlobalRole.ToString(),
                user.OrganizationId,
                user.Organization.Name,
                permissionNames,
                permissionIds
            );

            memberDtos.Add(memberDto);
        }

        return new PagedResponse<MemberPermissionDto>(
            memberDtos,
            page,
            pageSize,
            totalCount
        );
    }
}

