import { useQuery } from '@tanstack/react-query';
import { readModelService, type SprintSummaryDto, type BurndownPointDto } from '@/services/readModelService';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Clock,
  CheckCircle,
  AlertTriangle,
  AlertCircle,
} from 'lucide-react';
import { format } from 'date-fns';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

interface SprintMetricsProps {
  sprintId: number;
  compact?: boolean;
}

export default function SprintMetrics({
  sprintId,
  compact = false,
}: SprintMetricsProps) {
  const { data: sprintSummary, isLoading, error } = useQuery({
    queryKey: ['sprint-summary', sprintId],
    queryFn: () => readModelService.getSprintSummary(sprintId),
    staleTime: 1000 * 60, // 1 minute
    refetchInterval: 1000 * 60 * 2, // Auto-refresh every 2 minutes
  });

  if (isLoading) {
    return <SprintMetricsSkeleton compact={compact} />;
  }

  if (error || !sprintSummary) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>Failed to load sprint metrics</AlertDescription>
      </Alert>
    );
  }

  if (compact) {
    return <CompactSprintMetrics summary={sprintSummary} />;
  }

  return (
    <div className="space-y-6">
      {/* Sprint header with status */}
      <SprintHeader summary={sprintSummary} />

      {/* Key metrics cards */}
      <SprintMetricsGrid summary={sprintSummary} />

      {/* Burndown chart */}
      <BurndownChart data={sprintSummary.burndownData} />

      {/* Progress indicators */}
      <ProgressSection summary={sprintSummary} />
    </div>
  );
}

