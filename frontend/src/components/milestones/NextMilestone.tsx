import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { milestonesApi } from '@/api/milestones';
import { Check, Plus, Calendar } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { cn } from '@/lib/utils';

interface NextMilestoneProps {
  projectId: number;
  onCreateMilestone?: () => void;
  onComplete?: (milestoneId: number) => void;
  canCreate?: boolean;
  canComplete?: boolean;
}

/**
 * Widget component displaying the next upcoming milestone for a project.
 * Shows countdown, progress, and quick actions.
 */
export function NextMilestone({
  projectId,
  onCreateMilestone,
  onComplete,
  canCreate = false,
  canComplete = false,
}: NextMilestoneProps) {
  const { data: milestone, isLoading } = useQuery({
    queryKey: ['nextMilestone', projectId],
    queryFn: () => milestonesApi.getNextMilestone(projectId),
    enabled: !!projectId,
  });

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-32" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-20 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (!milestone) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Next Milestone</CardTitle>
          <CardDescription>No upcoming milestones</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">
            Create your first milestone to track important project deadlines.
          </p>
          {canCreate && onCreateMilestone && (
            <Button onClick={onCreateMilestone} size="sm" className="w-full">
              <Plus className="h-4 w-4 mr-2" />
              Create Milestone
            </Button>
          )}
        </CardContent>
      </Card>
    );
  }

  const dueDate = new Date(milestone.dueDate);
  const isPast = milestone.daysUntilDue < 0;
  const daysText = isPast
    ? `${Math.abs(milestone.daysUntilDue)} days overdue`
    : `${milestone.daysUntilDue} days left`;

  return (
    <Card className={cn(milestone.isOverdue && 'border-red-500')}>
      <CardHeader>
        <div className="flex items-start justify-between">
          <div>
            <CardTitle className="text-lg">{milestone.name}</CardTitle>
            <CardDescription className="mt-1">
              {milestone.type} • Due {formatDistanceToNow(dueDate, { addSuffix: true })}
            </CardDescription>
          </div>
          <Badge
            className={cn(
              milestone.status === 'Pending' && 'bg-blue-500 text-white',
              milestone.status === 'InProgress' && 'bg-orange-500 text-white'
            )}
          >
            {milestone.status}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Countdown */}
        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-muted-foreground" />
          <span className={cn(
            'text-sm font-medium',
            isPast && 'text-red-600 dark:text-red-400',
            !isPast && 'text-muted-foreground'
          )}>
            {daysText}
          </span>
        </div>

        {/* Progress */}
        {milestone.status === 'InProgress' && (
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Progress</span>
              <span className="font-medium">{milestone.progress}%</span>
            </div>
            <Progress value={milestone.progress} />
          </div>
        )}

        {/* Quick Action */}
        {canComplete && milestone.status !== 'Completed' && milestone.status !== 'Cancelled' && onComplete && (
          <Button
            onClick={() => onComplete(milestone.id)}
            size="sm"
            className="w-full"
            variant="outline"
          >
            <Check className="h-4 w-4 mr-2" />
            Mark Complete
          </Button>
        )}
      </CardContent>
    </Card>
  );
}

