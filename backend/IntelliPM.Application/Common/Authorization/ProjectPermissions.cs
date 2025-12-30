using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Common.Authorization;

public static class ProjectPermissions
{
    public static bool CanEditProject(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    public static bool CanDeleteProject(ProjectRole role) => 
        role == ProjectRole.ProductOwner;
    
    public static bool CanInviteMembers(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    public static bool CanRemoveMembers(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    public static bool CanChangeRoles(ProjectRole role) => 
        role == ProjectRole.ProductOwner;
    
    public static bool CanCreateTasks(ProjectRole role) => 
        role != ProjectRole.Viewer;
    
    public static bool CanEditTasks(ProjectRole role) => 
        role != ProjectRole.Viewer;
    
    public static bool CanDeleteTasks(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    public static bool CanManageSprints(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    public static bool CanViewOnly(ProjectRole role) => 
        role == ProjectRole.Viewer;
}