function SprintHeader({ summary }: { summary: SprintSummaryDto }) {
  const isActive = summary.status === 'Active';
  const daysRemaining = Math.ceil(
    (new Date(summary.endDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24)
  );

  return (
    <div className="flex items-start justify-between">
      <div>
        <h2 className="text-2xl font-bold">{summary.sprintName}</h2>
        <div className="flex items-center gap-4 mt-2">
          <Badge variant={isActive ? 'default' : 'secondary'}>
            {summary.status}
          </Badge>
          <div className="text-sm text-muted-foreground">
            {format(new Date(summary.startDate), 'MMM d')} -{' '}
            {format(new Date(summary.endDate), 'MMM d, yyyy')}
          </div>
          {isActive && daysRemaining >= 0 && (
            <Badge variant="outline">
              <Clock className="h-3 w-3 mr-1" />
              {daysRemaining} days remaining
            </Badge>
          )}
        </div>
      </div>

      {summary.isOnTrack ? (
        <Badge className="bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400">
          <CheckCircle className="h-3 w-3 mr-1" />
          On Track
        </Badge>
      ) : (
        <Badge className="bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400">
          <AlertTriangle className="h-3 w-3 mr-1" />
          Behind Schedule
        </Badge>
      )}
    </div>
  );
}

function SprintMetricsGrid({ summary }: { summary: SprintSummaryDto }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium">Completion</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {summary.completionPercentage.toFixed(1)}%
          </div>
          <Progress value={summary.completionPercentage} className="mt-2" />
          <p className="text-xs text-muted-foreground mt-1">
            {summary.completedTasks} / {summary.totalTasks} tasks
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium">Velocity</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {summary.completedStoryPoints} / {summary.totalStoryPoints}
          </div>
          <Progress value={summary.velocityPercentage} className="mt-2" />
          <p className="text-xs text-muted-foreground mt-1">
            {summary.velocityPercentage.toFixed(1)}% of planned
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium">Capacity</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {summary.capacityUtilization?.toFixed(0) || 0}%
          </div>
          <Progress value={summary.capacityUtilization || 0} className="mt-2" />
          <p className="text-xs text-muted-foreground mt-1">
            Planned capacity utilization
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium">Remaining</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {summary.remainingStoryPoints}
          </div>
          <p className="text-xs text-muted-foreground mt-1">
            story points remaining
          </p>
          {summary.estimatedDaysRemaining > 0 && (
            <p className="text-xs text-muted-foreground">
              ~{summary.estimatedDaysRemaining} days estimated
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function BurndownChart({ data }: { data: BurndownPointDto[] }) {
  if (data.length === 0) {
    return null;
  }

  const chartData = data.map((point) => ({
    date: format(new Date(point.date), 'MMM d'),
    remaining: point.remainingStoryPoints,
    ideal: point.idealRemainingPoints,
  }));

  return (
    <Card>
      <CardHeader>
        <CardTitle>Burndown Chart</CardTitle>
        <CardDescription>Actual vs. ideal story points remaining</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis label={{ value: 'Story Points', angle: -90, position: 'insideLeft' }} />
            <Tooltip />
            <Legend />
            <Line
              type="monotone"
              dataKey="ideal"
              stroke="hsl(var(--muted-foreground))"
              strokeDasharray="5 5"
              name="Ideal"
              dot={false}
            />
            <Line
              type="monotone"
              dataKey="remaining"
              stroke="hsl(var(--primary))"
              name="Actual"
              strokeWidth={2}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}

function ProgressSection({ summary }: { summary: SprintSummaryDto }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Task Status Breakdown</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <div>
            <div className="flex justify-between text-sm mb-1">
              <span>Completed</span>
              <span className="font-medium">{summary.completedTasks}</span>
            </div>
            <Progress
              value={(summary.completedTasks / summary.totalTasks) * 100}
              className="h-2"
            />
          </div>
          <div>
            <div className="flex justify-between text-sm mb-1">
              <span>In Progress</span>
              <span className="font-medium">{summary.inProgressTasks}</span>
            </div>
            <Progress
              value={(summary.inProgressTasks / summary.totalTasks) * 100}
              className="h-2"
            />
          </div>
          <div>
            <div className="flex justify-between text-sm mb-1">
              <span>To Do</span>
              <span className="font-medium">{summary.todoTasks}</span>
            </div>
            <Progress
              value={(summary.todoTasks / summary.totalTasks) * 100}
              className="h-2"
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Story Points Breakdown</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <div>
            <div className="flex justify-between text-sm mb-1">
              <span>Completed</span>
              <span className="font-medium">{summary.completedStoryPoints} SP</span>
            </div>
            <Progress
              value={(summary.completedStoryPoints / summary.totalStoryPoints) * 100}
              className="h-2"
            />
          </div>
          <div>
            <div className="flex justify-between text-sm mb-1">
              <span>In Progress</span>
              <span className="font-medium">{summary.inProgressStoryPoints} SP</span>
            </div>
            <Progress
              value={(summary.inProgressStoryPoints / summary.totalStoryPoints) * 100}
              className="h-2"
            />
          </div>
          <div>
            <div className="flex justify-between text-sm mb-1">
              <span>Remaining</span>
              <span className="font-medium">{summary.remainingStoryPoints} SP</span>
            </div>
            <Progress
              value={(summary.remainingStoryPoints / summary.totalStoryPoints) * 100}
              className="h-2"
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

function CompactSprintMetrics({ summary }: { summary: SprintSummaryDto }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{summary.sprintName}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <div>
          <div className="flex justify-between text-sm mb-1">
            <span>Tasks</span>
            <span className="font-medium">
              {summary.completedTasks}/{summary.totalTasks}
            </span>
          </div>
          <Progress value={summary.completionPercentage} />
        </div>

        <div>
          <div className="flex justify-between text-sm mb-1">
            <span>Story Points</span>
            <span className="font-medium">
              {summary.completedStoryPoints}/{summary.totalStoryPoints}
            </span>
          </div>
          <Progress value={summary.velocityPercentage} />
        </div>

        <Badge variant={summary.isOnTrack ? 'default' : 'secondary'} className="w-full justify-center">
          {summary.isOnTrack ? 'On Track' : 'Behind Schedule'}
        </Badge>
      </CardContent>
    </Card>
  );
}

function SprintMetricsSkeleton({ compact }: { compact?: boolean }) {
  if (compact) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-32" />
        </CardHeader>
        <CardContent className="space-y-3">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-8 w-full" />
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-6 w-48" />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {[...Array(4)].map((_, i) => (
          <Skeleton key={i} className="h-32" />
        ))}
      </div>
      <Skeleton className="h-64" />
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Skeleton className="h-48" />
        <Skeleton className="h-48" />
      </div>
    </div>
  );
}

