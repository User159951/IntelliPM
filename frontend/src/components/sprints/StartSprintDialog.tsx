import { useQuery } from '@tanstack/react-query';
import { tasksApi } from '@/api/tasks';
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
import { format, differenceInDays } from 'date-fns';
import type { Sprint } from '@/types';

interface StartSprintDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sprint: Sprint;
  onConfirm: () => void;
  isLoading?: boolean;
}

export function StartSprintDialog({
  open,
  onOpenChange,
  sprint,
  onConfirm,
  isLoading = false,
}: StartSprintDialogProps) {
  const { data: tasksData } = useQuery({
    queryKey: ['tasks', sprint.projectId],
    queryFn: () => tasksApi.getByProject(sprint.projectId),
    enabled: open,
  });

  const sprintTasks = tasksData?.tasks?.filter((t) => t.sprintId === sprint.id) || [];
  const taskCount = sprintTasks.length;
  const storyPoints = sprintTasks.reduce((sum, task) => sum + (task.storyPoints || 0), 0);

  const startDate = sprint.startDate ? new Date(sprint.startDate) : null;
  const endDate = sprint.endDate ? new Date(sprint.endDate) : null;
  const duration = startDate && endDate ? differenceInDays(endDate, startDate) : null;

  // Calculate team capacity (approximate: 1 SP per member per day)
  const capacity = sprint.capacity || 0;

  const canStart = taskCount > 0 && startDate && endDate;

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Start Sprint {sprint.name}?</AlertDialogTitle>
          <AlertDialogDescription>
            {!canStart ? (
              <div className="space-y-2 mt-2">
                {taskCount === 0 && (
                  <p className="text-destructive">⚠️ Sprint must have at least 1 task</p>
                )}
                {(!startDate || !endDate) && (
                  <p className="text-destructive">⚠️ Sprint must have dates defined</p>
                )}
              </div>
            ) : (
              <div className="space-y-3 mt-4">
                <div className="space-y-1">
                  <p className="font-medium">Sprint Summary:</p>
                  <ul className="list-disc list-inside space-y-1 text-sm text-muted-foreground">
                    <li>
                      <strong>{taskCount}</strong> {taskCount === 1 ? 'task' : 'tasks'} ({storyPoints} story points)
                    </li>
                    {startDate && endDate && (
                      <li>
                        Duration: {format(startDate, 'MMM d, yyyy')} to {format(endDate, 'MMM d, yyyy')} ({duration} {duration === 1 ? 'day' : 'days'})
                      </li>
                    )}
                    <li>Team capacity: {capacity} SP</li>
                  </ul>
                </div>
                <p className="text-sm">This will mark the sprint as active and notify all team members.</p>
              </div>
            )}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction onClick={onConfirm} disabled={!canStart || isLoading}>
            {isLoading ? 'Starting...' : 'Start Sprint'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
