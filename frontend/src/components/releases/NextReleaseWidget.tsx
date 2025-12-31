import { useState, useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import {
  Calendar,
  Clock,
  Package,
  ArrowRight,
  Rocket,
  Plus,
} from 'lucide-react';
import { format } from 'date-fns';
import { cn } from '@/lib/utils';
import { releasesApi } from '@/api/releases';
import { showToast } from '@/lib/sweetalert';
import { DeployReleaseDialog } from './DeployReleaseDialog';

interface NextReleaseWidgetProps {
  projectId: number;
  className?: string;
}

interface EmptyStateProps {
  projectId: number;
}

function EmptyState({ projectId }: EmptyStateProps) {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col items-center justify-center py-8 text-center">
      <Package className="h-12 w-12 text-muted-foreground mb-3" />
      <h4 className="font-semibold mb-1">No Upcoming Releases</h4>
      <p className="text-sm text-muted-foreground mb-4">
        Create your first release to track progress
      </p>
      <Button
        size="sm"
        onClick={() => navigate(`/projects/${projectId}/releases`)}
      >
        <Plus className="h-4 w-4 mr-2" />
        Create Release
      </Button>
    </div>
  );
}

function getQualityIndicatorColor(status: string | null): string {
  switch (status) {
    case 'Passed':
      return 'bg-green-500';
    case 'Warning':
      return 'bg-yellow-500';
    case 'Failed':
      return 'bg-red-500';
    case 'Pending':
      return 'bg-gray-400';
    default:
      return 'bg-gray-300';
  }
}

function getTypeBadgeVariant(
  type: string
): 'default' | 'destructive' | 'secondary' | 'outline' {
  switch (type) {
    case 'Major':
      return 'destructive';
    case 'Hotfix':
      return 'destructive';
    case 'Minor':
      return 'default';
    case 'Patch':
      return 'secondary';
    default:
      return 'default';
  }
}

/**
 * Widget component displaying the next planned release for a project.
 * Shows countdown, quality gates status, task completion, and quick actions.
 */
