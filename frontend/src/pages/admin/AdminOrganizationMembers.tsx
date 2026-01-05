import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { organizationsApi } from '@/api/organizations';
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { showSuccess, showError } from '@/lib/sweetalert';
import {
  Search,
  Loader2,
  Shield,
} from 'lucide-react';
import { Pagination } from '@/components/ui/pagination';

interface UserListDto {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'User' | 'Admin' | 'SuperAdmin';
  isActive: boolean;
  organizationId: number;
  organizationName: string;
  createdAt: string;
  lastLoginAt?: string | null;
}

export default function AdminOrganizationMembers() {
  const queryClient = useQueryClient();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [roleChangeDialog, setRoleChangeDialog] = useState<{
    open: boolean;
    user: UserListDto | null;
    newRole: 'User' | 'Admin';
  }>({
    open: false,
    user: null,
    newRole: 'User',
  });
  const pageSize = 20;

  const { data, isLoading } = useQuery({
    queryKey: ['admin-organization-members', currentPage, pageSize, searchQuery],
    queryFn: () => organizationsApi.getMembers(currentPage, pageSize, searchQuery || undefined),
  });

  const updateRoleMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: number; role: 'User' | 'Admin' }) =>
      organizationsApi.updateMemberRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-organization-members'] });
      setRoleChangeDialog({ open: false, user: null, newRole: 'User' });
      showSuccess('User role updated successfully');
    },
    onError: (error: Error) => {
      showError('Failed to update user role', error.message);
    },
  });

  const handleRoleChange = (user: UserListDto) => {
    setRoleChangeDialog({
      open: true,
      user,
      newRole: user.role === 'Admin' ? 'User' : 'Admin',
    });
  };

  const handleConfirmRoleChange = () => {
    if (roleChangeDialog.user) {
      updateRoleMutation.mutate({
        userId: roleChangeDialog.user.id,
        role: roleChangeDialog.newRole,
      });
    }
  };

  const getRoleBadgeVariant = (role: string) => {
    switch (role) {
      case 'SuperAdmin':
        return 'destructive';
      case 'Admin':
        return 'default';
      default:
        return 'secondary';
    }
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
        <h1 className="text-3xl font-bold">Organization Members</h1>
        <p className="text-muted-foreground mt-1">Manage members and their roles</p>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
          <Input
            placeholder="Search members..."
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
              <TableHead>Email</TableHead>
              <TableHead>Role</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Joined</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-48" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                  <TableCell className="text-right"><Skeleton className="h-4 w-20" /></TableCell>
                </TableRow>
              ))
            ) : data?.items && data.items.length > 0 ? (
              data.items.map((user: UserListDto) => (
                <TableRow key={user.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar>
                        <AvatarFallback>
                          {user.firstName?.[0] || user.username[0]}
                          {user.lastName?.[0] || ''}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <p className="font-medium">
                          {user.firstName} {user.lastName}
                        </p>
                        <p className="text-sm text-muted-foreground">{user.username}</p>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>
                    <Badge variant={getRoleBadgeVariant(user.role)}>
                      {user.role}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={user.isActive ? 'default' : 'secondary'}>
                      {user.isActive ? 'Active' : 'Inactive'}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    {format(new Date(user.createdAt), 'MMM dd, yyyy')}
                  </TableCell>
                  <TableCell className="text-right">
                    {user.role !== 'SuperAdmin' && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleRoleChange(user)}
                      >
                        <Shield className="h-4 w-4" />
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                  No members found
                </TableCell>
              </TableRow>
            )}
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

      {/* Role Change Dialog */}
      <Dialog
        open={roleChangeDialog.open}
        onOpenChange={(open) =>
          setRoleChangeDialog({ open, user: null, newRole: 'User' })
        }
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Change User Role</DialogTitle>
            <DialogDescription>
              Update the role for {roleChangeDialog.user?.firstName}{' '}
              {roleChangeDialog.user?.lastName}. Admin can only assign User or Admin roles.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label>New Role</Label>
              <Select
                value={roleChangeDialog.newRole}
                onValueChange={(value: 'User' | 'Admin') =>
                  setRoleChangeDialog({ ...roleChangeDialog, newRole: value })
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="User">User</SelectItem>
                  <SelectItem value="Admin">Admin</SelectItem>
                </SelectContent>
              </Select>
              <p className="text-xs text-muted-foreground mt-1">
                SuperAdmin role cannot be assigned by Admin users.
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() =>
                setRoleChangeDialog({ open: false, user: null, newRole: 'User' })
              }
            >
              Cancel
            </Button>
            <Button
              onClick={handleConfirmRoleChange}
              disabled={updateRoleMutation.isPending}
            >
              {updateRoleMutation.isPending && (
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
              )}
              Update Role
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

