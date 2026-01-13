import { useEffect, useMemo, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from '@/hooks/useTranslation';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Loader2 } from 'lucide-react';
import { releasesApi } from '@/api/releases';
import { showToast } from '@/lib/sweetalert';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import type { ReleaseDto } from '@/types/releases';
import { format } from 'date-fns';
import { ReleaseStatus } from '@/types/generated/enums';

interface EditReleaseDialogProps {
  release: ReleaseDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

export function EditReleaseDialog({
  release,
  open,
  onOpenChange,
  onSuccess,
}: EditReleaseDialogProps) {
  const queryClient = useQueryClient();
  const { t } = useTranslation('errors');

  // Create schema inside component so it has access to t function
  const schema = useMemo(() => z.object({
    name: z.string()
      .min(1, t('validation.nameRequired'))
      .max(200, t('validation.nameMaxLength', { max: 200 })),
    description: z.string()
      .max(2000, t('validation.descriptionMaxLength', { max: 2000 }))
      .optional(),
    plannedDate: z.string().min(1, t('validation.plannedDateRequired')),
    status: z.enum(['Planned', 'InProgress', 'Testing', 'ReadyForDeployment', 'Cancelled'] as [ReleaseStatus, ...ReleaseStatus[]]),
    isPreRelease: z.boolean(),
    tagName: z.string()
      .max(100, t('validation.tagNameMaxLength', { max: 100 }))
      .optional(),
  }), [t]);

  type FormData = z.infer<typeof schema>;

  const getValidStatus = useCallback((status: string): FormData['status'] => {
    const validStatuses: FormData['status'][] = ['Planned', 'InProgress', 'Testing', 'ReadyForDeployment', 'Cancelled'];
    return validStatuses.includes(status as FormData['status']) ? (status as FormData['status']) : 'Planned';
  }, []);

  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: release.name,
      description: release.description || '',
      plannedDate: release.plannedDate ? format(new Date(release.plannedDate), 'yyyy-MM-dd') : '',
      status: getValidStatus(release.status),
      isPreRelease: release.isPreRelease,
      tagName: release.tagName || '',
    },
  });

  // Reset form when release changes
  useEffect(() => {
    if (open && release) {
      form.reset({
        name: release.name,
        description: release.description || '',
        plannedDate: release.plannedDate ? format(new Date(release.plannedDate), 'yyyy-MM-dd') : '',
        status: getValidStatus(release.status),
        isPreRelease: release.isPreRelease,
        tagName: release.tagName || '',
      });
    }
  }, [open, release, form, getValidStatus]);

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      releasesApi.updateRelease(release.id, {
        name: data.name,
        version: release.version, // Version cannot be changed
        description: data.description,
        plannedDate: data.plannedDate,
        status: data.status,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projectReleases', release.projectId] });
      queryClient.invalidateQueries({ queryKey: ['release', release.id] });
      queryClient.invalidateQueries({ queryKey: ['releaseStatistics', release.projectId] });
      showToast('Release updated successfully', 'success');
      onSuccess?.();
      onOpenChange(false);
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { error?: string } }; message?: string };
      const message = apiError?.response?.data?.error || apiError?.message || 'Failed to update release';
      showToast(message, 'error');
    },
  });

  const onSubmit = (data: FormData) => {
    mutation.mutate(data);
  };

  // Determine which statuses can be selected
  const getAvailableStatuses = () => {
    const currentStatus = release.status;
    const allStatuses = ['Planned', 'InProgress', 'Testing', 'ReadyForDeployment', 'Cancelled'];
    
    // If already deployed or failed, can only view (but we'll allow Cancelled)
    if (currentStatus === 'Deployed' || currentStatus === 'Failed') {
      return ['Cancelled'];
    }
    
    return allStatuses;
  };

  const availableStatuses = getAvailableStatuses();
  const isDeployedOrFailed = release.status === 'Deployed' || release.status === 'Failed';
  const isReadOnly = isDeployedOrFailed;

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'Major':
        return 'bg-red-500 text-white';
      case 'Minor':
        return 'bg-blue-500 text-white';
      case 'Patch':
        return 'bg-green-500 text-white';
      case 'Hotfix':
        return 'bg-orange-500 text-white';
      default:
        return 'bg-gray-500 text-white';
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Edit Release</DialogTitle>
          <DialogDescription>
            Update release details. Version cannot be changed after creation.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {/* Version Display (Read-only) */}
            <div className="space-y-2">
              <Label>Version</Label>
              <div className="flex items-center gap-2">
                <Input value={release.version} disabled className="flex-1" />
                <Badge className={getTypeColor(release.type)}>{release.type}</Badge>
              </div>
              <p className="text-sm text-muted-foreground">
                Version cannot be changed after creation
              </p>
            </div>

            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g., Summer Release 2024" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="status"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Status</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={isReadOnly}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select status" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {availableStatuses.map((status) => (
                          <SelectItem key={status} value={status}>
                            {status}
                          </SelectItem>
                        ))}
                        {/* Show current status even if not in available list */}
                        {!availableStatuses.includes(release.status) && (
                          <SelectItem value={release.status} disabled>
                            {release.status} (Current)
                          </SelectItem>
                        )}
                      </SelectContent>
                    </Select>
                    {isReadOnly && (
                      <FormDescription>
                        Status cannot be changed for deployed or failed releases
                      </FormDescription>
                    )}
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="plannedDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Planned Date</FormLabel>
                    <FormControl>
                      <Input
                        type="date"
                        {...field}
                        disabled={release.status === 'Deployed'}
                      />
                    </FormControl>
                    {release.status === 'Deployed' && (
                      <FormDescription>
                        Planned date cannot be changed for deployed releases
                      </FormDescription>
                    )}
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Describe what's included in this release..."
                      className="min-h-[100px]"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="tagName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Git Tag</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g., v2.1.0" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="isPreRelease"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                    <FormControl>
                      <Checkbox
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                    <div className="space-y-1 leading-none">
                      <FormLabel>Pre-Release</FormLabel>
                      <FormDescription>Alpha, Beta, or Release Candidate</FormDescription>
                    </div>
                  </FormItem>
                )}
              />
            </div>

            {/* Current Status Display (if deployed or failed) */}
            {(release.status === 'Deployed' || release.status === 'Failed') && (
              <div className="p-4 border rounded-lg bg-muted/50">
                <div className="text-sm">
                  <strong>Current Status:</strong> {release.status}
                  {release.status === 'Deployed' && release.actualReleaseDate && (
                    <span className="ml-2 text-muted-foreground">
                      (Deployed on {format(new Date(release.actualReleaseDate), 'PPp')})
                    </span>
                  )}
                </div>
                <p className="text-sm text-muted-foreground mt-1">
                  Deployed or failed releases have limited editing capabilities.
                </p>
              </div>
            )}

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={mutation.isPending}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Save Changes
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

