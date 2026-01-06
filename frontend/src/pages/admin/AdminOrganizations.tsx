import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { organizationsApi, type OrganizationDto, type CreateOrganizationRequest } from '@/api/organizations';
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
import { showSuccess, showError } from '@/lib/sweetalert';
import {
  Search,
  Plus,
  Pencil,
  Trash2,
  Loader2,
} from 'lucide-react';
import { Pagination } from '@/components/ui/pagination';

export default function AdminOrganizations() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [deletingOrg, setDeletingOrg] = useState<OrganizationDto | null>(null);
  const [createForm, setCreateForm] = useState<CreateOrganizationRequest>({
    name: '',
    code: '',
  });
  const pageSize = 20;

  const { data, isLoading } = useQuery({
    queryKey: ['admin-organizations', currentPage, pageSize, searchQuery],
    queryFn: () => organizationsApi.getAll(currentPage, pageSize, searchQuery || undefined),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateOrganizationRequest) => organizationsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-organizations'] });
      setCreateDialogOpen(false);
      setCreateForm({ name: '', code: '' });
      showSuccess('Organization created successfully');
    },
    onError: (error: Error) => {
      showError('Failed to create organization', error.message);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (orgId: number) => organizationsApi.delete(orgId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-organizations'] });
      setDeletingOrg(null);
      showSuccess('Organization deleted successfully');
    },
    onError: (error: Error) => {
      // Check if error is about users in organization
      const errorMessage = error.message.toLowerCase();
      if (errorMessage.includes('user') || errorMessage.includes('member')) {
        showError(
          'Cannot delete organization',
          error.message || 'This organization has members. Please remove or reassign all members before deleting.'
        );
      } else {
        showError('Failed to delete organization', error.message);
      }
    },
  });

  const handleCreate = () => {
    if (!createForm.name.trim() || !createForm.code.trim()) {
      showError('Validation Error', 'Name and Code are required');
      return;
    }
    createMutation.mutate(createForm);
  };

  const handleDelete = () => {
    if (deletingOrg) {
      deleteMutation.mutate(deletingOrg.id);
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
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Organizations</h1>
          <p className="text-muted-foreground mt-1">Manage all organizations</p>
        </div>
        <Button onClick={() => setCreateDialogOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Create Organization
        </Button>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
          <Input
            placeholder="Search organizations..."
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
              <TableHead>Name</TableHead>
              <TableHead>Code</TableHead>
              <TableHead>Members</TableHead>
              <TableHead>Created</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                  <TableCell className="text-right"><Skeleton className="h-4 w-20" /></TableCell>
                </TableRow>
              ))
            ) : data?.items && data.items.length > 0 ? (
              data.items.map((org) => (
                <TableRow key={org.id}>
                  <TableCell className="font-medium">{org.name}</TableCell>
                  <TableCell>
                    <Badge variant="outline">{org.code}</Badge>
                  </TableCell>
                  <TableCell>{org.userCount}</TableCell>
                  <TableCell>
                    {format(new Date(org.createdAt), 'MMM dd, yyyy')}
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => navigate(`/admin/organizations/${org.id}`)}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setDeletingOrg(org)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={5} className="text-center py-8 text-muted-foreground">
                  No organizations found
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

      {/* Create Dialog */}
      <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Organization</DialogTitle>
            <DialogDescription>
              Create a new organization. The code must be unique and URL-friendly.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                value={createForm.name}
                onChange={(e) => setCreateForm({ ...createForm, name: e.target.value })}
                placeholder="Organization Name"
              />
            </div>
            <div>
              <Label htmlFor="code">Code</Label>
              <Input
                id="code"
                value={createForm.code}
                onChange={(e) => setCreateForm({ ...createForm, code: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') })}
                placeholder="organization-code"
              />
              <p className="text-xs text-muted-foreground mt-1">
                Lowercase letters, numbers, and hyphens only
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setCreateDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleCreate} disabled={createMutation.isPending}>
              {createMutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              Create
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={!!deletingOrg} onOpenChange={(open) => !open && setDeletingOrg(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Organization</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete "{deletingOrg?.name}"? This action cannot be undone.
              {deletingOrg && deletingOrg.userCount > 0 && (
                <span className="block mt-2 text-destructive font-semibold">
                  ⚠️ Warning: This organization has {deletingOrg.userCount} member(s). 
                  You must remove or reassign all members before deleting the organization.
                </span>
              )}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeletingOrg(null)} disabled={deleteMutation.isPending}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={deleteMutation.isPending}
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

