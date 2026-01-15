import { useQuery } from '@tanstack/react-query';
import { metricsApi } from '@/api/metrics';
import { projectsApi } from '@/api/projects';
import { sprintsApi } from '@/api/sprints';
import type { Project } from '@/types';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Skeleton } from '@/components/ui/skeleton';
import { EmptyState, LoadingState } from '@/components/states';
import { useState, useMemo } from 'react';
import { 
  TrendingUp, 
  Activity, 
  Target, 
  Bug,
  BarChart3,
  Gauge,
} from 'lucide-react';
import { 
  LineChart, 
  Line, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
} from 'recharts';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatDate, DateFormats } from '@/utils/dateFormat';
import { formatNumber, formatPercentage } from '@/utils/numberFormat';

const severityColors: Record<string, string> = {
  Critical: 'hsl(0, 72%, 50%)',
  High: 'hsl(25, 95%, 53%)',
  Medium: 'hsl(40, 96%, 50%)',
  Low: 'hsl(142, 71%, 45%)',
};

export default function Metrics() {
  const { language } = useLanguage();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('all');

  const projectIdNum = selectedProjectId === 'all' ? undefined : parseInt(selectedProjectId);

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const { data: metrics, isLoading: metricsLoading } = useQuery({
    queryKey: ['metrics', selectedProjectId],
    queryFn: () => 
      selectedProjectId === 'all' 
        ? metricsApi.get() 
        : metricsApi.get(parseInt(selectedProjectId)),
  });

  const { data: velocityChart, isLoading: velocityLoading } = useQuery({
    queryKey: ['sprint-velocity-chart', selectedProjectId],
    queryFn: () => metricsApi.getSprintVelocityChart(projectIdNum),
  });

  const { data: taskDistribution, isLoading: distributionLoading } = useQuery({
    queryKey: ['task-distribution', selectedProjectId],
    queryFn: () => metricsApi.getTaskDistribution(projectIdNum),
  });

  const { data: defectsBySeverity, isLoading: defectsLoading } = useQuery({
    queryKey: ['defects-by-severity', selectedProjectId],
    queryFn: () => metricsApi.getDefectsBySeverity(projectIdNum),
  });

  // Get active sprint for burndown
  const { data: sprintsData } = useQuery({
    queryKey: ['sprints', projectIdNum],
    queryFn: () => sprintsApi.getByProject(projectIdNum!),
    enabled: !!projectIdNum,
  });

  const activeSprint = sprintsData?.sprints?.find((s) => s.status === 'Active');

  const { data: burndownData, isLoading: burndownLoading } = useQuery({
    queryKey: ['sprint-burndown', activeSprint?.id],
    queryFn: () => metricsApi.getSprintBurndown(activeSprint!.id),
    enabled: !!activeSprint?.id,
  });

  const { isLoading: teamVelocityLoading } = useQuery({
    queryKey: ['team-velocity', selectedProjectId],
    queryFn: () => metricsApi.getTeamVelocity(projectIdNum),
  });

  // Transform data for charts
  const velocityChartData = useMemo(() => {
    if (!velocityChart?.sprints) return [];
    return velocityChart.sprints.map((s) => ({
      sprint: `Sprint ${s.number}`,
      points: s.storyPoints,
      date: formatDate(new Date(s.completedDate), DateFormats.MONTH_DAY(language), language),
    }));
  }, [velocityChart, language]);

  const burndownChartData = useMemo(() => {
    if (!burndownData?.days) return [];
    return burndownData.days.map((d) => ({
      day: `Day ${d.day}`,
      remaining: d.actual,
      ideal: d.ideal,
    }));
  }, [burndownData]);

  const defectChartData = useMemo(() => {
    if (!defectsBySeverity?.defects) return [];
    return defectsBySeverity.defects
      .filter((d) => d.count > 0)
      .map((d) => ({
        name: d.severity,
        value: d.count,
        color: severityColors[d.severity] || severityColors.Medium,
      }));
  }, [defectsBySeverity]);

  const isLoading = metricsLoading || velocityLoading || distributionLoading || defectsLoading || burndownLoading || teamVelocityLoading;

  const metricCards = [
    {
      title: 'Velocity',
      value: formatNumber(metrics?.velocity ?? 38, language),
      unit: 'pts/sprint',
      icon: TrendingUp,
      description: 'Average story points completed per sprint',
    },
    {
      title: 'Throughput',
      value: formatNumber(metrics?.throughput ?? 24, language),
      unit: 'tasks/sprint',
      icon: Activity,
      description: 'Average tasks completed per sprint',
    },
    {
      title: 'Delivery Predictability',
      value: formatPercentage((metrics?.deliveryPredictability ?? 85) / 100, language, { minimumFractionDigits: 0, maximumFractionDigits: 0 }),
      unit: '',
      icon: Target,
      description: 'Percentage of committed work delivered',
    },
    {
      title: 'Defect Rate',
      value: formatPercentage((metrics?.defectRate ?? 4.2) / 100, language, { minimumFractionDigits: 1, maximumFractionDigits: 1 }),
      unit: '',
      icon: Bug,
      description: 'Defects per total tasks completed',
    },
    {
      title: 'Sprint Health',
      value: metrics?.sprintHealth ?? 'Good',
      unit: '',
      icon: Gauge,
      description: 'Overall sprint health assessment',
    },
    {
      title: 'Open Defects',
      value: formatNumber(metrics?.defectsCount ?? 12, language),
      unit: '',
      icon: BarChart3,
      description: 'Total open defects across projects',
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Metrics</h1>
          <p className="text-muted-foreground">Track your team's performance and health</p>
        </div>
        <Select value={selectedProjectId} onValueChange={setSelectedProjectId}>
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="Select project" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Projects</SelectItem>
            {projectsData?.items?.map((project: Project) => (
              <SelectItem key={project.id} value={project.id.toString()}>
                {project.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <LoadingState count={6} skeletonHeight="h-32" />
      ) : (
        <>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {metricCards.map((metric) => (
              <Card key={metric.title}>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium text-muted-foreground">
                    {metric.title}
                  </CardTitle>
                  <metric.icon className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">
                    {metric.value}
                    {metric.unit && (
                      <span className="text-sm font-normal text-muted-foreground ml-1">
                        {metric.unit}
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">{metric.description}</p>
                </CardContent>
              </Card>
            ))}
          </div>

          <div className="grid gap-6 lg:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Sprint Burndown</CardTitle>
                <CardDescription>
                  {activeSprint 
                    ? `Remaining work vs ideal burndown for ${activeSprint.name}`
                    : 'No active sprint - burndown chart unavailable'}
                </CardDescription>
              </CardHeader>
              <CardContent>
                {burndownLoading ? (
                  <Skeleton className="h-[300px]" />
                ) : burndownChartData.length === 0 ? (
                  <EmptyState
                    icon={<Activity />}
                    title={activeSprint ? 'No burndown data available' : 'No active sprint'}
                    description={activeSprint ? 'Burndown data will appear here once available' : 'Select a project with an active sprint to view burndown chart'}
                    className="h-[300px] border-0 shadow-none py-8"
                  />
                ) : (
                  <div className="h-[300px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <LineChart data={burndownChartData}>
                        <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                        <XAxis dataKey="day" className="text-xs" />
                        <YAxis className="text-xs" />
                        <Tooltip
                          contentStyle={{
                            backgroundColor: 'hsl(var(--card))',
                            border: '1px solid hsl(var(--border))',
                            borderRadius: '8px',
                          }}
                        />
                        <Line
                          type="monotone"
                          dataKey="remaining"
                          stroke="hsl(var(--primary))"
                          strokeWidth={2}
                          dot={{ fill: 'hsl(var(--primary))' }}
                          name="Remaining"
                        />
                        <Line
                          type="monotone"
                          dataKey="ideal"
                          stroke="hsl(var(--muted-foreground))"
                          strokeDasharray="5 5"
                          strokeWidth={2}
                          dot={false}
                          name="Ideal"
                        />
                      </LineChart>
                    </ResponsiveContainer>
                  </div>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Defects by Severity</CardTitle>
                <CardDescription>Distribution of open defects</CardDescription>
              </CardHeader>
              <CardContent>
                {defectsLoading ? (
                  <Skeleton className="h-[300px]" />
                ) : defectChartData.length === 0 ? (
                  <EmptyState
                    icon={<Bug />}
                    title="No open defects"
                    description="Great job! There are no open defects to display"
                    className="h-[300px] border-0 shadow-none py-8"
                  />
                ) : (
                  <div className="h-[300px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <PieChart>
                        <Pie
                          data={defectChartData}
                          cx="50%"
                          cy="50%"
                          innerRadius={60}
                          outerRadius={100}
                          paddingAngle={4}
                          dataKey="value"
                          label={({ name, value }) => `${name}: ${value}`}
                        >
                          {defectChartData.map((entry, index) => (
                            <Cell key={`cell-${index}`} fill={entry.color} />
                          ))}
                        </Pie>
                        <Tooltip
                          contentStyle={{
                            backgroundColor: 'hsl(var(--card))',
                            border: '1px solid hsl(var(--border))',
                            borderRadius: '8px',
                          }}
                        />
                      </PieChart>
                    </ResponsiveContainer>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>

          {/* Additional Charts */}
          <div className="grid gap-6 lg:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Sprint Velocity</CardTitle>
                <CardDescription>Story points completed per sprint (last 6 sprints)</CardDescription>
              </CardHeader>
              <CardContent>
                {velocityLoading ? (
                  <Skeleton className="h-[300px]" />
                ) : velocityChartData.length === 0 ? (
                  <EmptyState
                    icon={<TrendingUp />}
                    title="No completed sprints available"
                    description="Sprint velocity data will appear here once sprints are completed"
                    className="h-[300px] border-0 shadow-none py-8"
                  />
                ) : (
                  <div className="h-[300px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={velocityChartData}>
                        <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                        <XAxis dataKey="sprint" className="text-xs" />
                        <YAxis className="text-xs" />
                        <Tooltip
                          contentStyle={{
                            backgroundColor: 'hsl(var(--card))',
                            border: '1px solid hsl(var(--border))',
                            borderRadius: '8px',
                          }}
                        />
                        <Bar dataKey="points" fill="hsl(var(--primary))" />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Task Distribution</CardTitle>
                <CardDescription>Tasks by status</CardDescription>
              </CardHeader>
              <CardContent>
                {distributionLoading ? (
                  <Skeleton className="h-[300px]" />
                ) : !taskDistribution?.distribution ? (
                  <EmptyState
                    icon={<BarChart3 />}
                    title="No task data available"
                    description="Task distribution data will appear here once tasks are created"
                    className="h-[300px] border-0 shadow-none py-8"
                  />
                ) : (
                  <div className="h-[300px]">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={taskDistribution.distribution}>
                        <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                        <XAxis dataKey="status" className="text-xs" />
                        <YAxis className="text-xs" />
                        <Tooltip
                          contentStyle={{
                            backgroundColor: 'hsl(var(--card))',
                            border: '1px solid hsl(var(--border))',
                            borderRadius: '8px',
                          }}
                        />
                        <Bar dataKey="count" fill="hsl(var(--primary))" />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </>
      )}
    </div>
  );
}
