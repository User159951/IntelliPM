import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { organizationsApi, type UpdateOrganizationRequest } from '@/api/organizations';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Label } from '@/components/ui/label';
import { showSuccess, showError } from '@/lib/sweetalert';
import {
  Loader2,
  ArrowLeft,
  Building2,
  Save,
  Trash2,
  Zap,
  Shield,
} from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

export default function AdminOrganizationDetail() {
  const { orgId } = useParams<{ orgId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState<UpdateOrganizationRequest>({
    name: '',
    code: '',
  });
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const { data: organization, isLoading } = useQuery({
    queryKey: ['admin-organization', orgId],
    queryFn: () => organizationsApi.getById(Number(orgId)),
    enabled: !!orgId,
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateOrganizationRequest) =>
      organizationsApi.update(Number(orgId!), data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-organization', orgId] });
      queryClient.invalidateQueries({ queryKey: ['admin-organizations'] });
      setIsEditing(false);
      showSuccess('Organization updated successfully');
    },
    onError: (error: Error) => {
      showError('Failed to update organization', error.message);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => organizationsApi.delete(Number(orgId!)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-organizations'] });
      navigate('/admin/organizations');
      showSuccess('Organization deleted successfully');
    },
    onError: (error: Error) => {
      showError('Failed to delete organization', error.message);
    },
  });

  useEffect(() => {
    if (organization) {
      setFormData({
        name: organization.name,
        code: organization.code,
      });
    }
  }, [organization]);

  const handleSave = () => {
    if (!formData.name.trim() || !formData.code.trim()) {
      showError('Validation Error', 'Name and Code are required');
      return;
    }
    updateMutation.mutate(formData);
  };

  const handleDelete = () => {
    deleteMutation.mutate();
  };

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
        <Button variant="ghost" size="sm" onClick={() => navigate('/admin/organizations')}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <div>
          <h1 className="text-3xl font-bold">{organization.name}</h1>
          <p className="text-muted-foreground mt-1">Organization Details</p>
        </div>
      </div>

      <div className="border rounded-lg p-6 space-y-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Building2 className="h-8 w-8 text-muted-foreground" />
            <div>
              <h2 className="text-xl font-semibold">Information</h2>
              <p className="text-sm text-muted-foreground">Organization details and settings</p>
            </div>
          </div>
          {!isEditing && (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => navigate(`/admin/organizations/${orgId}/ai-quota`)}>
                <Zap className="h-4 w-4 mr-2" />
                AI Quota
              </Button>
              <Button variant="outline" onClick={() => navigate(`/admin/organizations/${orgId}/permissions`)}>
                <Shield className="h-4 w-4 mr-2" />
                Permissions
              </Button>
              <Button variant="outline" onClick={() => setIsEditing(true)}>
                Edit
              </Button>
              <Button
                variant="destructive"
                onClick={() => setDeleteDialogOpen(true)}
                disabled={organization.userCount > 0}
              >
                <Trash2 className="h-4 w-4 mr-2" />
                Delete
              </Button>
            </div>
          )}
        </div>

        {isEditing ? (
          <div className="space-y-4 pt-4">
            <div>
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="mt-1"
              />
            </div>
            <div>
              <Label htmlFor="code">Code</Label>
              <Input
                id="code"
                value={formData.code}
                onChange={(e) => setFormData({ ...formData, code: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') })}
                className="mt-1"
              />
              <p className="text-xs text-muted-foreground mt-1">
                Lowercase letters, numbers, and hyphens only
              </p>
            </div>
            <div className="flex gap-2 pt-4">
              <Button onClick={handleSave} disabled={updateMutation.isPending}>
                {updateMutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                <Save className="h-4 w-4 mr-2" />
                Save
              </Button>
              <Button variant="outline" onClick={() => {
                setIsEditing(false);
                setFormData({
                  name: organization.name,
                  code: organization.code,
                });
              }}>
                Cancel
              </Button>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-6 pt-4">
            <div>
              <Label className="text-muted-foreground">Name</Label>
              <p className="text-lg font-medium mt-1">{organization.name}</p>
            </div>
            <div>
              <Label className="text-muted-foreground">Code</Label>
              <p className="text-lg font-medium mt-1">{organization.code}</p>
            </div>
            <div>
              <Label className="text-muted-foreground">Members</Label>
              <p className="text-lg font-medium mt-1">{organization.userCount}</p>
            </div>
            <div>
              <Label className="text-muted-foreground">Created</Label>
              <p className="text-lg font-medium mt-1">
                {format(new Date(organization.createdAt), 'MMM dd, yyyy')}
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Delete Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Organization</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete "{organization.name}"? This action cannot be undone.
              {organization.userCount > 0 && (
                <span className="block mt-2 text-destructive">
                  This organization has {organization.userCount} member(s). Please remove or reassign all members first.
                </span>
              )}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={deleteMutation.isPending || organization.userCount > 0}
            >
              {deleteMutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

