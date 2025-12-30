import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { projectsApi } from '@/api/projects';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Skeleton } from '@/components/ui/skeleton';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
import { Plus, Loader2, MoreVertical, UserX, ArrowLeft } from 'lucide-react';
import { InviteMemberModal } from '@/components/projects/InviteMemberModal';
import type { ProjectMember, ProjectRole } from '@/types';

const projectRoleLabels: Record<ProjectRole, string> = {
  ProductOwner: 'Product Owner',
  ScrumMaster: 'Scrum Master',
  Developer: 'Developer',
  Tester: 'Tester',
  Viewer: 'Viewer',
};

const projectRoleColors: Record<ProjectRole, string> = {
  ProductOwner: 'bg-purple-500/10 text-purple-500 border-purple-500/20',
  ScrumMaster: 'bg-blue-500/10 text-blue-500 border-blue-500/20',
  Developer: 'bg-green-500/10 text-green-500 border-green-500/20',
  Tester: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
  Viewer: 'bg-gray-500/10 text-gray-500 border-gray-500/20',
};

const projectRoleOptions: { value: ProjectRole; label: string }[] = [
  { value: 'ProductOwner', label: 'Product Owner' },
  { value: 'ScrumMaster', label: 'Scrum Master' },
  { value: 'Developer', label: 'Developer' },
  { value: 'Tester', label: 'Tester' },
  { value: 'Viewer', label: 'Viewer' },
];

