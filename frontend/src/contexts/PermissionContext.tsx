import React, { createContext, useContext, useCallback, useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { permissionsApi } from '@/api/permissions';
import { useAuth } from './AuthContext';

interface PermissionContextType {
  /**
   * Get user's global permissions
   * Returns permissions array from API or empty array if not loaded
   */
  getGlobalPermissions: () => string[];
  
  /**
   * Get user's project-specific permissions
   * @param projectId - Project ID
   * @returns Permissions array from API or empty array if not loaded
   */
  getProjectPermissions: (projectId: number) => string[];
  
  /**
   * Check if user has a specific global permission
   * @param permission - Permission string (e.g., "projects.create")
   * @returns true if user has the permission
   */
  hasGlobalPermission: (permission: string) => boolean;
  
  /**
   * Check if user has a specific project permission
   * @param projectId - Project ID
   * @param permission - Permission string (e.g., "tasks.create")
   * @returns true if user has the permission
   */
  hasProjectPermission: (projectId: number, permission: string) => boolean;
  
  /**
   * Loading state for global permissions
   */
  isLoadingGlobalPermissions: boolean;
  
  /**
   * Loading state for project permissions
   * @param projectId - Project ID
   */
  isLoadingProjectPermissions: (projectId: number) => boolean;
  
  /**
   * Error state for global permissions
   */
  globalPermissionsError: Error | null;
  
  /**
   * Error state for project permissions
   * @param projectId - Project ID
   */
  projectPermissionsError: (projectId: number) => Error | null;
  
  /**
   * Invalidate and refetch global permissions
   */
  invalidateGlobalPermissions: () => void;
  
  /**
   * Invalidate and refetch project permissions
   * @param projectId - Project ID
   */
  invalidateProjectPermissions: (projectId: number) => void;
}

const PermissionContext = createContext<PermissionContextType | undefined>(undefined);

export const PermissionProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user, isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  // Query for global permissions
  const {
    data: globalPermissionsData,
    isLoading: isLoadingGlobal,
    error: globalError,
  } = useQuery({
    queryKey: ['permissions', 'me'],
    queryFn: () => permissionsApi.getMyPermissions(),
    enabled: isAuthenticated && !!user,
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
  });

  // Helper to get project permissions query
  const getProjectPermissionsQuery = useCallback(
    (projectId: number) => {
      return queryClient.getQueryState<{ permissions: string[] }>(['project-permissions', projectId]);
    },
    [queryClient]
  );

  // Get global permissions
  const getGlobalPermissions = useCallback((): string[] => {
    if (!globalPermissionsData) {
      return [];
    }
    return globalPermissionsData.permissions || [];
  }, [globalPermissionsData]);

  // Get project permissions (lazy fetch)
  const getProjectPermissions = useCallback(
    (projectId: number): string[] => {
      const queryState = getProjectPermissionsQuery(projectId);
      if (!queryState?.data) {
        // Trigger fetch if not already loaded
        queryClient.prefetchQuery({
          queryKey: ['project-permissions', projectId],
          queryFn: () => permissionsApi.getProjectPermissions(projectId),
          staleTime: 5 * 60 * 1000,
          gcTime: 10 * 60 * 1000,
        });
        return [];
      }
      return queryState.data.permissions || [];
    },
    [getProjectPermissionsQuery, queryClient]
  );

  // Check global permission
  const hasGlobalPermission = useCallback(
    (permission: string): boolean => {
      const permissions = getGlobalPermissions();
      return permissions.includes(permission);
    },
    [getGlobalPermissions]
  );

  // Check project permission
  const hasProjectPermission = useCallback(
    (projectId: number, permission: string): boolean => {
      const permissions = getProjectPermissions(projectId);
      return permissions.includes(permission);
    },
    [getProjectPermissions]
  );

  // Loading states
  const isLoadingProjectPermissions = useCallback(
    (projectId: number): boolean => {
      const queryState = getProjectPermissionsQuery(projectId);
      return queryState?.status === 'pending' || queryState?.fetchStatus === 'fetching';
    },
    [getProjectPermissionsQuery]
  );

  // Error states
  const projectPermissionsError = useCallback(
    (projectId: number): Error | null => {
      const queryState = getProjectPermissionsQuery(projectId);
      return queryState?.error as Error | null || null;
    },
    [getProjectPermissionsQuery]
  );

  // Invalidate functions
  const invalidateGlobalPermissions = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['permissions', 'me'] });
  }, [queryClient]);

  const invalidateProjectPermissions = useCallback(
    (projectId: number) => {
      queryClient.invalidateQueries({ queryKey: ['project-permissions', projectId] });
    },
    [queryClient]
  );

  const value = useMemo(
    (): PermissionContextType => ({
      getGlobalPermissions,
      getProjectPermissions,
      hasGlobalPermission,
      hasProjectPermission,
      isLoadingGlobalPermissions: isLoadingGlobal,
      isLoadingProjectPermissions,
      globalPermissionsError: globalError as Error | null,
      projectPermissionsError,
      invalidateGlobalPermissions,
      invalidateProjectPermissions,
    }),
    [
      getGlobalPermissions,
      getProjectPermissions,
      hasGlobalPermission,
      hasProjectPermission,
      isLoadingGlobal,
      isLoadingProjectPermissions,
      globalError,
      projectPermissionsError,
      invalidateGlobalPermissions,
      invalidateProjectPermissions,
    ]
  );

  return <PermissionContext.Provider value={value}>{children}</PermissionContext.Provider>;
};

// eslint-disable-next-line react-refresh/only-export-components
export const usePermissionContext = (): PermissionContextType => {
  const context = useContext(PermissionContext);
  if (context === undefined) {
    throw new Error('usePermissionContext must be used within a PermissionProvider');
  }
  return context;
};

