import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { activityApi, type Activity } from '@/api/activity';
import { useAuth } from '@/contexts/AuthContext';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatRelativeTime } from '@/utils/dateFormat';
import { Clock, ArrowRight } from 'lucide-react';

interface RecentActivityProps {
  limit?: number;
  projectId?: number;
}

const getActivityText = (activity: Activity): { action: string; entity: string; project: string } => {
  const entityName = activity.entityName || `#${activity.entityId}`;
  const projectName = activity.projectName || 'project';

  const actionMap: Record<string, string> = {
    task_created: 'created task',
    task_updated: 'updated task',
    task_completed: 'completed task',
    task_status_changed: 'changed status of task',
    task_assigned: 'assigned task',
    task_unassigned: 'unassigned task',
    sprint_started: 'started sprint',
    sprint_completed: 'completed sprint',
    project_created: 'created project',
    project_archived: 'archived project',
    comment_added: 'commented on',
  };

  const action = actionMap[activity.type] || activity.type.replace('_', ' ');

  return { action, entity: entityName, project: projectName };
};

const getActivityIcon = (type: string): string => {
  if (type.startsWith('task_')) return '📝';
  if (type.startsWith('sprint_')) return '⚡';
  if (type.startsWith('project_')) return '📁';
  if (type.includes('comment')) return '💬';
  return '📌';
};

const getNavigationPath = (activity: Activity): string | null => {
  switch (activity.entityType) {
    case 'task':
      return `/projects/${activity.projectId}?task=${activity.entityId}`;
    case 'sprint':
      return `/sprints?project=${activity.projectId}&sprint=${activity.entityId}`;
    case 'project':
      return `/projects/${activity.projectId}`;
    default:
      return null;
  }
};

export function RecentActivity({ limit = 10, projectId }: RecentActivityProps) {
  const navigate = useNavigate();
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const { language } = useLanguage();

  const { data, isLoading } = useQuery({
    queryKey: ['recent-activity', limit, projectId],
    queryFn: () => activityApi.getRecent(limit, projectId),
    enabled: isAuthenticated && !isAuthLoading, // Only fetch when authenticated
    refetchInterval: isAuthenticated ? 30000 : false, // Refresh every 30 seconds only when authenticated
  });

  const activities = data?.activities || [];

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Recent Activity</CardTitle>
            <CardDescription>Latest updates across your projects</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-4">
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex gap-3">
                <Skeleton className="h-8 w-8 rounded-full" />
                <div className="flex-1 space-y-2">
                  <Skeleton className="h-4 w-full" />
                  <Skeleton className="h-3 w-24" />
                </div>
              </div>
            ))}
          </div>
        ) : activities.length === 0 ? (
          <div className="text-center py-8">
            <Clock className="h-12 w-12 mx-auto text-muted-foreground mb-2" />
            <p className="text-sm text-muted-foreground">No recent activity</p>
          </div>
        ) : (
          <div className="space-y-4">
            {activities.map((activity) => {
              const path = getNavigationPath(activity);
              return (
                <div
                  key={activity.id}
                  className="flex gap-3 group cursor-pointer hover:bg-muted/50 rounded-lg p-2 -m-2 transition-colors"
                  onClick={() => path && navigate(path)}
                >
                  <Avatar className="h-8 w-8 flex-shrink-0">
                    <AvatarFallback className="text-xs">
                      {activity.userName
                        .split(' ')
                        .map((n) => n[0])
                        .join('')
                        .toUpperCase()
                        .slice(0, 2)}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start gap-2">
                      <span className="text-lg flex-shrink-0">{getActivityIcon(activity.type)}</span>
                      <p className="text-sm leading-tight">
                        <span className="font-medium">{activity.userName}</span>{' '}
                        {(() => {
                          const { action, entity, project } = getActivityText(activity);
                          return (
                            <>
                              {action}{' '}
                              <span className="font-medium text-primary hover:underline cursor-pointer">
                                {entity}
                              </span>{' '}
                              in <span className="text-muted-foreground">{project}</span>
                            </>
                          );
                        })()}
                      </p>
                    </div>
                    <div className="flex items-center gap-2 mt-1">
                      <Clock className="h-3 w-3 text-muted-foreground" />
                      <span className="text-xs text-muted-foreground">
                        {formatRelativeTime(activity.timestamp, language)}
                      </span>
                    </div>
                  </div>
                  {path && (
                    <ArrowRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0 mt-1" />
                  )}
                </div>
              );
            })}
            <div className="pt-2 border-t">
              <Button
                variant="ghost"
                size="sm"
                className="w-full"
                onClick={() => navigate('/projects')}
              >
                View all activity
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
