import { useParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { RefreshCw } from 'lucide-react';
import { ReleaseHealthDashboard } from '@/components/releases/ReleaseHealthDashboard';

/**
 * Full page dashboard for release health monitoring.
 * Displays comprehensive quality metrics, trends, and release status.
 */
export default function ReleaseHealthDashboardPage() {
  const { projectId } = useParams<{ projectId: string }>();

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
          onClick={() => window.location.reload()}
        >
          <RefreshCw className="h-4 w-4 mr-2" />
          Refresh
        </Button>
      </div>

      <ReleaseHealthDashboard projectId={Number(projectId)} />
    </div>
  );
}

