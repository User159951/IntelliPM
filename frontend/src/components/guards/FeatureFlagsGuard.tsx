import { useFeatureFlags } from '@/contexts/FeatureFlagsContext';
import { FeatureFlagsMaintenance } from '@/components/FeatureFlagsMaintenance';
import { Skeleton } from '@/components/ui/skeleton';
import { useAuth } from '@/contexts/AuthContext';

interface FeatureFlagsGuardProps {
  children: React.ReactNode;
}

/**
 * Guard component that ensures feature flags are loaded before rendering children.
 * 
 * - Shows loading state while flags are being fetched
 * - Shows maintenance message if API fails (no assumptions/defaults)
 * - Only blocks protected routes (allows public routes to render)
 * 
 * @example
 * ```tsx
 * <FeatureFlagsGuard>
 *   <ProtectedRoutes />
 * </FeatureFlagsGuard>
 * ```
 */
export function FeatureFlagsGuard({ children }: FeatureFlagsGuardProps) {
  const { isLoading, error, refresh } = useFeatureFlags();
  const { isAuthenticated } = useAuth();

  // For unauthenticated users, don't block (public routes don't need flags)
  if (!isAuthenticated) {
    return <>{children}</>;
  }

  // Show loading state while fetching flags
  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background">
        <div className="w-full max-w-md space-y-4 p-4">
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-32 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      </div>
    );
  }

  // Show maintenance message if API failed (no assumptions about flags)
  if (error) {
    return <FeatureFlagsMaintenance error={error} onRetry={refresh} />;
  }

  // Flags loaded successfully, render children
  return <>{children}</>;
}

