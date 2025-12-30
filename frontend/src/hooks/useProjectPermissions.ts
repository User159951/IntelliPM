import { useQuery } from '@tanstack/react-query';
import { memberService } from '@/api/memberService';
import type { ProjectRole } from '@/types';

interface ProjectPermissions {
  userRole: ProjectRole | null | undefined;
  isLoading: boolean;
  canEditProject: boolean;
  canDeleteProject: boolean;
  canInviteMembers: boolean;
  canRemoveMembers: boolean;
  canChangeRoles: boolean;
  canCreateTasks: boolean;
  canEditTasks: boolean;
  canDeleteTasks: boolean;
  canManageSprints: boolean;
  canViewMilestones: boolean;
  canCreateMilestone: boolean;
  canEditMilestone: boolean;
  canCompleteMilestone: boolean;
  canDeleteMilestone: boolean;
  isViewer: boolean;
  isProductOwner: boolean;
  isScrumMaster: boolean;
}

export function useProjectPermissions(projectId: number): ProjectPermissions {
  const { data: userRole, isLoading } = useQuery({
    queryKey: ['user-role', projectId],
    queryFn: () => memberService.getUserRole(projectId),
    enabled: !!projectId && projectId > 0,
  });

  const role = userRole || null;

  return {
    userRole: role,
    isLoading,
    canEditProject: role === 'ProductOwner' || role === 'ScrumMaster',
    canDeleteProject: role === 'ProductOwner',
    canInviteMembers: role === 'ProductOwner' || role === 'ScrumMaster',
    canRemoveMembers: role === 'ProductOwner' || role === 'ScrumMaster',
    canChangeRoles: role === 'ProductOwner',
    canCreateTasks: role !== 'Viewer' && role !== null,
    canEditTasks: role !== 'Viewer' && role !== null,
    canDeleteTasks: role === 'ProductOwner' || role === 'ScrumMaster',
    canManageSprints: role === 'ProductOwner' || role === 'ScrumMaster',
    canViewMilestones: role !== null, // All project members can view milestones
    canCreateMilestone: role === 'ProductOwner' || role === 'ScrumMaster' || role === 'Developer',
    canEditMilestone: role === 'ProductOwner' || role === 'ScrumMaster',
    canCompleteMilestone: role === 'ProductOwner' || role === 'ScrumMaster' || role === 'Developer',
    canDeleteMilestone: role === 'ProductOwner' || role === 'ScrumMaster',
    isViewer: role === 'Viewer',
    isProductOwner: role === 'ProductOwner',
    isScrumMaster: role === 'ScrumMaster',
  };
}

