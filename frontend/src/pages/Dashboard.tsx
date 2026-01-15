import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { metricsApi } from '@/api/metrics';
import { projectsApi } from '@/api/projects';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { EmptyState, ErrorState } from '@/components/states';
import { 
  FolderKanban, 
  Zap, 
  ListTodo, 
  AlertTriangle, 
  Bug, 
  TrendingUp,
  ArrowUpRight,
  ArrowDownRight,
  ArrowRight,
} from 'lucide-react';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar } from 'recharts';
import { RecentActivity } from '@/components/dashboard/RecentActivity';
import { useTranslation } from 'react-i18next';
import type { Project } from '@/types';

// Helper to format trend values for display
const formatTrend = (value: number | undefined): { text: string; isPositive: boolean } => {
  if (value === undefined || value === 0) {
    return { text: '0%', isPositive: true };
  }
  const isPositive = value > 0;
  return {
    text: `${isPositive ? '+' : ''}${value}%`,
    isPositive
  };
};

export default function Dashboard() {
  const navigate = useNavigate();
  const { t } = useTranslation('dashboard');
  
  // Fetch metrics summary
  const { data: metrics, isLoading: metricsLoading } = useQuery({
    queryKey: ['metrics'],
    queryFn: () => metricsApi.get(),
  });

  // Fetch sprint velocity chart data
  const { data: velocityData, isLoading: velocityLoading, error: velocityError } = useQuery({
    queryKey: ['metrics', 'sprint-velocity-chart'],
    queryFn: () => metricsApi.getSprintVelocityChart(),
  });

  // Fetch task distribution data
  const { data: distributionData, isLoading: distributionLoading, error: distributionError } = useQuery({
    queryKey: ['metrics', 'task-distribution'],
    queryFn: () => metricsApi.getTaskDistribution(),
  });

  const { data: projectsData, isLoading: projectsLoading } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  // Transform velocity data for chart
  const chartVelocityData = velocityData?.sprints?.map(s => ({
    sprint: `S${s.number}`,
    points: s.storyPoints
  })) ?? [];

  // Transform task distribution data for chart - map status names for display
  const chartDistributionData = distributionData?.distribution?.map(d => ({
    status: d.status === 'InProgress' ? 'In Progress' : d.status,
    count: d.count
  })) ?? [];

  // Build stats with real trends from backend
  const trends = metrics?.trends;
  const stats = [
    {
      title: t('stats.totalProjects'),
      value: metrics?.totalProjects ?? projectsData?.items?.length ?? 0,
      icon: FolderKanban,
      ...formatTrend(trends?.projectsTrend),
    },
    {
      title: t('stats.activeSprints'),
      value: metrics?.activeSprints ?? 0,
      icon: Zap,
      ...formatTrend(trends?.sprintsTrend),
    },
    {
      title: t('stats.openTasks'),
      value: metrics?.openTasks ?? 0,
      icon: ListTodo,
      ...formatTrend(trends?.openTasksTrend),
      // For open tasks, a decrease is positive (fewer open tasks)
      get isPositive() { return (trends?.openTasksTrend ?? 0) <= 0; }
    },
    {
      title: t('stats.blockedTasks'),
      value: metrics?.blockedTasks ?? 0,
      icon: AlertTriangle,
      ...formatTrend(trends?.blockedTasksTrend),
      // For blocked tasks, a decrease is positive
      get isPositive() { return (trends?.blockedTasksTrend ?? 0) <= 0; }
    },
    {
      title: t('stats.defects'),
      value: metrics?.defectsCount ?? 0,
      icon: Bug,
      ...formatTrend(trends?.defectsTrend),
      // For defects, a decrease is positive
      get isPositive() { return (trends?.defectsTrend ?? 0) <= 0; }
    },
    {
      title: t('stats.velocity'),
      value: metrics?.velocity ?? 0,
      icon: TrendingUp,
      ...formatTrend(trends?.velocityTrend),
    },
  ];

  const isLoading = metricsLoading || projectsLoading;
  const hasVelocityData = chartVelocityData.length > 0;
  const hasDistributionData = chartDistributionData.length > 0;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">{t('title')}</h1>
        <p className="text-muted-foreground">{t('description')}</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        {stats.map((stat) => (
          <Card key={stat.title}>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {stat.title}
              </CardTitle>
              <stat.icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <Skeleton className="h-8 w-16" />
              ) : (
                <div className="flex items-baseline gap-2">
                  <div className="text-2xl font-bold">{stat.value}</div>
                  {trends && (
                    <span
                      className={`flex items-center text-xs ${
                        stat.isPositive ? 'text-green-500' : 'text-red-500'
                      }`}
                    >
                      {stat.isPositive ? (
                        <ArrowUpRight className="h-3 w-3" />
                      ) : (
                        <ArrowDownRight className="h-3 w-3" />
                      )}
                      {stat.text}
                    </span>
                  )}
                </div>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>{t('charts.sprintVelocity.title')}</CardTitle>
            <CardDescription>{t('charts.sprintVelocity.description')}</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="h-[300px]">
              {velocityLoading ? (
                <div className="flex items-center justify-center h-full">
                  <Skeleton className="h-full w-full" />
                </div>
              ) : velocityError ? (
                <ErrorState
                  title={t('charts.sprintVelocity.error', 'Failed to load velocity data')}
                  message="Please try again later"
                  onRetry={() => window.location.reload()}
                  className="h-full py-8"
                />
              ) : !hasVelocityData ? (
                <EmptyState
                  icon={<TrendingUp />}
                  title={t('charts.sprintVelocity.noData', 'No completed sprints yet')}
                  className="h-full border-0 shadow-none py-8"
                />
              ) : (
                <ResponsiveContainer width="100%" height="100%">
                  <AreaChart data={chartVelocityData}>
                    <defs>
                      <linearGradient id="colorPoints" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="hsl(var(--primary))" stopOpacity={0.3} />
                        <stop offset="95%" stopColor="hsl(var(--primary))" stopOpacity={0} />
                      </linearGradient>
                    </defs>
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
                    <Area
                      type="monotone"
                      dataKey="points"
                      stroke="hsl(var(--primary))"
                      fillOpacity={1}
                      fill="url(#colorPoints)"
                      strokeWidth={2}
                    />
                  </AreaChart>
                </ResponsiveContainer>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{t('charts.taskDistribution.title')}</CardTitle>
            <CardDescription>{t('charts.taskDistribution.description')}</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="h-[300px]">
              {distributionLoading ? (
                <div className="flex items-center justify-center h-full">
                  <Skeleton className="h-full w-full" />
                </div>
              ) : distributionError ? (
                <ErrorState
                  title={t('charts.taskDistribution.error', 'Failed to load distribution data')}
                  message="Please try again later"
                  onRetry={() => window.location.reload()}
                  className="h-full py-8"
                />
              ) : !hasDistributionData ? (
                <EmptyState
                  icon={<ListTodo />}
                  title={t('charts.taskDistribution.noData', 'No tasks yet')}
                  className="h-full border-0 shadow-none py-8"
                />
              ) : (
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={chartDistributionData}>
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
                    <Bar dataKey="count" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>{t('recentProjects.title')}</CardTitle>
            <CardDescription>{t('recentProjects.description')}</CardDescription>
          </CardHeader>
          <CardContent>
            {projectsLoading ? (
              <div className="space-y-4">
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} className="h-16 w-full" />
                ))}
              </div>
            ) : (
              <div className="space-y-4">
                {projectsData?.items && projectsData.items.length > 0 ? (
                  <>
                    {projectsData.items.slice(0, 5).map((project: Project) => (
                      <div
                        key={project.id}
                        className="flex items-center justify-between rounded-lg border border-border p-4 transition-colors hover:bg-accent/50 cursor-pointer"
                        onClick={() => navigate(`/projects/${project.id}`)}
                      >
                        <div className="space-y-1">
                          <p className="font-medium">{project.name}</p>
                          <p className="text-sm text-muted-foreground">
                            {project.description || 'No description'}
                          </p>
                        </div>
                        <div className="flex items-center gap-4">
                          <span
                            className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                              project.status === 'Active'
                                ? 'bg-green-500/10 text-green-500'
                                : project.status === 'OnHold'
                                ? 'bg-yellow-500/10 text-yellow-500'
                                : 'bg-muted text-muted-foreground'
                            }`}
                          >
                            {project.status}
                          </span>
                          <span className="text-sm text-muted-foreground">{project.type}</span>
                        </div>
                      </div>
                    ))}
                    <div className="pt-2 border-t">
                      <Button
                        variant="ghost"
                        size="sm"
                        className="w-full"
                        onClick={() => navigate('/projects')}
                      >
                        {t('recentProjects.viewAll')}
                        <ArrowRight className="ml-2 h-4 w-4" />
                      </Button>
                    </div>
                  </>
                ) : (
                  <EmptyState
                    icon={<FolderKanban />}
                    title={t('recentProjects.empty.message', 'No projects yet')}
                    description="Get started by creating your first project"
                    action={{
                      label: t('recentProjects.empty.button', 'Create Project'),
                      onClick: () => navigate('/projects'),
                    }}
                    className="border-0 shadow-none"
                  />
                )}
              </div>
            )}
          </CardContent>
        </Card>

        <RecentActivity limit={10} />
      </div>
    </div>
  );
}
