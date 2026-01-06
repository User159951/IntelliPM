import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { usersApi, type UserListDto, type BulkUpdateUsersStatusRequest } from '@/api/users';
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
import { Checkbox } from '@/components/ui/checkbox';
import { showToast, showError } from "@/lib/sweetalert";
import {
  Search,
  Pencil,
  Trash2,
  Loader2,
  UserPlus,
  Download,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
  Eye,
} from 'lucide-react';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { EditUserDialog } from '@/components/admin/EditUserDialog';
import { DeleteUserDialog } from '@/components/admin/DeleteUserDialog';
import { InviteUserDialog } from '@/components/admin/InviteUserDialog';
import { UserDetailDialog } from '@/components/admin/UserDetailDialog';
import { Pagination } from '@/components/ui/pagination';

type SortField = 'name' | 'email' | 'role' | 'createdAt' | 'status';
type SortDirection = 'asc' | 'desc';

export default function AdminUsers() {
  const queryClient = useQueryClient();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [roleFilter, setRoleFilter] = useState<string>('all');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [sortField, setSortField] = useState<SortField>('name');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [selectedUsers, setSelectedUsers] = useState<Set<number>>(new Set());
  const [editingUser, setEditingUser] = useState<UserListDto | null>(null);
  const [deletingUser, setDeletingUser] = useState<UserListDto | null>(null);
  const [detailUser, setDetailUser] = useState<UserListDto | null>(null);
  const [inviteDialogOpen, setInviteDialogOpen] = useState(false);
  const pageSize = 20;

  const isActiveFilter = useMemo(() => {
    if (statusFilter === 'active') return true;
    if (statusFilter === 'inactive') return false;
    return undefined;
  }, [statusFilter]);

  const { data, isLoading } = useQuery({
    queryKey: ['admin-users', currentPage, pageSize, roleFilter, isActiveFilter, sortField, sortDirection],
    queryFn: () =>
      usersApi.getAllPaginated(
        currentPage,
        pageSize,
        roleFilter !== 'all' ? roleFilter : undefined,
        isActiveFilter,
        sortField,
        sortDirection === 'desc'
      ),
  });

  // Client-side search filter (applied after server-side filters)
  const filteredUsers = useMemo(() => {
    if (!data?.items) return [];
    if (!searchQuery.trim()) return data.items;

    const query = searchQuery.toLowerCase();
    return data.items.filter(
      (user: UserListDto) =>
        user.firstName.toLowerCase().includes(query) ||
        user.lastName.toLowerCase().includes(query) ||
        user.email.toLowerCase().includes(query) ||
        user.username.toLowerCase().includes(query)
    );
  }, [data?.items, searchQuery]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const toggleUserSelection = (userId: number) => {
    const newSelected = new Set(selectedUsers);
    if (newSelected.has(userId)) {
      newSelected.delete(userId);
    } else {
      newSelected.add(userId);
    }
    setSelectedUsers(newSelected);
  };

  const toggleSelectAll = () => {
    if (selectedUsers.size === filteredUsers.length && filteredUsers.length > 0) {
      setSelectedUsers(new Set());
    } else {
      setSelectedUsers(new Set(filteredUsers.map((u: UserListDto) => u.id)));
    }
  };

  const bulkStatusMutation = useMutation({
    mutationFn: (data: BulkUpdateUsersStatusRequest) => usersApi.bulkUpdateStatus(data),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      setSelectedUsers(new Set());
      showToast(
        `${result.successCount} user(s) updated successfully.${result.failureCount > 0 ? ` ${result.failureCount} failed.` : ''}`,
        result.failureCount > 0 ? 'warning' : 'success'
      );
      if (result.errors.length > 0) {
        showError('Some errors occurred', result.errors.join(', '));
      }
    },
    onError: () => {
      showError('Failed to update users');
    },
  });

  const handleBulkActivate = () => {
    if (selectedUsers.size === 0) return;
    bulkStatusMutation.mutate({
      userIds: Array.from(selectedUsers),
      isActive: true,
    });
  };

  const handleBulkDeactivate = () => {
    if (selectedUsers.size === 0) return;
    bulkStatusMutation.mutate({
      userIds: Array.from(selectedUsers),
      isActive: false,
    });
  };

  const handleExportCSV = () => {
    if (!data?.items || data.items.length === 0) {
      showError("No data to export", "There are no users to export.");
      return;
    }

    // Enhanced CSV export with more columns and better formatting
    const headers = [
      'ID',
      'First Name',
      'Last Name',
      'Full Name',
      'Username',
      'Email',
      'Role',
      'Status',
      'Organization ID',
      'Organization Name',
      'Project Count',
      'Created At',
      'Is Active',
    ];
    
    const rows = data.items.map((user: UserListDto) => [
      user.id.toString(),
      user.firstName,
      user.lastName,
      `${user.firstName} ${user.lastName}`,
      user.username,
      user.email,
      user.globalRole,
      user.isActive ? 'Active' : 'Inactive',
      user.organizationId.toString(),
      user.organizationName,
      user.projectCount.toString(),
      format(new Date(user.createdAt), 'yyyy-MM-dd HH:mm:ss'),
      user.isActive ? 'Yes' : 'No',
    ]);

    // Add BOM for Excel compatibility
    const BOM = '\uFEFF';
    const csvContent = BOM + [
      headers.join(','),
      ...rows.map((row) => row.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(',')),
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `users_export_${format(new Date(), 'yyyy-MM-dd_HHmmss')}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    showToast(`Exported ${data.items.length} user(s) to CSV.`, 'success');
  };


  const SortIcon = ({ field }: { field: SortField }) => {
    if (sortField !== field) {
      return <ArrowUpDown className="h-4 w-4 ml-1" />;
    }
    return sortDirection === 'asc' ? (
      <ArrowUp className="h-4 w-4 ml-1" />
    ) : (
      <ArrowDown className="h-4 w-4 ml-1" />
    );
  };

  return (
    <div className="container mx-auto p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">User Management</h1>
        <p className="text-muted-foreground">
          Manage users in your organization. View, edit roles, and delete users.
        </p>
      </div>

      {/* Filters and Actions */}
      <div className="mb-4 space-y-4">
        <div className="flex items-center gap-4 flex-wrap">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search by name or email..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9"
            />
          </div>

          <Select value={roleFilter} onValueChange={setRoleFilter}>
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="All Roles" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Roles</SelectItem>
              <SelectItem value="Admin">Admin</SelectItem>
              <SelectItem value="User">User</SelectItem>
            </SelectContent>
          </Select>

          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="All Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>

          <Button onClick={() => setInviteDialogOpen(true)}>
            <UserPlus className="mr-2 h-4 w-4" />
            Inviter un utilisateur
          </Button>

          <Button variant="outline" onClick={handleExportCSV}>
            <Download className="mr-2 h-4 w-4" />
            Export CSV
          </Button>
        </div>

        {/* Bulk Actions Toolbar */}
        {selectedUsers.size > 0 && (
          <div className="flex items-center gap-2 p-3 bg-muted rounded-lg">
            <span className="text-sm font-medium">
              {selectedUsers.size} user(s) selected
            </span>
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  <span>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={handleBulkActivate}
                      disabled={selectedUsers.size === 0 || bulkStatusMutation.isPending}
                    >
                      {bulkStatusMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      {bulkStatusMutation.isPending ? 'Processing...' : 'Activate'}
                    </Button>
                  </span>
                </TooltipTrigger>
                {selectedUsers.size === 0 && (
                  <TooltipContent>
                    <p>Select at least one user to activate</p>
                  </TooltipContent>
                )}
              </Tooltip>
            </TooltipProvider>
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  <span>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={handleBulkDeactivate}
                      disabled={selectedUsers.size === 0 || bulkStatusMutation.isPending}
                    >
                      {bulkStatusMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      {bulkStatusMutation.isPending ? 'Processing...' : 'Deactivate'}
                    </Button>
                  </span>
                </TooltipTrigger>
                {selectedUsers.size === 0 && (
                  <TooltipContent>
                    <p>Select at least one user to deactivate</p>
                  </TooltipContent>
                )}
              </Tooltip>
            </TooltipProvider>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => setSelectedUsers(new Set())}
            >
              Clear
            </Button>
          </div>
        )}
      </div>

      {/* Table */}
      <div className="bg-card rounded-lg border">
        {isLoading ? (
          <div className="p-6 space-y-4">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </div>
        ) : filteredUsers.length === 0 ? (
          <div className="p-12 text-center">
            <p className="text-muted-foreground">
              {searchQuery ? 'No users found matching your search.' : 'No users found.'}
            </p>
          </div>
        ) : (
          <>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      checked={
                        filteredUsers.length > 0 &&
                        selectedUsers.size === filteredUsers.length
                      }
                      onCheckedChange={toggleSelectAll}
                      aria-label="Select all"
                    />
                  </TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('name')}
                      className="flex items-center hover:text-foreground"
                    >
                      Name
                      <SortIcon field="name" />
                    </button>
                  </TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('email')}
                      className="flex items-center hover:text-foreground"
                    >
                      Email
                      <SortIcon field="email" />
                    </button>
                  </TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('role')}
                      className="flex items-center hover:text-foreground"
                    >
                      Role
                      <SortIcon field="role" />
                    </button>
                  </TableHead>
                  <TableHead>Projects</TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('createdAt')}
                      className="flex items-center hover:text-foreground"
                    >
                      Created
                      <SortIcon field="createdAt" />
                    </button>
                  </TableHead>
                  <TableHead>Last Login</TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('status')}
                      className="flex items-center hover:text-foreground"
                    >
                      Status
                      <SortIcon field="status" />
                    </button>
                  </TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredUsers.map((user: UserListDto) => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <Checkbox
                        checked={selectedUsers.has(user.id)}
                        onCheckedChange={() => toggleUserSelection(user.id)}
                        aria-label={`Select user ${user.username}`}
                      />
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <Avatar className="h-8 w-8">
                          <AvatarFallback className="bg-primary text-primary-foreground text-xs">
                            {`${user.firstName[0]}${user.lastName[0]}`.toUpperCase()}
                          </AvatarFallback>
                        </Avatar>
                        <div>
                          <div className="font-medium">
                            {user.firstName} {user.lastName}
                          </div>
                          <div className="text-sm text-muted-foreground">@{user.username}</div>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      <Badge variant={user.globalRole === 'Admin' ? 'default' : 'secondary'}>
                        {user.globalRole}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline">{user.projectCount}</Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {format(new Date(user.createdAt), 'MMM d, yyyy')}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {user.lastLoginAt
                        ? format(new Date(user.lastLoginAt), 'MMM d, yyyy HH:mm')
                        : 'Never'}
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.isActive ? 'default' : 'destructive'}>
                        {user.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDetailUser(user)}
                          title="View details"
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <span>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setEditingUser(user)}
                                disabled={!user.isActive}
                              >
                                <Pencil className="h-4 w-4" />
                              </Button>
                            </span>
                          </TooltipTrigger>
                          {!user.isActive && (
                            <TooltipContent>
                              <p>Cannot edit inactive user. Activate user first.</p>
                            </TooltipContent>
                          )}
                        </Tooltip>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <span>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setDeletingUser(user)}
                                disabled={!user.isActive}
                              >
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </span>
                          </TooltipTrigger>
                          {!user.isActive && (
                            <TooltipContent>
                              <p>Cannot delete inactive user</p>
                            </TooltipContent>
                          )}
                        </Tooltip>
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

      {editingUser && (
        <EditUserDialog
          open={!!editingUser}
          onOpenChange={(open) => !open && setEditingUser(null)}
          user={editingUser}
        />
      )}

      {deletingUser && (
        <DeleteUserDialog
          open={!!deletingUser}
          onOpenChange={(open) => !open && setDeletingUser(null)}
          user={deletingUser}
        />
      )}
      {detailUser && (
        <UserDetailDialog
          open={!!detailUser}
          onOpenChange={(open) => !open && setDetailUser(null)}
          user={detailUser}
        />
      )}
      <InviteUserDialog
        open={inviteDialogOpen}
        onOpenChange={(open) => {
          setInviteDialogOpen(open);
          if (!open) {
            queryClient.invalidateQueries({ queryKey: ['admin-users'] });
          }
        }}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['admin-users'] });
        }}
      />
    </div>
  );
}
