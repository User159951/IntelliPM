using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Permissions.Queries;

public record PermissionDto(int Id, string Name, string Category, string? Description);

public record PermissionsMatrixDto(
    List<PermissionDto> Permissions,
    Dictionary<string, List<int>> RolePermissions
);

public record GetPermissionsMatrixQuery() : IRequest<PermissionsMatrixDto>;

