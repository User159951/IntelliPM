import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tasksApi } from '@/api/tasks';
import { sprintsApi } from '@/api/sprints';
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
import { Button } from '@/components/ui/button';
import { format, differenceInDays } from 'date-fns';
import { Sparkles } from 'lucide-react';
import { SprintPlanningAI } from './SprintPlanningAI';
import { showToast } from '@/lib/sweetalert';
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
  const queryClient = useQueryClient();
  const [showAIPlanning, setShowAIPlanning] = useState(false);

  const { data: tasksData } = useQuery({
    queryKey: ['tasks', sprint.projectId],
    queryFn: () => tasksApi.getByProject(sprint.projectId),
    enabled: open,
  });

  const assignTasksMutation = useMutation({
    mutationFn: (taskIds: number[]) => sprintsApi.assignTasks(sprint.id, taskIds),
    onSuccess: (_, taskIds) => {
      queryClient.invalidateQueries({ queryKey: ['tasks', sprint.projectId] });
      queryClient.invalidateQueries({ queryKey: ['sprints', sprint.projectId] });
      showToast(`${taskIds.length} tâches ajoutées au sprint`, 'success');
    },
    onError: () => {
      showToast('Erreur lors de l\'assignation des tâches', 'error');
    },
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
            <div className="space-y-4 mt-2">
              {/* AI Planning Section */}
              {taskCount === 0 && (
                <div className="mb-4">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="text-lg font-semibold">Sélectionner les Tâches</h3>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => setShowAIPlanning(!showAIPlanning)}
                      className="flex items-center gap-2"
                    >
                      <Sparkles className="h-4 w-4 text-purple-500" />
                      {showAIPlanning ? 'Masquer' : 'Afficher'} Planification IA
                    </Button>
                  </div>
                  {showAIPlanning && (
                    <div className="mb-4 p-4 border rounded-lg bg-purple-50 dark:bg-purple-950">
                      <SprintPlanningAI
                        sprintId={sprint.id}
                        onTasksSelected={(taskIds) => {
                          if (taskIds.length > 0) {
                            assignTasksMutation.mutate(taskIds);
                          }
                        }}
                      />
                    </div>
                  )}
                </div>
              )}

              {!canStart ? (
                <div className="space-y-2">
                  {taskCount === 0 && (
                    <p className="text-destructive">⚠️ Sprint must have at least 1 task</p>
                  )}
                  {(!startDate || !endDate) && (
                    <p className="text-destructive">⚠️ Sprint must have dates defined</p>
                  )}
                </div>
              ) : (
                <div className="space-y-3">
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
            </div>
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
