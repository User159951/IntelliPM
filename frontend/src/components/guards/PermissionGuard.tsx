import React, { useRef, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { usePermissions, usePermissionsWithProject, PERMISSIONS } from '@/hooks/usePermissions';
import { useAuth } from '@/contexts/AuthContext';
import { showError } from "@/lib/sweetalert";
import { Skeleton } from '@/components/ui/skeleton';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

/**
 * Props for the PermissionGuard component
 */
export interface PermissionGuardProps {
  /**
   * Required permission string in format "resource.action"
   * Example: "projects.create", "tasks.delete", "users.update"
   */
  requiredPermission: string;

  /**
   * Optional project ID for project-specific permissions
   * If provided, uses usePermissionsWithProject to check project-level permissions
   * If not provided, uses base usePermissions hook
   */
  projectId?: number;

  /**
   * Children to render if user has the required permission
   */
  children: React.ReactNode;

  /**
   * Optional fallback content to render if user doesn't have permission
   * If not provided and redirectTo is not set, redirects to /dashboard
   */
  fallback?: React.ReactNode;

  /**
   * Optional redirect path when user doesn't have permission
   * If provided, user will be redirected to this path instead of showing fallback
   * Default: "/dashboard"
   */
  redirectTo?: string;

  /**
   * Optional flag to show a toast notification when access is denied
   * Default: true
   */
  showNotification?: boolean;

  /**
   * Optional custom loading component to show while checking permissions
   */
  loadingComponent?: React.ReactNode;

  /**
   * Optional tooltip text to show when permission is denied
   * If not provided, a default message will be shown
   */
  tooltipText?: string;

  /**
   * Optional flag to show tooltip instead of fallback/redirect
   * When true, wraps children in a tooltip that explains why action is unavailable
   * Default: false
   */
  showTooltip?: boolean;
}

/**
 * PermissionGuard component that conditionally renders children based on user permissions.
 * 
 * This component checks if the current user has the required permission and:
 * - Renders children if permission is granted
 * - Renders fallback or redirects if permission is denied
 * - Shows loading state while checking permissions
 * - Supports project-specific permissions when projectId is provided
 * 
 * @example
 * ```tsx
 * // Basic usage with fallback
 * <PermissionGuard requiredPermission="projects.create">
 *   <CreateProjectButton />
 * </PermissionGuard>
 * 
 * // With project-specific permission
 * <PermissionGuard 
 *   requiredPermission="projects.edit" 
 *   projectId={projectId}
 * >
 *   <EditProjectButton />
 * </PermissionGuard>
 * 
 * // With custom fallback
 * <PermissionGuard 
 *   requiredPermission="tasks.delete"
 *   projectId={projectId}
 *   fallback={<div>You don't have permission to delete tasks</div>}
 * >
 *   <DeleteTaskButton />
 * </PermissionGuard>
 * 
 * // With redirect
 * <PermissionGuard 
 *   requiredPermission="admin.settings.update"
 *   redirectTo="/dashboard"
 * >
 *   <AdminSettings />
 * </PermissionGuard>
 * 
 * // In route definition
 * <Route
 *   path="/projects/:id/settings"
 *   element={
 *     <PermissionGuard requiredPermission="projects.edit" projectId={projectId}>
 *       <ProjectSettings />
 *     </PermissionGuard>
 *   }
 * />
 * ```
 * 
 * @param props - PermissionGuardProps
 * @returns React element or null
 */
/**
 * Get human-readable permission name from permission string
 */
function getPermissionDisplayName(permission: string): string {
  // Try to find in PERMISSIONS constant first
  const permissionEntry = Object.entries(PERMISSIONS).find(
    ([_, value]) => value === permission
  );
  
  if (permissionEntry) {
    // Convert "PROJECTS_CREATE" to "Create Projects"
    const key = permissionEntry[0];
    const parts = key.split('_');
    const action = parts[parts.length - 1].toLowerCase();
    const resource = parts.slice(0, -1).join(' ').toLowerCase();
    
    const actionMap: Record<string, string> = {
      create: 'Create',
      edit: 'Edit',
      delete: 'Delete',
      view: 'View',
      manage: 'Manage',
      invite: 'Invite',
      remove: 'Remove',
      changeRole: 'Change Role',
      assign: 'Assign',
      comment: 'Comment',
      update: 'Update',
    };
    
    const actionDisplay = actionMap[action] || action;
    const resourceDisplay = resource
      .split(' ')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
    
    return `${actionDisplay} ${resourceDisplay}`;
  }
  
  // Fallback: format permission string
  const parts = permission.split('.');
  if (parts.length === 2) {
    const [resource, action] = parts;
    const actionMap: Record<string, string> = {
      create: 'Create',
      edit: 'Edit',
      delete: 'Delete',
      view: 'View',
      manage: 'Manage',
      invite: 'Invite',
      remove: 'Remove',
      changerole: 'Change Role',
      assign: 'Assign',
      comment: 'Comment',
      update: 'Update',
    };
    
    const actionDisplay = actionMap[action.toLowerCase()] || action;
    const resourceDisplay = resource
      .split('.')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
    
    return `${actionDisplay} ${resourceDisplay}`;
  }
  
  return permission;
}

export function PermissionGuard({
  requiredPermission,
  projectId,
  children,
  fallback,
  redirectTo = '/dashboard',
  showNotification = true,
  loadingComponent,
  tooltipText,
  showTooltip = false,
}: PermissionGuardProps): React.ReactElement | null {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const hasNotifiedRef = useRef(false);

  // Use project-specific permissions if projectId is provided, otherwise use base permissions
  const basePermissions = usePermissions();
  const projectPermissions = usePermissionsWithProject(projectId);
  
  // Select the appropriate permissions hook based on whether projectId is provided
  const permissions = projectId !== undefined && projectId !== null && projectId > 0 
    ? projectPermissions 
    : basePermissions;
  const { can, isLoading: permissionsLoading, error } = permissions;

  // Combined loading state
  const isLoading = authLoading || permissionsLoading;

  // Check if user has the required permission
  const hasPermission = can(requiredPermission);

  // Show notification when permission is denied (only once)
  useEffect(() => {
    if (
      !isLoading &&
      isAuthenticated &&
      !hasPermission &&
      showNotification &&
      !hasNotifiedRef.current &&
      !showTooltip // Don't show notification if using tooltip mode
    ) {
      const permissionName = getPermissionDisplayName(requiredPermission);
      showError(
        'Access Denied',
        `You need ${permissionName} permission to perform this action.`
      );
      hasNotifiedRef.current = true;
    }
  }, [isLoading, isAuthenticated, hasPermission, showNotification, requiredPermission, showTooltip]);

  // Handle loading state
  if (isLoading) {
    if (loadingComponent) {
      return <>{loadingComponent}</>;
    }
    return (
      <div className="flex items-center justify-center p-4">
        <Skeleton className="h-8 w-full" />
      </div>
    );
  }

  // Handle error state
  if (error) {
    // Show user-friendly error message if notification is enabled
    if (showNotification && !hasNotifiedRef.current) {
      showError(
        'Permission Check Failed',
        'Unable to verify permissions. Please try again or contact support if the issue persists.'
      );
      hasNotifiedRef.current = true;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    return <Navigate to={redirectTo} replace />;
  }

  // User is not authenticated - redirect to login
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Validate projectId requirement
  if (projectId !== undefined && (projectId === null || projectId <= 0)) {
    // Show user-friendly error message if notification is enabled
    if (showNotification && !hasNotifiedRef.current) {
      showError(
        'Invalid Project',
        'A valid project is required to access this resource. Please select a project and try again.'
      );
      hasNotifiedRef.current = true;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    return <Navigate to={redirectTo} replace />;
  }

  // User doesn't have permission
  if (!hasPermission) {
    // If showTooltip is true, wrap children in tooltip instead of hiding
    if (showTooltip) {
      const defaultTooltipText = tooltipText || `You need ${getPermissionDisplayName(requiredPermission)} permission to perform this action.`;
      return (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <span className="inline-block cursor-not-allowed opacity-50">
                {children}
              </span>
            </TooltipTrigger>
            <TooltipContent>
              <p className="max-w-xs">{defaultTooltipText}</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      );
    }
    
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    if (fallback !== undefined) {
      return <>{fallback}</>;
    }
    // Default: return null (hide component) instead of redirecting
    return null;
  }

  // User has permission - render children
  return <>{children}</>;
}

// Default export for convenience
export default PermissionGuard;

/**
 * PermissionGuard variant that checks multiple permissions (user must have ALL)
 * 
 * @example
 * ```tsx
 * <PermissionGuardAll requiredPermissions={["projects.edit", "projects.delete"]}>
 *   <DeleteProjectButton />
 * </PermissionGuardAll>
 * ```
 */
export interface PermissionGuardAllProps {
  /**
   * Array of required permissions - user must have ALL of them
   */
  requiredPermissions: string[];
  children: React.ReactNode;
  fallback?: React.ReactNode;
  redirectTo?: string;
  showNotification?: boolean;
  loadingComponent?: React.ReactNode;
}

export function PermissionGuardAll({
  requiredPermissions,
  children,
  fallback,
  redirectTo,
  showNotification = true,
  loadingComponent,
}: PermissionGuardAllProps): React.ReactElement | null {
  const { canAll, isLoading, error } = usePermissions();
  const { isAuthenticated } = useAuth();
  const hasNotifiedRef = useRef(false);

  const hasAllPermissions = canAll(requiredPermissions);

  useEffect(() => {
    if (
      !isLoading &&
      isAuthenticated &&
      !hasAllPermissions &&
      showNotification &&
      !hasNotifiedRef.current
    ) {
      showError('Access Denied', `You don't have all required permissions: ${requiredPermissions.join(', ')}`);
      hasNotifiedRef.current = true;
    }
  }, [isLoading, isAuthenticated, hasAllPermissions, showNotification, requiredPermissions]);

  if (isLoading) {
    if (loadingComponent) {
      return <>{loadingComponent}</>;
    }
    return (
      <div className="flex items-center justify-center p-4">
        <Skeleton className="h-8 w-full" />
      </div>
    );
  }

  if (error) {
    // Show user-friendly error message if notification is enabled
    if (showNotification && !hasNotifiedRef.current) {
      showError(
        'Permission Check Failed',
        'Unable to verify permissions. Please try again or contact support if the issue persists.'
      );
      hasNotifiedRef.current = true;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    return null;
  }

  if (!isAuthenticated) {
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    return null;
  }

  if (!hasAllPermissions) {
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    return null;
  }

  return <>{children}</>;
}

/**
 * PermissionGuard variant that checks multiple permissions (user must have ANY)
 * 
 * @example
 * ```tsx
 * <PermissionGuardAny requiredPermissions={["projects.edit", "projects.delete"]}>
 *   <ProjectActions />
 * </PermissionGuardAny>
 * ```
 */
export interface PermissionGuardAnyProps {
  /**
   * Array of required permissions - user must have AT LEAST ONE of them
   */
  requiredPermissions: string[];
  children: React.ReactNode;
  fallback?: React.ReactNode;
  redirectTo?: string;
  showNotification?: boolean;
  loadingComponent?: React.ReactNode;
}

export function PermissionGuardAny({
  requiredPermissions,
  children,
  fallback,
  redirectTo,
  showNotification = true,
  loadingComponent,
}: PermissionGuardAnyProps): React.ReactElement | null {
  const { canAny, isLoading, error } = usePermissions();
  const { isAuthenticated } = useAuth();
  const hasNotifiedRef = useRef(false);

  const hasAnyPermission = canAny(requiredPermissions);

  useEffect(() => {
    if (
      !isLoading &&
      isAuthenticated &&
      !hasAnyPermission &&
      showNotification &&
      !hasNotifiedRef.current
    ) {
      showError('Access Denied', `You don't have any of the required permissions: ${requiredPermissions.join(', ')}`);
      hasNotifiedRef.current = true;
    }
  }, [isLoading, isAuthenticated, hasAnyPermission, showNotification, requiredPermissions]);

  if (isLoading) {
    if (loadingComponent) {
      return <>{loadingComponent}</>;
    }
    return (
      <div className="flex items-center justify-center p-4">
        <Skeleton className="h-8 w-full" />
      </div>
    );
  }

  if (error) {
    // Show user-friendly error message if notification is enabled
    if (showNotification && !hasNotifiedRef.current) {
      showError(
        'Permission Check Failed',
        'Unable to verify permissions. Please try again or contact support if the issue persists.'
      );
      hasNotifiedRef.current = true;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    return null;
  }

  if (!isAuthenticated) {
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    return null;
  }

  if (!hasAnyPermission) {
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    return null;
  }

  return <>{children}</>;
}

