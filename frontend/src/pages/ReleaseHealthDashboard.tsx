import React from 'react';
import { useParams } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { RefreshCw } from 'lucide-react';
import { ReleaseHealthDashboard } from '@/components/releases/ReleaseHealthDashboard';

/**
 * Full page dashboard for release health monitoring.
 * Displays comprehensive quality metrics, trends, and release status.
 */
export default function ReleaseHealthDashboardPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const queryClient = useQueryClient();
  const [isRefreshing, setIsRefreshing] = React.useState(false);

  const handleRefresh = async () => {
    if (!projectId) return;
    setIsRefreshing(true);
    try {
      // Invalidate all release-related queries for this project
      await queryClient.invalidateQueries({ queryKey: ['project-releases', Number(projectId)] });
      await queryClient.invalidateQueries({ queryKey: ['releaseStatistics', Number(projectId)] });
      await queryClient.invalidateQueries({ queryKey: ['projectReleases', Number(projectId)] });
    } finally {
      setIsRefreshing(false);
    }
  };

  if (!projectId) {
    return (
      <div className="container mx-auto py-6">
        <p className="text-muted-foreground">Invalid project ID</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Release Health Dashboard</h1>
          <p className="text-muted-foreground">
            Monitor release quality and deployment metrics
          </p>
        </div>
        <Button
          variant="outline"
          onClick={handleRefresh}
          disabled={isRefreshing}
        >
          <RefreshCw className={`h-4 w-4 mr-2 ${isRefreshing ? 'animate-spin' : ''}`} />
          Refresh
        </Button>
      </div>

      <ReleaseHealthDashboard projectId={Number(projectId)} />
    </div>
  );
}

