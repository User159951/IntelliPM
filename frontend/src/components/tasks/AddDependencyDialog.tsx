import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Loader2 } from 'lucide-react';
import { dependenciesApi } from '@/api/dependencies';
import { tasksApi } from '@/api/tasks';
import { MySwal } from '@/lib/sweetalert';
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { DependencyType } from '@/types/generated/enums';

interface AddDependencyDialogProps {
  taskId: number;
  projectId: number;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

const dependencyTypeSchema = z.enum(['FinishToStart', 'StartToStart', 'FinishToFinish', 'StartToFinish'] as [DependencyType, ...DependencyType[]]);

const schema = z.object({
  dependentTaskId: z.number().min(1, 'Please select a task'),
  dependencyType: dependencyTypeSchema,
});

type FormData = z.infer<typeof schema>;

/**
 * Dialog for adding a task dependency.
 * Allows selecting a dependent task and dependency type.
 */
export function AddDependencyDialog({
  taskId,
  projectId,
  open,
  onOpenChange,
  onSuccess,
}: AddDependencyDialogProps) {
  const queryClient = useQueryClient();

  // Fetch tasks from the same project
  const { data: tasksData, isLoading: isLoadingTasks } = useQuery({
    queryKey: ['projectTasks', projectId],
    queryFn: () => tasksApi.getByProject(projectId),
    enabled: open && !!projectId,
  });

  // Filter out current task
  const availableTasks = tasksData?.tasks?.filter((t) => t.id !== taskId) ?? [];

  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      dependentTaskId: 0,
      dependencyType: 'FinishToStart',
    },
  });

  // Add dependency mutation
  const mutation = useMutation({
    mutationFn: (data: FormData) => dependenciesApi.addTaskDependency(taskId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskDependencies', taskId] });
      queryClient.invalidateQueries({ queryKey: ['projectDependencyGraph', projectId] });
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
      
      MySwal.fire({
        icon: 'success',
        title: 'Dependency added!',
        timer: 2000,
        showConfirmButton: false,
      });
      
      form.reset();
      onSuccess?.();
      onOpenChange(false);
    },
    onError: (error: unknown) => {
      const apiError = error as { response?: { data?: { message?: string } }; message?: string };
      const message = apiError?.response?.data?.message || apiError?.message || 'Failed to add dependency';
      MySwal.fire({
        icon: 'error',
        title: 'Error',
        text: message,
      });
    },
  });

  const onSubmit = (data: FormData) => {
    // Additional validation: ensure dependentTaskId is not the same as taskId
    if (data.dependentTaskId === taskId) {
      form.setError('dependentTaskId', {
        type: 'manual',
        message: 'Cannot create dependency with itself',
      });
      return;
    }

    mutation.mutate(data);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Add Task Dependency</DialogTitle>
          <DialogDescription>
            Create a dependency relationship between tasks. The current task will depend on the selected task.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="dependentTaskId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Dependent Task</FormLabel>
                  <Select
                    onValueChange={(value) => field.onChange(parseInt(value, 10))}
                    value={field.value?.toString() || ''}
                    disabled={isLoadingTasks || mutation.isPending}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select a task" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {isLoadingTasks ? (
                        <SelectItem value="loading" disabled>
                          Loading tasks...
                        </SelectItem>
                      ) : availableTasks.length === 0 ? (
                        <SelectItem value="none" disabled>
                          No tasks available
                        </SelectItem>
                      ) : (
                        availableTasks.map((task) => (
                          <SelectItem key={task.id} value={task.id.toString()}>
                            {task.title} - {task.status}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                  <FormDescription>
                    Select the task that this task depends on
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="dependencyType"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Dependency Type</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    value={field.value}
                    disabled={mutation.isPending}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="FinishToStart">
                        <div>
                          <div className="font-medium">Finish-to-Start (FS)</div>
                          <div className="text-xs text-muted-foreground">
                            Task starts when this task finishes (most common)
                          </div>
                        </div>
                      </SelectItem>
                      <SelectItem value="StartToStart">
                        <div>
                          <div className="font-medium">Start-to-Start (SS)</div>
                          <div className="text-xs text-muted-foreground">
                            Task starts when this task starts
                          </div>
                        </div>
                      </SelectItem>
                      <SelectItem value="FinishToFinish">
                        <div>
                          <div className="font-medium">Finish-to-Finish (FF)</div>
                          <div className="text-xs text-muted-foreground">
                            Task finishes when this task finishes
                          </div>
                        </div>
                      </SelectItem>
                      <SelectItem value="StartToFinish">
                        <div>
                          <div className="font-medium">Start-to-Finish (SF)</div>
                          <div className="text-xs text-muted-foreground">
                            Task finishes when this task starts (rare)
                          </div>
                        </div>
                      </SelectItem>
                    </SelectContent>
                  </Select>
                  <FormDescription>
                    Choose the type of dependency relationship
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
              <Button type="submit" disabled={mutation.isPending || isLoadingTasks}>
                {mutation.isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                Add Dependency
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}

