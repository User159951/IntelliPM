import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@/contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { projectsApi } from '@/api/projects';
import { tasksApi } from '@/api/tasks';
import { useProjectPermissions } from '@/hooks/useProjectPermissions';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { showSuccess, showError } from "@/lib/sweetalert";
import { MoreVertical, UserPlus, User, Briefcase, X, Crown } from 'lucide-react';
import { InviteMemberModal } from './InviteMemberModal';
import RoleBadge from './RoleBadge';
import type { ProjectMember, ProjectRole } from '@/types';

interface TeamMembersListProps {
  projectId: number;
  ownerId: number;
}

export function TeamMembersList({ projectId, ownerId }: TeamMembersListProps) {
  useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const permissions = useProjectPermissions(projectId);
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
  const [removingMember, setRemovingMember] = useState<ProjectMember | null>(null);
  const [changingRole, setChangingRole] = useState<{ member: ProjectMember; newRole: ProjectRole } | null>(null);

  const { data: membersData, isLoading } = useQuery({
    queryKey: ['project-members', projectId],
    queryFn: () => projectsApi.getMembers(projectId),
    enabled: !!projectId,
  });

  // Backend returns ProjectMember[] directly, not { members: ProjectMember[] }
  const members = membersData || [];

  // Fetch tasks to calculate workload
  const { data: tasksData } = useQuery({
    queryKey: ['tasks', projectId],
    queryFn: () => tasksApi.getByProject(projectId),
    enabled: !!projectId,
  });

  // Calculate workload for each member
  const membersWithWorkload = members.map((member) => {
    const memberTasks = tasksData?.tasks?.filter((task) => task.assigneeId === member.userId) || [];
    const taskCount = memberTasks.length;
    const storyPoints = memberTasks.reduce((sum, task) => sum + (task.storyPoints || 0), 0);

    return {
      ...member,
      currentWorkload: {
        taskCount,
        storyPoints,
      },
      status: member.status || (taskCount > 5 ? 'Busy' : taskCount > 0 ? 'Available' : 'Available'),
      role: member.role || 'Developer',
    };
  }) || [];

  const removeMemberMutation = useMutation({
    mutationFn: (userId: number) => projectsApi.removeMember(projectId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      setRemovingMember(null);
      showSuccess("Member removed from project");
    },
    onError: (error) => {
      const errorMessage = error instanceof Error ? error.message : 'Failed to remove member';
      showError('Failed to remove member', errorMessage);
    },
  });

  const changeRoleMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: number; role: ProjectRole }) =>
      projectsApi.updateMemberRole(projectId, userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      queryClient.invalidateQueries({ queryKey: ['user-role', projectId] });
      setChangingRole(null);
      showSuccess("Member role updated");
    },
    onError: (error) => {
      const errorMessage = error instanceof Error ? error.message : 'Failed to update role';
      showError('Failed to update role', errorMessage);
    },
  });

  const handleRemove = (member: ProjectMember) => {
    if (member.userId === ownerId) {
      showError("Cannot remove owner", "You must transfer ownership before removing the owner");
      return;
    }

    const hasTasks = member.currentWorkload?.taskCount && member.currentWorkload.taskCount > 0;
    if (hasTasks) {
      showError("Member has assigned tasks", "Please reassign tasks before removing this member");
      return;
    }

    setRemovingMember(member);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Available':
        return 'bg-green-500';
      case 'Busy':
        return 'bg-orange-500';
      case 'Off':
        return 'bg-gray-500';
      default:
        return 'bg-gray-500';
    }
  };

  const getRoleColor = (role: string) => {
    switch (role) {
      case 'Owner':
        return 'border-yellow-500';
      case 'Admin':
        return 'border-blue-500';
      case 'Member':
        return 'border-gray-300';
      default:
        return 'border-gray-300';
    }
  };

  const getInitials = (member: ProjectMember) => {
    return `${member.firstName?.[0] || ''}${member.lastName?.[0] || ''}`.toUpperCase() || 'U';
  };

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Team Members</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-32 bg-muted animate-pulse rounded-lg" />
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Team Members</CardTitle>
          {permissions.canInviteMembers && (
            <Button size="sm" onClick={() => setIsInviteModalOpen(true)}>
              <UserPlus className="mr-2 h-4 w-4" />
              Invite Member
            </Button>
          )}
        </CardHeader>
        <CardContent>
          {membersWithWorkload.length === 0 ? (
            <div className="py-8 text-center text-muted-foreground">
              <p>No team members yet</p>
              {permissions.canInviteMembers && (
                <Button variant="outline" className="mt-4" onClick={() => setIsInviteModalOpen(true)}>
                  <UserPlus className="mr-2 h-4 w-4" />
                  Invite First Member
                </Button>
              )}
            </div>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {membersWithWorkload.map((member) => {
                const isMemberOwner = member.userId === ownerId;
                const canModify = permissions.canRemoveMembers && !isMemberOwner;
                const canChangeRole = permissions.canChangeRoles && !isMemberOwner;

                return (
                  <Card key={member.userId} className="relative">
                    <CardContent className="p-4">
                      <div className="flex items-start justify-between mb-3">
                        <Avatar
                          className={`h-16 w-16 border-2 ${getRoleColor(member.role || 'Member')}`}
                        >
                          <AvatarImage src={member.avatar || undefined} alt={`${member.firstName} ${member.lastName}`} />
                          <AvatarFallback className="text-lg">
                            {getInitials(member)}
                          </AvatarFallback>
                        </Avatar>
                        {canModify && (
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="icon" className="h-8 w-8" aria-label="Member options">
                                <MoreVertical className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem onClick={() => navigate(`/profile`)}>
                                <User className="mr-2 h-4 w-4" />
                                View profile
                              </DropdownMenuItem>
                              <DropdownMenuItem onClick={() => navigate(`/tasks?assignee=${member.userId}`)}>
                                <Briefcase className="mr-2 h-4 w-4" />
                                Assign tasks
                              </DropdownMenuItem>
                              <DropdownMenuSeparator />
                              {canChangeRole && (
                                <DropdownMenuItem
                                  onClick={() => {
                                    // Open role change dialog
                                    setChangingRole({ member, newRole: (member.role as ProjectRole) || 'Developer' });
                                  }}
                                >
                                  Change role
                                </DropdownMenuItem>
                              )}
                              {canModify && (
                                <>
                                  <DropdownMenuSeparator />
                                  <DropdownMenuItem
                                    className="text-destructive"
                                    onClick={() => handleRemove(member)}
                                  >
                                    <X className="mr-2 h-4 w-4" />
                                    Remove from project
                                  </DropdownMenuItem>
                                </>
                              )}
                            </DropdownMenuContent>
                          </DropdownMenu>
                        )}
                      </div>

                      <div className="space-y-2">
                        <div>
                          <h4 className="font-semibold text-sm">
                            {member.firstName && member.lastName 
                              ? `${member.firstName} ${member.lastName}`
                              : member.userName || member.email}
                            {isMemberOwner && (
                              <Crown className="inline-block ml-1 h-3 w-3 text-yellow-500" />
                            )}
                          </h4>
                          <p className="text-xs text-muted-foreground">{member.email}</p>
                        </div>

                        <div className="flex items-center gap-2 flex-wrap">
                          <RoleBadge role={member.role || 'Developer'} className="text-xs" />
                          <Badge
                            variant="outline"
                            className={`text-xs ${getStatusColor(member.status || 'Available')} text-white border-0`}
                          >
                            {member.status || 'Available'}
                          </Badge>
                        </div>

                        <div className="text-xs text-muted-foreground pt-1">
                          <p>
                            {member.currentWorkload?.taskCount || 0} tasks (
                            {member.currentWorkload?.storyPoints || 0} SP)
                          </p>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </CardContent>
      </Card>

      <InviteMemberModal
        projectId={projectId}
        isOpen={isInviteModalOpen}
        onClose={() => setIsInviteModalOpen(false)}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
          queryClient.invalidateQueries({ queryKey: ['project', projectId] });
          queryClient.invalidateQueries({ queryKey: ['user-role', projectId] });
        }}
      />

      <AlertDialog open={!!removingMember} onOpenChange={(open) => !open && setRemovingMember(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove member from project?</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to remove {removingMember?.firstName && removingMember?.lastName
                ? `${removingMember.firstName} ${removingMember.lastName}`
                : removingMember?.userName || removingMember?.email} from this
              project? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => removingMember && removeMemberMutation.mutate(removingMember.userId)}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Remove
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {changingRole && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <Card className="w-full max-w-md">
            <CardHeader>
              <CardTitle>Change Role</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-sm text-muted-foreground mb-2">
                  Change role for {changingRole.member.firstName && changingRole.member.lastName
                    ? `${changingRole.member.firstName} ${changingRole.member.lastName}`
                    : changingRole.member.userName || changingRole.member.email}
                </p>
                <Select
                  value={changingRole.newRole}
                  onValueChange={(value: ProjectRole) => setChangingRole({ ...changingRole, newRole: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="ProductOwner">Product Owner</SelectItem>
                    <SelectItem value="ScrumMaster">Scrum Master</SelectItem>
                    <SelectItem value="Developer">Developer</SelectItem>
                    <SelectItem value="Tester">Tester</SelectItem>
                    <SelectItem value="Viewer">Viewer</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="flex justify-end gap-2">
                <Button variant="outline" onClick={() => setChangingRole(null)}>
                  Cancel
                </Button>
                <Button
                  onClick={() => {
                    changeRoleMutation.mutate({
                      userId: changingRole.member.userId,
                      role: changingRole.newRole,
                    });
                  }}
                >
                  Update Role
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </>
  );
}