export function NextReleaseWidget({
  projectId,
  className,
}: NextReleaseWidgetProps) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isDeployOpen, setIsDeployOpen] = useState(false);

  // Fetch planned releases
  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId, 'Planned'],
    queryFn: () => releasesApi.getProjectReleases(projectId, 'Planned'),
    enabled: !!projectId,
    refetchInterval: 5 * 60 * 1000, // Auto-refresh every 5 minutes
  });

  // Find next planned release
  const nextRelease = useMemo(() => {
    if (!releases || releases.length === 0) return null;

    const now = new Date();
    const futureReleases = releases
      .filter((r) => new Date(r.plannedDate) > now)
      .sort(
        (a, b) =>
          new Date(a.plannedDate).getTime() -
          new Date(b.plannedDate).getTime()
      );

    return futureReleases.length > 0 ? futureReleases[0] : null;
  }, [releases]);

  // Calculate countdown
  const countdown = useMemo(() => {
    if (!nextRelease) return null;

    const now = new Date();
    const releaseDate = new Date(nextRelease.plannedDate);
    const diffMs = releaseDate.getTime() - now.getTime();

    if (diffMs < 0) {
      const overdueDays = Math.abs(
        Math.floor(diffMs / (1000 * 60 * 60 * 24))
      );
      return { text: `Overdue by ${overdueDays}d`, isOverdue: true };
    }

    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));

    if (diffDays > 30) {
      const months = Math.floor(diffDays / 30);
      return {
        text: `${months} month${months > 1 ? 's' : ''} left`,
        isOverdue: false,
      };
    } else if (diffDays >= 7) {
      const weeks = Math.floor(diffDays / 7);
      return {
        text: `${weeks} week${weeks > 1 ? 's' : ''} left`,
        isOverdue: false,
      };
    } else if (diffDays >= 1) {
      return {
        text: `${diffDays} day${diffDays > 1 ? 's' : ''} left`,
        isOverdue: false,
      };
    } else if (diffHours >= 1) {
      return {
        text: `${diffHours} hour${diffHours > 1 ? 's' : ''} left`,
        isOverdue: false,
      };
    } else {
      return { text: 'Releasing soon!', isOverdue: false };
    }
  }, [nextRelease]);

  // Calculate task completion percentage
  const taskCompletionPercentage = useMemo(() => {
    if (!nextRelease || nextRelease.totalTasksCount === 0) return 0;
    return Math.round(
      (nextRelease.completedTasksCount / nextRelease.totalTasksCount) * 100
    );
  }, [nextRelease]);

  // Check if release can be deployed
  const canDeploy = useMemo(() => {
    if (!nextRelease) return false;
    return (
      nextRelease.status === 'ReadyForDeployment' &&
      nextRelease.overallQualityStatus === 'Passed'
    );
  }, [nextRelease]);

  return (
    <TooltipProvider>
      <Card className={cn('relative overflow-hidden', className)}>
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">Next Release</CardTitle>
            <Badge variant="outline">
              <Calendar className="h-3 w-3 mr-1" />
              Upcoming
            </Badge>
          </div>
        </CardHeader>

        <CardContent>
          {isLoading ? (
            <div className="space-y-3">
              <Skeleton className="h-6 w-32" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-8 w-full" />
            </div>
          ) : !nextRelease ? (
            <EmptyState projectId={projectId} />
          ) : (
            <div className="space-y-4">
              {/* Version and Name */}
              <div>
                <div className="flex items-center gap-2 mb-1">
                  <h3 className="text-2xl font-bold">
                    {nextRelease.version}
                  </h3>
                  <Badge variant={getTypeBadgeVariant(nextRelease.type)}>
                    {nextRelease.type}
                  </Badge>
                  {nextRelease.isPreRelease && (
                    <Badge variant="secondary">Pre-release</Badge>
                  )}
                </div>
                <p className="text-sm text-muted-foreground">
                  {nextRelease.name}
                </p>
              </div>

              {/* Countdown */}
              <div className="flex items-center gap-2">
                <Clock
                  className={cn(
                    'h-4 w-4',
                    countdown?.isOverdue ? 'text-destructive' : 'text-primary'
                  )}
                />
                <span
                  className={cn(
                    'text-sm font-medium',
                    countdown?.isOverdue
                      ? 'text-destructive'
                      : 'text-foreground'
                  )}
                >
                  {countdown?.text}
                </span>
                <span className="text-xs text-muted-foreground">
                  ({format(new Date(nextRelease.plannedDate), 'MMM d, yyyy')})
                </span>
              </div>

              {/* Quality Gates Indicator */}
              <div className="flex items-center gap-2">
                <div className="flex items-center gap-1">
                  <div
                    className={cn(
                      'h-2 w-2 rounded-full',
                      getQualityIndicatorColor(nextRelease.overallQualityStatus)
                    )}
                  />
                  <span className="text-xs text-muted-foreground">
                    Quality Gates
                  </span>
                </div>
                <Tooltip>
                  <TooltipTrigger>
                    <Badge variant="outline" className="text-xs">
                      {nextRelease.overallQualityStatus || 'Pending'}
                    </Badge>
                  </TooltipTrigger>
                  <TooltipContent>
                    <p className="text-xs">Overall quality gate status</p>
                  </TooltipContent>
                </Tooltip>
              </div>

              {/* Task Progress */}
              <div className="space-y-2">
                <div className="flex items-center justify-between text-xs">
                  <span className="text-muted-foreground">
                    Task Completion
                  </span>
                  <span className="font-medium">
                    {nextRelease.completedTasksCount}/
                    {nextRelease.totalTasksCount} ({taskCompletionPercentage}%)
                  </span>
                </div>
                <Progress value={taskCompletionPercentage} className="h-2" />
              </div>

              {/* Quick Actions */}
              <div className="flex gap-2 pt-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="flex-1"
                  onClick={() =>
                    navigate(
                      `/projects/${projectId}/releases/${nextRelease.id}`
                    )
                  }
                >
                  <ArrowRight className="h-4 w-4 mr-2" />
                  View Details
                </Button>

                {canDeploy && (
                  <Button
                    size="sm"
                    className="flex-1"
                    onClick={() => setIsDeployOpen(true)}
                  >
                    <Rocket className="h-4 w-4 mr-2" />
                    Deploy
                  </Button>
                )}
              </div>
            </div>
          )}
        </CardContent>

        {/* Deploy Dialog */}
        {nextRelease && (
          <DeployReleaseDialog
            release={nextRelease}
            open={isDeployOpen}
            onOpenChange={setIsDeployOpen}
            onSuccess={() => {
              queryClient.invalidateQueries({
                queryKey: ['project-releases', projectId],
              });
              showToast('Release deployed successfully', 'success');
            }}
          />
        )}
      </Card>
    </TooltipProvider>
  );
}

