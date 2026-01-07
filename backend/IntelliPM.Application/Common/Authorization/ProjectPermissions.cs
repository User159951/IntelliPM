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
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster or ProjectRole.Developer or ProjectRole.Tester;
    
    public static bool CanEditTasks(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster or ProjectRole.Developer or ProjectRole.Tester;
    
    public static bool CanDeleteTasks(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    public static bool CanCommentOnTasks(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster or ProjectRole.Developer or ProjectRole.Tester or ProjectRole.Manager;
    
    public static bool CanManageSprints(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    /// <summary>
    /// EXCLUSIVE permission for ScrumMaster to start sprints.
    /// ProductOwner cannot start sprints directly.
    /// </summary>
    public static bool CanStartSprint(ProjectRole role) => 
        role == ProjectRole.ScrumMaster;
    
    /// <summary>
    /// EXCLUSIVE permission for ScrumMaster to close sprints.
    /// ProductOwner cannot close sprints directly.
    /// </summary>
    public static bool CanCloseSprint(ProjectRole role) => 
        role == ProjectRole.ScrumMaster;
    
    /// <summary>
    /// EXCLUSIVE permission for Tester/QA to approve releases.
    /// This is a required gatekeeper step before deployment.
    /// </summary>
    public static bool CanApproveRelease(ProjectRole role) => 
        role == ProjectRole.Tester;
    
    /// <summary>
    /// EXCLUSIVE permission for Tester/QA to validate quality gates.
    /// QA must validate quality gates before release approval.
    /// </summary>
    public static bool CanValidateQualityGate(ProjectRole role) => 
        role == ProjectRole.Tester;
    
    /// <summary>
    /// Permission to validate and complete milestones.
    /// Available to ProductOwner, ScrumMaster, and Manager.
    /// </summary>
    public static bool CanValidateMilestone(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster or ProjectRole.Manager;
    
    public static bool CanViewOnly(ProjectRole role) => 
        role == ProjectRole.Viewer;
}

