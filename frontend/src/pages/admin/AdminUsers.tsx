import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatDate, DateFormats } from '@/utils/dateFormat';
import { useTranslation } from 'react-i18next';
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
  UserCheck,
  UserX,
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
  const { language } = useLanguage();
  const { t } = useTranslation('admin');
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
  const [togglingUser, setTogglingUser] = useState<UserListDto | null>(null);
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
        `${result.successCount} ${t('users.bulkUpdate.success')}${result.failureCount > 0 ? ` ${result.failureCount}${t('users.bulkUpdate.failed')}` : ''}`,
        result.failureCount > 0 ? 'warning' : 'success'
      );
      if (result.errors.length > 0) {
        showError(t('users.bulkUpdate.errors'), result.errors.join(', '));
      }
    },
    onError: () => {
      showError(t('users.bulkUpdate.error'));
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

  const toggleUserStatusMutation = useMutation({
    mutationFn: (id: number) => {
      const user = filteredUsers.find((u: UserListDto) => u.id === id);
      if (!user) throw new Error('User not found');
      return user.isActive ? usersApi.deactivate(id) : usersApi.activate(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      setTogglingUser(null);
      showToast(t('users.status.toggleSuccess'), 'success');
    },
    onError: (error: unknown) => {
      let errorMessage = t('users.status.toggleError');
      if (error && typeof error === 'object') {
        if ('response' in error && error.response && typeof error.response === 'object') {
          if ('data' in error.response && error.response.data && typeof error.response.data === 'object') {
            if ('detail' in error.response.data && typeof error.response.data.detail === 'string') {
              errorMessage = error.response.data.detail;
            } else if ('error' in error.response.data && typeof error.response.data.error === 'string') {
              errorMessage = error.response.data.error;
            }
          }
        } else if ('message' in error && typeof error.message === 'string') {
          errorMessage = error.message;
        }
      }
      showError(t('users.status.toggleError'), errorMessage);
      setTogglingUser(null);
    },
  });

  const handleToggleUserStatus = (user: UserListDto) => {
    setTogglingUser(user);
    toggleUserStatusMutation.mutate(user.id);
  };

  const handleExportCSV = () => {
    if (!data?.items || data.items.length === 0) {
      showError(t('users.export.noData'), t('users.export.noDataMessage'));
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
      formatDate(user.createdAt, 'yyyy-MM-dd HH:mm:ss', language),
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
    link.setAttribute('download', `users_export_${formatDate(new Date(), 'yyyy-MM-dd_HHmmss', language)}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    showToast(t('users.export.success', { count: data.items.length }), 'success');
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
        <h1 className="text-3xl font-bold mb-2">{t('users.title')}</h1>
        <p className="text-muted-foreground">
          {t('users.description')}
        </p>
      </div>

      {/* Filters and Actions */}
      <div className="mb-4 space-y-4">
        <div className="flex items-center gap-4 flex-wrap">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="search-users"
              name="search"
              placeholder={t('users.table.searchPlaceholder')}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9"
            />
          </div>

          <Select value={roleFilter} onValueChange={setRoleFilter}>
            <SelectTrigger className="w-[150px]" id="role-filter" name="role">
              <SelectValue placeholder={t('users.filters.allRoles')} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t('users.filters.allRoles')}</SelectItem>
              <SelectItem value="Admin">Admin</SelectItem>
              <SelectItem value="User">User</SelectItem>
            </SelectContent>
          </Select>

          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[150px]" id="status-filter" name="status">
              <SelectValue placeholder={t('users.filters.allStatus')} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t('users.filters.allStatus')}</SelectItem>
              <SelectItem value="active">{t('users.filters.active')}</SelectItem>
              <SelectItem value="inactive">{t('users.filters.inactive')}</SelectItem>
            </SelectContent>
          </Select>

          <Button onClick={() => setInviteDialogOpen(true)}>
            <UserPlus className="mr-2 h-4 w-4" />
            {t('users.actions.invite')}
          </Button>

          <Button variant="outline" onClick={handleExportCSV}>
            <Download className="mr-2 h-4 w-4" />
            {t('users.actions.exportCSV')}
          </Button>
        </div>

        {/* Bulk Actions Toolbar */}
        {selectedUsers.size > 0 && (
          <div className="flex items-center gap-2 p-3 bg-muted rounded-lg">
            <span className="text-sm font-medium">
              {selectedUsers.size} {t('users.actions.selected')}
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
                      {bulkStatusMutation.isPending ? t('users.actions.processing') : t('users.actions.activate')}
                    </Button>
                  </span>
                </TooltipTrigger>
                {selectedUsers.size === 0 && (
                  <TooltipContent>
                    <p>{t('users.actions.selectAll')}</p>
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
                      {bulkStatusMutation.isPending ? t('users.actions.processing') : t('users.actions.deactivate')}
                    </Button>
                  </span>
                </TooltipTrigger>
                {selectedUsers.size === 0 && (
                  <TooltipContent>
                    <p>{t('users.actions.selectToDeactivate')}</p>
                  </TooltipContent>
                )}
              </Tooltip>
            </TooltipProvider>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => setSelectedUsers(new Set())}
            >
              {t('users.actions.clear')}
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
              {searchQuery ? t('users.table.noUsersMatching') : t('users.table.noUsers')}
            </p>
          </div>
        ) : (
          <>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      id="select-all-users"
                      name="selectAll"
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
                      {t('users.table.headers.name')}
                      <SortIcon field="name" />
                    </button>
                  </TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('email')}
                      className="flex items-center hover:text-foreground"
                    >
                      {t('users.table.headers.email')}
                      <SortIcon field="email" />
                    </button>
                  </TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('role')}
                      className="flex items-center hover:text-foreground"
                    >
                      {t('users.table.headers.role')}
                      <SortIcon field="role" />
                    </button>
                  </TableHead>
                  <TableHead>{t('users.table.headers.projects')}</TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('createdAt')}
                      className="flex items-center hover:text-foreground"
                    >
                      {t('users.table.headers.created')}
                      <SortIcon field="createdAt" />
                    </button>
                  </TableHead>
                  <TableHead>{t('users.table.headers.lastLogin')}</TableHead>
                  <TableHead>
                    <button
                      onClick={() => handleSort('status')}
                      className="flex items-center hover:text-foreground"
                    >
                      {t('users.table.headers.status')}
                      <SortIcon field="status" />
                    </button>
                  </TableHead>
                  <TableHead className="text-right">{t('users.table.headers.actions')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredUsers.map((user: UserListDto) => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <Checkbox
                        id={`select-user-${user.id}`}
                        name={`selectUser-${user.id}`}
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
                      {formatDate(user.createdAt, DateFormats.LONG(language), language)}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {user.lastLoginAt
                        ? formatDate(user.lastLoginAt, DateFormats.DATETIME(language), language)
                        : t('users.status.never')}
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.isActive ? 'default' : 'destructive'}>
                        {user.isActive ? t('users.status.active') : t('users.status.inactive')}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDetailUser(user)}
                          title={t('users.tooltips.viewDetails')}
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
                              <p>{t('users.tooltips.cannotEditInactive')}</p>
                            </TooltipContent>
                          )}
                        </Tooltip>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <span>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleToggleUserStatus(user)}
                                disabled={toggleUserStatusMutation.isPending && togglingUser?.id === user.id}
                                title={user.isActive ? t('users.actions.deactivate') : t('users.actions.activate')}
                              >
                                {toggleUserStatusMutation.isPending && togglingUser?.id === user.id ? (
                                  <Loader2 className="h-4 w-4 animate-spin" />
                                ) : user.isActive ? (
                                  <UserX className="h-4 w-4 text-orange-600" />
                                ) : (
                                  <UserCheck className="h-4 w-4 text-green-600" />
                                )}
                              </Button>
                            </span>
                          </TooltipTrigger>
                          <TooltipContent>
                            <p>{user.isActive ? t('users.tooltips.deactivate') : t('users.tooltips.activate')}</p>
                          </TooltipContent>
                        </Tooltip>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <span>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setDeletingUser(user)}
                              >
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </span>
                          </TooltipTrigger>
                          <TooltipContent>
                            <p>{t('users.tooltips.delete')}</p>
                          </TooltipContent>
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
