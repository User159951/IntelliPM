import { useQuery } from '@tanstack/react-query';
import { adminApi, type SystemHealthDto, type ExternalServiceStatus } from '@/api/admin';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { Loader2, Cpu, HardDrive, Database, CheckCircle, XCircle, AlertCircle, RefreshCw } from 'lucide-react';
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

const getStatusBadge = (status: string) => {
  switch (status.toLowerCase()) {
    case 'healthy':
      return <Badge variant="default" className="bg-green-500">Healthy</Badge>;
    case 'degraded':
      return <Badge variant="default" className="bg-yellow-500">Degraded</Badge>;
    case 'unhealthy':
      return <Badge variant="destructive">Unhealthy</Badge>;
    default:
      return <Badge variant="secondary">Unknown</Badge>;
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
          <p>Error loading system health. Please try again later.</p>
        </div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="container mx-auto p-6">
        <p className="text-muted-foreground">No system health data available.</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold mb-2">System Health</h1>
          <p className="text-muted-foreground">
            Monitor system resources and service status in real-time.
          </p>
        </div>
        <Button variant="outline" onClick={handleRefresh} disabled={isLoading}>
          <RefreshCw className={`mr-2 h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
          Refresh
        </Button>
      </div>

      {/* Resource Usage Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">CPU Usage</CardTitle>
            <Cpu className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.cpuUsage.toFixed(1)}%</div>
            <Progress value={data.cpuUsage} className="mt-2" />
            <p className="text-xs text-muted-foreground mt-2">
              {data.cpuUsage > 80 ? 'High usage' : data.cpuUsage > 50 ? 'Moderate usage' : 'Normal usage'}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Memory Usage</CardTitle>
            <HardDrive className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.memoryUsage.toFixed(1)}%</div>
            <Progress value={data.memoryUsage} className="mt-2" />
            <p className="text-xs text-muted-foreground mt-2">
              {formatBytes(data.usedMemoryBytes)} / {formatBytes(data.totalMemoryBytes)}
            </p>
            <p className="text-xs text-muted-foreground">
              Available: {formatBytes(data.availableMemoryBytes)}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Database</CardTitle>
            <Database className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2 mb-2">
              <div className="text-2xl font-bold">{getStatusBadge(data.databaseStatus)}</div>
            </div>
            <p className="text-xs text-muted-foreground">
              Response time: {data.databaseResponseTimeMs}ms
            </p>
          </CardContent>
        </Card>
      </div>

      {/* External Services */}
      <Card>
        <CardHeader>
          <CardTitle>External Services</CardTitle>
          <CardDescription>Status of external services and dependencies</CardDescription>
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
                      {service.statusMessage || (service.isHealthy ? 'Operational' : 'Unavailable')}
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
                    {service.isHealthy ? 'Healthy' : 'Unhealthy'}
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
          <CardTitle>System Information</CardTitle>
          <CardDescription>Last updated system metrics</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-muted-foreground">Last Updated:</span>
              <p className="font-medium">
                {format(new Date(data.timestamp), 'MMM d, yyyy HH:mm:ss')}
              </p>
            </div>
            <div>
              <span className="text-muted-foreground">Total Memory:</span>
              <p className="font-medium">{formatBytes(data.totalMemoryBytes)}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Used Memory:</span>
              <p className="font-medium">{formatBytes(data.usedMemoryBytes)}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Available Memory:</span>
              <p className="font-medium">{formatBytes(data.availableMemoryBytes)}</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

