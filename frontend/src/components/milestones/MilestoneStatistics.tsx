import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { milestonesApi } from '@/api/milestones';
import { CheckCircle2, XCircle, Clock, AlertCircle } from 'lucide-react';

interface MilestoneStatisticsProps {
  projectId: number;
}

/**
 * Component displaying milestone statistics for a project.
 * Shows counts by status and completion rate.
 */
export function MilestoneStatistics({ projectId }: MilestoneStatisticsProps) {
  const { data: stats, isLoading } = useQuery({
    queryKey: ['milestoneStatistics', projectId],
    queryFn: () => milestonesApi.getMilestoneStatistics(projectId),
    enabled: !!projectId,
  });

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-40" />
        </CardHeader>
        <CardContent className="space-y-4">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-16 w-full" />
          ))}
        </CardContent>
      </Card>
    );
  }

  if (!stats) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Milestone Statistics</CardTitle>
        <CardDescription>Overview of project milestones</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Total and Completion Rate */}
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium">Total Milestones</span>
            <span className="text-sm font-bold">{stats.totalMilestones}</span>
          </div>
          <div className="space-y-1">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Completion Rate</span>
              <span className="font-medium">{stats.completionRate.toFixed(1)}%</span>
            </div>
            <Progress value={stats.completionRate} />
          </div>
        </div>

        {/* Status Counts */}
        <div className="grid grid-cols-2 gap-3 pt-2 border-t">
          <div className="flex items-center gap-2">
            <CheckCircle2 className="h-4 w-4 text-green-600 dark:text-green-400" />
            <div>
              <div className="text-sm font-medium">{stats.completedMilestones}</div>
              <div className="text-xs text-muted-foreground">Completed</div>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Clock className="h-4 w-4 text-blue-600 dark:text-blue-400" />
            <div>
              <div className="text-sm font-medium">{stats.pendingMilestones}</div>
              <div className="text-xs text-muted-foreground">Pending</div>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Clock className="h-4 w-4 text-orange-600 dark:text-orange-400" />
            <div>
              <div className="text-sm font-medium">{stats.inProgressMilestones}</div>
              <div className="text-xs text-muted-foreground">In Progress</div>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <XCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
            <div>
              <div className="text-sm font-medium">{stats.missedMilestones}</div>
              <div className="text-xs text-muted-foreground">Missed</div>
            </div>
          </div>
        </div>

        {/* Upcoming */}
        <div className="pt-2 border-t">
          <div className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4 text-blue-600 dark:text-blue-400" />
            <div>
              <div className="text-sm font-medium">{stats.upcomingMilestones}</div>
              <div className="text-xs text-muted-foreground">Upcoming</div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

