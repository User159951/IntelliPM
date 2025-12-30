import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Activity } from 'lucide-react';
import { cn } from '@/lib/utils';
import { releasesApi } from '@/api/releases';
import { BlockedReleasesWidget } from './BlockedReleasesWidget';
import { PendingApprovalsWidget } from './PendingApprovalsWidget';
import { QualityTrendChart } from './QualityTrendChart';
import { DeploymentFrequencyChart } from './DeploymentFrequencyChart';

interface ReleaseHealthDashboardProps {
  projectId: number;
  className?: string;
}

interface UpcomingReleasesWidgetProps {
  projectId: number;
}

interface RecentDeploymentsWidgetProps {
  projectId: number;
}

function getHealthColor(score: number): string {
  if (score >= 80) return 'bg-green-100 text-green-600 dark:bg-green-900 dark:text-green-400';
  if (score >= 60) return 'bg-yellow-100 text-yellow-600 dark:bg-yellow-900 dark:text-yellow-400';
  return 'bg-red-100 text-red-600 dark:bg-red-900 dark:text-red-400';
}

function UpcomingReleasesWidget({ projectId }: UpcomingReleasesWidgetProps) {
  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  const upcomingReleases = useMemo(() => {
    if (!releases) return [];
    const now = new Date();
    return releases
      .filter(
        (r) =>
          new Date(r.plannedDate) > now &&
          (r.status === 'Planned' || r.status === 'InProgress')
      )
      .sort(
        (a, b) =>
          new Date(a.plannedDate).getTime() - new Date(b.plannedDate).getTime()
      )
      .slice(0, 5);
  }, [releases]);

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Upcoming Releases</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Loading...</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Upcoming Releases</CardTitle>
      </CardHeader>
      <CardContent>
        {upcomingReleases.length === 0 ? (
          <p className="text-sm text-muted-foreground">No upcoming releases</p>
        ) : (
          <div className="space-y-2">
            {upcomingReleases.map((release) => (
              <div
                key={release.id}
                className="flex items-center justify-between p-2 rounded-lg border"
              >
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-sm truncate">{release.version}</p>
                  <p className="text-xs text-muted-foreground truncate">
                    {release.name}
                  </p>
                </div>
                <div
                  className={cn(
                    'h-2 w-2 rounded-full ml-2 shrink-0',
                    release.overallQualityStatus === 'Passed'
                      ? 'bg-green-500'
                      : release.overallQualityStatus === 'Warning'
                        ? 'bg-yellow-500'
                        : release.overallQualityStatus === 'Failed'
                          ? 'bg-red-500'
                          : 'bg-gray-400'
                  )}
                  aria-label={`Quality status: ${release.overallQualityStatus || 'Pending'}`}
                />
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function RecentDeploymentsWidget({ projectId }: RecentDeploymentsWidgetProps) {
  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  const recentDeployments = useMemo(() => {
    if (!releases) return [];
    return releases
      .filter((r) => r.status === 'Deployed' && r.actualReleaseDate)
      .sort(
        (a, b) =>
          new Date(b.actualReleaseDate!).getTime() -
          new Date(a.actualReleaseDate!).getTime()
      )
      .slice(0, 5);
  }, [releases]);

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Recent Deployments</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Loading...</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Recent Deployments</CardTitle>
      </CardHeader>
      <CardContent>
        {recentDeployments.length === 0 ? (
          <p className="text-sm text-muted-foreground">No recent deployments</p>
        ) : (
          <div className="space-y-2">
            {recentDeployments.map((release) => (
              <div
                key={release.id}
                className="flex items-center justify-between p-2 rounded-lg border border-green-200 dark:border-green-800 bg-green-50 dark:bg-green-950"
              >
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-sm truncate">{release.version}</p>
                  <p className="text-xs text-muted-foreground truncate">
                    {release.name}
                  </p>
                </div>
                <div className="text-xs text-muted-foreground ml-2 shrink-0">
                  {new Date(release.actualReleaseDate!).toLocaleDateString()}
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

/**
 * Comprehensive dashboard showing release health metrics and quality insights.
 * Displays overall health score, upcoming releases, deployments, and quality trends.
 */
export function ReleaseHealthDashboard({
  projectId,
  className,
}: ReleaseHealthDashboardProps) {
  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  const healthScore = useMemo(() => {
    if (!releases || releases.length === 0) return 0;

    const totalReleases = releases.length;
    const passedReleases = releases.filter(
      (r) => r.overallQualityStatus === 'Passed'
    ).length;
    const deployedReleases = releases.filter((r) => r.status === 'Deployed').length;

    // Calculate score: 60% based on quality gates, 40% based on deployment success
    const qualityScore = (passedReleases / totalReleases) * 60;
    const deploymentScore = (deployedReleases / totalReleases) * 40;

    return Math.round(qualityScore + deploymentScore);
  }, [releases]);

  if (isLoading) {
    return (
      <div className={cn('space-y-6', className)}>
        <Card>
          <CardHeader>
            <CardTitle>Release Health Overview</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">Loading...</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className={cn('space-y-6', className)}>
      {/* Header with overall score */}
      <Card>
        <CardHeader>
          <CardTitle>Release Health Overview</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-3xl font-bold">{healthScore}%</p>
              <p className="text-sm text-muted-foreground">Overall Health Score</p>
            </div>
            <div
              className={cn(
                'h-16 w-16 rounded-full flex items-center justify-center',
                getHealthColor(healthScore)
              )}
            >
              <Activity className="h-8 w-8" />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Grid of widgets */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UpcomingReleasesWidget projectId={projectId} />
        <RecentDeploymentsWidget projectId={projectId} />
        <BlockedReleasesWidget projectId={projectId} />
        <PendingApprovalsWidget projectId={projectId} />
      </div>

      {/* Quality trend chart */}
      <QualityTrendChart projectId={projectId} />

      {/* Deployment frequency chart */}
      <DeploymentFrequencyChart projectId={projectId} />
    </div>
  );
}

