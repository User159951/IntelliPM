import { useEffect, useMemo } from 'react';
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
import { Slider } from '@/components/ui/slider';
import { Loader2 } from 'lucide-react';
import { milestonesApi } from '@/api/milestones';
import { showToast } from '@/lib/sweetalert';
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import type { MilestoneDto } from '@/types/milestones';

interface EditMilestoneDialogProps {
  milestone: MilestoneDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

/**
 * Dialog for editing an existing milestone.
 * Pre-filled with milestone data.
 */
export function EditMilestoneDialog({
  milestone,
  open,
  onOpenChange,
  onSuccess,
}: EditMilestoneDialogProps) {
  const queryClient = useQueryClient();
  const { t } = useTranslation('errors');

  // Create schema inside component so it has access to t function
  const schema = useMemo(() => z.object({
    name: z.string()
      .min(1, t('validation.nameRequired'))
      .max(200, t('validation.nameMaxLength', { max: 200 })),
    description: z.string()
      .max(1000, t('validation.descriptionMaxLength', { max: 1000 }))
      .optional(),
    dueDate: z.string().min(1, t('validation.dueDateRequired')),
    progress: z.number().min(0).max(100),
  }), [t]);

  type FormData = z.infer<typeof schema>;

  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: milestone.name,
      description: milestone.description || '',
      dueDate: new Date(milestone.dueDate).toISOString().slice(0, 16),
      progress: milestone.progress,
    },
  });

  // Update form when milestone changes
  useEffect(() => {
    if (milestone) {
      form.reset({
        name: milestone.name,
        description: milestone.description || '',
        dueDate: new Date(milestone.dueDate).toISOString().slice(0, 16),
        progress: milestone.progress,
      });
    }
  }, [milestone, form]);

  const mutation = useMutation({
    mutationFn: (data: FormData) => milestonesApi.updateMilestone(milestone.id, {
      name: data.name,
      description: data.description,
      dueDate: data.dueDate,
      progress: data.progress,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projectMilestones', milestone.projectId] });
      queryClient.invalidateQueries({ queryKey: ['nextMilestone', milestone.projectId] });
      queryClient.invalidateQueries({ queryKey: ['milestoneStatistics', milestone.projectId] });
      queryClient.invalidateQueries({ queryKey: ['milestone', milestone.id] });
      
      showToast('Milestone updated successfully', 'success');
      onSuccess?.();
      onOpenChange(false);
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { error?: string } }; message?: string };
      const message = apiError?.response?.data?.error || apiError?.message || 'Failed to update milestone';
      showToast(message, 'error');
    },
  });

  const onSubmit = (data: FormData) => {
    mutation.mutate(data);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Edit Milestone</DialogTitle>
          <DialogDescription>
            Update milestone information. Changes will be saved immediately.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input placeholder="Milestone name" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description (Optional)</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Describe the milestone..."
                      className="resize-none"
                      rows={3}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="dueDate"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Due Date</FormLabel>
                  <FormControl>
                    <Input
                      type="datetime-local"
                      {...field}
                    />
                  </FormControl>
                  <FormDescription>
                    The target date when this milestone should be reached.
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="progress"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Progress: {field.value}%</FormLabel>
                  <FormControl>
                    <Slider
                      min={0}
                      max={100}
                      step={1}
                      value={[field.value]}
                      onValueChange={(value) => field.onChange(value[0])}
                    />
                  </FormControl>
                  <FormDescription>
                    Current progress percentage (0-100).
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

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

