import { useState, useMemo, useEffect, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import { usersApi, type UserListDto } from '@/api/users';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Pagination } from '@/components/ui/pagination';
import { Card } from '@/components/ui/card';
import { UserCard } from '@/components/users/UserCard';
import { showError } from '@/lib/sweetalert';
import { Search, Users as UsersIcon, AlertCircle } from 'lucide-react';
import { useDebounce } from '@/hooks/useDebounce';

export default function Users() {
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<boolean | null>(null);
  const [page, setPage] = useState(1);
  const pageSize = 12;
  const hasShownErrorRef = useRef(false);

  // Debounce search term to avoid too many API calls
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  // Convert role filter to the format expected by API
  const roleFilterValue = useMemo(() => {
    if (!roleFilter || roleFilter === 'all') return undefined;
    return roleFilter;
  }, [roleFilter]);

  // Use TanStack Query to fetch users
  const { data, isLoading, error } = useQuery({
    queryKey: ['users', page, pageSize, roleFilterValue, statusFilter, debouncedSearchTerm],
    queryFn: () =>
      usersApi.getAllPaginated(
        page,
        pageSize,
        roleFilterValue,
        statusFilter ?? undefined,
        'CreatedAt', // Default sort field
        true, // Default sort descending
        debouncedSearchTerm || undefined
      ),
  });

  // Handle errors with useEffect to avoid showing errors on every render
  useEffect(() => {
    if (error && !hasShownErrorRef.current) {
      hasShownErrorRef.current = true;
      showError(
        'Failed to load users',
        error instanceof Error ? error.message : 'Please try again later'
      );
    }
    // Reset error flag when error is cleared
    if (!error) {
      hasShownErrorRef.current = false;
    }
  }, [error]);

  const users = data?.items || [];
  const totalPages = data?.totalPages || 1;
  const totalCount = data?.totalCount || 0;

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold mb-2">Team Members</h1>
        <p className="text-muted-foreground">
          View and search for team members in your organization.
        </p>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by name or email..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPage(1); // Reset to first page on search
            }}
            className="pl-9"
          />
        </div>

        <Select
          value={roleFilter || 'all'}
          onValueChange={(value) => {
            setRoleFilter(value === 'all' ? null : value);
            setPage(1); // Reset to first page on filter change
          }}
        >
          <SelectTrigger className="w-[150px]">
            <SelectValue placeholder="All Roles" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Roles</SelectItem>
            <SelectItem value="Admin">Admin</SelectItem>
            <SelectItem value="User">User</SelectItem>
          </SelectContent>
        </Select>

        <Select
          value={
            statusFilter === null
              ? 'all'
              : statusFilter === true
                ? 'active'
                : 'inactive'
          }
          onValueChange={(value) => {
            if (value === 'all') {
              setStatusFilter(null);
            } else if (value === 'active') {
              setStatusFilter(true);
            } else {
              setStatusFilter(false);
            }
            setPage(1); // Reset to first page on filter change
          }}
        >
          <SelectTrigger className="w-[150px]">
            <SelectValue placeholder="All Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Status</SelectItem>
            <SelectItem value="active">Active</SelectItem>
            <SelectItem value="inactive">Inactive</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Results count */}
      {!isLoading && (
        <div className="text-sm text-muted-foreground">
          Showing {users.length} of {totalCount} {totalCount === 1 ? 'member' : 'members'}
        </div>
      )}

      {/* Loading state */}
      {isLoading && (
        <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(6)].map((_, i) => (
            <Card key={i} className="p-4">
              <div className="flex items-center gap-3 mb-4">
                <Skeleton className="h-12 w-12 rounded-full" />
                <div className="space-y-2 flex-1">
                  <Skeleton className="h-4 w-3/4" />
                  <Skeleton className="h-3 w-1/2" />
                </div>
              </div>
              <div className="space-y-2">
                <Skeleton className="h-3 w-full" />
                <Skeleton className="h-3 w-2/3" />
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Error state */}
      {error && !isLoading && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Failed to load users. Please try again later.
          </AlertDescription>
        </Alert>
      )}

      {/* Empty state */}
      {!isLoading && !error && users.length === 0 && (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <UsersIcon className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-semibold mb-2">No members found</h3>
          <p className="text-muted-foreground max-w-md">
            {searchTerm || roleFilter || statusFilter !== null
              ? 'Try adjusting your search or filters to find more members.'
              : 'There are no members in your organization yet.'}
          </p>
        </div>
      )}

      {/* User grid */}
      {!isLoading && !error && users.length > 0 && (
        <>
          <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
            {users.map((user: UserListDto) => (
              <UserCard key={user.id} user={user} />
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex justify-center pt-4">
              <Pagination
                currentPage={page}
                totalPages={totalPages}
                onPageChange={setPage}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
}

