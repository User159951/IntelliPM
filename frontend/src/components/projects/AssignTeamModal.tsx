import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsApi } from '@/api/projects';
import { teamsApi } from '@/api/teams';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Skeleton } from '@/components/ui/skeleton';
import { showToast, showError, showConfirm } from '@/lib/sweetalert';
import { Checkbox } from '@/components/ui/checkbox';
import { X, Loader2, Users, Settings } from 'lucide-react';
import type { ProjectRole } from '@/types';

interface AssignTeamModalProps {
  projectId: number;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

const projectRoleOptions: { value: ProjectRole; label: string }[] = [
  { value: 'ProductOwner', label: 'Product Owner' },
  { value: 'ScrumMaster', label: 'Scrum Master' },
  { value: 'Developer', label: 'Developer' },
  { value: 'Tester', label: 'Tester' },
  { value: 'Viewer', label: 'Viewer' },
];

export default function AssignTeamModal({
  projectId,
  isOpen,
  onClose,
  onSuccess,
}: AssignTeamModalProps) {
  const queryClient = useQueryClient();
  const [selectedTeams, setSelectedTeams] = useState<number[]>([]);
  const [defaultRole, setDefaultRole] = useState<ProjectRole>('Developer');
  const [isAdvancedMode, setIsAdvancedMode] = useState(false);
  const [memberRoleOverrides, setMemberRoleOverrides] = useState<Record<number, ProjectRole>>({});

  // Fetch available teams
  const { data: teamsData, isLoading: teamsLoading } = useQuery({
    queryKey: ['teams'],
    queryFn: () => teamsApi.getAll(),
    enabled: isOpen,
  });

  // Fetch project to get already assigned teams
  const { data: _project } = useQuery({
    queryKey: ['project', projectId],
    queryFn: () => projectsApi.getById(projectId),
    enabled: isOpen && projectId > 0,
  });

  // Fetch team details for selected teams (for advanced mode)
  const selectedTeamsData = useMemo(() => {
    if (!teamsData?.teams) return [];
    return teamsData.teams.filter((team) => selectedTeams.includes(team.id));
  }, [teamsData, selectedTeams]);

  // Get already assigned team IDs from project
  // Note: This would ideally come from a project.assignedTeams property
  // For now, we'll show all teams and let the backend handle duplicates
  const assignedTeamIds = useMemo(() => {
    // TODO: Fetch assigned teams from project when API endpoint is available
    // For now, return empty array - backend will handle duplicate assignments gracefully
    return [] as number[];
  }, []);

  // Available teams (not already assigned)
  const availableTeams = useMemo(() => {
    if (!teamsData?.teams) return [];
    return teamsData.teams.filter((team) => !assignedTeamIds.includes(team.id));
  }, [teamsData, assignedTeamIds]);

  // Mutation to assign team
  const assignTeamMutation = useMutation({
    mutationFn: (data: {
      teamId: number;
      defaultRole?: ProjectRole;
      memberRoleOverrides?: Record<number, ProjectRole>;
    }) => projectsApi.assignTeam(projectId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['project', projectId] });
      queryClient.invalidateQueries({ queryKey: ['project-members', projectId] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
    },
    onError: (error: Error) => {
      showError('Failed to assign team', error.message || 'Please try again');
    },
  });

  // Handle team selection toggle
  const toggleTeam = (teamId: number) => {
    setSelectedTeams((prev) =>
      prev.includes(teamId) ? prev.filter((id) => id !== teamId) : [...prev, teamId]
    );
  };

  // Handle member role override change
  // Note: TeamMember.id is used as the key, but backend expects UserId
  // We need to map TeamMember to UserId - for now using member.id as placeholder
  // In production, TeamMember should have userId property
  const handleMemberRoleChange = (memberId: number, role: ProjectRole) => {
    setMemberRoleOverrides((prev) => ({
      ...prev,
      [memberId]: role,
    }));
  };

  // Handle submit
  const handleSubmit = async () => {
    if (selectedTeams.length === 0) {
      showError('No teams selected', 'Please select at least one team to assign');
      return;
    }

    // Confirmation if assigning many teams
    if (selectedTeams.length > 3) {
      const confirmed = await showConfirm(
        `Assign ${selectedTeams.length} teams?`,
        `This will assign ${selectedTeams.length} teams to the project. All team members will be added as project members.`,
        'Yes, assign teams',
        'Cancel'
      );
      if (!confirmed) return;
    }

    // Assign each team sequentially
    const assignPromises = selectedTeams.map((teamId) =>
      assignTeamMutation.mutateAsync({
        teamId,
        defaultRole,
        memberRoleOverrides: isAdvancedMode && Object.keys(memberRoleOverrides).length > 0
          ? memberRoleOverrides
          : undefined,
      })
    );

    try {
      await Promise.all(assignPromises);
      showToast(`${selectedTeams.length} team(s) assigned successfully`, 'success');
      onSuccess?.();
      handleClose();
    } catch (error) {
      // Error already handled in mutation
    }
  };

  // Reset form on close
  const handleClose = () => {
    setSelectedTeams([]);
    setDefaultRole('Developer');
    setIsAdvancedMode(false);
    setMemberRoleOverrides({});
    onClose();
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2) || 'TM';
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Assign Teams to Project</DialogTitle>
          <DialogDescription>
            Select one or more teams to assign to this project. All team members will be added as project members.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {/* Team Selection */}
          <div className="space-y-3">
            <Label>Select Teams</Label>
            {teamsLoading ? (
              <div className="space-y-2">
                {[...Array(3)].map((_, i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            ) : availableTeams.length === 0 ? (
              <div className="py-8 text-center text-sm text-muted-foreground">
                No teams available to assign. All teams may already be assigned to this project.
              </div>
            ) : (
              <div className="space-y-2 max-h-[200px] overflow-y-auto border rounded-md p-2">
                {availableTeams.map((team) => (
                  <div
                    key={team.id}
                    className="flex items-center gap-3 p-3 rounded-lg border hover:bg-muted/50 cursor-pointer transition-colors"
                    onClick={() => toggleTeam(team.id)}
                  >
                    <Checkbox
                      checked={selectedTeams.includes(team.id)}
                      onCheckedChange={() => toggleTeam(team.id)}
                      onClick={(e) => e.stopPropagation()}
                    />
                    <Users className="h-5 w-5 text-muted-foreground" />
                    <div className="flex-1">
                      <p className="text-sm font-medium">{team.name}</p>
                      <p className="text-xs text-muted-foreground">
                        {team.members?.length || 0} member{team.members?.length !== 1 ? 's' : ''}
                      </p>
                    </div>
                    {selectedTeams.includes(team.id) && (
                      <Badge variant="default" className="text-xs">
                        Selected
                      </Badge>
                    )}
                  </div>
                ))}
              </div>
            )}

            {/* Selected teams badges */}
            {selectedTeams.length > 0 && (
              <div className="flex flex-wrap gap-2 pt-2">
                {selectedTeams.map((teamId) => {
                  const team = availableTeams.find((t) => t.id === teamId);
                  if (!team) return null;
                  return (
                    <Badge key={teamId} variant="secondary" className="gap-1">
                      {team.name}
                      <button
                        onClick={() => toggleTeam(teamId)}
                        className="ml-1 hover:bg-destructive/20 rounded-full p-0.5"
                        aria-label={`Remove ${team.name}`}
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </Badge>
                  );
                })}
              </div>
            )}
          </div>

          {/* Default Role */}
          <div className="space-y-2">
            <Label>Default Role for Team Members</Label>
            <Select value={defaultRole} onValueChange={(value: ProjectRole) => setDefaultRole(value)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {projectRoleOptions.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <p className="text-xs text-muted-foreground">
              All team members will be assigned this role unless overridden in advanced mode.
            </p>
          </div>

          {/* Advanced Mode Toggle */}
          <div className="flex items-center justify-between p-3 border rounded-lg">
            <div className="space-y-0.5">
              <Label htmlFor="advanced-mode" className="flex items-center gap-2">
                <Settings className="h-4 w-4" />
                Customize Member Roles
              </Label>
              <p className="text-xs text-muted-foreground">
                Override roles for specific team members
              </p>
            </div>
            <Switch
              id="advanced-mode"
              checked={isAdvancedMode}
              onCheckedChange={setIsAdvancedMode}
            />
          </div>

          {/* Advanced Mode: Member Role Overrides */}
          {isAdvancedMode && selectedTeams.length > 0 && (
            <div className="space-y-3 border rounded-lg p-4">
              <Label className="text-sm font-semibold">Member Role Overrides</Label>
              <div className="space-y-4 max-h-[300px] overflow-y-auto">
                {selectedTeamsData.map((team) => (
                  <div key={team.id} className="space-y-2">
                    <p className="text-sm font-medium text-muted-foreground">{team.name}</p>
                    {team.members && team.members.length > 0 ? (
                      <div className="space-y-2 pl-4">
                        {team.members.map((member) => (
                          <div
                            key={member.id}
                            className="flex items-center gap-3 p-2 rounded border bg-muted/30"
                          >
                            <Avatar className="h-8 w-8">
                              <AvatarFallback className="text-xs">
                                {getInitials(member.name)}
                              </AvatarFallback>
                            </Avatar>
                            <div className="flex-1 min-w-0">
                              <p className="text-sm font-medium truncate">{member.name}</p>
                              {member.email && (
                                <p className="text-xs text-muted-foreground truncate">
                                  {member.email}
                                </p>
                              )}
                            </div>
                            <Select
                              value={memberRoleOverrides[member.id] || defaultRole}
                              onValueChange={(value: ProjectRole) =>
                                handleMemberRoleChange(member.id, value)
                              }
                            >
                              <SelectTrigger className="w-[140px]">
                                <SelectValue />
                              </SelectTrigger>
                              <SelectContent>
                                {projectRoleOptions.map((option) => (
                                  <SelectItem key={option.value} value={option.value}>
                                    {option.label}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <p className="text-xs text-muted-foreground pl-4">
                        No members in this team
                      </p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose} disabled={assignTeamMutation.isPending}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={selectedTeams.length === 0 || assignTeamMutation.isPending}
          >
            {assignTeamMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Assign {selectedTeams.length > 0 ? `${selectedTeams.length} ` : ''}Team
            {selectedTeams.length !== 1 ? 's' : ''}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

