import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
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
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { showToast, showError } from "@/lib/sweetalert";
import { useDebounce } from '@/hooks/use-debounce';
import { Loader2, AlertTriangle, Search, CheckSquare, Square } from 'lucide-react';
import type { Sprint } from '@/types';

interface AddTasksToSprintDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sprint: Sprint;
}

type PriorityFilter = 'all' | 'Critical' | 'High' | 'Medium' | 'Low';
type SortOption = 'priority' | 'points' | 'title';

const priorityOrder: Record<string, number> = {
  Critical: 4,
  High: 3,
  Medium: 2,
  Low: 1,
};

export function AddTasksToSprintDialog({
  open,
  onOpenChange,
  sprint,
}: AddTasksToSprintDialogProps) {
  const queryClient = useQueryClient();
  const [selectedTaskIds, setSelectedTaskIds] = useState<Set<number>>(new Set());
  const [searchQuery, setSearchQuery] = useState('');
  const [priorityFilter, setPriorityFilter] = useState<PriorityFilter>('all');
  const [sortBy, setSortBy] = useState<SortOption>('priority');
  const debouncedSearch = useDebounce(searchQuery, 300);

  // Fetch all tasks for the project
  const { data: tasksData, isLoading: tasksLoading } = useQuery({
    queryKey: ['tasks', sprint.projectId],
    queryFn: () => tasksApi.getByProject(sprint.projectId),
    enabled: open,
  });

  // Fetch sprint details to get current capacity usage
  useQuery({
    queryKey: ['sprints', sprint.projectId],
    queryFn: () => sprintsApi.getByProject(sprint.projectId),
    enabled: open,
  });

  // Get backlog tasks (not assigned to any sprint)
  const backlogTasks = useMemo(() => {
    if (!tasksData?.tasks) return [];
    return tasksData.tasks.filter((task) => !task.sprintId);
  }, [tasksData]);

  // Filter and sort tasks
  const filteredTasks = useMemo(() => {
    let filtered = backlogTasks;

    // Search filter
    if (debouncedSearch) {
      const query = debouncedSearch.toLowerCase();
      filtered = filtered.filter(
        (task) =>
          task.title.toLowerCase().includes(query) ||
          task.id.toString().includes(query) ||
          task.description?.toLowerCase().includes(query)
      );
    }

    // Priority filter
    if (priorityFilter !== 'all') {
      filtered = filtered.filter((task) => task.priority === priorityFilter);
    }

    // Sort
    filtered = [...filtered].sort((a, b) => {
      switch (sortBy) {
        case 'priority':
          return (priorityOrder[b.priority] || 0) - (priorityOrder[a.priority] || 0);
        case 'points':
          return (b.storyPoints || 0) - (a.storyPoints || 0);
        case 'title':
          return a.title.localeCompare(b.title);
        default:
          return 0;
      }
    });

    return filtered;
  }, [backlogTasks, debouncedSearch, priorityFilter, sortBy]);

  // Calculate current sprint capacity usage
  const currentCapacity = useMemo(() => {
    if (!tasksData?.tasks) return 0;
    const sprintTasks = tasksData.tasks.filter((t) => t.sprintId === sprint.id);
    return sprintTasks.reduce((sum, task) => sum + (task.storyPoints || 0), 0);
  }, [tasksData, sprint.id]);

  // Calculate selected tasks total
  const selectedTotal = useMemo(() => {
    return filteredTasks
      .filter((task) => selectedTaskIds.has(task.id))
      .reduce((sum, task) => sum + (task.storyPoints || 0), 0);
  }, [filteredTasks, selectedTaskIds]);

  const newCapacity = currentCapacity + selectedTotal;
  const exceedsCapacity = newCapacity > sprint.capacity;
  const overBy = exceedsCapacity ? newCapacity - sprint.capacity : 0;
  const availableCapacity = sprint.capacity - currentCapacity;

  const assignMutation = useMutation({
    mutationFn: (taskIds: number[]) => sprintsApi.assignTasks(sprint.id, taskIds),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['sprints'] });
      setSelectedTaskIds(new Set());
      setSearchQuery('');
      onOpenChange(false);
      showToast(`${selectedTaskIds.size} task(s) successfully assigned to ${sprint.name}`, 'success');
    },
    onError: () => {
      showError('Failed to add tasks');
    },
  });

  const handleToggleTask = (taskId: number) => {
    const newSelected = new Set(selectedTaskIds);
    if (newSelected.has(taskId)) {
      newSelected.delete(taskId);
    } else {
      newSelected.add(taskId);
    }
    setSelectedTaskIds(newSelected);
  };

  const handleSelectAll = () => {
    const allIds = new Set(filteredTasks.map((t) => t.id));
    setSelectedTaskIds(allIds);
  };

  const handleDeselectAll = () => {
    setSelectedTaskIds(new Set());
  };

  const handleSubmit = () => {
    if (selectedTaskIds.size === 0) {
      showError("No tasks selected", "Please select at least one task to add to the sprint");
      return;
    }

    if (sprint.status === 'Completed') {
      showError("Cannot add tasks", "Cannot add tasks to a completed sprint");
      return;
    }

    assignMutation.mutate(Array.from(selectedTaskIds));
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'Critical':
        return 'bg-red-500/10 text-red-500 border-red-500/20';
      case 'High':
        return 'bg-orange-500/10 text-orange-500 border-orange-500/20';
      case 'Medium':
        return 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20';
      case 'Low':
        return 'bg-blue-500/10 text-blue-500 border-blue-500/20';
      default:
        return 'bg-muted text-muted-foreground';
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[700px] max-h-[90vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Add Tasks to Sprint {sprint.name}</DialogTitle>
          <DialogDescription>
            Select tasks from the backlog to add to this sprint.
          </DialogDescription>
        </DialogHeader>

        {/* Capacity Info */}
        <div className="p-3 border rounded-lg bg-muted/50">
          <div className="flex items-center justify-between text-sm">
            <span className="font-medium">
              Capacity: {currentCapacity}/{sprint.capacity} SP
            </span>
            <span className="text-muted-foreground">
              {availableCapacity > 0 ? `${availableCapacity} SP available` : 'No capacity available'}
            </span>
          </div>
        </div>

        {/* Search and Filters */}
        <div className="space-y-3">
          <div className="flex gap-2">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search tasks by title or ID..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>
            <Select value={priorityFilter} onValueChange={(v) => setPriorityFilter(v as PriorityFilter)}>
              <SelectTrigger className="w-[140px]">
                <SelectValue placeholder="Priority" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Priorities</SelectItem>
                <SelectItem value="Critical">Critical</SelectItem>
                <SelectItem value="High">High</SelectItem>
                <SelectItem value="Medium">Medium</SelectItem>
                <SelectItem value="Low">Low</SelectItem>
              </SelectContent>
            </Select>
            <Select value={sortBy} onValueChange={(v) => setSortBy(v as SortOption)}>
              <SelectTrigger className="w-[140px]">
                <SelectValue placeholder="Sort by" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="priority">Priority</SelectItem>
                <SelectItem value="points">Story Points</SelectItem>
                <SelectItem value="title">Title</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Select All / Deselect All */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={handleSelectAll}
                disabled={filteredTasks.length === 0}
              >
                <CheckSquare className="mr-2 h-4 w-4" />
                Select All
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={handleDeselectAll}
                disabled={selectedTaskIds.size === 0}
              >
                <Square className="mr-2 h-4 w-4" />
                Deselect All
              </Button>
            </div>
            <span className="text-sm text-muted-foreground">
              {filteredTasks.length} task{filteredTasks.length !== 1 ? 's' : ''} found
            </span>
          </div>
        </div>

        {/* Task List */}
        <div className="flex-1 overflow-y-auto border rounded-lg min-h-[200px] max-h-[400px]">
          {tasksLoading ? (
            <div className="flex items-center justify-center p-8">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : filteredTasks.length === 0 ? (
            <div className="flex flex-col items-center justify-center p-8 text-center">
              <p className="text-muted-foreground">
                {backlogTasks.length === 0
                  ? 'No backlog tasks available'
                  : 'No tasks match your filters'}
              </p>
            </div>
          ) : (
            <div className="divide-y">
              {filteredTasks.map((task) => (
                <div
                  key={task.id}
                  className="flex items-start gap-3 p-3 hover:bg-muted/50 transition-colors"
                >
                  <Checkbox
                    checked={selectedTaskIds.has(task.id)}
                    onCheckedChange={() => handleToggleTask(task.id)}
                    className="mt-1"
                  />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="text-sm font-medium text-muted-foreground">#{task.id}</span>
                      <span className="text-sm font-medium truncate">{task.title}</span>
                      <Badge className={getPriorityColor(task.priority)} variant="outline">
                        {task.priority}
                      </Badge>
                    </div>
                    {task.description && (
                      <p className="text-xs text-muted-foreground line-clamp-1">{task.description}</p>
                    )}
                  </div>
                  <div className="text-sm font-medium text-right whitespace-nowrap">
                    {task.storyPoints || 0} SP
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Selected Summary */}
        {selectedTaskIds.size > 0 && (
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm p-2 bg-muted/50 rounded">
              <span className="font-medium">
                Selected: {selectedTaskIds.size} task{selectedTaskIds.size !== 1 ? 's' : ''} ({selectedTotal} SP)
              </span>
              <span className={exceedsCapacity ? 'text-destructive font-medium' : 'text-muted-foreground'}>
                New capacity: {newCapacity}/{sprint.capacity} SP
              </span>
            </div>
            {exceedsCapacity && (
              <Alert variant="destructive">
                <AlertTriangle className="h-4 w-4" />
                <AlertDescription>
                  Warning: Exceeds capacity by {overBy} SP. Consider removing some tasks or increasing sprint capacity.
                </AlertDescription>
              </Alert>
            )}
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={assignMutation.isPending}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={assignMutation.isPending || selectedTaskIds.size === 0 || sprint.status === 'Completed'}
          >
            {assignMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Add to Sprint
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
