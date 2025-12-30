import { useMemo, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@/contexts/AuthContext';
import { memberService } from '@/api/memberService';
import type { GlobalRole, ProjectRole } from '@/types';

/**
 * Permission string format: "resource.action"
 * Examples: "projects.create", "tasks.delete", "users.update"
 */
export type Permission = string;

/**
 * Permission constants for consistent permission checking across the application
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
   * Loading state - true when fetching user or project role data
   */
  isLoading: boolean;

  /**
   * Error state - Error object if any error occurred, null otherwise
   */
  error: Error | null;
}

/**
 * Maps project role to permission strings
 * @param role - Project role
 * @returns Array of permission strings
 */
function getProjectRolePermissions(role: ProjectRole | null | undefined): Permission[] {
  if (!role) {
    return [];
  }

  const permissions: Permission[] = [];

  switch (role) {
    case 'ProductOwner':
      // ProductOwner has all project permissions
      permissions.push(
        'projects.view',
        'projects.edit',
        'projects.delete',
        'projects.members.invite',
        'projects.members.remove',
        'projects.members.changeRole',
        'tasks.create',
        'tasks.edit',
        'tasks.delete',
        'tasks.view',
        'tasks.assign',
        'tasks.comment',
        'sprints.create',
        'sprints.edit',
        'sprints.delete',
        'sprints.manage',
        'defects.create',
        'defects.edit',
        'defects.delete',
        'defects.view',
        'backlog.create',
        'backlog.edit',
        'backlog.delete',
        'backlog.view'
      );
      break;

    case 'ScrumMaster':
      // ScrumMaster has most permissions except delete project and change roles
      permissions.push(
        'projects.view',
        'projects.edit',
        'projects.members.invite',
        'projects.members.remove',
        'tasks.create',
        'tasks.edit',
        'tasks.delete',
        'tasks.view',
        'tasks.assign',
        'tasks.comment',
        'sprints.create',
        'sprints.edit',
        'sprints.delete',
        'sprints.manage',
        'defects.create',
        'defects.edit',
        'defects.view',
        'backlog.view'
      );
      break;

    case 'Developer':
    case 'Tester':
      // Developer and Tester have member-level permissions
      permissions.push(
        'projects.view',
        'tasks.create',
        'tasks.edit',
        'tasks.view',
        'tasks.assign',
        'tasks.comment',
        'defects.create',
        'defects.edit',
        'defects.view',
        'backlog.view'
      );
      break;

    case 'Viewer':
      // Viewer has read-only permissions
      permissions.push(
        'projects.view',
        'tasks.view',
        'defects.view',
        'backlog.view',
        'sprints.view'
      );
      break;
  }

  return permissions;
}

/**
 * Maps role string to ProjectRole enum value
 * Supports both actual roles and simplified role names
 */
function normalizeProjectRole(role: 'Owner' | 'Member' | 'Viewer' | ProjectRole): ProjectRole | null {
  switch (role) {
    case 'Owner':
    case 'ProductOwner':
      return 'ProductOwner';
    case 'Member':
    case 'ScrumMaster':
    case 'Developer':
    case 'Tester':
      // For 'Member', we check if user has any of these roles
      // Return the role as-is for member-level roles
      return role as ProjectRole;
    case 'Viewer':
      return 'Viewer';
    default:
      return null;
  }
}

