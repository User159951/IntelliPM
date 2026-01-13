import { useQuery } from '@tanstack/react-query';
import { readModelService, type ProjectOverviewDto, type TaskBoardDto } from '@/services/readModelService';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Skeleton } from '@/components/ui/skeleton';
import {
  HeartPulse,
  RefreshCw,
  AlertCircle,
  TrendingUp,
  Target,
  Users,
  Zap,
  Calendar,
  AlertTriangle,
} from 'lucide-react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend, LineChart, Line, XAxis, YAxis, CartesianGrid } from 'recharts';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatNumber, formatPercentage, formatDecimal } from '@/utils/numberFormat';

interface ProjectDashboardProps {
  projectId: number;
}

export default function ProjectDashboard({ projectId }: ProjectDashboardProps) {
  // Fetch project overview read model
  const { data: overview, isLoading, error, refetch } = useQuery({
    queryKey: ['project-overview', projectId],
    queryFn: () => readModelService.getProjectOverview(projectId),
    staleTime: 1000 * 60 * 2, // 2 minutes
    refetchInterval: 1000 * 60 * 5, // Auto-refresh every 5 minutes
  });

  // Fetch task board read model
  const { data: taskBoard } = useQuery({
    queryKey: ['task-board', projectId],
    queryFn: () => readModelService.getTaskBoard(projectId),
    staleTime: 1000 * 60 * 2,
  });

  if (isLoading) {
    return <ProjectDashboardSkeleton />;
  }

  if (error) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertTitle>Error</AlertTitle>
        <AlertDescription>Failed to load project dashboard</AlertDescription>
      </Alert>
    );
  }

  if (!overview) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center">
        <AlertCircle className="h-12 w-12 mb-3 opacity-50 text-muted-foreground" />
        <p className="text-sm text-muted-foreground">Project overview not available</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header with project info and health */}
      <ProjectHeader overview={overview} onRefresh={refetch} />

      {/* Key metrics cards */}
      <MetricsGrid overview={overview} />

      {/* Task distribution chart */}
      {taskBoard && <TaskDistributionChart taskBoard={taskBoard} />}

      {/* Velocity trend chart */}
      {overview.velocityTrend.length > 0 && (
        <VelocityTrendChart data={overview.velocityTrend} />
      )}

      {/* Team members section */}
      <TeamSection members={overview.teamMembers} />

      {/* Risk factors */}
      {overview.riskFactors.length > 0 && (
        <RiskFactorsCard risks={overview.riskFactors} />
      )}
    </div>
  );
}

function ProjectHeader({ overview, onRefresh }: {
  overview: ProjectOverviewDto;
  onRefresh: () => void;
}) {
  const { language } = useLanguage();
  const healthColor = {
    Excellent: 'text-green-600 bg-green-100 dark:text-green-400 dark:bg-green-900/20',
    Good: 'text-blue-600 bg-blue-100 dark:text-blue-400 dark:bg-blue-900/20',
    Fair: 'text-yellow-600 bg-yellow-100 dark:text-yellow-400 dark:bg-yellow-900/20',
    Poor: 'text-red-600 bg-red-100 dark:text-red-400 dark:bg-red-900/20',
  }[overview.healthStatus] || 'text-gray-600 bg-gray-100 dark:text-gray-400 dark:bg-gray-900/20';

  return (
    <div className="flex items-start justify-between">
      <div>
        <h1 className="text-3xl font-bold">{overview.projectName}</h1>
        <div className="flex items-center gap-4 mt-2">
          <Badge variant="outline">{overview.projectType}</Badge>
          <Badge variant={overview.status === 'Active' ? 'default' : 'secondary'}>
            {overview.status}
          </Badge>
          <div className={`flex items-center gap-2 px-3 py-1 rounded-full ${healthColor}`}>
            <HeartPulse className="h-4 w-4" />
            <span className="font-medium">{overview.healthStatus}</span>
            <span className="text-sm">({formatPercentage(overview.projectHealth / 100, language, { minimumFractionDigits: 0, maximumFractionDigits: 0 })})</span>
          </div>
        </div>
      </div>

      <Button variant="outline" size="sm" onClick={() => onRefresh()}>
        <RefreshCw className="h-4 w-4 mr-2" />
        Refresh
      </Button>
    </div>
  );
}

