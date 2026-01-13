import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Package,
  CheckCircle,
  Calendar,
  XCircle,
  Clock,
  TrendingUp,
  AlertTriangle,
} from 'lucide-react';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatNumber, formatDecimal, formatPercentage } from '@/utils/numberFormat';
import { format } from 'date-fns';
import { cn } from '@/lib/utils';
import { releasesApi } from '@/api/releases';
import type { ReleaseDto, ReleaseStatistics } from '@/types/releases';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';

interface ReleaseStatisticsProps {
  projectId: number;
  className?: string;
}

interface MetricCardProps {
  icon: React.ElementType;
  label: string;
  value: string | number;
  subtitle?: string;
  iconColor?: string;
  showAlert?: boolean;
}

function MetricCard({
  icon: Icon,
  label,
  value,
  subtitle,
  iconColor,
  showAlert,
}: MetricCardProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{label}</p>
            <p className="text-2xl font-bold">{value}</p>
            {subtitle && (
              <p className="text-xs text-muted-foreground">{subtitle}</p>
            )}
          </div>
          <div className={cn('p-3 rounded-full bg-muted', iconColor)}>
            <Icon className="h-6 w-6" />
          </div>
        </div>
        {showAlert && (
          <Alert variant="destructive" className="mt-3">
            <AlertTriangle className="h-4 w-4" />
            <AlertDescription className="text-xs">
              Some releases have failed
            </AlertDescription>
          </Alert>
        )}
      </CardContent>
    </Card>
  );
}

interface RecentReleaseItemProps {
  release: ReleaseDto;
  onClick: () => void;
}

function RecentReleaseItem({ release, onClick }: RecentReleaseItemProps) {
  return (
    <div
      onClick={onClick}
      className="flex items-center justify-between p-3 rounded-lg border cursor-pointer hover:bg-muted/50 transition-colors"
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onClick();
        }
      }}
    >
      <div className="flex items-center gap-3">
        <div
          className={cn('h-2 w-2 rounded-full', getStatusColor(release.status))}
        />
        <div>
          <p className="font-medium text-sm">{release.version}</p>
          <p className="text-xs text-muted-foreground">{release.name}</p>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <Badge variant="outline">{release.status}</Badge>
        <span className="text-xs text-muted-foreground">
          {format(new Date(release.plannedDate), 'MMM d, yyyy')}
        </span>
      </div>
    </div>
  );
}

function getStatusColor(status: string): string {
  switch (status) {
    case 'Deployed':
      return 'bg-green-500';
    case 'Planned':
      return 'bg-blue-500';
    case 'InProgress':
      return 'bg-orange-500';
    case 'Testing':
      return 'bg-yellow-500';
    case 'ReadyForDeployment':
      return 'bg-purple-500';
    case 'Failed':
      return 'bg-red-500';
    case 'Cancelled':
      return 'bg-gray-500';
    default:
      return 'bg-gray-400';
  }
}

/**
 * Component displaying release statistics and metrics for a project.
 * Shows key metrics, recent releases, and optional chart visualization.
 */
