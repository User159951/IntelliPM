import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { superAdminAIQuotaApi, type UpdateOrganizationAIQuotaRequest } from '@/api/superAdminAIQuota';
import { organizationsApi } from '@/api/organizations';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { showSuccess, showError } from '@/lib/sweetalert';
import {
  Loader2,
  ArrowLeft,
  Save,
  Zap,
} from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export default function SuperAdminOrganizationAIQuota() {
  const { orgId } = useParams<{ orgId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<UpdateOrganizationAIQuotaRequest>({
    monthlyTokenLimit: 100000,
    monthlyRequestLimit: null,
    resetDayOfMonth: null,
    isAIEnabled: true,
  });

  const { data: organization, isLoading: orgLoading } = useQuery({
    queryKey: ['admin-organization', orgId],
    queryFn: () => organizationsApi.getById(Number(orgId)),
    enabled: !!orgId,
  });

  const { data: quota, isLoading: quotaLoading } = useQuery({
    queryKey: ['superadmin-org-ai-quota', orgId],
    queryFn: () => superAdminAIQuotaApi.getOrganizationAIQuota(Number(orgId!)),
    enabled: !!orgId,
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateOrganizationAIQuotaRequest) =>
      superAdminAIQuotaApi.upsertOrganizationAIQuota(Number(orgId!), data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['superadmin-org-ai-quota', orgId] });
      showSuccess('Organization AI quota updated successfully');
    },
    onError: (error: Error) => {
      showError('Failed to update organization AI quota', error.message);
    },
  });

  useEffect(() => {
    if (quota) {
      setFormData({
        monthlyTokenLimit: quota.monthlyTokenLimit,
        monthlyRequestLimit: quota.monthlyRequestLimit ?? null,
        resetDayOfMonth: quota.resetDayOfMonth ?? null,
        isAIEnabled: quota.isAIEnabled,
      });
    }
  }, [quota]);

  const handleSave = () => {
    if (formData.monthlyTokenLimit <= 0) {
      showError('Validation Error', 'Monthly token limit must be greater than 0');
      return;
    }
    if (formData.monthlyRequestLimit !== null && formData.monthlyRequestLimit !== undefined && formData.monthlyRequestLimit <= 0) {
      showError('Validation Error', 'Monthly request limit must be greater than 0 if provided');
      return;
    }
    if (formData.resetDayOfMonth !== null && formData.resetDayOfMonth !== undefined && (formData.resetDayOfMonth < 1 || formData.resetDayOfMonth > 31)) {
      showError('Validation Error', 'Reset day of month must be between 1 and 31');
      return;
    }
    updateMutation.mutate(formData);
  };

  const isLoading = orgLoading || quotaLoading;

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (!organization) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Organization not found</p>
        <Button onClick={() => navigate('/admin/organizations')} className="mt-4">
          Back to Organizations
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(`/admin/organizations/${orgId}`)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <div>
          <h1 className="text-3xl font-bold">AI Quota Settings</h1>
          <p className="text-muted-foreground mt-1">{organization.name}</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-3">
            <Zap className="h-6 w-6 text-primary" />
            <div>
              <CardTitle>Organization AI Quota</CardTitle>
              <CardDescription>
                Configure AI quota limits and settings for this organization
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="space-y-4">
            <div>
              <Label htmlFor="monthlyTokenLimit">Monthly Token Limit *</Label>
              <Input
                id="monthlyTokenLimit"
                type="number"
                min="1"
                value={formData.monthlyTokenLimit}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    monthlyTokenLimit: parseInt(e.target.value) || 0,
                  })
                }
                className="mt-1"
              />
              <p className="text-xs text-muted-foreground mt-1">
                Maximum number of tokens allowed per month for this organization
              </p>
            </div>

            <div>
              <Label htmlFor="monthlyRequestLimit">Monthly Request Limit (Optional)</Label>
              <Input
                id="monthlyRequestLimit"
                type="number"
                min="1"
                value={formData.monthlyRequestLimit ?? ''}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    monthlyRequestLimit: e.target.value ? parseInt(e.target.value) || null : null,
                  })
                }
                className="mt-1"
                placeholder="Leave empty for unlimited"
              />
              <p className="text-xs text-muted-foreground mt-1">
                Maximum number of AI requests allowed per month (optional)
              </p>
            </div>

            <div>
              <Label htmlFor="resetDayOfMonth">Reset Day of Month (Optional)</Label>
              <Input
                id="resetDayOfMonth"
                type="number"
                min="1"
                max="31"
                value={formData.resetDayOfMonth ?? ''}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    resetDayOfMonth: e.target.value ? parseInt(e.target.value) || null : null,
                  })
                }
                className="mt-1"
                placeholder="Leave empty for first day of month"
              />
              <p className="text-xs text-muted-foreground mt-1">
                Day of the month when quota resets (1-31). Defaults to 1st if not specified.
              </p>
            </div>

            <div className="flex items-center justify-between p-4 border rounded-lg">
              <div>
                <Label htmlFor="isAIEnabled" className="text-base font-medium">
                  AI Features Enabled
                </Label>
                <p className="text-sm text-muted-foreground mt-1">
                  Enable or disable AI features for this organization
                </p>
              </div>
              <Switch
                id="isAIEnabled"
                checked={formData.isAIEnabled ?? true}
                onCheckedChange={(checked) =>
                  setFormData({ ...formData, isAIEnabled: checked })
                }
              />
            </div>
          </div>

          <div className="flex gap-2 pt-4 border-t">
            <Button onClick={handleSave} disabled={updateMutation.isPending}>
              {updateMutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              <Save className="h-4 w-4 mr-2" />
              Save Changes
            </Button>
            <Button
              variant="outline"
              onClick={() => {
                if (quota) {
                  setFormData({
                    monthlyTokenLimit: quota.monthlyTokenLimit,
                    monthlyRequestLimit: quota.monthlyRequestLimit ?? null,
                    resetDayOfMonth: quota.resetDayOfMonth ?? null,
                    isAIEnabled: quota.isAIEnabled,
                  });
                }
              }}
            >
              Reset
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

