import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { permissionsApi, type PermissionsMatrixDto, type PermissionDto } from '@/api/permissions';
import { GlobalRole } from '@/types';
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '@/components/ui/accordion';
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
import { Loader2, ShieldCheck } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';

type RoleMap = Record<GlobalRole, Set<number>>;

export default function AdminPermissions() {
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
      });
    }
  }, [data, rolePermissions]);

  const togglePermission = (role: GlobalRole, permissionId: number) => {
    if (!rolePermissions) return;
    const next = {
      Admin: new Set(rolePermissions.Admin),
      User: new Set(rolePermissions.User),
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
          },
        });
      }
      return { prev };
    },
    onError: (error, _variables, context) => {
      if (context?.prev) {
        queryClient.setQueryData(['permissions-matrix'], context.prev);
      }
      showError('Failed to save permissions');
    },
    onSuccess: () => {
      showSuccess("Permissions updated", "Role permissions have been saved.");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['permissions-matrix'] });
      setConfirmOpen(false);
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

  const confirmSave = () => {
    if (!pendingPayload) return;
    mutation.mutate(pendingPayload);
  };

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
          <h1 className="text-3xl font-bold mb-1">Permissions Matrix</h1>
          <p className="text-muted-foreground">
            Toggle which permissions apply to Admin and User roles.
          </p>
        </div>
        <Button onClick={handleSave} disabled={mutation.isPending}>
          {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          Save Changes
        </Button>
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center gap-2">
          <ShieldCheck className="h-5 w-5 text-primary" />
          <CardTitle>Role Permissions</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4 pb-3 text-sm font-semibold text-muted-foreground">
            <div>Permission</div>
            <div className="text-center">Admin</div>
            <div className="text-center">User</div>
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
            <AlertDialogTitle>Save permission changes?</AlertDialogTitle>
            <AlertDialogDescription>
              This will replace role-permission mappings for Admin and User.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={mutation.isPending}>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={confirmSave} disabled={mutation.isPending}>
              {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Save
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

