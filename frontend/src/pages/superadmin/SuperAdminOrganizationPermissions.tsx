import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { organizationPermissionPolicyApi, type UpdateOrganizationPermissionPolicyRequest } from '@/api/organizationPermissionPolicy';
import { organizationsApi } from '@/api/organizations';
import { permissionsApi } from '@/api/permissions';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Checkbox } from '@/components/ui/checkbox';
import { showSuccess, showError } from '@/lib/sweetalert';
import { Loader2, ArrowLeft, Save, Shield, CheckSquare } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';

export default function SuperAdminOrganizationPermissions() {
  const { orgId } = useParams<{ orgId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const organizationId = Number(orgId);

  const [selectedPermissions, setSelectedPermissions] = useState<Set<string>>(new Set());
  const [isActive, setIsActive] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  const { data: organization, isLoading: isLoadingOrg } = useQuery({
    queryKey: ['admin-organization', organizationId],
    queryFn: () => organizationsApi.getById(organizationId),
    enabled: !!organizationId,
  });

  const { data: policy, isLoading: isLoadingPolicy } = useQuery({
    queryKey: ['organization-permission-policy', organizationId],
    queryFn: () => organizationPermissionPolicyApi.getOrganizationPermissionPolicy(organizationId),
    enabled: !!organizationId,
  });

  const { data: permissionsMatrix, isLoading: isLoadingPermissions } = useQuery({
    queryKey: ['permissions-matrix'],
    queryFn: () => permissionsApi.getMatrix(),
  });

  const upsertMutation = useMutation({
    mutationFn: (data: UpdateOrganizationPermissionPolicyRequest) =>
      organizationPermissionPolicyApi.upsertOrganizationPermissionPolicy(organizationId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organization-permission-policy', organizationId] });
      showSuccess('Permission policy updated successfully');
    },
    onError: (error: Error) => {
      showError('Failed to update permission policy', error.message);
    },
  });

  useEffect(() => {
    if (policy) {
      setSelectedPermissions(new Set(policy.allowedPermissions));
      setIsActive(policy.isActive);
    }
  }, [policy]);

  const handleSave = () => {
    upsertMutation.mutate({
      allowedPermissions: Array.from(selectedPermissions),
      isActive,
    });
  };

  const handleTogglePermission = (permissionName: string) => {
    const newSelected = new Set(selectedPermissions);
    if (newSelected.has(permissionName)) {
      newSelected.delete(permissionName);
    } else {
      newSelected.add(permissionName);
    }
    setSelectedPermissions(newSelected);
  };

  const handleSelectAll = () => {
    if (permissionsMatrix) {
      const allPermissions = permissionsMatrix.permissions.map((p) => p.name);
      setSelectedPermissions(new Set(allPermissions));
    }
  };

  const handleDeselectAll = () => {
    setSelectedPermissions(new Set());
  };

  const filteredPermissions = permissionsMatrix?.permissions.filter((p) =>
    p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.category.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  const permissionsByCategory = filteredPermissions.reduce((acc, perm) => {
    if (!acc[perm.category]) {
      acc[perm.category] = [];
    }
    acc[perm.category].push(perm);
    return acc;
  }, {} as Record<string, typeof filteredPermissions>);

  const isLoading = isLoadingOrg || isLoadingPolicy || isLoadingPermissions;
  const isSaving = upsertMutation.isPending;

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
        <Button variant="ghost" size="sm" onClick={() => navigate(`/admin/organizations/${organizationId}`)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Organization
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Permission Policy for {organization.name}</h1>
          <p className="text-muted-foreground mt-1">Manage allowed permissions for this organization</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" /> Permission Policy Settings
          </CardTitle>
          <CardDescription>
            Select which permissions are allowed for members of this organization. Members can only be assigned permissions that are checked here.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex items-center space-x-2">
            <Switch
              id="isActive"
              checked={isActive}
              onCheckedChange={setIsActive}
            />
            <Label htmlFor="isActive">Policy Active</Label>
            <p className="text-sm text-muted-foreground ml-4">
              {isActive
                ? 'Policy is active. Only selected permissions are allowed.'
                : 'Policy is inactive. All permissions are allowed (default behavior).'}
            </p>
          </div>

          {isActive && (
            <>
              <div className="flex items-center gap-4">
                <Input
                  placeholder="Search permissions..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="max-w-sm"
                />
                <Button variant="outline" size="sm" onClick={handleSelectAll}>
                  Select All
                </Button>
                <Button variant="outline" size="sm" onClick={handleDeselectAll}>
                  Deselect All
                </Button>
                <div className="text-sm text-muted-foreground">
                  {selectedPermissions.size} of {permissionsMatrix?.permissions.length || 0} selected
                </div>
              </div>

              <ScrollArea className="h-[600px] border rounded-md p-4">
                <div className="space-y-6">
                  {Object.entries(permissionsByCategory).map(([category, perms]) => (
                    <div key={category} className="space-y-2">
                      <h3 className="font-semibold text-lg">{category}</h3>
                      <Separator />
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3 pl-4">
                        {perms.map((perm) => (
                          <div key={perm.id} className="flex items-center space-x-2">
                            <Checkbox
                              id={`perm-${perm.id}`}
                              checked={selectedPermissions.has(perm.name)}
                              onCheckedChange={() => handleTogglePermission(perm.name)}
                            />
                            <Label
                              htmlFor={`perm-${perm.id}`}
                              className="text-sm font-normal cursor-pointer flex-1"
                            >
                              <div className="font-medium">{perm.name}</div>
                              {perm.description && (
                                <div className="text-xs text-muted-foreground">{perm.description}</div>
                              )}
                            </Label>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </ScrollArea>
            </>
          )}

          <div className="flex justify-end gap-2 pt-4">
            <Button onClick={handleSave} disabled={isSaving}>
              {isSaving && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              <Save className="h-4 w-4 mr-2" />
              Save Policy
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CheckSquare className="h-5 w-5" /> Policy Information
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label className="text-muted-foreground">Last Updated</Label>
              <p className="text-lg font-medium mt-1">
                {policy?.updatedAt ? format(new Date(policy.updatedAt), 'MMM dd, yyyy HH:mm') : 'Never'}
              </p>
            </div>
            <div>
              <Label className="text-muted-foreground">Created At</Label>
              <p className="text-lg font-medium mt-1">
                {policy?.createdAt ? format(new Date(policy.createdAt), 'MMM dd, yyyy HH:mm') : 'N/A'}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

