import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsApi } from '@/api/projects';
import { usersApi } from '@/api/users';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
import { Search, Loader2 } from 'lucide-react';
import type { User } from '@/api/users';

interface AddMemberDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  projectId: number;
  existingMemberIds: number[];
  onMemberAdded: () => void;
}

export function AddMemberDialog({
  open,
  onOpenChange,
  projectId,
  existingMemberIds,
  onMemberAdded,
}: AddMemberDialogProps) {
  const queryClient = useQueryClient();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [selectedRole, setSelectedRole] = useState<'Owner' | 'Admin' | 'Member'>('Member');

  const { data: usersData, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(),
    enabled: open,
  });

  const addMemberMutation = useMutation({
    mutationFn: (_data: { userId: number; role: 'Owner' | 'Admin' | 'Member' }) =>
      projectsApi.inviteMember(projectId, { email: '', role: 'Developer' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      onMemberAdded();
      onOpenChange(false);
      setSearchQuery('');
      setSelectedUserId(null);
      setSelectedRole('Member');
      showSuccess("Member added to project");
    },
    onError: (error) => {
      showError('Failed to add member');
    },
  });

  const availableUsers = usersData?.users?.filter(
    (user) =>
      !existingMemberIds.includes(user.id) &&
      (searchQuery.trim() === '' ||
        `${user.firstName} ${user.lastName}`.toLowerCase().includes(searchQuery.toLowerCase()) ||
        user.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
        user.username.toLowerCase().includes(searchQuery.toLowerCase()))
  ) || [];

  const handleSubmit = () => {
    if (!selectedUserId) {
      showError("Please select a user");
      return;
    }

    addMemberMutation.mutate({
      userId: selectedUserId,
      role: selectedRole,
    });
  };

  const getInitials = (user: User) => {
    return `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`.toUpperCase() || user.username[0].toUpperCase();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Add Team Member</DialogTitle>
          <DialogDescription>Search for a user to add to this project</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search by name, email, or username..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9"
            />
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium">Role</label>
            <Select value={selectedRole} onValueChange={(value: typeof selectedRole) => setSelectedRole(value)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Member">Member</SelectItem>
                <SelectItem value="Admin">Admin</SelectItem>
                <SelectItem value="Owner">Owner</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2 max-h-[300px] overflow-y-auto">
            <label className="text-sm font-medium">Select User</label>
            {isLoading ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
              </div>
            ) : availableUsers.length === 0 ? (
              <div className="py-8 text-center text-sm text-muted-foreground">
                {searchQuery.trim() ? 'No users found matching your search' : 'All users are already members'}
              </div>
            ) : (
              <div className="space-y-2">
                {availableUsers.map((user) => (
                  <div
                    key={user.id}
                    className={`flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                      selectedUserId === user.id
                        ? 'border-primary bg-primary/5'
                        : 'border-border hover:bg-muted/50'
                    }`}
                    onClick={() => setSelectedUserId(user.id)}
                  >
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={undefined} alt={`${user.firstName} ${user.lastName}`} />
                      <AvatarFallback>{getInitials(user)}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1">
                      <p className="text-sm font-medium">
                        {user.firstName} {user.lastName}
                      </p>
                      <p className="text-xs text-muted-foreground">{user.email}</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!selectedUserId || addMemberMutation.isPending}>
            {addMemberMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Add Member
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
