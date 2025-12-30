using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Common.Authorization;

public static class GlobalPermissions
{
    public static bool CanManageUsers(GlobalRole role) => role == GlobalRole.Admin;
    public static bool CanAccessSystem(GlobalRole role, bool isActive) => isActive;
    public static bool CanManageGlobalSettings(GlobalRole role) => role == GlobalRole.Admin;
    public static bool CanViewAllProjects(GlobalRole role) => role == GlobalRole.Admin;
    public static bool CanDeleteAnyProject(GlobalRole role) => role == GlobalRole.Admin;
}

