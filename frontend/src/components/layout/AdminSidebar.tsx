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

const adminNavItems = [
  { title: 'Dashboard', url: '/admin/dashboard', icon: LayoutDashboard },
  { title: 'Users', url: '/admin/users', icon: Users },
  { title: 'Permissions', url: '/admin/permissions', icon: Shield },
  { title: 'Settings', url: '/admin/settings', icon: Settings },
  { title: 'Audit Logs', url: '/admin/audit-logs', icon: FileText },
  { title: 'System Health', url: '/admin/system-health', icon: Activity },
  { title: 'AI Governance', url: '/admin/ai-governance', icon: Brain },
  { title: 'AI Quota', url: '/admin/ai-quota', icon: Zap },
];

const superAdminNavItems = [
  { title: 'Organizations', url: '/admin/organizations', icon: Building2 },
];

const adminOwnOrgNavItems = [
  { title: 'My Organization', url: '/admin/organization', icon: Building2 },
  { title: 'Members', url: '/admin/organization/members', icon: Users },
  { title: 'Member AI Quotas', url: '/admin/ai-quotas', icon: Zap },
  { title: 'Member Permissions', url: '/admin/permissions/members', icon: Shield },
];

export function AdminSidebar() {
  const { user, isSuperAdmin } = useAuth();
  const { state } = useSidebar();
  const collapsed = state === 'collapsed';
  const location = useLocation();

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
              <span className="text-xs text-muted-foreground">Admin</span>
            </div>
          )}
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>Admin</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {adminNavItems.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton
                    asChild
                    isActive={isActive(item.url)}
                    tooltip={item.title}
                  >
                    <NavLink to={item.url}>
                      <item.icon className="h-4 w-4" />
                      <span>{item.title}</span>
                    </NavLink>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        {isSuperAdmin && (
          <SidebarGroup>
            <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>SuperAdmin</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {superAdminNavItems.map((item) => (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive(item.url)}
                      tooltip={item.title}
                    >
                      <NavLink to={item.url}>
                        <item.icon className="h-4 w-4" />
                        <span>{item.title}</span>
                      </NavLink>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        )}

        {!isSuperAdmin && (
          <SidebarGroup>
            <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>Organization</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {adminOwnOrgNavItems.map((item) => (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive(item.url)}
                      tooltip={item.title}
                    >
                      <NavLink to={item.url}>
                        <item.icon className="h-4 w-4" />
                        <span>{item.title}</span>
                      </NavLink>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        )}
      </SidebarContent>

      <SidebarFooter className="p-4">
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton asChild tooltip="Back to App">
              <NavLink to="/dashboard">
                <ArrowLeft className="h-4 w-4" />
                <span>Back to App</span>
              </NavLink>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>

      <SidebarRail />
    </Sidebar>
  );
}

