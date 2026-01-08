import { useQuery } from '@tanstack/react-query';
import { permissionsApi } from '@/api/permissions';
import { useMemo } from 'react';

interface ProjectPermissions {
  userRole: string | null;
  isLoading: boolean;
  canEditProject: boolean;
  canDeleteProject: boolean;
  canInviteMembers: boolean;
  canRemoveMembers: boolean;
  canChangeRoles: boolean;
  canCreateTasks: boolean;
  canEditTasks: boolean;
  canDeleteTasks: boolean;
  canManageSprints: boolean;
  canViewMilestones: boolean;
  canCreateMilestone: boolean;
  canEditMilestone: boolean;
  canCompleteMilestone: boolean;
  canDeleteMilestone: boolean;
  isViewer: boolean;
  isProductOwner: boolean;
  isScrumMaster: boolean;
}

/**
 * Custom hook for project-specific permission checking
 * 
 * Fetches permissions from API endpoint: GET /api/v1/projects/{id}/permissions
 * All permission checks are based on API-returned permissions, not hardcoded role logic.
 * 
 * @param projectId - Project ID to check permissions for
 * @returns ProjectPermissions object with permission flags and role information
 */
export function useProjectPermissions(projectId: number): ProjectPermissions {
  const { data: projectPermissionsData, isLoading } = useQuery({
    queryKey: ['project-permissions', projectId],
    queryFn: () => permissionsApi.getProjectPermissions(projectId),
    enabled: !!projectId && projectId > 0,
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });

  // Get permissions array from API response
  const permissions = useMemo((): string[] => {
    if (!projectPermissionsData) {
      return [];
    }
    return projectPermissionsData.permissions || [];
  }, [projectPermissionsData]);

  // Get project role from API response
  const userRole = projectPermissionsData?.projectRole || null;

  // Helper function to check if a permission exists in the permissions array
  const hasPermission = (permission: string): boolean => {
    return permissions.includes(permission);
  };

  // Calculate permission flags based on API-returned permissions
  const canEditProject = useMemo(() => {
    return hasPermission('projects.edit');
  }, [permissions]);

  const canDeleteProject = useMemo(() => {
    return hasPermission('projects.delete');
  }, [permissions]);

  const canInviteMembers = useMemo(() => {
    return hasPermission('projects.members.invite');
  }, [permissions]);

  const canRemoveMembers = useMemo(() => {
    return hasPermission('projects.members.remove');
  }, [permissions]);

  const canChangeRoles = useMemo(() => {
    return hasPermission('projects.members.changeRole');
  }, [permissions]);

  const canCreateTasks = useMemo(() => {
    return hasPermission('tasks.create');
  }, [permissions]);

  const canEditTasks = useMemo(() => {
    return hasPermission('tasks.edit');
  }, [permissions]);

  const canDeleteTasks = useMemo(() => {
    return hasPermission('tasks.delete');
  }, [permissions]);

  const canManageSprints = useMemo(() => {
    return hasPermission('sprints.manage') || hasPermission('sprints.create') || hasPermission('sprints.edit');
  }, [permissions]);

  const canViewMilestones = useMemo(() => {
    // All project members can view milestones if they can view the project
    return hasPermission('projects.view');
  }, [permissions]);

  const canCreateMilestone = useMemo(() => {
    // Check for milestone-specific permission or general project edit permission
    return hasPermission('milestones.create') || hasPermission('projects.edit');
  }, [permissions]);

  const canEditMilestone = useMemo(() => {
    return hasPermission('milestones.edit') || hasPermission('projects.edit');
  }, [permissions]);

  const canCompleteMilestone = useMemo(() => {
    return hasPermission('milestones.complete') || hasPermission('milestones.edit') || hasPermission('projects.edit');
  }, [permissions]);

  const canDeleteMilestone = useMemo(() => {
    return hasPermission('milestones.delete') || hasPermission('projects.delete');
  }, [permissions]);

  // Role checks based on API-returned role
  const isViewer = useMemo(() => {
    if (!userRole) return false;
    return userRole.toLowerCase() === 'viewer';
  }, [userRole]);

  const isProductOwner = useMemo(() => {
    if (!userRole) return false;
    return userRole.toLowerCase() === 'productowner';
  }, [userRole]);

  const isScrumMaster = useMemo(() => {
    if (!userRole) return false;
    return userRole.toLowerCase() === 'scrummaster';
  }, [userRole]);

  return {
    userRole,
    isLoading,
    canEditProject,
    canDeleteProject,
    canInviteMembers,
    canRemoveMembers,
    canChangeRoles,
    canCreateTasks,
    canEditTasks,
    canDeleteTasks,
    canManageSprints,
    canViewMilestones,
    canCreateMilestone,
    canEditMilestone,
    canCompleteMilestone,
    canDeleteMilestone,
    isViewer,
    isProductOwner,
    isScrumMaster,
  };
}
