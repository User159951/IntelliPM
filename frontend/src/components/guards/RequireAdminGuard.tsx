import React, { useRef, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { showError } from "@/lib/sweetalert";

interface RequireAdminGuardProps {
  children: React.ReactNode;
}

/**
 * Route guard that requires Admin or SuperAdmin role.
 * Uses role-based authorization consistent with backend [RequireAdmin] attribute.
 * Admin can access their own organization; SuperAdmin can access all organizations.
 */
export function RequireAdminGuard({ children }: RequireAdminGuardProps) {
  const { isAdmin, isLoading } = useAuth();
  const hasNotifiedRef = useRef(false);

  // Move showError to useEffect to avoid React warning about updating state during render
  useEffect(() => {
    if (!isLoading && !isAdmin && !hasNotifiedRef.current) {
      showError("Access Denied", "You need Admin or SuperAdmin role to access this page.");
      hasNotifiedRef.current = true;
    }
  }, [isAdmin, isLoading]);

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}

