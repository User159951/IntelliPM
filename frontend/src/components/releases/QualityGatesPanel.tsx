import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, CheckCircle2, XCircle, AlertTriangle, Clock, CheckCircle } from 'lucide-react';
import { releasesApi } from '@/api/releases';
import { showToast } from '@/lib/sweetalert';
import type { ReleaseDto, QualityGateDto } from '@/types/releases';
import { cn } from '@/lib/utils';
import { format } from 'date-fns';

interface QualityGatesPanelProps {
  release: ReleaseDto;
}

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'Passed':
      return <CheckCircle2 className="h-5 w-5 text-green-500" />;
    case 'Failed':
      return <XCircle className="h-5 w-5 text-red-500" />;
    case 'Warning':
      return <AlertTriangle className="h-5 w-5 text-yellow-500" />;
    default:
      return <Clock className="h-5 w-5 text-gray-400" />;
  }
};

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Passed':
      return 'bg-green-500';
    case 'Failed':
      return 'bg-red-500';
    case 'Warning':
      return 'bg-yellow-500';
    default:
      return 'bg-gray-400';
  }
};

const getGateTypeLabel = (type: string) => {
  const labels: Record<string, string> = {
    CodeCoverage: 'Code Coverage',
    AllTasksCompleted: 'All Tasks Completed',
    NoOpenBugs: 'No Open Bugs',
    CodeReviewApproval: 'Code Review Approval',
    SecurityScan: 'Security Scan',
    PerformanceTests: 'Performance Tests',
    DocumentationComplete: 'Documentation Complete',
    ManualApproval: 'Manual Approval',
  };
  return labels[type] || type;
};

export function QualityGatesPanel({ release }: QualityGatesPanelProps) {
  const queryClient = useQueryClient();

  const evaluateMutation = useMutation({
    mutationFn: () => releasesApi.evaluateQualityGates(release.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['release', release.id] });
      queryClient.invalidateQueries({ queryKey: ['projectReleases', release.projectId] });
      showToast('Quality gates re-evaluated successfully', 'success');
    },
    onError: (error: any) => {
      const message = error?.response?.data?.error || error?.message || 'Failed to evaluate quality gates';
      showToast(message, 'error');
    },
  });

  const approveMutation = useMutation({
    mutationFn: (gateType: number) => releasesApi.approveQualityGate(release.id, gateType),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['release', release.id] });
      queryClient.invalidateQueries({ queryKey: ['projectReleases', release.projectId] });
      showToast('Quality gate approved successfully', 'success');
    },
    onError: (error: any) => {
      const message = error?.response?.data?.error || error?.message || 'Failed to approve quality gate';
      showToast(message, 'error');
    },
  });

  const qualityGates = release.qualityGates || [];
  const overallStatus = release.overallQualityStatus || 'Pending';
  const failedGates = qualityGates.filter((g) => g.status === 'Failed' && g.isRequired);
  const manualApprovalGate = qualityGates.find((g) => g.type === 'ManualApproval');

  const handleApprove = () => {
    if (window.confirm('Are you sure you want to approve this release?')) {
      approveMutation.mutate(7); // ManualApproval = 7
    }
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Quality Gates</CardTitle>
          <Button
            variant="outline"
            size="sm"
            onClick={() => evaluateMutation.mutate()}
            disabled={evaluateMutation.isPending}
          >
            {evaluateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Re-evaluate All Gates
          </Button>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Overall Status */}
        <div className="flex items-center gap-3 p-4 border rounded-lg">
          <div className={cn('h-4 w-4 rounded-full', getStatusColor(overallStatus))} />
          <div className="flex-1">
            <div className="font-semibold">
              {overallStatus === 'Passed'
                ? 'All quality gates passed'
                : overallStatus === 'Failed'
                ? `${failedGates.length} required gate(s) failed`
                : overallStatus === 'Warning'
                ? 'Some quality gates have warnings'
                : 'Quality gates pending evaluation'}
            </div>
            <div className="text-sm text-muted-foreground">
              {qualityGates.length} gate(s) configured
            </div>
          </div>
        </div>

        {/* Warning if gates failed */}
        {failedGates.length > 0 && (
          <Alert variant="destructive">
            <AlertTriangle className="h-4 w-4" />
            <AlertDescription>
              <strong>Required quality gates have not passed:</strong>
              <ul className="mt-2 list-disc list-inside">
                {failedGates.map((gate) => (
                  <li key={gate.id}>{getGateTypeLabel(gate.type)}</li>
                ))}
              </ul>
            </AlertDescription>
          </Alert>
        )}

        {/* Manual Approval Button */}
        {manualApprovalGate?.status === 'Pending' && (
          <Button
            variant="default"
            className="w-full"
            onClick={handleApprove}
            disabled={approveMutation.isPending}
          >
            {approveMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            <CheckCircle className="mr-2 h-4 w-4" />
            Approve Release
          </Button>
        )}

        {/* Quality Gates List */}
        {qualityGates.length > 0 ? (
          <Accordion type="single" collapsible className="w-full">
            {qualityGates.map((gate) => (
              <AccordionItem key={gate.id} value={`gate-${gate.id}`}>
                <AccordionTrigger>
                  <div className="flex items-center gap-3 flex-1">
                    {getStatusIcon(gate.status)}
                    <div className="flex-1 text-left">
                      <div className="font-medium">{getGateTypeLabel(gate.type)}</div>
                      <div className="text-sm text-muted-foreground">{gate.message}</div>
                    </div>
                    <div className="flex items-center gap-2">
                      {gate.isRequired && (
                        <Badge variant="outline" className="text-xs">
                          Required
                        </Badge>
                      )}
                      <Badge
                        variant={
                          gate.status === 'Passed'
                            ? 'default'
                            : gate.status === 'Failed'
                            ? 'destructive'
                            : 'outline'
                        }
                      >
                        {gate.status}
                      </Badge>
                    </div>
                  </div>
                </AccordionTrigger>
                <AccordionContent>
                  <div className="space-y-2 pt-2">
                    {gate.details && (
                      <div className="text-sm text-muted-foreground">{gate.details}</div>
                    )}
                    {gate.threshold !== null && gate.actualValue !== null && (
                      <div className="text-sm">
                        <span className="font-medium">Threshold:</span> {gate.threshold}% |{' '}
                        <span className="font-medium">Actual:</span> {gate.actualValue}%
                      </div>
                    )}
                    {gate.checkedAt && (
                      <div className="text-xs text-muted-foreground">
                        Checked: {format(new Date(gate.checkedAt), 'PPp')}
                        {gate.checkedByName && ` by ${gate.checkedByName}`}
                      </div>
                    )}
                  </div>
                </AccordionContent>
              </AccordionItem>
            ))}
          </Accordion>
        ) : (
          <div className="text-center py-8 text-muted-foreground">
            <p>No quality gates configured yet.</p>
            <Button
              variant="outline"
              size="sm"
              className="mt-4"
              onClick={() => evaluateMutation.mutate()}
              disabled={evaluateMutation.isPending}
            >
              Evaluate Quality Gates
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