/**
 * Custom hook for global permission checking
 * 
 * Provides methods to check user permissions based on:
 * - Global role (Admin/User)
 * - Explicit permissions from user.permissions array
 * 
 * Admins have all permissions automatically. Regular users have permissions
 * based on their role and explicit permissions from the backend.
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

  /**
   * Calculate all permissions for the current user
   */
  const allPermissions = useMemo((): Permission[] => {
    if (!user) {
      return [];
    }

    const permissions: Permission[] = [];

    // Admin has all permissions
    if (user.globalRole === 'Admin') {
      // Add all possible permissions for admin
      permissions.push(
        PERMISSIONS.USERS_MANAGE,
        PERMISSIONS.USERS_CREATE,
        PERMISSIONS.USERS_UPDATE,
        PERMISSIONS.USERS_DELETE,
        PERMISSIONS.USERS_VIEW,
        PERMISSIONS.ADMIN_PANEL_VIEW,
        PERMISSIONS.ADMIN_SETTINGS_UPDATE,
        PERMISSIONS.ADMIN_PERMISSIONS_UPDATE,
        PERMISSIONS.PROJECTS_CREATE,
        PERMISSIONS.PROJECTS_VIEW,
        PERMISSIONS.PROJECTS_EDIT,
        PERMISSIONS.PROJECTS_DELETE,
        PERMISSIONS.PROJECTS_MEMBERS_INVITE,
        PERMISSIONS.PROJECTS_MEMBERS_REMOVE,
        PERMISSIONS.PROJECTS_MEMBERS_CHANGE_ROLE,
        PERMISSIONS.TASKS_CREATE,
        PERMISSIONS.TASKS_EDIT,
        PERMISSIONS.TASKS_DELETE,
        PERMISSIONS.TASKS_VIEW,
        PERMISSIONS.TASKS_ASSIGN,
        PERMISSIONS.TASKS_COMMENT,
        PERMISSIONS.SPRINTS_CREATE,
        PERMISSIONS.SPRINTS_EDIT,
        PERMISSIONS.SPRINTS_DELETE,
        PERMISSIONS.SPRINTS_MANAGE,
        PERMISSIONS.SPRINTS_VIEW,
        PERMISSIONS.DEFECTS_CREATE,
        PERMISSIONS.DEFECTS_EDIT,
        PERMISSIONS.DEFECTS_DELETE,
        PERMISSIONS.DEFECTS_VIEW,
        PERMISSIONS.BACKLOG_CREATE,
        PERMISSIONS.BACKLOG_EDIT,
        PERMISSIONS.BACKLOG_DELETE,
        PERMISSIONS.BACKLOG_VIEW
      );
    } else {
      // Regular users have basic read-only permissions by default
      permissions.push(
        PERMISSIONS.PROJECTS_VIEW,
        PERMISSIONS.TASKS_VIEW,
        PERMISSIONS.DEFECTS_VIEW,
        PERMISSIONS.BACKLOG_VIEW,
        PERMISSIONS.SPRINTS_VIEW
      );

      // Regular users can create projects by default (unless restricted by backend)
      // This can be controlled by backend settings (e.g., ProjectCreation.AllowedRoles)
      permissions.push(PERMISSIONS.PROJECTS_CREATE);

      // Add permissions from user.permissions array (if backend provides explicit permissions)
      if (user.permissions && Array.isArray(user.permissions)) {
        permissions.push(...user.permissions);
      }
    }

    return [...new Set(permissions)]; // Remove duplicates
  }, [user]);

  /**
   * Check if user has a specific permission
   */
  const can = useCallback(
    (permission: Permission): boolean => {
      if (!user) {
        return false;
      }

      // Admin has all permissions
      if (user.globalRole === 'Admin') {
        return true;
      }

      // Check explicit permissions
      return allPermissions.includes(permission);
    },
    [user, allPermissions]
  );

  /**
   * Check if user has any of the specified permissions
   */
  const canAny = useCallback(
    (permissions: Permission[]): boolean => {
      if (!user || permissions.length === 0) {
        return false;
      }

      return permissions.some((permission) => can(permission));
    },
    [user, can]
  );

  /**
   * Check if user has all of the specified permissions
   */
  const canAll = useCallback(
    (permissions: Permission[]): boolean => {
      if (!user || permissions.length === 0) {
        return false;
      }

      return permissions.every((permission) => can(permission));
    },
    [user, can]
  );

  /**
   * Check if user has a specific role (case-insensitive)
   */
  const hasRole = useCallback(
    (role: string): boolean => {
      if (!user) {
        return false;
      }

      const normalizedRole = role.toLowerCase();

      // Check global role
      if (normalizedRole === user.globalRole.toLowerCase()) {
        return true;
      }

      // Check if role matches any project role (we can't check without projectId)
      // This is a simplified check - for project roles, use hasProjectRole
      return false;
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
  const isLoading = authLoading;

  // Calculate error state
  const error: Error | null = null; // AuthContext doesn't expose errors currently

  // Calculate specific permission flags (cached with useMemo)
  const canCreateProject = useMemo(() => {
    return can(PERMISSIONS.PROJECTS_CREATE);
  }, [can]);

  const canManageUsers = useMemo(() => {
    return can(PERMISSIONS.USERS_MANAGE) || can(PERMISSIONS.USERS_CREATE) || can(PERMISSIONS.USERS_UPDATE) || can(PERMISSIONS.USERS_DELETE);
  }, [can]);

  const canManageSettings = useMemo(() => {
    return can(PERMISSIONS.ADMIN_SETTINGS_UPDATE);
  }, [can]);

  const canViewAdminPanel = useMemo(() => {
    return can(PERMISSIONS.ADMIN_PANEL_VIEW) || can(PERMISSIONS.ADMIN_SETTINGS_UPDATE) || can(PERMISSIONS.ADMIN_PERMISSIONS_UPDATE);
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

  const projectIdNum = projectId ? (typeof projectId === 'string' ? parseInt(projectId, 10) : projectId) : null;

  // Fetch project role
  const { data: projectRole, isLoading: projectRoleLoading, error: projectRoleError } = useQuery({
    queryKey: ['user-role', projectIdNum],
    queryFn: () => memberService.getUserRole(projectIdNum!),
    enabled: !!projectIdNum && !isNaN(projectIdNum),
  });

  // Calculate project-specific permissions
  const projectPermissions = useMemo((): Permission[] => {
    if (!projectRole) {
      return [];
    }

    return getProjectRolePermissions(projectRole);
  }, [projectRole]);

  // Enhanced can function that includes project permissions
  const can = useCallback(
    (permission: Permission): boolean => {
      // Check base permissions first
      if (basePermissions.can(permission)) {
        return true;
      }

      // Check project-specific permissions
      return projectPermissions.includes(permission);
    },
    [basePermissions, projectPermissions]
  );

  // Enhanced hasProjectRole that actually works
  const hasProjectRole = useCallback(
    (pid: string | number, role: 'Owner' | 'Member' | 'Viewer'): boolean => {
      // Verify the projectId matches
      const pidNum = typeof pid === 'string' ? parseInt(pid, 10) : pid;
      if (pidNum !== projectIdNum) {
        return false;
      }

      if (!projectRole) {
        return false;
      }

      const normalizedRole = normalizeProjectRole(role);

      if (normalizedRole === null) {
        return false;
      }

      // For 'Member', check if user has any member-level role
      if (role === 'Member') {
        return ['ScrumMaster', 'Developer', 'Tester'].includes(projectRole);
      }

      return projectRole === normalizedRole;
    },
    [projectRole, projectIdNum]
  );

  return {
    ...basePermissions,
    can,
    hasProjectRole,
    isLoading: basePermissions.isLoading || projectRoleLoading,
    error: basePermissions.error || (projectRoleError as Error | null),
  };
}

