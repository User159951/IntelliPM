import { useState, useEffect, useMemo } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { teamsApi } from '@/api/teams';
import { usersApi, type User } from '@/api/users';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '@/components/ui/command';
import { Check, X } from 'lucide-react';
import { showSuccess, showError } from "@/lib/sweetalert";
import { Loader2 } from 'lucide-react';
import type { Team } from '@/types';
import { useTranslation } from 'react-i18next';
import { cn } from '@/lib/utils';

interface EditTeamDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  team: Team;
}

export function EditTeamDialog({ open, onOpenChange, team }: EditTeamDialogProps) {
  const queryClient = useQueryClient();
  const { t } = useTranslation('teams');
  const [capacity, setCapacity] = useState(team.totalCapacity);
  const [selectedMemberIds, setSelectedMemberIds] = useState<number[]>([]);
  const [memberSearchOpen, setMemberSearchOpen] = useState(false);

  // Fetch available users
  const { data: usersData, isLoading: isLoadingUsers } = useQuery({
    queryKey: ['users', 'all'],
    queryFn: () => usersApi.getAll(true),
    enabled: open,
  });

  // Update state when team changes
  useEffect(() => {
    if (team) {
      setCapacity(team.totalCapacity);
      // TeamMember has 'id' which is the userId
      setSelectedMemberIds(team.members?.map(m => m.id) || []);
    }
  }, [team]);

  // Get current team members as User objects
  // Note: TeamMember.id corresponds to User.id (userId)
  const currentMembers = useMemo(() => {
    if (!usersData?.users || !team.members) return [];
    return team.members
      .map(tm => {
        // Find user by matching id (which is userId in TeamMember)
        const user = usersData.users.find(u => u.id === tm.id);
        return user;
      })
      .filter((u): u is User => u !== undefined);
  }, [usersData, team.members]);

  // Get available users (not already in team)
  const availableUsers = useMemo(() => {
    if (!usersData?.users) return [];
    return usersData.users.filter(u => !selectedMemberIds.includes(u.id));
  }, [usersData, selectedMemberIds]);

  const updateCapacityMutation = useMutation({
    mutationFn: (newCapacity: number) => teamsApi.updateCapacity(team.id, newCapacity),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      showSuccess(t('messages.capacityUpdated') || 'Team capacity updated successfully');
    },
    onError: () => {
      showError(t('messages.updateError') || 'Failed to update team capacity');
    },
  });

  const addMemberMutation = useMutation({
    mutationFn: (userId: number) => teamsApi.addMember(team.id, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      showSuccess(t('messages.memberAdded') || 'Member added successfully');
    },
    onError: (error: any) => {
      const message = error?.response?.data?.detail || error?.message || t('messages.addMemberError') || 'Failed to add member';
      showError(message);
    },
  });

  const removeMemberMutation = useMutation({
    mutationFn: (userId: number) => teamsApi.removeMember(team.id, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      showSuccess(t('messages.memberRemoved') || 'Member removed successfully');
    },
    onError: (error: any) => {
      const message = error?.response?.data?.detail || error?.message || t('messages.removeMemberError') || 'Failed to remove member';
      showError(message);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (capacity <= 0) {
      showError(t('messages.invalidCapacity') || 'Capacity must be greater than 0');
      return;
    }
    
    updateCapacityMutation.mutate(capacity);
  };

  const handleCancel = () => {
    setCapacity(team.totalCapacity);
    setSelectedMemberIds(team.members?.map(m => m.id) || []);
    onOpenChange(false);
  };

  const toggleMember = (user: User) => {
    if (selectedMemberIds.includes(user.id)) {
      // Remove member
      removeMemberMutation.mutate(user.id);
    } else {
      // Add member
      addMemberMutation.mutate(user.id);
    }
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{t('edit.title') || 'Edit Team'}</DialogTitle>
            <DialogDescription>
              {t('edit.description') || `Update settings for ${team.name}`}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="team-name">{t('form.name') || 'Team name'}</Label>
              <Input
                id="team-name"
                value={team.name}
                disabled
                className="bg-muted"
              />
              <p className="text-xs text-muted-foreground">
                {t('edit.nameReadOnly') || 'Team name cannot be changed'}
              </p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="capacity">{t('form.capacity') || 'Total capacity (hours/sprint)'}</Label>
              <Input
                id="capacity"
                type="number"
                min={1}
                value={capacity}
                onChange={(e) => setCapacity(parseInt(e.target.value) || 100)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label>{t('form.members') || 'Team members'}</Label>
              <div className="space-y-3">
                {/* Current members */}
                <div className="flex flex-wrap gap-2">
                  {currentMembers.map((member) => (
                    <Badge
                      key={member.id}
                      variant="secondary"
                      className="flex items-center gap-1 px-2 py-1"
                    >
                      <Avatar className="h-4 w-4">
                        <AvatarFallback className="text-[10px]">
                          {getInitials(`${member.firstName} ${member.lastName}`)}
                        </AvatarFallback>
                      </Avatar>
                      <span className="text-xs">
                        {member.firstName} {member.lastName}
                      </span>
                      <button
                        type="button"
                        onClick={() => removeMemberMutation.mutate(member.id)}
                        disabled={removeMemberMutation.isPending || currentMembers.length <= 1}
                        className="ml-1 hover:bg-destructive/20 rounded-full p-0.5 disabled:opacity-50 disabled:cursor-not-allowed"
                        title={currentMembers.length <= 1 ? t('edit.cannotRemoveLastMember') || 'Cannot remove the last member' : t('edit.removeMember') || 'Remove member'}
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </Badge>
                  ))}
                </div>

                {/* Add member button */}
                <Popover open={memberSearchOpen} onOpenChange={setMemberSearchOpen}>
                  <PopoverTrigger asChild>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      disabled={isLoadingUsers || addMemberMutation.isPending}
                    >
                      {isLoadingUsers ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          {t('form.loadingUsers') || 'Loading users...'}
                        </>
                      ) : (
                        <>
                          + {t('edit.addMember') || 'Add member'}
                        </>
                      )}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-full p-0" align="start">
                    <Command>
                      <CommandInput 
                        placeholder={t('form.searchUsers') || 'Search users...'} 
                      />
                      <CommandList>
                        <CommandEmpty>{t('form.noUsersFound') || 'No users found'}</CommandEmpty>
                        <CommandGroup>
                          {availableUsers.map((user) => (
                            <CommandItem
                              key={user.id}
                              value={`${user.firstName} ${user.lastName} ${user.email}`}
                              onSelect={() => {
                                toggleMember(user);
                                setMemberSearchOpen(false);
                              }}
                            >
                              <Check
                                className={cn(
                                  "mr-2 h-4 w-4",
                                  selectedMemberIds.includes(user.id) ? "opacity-100" : "opacity-0"
                                )}
                              />
                              <div className="flex items-center gap-2">
                                <Avatar className="h-6 w-6">
                                  <AvatarFallback className="text-xs">
                                    {getInitials(`${user.firstName} ${user.lastName}`)}
                                  </AvatarFallback>
                                </Avatar>
                                <div className="flex flex-col">
                                  <span className="text-sm">
                                    {user.firstName} {user.lastName}
                                  </span>
                                  <span className="text-xs text-muted-foreground">
                                    {user.email}
                                  </span>
                                </div>
                              </div>
                            </CommandItem>
                          ))}
                        </CommandGroup>
                      </CommandList>
                    </Command>
                  </PopoverContent>
                </Popover>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleCancel}>
              {t('actions.cancel') || 'Cancel'}
            </Button>
            <Button type="submit" disabled={updateCapacityMutation.isPending}>
              {updateCapacityMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {t('actions.save') || 'Save changes'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
