import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
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
import { Loader2 } from 'lucide-react';
import { milestonesApi } from '@/api/milestones';
import { showToast } from '@/lib/sweetalert';
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import type { MilestoneDto } from '@/types/milestones';

interface CompleteMilestoneDialogProps {
  milestone: MilestoneDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

const schema = z.object({
  completedAt: z.string().optional(),
});

type FormData = z.infer<typeof schema>;

/**
 * Dialog for marking a milestone as completed.
 * Allows setting an optional completion date (defaults to now).
 */
export function CompleteMilestoneDialog({
  milestone,
  open,
  onOpenChange,
  onSuccess,
}: CompleteMilestoneDialogProps) {
  const queryClient = useQueryClient();

  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      completedAt: new Date().toISOString().slice(0, 16),
    },
  });

  const mutation = useMutation({
    mutationFn: (data: FormData) => milestonesApi.completeMilestone(milestone.id, {
      completedAt: data.completedAt ? new Date(data.completedAt).toISOString() : undefined,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projectMilestones', milestone.projectId] });
      queryClient.invalidateQueries({ queryKey: ['nextMilestone', milestone.projectId] });
      queryClient.invalidateQueries({ queryKey: ['milestoneStatistics', milestone.projectId] });
      queryClient.invalidateQueries({ queryKey: ['milestone', milestone.id] });
      
      showToast('Milestone marked as completed', 'success');
      form.reset();
      onSuccess?.();
      onOpenChange(false);
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { error?: string } }; message?: string };
      const message = apiError?.response?.data?.error || apiError?.message || 'Failed to complete milestone';
      showToast(message, 'error');
    },
  });

  const onSubmit = (data: FormData) => {
    mutation.mutate(data);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[400px]">
        <DialogHeader>
          <DialogTitle>Complete Milestone</DialogTitle>
          <DialogDescription>
            Mark "{milestone.name}" as completed. You can optionally set a completion date.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="completedAt"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Completion Date (Optional)</FormLabel>
                  <FormControl>
                    <Input
                      type="datetime-local"
                      {...field}
                    />
                  </FormControl>
                  <FormDescription>
                    Leave empty to use current date and time.
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
                Mark Complete
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

