import { useQuery } from '@tanstack/react-query';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  ReferenceLine,
} from 'recharts';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { AlertCircle } from 'lucide-react';
import { metricsApi, type SprintVelocityResponse } from '@/api/metrics';

interface VelocityChartProps {
  projectId: number;
  lastNSprints?: number;
  height?: number;
  showAverageLine?: boolean;
}

interface CustomTooltipProps {
  active?: boolean;
  payload?: Array<{
    value: number;
    dataKey: string;
    payload: {
      name: string;
      completed: number;
      planned: number;
      completionRate: number;
    };
  }>;
}

const CustomTooltip = ({ active, payload }: CustomTooltipProps) => {
  if (active && payload && payload.length) {
    const data = payload[0].payload;
    // Find completed and planned values from payload
    const completedBar = payload.find((p) => p.dataKey === 'completed');
    const plannedBar = payload.find((p) => p.dataKey === 'planned');
    
    return (
      <div className="bg-popover p-3 rounded-lg border shadow-lg">
        <p className="font-semibold mb-2">{data.name}</p>
        <div className="space-y-1 text-sm">
          <p>
            Completed: <span className="font-bold">{completedBar?.value ?? data.completed}</span> points
          </p>
          <p>
            Planned: <span className="font-bold">{plannedBar?.value ?? data.planned}</span> points
          </p>
          <p>
            Completion: <span className="font-bold">{data.completionRate.toFixed(1)}%</span>
          </p>
        </div>
      </div>
    );
  }
  return null;
};

export default function VelocityChart({
  projectId,
  lastNSprints = 5,
  height = 300,
  showAverageLine = true,
}: VelocityChartProps) {
  // Fetch velocity data
  const { data, isLoading, error } = useQuery<SprintVelocityResponse>({
    queryKey: ['sprint-velocity', projectId, lastNSprints],
    queryFn: () => metricsApi.getVelocity(projectId, lastNSprints),
    enabled: !!projectId,
  });

  // Prepare chart data
  const chartData =
    data?.sprints.map((sprint) => ({
      name: sprint.sprintName,
      completed: sprint.completedStoryPoints,
      planned: sprint.plannedStoryPoints,
      completionRate: sprint.completionRate,
    })) || [];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Sprint Velocity</CardTitle>
        <CardDescription>
          Completed story points per sprint (Last {lastNSprints} sprints)
        </CardDescription>
      </CardHeader>
      <CardContent>
        {isLoading && <Skeleton className="h-[300px]" />}

        {error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>Error</AlertTitle>
            <AlertDescription>Failed to load velocity data</AlertDescription>
          </Alert>
        )}

        {data && !isLoading && (
          <>
            <div className="mb-4 grid grid-cols-3 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Average Velocity</p>
                <p className="text-2xl font-bold">{data.averageVelocity.toFixed(1)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Completed</p>
                <p className="text-2xl font-bold">{data.totalCompletedStoryPoints}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Sprints</p>
                <p className="text-2xl font-bold">{data.sprints.length}</p>
              </div>
            </div>

            {chartData.length === 0 ? (
              <div className="h-[300px] flex items-center justify-center text-muted-foreground">
                <div className="text-center">
                  <p className="font-medium">No sprint data available</p>
                  <p className="text-sm mt-1">Complete sprints to see velocity metrics</p>
                </div>
              </div>
            ) : (
              <ResponsiveContainer width="100%" height={height} aria-label="Sprint velocity chart">
                <BarChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis
                    dataKey="name"
                    className="text-xs"
                    tick={{ fill: 'hsl(var(--muted-foreground))' }}
                  />
                  <YAxis
                    label={{ value: 'Story Points', angle: -90, position: 'insideLeft' }}
                    className="text-xs"
                    tick={{ fill: 'hsl(var(--muted-foreground))' }}
                  />
                  <Tooltip content={<CustomTooltip />} />
                  <Legend />
                  <Bar
                    dataKey="completed"
                    fill="hsl(var(--primary))"
                    name="Completed"
                    radius={[4, 4, 0, 0]}
                  />
                  <Bar
                    dataKey="planned"
                    fill="hsl(var(--muted))"
                    name="Planned"
                    radius={[4, 4, 0, 0]}
                  />
                  {showAverageLine && data.averageVelocity > 0 && (
                    <ReferenceLine
                      y={data.averageVelocity}
                      stroke="hsl(var(--destructive))"
                      strokeDasharray="5 5"
                      label={{
                        value: 'Average',
                        position: 'right',
                        fill: 'hsl(var(--destructive))',
                      }}
                    />
                  )}
                </BarChart>
              </ResponsiveContainer>
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}

