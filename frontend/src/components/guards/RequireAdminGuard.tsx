import React, { useRef, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { usePermissions } from '@/hooks/usePermissions';
import { showError } from "@/lib/sweetalert";

interface RequireAdminGuardProps {
  children: React.ReactNode;
}

/**
 * Route guard that requires admin panel view permission
 * Uses API-driven permission checks instead of hardcoded role checks
 */
export function RequireAdminGuard({ children }: RequireAdminGuardProps) {
  const { canViewAdminPanel, isLoading } = usePermissions();
  const hasNotifiedRef = useRef(false);

  // Move showError to useEffect to avoid React warning about updating state during render
  useEffect(() => {
    if (!isLoading && !canViewAdminPanel && !hasNotifiedRef.current) {
      showError("Access Denied", "You need admin panel access permission to view this page.");
      hasNotifiedRef.current = true;
    }
  }, [canViewAdminPanel, isLoading]);

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!canViewAdminPanel) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}

