import { useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  FolderKanban,
  ListTodo,
  Layers,
  Bug,
  Users,
  BarChart3,
  Lightbulb,
  Bot,
  Zap,
  Shield,
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
  useSidebar,
} from '@/components/ui/sidebar';
import { cn } from '@/lib/utils';
import { useAuth } from '@/contexts/AuthContext';
import { useTranslation } from 'react-i18next';

// Navigation items will be translated in the component
const mainNavItems = [
  { titleKey: 'menuItems.dashboard', url: '/dashboard', icon: LayoutDashboard },
  { titleKey: 'menuItems.projects', url: '/projects', icon: FolderKanban },
];

const projectNavItems = [
  { titleKey: 'menuItems.tasks', url: '/tasks', icon: ListTodo },
  { titleKey: 'menuItems.sprints', url: '/sprints', icon: Zap },
  { titleKey: 'menuItems.backlog', url: '/backlog', icon: Layers },
  { titleKey: 'menuItems.defects', url: '/defects', icon: Bug },
];

const teamNavItems = [
  { titleKey: 'menuItems.teams', url: '/teams', icon: Users },
  { titleKey: 'menuItems.metrics', url: '/metrics', icon: BarChart3 },
  { titleKey: 'menuItems.insights', url: '/insights', icon: Lightbulb },
  { titleKey: 'menuItems.aiAgents', url: '/agents', icon: Bot },
];

const settingsNavItems = [
  { titleKey: 'menuItems.aiQuota', url: '/settings/ai-quota', icon: Zap },
];

export function AppSidebar() {
  const { state } = useSidebar();
  const collapsed = state === 'collapsed';
  const location = useLocation();
  const { isAdmin } = useAuth();
  const { t } = useTranslation('navigation');

  const isActive = (path: string) => location.pathname.startsWith(path);

  return (
    <Sidebar collapsible="icon" className="border-r border-border">
      <SidebarHeader className="p-4">
        <div className={cn('flex items-center gap-2', collapsed && 'justify-center')}>
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
            <Zap className="h-5 w-5 text-primary-foreground" />
          </div>
          {!collapsed && (
            <span className="text-lg font-semibold text-foreground">IntelliPM</span>
          )}
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>
            {t('sections.main')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {mainNavItems.map((item) => {
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

        <SidebarGroup>
          <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>
            {t('sections.project')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {projectNavItems.map((item) => {
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

        <SidebarGroup>
          <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>
            {t('sections.teamInsights')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {teamNavItems.map((item) => {
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

        <SidebarGroup>
          <SidebarGroupLabel className={cn(collapsed && 'sr-only')}>
            {t('sections.settings')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {settingsNavItems.map((item) => {
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
      </SidebarContent>

      <SidebarFooter className="p-4">
        {isAdmin && (
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton asChild tooltip={t('adminMenuItems.adminDashboard')}>
                <NavLink to="/admin/dashboard">
                  <Shield className="h-4 w-4" />
                  <span>{t('adminMenuItems.adminDashboard')}</span>
                </NavLink>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        )}
        {!collapsed && (
          <div className="space-y-1 mt-2">
            <div className="text-xs text-muted-foreground">
              © 2025 IntelliPM
            </div>
            <div className="text-[0.75rem] text-muted-foreground/70">
              v{import.meta.env.VITE_APP_VERSION || '1.0.0'} (Build {import.meta.env.VITE_BUILD_DATE || new Date().toISOString().split('T')[0].replace(/-/g, '.')})
            </div>
          </div>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
