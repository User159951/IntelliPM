import React, { useRef, useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { showError } from "@/lib/sweetalert";

interface RequireAdminGuardProps {
  children: React.ReactNode;
}

export function RequireAdminGuard({ children }: RequireAdminGuardProps) {
  const { isAdmin, isLoading } = useAuth();
  const hasNotifiedRef = useRef(false);

  // Move showError to useEffect to avoid React warning about updating state during render
  useEffect(() => {
    if (!isLoading && !isAdmin && !hasNotifiedRef.current) {
      showError("Access Denied");
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

