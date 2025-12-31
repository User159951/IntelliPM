import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, AlertTriangle, Rocket } from 'lucide-react';
import { releasesApi } from '@/api/releases';
import { showToast } from '@/lib/sweetalert';
import type { ReleaseDto } from '@/types/releases';

interface DeployReleaseDialogProps {
  release: ReleaseDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

export function DeployReleaseDialog({
  release,
  open,
  onOpenChange,
  onSuccess,
}: DeployReleaseDialogProps) {
  const queryClient = useQueryClient();
  const [checklist, setChecklist] = useState({
    testsPassed: false,
    documentationUpdated: false,
    stakeholdersNotified: false,
    backupCreated: false,
  });
  const [deploymentNotes, setDeploymentNotes] = useState('');
  const [confirmed, setConfirmed] = useState(false);

  const failedGates =
    release.qualityGates?.filter((g) => g.status === 'Failed' && g.isRequired) || [];
  const canDeploy = failedGates.length === 0 && confirmed;

  const mutation = useMutation({
    mutationFn: () => releasesApi.deployRelease(release.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['release', release.id] });
      queryClient.invalidateQueries({ queryKey: ['projectReleases', release.projectId] });
      queryClient.invalidateQueries({ queryKey: ['releaseStatistics', release.projectId] });
      showToast(`Release ${release.version} deployed successfully 🚀`, 'success');
      onSuccess?.();
      onOpenChange(false);
      // Reset form
      setChecklist({
        testsPassed: false,
        documentationUpdated: false,
        stakeholdersNotified: false,
        backupCreated: false,
      });
      setDeploymentNotes('');
      setConfirmed(false);
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { error?: string } }; message?: string };
      const message = apiError?.response?.data?.error || apiError?.message || 'Failed to deploy release';
      showToast(message, 'error');
    },
  });

  const toggleChecklist = (key: keyof typeof checklist) => {
    setChecklist((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Rocket className="h-5 w-5" />
            Deploy Release {release.version}
          </DialogTitle>
          <DialogDescription>
            Confirm deployment readiness and deploy this release to production.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Quality Gates Summary */}
          <div>
            <h4 className="font-semibold mb-2">Quality Gates Status</h4>
            {failedGates.length > 0 ? (
              <Alert variant="destructive">
                <AlertTriangle className="h-4 w-4" />
                <AlertDescription>
                  <strong>⚠️ Required quality gates have not passed:</strong>
                  <ul className="mt-2 list-disc list-inside">
                    {failedGates.map((gate) => (
                      <li key={gate.id}>{gate.type}</li>
                    ))}
                  </ul>
                  <p className="mt-2">Please resolve these issues before deploying.</p>
                </AlertDescription>
              </Alert>
            ) : (
              <Alert>
                <AlertDescription>
                  ✅ All required quality gates have passed.
                </AlertDescription>
              </Alert>
            )}
          </div>

          {/* Deployment Checklist */}
          <div>
            <h4 className="font-semibold mb-3">Pre-Deployment Checklist</h4>
            <div className="space-y-3">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="tests"
                  checked={checklist.testsPassed}
                  onCheckedChange={() => toggleChecklist('testsPassed')}
                />
                <Label htmlFor="tests" className="cursor-pointer">
                  All tests passed
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="docs"
                  checked={checklist.documentationUpdated}
                  onCheckedChange={() => toggleChecklist('documentationUpdated')}
                />
                <Label htmlFor="docs" className="cursor-pointer">
                  Documentation updated
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="stakeholders"
                  checked={checklist.stakeholdersNotified}
                  onCheckedChange={() => toggleChecklist('stakeholdersNotified')}
                />
                <Label htmlFor="stakeholders" className="cursor-pointer">
                  Stakeholders notified
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="backup"
                  checked={checklist.backupCreated}
                  onCheckedChange={() => toggleChecklist('backupCreated')}
                />
                <Label htmlFor="backup" className="cursor-pointer">
                  Backup created
                </Label>
              </div>
            </div>
          </div>

          {/* Deployment Notes */}
          <div>
            <Label htmlFor="notes">Deployment Notes (Optional)</Label>
            <Textarea
              id="notes"
              placeholder="Add any notes about this deployment..."
              value={deploymentNotes}
              onChange={(e) => setDeploymentNotes(e.target.value)}
              className="mt-2 min-h-[100px]"
            />
          </div>

          {/* Final Confirmation */}
          <div className="flex items-center space-x-2 p-4 border rounded-lg">
            <Checkbox
              id="confirm"
              checked={confirmed}
              onCheckedChange={(checked) => setConfirmed(checked === true)}
            />
            <Label htmlFor="confirm" className="cursor-pointer">
              I confirm this release is ready for deployment
            </Label>
          </div>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={mutation.isPending}
          >
            Cancel
          </Button>
          <Button
            type="button"
            onClick={() => mutation.mutate()}
            disabled={!canDeploy || mutation.isPending}
          >
            {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            <Rocket className="mr-2 h-4 w-4" />
            Deploy Release
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

