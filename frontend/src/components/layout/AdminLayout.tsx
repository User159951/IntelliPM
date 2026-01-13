import { useEffect, useState } from 'react';
import { Outlet, Navigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { SidebarProvider, SidebarInset } from '@/components/ui/sidebar';
import { Header } from './Header';
import { GlobalSearchModal } from '@/components/search/GlobalSearchModal';
import { Skeleton } from '@/components/ui/skeleton';
import { AdminSidebar } from './AdminSidebar';
import { useTranslation } from 'react-i18next';

export function AdminLayout() {
  const { isAuthenticated, isLoading, isAdmin } = useAuth();
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const { t } = useTranslation('navigation');

  // Disable global search keyboard shortcut unless authenticated
  useEffect(() => {
    if (!isAuthenticated) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        setIsSearchOpen(true);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isAuthenticated]);

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-background">
        <div className="space-y-4">
          <Skeleton className="h-8 w-32" />
          <Skeleton className="h-4 w-48" />
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!isAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <SidebarProvider>
      <div className="flex min-h-screen w-full bg-background">
        <AdminSidebar />
        <SidebarInset>
          <Header onSearchClick={() => setIsSearchOpen(true)} />
          <div className="flex items-center justify-between border-b border-border bg-muted/40 px-6 py-2 text-xs text-muted-foreground">
            <div className="flex items-center gap-2 font-medium text-foreground">
              <span className="rounded-full bg-primary/10 px-2 py-0.5 text-[11px] text-primary">{t('admin.adminArea')}</span>
              <span className="text-muted-foreground">{t('admin.adminDescription')}</span>
            </div>
          </div>
          <div className="flex-1 overflow-auto bg-background p-6">
            <Outlet />
          </div>
        </SidebarInset>
      </div>
      <GlobalSearchModal open={isSearchOpen} onOpenChange={setIsSearchOpen} />
    </SidebarProvider>
  );
}

