import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { aiGovernanceApi } from '@/api/aiGovernance';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Gauge, MoreVertical, Edit, Power } from 'lucide-react';
import { showToast, showError } from '@/lib/sweetalert';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';

function EmptyState({ icon: Icon, message }: { icon: React.ComponentType<{ className?: string }>; message: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <Icon className="h-12 w-12 text-muted-foreground mb-4" />
      <h3 className="text-lg font-semibold">{message}</h3>
    </div>
  );
}

function QuotaGauge({ quota, compact = false }: { quota: AIQuota; compact?: boolean }) {
  const tokensPercentage = quota.usage.tokensPercentage;
  const requestsPercentage = quota.usage.requestsPercentage;
  const costPercentage = quota.usage.costPercentage;

  return (
    <div className="space-y-3">
      <div>
        <div className="flex items-center justify-between mb-1">
          <span className="text-sm font-medium">Tokens</span>
          <span className="text-sm text-muted-foreground">
            {quota.usage.tokensUsed.toLocaleString()} / {quota.usage.tokensLimit.toLocaleString()}
          </span>
        </div>
        <div className="w-full bg-secondary rounded-full h-2">
          <div
            className="bg-primary h-2 rounded-full transition-all"
            style={{ width: `${Math.min(tokensPercentage, 100)}%` }}
          />
        </div>
        <span className="text-xs text-muted-foreground">
          {tokensPercentage.toFixed(1)}% used
        </span>
      </div>

      <div>
        <div className="flex items-center justify-between mb-1">
          <span className="text-sm font-medium">Requests</span>
          <span className="text-sm text-muted-foreground">
            {quota.usage.requestsUsed} / {quota.usage.requestsLimit}
          </span>
        </div>
        <div className="w-full bg-secondary rounded-full h-2">
          <div
            className="bg-primary h-2 rounded-full transition-all"
            style={{ width: `${Math.min(requestsPercentage, 100)}%` }}
          />
        </div>
        <span className="text-xs text-muted-foreground">
          {requestsPercentage.toFixed(1)}% used
        </span>
      </div>

      {!compact && (
        <div>
          <div className="flex items-center justify-between mb-1">
            <span className="text-sm font-medium">Cost</span>
            <span className="text-sm text-muted-foreground">
              ${quota.usage.costAccumulated.toFixed(2)} / ${quota.usage.costLimit.toFixed(2)}
            </span>
          </div>
          <div className="w-full bg-secondary rounded-full h-2">
            <div
              className="bg-primary h-2 rounded-full transition-all"
              style={{ width: `${Math.min(costPercentage, 100)}%` }}
            />
          </div>
          <span className="text-xs text-muted-foreground">
            {costPercentage.toFixed(1)}% used
          </span>
        </div>
      )}
    </div>
  );
}

export function AIQuotasList() {
  const [page, setPage] = useState(1);
  const [selectedOrg, setSelectedOrg] = useState<number | null>(null);
  const [disableDialogOpen, setDisableDialogOpen] = useState(false);
  const [disableReason, setDisableReason] = useState('');
  const [orgToDisable, setOrgToDisable] = useState<{ id: number; name: string } | null>(null);

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['ai-quotas-admin', page],
    queryFn: () =>
      aiGovernanceApi.getAllQuotas({
        page,
        pageSize: 20,
        isActive: true,
      }),
    staleTime: 1000 * 30,
  });

  const disableAIMutation = useMutation({
    mutationFn: ({ orgId, reason }: { orgId: number; reason: string }) =>
      aiGovernanceApi.disableAI(orgId, reason, true, false),
    onSuccess: () => {
      showToast('AI disabled successfully', 'success');
      setDisableDialogOpen(false);
      setDisableReason('');
      setOrgToDisable(null);
      refetch();
    },
    onError: (error: unknown) => {
      const apiError = error as { message?: string };
      showError('Failed to disable AI', apiError.message);
    },
  });

  const handleDisableAI = (orgId: number, orgName: string) => {
    setOrgToDisable({ id: orgId, name: orgName });
    setDisableDialogOpen(true);
  };

  const confirmDisable = () => {
    if (!orgToDisable || !disableReason.trim()) {
      showError('Validation Error', 'Please provide a reason for disabling AI');
      return;
    }
    disableAIMutation.mutate({ orgId: orgToDisable.id, reason: disableReason });
  };

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>Organization AI Quotas</CardTitle>
          <CardDescription>Manage AI quotas and usage limits</CardDescription>
        </CardHeader>

        <CardContent>
          {isLoading ? (
            <div className="space-y-4">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-32" />
              ))}
            </div>
          ) : data && data.items.length > 0 ? (
            <div className="space-y-4">
              {data.items.map((quota) => (
                <Card key={quota.id} className="overflow-hidden">
                  <CardHeader className="pb-3">
                    <div className="flex items-start justify-between">
                      <div>
                        <CardTitle className="text-base">{quota.organizationName}</CardTitle>
                        <div className="flex items-center gap-2 mt-1">
                          <Badge variant={quota.isActive ? 'default' : 'secondary'}>
                            {quota.tierName}
                          </Badge>
                          {quota.isExceeded && (
                            <Badge variant="destructive">Quota Exceeded</Badge>
                          )}
                        </div>
                      </div>

                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm">
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onClick={() => setSelectedOrg(quota.organizationId)}>
                            <Edit className="h-4 w-4 mr-2" />
                            Edit Quota
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() =>
                              handleDisableAI(quota.organizationId, quota.organizationName)
                            }
                            className="text-destructive"
                          >
                            <Power className="h-4 w-4 mr-2" />
                            Disable AI
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  </CardHeader>

                  <CardContent className="pt-0">
                    <QuotaGauge quota={quota} compact />
                  </CardContent>
                </Card>
              ))}

              {/* Pagination */}
              <div className="flex items-center justify-center gap-2 pt-4">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <span className="text-sm">
                  Page {page} of {data.totalPages}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                  disabled={page === data.totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          ) : (
            <EmptyState icon={Gauge} message="No quotas found" />
          )}
        </CardContent>
      </Card>

      {/* Disable AI Dialog */}
      <Dialog open={disableDialogOpen} onOpenChange={setDisableDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Disable AI for {orgToDisable?.name}</DialogTitle>
            <DialogDescription>
              This will immediately disable all AI features for this organization. Please provide a
              reason for this action.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div>
              <Label htmlFor="reason">Reason *</Label>
              <Input
                id="reason"
                value={disableReason}
                onChange={(e) => setDisableReason(e.target.value)}
                placeholder="e.g., Abuse detected, billing issues..."
                className="mt-1"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDisableDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={confirmDisable}
              disabled={!disableReason.trim() || disableAIMutation.isPending}
            >
              {disableAIMutation.isPending ? 'Disabling...' : 'Disable AI'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

