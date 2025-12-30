import React, { useRef, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { usePermissions, usePermissionsWithProject } from '@/hooks/usePermissions';
import { useAuth } from '@/contexts/AuthContext';
import { showError } from "@/lib/sweetalert";
import { Skeleton } from '@/components/ui/skeleton';

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
export function PermissionGuard({
  requiredPermission,
  projectId,
  children,
  fallback,
  redirectTo = '/dashboard',
  showNotification = true,
  loadingComponent,
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
      !hasNotifiedRef.current
    ) {
      showError(
        'Access Denied',
        `You don't have permission to access this resource. Required: ${requiredPermission}`
      );
      hasNotifiedRef.current = true;
    }
  }, [isLoading, isAuthenticated, hasPermission, showNotification, requiredPermission]);

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
    console.error('Permission check error:', error);
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
    console.warn('PermissionGuard: projectId is required but invalid:', projectId);
    if (fallback) {
      return <>{fallback}</>;
    }
    return <Navigate to={redirectTo} replace />;
  }

  // User doesn't have permission
  if (!hasPermission) {
    if (redirectTo) {
      return <Navigate to={redirectTo} replace />;
    }
    if (fallback) {
      return <>{fallback}</>;
    }
    // Default: redirect to dashboard with error toast
    return <Navigate to="/dashboard" replace />;
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
    console.error('Permission check error:', error);
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
    console.error('Permission check error:', error);
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

