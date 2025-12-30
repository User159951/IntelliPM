import { useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { releasesApi } from '@/api/releases';
import { showToast } from '@/lib/sweetalert';

interface PendingApprovalsWidgetProps {
  projectId: number;
  className?: string;
}

/**
 * Widget displaying releases awaiting manual quality gate approval.
 * Shows releases with pending ManualApproval gates.
 */
export function PendingApprovalsWidget({
  projectId,
  className,
}: PendingApprovalsWidgetProps) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  const pendingReleases = useMemo(() => {
    if (!releases) return [];
    return releases.filter((r) =>
      r.qualityGates?.some(
        (qg) => qg.type === 'ManualApproval' && qg.status === 'Pending'
      )
    );
  }, [releases]);

  const approveMutation = useMutation({
    mutationFn: async ({ releaseId, gateType }: { releaseId: number; gateType: number }) => {
      await releasesApi.approveQualityGate(releaseId, gateType);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project-releases', projectId] });
      showToast('Approval submitted successfully', 'success');
    },
    onError: () => {
      showToast('Failed to submit approval', 'error');
    },
  });

  const handleApprove = async (releaseId: number) => {
    const release = releases?.find((r) => r.id === releaseId);
    const manualApprovalGate = release?.qualityGates?.find(
      (qg) => qg.type === 'ManualApproval' && qg.status === 'Pending'
    );

    if (manualApprovalGate) {
      // Map gate type string to number (assuming ManualApproval = 7 based on QualityGateType enum)
      const gateType = 7; // ManualApproval
      approveMutation.mutate({ releaseId, gateType });
    }
  };

  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle className="text-lg">Pending Approvals</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Loading...</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">Pending Approvals</CardTitle>
          <Badge variant="secondary">{pendingReleases.length}</Badge>
        </div>
      </CardHeader>
      <CardContent>
        {pendingReleases.length === 0 ? (
          <p className="text-sm text-muted-foreground">No pending approvals</p>
        ) : (
          <div className="space-y-2">
            {pendingReleases.map((release) => {
              const daysWaiting = Math.floor(
                (Date.now() - new Date(release.plannedDate).getTime()) /
                  (1000 * 60 * 60 * 24)
              );

              return (
                <div
                  key={release.id}
                  className="flex items-center justify-between p-3 rounded-lg border"
                >
                  <div>
                    <p className="font-medium text-sm">{release.version}</p>
                    <p className="text-xs text-muted-foreground">
                      Waiting for {daysWaiting} day{daysWaiting !== 1 ? 's' : ''}
                    </p>
                  </div>
                  <Button
                    size="sm"
                    onClick={() => handleApprove(release.id)}
                    disabled={approveMutation.isPending}
                  >
                    Approve
                  </Button>
                </div>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

