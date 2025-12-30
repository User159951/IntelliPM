using MediatR;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Permissions.Commands;

public record UpdateRolePermissionsCommand(
    GlobalRole Role,
    List<int> PermissionIds
) : IRequest<Unit>;

