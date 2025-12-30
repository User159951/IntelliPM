import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import {
  CheckCircle,
  AlertTriangle,
  XCircle,
  Clock,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { releasesApi } from '@/api/releases';

interface QualityGateWidgetProps {
  releaseId: number;
  className?: string;
  onViewDetails?: () => void;
}

function getOverallStatusColor(status?: string | null): string {
  switch (status) {
    case 'Passed':
      return 'bg-green-500';
    case 'Warning':
      return 'bg-yellow-500';
    case 'Failed':
      return 'bg-red-500';
    default:
      return 'bg-gray-400';
  }
}

/**
 * Compact widget displaying quality gate status for a release.
 * Shows overall status, gate counts, and quick navigation.
 */
export function QualityGateWidget({
  releaseId,
  className,
  onViewDetails,
}: QualityGateWidgetProps) {
  const navigate = useNavigate();

  const { data: release, isLoading } = useQuery({
    queryKey: ['release', releaseId],
    queryFn: () => releasesApi.getRelease(releaseId),
    enabled: !!releaseId,
    refetchInterval: 5 * 60 * 1000, // Auto-refresh every 5 minutes
  });

  const gatesSummary = useMemo(() => {
    if (!release?.qualityGates)
      return { passed: 0, warning: 0, failed: 0, pending: 0 };
    return release.qualityGates.reduce(
      (acc, gate) => {
        if (gate.status === 'Passed') acc.passed++;
        else if (gate.status === 'Warning') acc.warning++;
        else if (gate.status === 'Failed') acc.failed++;
        else acc.pending++;
        return acc;
      },
      { passed: 0, warning: 0, failed: 0, pending: 0 }
    );
  }, [release]);

  const handleViewDetails = () => {
    if (onViewDetails) {
      onViewDetails();
    } else if (release) {
      navigate(
        `/projects/${release.projectId}/releases/${release.id}#quality`
      );
    }
  };

  return (
    <Card className={cn('', className)}>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-sm font-medium">Quality Gates</CardTitle>
          <div
            className={cn(
              'h-3 w-3 rounded-full',
              getOverallStatusColor(release?.overallQualityStatus)
            )}
            aria-label={`Overall quality status: ${release?.overallQualityStatus || 'Pending'}`}
          />
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {isLoading ? (
          <Skeleton className="h-16 w-full" />
        ) : (
          <>
            <div className="grid grid-cols-2 gap-2 text-xs">
              <div className="flex items-center gap-2">
                <CheckCircle className="h-4 w-4 text-green-500" />
                <span>{gatesSummary.passed} Passed</span>
              </div>
              <div className="flex items-center gap-2">
                <AlertTriangle className="h-4 w-4 text-yellow-500" />
                <span>{gatesSummary.warning} Warning</span>
              </div>
              <div className="flex items-center gap-2">
                <XCircle className="h-4 w-4 text-red-500" />
                <span>{gatesSummary.failed} Failed</span>
              </div>
              <div className="flex items-center gap-2">
                <Clock className="h-4 w-4 text-gray-400" />
                <span>{gatesSummary.pending} Pending</span>
              </div>
            </div>

            <Button
              variant="outline"
              size="sm"
              className="w-full"
              onClick={handleViewDetails}
            >
              View Details
            </Button>
          </>
        )}
      </CardContent>
    </Card>
  );
}

