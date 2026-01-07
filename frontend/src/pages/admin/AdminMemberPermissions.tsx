import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { memberPermissionsApi, type MemberPermissionDto, type UpdateMemberPermissionRequest } from '@/api/memberPermissions';
import { organizationPermissionPolicyApi } from '@/api/organizationPermissionPolicy';
import { permissionsApi } from '@/api/permissions';
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { ScrollArea } from '@/components/ui/scroll-area';
import { showSuccess, showError } from '@/lib/sweetalert';
import { Search, Pencil, Loader2, UsersRound } from 'lucide-react';
import { Pagination } from '@/components/ui/pagination';
import { cn } from '@/lib/utils';

export default function AdminMemberPermissions() {
  const queryClient = useQueryClient();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedMember, setSelectedMember] = useState<MemberPermissionDto | null>(null);
  const [formData, setFormData] = useState<UpdateMemberPermissionRequest>({
    globalRole: null,
    permissionIds: null,
  });
  const pageSize = 10;

  const { data, isLoading, error } = useQuery({
    queryKey: ['admin-member-permissions', currentPage, pageSize, searchQuery],
    queryFn: () =>
      memberPermissionsApi.getMemberPermissions({
        page: currentPage,
        pageSize,
        searchTerm: searchQuery || undefined,
      }),
  });

  // Get organization permission policy to know which permissions are allowed
  // Admin users can now access their own organization's permission policy via the admin endpoint
  const { data: orgPolicy } = useQuery({
    queryKey: ['my-organization-permission-policy'],
    queryFn: () => organizationPermissionPolicyApi.getMyOrganizationPermissionPolicy(),
  });

  // Get permissions matrix to show all available permissions
  const { data: permissionsMatrix } = useQuery({
    queryKey: ['permissions-matrix'],
    queryFn: () => permissionsApi.getMatrix(),
  });

  const updateMutation = useMutation({
    mutationFn: ({ userId, data }: { userId: number; data: UpdateMemberPermissionRequest }) =>
      memberPermissionsApi.updateMemberPermission(userId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-member-permissions'] });
      setEditDialogOpen(false);
      showSuccess('Member permissions updated successfully');
    },
    onError: (err: Error) => {
      showError('Failed to update member permissions', err.message);
    },
  });

  useEffect(() => {
    if (selectedMember) {
      setFormData({
        globalRole: selectedMember.globalRole,
        permissionIds: selectedMember.permissionIds,
      });
    }
  }, [selectedMember]);

  const handleEditClick = (member: MemberPermissionDto) => {
    setSelectedMember(member);
    setEditDialogOpen(true);
  };

  const handleSave = () => {
    if (selectedMember) {
      updateMutation.mutate({ userId: selectedMember.userId, data: formData });
    }
  };

  // Get allowed permissions based on org policy
  const getAllowedPermissions = (): Set<string> => {
    if (!orgPolicy || !orgPolicy.isActive || orgPolicy.allowedPermissions.length === 0) {
      // No policy or inactive = all permissions allowed
      return new Set(permissionsMatrix?.permissions.map((p) => p.name) || []);
    }
    return new Set(orgPolicy.allowedPermissions);
  };

  const allowedPermissions = getAllowedPermissions();

  // Filter permissions by role and policy
  const getAvailablePermissionsForRole = (role: string) => {
    if (!permissionsMatrix) return [];
    
    const rolePermissions = permissionsMatrix.rolePermissions[role as keyof typeof permissionsMatrix.rolePermissions] || [];
    const availablePerms = permissionsMatrix.permissions.filter(
      (p) => rolePermissions.includes(p.id) && allowedPermissions.has(p.name)
    );
    return availablePerms;
  };

  const handleRoleChange = (newRole: string) => {
    setFormData({ ...formData, globalRole: newRole });
    
    // When role changes, update permissions to match the role
    if (permissionsMatrix) {
      const rolePerms = permissionsMatrix.rolePermissions[newRole as keyof typeof permissionsMatrix.rolePermissions] || [];
      const availablePermIds = permissionsMatrix.permissions
        .filter((p) => rolePerms.includes(p.id) && allowedPermissions.has(p.name))
        .map((p) => p.id);
      setFormData({ ...formData, globalRole: newRole, permissionIds: availablePermIds });
    }
  };

  const handleTogglePermission = (permissionId: number) => {
    const currentIds = formData.permissionIds || [];
    const newIds = currentIds.includes(permissionId)
      ? currentIds.filter((id) => id !== permissionId)
      : [...currentIds, permissionId];
    setFormData({ ...formData, permissionIds: newIds });
  };

  if (isLoading && !data) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12 text-destructive">
        <p>Error loading member permissions: {error.message}</p>
      </div>
    );
  }

  const availablePermissions = selectedMember
    ? getAvailablePermissionsForRole(formData.globalRole || selectedMember.globalRole)
    : [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold flex items-center gap-2">
          <UsersRound className="h-7 w-7" /> Member Permissions
        </h1>
        <p className="text-muted-foreground mt-1">
          Manage roles and permissions for organization members
        </p>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            id="search-member-permissions"
            name="search"
            placeholder="Search members..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
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
              <TableHead>Permissions</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data?.items?.map((member) => (
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
                  <Badge variant={member.globalRole === 'Admin' ? 'default' : 'secondary'}>
                    {member.globalRole}
                  </Badge>
                </TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {member.permissions.slice(0, 3).map((perm) => (
                      <Badge key={perm} variant="outline" className="text-xs">
                        {perm}
                      </Badge>
                    ))}
                    {member.permissions.length > 3 && (
                      <Badge variant="outline" className="text-xs">
                        +{member.permissions.length - 3} more
                      </Badge>
                    )}
                  </div>
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleEditClick(member)}
                  >
                    <Pencil className="h-4 w-4 mr-2" />
                    Edit
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {data && (data.totalPages ?? 0) > 1 && (
        <Pagination
          currentPage={currentPage}
          totalPages={data.totalPages ?? 1}
          onPageChange={setCurrentPage}
        />
      )}

      {/* Edit Dialog */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh]">
          <DialogHeader>
            <DialogTitle>Edit Member Permissions</DialogTitle>
            <DialogDescription>
              Update role and permissions for {selectedMember?.fullName}. Only permissions allowed by your organization policy are available.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-6 py-4">
            <div className="space-y-2">
              <Label htmlFor="role">Global Role</Label>
              <Select
                value={formData.globalRole || selectedMember?.globalRole || 'User'}
                onValueChange={handleRoleChange}
              >
                <SelectTrigger id="role" name="globalRole">
                  <SelectValue placeholder="Select role" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="User">User</SelectItem>
                  <SelectItem value="Admin">Admin</SelectItem>
                </SelectContent>
              </Select>
              <p className="text-sm text-muted-foreground">
                Permissions will be derived from the selected role, filtered by organization policy.
              </p>
            </div>

            <div className="space-y-2">
              <Label>Permissions</Label>
              <ScrollArea className="h-[400px] border rounded-md p-4">
                <div className="space-y-4">
                  {availablePermissions.length === 0 ? (
                    <p className="text-sm text-muted-foreground text-center py-8">
                      No permissions available for this role based on organization policy.
                    </p>
                  ) : (
                    availablePermissions.map((perm) => {
                      const isSelected = formData.permissionIds?.includes(perm.id) || false;
                      // Permissions are already filtered by allowedPermissions, so they should all be enabled
                      // But we keep the check for safety
                      const isAllowed = allowedPermissions.has(perm.name);
                      
                      return (
                        <div
                          key={perm.id}
                          className="flex items-center space-x-2 p-2 rounded"
                        >
                          <Checkbox
                            id={`perm-${perm.id}`}
                            name={`permission-${perm.id}`}
                            checked={isSelected}
                            disabled={!isAllowed}
                            onCheckedChange={() => handleTogglePermission(perm.id)}
                          />
                          <Label
                            htmlFor={`perm-${perm.id}`}
                            className={cn(
                              'text-sm font-normal cursor-pointer flex-1',
                              !isAllowed && 'cursor-not-allowed opacity-50'
                            )}
                          >
                            <div className="font-medium">{perm.name}</div>
                            {perm.description && (
                              <div className="text-xs text-muted-foreground">{perm.description}</div>
                            )}
                          </Label>
                        </div>
                      );
                    })
                  )}
                </div>
              </ScrollArea>
              {orgPolicy && orgPolicy.isActive && (
                <p className="text-xs text-muted-foreground">
                  Showing only permissions allowed by organization policy ({orgPolicy.allowedPermissions.length} allowed)
                </p>
              )}
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setEditDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={updateMutation.isPending}>
              {updateMutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              Save Changes
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

