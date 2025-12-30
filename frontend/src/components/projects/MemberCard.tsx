import { useState } from 'react';
import { format } from 'date-fns';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
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
import { MoreVertical, UserX } from 'lucide-react';
import RoleBadge from './RoleBadge';
import type { ProjectMember, ProjectRole } from '@/types';

interface MemberCardProps {
  member: ProjectMember;
  currentUserRole: ProjectRole | string | undefined;
  currentUserId?: number;
  onChangeRole: (userId: number, newRole: ProjectRole) => void;
  onRemove: (userId: number) => void;
}

const projectRoleOptions: { value: ProjectRole; label: string }[] = [
  { value: 'ProductOwner', label: 'Product Owner' },
  { value: 'ScrumMaster', label: 'Scrum Master' },
  { value: 'Developer', label: 'Developer' },
  { value: 'Tester', label: 'Tester' },
  { value: 'Viewer', label: 'Viewer' },
];

export function MemberCard({
  member,
  currentUserRole,
  currentUserId,
  onChangeRole,
  onRemove,
}: MemberCardProps) {
  const [showRemoveDialog, setShowRemoveDialog] = useState(false);

  // Cast to string for comparison to avoid type narrowing issues
  const role = member.role as string | undefined;
  const isCurrentUser = member.userId === currentUserId;

  // Permission checks (simplified - backend will also check)
  const canChangeRoles = currentUserRole === 'ProductOwner';
  const canRemoveMembers = currentUserRole === 'ProductOwner' || currentUserRole === 'ScrumMaster';

  // Check if we can edit this specific member
  const canEditThisMember =
    !isCurrentUser && (canChangeRoles || canRemoveMembers) && role !== 'ProductOwner';

  const getDisplayName = () => {
    if (member.firstName && member.lastName) {
      return `${member.firstName} ${member.lastName}`;
    }
    return member.userName || member.email;
  };

  const getInitials = () => {
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

  const handleChangeRole = (newRole: ProjectRole) => {
    // Prevent changing ProductOwner role
    if (role === 'ProductOwner') {
      return;
    }
    onChangeRole(member.userId, newRole);
  };

  const handleRemove = () => {
    // Prevent removing ProductOwner
    if (role === 'ProductOwner') {
      return;
    }
    onRemove(member.userId);
    setShowRemoveDialog(false);
  };

  return (
    <>
      <Card>
        <CardHeader className="flex flex-row items-start justify-between space-y-0 pb-3">
          <div className="flex items-center gap-3 flex-1 min-w-0">
            <Avatar className="h-10 w-10 flex-shrink-0">
              {member.avatar ? (
                <AvatarImage src={member.avatar} alt={getDisplayName()} />
              ) : (
                <AvatarFallback className="text-sm">{getInitials()}</AvatarFallback>
              )}
            </Avatar>
            <div className="flex flex-col min-w-0 flex-1">
              <span className="font-medium truncate">{getDisplayName()}</span>
              <span className="text-sm text-muted-foreground truncate">{member.email}</span>
            </div>
          </div>
          {canEditThisMember && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" className="h-8 w-8 flex-shrink-0">
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                {canChangeRoles && role !== 'ProductOwner' && (
                  <>
                    <DropdownMenuItem disabled className="text-xs text-muted-foreground">
                      Change Role
                    </DropdownMenuItem>
                    {projectRoleOptions
                      .filter((opt) => opt.value !== role)
                      .map((opt) => (
                        <DropdownMenuItem
                          key={opt.value}
                          onSelect={(e) => {
                            e.preventDefault();
                            handleChangeRole(opt.value);
                          }}
                          className="pl-6"
                        >
                          {opt.label}
                        </DropdownMenuItem>
                      ))}
                    <DropdownMenuSeparator />
                  </>
                )}
                {canRemoveMembers && role !== 'ProductOwner' && (
                  <DropdownMenuItem
                    className="text-destructive"
                    onSelect={(e) => {
                      e.preventDefault();
                      setShowRemoveDialog(true);
                    }}
                  >
                    <UserX className="mr-2 h-4 w-4" />
                    Remove
                  </DropdownMenuItem>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </CardHeader>
        <CardContent className="space-y-3">
          <div>
            {role ? (
              <RoleBadge role={role} />
            ) : (
              <span className="text-sm text-muted-foreground">No role assigned</span>
            )}
          </div>
          {member.invitedAt && (
            <div className="text-sm text-muted-foreground">
              {member.invitedByName ? (
                <>
                  Invited by <span className="font-medium">{member.invitedByName}</span> on{' '}
                  {format(new Date(member.invitedAt), 'MMM d, yyyy')}
                </>
              ) : (
                <>Invited on {format(new Date(member.invitedAt), 'MMM d, yyyy')}</>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      <AlertDialog open={showRemoveDialog} onOpenChange={setShowRemoveDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Remove member?</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to remove <strong>{getDisplayName()}</strong> from this
              project? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleRemove}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Remove
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}

