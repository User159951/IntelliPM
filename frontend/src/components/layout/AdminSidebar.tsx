import { useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Shield,
  Settings,
  ArrowLeft,
  FileText,
  Activity,
  Brain,
  Zap,
  Building2,
} from 'lucide-react';
import { NavLink } from '@/components/NavLink';
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarHeader,
  SidebarFooter,
  SidebarRail,
  useSidebar,
} from '@/components/ui/sidebar';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { useAuth } from '@/contexts/AuthContext';
import { cn } from '@/lib/utils';
import { useTranslation } from 'react-i18next';

// Navigation items will be translated in the component
const adminNavItems = [
  { titleKey: 'adminMenuItems.adminDashboard', url: '/admin/dashboard', icon: LayoutDashboard },
  { titleKey: 'adminMenuItems.users', url: '/admin/users', icon: Users },
  { titleKey: 'adminMenuItems.permissions', url: '/admin/permissions', icon: Shield },
  { titleKey: 'adminMenuItems.settings', url: '/admin/settings', icon: Settings },
  { titleKey: 'adminMenuItems.auditLogs', url: '/admin/audit-logs', icon: FileText },
  { titleKey: 'adminMenuItems.systemHealth', url: '/admin/system-health', icon: Activity },
  { titleKey: 'adminMenuItems.aiGovernance', url: '/admin/ai-governance', icon: Brain },
  { titleKey: 'adminMenuItems.aiQuota', url: '/admin/ai-quota', icon: Zap },
];

const superAdminNavItems = [
  { titleKey: 'adminMenuItems.organizations', url: '/admin/organizations', icon: Building2 },
];

const adminOwnOrgNavItems = [
  { titleKey: 'adminMenuItems.myOrganization', url: '/admin/organization', icon: Building2 },
  { titleKey: 'adminMenuItems.members', url: '/admin/organization/members', icon: Users },
  { titleKey: 'adminMenuItems.memberAIQuotas', url: '/admin/ai-quotas', icon: Zap },
  { titleKey: 'adminMenuItems.memberPermissions', url: '/admin/permissions/members', icon: Shield },
];

export function AdminSidebar() {
  const { user, isSuperAdmin } = useAuth();
  const { state } = useSidebar();
  const collapsed = state === 'collapsed';
  const location = useLocation();
  const { t } = useTranslation('navigation');

  const initials = user
    ? `${user.firstName?.[0] || user.username[0]}${user.lastName?.[0] || ''}`
    : 'A';

  const isActive = (path: string) => location.pathname.startsWith(path);

  return (
    <Sidebar collapsible="icon" className="border-r border-border">
      <SidebarHeader className="p-4">
        <div className={cn('flex items-center gap-3', collapsed && 'justify-center')}>
          <Avatar className="h-9 w-9">
            <AvatarFallback className="bg-primary text-primary-foreground text-sm">
              {initials.toUpperCase()}
            </AvatarFallback>
          </Avatar>
          {!collapsed && (
            <div className="flex flex-col">
              <span className="text-sm font-semibold text-foreground">
                {user?.firstName || user?.username || 'Admin'}
              </span>
              <span className="text-xs text-muted-foreground">{t('admin.roleBadge')}</span>
            </div>
          )}
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>{t('sections.admin')}</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {adminNavItems.map((item) => {
                const title = t(item.titleKey);
                return (
                  <SidebarMenuItem key={item.titleKey}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive(item.url)}
                      tooltip={title}
                    >
                      <NavLink to={item.url}>
                        <item.icon className="h-4 w-4" />
                        <span>{title}</span>
                      </NavLink>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        {isSuperAdmin && (
          <SidebarGroup>
            <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>{t('sections.superAdmin')}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {superAdminNavItems.map((item) => {
                  const title = t(item.titleKey);
                  return (
                    <SidebarMenuItem key={item.titleKey}>
                      <SidebarMenuButton
                        asChild
                        isActive={isActive(item.url)}
                        tooltip={title}
                      >
                        <NavLink to={item.url}>
                          <item.icon className="h-4 w-4" />
                          <span>{title}</span>
                        </NavLink>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  );
                })}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        )}

        {!isSuperAdmin && (
          <SidebarGroup>
            <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>{t('sections.organization')}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {adminOwnOrgNavItems.map((item) => {
                  const title = t(item.titleKey);
                  return (
                    <SidebarMenuItem key={item.titleKey}>
                      <SidebarMenuButton
                        asChild
                        isActive={isActive(item.url)}
                        tooltip={title}
                      >
                        <NavLink to={item.url}>
                          <item.icon className="h-4 w-4" />
                          <span>{title}</span>
                        </NavLink>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  );
                })}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        )}
      </SidebarContent>

      <SidebarFooter className="p-4">
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton asChild tooltip={t('admin.backToApp')}>
              <NavLink to="/dashboard">
                <ArrowLeft className="h-4 w-4" />
                <span>{t('admin.backToApp')}</span>
              </NavLink>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>

      <SidebarRail />
    </Sidebar>
  );
}

