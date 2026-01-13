import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsApi } from '@/api/projects';
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
import type { Project } from '@/types';
import { useTranslation } from 'react-i18next';

interface DeleteProjectDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  project: Project;
  onDeleted?: () => void;
}

export function DeleteProjectDialog({ open, onOpenChange, project, onDeleted }: DeleteProjectDialogProps) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { t } = useTranslation('common');
  const [confirmText, setConfirmText] = useState('');
  const isConfirmed = confirmText === 'DELETE';

  const deleteMutation = useMutation({
    mutationFn: () => projectsApi.deletePermanent(project.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['project', project.id] });
      onOpenChange(false);
      setConfirmText('');
      showToast(`"${project.name}" has been permanently deleted.`, 'success');
      if (onDeleted) {
        onDeleted();
      } else {
        navigate('/projects');
      }
    },
    onError: (error) => {
      showError(
        'Failed to delete project',
        error instanceof Error ? error.message : 'Please try again'
      );
    },
  });

  const handleCancel = () => {
    setConfirmText('');
    onOpenChange(false);
  };

  const handleDelete = () => {
    if (!isConfirmed) return;
    deleteMutation.mutate();
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
              <AlertDialogTitle className="text-xl">{t('confirmations.deleteProjectTitle')}</AlertDialogTitle>
              <AlertDialogDescription className="mt-2">
                {t('confirmations.deleteMessage')}
              </AlertDialogDescription>
            </div>
          </div>
        </AlertDialogHeader>
        <div className="py-4">
          <div className="rounded-lg border border-destructive/20 bg-destructive/5 p-4 mb-4">
            <p className="text-sm font-medium text-destructive mb-2">
              ⚠️ {t('confirmations.destructiveAction')}
            </p>
            <p className="text-sm text-muted-foreground">
              {t('confirmations.deleteProjectMessage')} <strong>"{project.name}"</strong>
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirm-delete" className="text-sm font-medium">
              {t('confirmations.deleteConfirmText')}
            </Label>
            <Input
              id="confirm-delete"
              type="text"
              placeholder={t('confirmations.deleteConfirmPlaceholder')}
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
            {t('buttons.cancel')}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            disabled={!isConfirmed || deleteMutation.isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90 focus:ring-destructive"
          >
            {deleteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {t('buttons.deleteForever')}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
