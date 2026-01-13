import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi, type UserListDto } from '@/api/users';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
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
import { showSuccess, showError } from '@/lib/sweetalert';
import { useTranslation } from 'react-i18next';
import { Loader2, AlertTriangle } from 'lucide-react';

interface DeactivateUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListDto;
}

export function DeactivateUserDialog({ open, onOpenChange, user }: DeactivateUserDialogProps) {
  const { t } = useTranslation('admin');
  const queryClient = useQueryClient();
  const [confirmText, setConfirmText] = useState('');
  const confirmTextKey = 'DESACTIVER';
  const isConfirmed = confirmText === confirmTextKey;

  const deactivateMutation = useMutation({
    mutationFn: (id: number) => usersApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      onOpenChange(false);
      setConfirmText('');
      showSuccess(t('dialogs.deactivate.success'), t('dialogs.deactivate.successDetail', { name: `${user.firstName} ${user.lastName}` }));
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { error?: string } }; message?: string };
      const errorMessage =
        apiError?.response?.data?.error ||
        apiError?.message ||
        t('dialogs.deactivate.errorMessage');
      showError(t('dialogs.deactivate.error'), errorMessage);
    },
  });

  const handleCancel = () => {
    setConfirmText('');
    onOpenChange(false);
  };

  const handleDeactivate = () => {
    if (!isConfirmed) return;
    deactivateMutation.mutate(user.id);
  };

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent className="sm:max-w-[500px]">
        <AlertDialogHeader>
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-destructive/10">
              <AlertTriangle className="h-5 w-5 text-destructive" />
            </div>
            <div>
              <AlertDialogTitle className="text-xl">{t('dialogs.deactivate.title')}</AlertDialogTitle>
              <AlertDialogDescription className="mt-2">
                {t('dialogs.deactivate.description')}
              </AlertDialogDescription>
            </div>
          </div>
        </AlertDialogHeader>
        <div className="py-4">
          <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 mb-4">
            <p className="text-sm font-medium text-destructive mb-2">
              ⚠️ {t('dialogs.deactivate.warningTitle')}
            </p>
            <p className="text-sm text-muted-foreground">
              {t('dialogs.deactivate.warningMessage', { name: `${user.firstName} ${user.lastName}`, email: user.email })}
            </p>
            <p className="text-sm text-muted-foreground mt-2">
              {t('dialogs.deactivate.reversible')}
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirm-deactivate" className="text-sm font-medium">
              {t('dialogs.deactivate.confirmLabel', { confirmText: confirmTextKey })}
            </Label>
            <Input
              id="confirm-deactivate"
              type="text"
              placeholder={t('dialogs.deactivate.confirmPlaceholder', { confirmText: confirmTextKey })}
              value={confirmText}
              onChange={(e) => setConfirmText(e.target.value)}
              className="font-mono"
              autoFocus
              disabled={deactivateMutation.isPending}
            />
          </div>
        </div>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={handleCancel} disabled={deactivateMutation.isPending}>
            {t('dialogs.deactivate.cancel')}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDeactivate}
            disabled={!isConfirmed || deactivateMutation.isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90 focus:ring-destructive"
          >
            {deactivateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {t('dialogs.deactivate.deactivate')}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
