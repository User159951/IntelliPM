import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Bell,
  Inbox,
  CheckSquare,
  AtSign,
  MessageSquare,
  Users,
  Calendar,
  AlertCircle,
} from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { ScrollArea } from '@/components/ui/scroll-area';
import { notificationsApi } from '@/api/notifications';
import { showToast } from '@/lib/sweetalert';
import { cn } from '@/lib/utils';
import { useAuth } from '@/contexts/AuthContext';
import type { Notification, GetNotificationsResponse } from '@/api/notifications';

interface NotificationItemProps {
  notification: Notification;
  onClick: () => void;
}

function NotificationItem({ notification, onClick }: NotificationItemProps) {
  const getIcon = () => {
    switch (notification.type) {
      case 'TaskAssigned':
        return <CheckSquare className="h-4 w-4 text-blue-500" />;
      case 'Mentioned':
        return <AtSign className="h-4 w-4 text-purple-500" />;
      case 'TaskCommented':
        return <MessageSquare className="h-4 w-4 text-green-500" />;
      case 'ProjectInvite':
        return <Users className="h-4 w-4 text-orange-500" />;
      case 'SprintStarted':
      case 'SprintCompleted':
        return <Calendar className="h-4 w-4 text-indigo-500" />;
      case 'DefectReported':
        return <AlertCircle className="h-4 w-4 text-red-500" />;
      default:
        return <Bell className="h-4 w-4 text-gray-500" />;
    }
  };

  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full flex items-start gap-3 px-4 py-3 hover:bg-accent transition-colors text-left',
        !notification.isRead && 'bg-accent/50'
      )}
    >
      <div className="flex-shrink-0 mt-0.5">{getIcon()}</div>

      <div className="flex-1 min-w-0 space-y-1">
        <p className="text-sm font-medium leading-tight">{notification.message}</p>
        <p className="text-xs text-muted-foreground">
          {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
        </p>
      </div>

      {!notification.isRead && (
        <div className="flex-shrink-0">
          <div className="h-2 w-2 rounded-full bg-primary" />
        </div>
      )}
    </button>
  );
}

export default function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();

  // Fetch notifications - only when authenticated
  const { data: notificationsData, refetch } = useQuery<GetNotificationsResponse>({
    queryKey: ['notifications'],
    queryFn: () => notificationsApi.getAll({ limit: 10, unreadOnly: false }),
    enabled: isAuthenticated && !isAuthLoading, // Only fetch when authenticated
    staleTime: 1000 * 30, // 30 seconds
    refetchInterval: isAuthenticated ? 1000 * 60 : false, // Poll every minute only when authenticated
    refetchOnWindowFocus: isAuthenticated && !isAuthLoading, // Only refetch on window focus if authenticated
    retry: (failureCount, error) => {
      // Don't retry on 401 (Unauthorized) errors - token refresh will be handled by API client
      // If refresh fails, auth context will be updated and queries will be disabled
      if (error instanceof Error && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
        return false;
      }
      return failureCount < 3;
    },
    // Disable query on 401 errors to prevent repeated failed requests
    onError: (error) => {
      if (error instanceof Error && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
        // Query will be automatically disabled when isAuthenticated becomes false
        // This is just to prevent unnecessary retries
      }
    },
  });

  const notifications = (notificationsData as GetNotificationsResponse)?.notifications ?? [];

  // ✅ New query for unread count using dedicated endpoint
  const { data: unreadData } = useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: () => notificationsApi.getUnreadCount(),
    enabled: isAuthenticated && !isAuthLoading,
    refetchInterval: isAuthenticated ? 1000 * 30 : false, // Refresh every 30 seconds
    refetchOnWindowFocus: isAuthenticated && !isAuthLoading, // Only refetch on window focus if authenticated
    staleTime: 1000 * 10, // Cache valid for 10 seconds
    retry: (failureCount, error) => {
      // Don't retry on 401 (Unauthorized) errors - token refresh will be handled by API client
      // If refresh fails, auth context will be updated and queries will be disabled
      if (error instanceof Error && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
        return false;
      }
      return failureCount < 3;
    },
    // Disable query on 401 errors to prevent repeated failed requests
    onError: (error) => {
      if (error instanceof Error && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
        // Query will be automatically disabled when isAuthenticated becomes false
        // This is just to prevent unnecessary retries
      }
    },
  });

  const unreadCount = unreadData?.unreadCount ?? 0;

  // Mark notification as read mutation
  const markAsReadMutation = useMutation({
    mutationFn: (notificationId: number) => notificationsApi.markAsRead(notificationId),
    onSuccess: () => {
      refetch();
      // Invalidate unread count query to refresh the badge
      queryClient.invalidateQueries({ queryKey: ['notifications', 'unread-count'] });
    },
  });

  // Mark all as read mutation
  const markAllAsReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      showToast('All notifications marked as read', 'success');
      refetch();
      // Invalidate unread count query to refresh the badge
      queryClient.invalidateQueries({ queryKey: ['notifications', 'unread-count'] });
    },
  });

  const handleNotificationClick = (notification: Notification) => {
    // Mark as read
    if (!notification.isRead) {
      markAsReadMutation.mutate(notification.id);
    }

    // Navigate to entity
    navigateToEntity(notification.entityType, notification.entityId);
    setIsOpen(false);
  };

  const navigateToEntity = (entityType?: string | null, entityId?: number | null) => {
    if (!entityType || !entityId) return;

    const routes: Record<string, string> = {
      Task: `/tasks?taskId=${entityId}`,
      Project: `/projects/${entityId}`,
      Sprint: `/sprints?sprintId=${entityId}`,
      Defect: `/defects/${entityId}`,
    };

    const route = routes[entityType];
    if (route) {
      navigate(route);
    }
  };

  return (
    <DropdownMenu open={isOpen} onOpenChange={setIsOpen}>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-5 w-5" />
          {unreadCount > 0 && (
            <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-destructive text-destructive-foreground text-xs flex items-center justify-center font-semibold">
              {unreadCount > 9 ? '9+' : unreadCount}
            </span>
          )}
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" className="w-[380px]">
        <div className="flex items-center justify-between px-4 py-2 border-b">
          <h3 className="font-semibold">Notifications</h3>
          {unreadCount > 0 && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => markAllAsReadMutation.mutate()}
              disabled={markAllAsReadMutation.isPending}
            >
              Mark all as read
            </Button>
          )}
        </div>

        <ScrollArea className="h-[400px]">
          {notifications.length > 0 ? (
            <div className="divide-y">
              {notifications.map((notification: Notification) => (
                <NotificationItem
                  key={notification.id}
                  notification={notification}
                  onClick={() => handleNotificationClick(notification)}
                />
              ))}
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Inbox className="h-12 w-12 text-muted-foreground mb-2" />
              <p className="text-sm text-muted-foreground">No notifications yet</p>
            </div>
          )}
        </ScrollArea>

        <div className="border-t px-4 py-2">
          <Button
            variant="ghost"
            size="sm"
            className="w-full"
            onClick={() => {
              navigate('/notifications');
              setIsOpen(false);
            }}
          >
            View all notifications
          </Button>
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

