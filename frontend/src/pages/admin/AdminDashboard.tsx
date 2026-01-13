import { useQuery } from '@tanstack/react-query';
import { adminApi } from '@/api/admin';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Users, FolderKanban, Building2, TrendingUp, Activity, CheckCircle, AlertCircle } from 'lucide-react';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatDate, DateFormats } from '@/utils/dateFormat';
import { useTranslation } from 'react-i18next';
import {
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042'];

export default function AdminDashboard() {
  const { language } = useLanguage();
  const { t } = useTranslation('admin');
  const { data, isLoading, error } = useQuery({
    queryKey: ['admin-dashboard-stats'],
    queryFn: adminApi.getDashboardStats,
  });

  if (isLoading) {
    return (
      <div className="container mx-auto p-6 space-y-6">
        <Skeleton className="h-10 w-48" />
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[...Array(3)].map((_, i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <div className="bg-destructive/10 text-destructive p-4 rounded-lg">
          <p>{t('dashboard.errors.loadError')}</p>
        </div>
      </div>
    );
  }

  if (!data) {
    return null;
  }

  // Prepare pie chart data for Admin vs User
  const roleDistribution = [
    { name: 'Admin', value: data.adminCount || 0 },
    { name: 'User', value: data.userCount || 0 },
  ];

  // Prepare line chart data for user growth
  const growthData = (data.userGrowth || []).map((item) => ({
    month: item.month,
    users: item.count,
  }));

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold mb-2">{t('dashboard.title')}</h1>
        <p className="text-muted-foreground">
          {t('dashboard.description')}
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{t('dashboard.stats.totalUsers')}</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.totalUsers}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {data.activeUsers} {t('dashboard.stats.activeUsers')}, {data.inactiveUsers} {t('dashboard.stats.inactiveUsers')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{t('dashboard.stats.totalProjects')}</CardTitle>
            <FolderKanban className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.totalProjects}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {data.activeProjects} {t('dashboard.stats.activeProjects')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{t('dashboard.stats.organizations')}</CardTitle>
            <Building2 className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.totalOrganizations}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {t('dashboard.stats.totalOrganizations')}
            </p>
          </CardContent>
        </Card>

        {data.systemHealth && (
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{t('dashboard.stats.systemHealth')}</CardTitle>
              {data.systemHealth.databaseStatus === 'Healthy' ? (
                <CheckCircle className="h-4 w-4 text-green-500" />
              ) : (
                <AlertCircle className="h-4 w-4 text-destructive" />
              )}
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{data.systemHealth.databaseStatus || 'Unknown'}</div>
              <p className="text-xs text-muted-foreground mt-1">
                CPU: {typeof data.systemHealth.cpuUsage === 'number' ? data.systemHealth.cpuUsage.toFixed(1) : '0.0'}% | Memory: {typeof data.systemHealth.memoryUsage === 'number' ? data.systemHealth.memoryUsage.toFixed(1) : '0.0'}%
              </p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* User Growth Chart */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5" />
              {t('dashboard.charts.userGrowth')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {growthData.length === 0 ? (
              <p className="text-muted-foreground text-center py-8">
                {t('dashboard.charts.noGrowthData')}
              </p>
            ) : (
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={growthData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="month" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="users"
                  stroke="#0088FE"
                  strokeWidth={2}
                  name={t('dashboard.charts.newUsers')}
                />
              </LineChart>
            </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        {/* Role Distribution Chart */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              {t('dashboard.charts.roleDistribution')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {roleDistribution.every(r => r.value === 0) ? (
              <p className="text-muted-foreground text-center py-8">
                {t('dashboard.charts.noRoleData')}
              </p>
            ) : (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={roleDistribution}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                    {roleDistribution.map((_, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Recent Activities */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Activity className="h-5 w-5" />
            {t('dashboard.charts.recentActivities')}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {!data.recentActivities || data.recentActivities.length === 0 ? (
            <p className="text-muted-foreground text-center py-8">
              {t('dashboard.charts.noActivities')}
            </p>
          ) : (
            <div className="space-y-4">
              {data.recentActivities.map((activity, index) => (
                <div
                  key={`activity-${activity.userName}-${activity.timestamp}-${index}`}
                  className="flex items-center justify-between p-3 rounded-lg border"
                >
                  <div className="flex-1">
                    <p className="font-medium">{activity.action}</p>
                    <p className="text-sm text-muted-foreground">
                      by {activity.userName}
                    </p>
                  </div>
                  <div className="text-sm text-muted-foreground">
                    {activity.timestamp ? (() => {
                      try {
                        return formatDate(activity.timestamp, DateFormats.DATETIME(language), language);
                      } catch {
                        return 'Invalid date';
                      }
                    })() : 'N/A'}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
