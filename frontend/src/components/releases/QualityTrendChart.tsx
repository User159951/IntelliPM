import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { format } from 'date-fns';
import { releasesApi } from '@/api/releases';

interface QualityTrendChartProps {
  projectId: number;
  timeRange?: '30d' | '90d' | '6m';
  className?: string;
}

/**
 * Line chart showing quality gate pass rate over time.
 * Displays monthly pass rate percentage for releases.
 */
export function QualityTrendChart({
  projectId,
  timeRange = '90d',
  className,
}: QualityTrendChartProps) {
  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  const chartData = useMemo(() => {
    if (!releases) return [];

    // Filter by time range
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

    const filteredReleases = releases.filter(
      (r) => new Date(r.plannedDate) >= cutoffDate
    );

    // Group releases by month
    const monthlyData = filteredReleases.reduce(
      (acc, release) => {
        const month = format(new Date(release.plannedDate), 'MMM yyyy');
        if (!acc[month]) {
          acc[month] = { month, total: 0, passed: 0 };
        }
        acc[month].total++;
        if (release.overallQualityStatus === 'Passed') {
          acc[month].passed++;
        }
        return acc;
      },
      {} as Record<
        string,
        { month: string; total: number; passed: number }
      >
    );

    // Calculate pass rate and sort by date
    return Object.values(monthlyData)
      .map((item) => ({
        month: item.month,
        passRate:
          item.total > 0
            ? Math.round((item.passed / item.total) * 100)
            : 0,
      }))
      .sort((a, b) => {
        const dateA = new Date(a.month);
        const dateB = new Date(b.month);
        return dateA.getTime() - dateB.getTime();
      });
  }, [releases, timeRange]);

  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle>Quality Gate Pass Rate</CardTitle>
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
          <CardTitle>Quality Gate Pass Rate</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-[300px] flex items-center justify-center">
            <p className="text-sm text-muted-foreground">
              No release data available for the selected time range
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>Quality Gate Pass Rate</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
            <XAxis
              dataKey="month"
              tick={{ fontSize: 12 }}
              stroke="hsl(var(--muted-foreground))"
            />
            <YAxis
              domain={[0, 100]}
              tick={{ fontSize: 12 }}
              stroke="hsl(var(--muted-foreground))"
            />
            <Tooltip
              contentStyle={{
                backgroundColor: 'hsl(var(--popover))',
                border: '1px solid hsl(var(--border))',
                borderRadius: '6px',
              }}
              formatter={(value: number) => [`${value}%`, 'Pass Rate']}
            />
            <Line
              type="monotone"
              dataKey="passRate"
              stroke="hsl(var(--primary))"
              strokeWidth={2}
              name="Pass Rate (%)"
              dot={{ fill: 'hsl(var(--primary))', r: 4 }}
              activeDot={{ r: 6 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}

