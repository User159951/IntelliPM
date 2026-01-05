import React, { useRef, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { showError } from "@/lib/sweetalert";

interface RequireSuperAdminGuardProps {
  children: React.ReactNode;
}

export function RequireSuperAdminGuard({ children }: RequireSuperAdminGuardProps) {
  const { isSuperAdmin, isLoading } = useAuth();
  const hasNotifiedRef = useRef(false);

  // Move showError to useEffect to avoid React warning about updating state during render
  useEffect(() => {
    if (!isLoading && !isSuperAdmin && !hasNotifiedRef.current) {
      showError("Access Denied", "Only SuperAdmin can access this page.");
      hasNotifiedRef.current = true;
    }
  }, [isSuperAdmin, isLoading]);

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isSuperAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}

