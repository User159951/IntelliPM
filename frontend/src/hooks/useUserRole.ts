import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@/contexts/AuthContext';
import { memberService } from '@/api/memberService';
import type { GlobalRole, ProjectRole } from '@/types';

/**
 * Hook return type for useUserRole
 */
export interface UseUserRoleReturn {
  /**
   * Current user's global role (Admin, User, SuperAdmin)
   */
  globalRole: GlobalRole | null;

  /**
   * Current user's project role for a specific project
   */
  projectRole: ProjectRole | null;

  /**
   * Check if user has a specific global role
   */
  hasGlobalRole: (role: GlobalRole) => boolean;

  /**
   * Check if user has a specific project role
   */
  hasProjectRole: (role: ProjectRole | 'Owner' | 'Member' | 'Viewer') => boolean;

  /**
   * Check if user is Admin or SuperAdmin
   */
  isAdmin: boolean;

  /**
   * Check if user is SuperAdmin
   */
  isSuperAdmin: boolean;

  /**
   * Check if user is ProductOwner in the project
   */
  isProductOwner: boolean;

  /**
   * Check if user is ScrumMaster in the project
   */
  isScrumMaster: boolean;

  /**
   * Check if user is Developer or Tester (member-level role)
   */
  isMember: boolean;

  /**
   * Check if user is Viewer (read-only role)
   */
  isViewer: boolean;

  /**
   * Loading state for project role fetch
   */
  isLoading: boolean;

  /**
   * Error state for project role fetch
   */
  error: Error | null;
}

/**
 * Maps simplified role names to ProjectRole enum values
 */
function normalizeProjectRole(role: ProjectRole | 'Owner' | 'Member' | 'Viewer'): ProjectRole | null {
  switch (role) {
    case 'Owner':
    case 'ProductOwner':
      return 'ProductOwner';
    case 'Member':
    case 'ScrumMaster':
    case 'Developer':
    case 'Tester':
      return role as ProjectRole;
    case 'Viewer':
      return 'Viewer';
    default:
      return null;
  }
}

/**
 * Custom hook for role-based UI customization
 * 
 * Provides role information for both global and project-specific contexts.
 * Use this hook to conditionally render UI sections based on user roles.
 * 
 * @param projectId - Optional project ID to fetch project-specific role
 * @returns UseUserRoleReturn with role information and helper methods
 * 
 * @example
 * ```tsx
 * // Global role check
 * const { isAdmin, globalRole } = useUserRole();
 * 
 * if (isAdmin) {
 *   return <AdminPanel />;
 * }
 * 
 * // Project-specific role check
 * const { isScrumMaster, isProductOwner, projectRole } = useUserRole(projectId);
 * 
 * if (isScrumMaster) {
 *   return <SprintManagementPanel />;
 * }
 * 
 * if (isProductOwner) {
 *   return <ProductBacklogPanel />;
 * }
 * 
 * // Conditional rendering based on role
 * const { isMember, isViewer } = useUserRole(projectId);
 * 
 * {isMember && (
 *   <Button onClick={handleEdit}>Edit Task</Button>
 * )}
 * 
 * {isViewer && (
 *   <div>You have read-only access to this project</div>
 * )}
 * ```
 */
export function useUserRole(projectId?: number | string | null): UseUserRoleReturn {
  const { user } = useAuth();

  const projectIdNum = projectId 
    ? (typeof projectId === 'string' ? parseInt(projectId, 10) : projectId) 
    : null;

  // Fetch project role if projectId is provided
  const { 
    data: projectRole, 
    isLoading: projectRoleLoading, 
    error: projectRoleError 
  } = useQuery({
    queryKey: ['user-role', projectIdNum],
    queryFn: () => memberService.getUserRole(projectIdNum!),
    enabled: !!projectIdNum && !isNaN(projectIdNum),
  });

  // Global role from user context
  const globalRole = useMemo(() => {
    return user?.globalRole || null;
  }, [user]);

  // Project role from API
  const normalizedProjectRole = useMemo(() => {
    return projectRole || null;
  }, [projectRole]);

  // Helper methods
  const hasGlobalRole = useMemo(() => {
    return (role: GlobalRole): boolean => {
      return globalRole === role;
    };
  }, [globalRole]);

  const hasProjectRole = useMemo(() => {
    return (role: ProjectRole | 'Owner' | 'Member' | 'Viewer'): boolean => {
      if (!normalizedProjectRole) {
        return false;
      }

      const normalizedRole = normalizeProjectRole(role);
      if (normalizedRole === null) {
        return false;
      }

      // For 'Member', check if user has any member-level role
      if (role === 'Member') {
        return ['ScrumMaster', 'Developer', 'Tester'].includes(normalizedProjectRole);
      }

      return normalizedProjectRole === normalizedRole;
    };
  }, [normalizedProjectRole]);

  // Convenience flags
  const isAdmin = useMemo(() => {
    return globalRole === 'Admin' || globalRole === 'SuperAdmin';
  }, [globalRole]);

  const isSuperAdmin = useMemo(() => {
    return globalRole === 'SuperAdmin';
  }, [globalRole]);

  const isProductOwner = useMemo(() => {
    return normalizedProjectRole === 'ProductOwner';
  }, [normalizedProjectRole]);

  const isScrumMaster = useMemo(() => {
    return normalizedProjectRole === 'ScrumMaster';
  }, [normalizedProjectRole]);

  const isMember = useMemo(() => {
    return ['ScrumMaster', 'Developer', 'Tester'].includes(normalizedProjectRole || '');
  }, [normalizedProjectRole]);

  const isViewer = useMemo(() => {
    return normalizedProjectRole === 'Viewer';
  }, [normalizedProjectRole]);

  return {
    globalRole,
    projectRole: normalizedProjectRole,
    hasGlobalRole,
    hasProjectRole,
    isAdmin,
    isSuperAdmin,
    isProductOwner,
    isScrumMaster,
    isMember,
    isViewer,
    isLoading: projectRoleLoading,
    error: projectRoleError as Error | null,
  };
}

