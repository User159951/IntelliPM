import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Rocket, Flag, Calendar, Star, Check, Edit, Trash2, AlertCircle } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { cn } from '@/lib/utils';
import type { MilestoneDto } from '@/types/milestones';

interface MilestoneCardProps {
  milestone: MilestoneDto;
  onEdit?: () => void;
  onComplete?: () => void;
  onDelete?: () => void;
  canEdit?: boolean;
  canComplete?: boolean;
  canDelete?: boolean;
}

/**
 * Get icon component based on milestone type.
 */
function getTypeIcon(type: string) {
  switch (type) {
    case 'Release':
      return Rocket;
    case 'Sprint':
      return Flag;
    case 'Deadline':
      return Calendar;
    case 'Custom':
      return Star;
    default:
      return Star;
  }
}

/**
 * Get status badge color based on milestone status.
 */
function getStatusColor(status: string): string {
  switch (status) {
    case 'Completed':
      return 'bg-green-500 text-white dark:bg-green-600';
    case 'Pending':
      return 'bg-blue-500 text-white dark:bg-blue-600';
    case 'InProgress':
      return 'bg-orange-500 text-white dark:bg-orange-600';
    case 'Missed':
      return 'bg-red-500 text-white dark:bg-red-600';
    case 'Cancelled':
      return 'bg-gray-500 text-white dark:bg-gray-600';
    default:
      return 'bg-gray-500 text-white';
  }
}

/**
 * Card component for displaying a milestone.
 * Shows milestone information with type icon, status badge, progress, and action buttons.
 */
export function MilestoneCard({
  milestone,
  onEdit,
  onComplete,
  onDelete,
  canEdit = false,
  canComplete = false,
  canDelete = false,
}: MilestoneCardProps) {
  const TypeIcon = getTypeIcon(milestone.type);
  const dueDate = new Date(milestone.dueDate);
  const isPast = milestone.daysUntilDue < 0;
  const daysText = isPast
    ? `${Math.abs(milestone.daysUntilDue)} days overdue`
    : `${milestone.daysUntilDue} days left`;

  return (
    <Card className={cn('transition-all hover:shadow-md', milestone.isOverdue && 'border-red-500')}>
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex items-start gap-3 flex-1">
            <div className={cn(
              'p-2 rounded-lg',
              milestone.type === 'Release' && 'bg-blue-100 dark:bg-blue-900',
              milestone.type === 'Sprint' && 'bg-green-100 dark:bg-green-900',
              milestone.type === 'Deadline' && 'bg-purple-100 dark:bg-purple-900',
              milestone.type === 'Custom' && 'bg-yellow-100 dark:bg-yellow-900'
            )}>
              <TypeIcon className={cn(
                'h-5 w-5',
                milestone.type === 'Release' && 'text-blue-600 dark:text-blue-400',
                milestone.type === 'Sprint' && 'text-green-600 dark:text-green-400',
                milestone.type === 'Deadline' && 'text-purple-600 dark:text-purple-400',
                milestone.type === 'Custom' && 'text-yellow-600 dark:text-yellow-400'
              )} />
            </div>
            <div className="flex-1 min-w-0">
              <CardTitle className="text-lg">{milestone.name}</CardTitle>
              {milestone.description && (
                <CardDescription className="mt-1 line-clamp-2">
                  {milestone.description}
                </CardDescription>
              )}
            </div>
          </div>
          <Badge className={getStatusColor(milestone.status)}>
            {milestone.status}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Due Date and Countdown */}
        <div className="flex items-center justify-between text-sm">
          <div className="flex items-center gap-2">
            <Calendar className="h-4 w-4 text-muted-foreground" />
            <span className="text-muted-foreground">
              Due: {formatDistanceToNow(dueDate, { addSuffix: true })}
            </span>
          </div>
          <span className={cn(
            'font-medium',
            isPast && 'text-red-600 dark:text-red-400',
            !isPast && 'text-muted-foreground'
          )}>
            {daysText}
          </span>
        </div>

        {/* Progress Bar */}
        {milestone.status === 'InProgress' && (
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Progress</span>
              <span className="font-medium">{milestone.progress}%</span>
            </div>
            <Progress value={milestone.progress} />
          </div>
        )}

        {/* Overdue Warning */}
        {milestone.isOverdue && (
          <div className="flex items-center gap-2 p-2 bg-red-50 dark:bg-red-950 rounded-md">
            <AlertCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
            <span className="text-sm text-red-600 dark:text-red-400 font-medium">
              This milestone is overdue
            </span>
          </div>
        )}

        {/* Action Buttons */}
        {(canEdit || canComplete || canDelete) && (
          <div className="flex items-center gap-2 pt-2 border-t">
            {canComplete && milestone.status !== 'Completed' && milestone.status !== 'Cancelled' && (
              <Button
                variant="outline"
                size="sm"
                onClick={onComplete}
                className="flex-1"
              >
                <Check className="h-4 w-4 mr-2" />
                Complete
              </Button>
            )}
            {canEdit && (
              <Button
                variant="outline"
                size="sm"
                onClick={onEdit}
              >
                <Edit className="h-4 w-4" />
              </Button>
            )}
            {canDelete && (
              <Button
                variant="outline"
                size="sm"
                onClick={onDelete}
                className="text-destructive hover:text-destructive"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

