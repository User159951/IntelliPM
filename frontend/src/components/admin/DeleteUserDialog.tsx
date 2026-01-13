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

interface DeleteUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListDto;
}

export function DeleteUserDialog({ open, onOpenChange, user }: DeleteUserDialogProps) {
  const { t } = useTranslation('admin');
  const queryClient = useQueryClient();
  const [confirmText, setConfirmText] = useState('');
  const confirmTextKey = 'DELETE';
  const isConfirmed = confirmText === confirmTextKey;

  const deleteMutation = useMutation({
    mutationFn: (id: number) => usersApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      onOpenChange(false);
      setConfirmText('');
      showSuccess(t('dialogs.delete.success'), t('dialogs.delete.successDetail', { name: `${user.firstName} ${user.lastName}` }));
    },
    onError: (error: unknown) => {
      // The API client throws an Error with the message
      const errorMessage = error instanceof Error 
        ? error.message 
        : typeof error === 'string'
        ? error
        : (error && typeof error === 'object' && 'message' in error && typeof error.message === 'string')
        ? error.message
        : t('dialogs.delete.errorMessage');
      showError(t('dialogs.delete.error'), errorMessage);
    },
  });

  const handleCancel = () => {
    setConfirmText('');
    onOpenChange(false);
  };

  const handleDelete = () => {
    if (!isConfirmed) return;
    deleteMutation.mutate(user.id);
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
              <AlertDialogTitle className="text-xl">{t('dialogs.delete.title')}</AlertDialogTitle>
              <AlertDialogDescription className="mt-2">
                {t('dialogs.delete.description')}
              </AlertDialogDescription>
            </div>
          </div>
        </AlertDialogHeader>
        <div className="py-4">
          <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 mb-4">
            <p className="text-sm font-medium text-destructive mb-2">
              ⚠️ {t('dialogs.delete.warningTitle')}
            </p>
            <p className="text-sm text-muted-foreground">
              {t('dialogs.delete.warningMessage', { name: `${user.firstName} ${user.lastName}`, email: user.email })}
            </p>
            <p className="text-sm text-destructive mt-2 font-medium">
              {t('dialogs.delete.irreversible')}
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirm-delete" className="text-sm font-medium">
              {t('dialogs.delete.confirmLabel', { confirmText: confirmTextKey })}
            </Label>
            <Input
              id="confirm-delete"
              type="text"
              placeholder={t('dialogs.delete.confirmPlaceholder', { confirmText: confirmTextKey })}
              value={confirmText}
              onChange={(e) => setConfirmText(e.target.value)}
              className="font-mono"
              autoFocus
              disabled={deleteMutation.isPending}
            />
          </div>
        </div>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={handleCancel} disabled={deleteMutation.isPending}>
            {t('dialogs.delete.cancel')}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            disabled={!isConfirmed || deleteMutation.isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90 focus:ring-destructive"
          >
            {deleteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {t('dialogs.delete.delete')}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

