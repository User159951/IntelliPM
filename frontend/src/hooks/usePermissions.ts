import { useMemo, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@/contexts/AuthContext';
import { usePermissionContext } from '@/contexts/PermissionContext';
import { permissionsApi } from '@/api/permissions';
import type { GlobalRole } from '@/types';

/**
 * Permission string format: "resource.action"
 * Examples: "projects.create", "tasks.delete", "users.update"
 */
export type Permission = string;

/**
 * Permission constants for consistent permission checking across the application
 * These are just constants for reference - actual permissions come from the API
 */
export const PERMISSIONS = {
  // User management permissions
  USERS_MANAGE: 'users.manage',
  USERS_CREATE: 'users.create',
  USERS_UPDATE: 'users.update',
  USERS_DELETE: 'users.delete',
  USERS_VIEW: 'users.view',

  // Admin permissions
  ADMIN_PANEL_VIEW: 'admin.panel.view',
  ADMIN_SETTINGS_UPDATE: 'admin.settings.update',
  ADMIN_PERMISSIONS_UPDATE: 'admin.permissions.update',

  // Project permissions
  PROJECTS_CREATE: 'projects.create',
  PROJECTS_VIEW: 'projects.view',
  PROJECTS_EDIT: 'projects.edit',
  PROJECTS_DELETE: 'projects.delete',
  PROJECTS_MEMBERS_INVITE: 'projects.members.invite',
  PROJECTS_MEMBERS_REMOVE: 'projects.members.remove',
  PROJECTS_MEMBERS_CHANGE_ROLE: 'projects.members.changeRole',

  // Task permissions
  TASKS_CREATE: 'tasks.create',
  TASKS_EDIT: 'tasks.edit',
  TASKS_DELETE: 'tasks.delete',
  TASKS_VIEW: 'tasks.view',
  TASKS_ASSIGN: 'tasks.assign',
  TASKS_COMMENT: 'tasks.comment',

  // Sprint permissions
  SPRINTS_CREATE: 'sprints.create',
  SPRINTS_EDIT: 'sprints.edit',
  SPRINTS_DELETE: 'sprints.delete',
  SPRINTS_MANAGE: 'sprints.manage',
  SPRINTS_VIEW: 'sprints.view',

  // Defect permissions
  DEFECTS_CREATE: 'defects.create',
  DEFECTS_EDIT: 'defects.edit',
  DEFECTS_DELETE: 'defects.delete',
  DEFECTS_VIEW: 'defects.view',

  // Backlog permissions
  BACKLOG_CREATE: 'backlog.create',
  BACKLOG_EDIT: 'backlog.edit',
  BACKLOG_DELETE: 'backlog.delete',
  BACKLOG_VIEW: 'backlog.view',
} as const;

/**
 * Interface for the usePermissions hook return value
 */
export interface UsePermissionsReturn {
  /**
   * Check if user has a specific permission
   * @param permission - Permission string in format "resource.action"
   * @returns true if user has the permission, false otherwise
   */
  can: (permission: Permission) => boolean;

  /**
   * Alias for can() - Check if user has a specific permission
   * @param permission - Permission string in format "resource.action"
   * @returns true if user has the permission, false otherwise
   */
  hasPermission: (permission: Permission) => boolean;

  /**
   * Check if user has any of the specified permissions
   * @param permissions - Array of permission strings
   * @returns true if user has at least one of the permissions
   */
  canAny: (permissions: Permission[]) => boolean;

  /**
   * Alias for canAny() - Check if user has any of the specified permissions
   * @param permissions - Array of permission strings
   * @returns true if user has at least one of the permissions
   */
  hasAnyPermission: (permissions: Permission[]) => boolean;

  /**
   * Check if user has all of the specified permissions
   * @param permissions - Array of permission strings
   * @returns true if user has all of the permissions
   */
  canAll: (permissions: Permission[]) => boolean;

  /**
   * Alias for canAll() - Check if user has all of the specified permissions
   * @param permissions - Array of permission strings
   * @returns true if user has all of the permissions
   */
  hasAllPermissions: (permissions: Permission[]) => boolean;

  /**
   * Check if user has a specific role (case-insensitive)
   * @param role - Role string to check
   * @returns true if user has the role
   */
  hasRole: (role: string) => boolean;

  /**
   * Check if user has a specific global role
   * @param role - Global role to check ('Admin' or 'User')
   * @returns true if user has the global role
   */
  hasGlobalRole: (role: GlobalRole) => boolean;

  /**
   * Check if user has a specific project role in a project
   * @param projectId - Project ID (as string or number)
   * @param role - Project role to check ('Owner' maps to 'ProductOwner', 'Member' maps to 'Developer'/'ScrumMaster'/'Tester', 'Viewer' maps to 'Viewer')
   * @returns true if user has the project role
   */
  hasProjectRole: (projectId: string | number, role: 'Owner' | 'Member' | 'Viewer') => boolean;

  /**
   * Specific permission flags for common operations
   */
  canCreateProject: boolean;
  canManageUsers: boolean;
  canManageSettings: boolean;
  canViewAdminPanel: boolean;

  /**
   * Loading state - true when fetching permissions from API
   */
  isLoading: boolean;

  /**
   * Error state - Error object if any error occurred, null otherwise
   */
  error: Error | null;
}

/**
 * Custom hook for global permission checking
 * 
 * Provides methods to check user permissions based on API-returned permissions.
 * All permissions are fetched from the API endpoint: GET /api/v1/permissions/me
 * 
 * @example
 * ```tsx
 * // Basic usage with permission checking
 * const { hasPermission, canCreateProject, isLoading } = usePermissions();
 * 
 * if (hasPermission('projects.create')) {
 *   // Show create button
 * }
 * 
 * // Using convenience flags
 * if (canManageUsers) {
 *   return <UserManagementPanel />;
 * }
 * 
 * // Using permission constants
 * import { PERMISSIONS } from '@/hooks/usePermissions';
 * 
 * if (hasPermission(PERMISSIONS.USERS_MANAGE)) {
 *   // Show user management features
 * }
 * 
 * // Check multiple permissions
 * const { hasAnyPermission, hasAllPermissions } = usePermissions();
 * 
 * if (hasAnyPermission(['projects.create', 'projects.edit'])) {
 *   // User can create or edit projects
 * }
 * 
 * if (hasAllPermissions(['users.create', 'users.update'])) {
 *   // User can both create and update users
 * }
 * ```
 * 
 * @returns UsePermissionsReturn object with permission checking methods and convenience flags
 */
export function usePermissions(): UsePermissionsReturn {
  const { user, isLoading: authLoading } = useAuth();
  usePermissionContext(); // Initialize context

  // Fetch permissions from API
  const {
    data: permissionsData,
    isLoading: permissionsLoading,
    error: permissionsError,
  } = useQuery({
    queryKey: ['permissions', 'me'],
    queryFn: () => permissionsApi.getMyPermissions(),
    enabled: !!user,
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });

  // Get permissions array from API response
  const permissions = useMemo((): Permission[] => {
    if (!permissionsData) {
      return [];
    }
    return permissionsData.permissions || [];
  }, [permissionsData]);

  /**
   * Check if user has a specific permission
   */
  const can = useCallback(
    (permission: Permission): boolean => {
      if (!user) {
        return false;
      }

      // Check permissions from API
      return permissions.includes(permission);
    },
    [user, permissions]
  );

  /**
   * Check if user has any of the specified permissions
   */
  const canAny = useCallback(
    (permissionList: Permission[]): boolean => {
      if (!user || permissionList.length === 0) {
        return false;
      }

      return permissionList.some((permission) => can(permission));
    },
    [user, can]
  );

  /**
   * Check if user has all of the specified permissions
   */
  const canAll = useCallback(
    (permissionList: Permission[]): boolean => {
      if (!user || permissionList.length === 0) {
        return false;
      }

      return permissionList.every((permission) => can(permission));
    },
    [user, can]
  );

  /**
   * Check if user has a specific role (case-insensitive)
   * Note: This checks global role only. For project roles, use hasProjectRole.
   */
  const hasRole = useCallback(
    (role: string): boolean => {
      if (!user) {
        return false;
      }

      const normalizedRole = role.toLowerCase();
      return user.globalRole.toLowerCase() === normalizedRole;
    },
    [user]
  );

  /**
   * Check if user has a specific global role
   */
  const hasGlobalRole = useCallback(
    (role: GlobalRole): boolean => {
      if (!user) {
        return false;
      }

      return user.globalRole === role;
    },
    [user]
  );

  /**
   * Check if user has a specific project role
   * Note: This method requires project role data to be fetched separately.
   * For project-specific role checking, use `usePermissionsWithProject` hook instead.
   * 
   * @param _projectId - Project ID (not used in base hook, kept for API compatibility)
   * @param _role - Project role to check (not used in base hook, kept for API compatibility)
   * @returns Always returns false in base hook - use usePermissionsWithProject for actual checks
   */
  const hasProjectRole = useCallback(
    (_projectId: string | number, _role: 'Owner' | 'Member' | 'Viewer'): boolean => {
      // Base hook doesn't have project role data
      // Use usePermissionsWithProject hook for project-specific role checking
      return false;
    },
    []
  );

  // Calculate loading state
  const isLoading = authLoading || permissionsLoading;

  // Calculate error state
  const error: Error | null = permissionsError as Error | null;

  // Calculate specific permission flags (cached with useMemo)
  const canCreateProject = useMemo(() => {
    return can(PERMISSIONS.PROJECTS_CREATE);
  }, [can]);

  const canManageUsers = useMemo(() => {
    return (
      can(PERMISSIONS.USERS_MANAGE) ||
      can(PERMISSIONS.USERS_CREATE) ||
      can(PERMISSIONS.USERS_UPDATE) ||
      can(PERMISSIONS.USERS_DELETE)
    );
  }, [can]);

  const canManageSettings = useMemo(() => {
    return can(PERMISSIONS.ADMIN_SETTINGS_UPDATE);
  }, [can]);

  const canViewAdminPanel = useMemo(() => {
    return (
      can(PERMISSIONS.ADMIN_PANEL_VIEW) ||
      can(PERMISSIONS.ADMIN_SETTINGS_UPDATE) ||
      can(PERMISSIONS.ADMIN_PERMISSIONS_UPDATE)
    );
  }, [can]);

  return {
    can,
    hasPermission: can, // Alias for can()
    canAny,
    hasAnyPermission: canAny, // Alias for canAny()
    canAll,
    hasAllPermissions: canAll, // Alias for canAll()
    hasRole,
    hasGlobalRole,
    hasProjectRole,
    canCreateProject,
    canManageUsers,
    canManageSettings,
    canViewAdminPanel,
    isLoading,
    error,
  };
}

/**
 * Hook variant that includes project-specific permissions
 * Use this when you need to check permissions for a specific project
 * 
 * @param projectId - Project ID to check permissions for
 * @returns UsePermissionsReturn with project-specific permissions included
 */
export function usePermissionsWithProject(projectId: number | string | null | undefined): UsePermissionsReturn {
  const basePermissions = usePermissions();
  usePermissionContext(); // Initialize context

  const projectIdNum = projectId ? (typeof projectId === 'string' ? parseInt(projectId, 10) : projectId) : null;

  // Fetch project permissions from API
  const {
    data: projectPermissionsData,
    isLoading: projectPermissionsLoading,
    error: projectPermissionsError,
  } = useQuery({
    queryKey: ['project-permissions', projectIdNum],
    queryFn: () => permissionsApi.getProjectPermissions(projectIdNum!),
    enabled: !!projectIdNum && !isNaN(projectIdNum),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });

  // Get project permissions array from API response
  const projectPermissions = useMemo((): Permission[] => {
    if (!projectPermissionsData) {
      return [];
    }
    return projectPermissionsData.permissions || [];
  }, [projectPermissionsData]);

  // Enhanced can function that includes project permissions
  const can = useCallback(
    (permission: Permission): boolean => {
      // Check base permissions first
      if (basePermissions.can(permission)) {
        return true;
      }

      // Check project-specific permissions from API
      return projectPermissions.includes(permission);
    },
    [basePermissions, projectPermissions]
  );

  // Enhanced hasProjectRole that uses API data
  const hasProjectRole = useCallback(
    (pid: string | number, role: 'Owner' | 'Member' | 'Viewer'): boolean => {
      // Verify the projectId matches
      const pidNum = typeof pid === 'string' ? parseInt(pid, 10) : pid;
      if (pidNum !== projectIdNum) {
        return false;
      }

      if (!projectPermissionsData) {
        return false;
      }

      const projectRole = projectPermissionsData.projectRole;
      if (!projectRole) {
        return false;
      }

      // Normalize role names
      const normalizedRole = projectRole.toLowerCase();
      const checkRole = role.toLowerCase();

      // Map simplified role names to actual project roles
      if (checkRole === 'owner') {
        return normalizedRole === 'productowner';
      }
      if (checkRole === 'member') {
        return ['scrummaster', 'developer', 'tester'].includes(normalizedRole);
      }
      if (checkRole === 'viewer') {
        return normalizedRole === 'viewer';
      }

      return normalizedRole === checkRole;
    },
    [projectPermissionsData, projectIdNum]
  );

  return {
    ...basePermissions,
    can,
    hasProjectRole,
    isLoading: basePermissions.isLoading || projectPermissionsLoading,
    error: basePermissions.error || (projectPermissionsError as Error | null),
  };
}
