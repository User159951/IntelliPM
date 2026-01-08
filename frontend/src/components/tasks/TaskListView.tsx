import { useState, useMemo } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { tasksApi } from '@/api/tasks';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
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
import { showToast, showSuccess, showError } from "@/lib/sweetalert";
import { ArrowUpDown, MoreHorizontal, Trash2, Edit, ArrowUp, ArrowDown } from 'lucide-react';
import { format } from 'date-fns';
import type { Task, TaskStatus, TaskPriority } from '@/types';
import { useTaskStatuses, useTaskPriorities } from '@/hooks/useLookups';
import { Skeleton } from '@/components/ui/skeleton';

interface TaskListViewProps {
  tasks: Task[];
  projectId: number;
  sprints?: Array<{ id: number; name: string }>;
  onTaskClick?: (task: Task) => void;
}

type SortColumn = 'id' | 'title' | 'status' | 'priority' | 'assignee' | 'points' | 'sprint' | 'dueDate';
type SortDirection = 'asc' | 'desc';

const priorityColors: Record<TaskPriority, string> = {
  Low: 'bg-slate-500/10 text-slate-500 border-slate-500/20',
  Medium: 'bg-blue-500/10 text-blue-500 border-blue-500/20',
  High: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
  Critical: 'bg-red-500/10 text-red-500 border-red-500/20',
};

const statusColors: Record<TaskStatus, string> = {
  Todo: 'bg-muted text-muted-foreground',
  InProgress: 'bg-blue-500/10 text-blue-500',
  Blocked: 'bg-red-500/10 text-red-500',
  Done: 'bg-green-500/10 text-green-500',
};

