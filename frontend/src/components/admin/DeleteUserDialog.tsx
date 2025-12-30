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
import { showToast, showError } from '@/lib/sweetalert';
import { Loader2, AlertTriangle } from 'lucide-react';

interface DeleteUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListDto;
}

export function DeleteUserDialog({ open, onOpenChange, user }: DeleteUserDialogProps) {
  const queryClient = useQueryClient();
  const [confirmText, setConfirmText] = useState('');
  const isConfirmed = confirmText === 'DELETE';

  const deleteMutation = useMutation({
    mutationFn: (id: number) => usersApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      onOpenChange(false);
      setConfirmText('');
      showToast(`"${user.firstName} ${user.lastName}" has been successfully deleted.`, 'success');
    },
    onError: (error: any) => {
      const errorMessage =
        error?.response?.data?.error ||
        error?.message ||
        'Please try again';
      showError('Failed to delete user', errorMessage);
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
              <AlertDialogTitle className="text-xl">Delete user?</AlertDialogTitle>
              <AlertDialogDescription className="mt-2">
                This will deactivate the user account. This action can be undone by an admin.
              </AlertDialogDescription>
            </div>
          </div>
        </AlertDialogHeader>
        <div className="py-4">
          <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 mb-4">
            <p className="text-sm font-medium text-destructive mb-2">
              ⚠️ This will deactivate the user account
            </p>
            <p className="text-sm text-muted-foreground">
              The user <strong>"{user.firstName} {user.lastName}"</strong> ({user.email}) will be
              deactivated and will not be able to log in.
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirm-delete" className="text-sm font-medium">
              Type <span className="font-mono font-bold">DELETE</span> to confirm:
            </Label>
            <Input
              id="confirm-delete"
              type="text"
              placeholder="Type DELETE to confirm"
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
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            disabled={!isConfirmed || deleteMutation.isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90 focus:ring-destructive"
          >
            {deleteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Delete User
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

