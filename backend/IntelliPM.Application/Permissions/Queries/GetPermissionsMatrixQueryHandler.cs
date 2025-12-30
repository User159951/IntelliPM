using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Permissions.Queries;

public class GetPermissionsMatrixQueryHandler : IRequestHandler<GetPermissionsMatrixQuery, PermissionsMatrixDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPermissionsMatrixQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PermissionsMatrixDto> Handle(GetPermissionsMatrixQuery request, CancellationToken cancellationToken)
    {
        var permissionRepo = _unitOfWork.Repository<Permission>();
        var rolePermissionRepo = _unitOfWork.Repository<RolePermission>();

        var permissions = await permissionRepo.Query()
            .AsNoTracking()
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionDto(p.Id, p.Name, p.Category, p.Description))
            .ToListAsync(cancellationToken);

        var rolePermissionList = await rolePermissionRepo.Query()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rolePermissions = rolePermissionList
            .GroupBy(rp => rp.Role)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Select(rp => rp.PermissionId).ToList());

        // Ensure dictionary contains both roles
        if (!rolePermissions.ContainsKey(GlobalRole.Admin.ToString()))
        {
            rolePermissions[GlobalRole.Admin.ToString()] = new List<int>();
        }
        if (!rolePermissions.ContainsKey(GlobalRole.User.ToString()))
        {
            rolePermissions[GlobalRole.User.ToString()] = new List<int>();
        }

        return new PermissionsMatrixDto(permissions, rolePermissions);
    }
}