export function TaskListView({
  tasks,
  projectId: _projectId,
  sprints = [],
  onTaskClick,
}: TaskListViewProps) {
  const queryClient = useQueryClient();
  const [selectedTaskIds, setSelectedTaskIds] = useState<Set<number>>(new Set());
  const [sortColumn, setSortColumn] = useState<SortColumn>('id');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [pageSize, setPageSize] = useState(20);
  const [currentPage, setCurrentPage] = useState(1);
  const [editingTaskId, setEditingTaskId] = useState<number | null>(null);
  const [editingField, setEditingField] = useState<'status' | 'priority' | null>(null);
  const [deleteTaskId, setDeleteTaskId] = useState<number | null>(null);
  const { statuses, isLoading: isLoadingStatuses } = useTaskStatuses();
  const { priorities, isLoading: isLoadingPriorities } = useTaskPriorities();

  const updateStatusMutation = useMutation({
    mutationFn: ({ taskId, status }: { taskId: number; status: TaskStatus }) =>
      tasksApi.changeStatus(taskId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setEditingTaskId(null);
      setEditingField(null);
    },
    onError: () => {
      showError('Failed to update status');
    },
  });

  const updatePriorityMutation = useMutation({
    mutationFn: ({ taskId, priority }: { taskId: number; priority: TaskPriority }) =>
      tasksApi.update(taskId, { priority }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setEditingTaskId(null);
      setEditingField(null);
    },
    onError: () => {
      showError('Failed to update priority');
    },
  });


  const bulkUpdateStatusMutation = useMutation({
    mutationFn: ({ taskIds, status }: { taskIds: number[]; status: TaskStatus }) => {
      return Promise.all(taskIds.map((id) => tasksApi.changeStatus(id, status)));
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setSelectedTaskIds(new Set());
      showToast(`Updated status for ${selectedTaskIds.size} task(s)`, 'success');
    },
    onError: () => {
      showError('Failed to update tasks');
    },
  });

  const bulkUpdatePriorityMutation = useMutation({
    mutationFn: ({ taskIds, priority }: { taskIds: number[]; priority: TaskPriority }) => {
      return Promise.all(taskIds.map((id) => tasksApi.update(id, { priority })));
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setSelectedTaskIds(new Set());
      showToast(`Updated priority for ${selectedTaskIds.size} task(s)`, 'success');
    },
    onError: () => {
      showError('Failed to update tasks');
    },
  });

  const deleteTaskMutation = useMutation({
    mutationFn: (taskId: number) => tasksApi.changeStatus(taskId, 'Done'), // Soft delete by marking as Done
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setDeleteTaskId(null);
      showSuccess("Task deleted");
    },
    onError: () => {
      showError('Failed to delete task');
    },
  });

  const handleSort = (column: SortColumn) => {
    if (sortColumn === column) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(column);
      setSortDirection('asc');
    }
  };

  const sortedTasks = useMemo(() => {
    const sorted = [...tasks].sort((a, b) => {
      let comparison = 0;

      switch (sortColumn) {
        case 'id':
          comparison = a.id - b.id;
          break;
        case 'title':
          comparison = a.title.localeCompare(b.title);
          break;
        case 'status':
          comparison = a.status.localeCompare(b.status);
          break;
        case 'priority': {
          const priorityOrder: Record<TaskPriority, number> = {
            Critical: 4,
            High: 3,
            Medium: 2,
            Low: 1,
          };
          comparison = priorityOrder[b.priority] - priorityOrder[a.priority];
          break;
        }
        case 'assignee':
          comparison = (a.assigneeName || '').localeCompare(b.assigneeName || '');
          break;
        case 'points':
          comparison = (a.storyPoints || 0) - (b.storyPoints || 0);
          break;
        case 'sprint': {
          const aSprint = sprints.find((s) => s.id === a.sprintId)?.name || '';
          const bSprint = sprints.find((s) => s.id === b.sprintId)?.name || '';
          comparison = aSprint.localeCompare(bSprint);
          break;
        }
        case 'dueDate': {
          // Note: dueDate not in Task interface yet, using updatedAt as fallback
          const aDate = a.updatedAt ? new Date(a.updatedAt).getTime() : 0;
          const bDate = b.updatedAt ? new Date(b.updatedAt).getTime() : 0;
          comparison = aDate - bDate;
          break;
        }
      }

      return sortDirection === 'asc' ? comparison : -comparison;
    });

    return sorted;
  }, [tasks, sortColumn, sortDirection, sprints]);

  const paginatedTasks = useMemo(() => {
    const start = (currentPage - 1) * pageSize;
    return sortedTasks.slice(start, start + pageSize);
  }, [sortedTasks, currentPage, pageSize]);

  const totalPages = Math.ceil(sortedTasks.length / pageSize);

  const handleSelectAll = () => {
    if (selectedTaskIds.size === paginatedTasks.length) {
      setSelectedTaskIds(new Set());
    } else {
      setSelectedTaskIds(new Set(paginatedTasks.map((t) => t.id)));
    }
  };

  const handleSelectTask = (taskId: number) => {
    const newSelected = new Set(selectedTaskIds);
    if (newSelected.has(taskId)) {
      newSelected.delete(taskId);
    } else {
      newSelected.add(taskId);
    }
    setSelectedTaskIds(newSelected);
  };

  const SortButton = ({ column }: { column: SortColumn }) => (
    <Button
      variant="ghost"
      size="sm"
      className="h-8 w-8 p-0"
      onClick={() => handleSort(column)}
    >
      {sortColumn === column ? (
        sortDirection === 'asc' ? (
          <ArrowUp className="h-4 w-4" />
        ) : (
          <ArrowDown className="h-4 w-4" />
        )
      ) : (
        <ArrowUpDown className="h-4 w-4 opacity-50" />
      )}
    </Button>
  );

  return (
    <div className="space-y-4">
      {/* Bulk Actions Toolbar */}
      {selectedTaskIds.size > 0 && (
        <div className="flex items-center justify-between p-3 border rounded-lg bg-muted/50">
          <span className="text-sm font-medium">
            {selectedTaskIds.size} task{selectedTaskIds.size !== 1 ? 's' : ''} selected
          </span>
          <div className="flex items-center gap-2">
            <Select
              onValueChange={(value: TaskStatus) => {
                bulkUpdateStatusMutation.mutate({
                  taskIds: Array.from(selectedTaskIds),
                  status: value,
                });
              }}
            >
              <SelectTrigger className="w-[140px]">
                <SelectValue placeholder="Change Status" />
              </SelectTrigger>
              <SelectContent>
                {isLoadingStatuses ? (
                  <div className="p-2">
                    <Skeleton className="h-8 w-full mb-1" />
                    <Skeleton className="h-8 w-full mb-1" />
                    <Skeleton className="h-8 w-full" />
                  </div>
                ) : (
                  statuses.map((status) => (
                    <SelectItem key={status.value} value={status.value}>
                      {status.label}
                    </SelectItem>
                  ))
                )}
              </SelectContent>
            </Select>
            <Select
              onValueChange={(value: TaskPriority) => {
                bulkUpdatePriorityMutation.mutate({
                  taskIds: Array.from(selectedTaskIds),
                  priority: value,
                });
              }}
            >
              <SelectTrigger className="w-[140px]">
                <SelectValue placeholder="Change Priority" />
              </SelectTrigger>
              <SelectContent>
                {isLoadingPriorities ? (
                  <div className="p-2">
                    <Skeleton className="h-8 w-full mb-1" />
                    <Skeleton className="h-8 w-full mb-1" />
                    <Skeleton className="h-8 w-full" />
                  </div>
                ) : (
                  priorities.map((priority) => (
                    <SelectItem key={priority.value} value={priority.value}>
                      {priority.label}
                    </SelectItem>
                  ))
                )}
              </SelectContent>
            </Select>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setSelectedTaskIds(new Set())}
            >
              Clear Selection
            </Button>
          </div>
        </div>
      )}

      {/* Table */}
      <div className="border rounded-lg overflow-hidden">
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-12">
                  <Checkbox
                    checked={selectedTaskIds.size === paginatedTasks.length && paginatedTasks.length > 0}
                    onCheckedChange={handleSelectAll}
                  />
                </TableHead>
                <TableHead className="w-20">
                  <div className="flex items-center gap-2">
                    ID
                    <SortButton column="id" />
                  </div>
                </TableHead>
                <TableHead className="min-w-[200px]">
                  <div className="flex items-center gap-2">
                    Title
                    <SortButton column="title" />
                  </div>
                </TableHead>
                <TableHead className="w-32">
                  <div className="flex items-center gap-2">
                    Status
                    <SortButton column="status" />
                  </div>
                </TableHead>
                <TableHead className="w-32">
                  <div className="flex items-center gap-2">
                    Priority
                    <SortButton column="priority" />
                  </div>
                </TableHead>
                <TableHead className="w-40">
                  <div className="flex items-center gap-2">
                    Assignee
                    <SortButton column="assignee" />
                  </div>
                </TableHead>
                <TableHead className="w-24">
                  <div className="flex items-center gap-2">
                    Points
                    <SortButton column="points" />
                  </div>
                </TableHead>
                <TableHead className="w-32">
                  <div className="flex items-center gap-2">
                    Sprint
                    <SortButton column="sprint" />
                  </div>
                </TableHead>
                <TableHead className="w-32">
                  <div className="flex items-center gap-2">
                    Due Date
                    <SortButton column="dueDate" />
                  </div>
                </TableHead>
                <TableHead className="w-12"></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {paginatedTasks.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={10} className="text-center py-8 text-muted-foreground">
                    No tasks found
                  </TableCell>
                </TableRow>
              ) : (
                paginatedTasks.map((task) => (
                  <TableRow
                    key={task.id}
                    className={selectedTaskIds.has(task.id) ? 'bg-muted/50' : ''}
                  >
                    <TableCell>
                      <Checkbox
                        checked={selectedTaskIds.has(task.id)}
                        onCheckedChange={() => handleSelectTask(task.id)}
                      />
                    </TableCell>
                    <TableCell className="font-mono text-xs">#{task.id}</TableCell>
                    <TableCell>
                      <button
                        onClick={() => onTaskClick?.(task)}
                        className="text-left hover:underline font-medium"
                      >
                        {task.title}
                      </button>
                    </TableCell>
                    <TableCell>
                      {editingTaskId === task.id && editingField === 'status' ? (
                        <Select
                          value={task.status}
                          onValueChange={(value: TaskStatus) => {
                            updateStatusMutation.mutate({ taskId: task.id, status: value });
                          }}
                          onOpenChange={(open) => {
                            if (!open) {
                              setEditingTaskId(null);
                              setEditingField(null);
                            }
                          }}
                        >
                          <SelectTrigger className="w-[120px] h-8">
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="Todo">To Do</SelectItem>
                            <SelectItem value="InProgress">In Progress</SelectItem>
                            <SelectItem value="Blocked">Blocked</SelectItem>
                            <SelectItem value="Done">Done</SelectItem>
                          </SelectContent>
                        </Select>
                      ) : (
                        <Badge
                          className={statusColors[task.status]}
                          variant="outline"
                          onClick={() => {
                            setEditingTaskId(task.id);
                            setEditingField('status');
                          }}
                        >
                          {task.status === 'InProgress' ? 'In Progress' : task.status}
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell>
                      {editingTaskId === task.id && editingField === 'priority' ? (
                        <Select
                          value={task.priority}
                          onValueChange={(value: TaskPriority) => {
                            updatePriorityMutation.mutate({ taskId: task.id, priority: value });
                          }}
                          onOpenChange={(open) => {
                            if (!open) {
                              setEditingTaskId(null);
                              setEditingField(null);
                            }
                          }}
                        >
                          <SelectTrigger className="w-[100px] h-8">
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="Low">Low</SelectItem>
                            <SelectItem value="Medium">Medium</SelectItem>
                            <SelectItem value="High">High</SelectItem>
                            <SelectItem value="Critical">Critical</SelectItem>
                          </SelectContent>
                        </Select>
                      ) : (
                        <Badge
                          className={priorityColors[task.priority]}
                          variant="outline"
                          onClick={() => {
                            setEditingTaskId(task.id);
                            setEditingField('priority');
                          }}
                        >
                          {task.priority}
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell>
                      {task.assigneeName ? (
                        <div className="flex items-center gap-2">
                          <Avatar className="h-6 w-6">
                            <AvatarFallback className="text-xs">
                              {task.assigneeName
                                .split(' ')
                                .map((n) => n[0])
                                .join('')
                                .toUpperCase()}
                            </AvatarFallback>
                          </Avatar>
                          <span className="text-sm">{task.assigneeName}</span>
                        </div>
                      ) : (
                        <span className="text-muted-foreground text-sm">Unassigned</span>
                      )}
                    </TableCell>
                    <TableCell>{task.storyPoints || '-'}</TableCell>
                    <TableCell>
                      {task.sprintId
                        ? sprints.find((s) => s.id === task.sprintId)?.name || 'Unknown'
                        : 'Backlog'}
                    </TableCell>
                    <TableCell>
                      {task.updatedAt
                        ? format(new Date(task.updatedAt), 'MMM d, yyyy')
                        : '-'}
                    </TableCell>
                    <TableCell>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon" className="h-8 w-8">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onClick={() => onTaskClick?.(task)}>
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => setDeleteTaskId(task.id)}
                            className="text-destructive"
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      </div>

      {/* Pagination */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">Rows per page:</span>
          <Select value={pageSize.toString()} onValueChange={(v) => {
            setPageSize(parseInt(v));
            setCurrentPage(1);
          }}>
            <SelectTrigger className="w-[80px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="20">20</SelectItem>
              <SelectItem value="50">50</SelectItem>
              <SelectItem value="100">100</SelectItem>
            </SelectContent>
          </Select>
          <span className="text-sm text-muted-foreground">
            Showing {((currentPage - 1) * pageSize) + 1}-
            {Math.min(currentPage * pageSize, sortedTasks.length)} of {sortedTasks.length}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            disabled={currentPage === 1}
          >
            Previous
          </Button>
          <span className="text-sm">
            Page {currentPage} of {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            disabled={currentPage === totalPages}
          >
            Next
          </Button>
        </div>
      </div>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteTaskId} onOpenChange={(open) => !open && setDeleteTaskId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Task</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this task? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => deleteTaskId && deleteTaskMutation.mutate(deleteTaskId)}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
