import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminAIQuotasApi, type MemberAIQuotaDto, type UpdateMemberAIQuotaRequest } from '@/api/adminAIQuotas';
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
import { Skeleton } from '@/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { showSuccess, showError } from '@/lib/sweetalert';
import {
  Search,
  Loader2,
  Edit,
  Zap,
} from 'lucide-react';
import { Pagination } from '@/components/ui/pagination';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';

export default function AdminMemberAIQuotas() {
  const queryClient = useQueryClient();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingMember, setEditingMember] = useState<MemberAIQuotaDto | null>(null);
  const [formData, setFormData] = useState<UpdateMemberAIQuotaRequest>({
    monthlyTokenLimitOverride: null,
    monthlyRequestLimitOverride: null,
    isAIEnabledOverride: null,
  });
  const pageSize = 20;

  const { data, isLoading } = useQuery({
    queryKey: ['admin-member-ai-quotas', currentPage, pageSize, searchQuery],
    queryFn: () =>
      adminAIQuotasApi.getMemberAIQuotas({
        page: currentPage,
        pageSize,
        searchTerm: searchQuery || undefined,
      }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ userId, request }: { userId: number; request: UpdateMemberAIQuotaRequest }) =>
      adminAIQuotasApi.updateMemberAIQuota(userId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-member-ai-quotas'] });
      setEditDialogOpen(false);
      setEditingMember(null);
      showSuccess('Member AI quota updated successfully');
    },
    onError: (error: unknown) => {
      // Extract detailed error message from API response
      const apiError = error as { 
        response?: { 
          data?: { 
            message?: string; 
            detail?: string; 
            error?: string;
          }; 
          status?: number;
        }; 
        message?: string;
      };
      
      let errorMessage = 'An unknown error occurred';
      
      // Try to get the most specific error message
      if (apiError.response?.data?.message) {
        errorMessage = apiError.response.data.message;
      } else if (apiError.response?.data?.detail) {
        errorMessage = apiError.response.data.detail;
      } else if (apiError.response?.data?.error) {
        errorMessage = apiError.response.data.error;
      } else if (apiError.message) {
        errorMessage = apiError.message;
      }
      
      // Add context for 404 errors
      if (apiError.response?.status === 404) {
        if (editingMember) {
          errorMessage = `User ${editingMember.fullName} (ID: ${editingMember.userId}) not found. ${errorMessage}`;
        } else {
          errorMessage = `Resource not found. ${errorMessage}`;
        }
      }
      
      console.error('Failed to update member AI quota:', {
        error,
        userId: editingMember?.userId,
        errorMessage,
        status: apiError.response?.status,
      });
      
      showError('Failed to update quota', errorMessage);
    },
  });

  const handleEdit = (member: MemberAIQuotaDto) => {
    setEditingMember(member);
    setFormData({
      monthlyTokenLimitOverride: member.override?.monthlyTokenLimitOverride ?? null,
      monthlyRequestLimitOverride: member.override?.monthlyRequestLimitOverride ?? null,
      isAIEnabledOverride: member.override?.isAIEnabledOverride ?? null,
    });
    setEditDialogOpen(true);
  };

  const handleSave = () => {
    if (!editingMember) return;

    // Validate that overrides don't exceed org limits
    if (
      formData.monthlyTokenLimitOverride !== null &&
      formData.monthlyTokenLimitOverride !== undefined &&
      formData.monthlyTokenLimitOverride > editingMember.organizationQuota.monthlyTokenLimit
    ) {
      showError(
        'Validation Error',
        `Token limit override cannot exceed organization limit (${editingMember.organizationQuota.monthlyTokenLimit})`
      );
      return;
    }

    if (
      formData.monthlyRequestLimitOverride !== null &&
      formData.monthlyRequestLimitOverride !== undefined &&
      editingMember.organizationQuota.monthlyRequestLimit !== null &&
      formData.monthlyRequestLimitOverride > editingMember.organizationQuota.monthlyRequestLimit
    ) {
      showError(
        'Validation Error',
        `Request limit override cannot exceed organization limit (${editingMember.organizationQuota.monthlyRequestLimit})`
      );
      return;
    }

    updateMutation.mutate({
      userId: editingMember.userId,
      request: formData,
    });
  };

  const formatNumber = (num: number | null | undefined): string => {
    if (num === null || num === undefined) return 'N/A';
    return num.toLocaleString();
  };

  if (isLoading && !data) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold flex items-center gap-3">
          <Zap className="h-8 w-8 text-primary" />
          Member AI Quotas
        </h1>
        <p className="text-muted-foreground mt-1">
          Manage AI quota overrides for members in your organization
        </p>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            id="search-member-ai-quotas"
            name="search"
            placeholder="Search by name or email..."
            value={searchQuery}
            onChange={(e) => {
              setSearchQuery(e.target.value);
              setCurrentPage(1);
            }}
            className="pl-10"
          />
        </div>
      </div>

      <div className="border rounded-lg">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Member</TableHead>
              <TableHead>Role</TableHead>
              <TableHead>Token Limit</TableHead>
              <TableHead>Request Limit</TableHead>
              <TableHead>AI Enabled</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center py-8 text-muted-foreground">
                  No members found
                </TableCell>
              </TableRow>
            ) : (
              data?.items.map((member) => (
                <TableRow key={member.userId}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar className="h-8 w-8">
                        <AvatarFallback>
                          {member.firstName[0]}{member.lastName[0]}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <div className="font-medium">{member.fullName}</div>
                        <div className="text-sm text-muted-foreground">{member.email}</div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline">{member.globalRole}</Badge>
                  </TableCell>
                  <TableCell>
                    <div>
                      <div className="font-medium">
                        {formatNumber(member.effectiveQuota.monthlyTokenLimit)}
                      </div>
                      {member.effectiveQuota.hasOverride && (
                        <div className="text-xs text-muted-foreground">
                          Override: {formatNumber(member.override?.monthlyTokenLimitOverride)}
                        </div>
                      )}
                      <div className="text-xs text-muted-foreground">
                        Org: {formatNumber(member.organizationQuota.monthlyTokenLimit)}
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div>
                      <div className="font-medium">
                        {formatNumber(member.effectiveQuota.monthlyRequestLimit)}
                      </div>
                      {member.effectiveQuota.hasOverride && member.override?.monthlyRequestLimitOverride !== null && (
                        <div className="text-xs text-muted-foreground">
                          Override: {formatNumber(member.override?.monthlyRequestLimitOverride)}
                        </div>
                      )}
                      {member.organizationQuota.monthlyRequestLimit !== null && (
                        <div className="text-xs text-muted-foreground">
                          Org: {formatNumber(member.organizationQuota.monthlyRequestLimit)}
                        </div>
                      )}
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant={member.effectiveQuota.isAIEnabled ? 'default' : 'destructive'}>
                      {member.effectiveQuota.isAIEnabled ? 'Enabled' : 'Disabled'}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    {member.effectiveQuota.hasOverride ? (
                      <Badge variant="secondary">Custom</Badge>
                    ) : (
                      <Badge variant="outline">Default</Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleEdit(member)}
                    >
                      <Edit className="h-4 w-4 mr-2" />
                      Edit
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {data && data.totalPages > 1 && (
        <Pagination
          currentPage={currentPage}
          totalPages={data.totalPages}
          onPageChange={setCurrentPage}
        />
      )}

      {/* Edit Dialog */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Edit AI Quota Override</DialogTitle>
            <DialogDescription>
              Set custom quota limits for {editingMember?.fullName}. Leave fields empty to use
              organization defaults.
            </DialogDescription>
          </DialogHeader>
          {editingMember && (
            <div className="space-y-4 py-4">
              <div className="p-4 bg-muted rounded-lg space-y-2">
                <div className="text-sm font-medium">Organization Defaults</div>
                <div className="text-sm text-muted-foreground">
                  Token Limit: {formatNumber(editingMember.organizationQuota.monthlyTokenLimit)}
                </div>
                {editingMember.organizationQuota.monthlyRequestLimit !== null && (
                  <div className="text-sm text-muted-foreground">
                    Request Limit: {formatNumber(editingMember.organizationQuota.monthlyRequestLimit)}
                  </div>
                )}
                <div className="text-sm text-muted-foreground">
                  AI Enabled: {editingMember.organizationQuota.isAIEnabled ? 'Yes' : 'No'}
                </div>
              </div>

              <div>
                <Label htmlFor="tokenLimit">Monthly Token Limit Override</Label>
                <Input
                  id="tokenLimit"
                  name="monthlyTokenLimitOverride"
                  type="number"
                  min="1"
                  max={editingMember.organizationQuota.monthlyTokenLimit}
                  value={formData.monthlyTokenLimitOverride ?? ''}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      monthlyTokenLimitOverride: e.target.value
                        ? parseInt(e.target.value) || null
                        : null,
                    })
                  }
                  className="mt-1"
                  placeholder="Leave empty for org default"
                />
                <p className="text-xs text-muted-foreground mt-1">
                  Max: {formatNumber(editingMember.organizationQuota.monthlyTokenLimit)} (org limit)
                </p>
              </div>

              {editingMember.organizationQuota.monthlyRequestLimit !== null && (
                <div>
                  <Label htmlFor="requestLimit">Monthly Request Limit Override</Label>
                  <Input
                    id="requestLimit"
                    name="monthlyRequestLimitOverride"
                    type="number"
                    min="1"
                    max={editingMember.organizationQuota.monthlyRequestLimit ?? undefined}
                    value={formData.monthlyRequestLimitOverride ?? ''}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        monthlyRequestLimitOverride: e.target.value
                          ? parseInt(e.target.value) || null
                          : null,
                      })
                    }
                    className="mt-1"
                    placeholder="Leave empty for org default"
                  />
                  <p className="text-xs text-muted-foreground mt-1">
                    Max: {formatNumber(editingMember.organizationQuota.monthlyRequestLimit)} (org limit)
                  </p>
                </div>
              )}

              <div className="flex items-center justify-between p-4 border rounded-lg">
                <div>
                  <Label htmlFor="aiEnabled" className="text-base font-medium">
                    AI Enabled Override
                  </Label>
                  <p className="text-sm text-muted-foreground mt-1">
                    Override organization AI enabled setting for this user
                  </p>
                </div>
                <Switch
                  id="aiEnabled"
                  name="isAIEnabledOverride"
                  checked={formData.isAIEnabledOverride ?? editingMember.organizationQuota.isAIEnabled}
                  onCheckedChange={(checked) =>
                    setFormData({ ...formData, isAIEnabledOverride: checked })
                  }
                />
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setEditDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={() => {
                // Clear overrides (set all to null)
                setFormData({
                  monthlyTokenLimitOverride: null,
                  monthlyRequestLimitOverride: null,
                  isAIEnabledOverride: null,
                });
                handleSave();
              }}
              variant="outline"
            >
              Reset to Default
            </Button>
            <Button onClick={handleSave} disabled={updateMutation.isPending}>
              {updateMutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              Save
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

