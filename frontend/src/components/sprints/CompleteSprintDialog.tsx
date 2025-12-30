import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { tasksApi } from '@/api/tasks';
import { sprintsApi } from '@/api/sprints';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Label } from '@/components/ui/label';
import { CheckCircle2, AlertTriangle, Loader2 } from 'lucide-react';
import type { Sprint } from '@/types';

interface CompleteSprintDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sprint: Sprint;
  onConfirm: (incompleteTasksAction: 'next_sprint' | 'backlog' | 'keep') => void;
  isLoading?: boolean;
}

export function CompleteSprintDialog({
  open,
  onOpenChange,
  sprint,
  onConfirm,
  isLoading = false,
}: CompleteSprintDialogProps) {
  const [incompleteTasksAction, setIncompleteTasksAction] = useState<'next_sprint' | 'backlog' | 'keep'>('backlog');

  const { data: tasksData } = useQuery({
    queryKey: ['tasks', sprint.projectId],
    queryFn: () => tasksApi.getByProject(sprint.projectId),
    enabled: open,
  });

  const { data: sprintsData } = useQuery({
    queryKey: ['sprints', sprint.projectId],
    queryFn: () => sprintsApi.getByProject(sprint.projectId),
    enabled: open,
  });

  const sprintTasks = tasksData?.tasks?.filter((t) => t.sprintId === sprint.id) || [];
  const completedTasks = sprintTasks.filter((t) => t.status === 'Done');
  const incompleteTasks = sprintTasks.filter((t) => t.status !== 'Done');
  
  const totalStoryPoints = sprintTasks.reduce((sum, task) => sum + (task.storyPoints || 0), 0);
  const completedStoryPoints = completedTasks.reduce((sum, task) => sum + (task.storyPoints || 0), 0);
  
  const velocity = completedStoryPoints;
  const completionRate = sprintTasks.length > 0 ? (completedTasks.length / sprintTasks.length) * 100 : 0;

  const nextSprint = sprintsData?.sprints?.find((s) => s.status === 'Planned' && s.id !== sprint.id);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Complete Sprint {sprint.name}?</DialogTitle>
          <DialogDescription>
            Review the sprint summary and decide what to do with incomplete tasks.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* Summary */}
          <div className="space-y-2 p-4 border rounded-lg bg-muted/50">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-green-500" />
              <span className="text-sm font-medium">
                Tasks completed: {completedTasks.length}/{sprintTasks.length}
              </span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-green-500" />
              <span className="text-sm font-medium">
                Story points: {completedStoryPoints}/{totalStoryPoints}
              </span>
            </div>
            {incompleteTasks.length > 0 && (
              <div className="flex items-center gap-2">
                <AlertTriangle className="h-4 w-4 text-orange-500" />
                <span className="text-sm font-medium text-orange-600 dark:text-orange-400">
                  Incomplete tasks: {incompleteTasks.length}
                </span>
              </div>
            )}
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 gap-4 p-4 border rounded-lg">
            <div>
              <p className="text-xs text-muted-foreground">Velocity</p>
              <p className="text-lg font-semibold">{velocity} SP</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Completion Rate</p>
              <p className="text-lg font-semibold">{completionRate.toFixed(0)}%</p>
            </div>
          </div>

          {/* Incomplete tasks action */}
          {incompleteTasks.length > 0 && (
            <div className="space-y-3">
              <Label className="text-sm font-medium">What to do with incomplete tasks?</Label>
              <RadioGroup
                value={incompleteTasksAction}
                onValueChange={(value) => setIncompleteTasksAction(value as typeof incompleteTasksAction)}
              >
                <div className="flex items-start space-x-2 p-3 border rounded-lg hover:bg-muted/50">
                  <RadioGroupItem value="next_sprint" id="next_sprint" className="mt-1" />
                  <Label htmlFor="next_sprint" className="flex-1 cursor-pointer">
                    <div className="font-medium">Move to next sprint</div>
                    <div className="text-xs text-muted-foreground">
                      {nextSprint
                        ? `Tasks will be moved to ${nextSprint.name}`
                        : 'No planned sprint found. Tasks will be moved to backlog.'}
                    </div>
                  </Label>
                </div>
                <div className="flex items-start space-x-2 p-3 border rounded-lg hover:bg-muted/50">
                  <RadioGroupItem value="backlog" id="backlog" className="mt-1" />
                  <Label htmlFor="backlog" className="flex-1 cursor-pointer">
                    <div className="font-medium">Move to backlog</div>
                    <div className="text-xs text-muted-foreground">
                      Tasks will be unassigned from this sprint
                    </div>
                  </Label>
                </div>
                <div className="flex items-start space-x-2 p-3 border rounded-lg hover:bg-muted/50 border-orange-200 dark:border-orange-800">
                  <RadioGroupItem value="keep" id="keep" className="mt-1" />
                  <Label htmlFor="keep" className="flex-1 cursor-pointer">
                    <div className="font-medium">Keep in current sprint</div>
                    <div className="text-xs text-muted-foreground text-orange-600 dark:text-orange-400">
                      Not recommended - tasks will remain in completed sprint
                    </div>
                  </Label>
                </div>
              </RadioGroup>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isLoading}>
            Cancel
          </Button>
          <Button onClick={() => onConfirm(incompleteTasksAction)} disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Complete Sprint
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