function MetricsGrid({ overview }: { overview: ProjectOverviewDto }) {
  const { language } = useLanguage();
  
  const metrics = [
    {
      label: 'Overall Progress',
      value: formatPercentage(overview.overallProgress / 100, language, { minimumFractionDigits: 1, maximumFractionDigits: 1 }),
      icon: TrendingUp,
      color: 'text-blue-600',
      subtext: `${formatNumber(overview.completedTasks, language)} / ${formatNumber(overview.totalTasks, language)} tasks`,
    },
    {
      label: 'Story Points',
      value: formatNumber(overview.totalStoryPoints, language),
      icon: Target,
      color: 'text-purple-600',
      subtext: `${formatNumber(overview.completedStoryPoints, language)} completed`,
    },
    {
      label: 'Team Members',
      value: formatNumber(overview.totalMembers, language),
      icon: Users,
      color: 'text-green-600',
      subtext: `${formatNumber(overview.activeMembers, language)} active`,
    },
    {
      label: 'Average Velocity',
      value: formatDecimal(overview.averageVelocity, language, 1),
      icon: Zap,
      color: 'text-orange-600',
      subtext: 'points per sprint',
    },
    {
      label: 'Sprints',
      value: formatNumber(overview.totalSprints, language),
      icon: Calendar,
      color: 'text-indigo-600',
      subtext: `${formatNumber(overview.activeSprintsCount, language)} active`,
    },
    {
      label: 'Open Defects',
      value: formatNumber(overview.openDefects, language),
      icon: AlertTriangle,
      color: overview.criticalDefects > 0 ? 'text-red-600' : 'text-yellow-600',
      subtext: `${formatNumber(overview.criticalDefects, language)} critical`,
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {metrics.map((metric) => (
        <Card key={metric.label}>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {metric.label}
            </CardTitle>
            <metric.icon className={`h-4 w-4 ${metric.color}`} />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metric.value}</div>
            <p className="text-xs text-muted-foreground mt-1">{metric.subtext}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

function TaskDistributionChart({ taskBoard }: { taskBoard: TaskBoardDto }) {
  const data = [
    { name: 'To Do', value: taskBoard.todoCount, color: 'hsl(var(--chart-1))' },
    { name: 'In Progress', value: taskBoard.inProgressCount, color: 'hsl(var(--chart-2))' },
    { name: 'Done', value: taskBoard.doneCount, color: 'hsl(var(--chart-3))' },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Task Distribution</CardTitle>
        <CardDescription>Tasks by status</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={200}>
          <PieChart>
            <Pie
              data={data}
              dataKey="value"
              nameKey="name"
              cx="50%"
              cy="50%"
              outerRadius={80}
              label
            >
              {data.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.color} />
              ))}
            </Pie>
            <Tooltip />
            <Legend />
          </PieChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}

function VelocityTrendChart({ data }: { data: ProjectOverviewDto['velocityTrend'] }) {
  const chartData = data.map((item) => ({
    name: item.sprintName,
    velocity: item.velocity,
  }));

  return (
    <Card>
      <CardHeader>
        <CardTitle>Velocity Trend</CardTitle>
        <CardDescription>Sprint velocity over time</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <Tooltip />
            <Legend />
            <Line
              type="monotone"
              dataKey="velocity"
              stroke="hsl(var(--chart-1))"
              strokeWidth={2}
              name="Velocity"
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}

function TeamSection({ members }: { members: ProjectOverviewDto['teamMembers'] }) {
  if (members.length === 0) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Team Members</CardTitle>
        <CardDescription>Active team members and their contributions</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {members.map((member) => (
            <div key={member.userId} className="flex items-center justify-between p-3 border rounded-lg">
              <div>
                <p className="font-medium">{member.username}</p>
                <p className="text-sm text-muted-foreground">{member.role}</p>
              </div>
              <div className="text-right">
                <p className="text-sm font-medium">{member.tasksCompleted} / {member.tasksAssigned}</p>
                <p className="text-xs text-muted-foreground">tasks</p>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

function RiskFactorsCard({ risks }: { risks: string[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <AlertTriangle className="h-5 w-5 text-yellow-600" />
          Risk Factors
        </CardTitle>
        <CardDescription>Potential issues that may impact project health</CardDescription>
      </CardHeader>
      <CardContent>
        <ul className="space-y-2">
          {risks.map((risk, index) => (
            <li key={index} className="flex items-start gap-2">
              <span className="text-yellow-600 mt-1">•</span>
              <span className="text-sm">{risk}</span>
            </li>
          ))}
        </ul>
      </CardContent>
    </Card>
  );
}

function ProjectDashboardSkeleton() {
  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-6 w-48" />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {[...Array(6)].map((_, i) => (
          <Skeleton key={i} className="h-32" />
        ))}
      </div>
      <Skeleton className="h-64" />
    </div>
  );
}

