import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { adminApi } from '@/api/admin';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { Cpu, HardDrive, Database, CheckCircle, XCircle, RefreshCw } from 'lucide-react';
import { format } from 'date-fns';
import { Button } from '@/components/ui/button';
import { useQueryClient } from '@tanstack/react-query';

const formatBytes = (bytes: number): string => {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${(bytes / Math.pow(k, i)).toFixed(2)} ${sizes[i]}`;
};

const getStatusBadge = (status: string, t: (key: string) => string) => {
  switch (status.toLowerCase()) {
    case 'healthy':
      return <Badge variant="default" className="bg-green-500">{t('systemHealth.status.healthy')}</Badge>;
    case 'degraded':
      return <Badge variant="default" className="bg-yellow-500">{t('systemHealth.status.degraded')}</Badge>;
    case 'unhealthy':
      return <Badge variant="destructive">{t('systemHealth.status.unhealthy')}</Badge>;
    default:
      return <Badge variant="secondary">{t('systemHealth.status.unknown')}</Badge>;
  }
};

const getServiceStatusIcon = (isHealthy: boolean) => {
  return isHealthy ? (
    <CheckCircle className="h-5 w-5 text-green-500" />
  ) : (
    <XCircle className="h-5 w-5 text-destructive" />
  );
};

export default function AdminSystemHealth() {
  const { t } = useTranslation('admin');
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ['system-health'],
    queryFn: adminApi.getSystemHealth,
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  const handleRefresh = () => {
    queryClient.invalidateQueries({ queryKey: ['system-health'] });
  };

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
          <p>{t('systemHealth.errors.loadError')}</p>
        </div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="container mx-auto p-6">
        <p className="text-muted-foreground">{t('systemHealth.errors.noData')}</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold mb-2">{t('systemHealth.title')}</h1>
          <p className="text-muted-foreground">
            {t('systemHealth.description')}
          </p>
        </div>
        <Button variant="outline" onClick={handleRefresh} disabled={isLoading}>
          <RefreshCw className={`mr-2 h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
          {t('systemHealth.refresh')}
        </Button>
      </div>

      {/* Resource Usage Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{t('systemHealth.resources.cpuUsage')}</CardTitle>
            <Cpu className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.cpuUsage.toFixed(1)}%</div>
            <Progress value={data.cpuUsage} className="mt-2" />
            <p className="text-xs text-muted-foreground mt-2">
              {data.cpuUsage > 80 ? t('systemHealth.resources.cpuStatus.high') : data.cpuUsage > 50 ? t('systemHealth.resources.cpuStatus.moderate') : t('systemHealth.resources.cpuStatus.normal')}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{t('systemHealth.resources.memoryUsage')}</CardTitle>
            <HardDrive className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.memoryUsage.toFixed(1)}%</div>
            <Progress value={data.memoryUsage} className="mt-2" />
            <p className="text-xs text-muted-foreground mt-2">
              {formatBytes(data.usedMemoryBytes)} / {formatBytes(data.totalMemoryBytes)}
            </p>
            <p className="text-xs text-muted-foreground">
              {t('systemHealth.systemInfo.availableMemory')} {formatBytes(data.availableMemoryBytes)}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{t('systemHealth.resources.database')}</CardTitle>
            <Database className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2 mb-2">
              <div className="text-2xl font-bold">{getStatusBadge(data.databaseStatus, t)}</div>
            </div>
            <p className="text-xs text-muted-foreground">
              {t('systemHealth.resources.responseTime')} {data.databaseResponseTimeMs}ms
            </p>
          </CardContent>
        </Card>
      </div>

      {/* External Services */}
      <Card>
        <CardHeader>
          <CardTitle>{t('systemHealth.externalServices.title')}</CardTitle>
          <CardDescription>{t('systemHealth.externalServices.description')}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {Object.entries(data.externalServices).map(([key, service]) => (
              <div
                key={key}
                className="flex items-center justify-between p-4 rounded-lg border"
              >
                <div className="flex items-center gap-3">
                  {getServiceStatusIcon(service.isHealthy)}
                  <div>
                    <p className="font-medium">{service.name}</p>
                    <p className="text-sm text-muted-foreground">
                      {service.statusMessage || (service.isHealthy ? t('systemHealth.externalServices.operational') : t('systemHealth.externalServices.unavailable'))}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-4">
                  {service.responseTimeMs !== undefined && (
                    <span className="text-sm text-muted-foreground">
                      {service.responseTimeMs}ms
                    </span>
                  )}
                  <Badge variant={service.isHealthy ? 'default' : 'destructive'}>
                    {service.isHealthy ? t('systemHealth.status.healthy') : t('systemHealth.status.unhealthy')}
                  </Badge>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* System Information */}
      <Card>
        <CardHeader>
          <CardTitle>{t('systemHealth.systemInfo.title')}</CardTitle>
          <CardDescription>{t('systemHealth.systemInfo.description')}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-muted-foreground">{t('systemHealth.systemInfo.lastUpdated')}</span>
              <p className="font-medium">
                {format(new Date(data.timestamp), 'MMM d, yyyy HH:mm:ss')}
              </p>
            </div>
            <div>
              <span className="text-muted-foreground">{t('systemHealth.systemInfo.totalMemory')}</span>
              <p className="font-medium">{formatBytes(data.totalMemoryBytes)}</p>
            </div>
            <div>
              <span className="text-muted-foreground">{t('systemHealth.systemInfo.usedMemory')}</span>
              <p className="font-medium">{formatBytes(data.usedMemoryBytes)}</p>
            </div>
            <div>
              <span className="text-muted-foreground">{t('systemHealth.systemInfo.availableMemory')}</span>
              <p className="font-medium">{formatBytes(data.availableMemoryBytes)}</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