export function ReleaseStatistics({
  projectId,
  className,
}: ReleaseStatisticsProps) {
  const navigate = useNavigate();
  const { language } = useLanguage();

  // Fetch statistics
  const { data: stats, isLoading: isLoadingStats } = useQuery({
    queryKey: ['release-statistics', projectId],
    queryFn: () => releasesApi.getReleaseStatistics(projectId),
    enabled: !!projectId,
    refetchInterval: 5 * 60 * 1000, // Auto-refresh every 5 minutes
  });

  // Fetch recent releases
  const { data: releases, isLoading: isLoadingReleases } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  // Calculate derived metrics
  const deploymentRate = useMemo(() => {
    if (!stats || stats.totalReleases === 0) return 0;
    return Math.round((stats.deployedReleases / stats.totalReleases) * 100);
  }, [stats]);

  const successRate = useMemo(() => {
    if (!stats || stats.totalReleases === 0) return 0;
    const completed = stats.deployedReleases + stats.failedReleases;
    if (completed === 0) return 0;
    return Math.round((stats.deployedReleases / completed) * 100);
  }, [stats]);

  // Get recent releases (last 5)
  const recentReleases = useMemo(() => {
    if (!releases) return [];
    return releases
      .sort(
        (a, b) =>
          new Date(b.plannedDate).getTime() -
          new Date(a.plannedDate).getTime()
      )
      .slice(0, 5);
  }, [releases]);

  // Prepare chart data (optional - releases by month)
  const chartData = useMemo(() => {
    if (!releases || releases.length === 0) return null;

    const monthMap = new Map<string, number>();

    releases.forEach((release) => {
      const date = new Date(release.plannedDate);
      const monthKey = format(date, 'MMM yyyy');
      monthMap.set(monthKey, (monthMap.get(monthKey) || 0) + 1);
    });

    return Array.from(monthMap.entries())
      .map(([month, count]) => ({ month, count }))
      .sort(
        (a, b) =>
          new Date(a.month).getTime() - new Date(b.month).getTime()
      )
      .slice(-6); // Last 6 months
  }, [releases]);

  const isLoading = isLoadingStats || isLoadingReleases;

  return (
    <div className={cn('space-y-6', className)}>
      {/* Metrics Grid */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[...Array(6)].map((_, i) => (
            <Card key={i}>
              <CardContent className="p-6">
                <Skeleton className="h-20 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : !stats ? (
        <Card>
          <CardContent className="p-12 text-center">
            <Package className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <p className="text-sm text-muted-foreground">
              No release data available. Create your first release to see
              statistics.
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <MetricCard
            icon={Package}
            label="Total Releases"
            value={formatNumber(stats.totalReleases ?? 0, language)}
            iconColor="text-blue-500"
          />

          <MetricCard
            icon={CheckCircle}
            label="Deployed"
            value={formatNumber(stats.deployedReleases ?? 0, language)}
            subtitle={`${formatPercentage(deploymentRate / 100, language, { minimumFractionDigits: 0, maximumFractionDigits: 0 })} of total`}
            iconColor="text-green-500"
          />

          <MetricCard
            icon={Calendar}
            label="Planned"
            value={formatNumber(stats.plannedReleases ?? 0, language)}
            iconColor="text-blue-500"
          />

          <MetricCard
            icon={XCircle}
            label="Failed"
            value={formatNumber(stats.failedReleases ?? 0, language)}
            iconColor="text-red-500"
            showAlert={stats.failedReleases > 0}
          />

          <MetricCard
            icon={Clock}
            label="Avg Lead Time"
            value={`${formatDecimal(stats.averageLeadTime ?? 0, language, 1)} days`}
            iconColor="text-purple-500"
          />

          <MetricCard
            icon={TrendingUp}
            label="Success Rate"
            value={formatPercentage(successRate / 100, language, { minimumFractionDigits: 0, maximumFractionDigits: 0 })}
            iconColor="text-green-500"
          />
        </div>
      )}

      {/* Recent Releases */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Recent Releases</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoadingReleases ? (
            <div className="space-y-2">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : recentReleases.length === 0 ? (
            <div className="text-center py-8">
              <Package className="h-12 w-12 text-muted-foreground mx-auto mb-2" />
              <p className="text-sm text-muted-foreground">No releases yet</p>
            </div>
          ) : (
            <div className="space-y-2">
              {recentReleases.map((release) => (
                <RecentReleaseItem
                  key={release.id}
                  release={release}
                  onClick={() =>
                    navigate(`/projects/${projectId}/releases/${release.id}`)
                  }
                />
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Optional: Releases over time chart */}
      {chartData && chartData.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Releases Over Time</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={200}>
              <BarChart data={chartData}>
                <XAxis
                  dataKey="month"
                  tick={{ fontSize: 12 }}
                  angle={-45}
                  textAnchor="end"
                  height={60}
                />
                <YAxis tick={{ fontSize: 12 }} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: 'hsl(var(--popover))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: '6px',
                  }}
                />
                <Bar dataKey="count" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

