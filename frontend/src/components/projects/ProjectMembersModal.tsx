import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import type { ProjectMember } from '@/types';

interface ProjectMembersModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  members: ProjectMember[];
  projectName: string;
}

export function ProjectMembersModal({
  open,
  onOpenChange,
  members,
  projectName,
}: ProjectMembersModalProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Team Members</DialogTitle>
          <DialogDescription>
            All members of "{projectName}"
          </DialogDescription>
        </DialogHeader>
        <div className="py-4">
          {members.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-4">
              No members assigned to this project.
            </p>
          ) : (
            <div className="space-y-3">
              {members.map((member) => (
                <div key={member.userId} className="flex items-center gap-3">
                  <Avatar className="h-10 w-10">
                    {member.avatar ? (
                      <img src={member.avatar} alt={`${member.firstName} ${member.lastName}`} />
                    ) : (
                      <AvatarFallback>
                        {(member.firstName?.[0] ?? '')}{(member.lastName?.[0] ?? '')}
                      </AvatarFallback>
                    )}
                  </Avatar>
                  <div className="flex-1">
                    <p className="text-sm font-medium">
                      {member.firstName} {member.lastName}
                    </p>
                    <p className="text-xs text-muted-foreground">{member.email}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
