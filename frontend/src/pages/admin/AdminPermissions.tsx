import { useMemo, useState, useCallback } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { permissionsApi, type PermissionsMatrixDto, type PermissionDto } from '@/api/permissions';
import { GlobalRole } from '@/types';
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '@/components/ui/accordion';
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { showError, showSuccess, showWarning } from "@/lib/sweetalert";
import { useTranslation } from 'react-i18next';
import { Loader2, ShieldCheck } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';

type RoleMap = Record<GlobalRole, Set<number>>;

export default function AdminPermissions() {
  const { t } = useTranslation('admin');
  const queryClient = useQueryClient();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [pendingPayload, setPendingPayload] = useState<RoleMap | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['permissions-matrix'],
    queryFn: permissionsApi.getMatrix,
  });

  const [rolePermissions, setRolePermissions] = useState<RoleMap | null>(null);

  // Initialize local state when data loads
  useMemo(() => {
    if (data && !rolePermissions) {
      setRolePermissions({
        Admin: new Set(data.rolePermissions.Admin ?? []),
        User: new Set(data.rolePermissions.User ?? []),
        SuperAdmin: new Set(data.rolePermissions.SuperAdmin ?? []),
      });
    }
  }, [data, rolePermissions]);

  const togglePermission = (role: GlobalRole, permissionId: number) => {
    if (!rolePermissions) return;
    if (role === 'SuperAdmin') return; // Don't allow editing SuperAdmin permissions
    const next = {
      Admin: new Set(rolePermissions.Admin),
      User: new Set(rolePermissions.User),
      SuperAdmin: new Set(rolePermissions.SuperAdmin),
    };
    const set = role === 'Admin' ? next.Admin : next.User;
    if (set.has(permissionId)) {
      set.delete(permissionId);
    } else {
      set.add(permissionId);
    }
    setRolePermissions(next);
  };

  const mutation = useMutation({
    mutationFn: async (payload: RoleMap) => {
      await permissionsApi.updateRolePermissions('Admin', Array.from(payload.Admin));
      await permissionsApi.updateRolePermissions('User', Array.from(payload.User));
    },
    onMutate: async (payload) => {
      await queryClient.cancelQueries({ queryKey: ['permissions-matrix'] });
      const prev = queryClient.getQueryData<PermissionsMatrixDto>(['permissions-matrix']);
      if (prev) {
        queryClient.setQueryData<PermissionsMatrixDto>(['permissions-matrix'], {
          ...prev,
          rolePermissions: {
            Admin: Array.from(payload.Admin),
            User: Array.from(payload.User),
            SuperAdmin: Array.from(payload.SuperAdmin),
          },
        });
      }
      return { prev };
    },
    onError: (_error, _variables, context) => {
      if (context?.prev) {
        queryClient.setQueryData(['permissions-matrix'], context.prev);
      }
      // Don't close dialog on error - let confirmSave handle it
    },
    onSuccess: () => {
      showSuccess(t('permissions.messages.saveSuccess'), t('permissions.messages.saveSuccessDetail'));
      setConfirmOpen(false);
      queryClient.invalidateQueries({ queryKey: ['permissions-matrix'] });
    },
  });

  const grouped = useMemo(() => {
    if (!data) return {};
    return data.permissions.reduce<Record<string, PermissionDto[]>>((acc, perm) => {
      if (!acc[perm.category]) acc[perm.category] = [];
      acc[perm.category].push(perm);
      return acc;
    }, {});
  }, [data]);

  const handleSave = () => {
    if (!rolePermissions) return;
    setPendingPayload(rolePermissions);
    setConfirmOpen(true);
  };

  const confirmSave = useCallback(async () => {
    if (!pendingPayload) {
      showWarning(t('permissions.messages.noChanges'));
      return;
    }
    
    if (mutation.isPending) {
      return; // Prevent double-click
    }

    try {
      await mutation.mutateAsync(pendingPayload);
      // Success handled in mutation.onSuccess
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : t('permissions.messages.saveError');
      showError(t('permissions.messages.saveError'), errorMessage);
      // Don't close dialog on error - it stays open
    }
  }, [pendingPayload, mutation, t]);

  if (isLoading || !rolePermissions || !data) {
    return (
      <div className="container mx-auto p-6 space-y-4">
        <Skeleton className="h-10 w-48" />
        <Skeleton className="h-6 w-64" />
        <div className="space-y-2">
          {[...Array(4)].map((_, idx) => (
            <Skeleton key={idx} className="h-14 w-full" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold mb-1">{t('permissions.title')}</h1>
          <p className="text-muted-foreground">
            {t('permissions.description')}
          </p>
        </div>
        <Button onClick={handleSave} disabled={mutation.isPending}>
          {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {t('permissions.saveChanges')}
        </Button>
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center gap-2">
          <ShieldCheck className="h-5 w-5 text-primary" />
          <CardTitle>Role Permissions</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4 pb-3 text-sm font-semibold text-muted-foreground">
            <div>{t('permissions.matrix.permission')}</div>
            <div className="text-center">{t('permissions.matrix.admin')}</div>
            <div className="text-center">{t('permissions.matrix.user')}</div>
          </div>
          <Accordion type="multiple" className="w-full" defaultValue={Object.keys(grouped)}>
            {Object.entries(grouped).map(([category, perms]) => (
              <AccordionItem key={category} value={category}>
                <AccordionTrigger className="text-left">{category}</AccordionTrigger>
                <AccordionContent>
                  <div className="space-y-2">
                    {perms.map((perm) => (
                      <div
                        key={perm.id}
                        className="grid grid-cols-3 items-center gap-4 rounded-lg border p-3"
                      >
                        <div>
                          <div className="font-medium">{perm.name}</div>
                          {perm.description && (
                            <div className="text-sm text-muted-foreground">{perm.description}</div>
                          )}
                        </div>
                        <div className="flex justify-center">
                          <Checkbox
                            aria-label={`${perm.name}-Admin`}
                            checked={rolePermissions.Admin.has(perm.id)}
                            onCheckedChange={() => togglePermission('Admin', perm.id)}
                          />
                        </div>
                        <div className="flex justify-center">
                          <Checkbox
                            aria-label={`${perm.name}-User`}
                            checked={rolePermissions.User.has(perm.id)}
                            onCheckedChange={() => togglePermission('User', perm.id)}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </AccordionContent>
              </AccordionItem>
            ))}
          </Accordion>
        </CardContent>
      </Card>

      <AlertDialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('permissions.confirmDialog.title')}</AlertDialogTitle>
            <AlertDialogDescription>
              {t('permissions.confirmDialog.description')}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={mutation.isPending}>{t('permissions.confirmDialog.cancel')}</AlertDialogCancel>
            <AlertDialogAction onClick={confirmSave} disabled={mutation.isPending || !pendingPayload}>
              {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {mutation.isPending ? t('permissions.confirmDialog.saving') : t('permissions.confirmDialog.confirm')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

