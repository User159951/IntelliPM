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
import { Loader2, AlertTriangle } from 'lucide-react';

interface DeleteUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListDto;
}

export function DeleteUserDialog({ open, onOpenChange, user }: DeleteUserDialogProps) {
  const queryClient = useQueryClient();
  const [confirmText, setConfirmText] = useState('');
  const isConfirmed = confirmText === 'DESACTIVER';

  const deactivateMutation = useMutation({
    mutationFn: (id: number) => usersApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      onOpenChange(false);
      setConfirmText('');
      showSuccess('Utilisateur désactivé', `L'utilisateur "${user.firstName} ${user.lastName}" a été désactivé avec succès.`);
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { error?: string } }; message?: string };
      const errorMessage =
        apiError?.response?.data?.error ||
        apiError?.message ||
        'Veuillez réessayer';
      showError('Échec de la désactivation de l\'utilisateur', errorMessage);
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
              <AlertDialogTitle className="text-xl">Désactiver l'utilisateur ?</AlertDialogTitle>
              <AlertDialogDescription className="mt-2">
                Cette action désactivera le compte utilisateur. L'utilisateur ne pourra plus se connecter, mais cette action peut être annulée par un administrateur.
              </AlertDialogDescription>
            </div>
          </div>
        </AlertDialogHeader>
        <div className="py-4">
          <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 mb-4">
            <p className="text-sm font-medium text-destructive mb-2">
              ⚠️ Désactivation du compte utilisateur
            </p>
            <p className="text-sm text-muted-foreground">
              L'utilisateur <strong>"{user.firstName} {user.lastName}"</strong> ({user.email}) sera désactivé et ne pourra plus se connecter à son compte.
            </p>
            <p className="text-sm text-muted-foreground mt-2">
              Cette action est réversible : un administrateur pourra réactiver le compte ultérieurement.
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirm-delete" className="text-sm font-medium">
              Tapez <span className="font-mono font-bold">DESACTIVER</span> pour confirmer :
            </Label>
            <Input
              id="confirm-delete"
              type="text"
              placeholder="Tapez DESACTIVER pour confirmer"
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
            Annuler
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDeactivate}
            disabled={!isConfirmed || deactivateMutation.isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90 focus:ring-destructive"
          >
            {deactivateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Désactiver
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