export default function ProjectMembers() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const projectId = id ? parseInt(id) : 0;
  
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
  const [memberToRemove, setMemberToRemove] = useState<ProjectMember | null>(null);

  const { data: members, isLoading, error } = useQuery({
    queryKey: ['project-members', projectId],
    queryFn: () => projectsApi.getMembers(projectId),
    enabled: projectId > 0,
  });

  const { data: project } = useQuery({
    queryKey: ['project', projectId],
    queryFn: () => projectsApi.getById(projectId),
    enabled: projectId > 0,
  });

  // Get current user's role in the project (for permission checks)
  const currentUserMember = members?.find(m => m.userId === user?.userId);
  const currentUserRole = currentUserMember?.role as ProjectRole | undefined;

  // Permission checks (simplified - backend will also check)
  const canInviteMembers = currentUserRole === 'ProductOwner' || currentUserRole === 'ScrumMaster';
  const canRemoveMembers = currentUserRole === 'ProductOwner' || currentUserRole === 'ScrumMaster';
  const canChangeRoles = currentUserRole === 'ProductOwner';

  const removeMemberMutation = useMutation({
    mutationFn: (userId: number) => projectsApi.removeMember(projectId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      setMemberToRemove(null);
      showSuccess("Member removed", "The member has been removed from the project.");
    },
    onError: (error) => {
      showError('Failed to remove member');
    },
  });

  const changeRoleMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: number; role: ProjectRole }) => 
      projectsApi.updateMemberRole(projectId, userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      showToast('Role updated', "success");
    },
    onError: (error) => {
      showError('Failed to update role');
    },
  });

  const handleChangeRole = (member: ProjectMember, newRole: ProjectRole) => {
    // Prevent changing ProductOwner role
    if (member.role === 'ProductOwner') {
      showError("Cannot change role", "Product Owner role cannot be changed");
      return;
    }

    changeRoleMutation.mutate({ userId: member.userId, role: newRole });
  };

  const handleRemoveMember = (member: ProjectMember) => {
    // Prevent removing ProductOwner
    if (member.role === 'ProductOwner') {
      showError("Cannot remove member", "Product Owner cannot be removed from the project");
      return;
    }

    removeMemberMutation.mutate(member.userId);
  };

  const getInitials = (member: ProjectMember) => {
    if (member.firstName && member.lastName) {
      return `${member.firstName[0]}${member.lastName[0]}`.toUpperCase();
    }
    if (member.userName) {
      const parts = member.userName.split(' ');
      if (parts.length >= 2) {
        return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
      }
      return member.userName.substring(0, 2).toUpperCase();
    }
    return member.email.substring(0, 2).toUpperCase();
  };

  const getDisplayName = (member: ProjectMember) => {
    if (member.firstName && member.lastName) {
      return `${member.firstName} ${member.lastName}`;
    }
    return member.userName || member.email;
  };

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(`/projects/${projectId}`)}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold text-foreground">Project Members</h1>
          </div>
        </div>
        <Card className="py-16 text-center">
          <p className="text-muted-foreground">
            {error instanceof Error ? error.message : 'Failed to load project members'}
          </p>
          <Button 
            variant="outline" 
            className="mt-4" 
            onClick={() => queryClient.invalidateQueries({ queryKey: ['project-members', projectId] })}
          >
            Retry
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(`/projects/${projectId}`)}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold text-foreground">
              {project ? `${project.name} - Members` : 'Project Members'}
            </h1>
            <p className="text-muted-foreground">
              Manage team members and their roles in this project
            </p>
          </div>
        </div>
        {canInviteMembers && (
          <Button onClick={() => setIsInviteModalOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Invite Member
          </Button>
        )}
      </div>

      {isLoading ? (
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-96 mt-2" />
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[1, 2, 3, 4].map((i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          </CardContent>
        </Card>
      ) : members && members.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Team Members ({members.length})</CardTitle>
            <CardDescription>
              Manage roles and permissions for project members
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Member</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Invited</TableHead>
                  <TableHead>Invited By</TableHead>
                  <TableHead className="w-[100px]">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {members.map((member) => {
                  const role = member.role as ProjectRole | undefined;
                  const isCurrentUser = member.userId === user?.userId;
                  return (
                    <TableRow key={member.id}>
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <Avatar className="h-10 w-10">
                            {member.avatar ? (
                              <img src={member.avatar} alt={getDisplayName(member)} />
                            ) : (
                              <AvatarFallback className="text-sm">
                                {getInitials(member)}
                              </AvatarFallback>
                            )}
                          </Avatar>
                          <div className="flex flex-col">
                            <span className="font-medium">{getDisplayName(member)}</span>
                            <span className="text-sm text-muted-foreground">{member.email}</span>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        {role && canChangeRoles && !isCurrentUser && role !== 'ProductOwner' ? (
                          <Select
                            value={role}
                            onValueChange={(newRole: ProjectRole) => handleChangeRole(member, newRole)}
                            disabled={changeRoleMutation.isPending}
                          >
                            <SelectTrigger className="w-[160px] border-0 bg-transparent p-0 h-auto hover:bg-transparent focus:ring-0">
                              <SelectValue>
                                <Badge 
                                  variant="outline" 
                                  className={`cursor-pointer ${projectRoleColors[role]}`}
                                >
                                  {projectRoleLabels[role]}
                                </Badge>
                              </SelectValue>
                            </SelectTrigger>
                            <SelectContent>
                              {projectRoleOptions.map((option) => (
                                <SelectItem key={option.value} value={option.value}>
                                  {option.label}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        ) : role ? (
                          <Badge 
                            variant="outline" 
                            className={projectRoleColors[role]}
                          >
                            {projectRoleLabels[role]}
                          </Badge>
                        ) : (
                          <span className="text-sm text-muted-foreground">—</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {member.invitedAt ? (
                          <span className="text-sm text-muted-foreground">
                            {format(new Date(member.invitedAt), 'MMM d, yyyy')}
                          </span>
                        ) : (
                          <span className="text-sm text-muted-foreground">—</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {member.invitedByName ? (
                          <span className="text-sm text-muted-foreground">
                            {member.invitedByName}
                          </span>
                        ) : (
                          <span className="text-sm text-muted-foreground">—</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {canRemoveMembers && !isCurrentUser && role !== 'ProductOwner' && (
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="icon" className="h-8 w-8">
                                <MoreVertical className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem
                                className="text-destructive"
                                onSelect={(e) => {
                                  e.preventDefault();
                                  setMemberToRemove(member);
                                }}
                              >
                                <UserX className="mr-2 h-4 w-4" />
                                Remove
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ) : (
        <Card className="py-16 text-center">
          <p className="text-muted-foreground mb-4">No members found</p>
          {canInviteMembers && (
            <Button onClick={() => setIsInviteModalOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Invite First Member
            </Button>
          )}
        </Card>
      )}

      <InviteMemberModal
        isOpen={isInviteModalOpen}
        onClose={() => setIsInviteModalOpen(false)}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
          queryClient.invalidateQueries({ queryKey: ['project', projectId] });
        }}
        projectId={projectId}
      />

      {/* Remove member confirmation dialog */}
      <AlertDialog open={!!memberToRemove} onOpenChange={(open) => !open && setMemberToRemove(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove member?</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to remove <strong>{memberToRemove && getDisplayName(memberToRemove)}</strong> from this project?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={removeMemberMutation.isPending}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => memberToRemove && handleRemoveMember(memberToRemove)}
              disabled={removeMemberMutation.isPending}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {removeMemberMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Remove
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

