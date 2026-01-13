import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi, type UserListDto, type UpdateUserRequest } from '@/api/users';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { showSuccess, showError } from "@/lib/sweetalert";
import { useTranslation } from 'react-i18next';
import { Loader2 } from 'lucide-react';

interface EditUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListDto;
}

export function EditUserDialog({ open, onOpenChange, user }: EditUserDialogProps) {
  const { t } = useTranslation('admin');
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<UpdateUserRequest>({
    firstName: user.firstName,
    lastName: user.lastName,
    email: user.email,
    globalRole: user.globalRole,
  });

  // Update form data when user changes
  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        globalRole: user.globalRole,
      });
    }
  }, [user]);

  const updateMutation = useMutation({
    mutationFn: (data: UpdateUserRequest) => usersApi.update(user.id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      onOpenChange(false);
      showSuccess(t('dialogs.edit.success'), t('dialogs.edit.successDetail'));
    },
    onError: () => {
      showError(t('dialogs.edit.error'));
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Only send fields that have changed
    const changes: UpdateUserRequest = {};
    if (formData.firstName !== user.firstName) changes.firstName = formData.firstName;
    if (formData.lastName !== user.lastName) changes.lastName = formData.lastName;
    if (formData.email !== user.email) changes.email = formData.email;
    if (formData.globalRole !== user.globalRole) changes.globalRole = formData.globalRole;

    // If no changes, just close the dialog
    if (Object.keys(changes).length === 0) {
      onOpenChange(false);
      return;
    }

    updateMutation.mutate(changes);
  };

  const handleCancel = () => {
    setFormData({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      globalRole: user.globalRole,
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{t('dialogs.edit.title')}</DialogTitle>
            <DialogDescription>
              {t('dialogs.edit.description')}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-firstName">{t('dialogs.edit.firstName')}</Label>
                <Input
                  id="edit-firstName"
                  name="firstName"
                  autoComplete="given-name"
                  placeholder={t('dialogs.edit.firstName')}
                  value={formData.firstName || ''}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                  maxLength={100}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-lastName">{t('dialogs.edit.lastName')}</Label>
                <Input
                  id="edit-lastName"
                  name="lastName"
                  autoComplete="family-name"
                  placeholder={t('dialogs.edit.lastName')}
                  value={formData.lastName || ''}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                  maxLength={100}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-email">{t('dialogs.edit.email')}</Label>
              <Input
                id="edit-email"
                name="email"
                type="email"
                autoComplete="email"
                placeholder="email@example.com"
                value={formData.email || ''}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                maxLength={255}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-role">{t('dialogs.edit.role')}</Label>
              <Select
                value={formData.globalRole || 'User'}
                onValueChange={(value) => setFormData({ ...formData, globalRole: value })}
              >
                <SelectTrigger id="edit-role">
                  <SelectValue placeholder={t('dialogs.edit.rolePlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="User">User</SelectItem>
                  <SelectItem value="Admin">Admin</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleCancel} disabled={updateMutation.isPending}>
              {t('dialogs.edit.cancel')}
            </Button>
            <Button type="submit" disabled={updateMutation.isPending}>
              {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {t('dialogs.edit.save')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

