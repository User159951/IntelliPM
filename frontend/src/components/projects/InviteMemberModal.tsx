import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useMemo } from 'react';
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
import { ProjectRole as ProjectRoleType } from '@/types/generated/enums';
import { useTranslation } from '@/hooks/useTranslation';

interface InviteMemberModalProps {
  projectId: number;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

// Create zod enum from ProjectRole type
const projectRoleEnum = z.enum([
  'ProductOwner',
  'ScrumMaster', 
  'Developer',
  'Tester',
  'Viewer'
] as [ProjectRoleType, ...ProjectRoleType[]]);

const getProjectRoleOptions = (t: (key: string) => string): { value: ProjectRole; label: string; description: string }[] => [
  { value: 'ProductOwner', label: t('common:roles.productOwner'), description: t('common:roles.roleDescriptions.productOwner') },
  { value: 'ScrumMaster', label: t('common:roles.scrumMaster'), description: t('common:roles.roleDescriptions.scrumMaster') },
  { value: 'Developer', label: t('common:roles.developer'), description: t('common:roles.roleDescriptions.developer') },
  { value: 'Tester', label: t('common:roles.tester'), description: t('common:roles.roleDescriptions.tester') },
  { value: 'Viewer', label: t('common:roles.viewer'), description: t('common:roles.roleDescriptions.viewer') },
];

export function InviteMemberModal({ projectId, isOpen, onClose, onSuccess }: InviteMemberModalProps) {
  const { t } = useTranslation('common');
  
  // Create schema inside component so it has access to t function
  const inviteMemberSchema = useMemo(() => z.object({
    email: z.string().email(t('errors.validation.email')),
    role: projectRoleEnum,
  }), [t]);

  type InviteMemberFormValues = z.infer<typeof inviteMemberSchema>;

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
      showSuccess(t('messages.success.memberInvited'), t('messages.success.memberInvitedDesc'));
      form.reset();
      onSuccess();
      onClose();
    },
    onError: () => {
      showError(t('messages.error.failedToInvite'));
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
              <DialogTitle>{t('buttons.create')} {t('labels.member')}</DialogTitle>
              <DialogDescription>
                {t('descriptions.inviteMemberDesc')}
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t('labels.email')}</FormLabel>
                    <FormControl>
                      <Input
                        type="email"
                        placeholder={t('placeholders.userExample')}
                        disabled={inviteMutation.isPending}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      {t('placeholders.enterEmail')}
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="role"
                render={({ field }) => {
                  const projectRoleOptions = getProjectRoleOptions(t);
                  return (
                    <FormItem>
                      <FormLabel>{t('labels.role')}</FormLabel>
                      <Select
                        onValueChange={field.onChange}
                        value={field.value}
                        disabled={inviteMutation.isPending}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={t('placeholders.selectRole')} />
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
                        {t('descriptions.selectRoleDesc')}
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  );
                }}
              />
            </div>
            <DialogFooter>
              <Button 
                type="button" 
                variant="outline" 
                onClick={onClose} 
                disabled={inviteMutation.isPending}
              >
                {t('buttons.cancel')}
              </Button>
              <Button type="submit" disabled={inviteMutation.isPending}>
                {inviteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                {t('buttons.sendInvitation')}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
