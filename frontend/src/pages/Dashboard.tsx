import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { metricsApi } from '@/api/metrics';
import { projectsApi } from '@/api/projects';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
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

const velocityData = [
  { sprint: 'S1', points: 24 },
  { sprint: 'S2', points: 32 },
  { sprint: 'S3', points: 28 },
  { sprint: 'S4', points: 36 },
  { sprint: 'S5', points: 42 },
  { sprint: 'S6', points: 38 },
];

const taskDistribution = [
  { status: 'Todo', count: 12 },
  { status: 'In Progress', count: 8 },
  { status: 'Blocked', count: 3 },
  { status: 'Done', count: 24 },
];

export default function Dashboard() {
  const navigate = useNavigate();
  const { t } = useTranslation('dashboard');
  
  const { data: metrics, isLoading: metricsLoading } = useQuery({
    queryKey: ['metrics'],
    queryFn: () => metricsApi.get(),
  });

  const { data: projectsData, isLoading: projectsLoading } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const stats = [
    {
      title: t('stats.totalProjects'),
      value: metrics?.totalProjects ?? projectsData?.items?.length ?? 0,
      icon: FolderKanban,
      trend: '+12%',
      trendUp: true,
    },
    {
      title: t('stats.activeSprints'),
      value: metrics?.activeSprints ?? 0,
      icon: Zap,
      trend: '+3',
      trendUp: true,
    },
    {
      title: t('stats.openTasks'),
      value: metrics?.openTasks ?? 0,
      icon: ListTodo,
      trend: '-8%',
      trendUp: false,
    },
    {
      title: t('stats.blockedTasks'),
      value: metrics?.blockedTasks ?? 0,
      icon: AlertTriangle,
      trend: '-2',
      trendUp: false,
    },
    {
      title: t('stats.defects'),
      value: metrics?.defectsCount ?? 0,
      icon: Bug,
      trend: '+5%',
      trendUp: true,
    },
    {
      title: t('stats.velocity'),
      value: metrics?.velocity ?? 38,
      icon: TrendingUp,
      trend: '+15%',
      trendUp: true,
    },
  ];

  const isLoading = metricsLoading || projectsLoading;

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
                  <span
                    className={`flex items-center text-xs ${
                      stat.trendUp ? 'text-green-500' : 'text-red-500'
                    }`}
                  >
                    {stat.trendUp ? (
                      <ArrowUpRight className="h-3 w-3" />
                    ) : (
                      <ArrowDownRight className="h-3 w-3" />
                    )}
                    {stat.trend}
                  </span>
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
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={velocityData}>
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
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={taskDistribution}>
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
                  <div className="text-center py-8">
                    <p className="text-muted-foreground mb-4">
                      {t('recentProjects.empty.message')}
                    </p>
                    <Button onClick={() => navigate('/projects')}>
                      {t('recentProjects.empty.button')}
                      <ArrowRight className="ml-2 h-4 w-4" />
                    </Button>
                  </div>
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
