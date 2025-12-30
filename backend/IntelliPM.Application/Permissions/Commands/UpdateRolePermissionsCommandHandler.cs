using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Permissions.Commands;

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public UpdateRolePermissionsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    public async Task<Unit> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetUserId();

        // Permission check
        var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, "admin.permissions.update", cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedException("You don't have permission to update role permissions");
        }

        // Validate role (only Admin/User supported for now)
        if (request.Role != GlobalRole.Admin && request.Role != GlobalRole.User)
        {
            throw new ValidationException("Unsupported role");
        }

        var requestedIds = request.PermissionIds?.Distinct().ToList() ?? new List<int>();

        var permissionRepo = _unitOfWork.Repository<Permission>();
        var rolePermissionRepo = _unitOfWork.Repository<RolePermission>();

        // Validate that permissions exist
        var existingPermissionIds = await permissionRepo.Query()
            .Where(p => requestedIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissionIds.Count != requestedIds.Count)
        {
            var missing = requestedIds.Except(existingPermissionIds).ToList();
            throw new ValidationException("Some permissions do not exist")
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "PermissionIds", new[] { $"Invalid permission IDs: {string.Join(", ", missing)}" } }
                }
            };
        }

        // Remove existing role-permissions for this role
        var existingRolePermissions = await rolePermissionRepo.Query()
            .Where(rp => rp.Role == request.Role)
            .ToListAsync(cancellationToken);

        foreach (var rp in existingRolePermissions)
        {
            rolePermissionRepo.Delete(rp);
        }

        // Add new role-permissions
        var now = DateTimeOffset.UtcNow;
        var newRolePermissions = requestedIds.Select(id => new RolePermission
        {
            Role = request.Role,
            PermissionId = id,
            CreatedAt = now
        });

        foreach (var rp in newRolePermissions)
        {
            await rolePermissionRepo.AddAsync(rp, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

