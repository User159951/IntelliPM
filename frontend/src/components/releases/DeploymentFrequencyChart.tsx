import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { format } from 'date-fns';
import { releasesApi } from '@/api/releases';

interface DeploymentFrequencyChartProps {
  projectId: number;
  timeRange?: '30d' | '90d' | '6m';
  className?: string;
}

/**
 * Bar chart showing deployment frequency by release type.
 * Displays monthly deployment counts grouped by Major, Minor, Patch, Hotfix.
 */
export function DeploymentFrequencyChart({
  projectId,
  timeRange = '90d',
  className,
}: DeploymentFrequencyChartProps) {
  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  const chartData = useMemo(() => {
    if (!releases) return [];

    // Filter deployed releases by time range
    const now = new Date();
    const cutoffDate = new Date();
    switch (timeRange) {
      case '30d':
        cutoffDate.setDate(now.getDate() - 30);
        break;
      case '90d':
        cutoffDate.setDate(now.getDate() - 90);
        break;
      case '6m':
        cutoffDate.setMonth(now.getMonth() - 6);
        break;
    }

    const deployed = releases.filter(
      (r) =>
        r.status === 'Deployed' &&
        r.actualReleaseDate &&
        new Date(r.actualReleaseDate) >= cutoffDate
    );

    // Group by month and type
    const monthlyData = deployed.reduce(
      (acc, release) => {
        const month = format(
          new Date(release.actualReleaseDate!),
          'MMM yyyy'
        );
        if (!acc[month]) {
          acc[month] = { month, Major: 0, Minor: 0, Patch: 0, Hotfix: 0 };
        }
        const type = release.type as 'Major' | 'Minor' | 'Patch' | 'Hotfix';
        if (Object.prototype.hasOwnProperty.call(acc[month], type)) {
          acc[month][type]++;
        }
        return acc;
      },
      {} as Record<
        string,
        { month: string; Major: number; Minor: number; Patch: number; Hotfix: number }
      >
    );

    // Sort by date
    return Object.values(monthlyData).sort((a, b) => {
      const dateA = new Date(a.month);
      const dateB = new Date(b.month);
      return dateA.getTime() - dateB.getTime();
    });
  }, [releases, timeRange]);

  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle>Deployment Frequency</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[300px] flex items-center justify-center">
            <p className="text-sm text-muted-foreground">Loading chart data...</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (chartData.length === 0) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle>Deployment Frequency</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[300px] flex items-center justify-center">
            <p className="text-sm text-muted-foreground">
              No deployment data available for the selected time range
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>Deployment Frequency</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
            <XAxis
              dataKey="month"
              tick={{ fontSize: 12 }}
              stroke="hsl(var(--muted-foreground))"
            />
            <YAxis tick={{ fontSize: 12 }} stroke="hsl(var(--muted-foreground))" />
            <Tooltip
              contentStyle={{
                backgroundColor: 'hsl(var(--popover))',
                border: '1px solid hsl(var(--border))',
                borderRadius: '6px',
              }}
            />
            <Legend />
            <Bar dataKey="Major" fill="#ef4444" name="Major" />
            <Bar dataKey="Minor" fill="#3b82f6" name="Minor" />
            <Bar dataKey="Patch" fill="#10b981" name="Patch" />
            <Bar dataKey="Hotfix" fill="#f59e0b" name="Hotfix" />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}

