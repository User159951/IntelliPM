import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { adminAiQuotaApi, type AdminAiQuotaMember, type UpdateMemberQuotaRequest } from '@/api/adminAiQuota';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Skeleton } from '@/components/ui/skeleton';
import { Progress } from '@/components/ui/progress';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { showError, showSuccess } from '@/lib/sweetalert';
import {
  Search,
  Edit,
  RotateCcw,
  Loader2,
  Zap,
} from 'lucide-react';
import { Pagination } from '@/components/ui/pagination';
import { cn } from '@/lib/utils';

export default function AdminAIQuota() {
  const queryClient = useQueryClient();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [editingMember, setEditingMember] = useState<AdminAiQuotaMember | null>(null);
  const [editForm, setEditForm] = useState<UpdateMemberQuotaRequest>({});
  const pageSize = 20;

  const { data, isLoading } = useQuery({
    queryKey: ['admin-ai-quota-members', currentPage, pageSize, searchQuery],
    queryFn: () => adminAiQuotaApi.getMembers(currentPage, pageSize, searchQuery || undefined),
  });

  // Client-side search filter (applied after server-side search)
  const filteredMembers = useMemo(() => {
    if (!data?.items) return [];
    return data.items;
  }, [data?.items]);

  const updateMutation = useMutation({
    mutationFn: ({ userId, request }: { userId: number; request: UpdateMemberQuotaRequest }) =>
      adminAiQuotaApi.updateMemberQuota(userId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-ai-quota-members'] });
      setEditingMember(null);
      showSuccess('Quota updated', 'User AI quota override has been updated successfully.');
    },
    onError: (error: Error) => {
      showError('Failed to update quota', error.message);
    },
  });

  const resetMutation = useMutation({
    mutationFn: (userId: number) => adminAiQuotaApi.resetMemberQuota(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-ai-quota-members'] });
      showSuccess('Quota reset', 'User AI quota override has been reset to organization default.');
    },
    onError: (error: Error) => {
      showError('Failed to reset quota', error.message);
    },
  });

  const handleEditClick = (member: AdminAiQuotaMember) => {
    setEditingMember(member);
    setEditForm({
      maxTokensPerPeriod: member.override?.maxTokensPerPeriod ?? null,
      maxRequestsPerPeriod: member.override?.maxRequestsPerPeriod ?? null,
      maxDecisionsPerPeriod: member.override?.maxDecisionsPerPeriod ?? null,
      maxCostPerPeriod: member.override?.maxCostPerPeriod ?? null,
      reason: member.override?.reason ?? null,
    });
  };

  const handleSave = () => {
    if (!editingMember) return;

    // Convert empty strings to null
    const request: UpdateMemberQuotaRequest = {
      maxTokensPerPeriod: editForm.maxTokensPerPeriod === undefined || editForm.maxTokensPerPeriod === null
        ? null
        : editForm.maxTokensPerPeriod === 0
        ? null
        : editForm.maxTokensPerPeriod,
      maxRequestsPerPeriod: editForm.maxRequestsPerPeriod === undefined || editForm.maxRequestsPerPeriod === null
        ? null
        : editForm.maxRequestsPerPeriod === 0
        ? null
        : editForm.maxRequestsPerPeriod,
      maxDecisionsPerPeriod: editForm.maxDecisionsPerPeriod === undefined || editForm.maxDecisionsPerPeriod === null
        ? null
        : editForm.maxDecisionsPerPeriod === 0
        ? null
        : editForm.maxDecisionsPerPeriod,
      maxCostPerPeriod: editForm.maxCostPerPeriod === undefined || editForm.maxCostPerPeriod === null
        ? null
        : editForm.maxCostPerPeriod === 0
        ? null
        : editForm.maxCostPerPeriod,
      reason: editForm.reason || null,
    };

    updateMutation.mutate({ userId: editingMember.userId, request });
  };

  const handleReset = (member: AdminAiQuotaMember) => {
    if (confirm(`Reset quota override for ${member.fullName}? This will revert to organization default.`)) {
      resetMutation.mutate(member.userId);
    }
  };

  const getPercentageColor = (percentage: number): string => {
    if (percentage >= 80) return 'bg-red-500';
    if (percentage >= 50) return 'bg-yellow-500';
    return 'bg-green-500';
  };

  return (
    <div className="container mx-auto p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">AI Quota Management</h1>
        <p className="text-muted-foreground">
          Manage AI quota limits for organization members. Set per-user overrides or use organization defaults.
        </p>
      </div>

      {/* Search */}
      <div className="mb-4">
        <div className="relative max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            id="search-ai-quota"
            name="search"
            placeholder="Search by name or email..."
            value={searchQuery}
            onChange={(e) => {
              setSearchQuery(e.target.value);
              setCurrentPage(1); // Reset to first page on search
            }}
            className="pl-9"
          />
        </div>
      </div>

      {/* Table */}
      <div className="bg-card rounded-lg border">
        {isLoading ? (
          <div className="p-6 space-y-4">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </div>
        ) : filteredMembers.length === 0 ? (
          <div className="p-12 text-center">
            <p className="text-muted-foreground">
              {searchQuery ? 'No members found matching your search.' : 'No members found.'}
            </p>
          </div>
        ) : (
          <>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Member</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Tokens Usage</TableHead>
                  <TableHead>Requests Usage</TableHead>
                  <TableHead>Decisions Usage</TableHead>
                  <TableHead>Effective Quota</TableHead>
                  <TableHead>Period</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredMembers.map((member: AdminAiQuotaMember) => (
                  <TableRow key={member.userId}>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <Avatar className="h-8 w-8">
                          <AvatarFallback className="bg-primary text-primary-foreground text-xs">
                            {`${member.firstName[0]}${member.lastName[0]}`.toUpperCase()}
                          </AvatarFallback>
                        </Avatar>
                        <div>
                          <div className="font-medium">{member.fullName}</div>
                          <div className="text-sm text-muted-foreground">{member.email}</div>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={member.userRole === 'Admin' ? 'default' : 'secondary'}>
                        {member.userRole}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1 min-w-[120px]">
                        <div className="flex items-center justify-between text-xs">
                          <span className="text-muted-foreground">
                            {member.usage.tokensUsed.toLocaleString()} / {member.effectiveQuota.maxTokensPerPeriod.toLocaleString()}
                          </span>
                          <span className={cn(
                            'text-xs font-medium',
                            member.usage.tokensPercentage >= 80 ? 'text-red-500' :
                            member.usage.tokensPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
                          )}>
                            {member.usage.tokensPercentage.toFixed(0)}%
                          </span>
                        </div>
                        <Progress
                          value={member.usage.tokensPercentage}
                          className={cn('h-2', getPercentageColor(member.usage.tokensPercentage))}
                        />
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1 min-w-[120px]">
                        <div className="flex items-center justify-between text-xs">
                          <span className="text-muted-foreground">
                            {member.usage.requestsUsed.toLocaleString()} / {member.effectiveQuota.maxRequestsPerPeriod.toLocaleString()}
                          </span>
                          <span className={cn(
                            'text-xs font-medium',
                            member.usage.requestsPercentage >= 80 ? 'text-red-500' :
                            member.usage.requestsPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
                          )}>
                            {member.usage.requestsPercentage.toFixed(0)}%
                          </span>
                        </div>
                        <Progress
                          value={member.usage.requestsPercentage}
                          className={cn('h-2', getPercentageColor(member.usage.requestsPercentage))}
                        />
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1 min-w-[120px]">
                        <div className="flex items-center justify-between text-xs">
                          <span className="text-muted-foreground">
                            {member.usage.decisionsMade.toLocaleString()} / {member.effectiveQuota.maxDecisionsPerPeriod.toLocaleString()}
                          </span>
                          <span className={cn(
                            'text-xs font-medium',
                            member.usage.decisionsPercentage >= 80 ? 'text-red-500' :
                            member.usage.decisionsPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
                          )}>
                            {member.usage.decisionsPercentage.toFixed(0)}%
                          </span>
                        </div>
                        <Progress
                          value={member.usage.decisionsPercentage}
                          className={cn('h-2', getPercentageColor(member.usage.decisionsPercentage))}
                        />
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        {member.effectiveQuota.hasOverride ? (
                          <Badge variant="outline" className="text-xs">
                            <Zap className="h-3 w-3 mr-1" />
                            Override
                          </Badge>
                        ) : (
                          <Badge variant="secondary" className="text-xs">
                            Default
                          </Badge>
                        )}
                        <div className="text-xs text-muted-foreground">
                          {member.effectiveQuota.maxTokensPerPeriod.toLocaleString()} tokens
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="text-xs text-muted-foreground">
                        <div>{format(new Date(member.period.periodStartDate), 'MMM d')} - {format(new Date(member.period.periodEndDate), 'MMM d')}</div>
                        <div>{member.period.daysRemaining} days left</div>
                      </div>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditClick(member)}
                          title="Edit quota"
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        {member.effectiveQuota.hasOverride && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleReset(member)}
                            disabled={resetMutation.isPending}
                            title="Reset to default"
                          >
                            {resetMutation.isPending ? (
                              <Loader2 className="h-4 w-4 animate-spin" />
                            ) : (
                              <RotateCcw className="h-4 w-4" />
                            )}
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {data && (data.totalPages ?? 0) > 1 && (
              <div className="p-4 border-t">
                <Pagination
                  currentPage={currentPage}
                  totalPages={data.totalPages ?? 1}
                  onPageChange={setCurrentPage}
                />
              </div>
            )}
          </>
        )}
      </div>

      {/* Edit Dialog */}
      {editingMember && (
        <Dialog open={!!editingMember} onOpenChange={(open) => !open && setEditingMember(null)}>
          <DialogContent className="sm:max-w-[600px]">
            <DialogHeader>
              <DialogTitle>Edit AI Quota for {editingMember.fullName}</DialogTitle>
              <DialogDescription>
                Leave fields empty to use organization default. Set to 0 to remove override for that field.
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">

              <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="maxTokensPerPeriod" className="text-right">
                  Max Tokens
                </Label>
                <Input
                  id="maxTokensPerPeriod"
                  name="maxTokensPerPeriod"
                  type="number"
                  value={editForm.maxTokensPerPeriod ?? ''}
                  onChange={(e) =>
                    setEditForm({
                      ...editForm,
                      maxTokensPerPeriod: e.target.value ? parseInt(e.target.value) : null,
                    })
                  }
                  className="col-span-3"
                  placeholder="Organization default"
                />
              </div>

              <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="maxRequestsPerPeriod" className="text-right">
                  Max Requests
                </Label>
                <Input
                  id="maxRequestsPerPeriod"
                  name="maxRequestsPerPeriod"
                  type="number"
                  value={editForm.maxRequestsPerPeriod ?? ''}
                  onChange={(e) =>
                    setEditForm({
                      ...editForm,
                      maxRequestsPerPeriod: e.target.value ? parseInt(e.target.value) : null,
                    })
                  }
                  className="col-span-3"
                  placeholder="Organization default"
                />
              </div>

              <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="maxDecisionsPerPeriod" className="text-right">
                  Max Decisions
                </Label>
                <Input
                  id="maxDecisionsPerPeriod"
                  name="maxDecisionsPerPeriod"
                  type="number"
                  value={editForm.maxDecisionsPerPeriod ?? ''}
                  onChange={(e) =>
                    setEditForm({
                      ...editForm,
                      maxDecisionsPerPeriod: e.target.value ? parseInt(e.target.value) : null,
                    })
                  }
                  className="col-span-3"
                  placeholder="Organization default"
                />
              </div>

              <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="maxCostPerPeriod" className="text-right">
                  Max Cost ($)
                </Label>
                <Input
                  id="maxCostPerPeriod"
                  name="maxCostPerPeriod"
                  type="number"
                  step="0.01"
                  value={editForm.maxCostPerPeriod ?? ''}
                  onChange={(e) =>
                    setEditForm({
                      ...editForm,
                      maxCostPerPeriod: e.target.value ? parseFloat(e.target.value) : null,
                    })
                  }
                  className="col-span-3"
                  placeholder="Organization default"
                />
              </div>

              <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="reason" className="text-right">
                  Reason
                </Label>
                <Textarea
                  id="reason"
                  name="reason"
                  value={editForm.reason ?? ''}
                  onChange={(e) => setEditForm({ ...editForm, reason: e.target.value })}
                  className="col-span-3"
                  placeholder="Optional: reason for this override"
                  rows={3}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setEditingMember(null)}>
                Cancel
              </Button>
              <Button onClick={handleSave} disabled={updateMutation.isPending}>
                {updateMutation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  'Save Changes'
                )}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}

