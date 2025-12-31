import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { projectsApi } from '@/api/projects';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { showSuccess, showError } from "@/lib/sweetalert";
import { Loader2 } from 'lucide-react';
import type { ProjectRole } from '@/types';

interface InviteMemberModalProps {
  projectId: number;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

const inviteMemberSchema = z.object({
  email: z.string().email('Invalid email'),
  role: z.enum(['ProductOwner', 'ScrumMaster', 'Developer', 'Tester', 'Viewer']),
});

type InviteMemberFormValues = z.infer<typeof inviteMemberSchema>;

const projectRoleOptions: { value: ProjectRole; label: string; description: string }[] = [
  { value: 'ProductOwner', label: 'Product Owner', description: 'Can manage project settings and all members' },
  { value: 'ScrumMaster', label: 'Scrum Master', description: 'Can manage sprints and invite members' },
  { value: 'Developer', label: 'Developer', description: 'Can create and edit tasks' },
  { value: 'Tester', label: 'Tester', description: 'Can create and edit tasks' },
  { value: 'Viewer', label: 'Viewer', description: 'Read-only access to project' },
];

export function InviteMemberModal({ projectId, isOpen, onClose, onSuccess }: InviteMemberModalProps) {
  const form = useForm<InviteMemberFormValues>({
    resolver: zodResolver(inviteMemberSchema),
    defaultValues: {
      email: '',
      role: 'Developer',
    },
  });

  const inviteMutation = useMutation({
    mutationFn: (data: { email: string; role: ProjectRole }) => 
      projectsApi.inviteMember(projectId, data),
    onSuccess: () => {
      showSuccess("Member invited", "An invitation has been sent to the member.");
      form.reset();
      onSuccess();
      onClose();
    },
    onError: () => {
      showError('Failed to invite member');
    },
  });

  const onSubmit = (values: InviteMemberFormValues) => {
    inviteMutation.mutate({
      email: values.email.trim(),
      role: values.role,
    });
  };

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      form.reset();
      onClose();
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)}>
            <DialogHeader>
              <DialogTitle>Invite Member</DialogTitle>
              <DialogDescription>
                Invite a user to join this project by email. They will receive an invitation.
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email address</FormLabel>
                    <FormControl>
                      <Input
                        type="email"
                        placeholder="user@example.com"
                        disabled={inviteMutation.isPending}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Enter the email address of the user you want to invite
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="role"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Role</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={inviteMutation.isPending}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select role" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {projectRoleOptions.map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            <div className="flex flex-col">
                              <span className="font-medium">{option.label}</span>
                              <span className="text-xs text-muted-foreground">{option.description}</span>
                            </div>
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormDescription>
                      Select the role for this member in the project
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
            <DialogFooter>
              <Button 
                type="button" 
                variant="outline" 
                onClick={onClose} 
                disabled={inviteMutation.isPending}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={inviteMutation.isPending}>
                {inviteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Send Invitation
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
