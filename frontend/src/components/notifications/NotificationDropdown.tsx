import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { notificationsApi } from '@/api/notifications';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Bell, CheckCircle2, Circle, CheckCheck } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
const notificationIcons: Record<string, React.ReactNode> = {
  task_assigned: <Circle className="h-3 w-3 text-blue-500" />,
  task_completed: <CheckCircle2 className="h-3 w-3 text-green-500" />,
  sprint_started: <Circle className="h-3 w-3 text-orange-500" />,
  comment_added: <Circle className="h-3 w-3 text-purple-500" />,
  project_invite: <Circle className="h-3 w-3 text-indigo-500" />,
};

export function NotificationDropdown() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: ['notifications', 'all'],
    queryFn: () => notificationsApi.getAll({ limit: 10, unreadOnly: false }),
    refetchInterval: 30000, // Poll every 30s
  });

  const markReadMutation = useMutation({
    mutationFn: (id: number) => notificationsApi.markRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });

  const markAllReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAllRead(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      showSuccess("All notifications marked as read");
    },
  });

  const handleNotificationClick = (notification: NonNullable<typeof data>['notifications'][0]) => {
    // Mark as read
    if (!notification.isRead) {
      markReadMutation.mutate(notification.id);
    }

    // Navigate based on entity type
    if (notification.entityType && notification.entityId) {
      switch (notification.entityType) {
        case 'task':
          navigate(`/tasks?taskId=${notification.entityId}`);
          break;
        case 'sprint':
          navigate(`/sprints?sprintId=${notification.entityId}`);
          break;
        case 'project':
          navigate(`/projects/${notification.entityId}`);
          break;
        default:
          if (notification.projectId) {
            navigate(`/projects/${notification.projectId}`);
          }
      }
    } else if (notification.projectId) {
      navigate(`/projects/${notification.projectId}`);
    }
  };

  const unreadCount = data?.unreadCount ?? 0;
  const notifications = data?.notifications ?? [];

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-4 w-4" />
          {unreadCount > 0 && (
            <span className="absolute -right-0.5 -top-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-primary text-[10px] text-primary-foreground">
              {unreadCount > 9 ? '9+' : unreadCount}
            </span>
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent className="w-80" align="end">
        <DropdownMenuLabel className="flex items-center justify-between">
          <span>Notifications</span>
          {unreadCount > 0 && (
            <Badge variant="secondary" className="text-xs">
              {unreadCount} unread
            </Badge>
          )}
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <div className="max-h-[400px] overflow-y-auto">
          {isLoading ? (
            <div className="p-4 text-center text-sm text-muted-foreground">Loading...</div>
          ) : notifications.length === 0 ? (
            <div className="p-4 text-center text-sm text-muted-foreground">
              No notifications
            </div>
          ) : (
            notifications.map((notification) => (
              <DropdownMenuItem
                key={notification.id}
                onClick={() => handleNotificationClick(notification)}
                className="flex flex-col items-start gap-1 whitespace-normal p-3 cursor-pointer"
              >
                <div className="flex items-start gap-2 w-full">
                  <div className="mt-0.5">
                    {notificationIcons[notification.type] || (
                      <Circle className="h-3 w-3 text-muted-foreground" />
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      {!notification.isRead && (
                        <div className="h-2 w-2 rounded-full bg-primary flex-shrink-0" />
                      )}
                      <p
                        className={`text-sm ${
                          notification.isRead ? 'text-muted-foreground' : 'font-medium'
                        }`}
                      >
                        {notification.message}
                      </p>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">
                      {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
                    </p>
                  </div>
                </div>
              </DropdownMenuItem>
            ))
          )}
        </div>
        {notifications.length > 0 && (
          <>
            <DropdownMenuSeparator />
            <div className="p-2 space-y-1">
              <DropdownMenuItem
                onClick={() => markAllReadMutation.mutate()}
                disabled={markAllReadMutation.isPending || unreadCount === 0}
                className="cursor-pointer"
              >
                <CheckCheck className="mr-2 h-4 w-4" />
                Mark all as read
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => navigate('/notifications')}
                className="cursor-pointer"
              >
                View all notifications
              </DropdownMenuItem>
            </div>
          </>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
