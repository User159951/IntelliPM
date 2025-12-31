import { useState, useEffect, useMemo } from 'react';
import { formatDistanceToNow, isAfter } from 'date-fns';
import { DragDropContext, Droppable, Draggable, DropResult } from 'react-beautiful-dnd';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';
import { Tooltip, TooltipContent, TooltipTrigger, TooltipProvider } from '@/components/ui/tooltip';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Circle,
  Clock,
  CheckCircle2,
  AlertCircle,
  Plus,
  User,
  GripVertical,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { Task, TaskStatus, TaskPriority } from '@/types';
import BlockedBadge from './BlockedBadge';
import { useProjectTaskDependencies } from '@/hooks/useProjectTaskDependencies';

/**
 * Extended Task interface for TaskBoard component.
 */
export interface TaskBoardTask {
  id: number;
  title: string;
  description?: string;
  status: 'Todo' | 'InProgress' | 'Done';
  priority: TaskPriority;
  assignee?: {
    id: number;
    username: string;
    firstName?: string;
    lastName?: string;
  };
  assigneeId?: number;
  assigneeName?: string;
  storyPoints?: number;
  createdAt: string;
  dueDate?: string;
}

/**
 * Props for the TaskBoard component.
 */
export interface TaskBoardProps {
  /** Optional project ID */
  projectId?: number;
  /** Optional sprint ID */
  sprintId?: number;
  /** Array of tasks to display */
  tasks: TaskBoardTask[] | Task[];
  /** Loading state */
  isLoading?: boolean;
  /** Click handler for tasks */
  onTaskClick?: (taskId: number) => void;
  /** Status change handler (for drag-and-drop) */
  onStatusChange?: (taskId: number, newStatus: TaskStatus) => void;
  /** Whether user can edit tasks */
  canEditTasks?: boolean;
  /** Handler for adding new task */
  onAddTask?: (status: 'Todo' | 'InProgress' | 'Done') => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Column configuration for Kanban board.
 */
interface ColumnConfig {
  key: 'Todo' | 'InProgress' | 'Done';
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  headerBg: string;
  headerText: string;
  borderColor: string;
}

const columns: ColumnConfig[] = [
  {
    key: 'Todo',
    label: 'To Do',
    icon: Circle,
    headerBg: 'bg-blue-500/10',
    headerText: 'text-blue-600 dark:text-blue-400',
    borderColor: 'border-blue-500/20',
  },
  {
    key: 'InProgress',
    label: 'In Progress',
    icon: Clock,
    headerBg: 'bg-amber-500/10',
    headerText: 'text-amber-600 dark:text-amber-400',
    borderColor: 'border-amber-500/20',
  },
  {
    key: 'Done',
    label: 'Done',
    icon: CheckCircle2,
    headerBg: 'bg-green-500/10',
    headerText: 'text-green-600 dark:text-green-400',
    borderColor: 'border-green-500/20',
  },
];

/**
 * Priority colors for task cards.
 */
const priorityColors: Record<TaskPriority, string> = {
  Low: 'border-l-slate-400',
  Medium: 'border-l-yellow-400',
  High: 'border-l-orange-400',
  Critical: 'border-l-red-500',
};

/**
 * TaskBoard component for displaying tasks in a Kanban-style board view with drag-and-drop.
 * 
 * Features:
 * - Three columns: Todo, In Progress, Done
 * - Drag-and-drop between columns using react-beautiful-dnd
 * - Task cards with priority indicators
 * - Assignee avatars
 * - Story points badges
 * - Overdue date warnings
 * - Scrollable columns
 * - Loading and empty states
 * - Responsive design
 * - Optimistic updates
 * 
 * @example
 * ```tsx
 * <TaskBoard
 *   projectId={projectId}
 *   tasks={tasks}
 *   isLoading={isLoading}
 *   onTaskClick={handleTaskClick}
 *   onStatusChange={handleStatusChange}
 *   canEditTasks={canEdit}
 * />
 * ```
 */
export default function TaskBoard({
  projectId,
  sprintId,
  tasks,
  isLoading = false,
  onTaskClick,
  onStatusChange,
  canEditTasks = false,
  onAddTask,
  className,
}: TaskBoardProps) {
  // Local state for optimistic updates
  const [localTasks, setLocalTasks] = useState<TaskBoardTask[] | Task[]>(tasks);
  const [showOnlyBlocked, setShowOnlyBlocked] = useState(false);

  // Fetch blocking info for all tasks in project (batch optimization)
  const { blockingMap } = useProjectTaskDependencies(projectId ?? 0);

  // Sync local state with props
  useEffect(() => {
    setLocalTasks(tasks);
  }, [tasks]);

  // Check if mobile device (disable drag on mobile)
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 768);
    };
    
    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  // Handle drag end
  const handleDragEnd = (result: DropResult) => {
    const { source, destination, draggableId } = result;

    // Dropped outside a valid droppable area
    if (!destination) {
      return;
    }

    // Dropped in same position
    if (
      source.droppableId === destination.droppableId &&
      source.index === destination.index
    ) {
      return;
    }

    // Parse task ID from draggableId
    const taskId = parseInt(draggableId.replace('task-', ''));
    const task = localTasks.find((t) => t.id === taskId);
    
    if (!task) {
      return;
    }

    // Determine new status from destination column
    const newStatus = destination.droppableId as TaskStatus;

    // Only update if status actually changed
    if (task.status === newStatus) {
      return;
    }

    // Optimistically update local state
    const updatedTasks = localTasks.map((t) =>
      t.id === taskId ? { ...t, status: newStatus as TaskStatus } : t
    ) as typeof localTasks;
    setLocalTasks(updatedTasks);

    // Call parent handler to persist change
    // Note: Parent component should handle errors and refetch on failure
    // to revert optimistic update
    if (onStatusChange) {
      onStatusChange(taskId, newStatus);
    }
  };

  // Filter tasks by blocked status if filter is enabled
  const filteredTasks = useMemo(() => {
    if (!showOnlyBlocked || !projectId) {
      return localTasks;
    }

    return localTasks.filter((task) => {
      const blockingInfo = blockingMap.get(task.id);
      return blockingInfo?.isBlocked ?? false;
    });
  }, [localTasks, showOnlyBlocked, blockingMap, projectId]);

  // Group tasks by status
  const tasksByStatus = useMemo(() => {
    const grouped = {
      Todo: [] as (TaskBoardTask | Task)[],
      InProgress: [] as (TaskBoardTask | Task)[],
      Done: [] as (TaskBoardTask | Task)[],
    };

    filteredTasks.forEach((task) => {
      const status = task.status;
      if (status === 'Todo' || status === 'InProgress' || status === 'Done') {
        grouped[status].push(task);
      }
    });

    return grouped;
  }, [filteredTasks]);

  return (
    <TooltipProvider>
      <div className="space-y-4">
        {/* Filter controls */}
        {projectId && (
          <div className="flex items-center gap-2 px-2">
            <Checkbox
              id="show-blocked-only"
              checked={showOnlyBlocked}
              onCheckedChange={(checked) => setShowOnlyBlocked(checked === true)}
            />
            <label
              htmlFor="show-blocked-only"
              className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
            >
              Show only blocked tasks
            </label>
          </div>
        )}

        <DragDropContext onDragEnd={handleDragEnd}>
          <div
            className={cn(
              'flex flex-col md:flex-row gap-4 h-full min-h-[600px]',
              className
            )}
            role="region"
            aria-label="Task board"
          >
            {columns.map((column) => (
              <TaskColumn
                key={column.key}
                column={column}
                tasks={tasksByStatus[column.key]}
                isLoading={isLoading}
                onTaskClick={onTaskClick}
                canEdit={canEditTasks}
                onAddTask={onAddTask}
                isDragDisabled={!canEditTasks || isMobile}
                blockingMap={blockingMap}
              />
            ))}
          </div>
        </DragDropContext>
      </div>
    </TooltipProvider>
  );
}

/**
 * Props for TaskColumn component.
 */
interface TaskColumnProps {
  column: ColumnConfig;
  tasks: (TaskBoardTask | Task)[];
  isLoading: boolean;
  onTaskClick?: (taskId: number) => void;
  canEdit: boolean;
  onAddTask?: (status: 'Todo' | 'InProgress' | 'Done') => void;
  isDragDisabled: boolean;
  blockingMap: Map<number, { isBlocked: boolean; blockedByCount: number; blockingTasks: Array<{ taskId: number; title: string; status: string }> }>;
}

/**
 * TaskColumn component representing a single Kanban column.
 */
function TaskColumn({
  column,
  tasks,
  isLoading,
  onTaskClick,
  canEdit,
  onAddTask,
  isDragDisabled,
  blockingMap,
}: TaskColumnProps) {
  const Icon = column.icon;

  return (
    <div
      className={cn(
        'flex flex-col flex-1 min-w-0 rounded-lg border',
        column.borderColor,
        'bg-card'
      )}
      role="group"
      aria-label={`${column.label} column`}
      data-column={column.key}
    >
      {/* Column Header */}
      <div
        className={cn(
          'flex items-center justify-between p-4 rounded-t-lg border-b',
          column.headerBg,
          column.borderColor
        )}
      >
        <div className="flex items-center gap-2">
          <Icon className={cn('h-5 w-5', column.headerText)} />
          <h3 className={cn('font-semibold text-sm', column.headerText)}>
            {column.label}
          </h3>
          <Badge variant="secondary" className="text-xs">
            {tasks.length} {tasks.length === 1 ? 'task' : 'tasks'}
          </Badge>
        </div>
        {canEdit && onAddTask && (
          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7"
            onClick={() => onAddTask(column.key)}
            aria-label={`Add task to ${column.label}`}
          >
            <Plus className="h-4 w-4" />
          </Button>
        )}
      </div>

      {/* Droppable Column Body */}
      <Droppable droppableId={column.key}>
        {(provided, snapshot) => (
          <ScrollArea
            ref={provided.innerRef}
            {...provided.droppableProps}
            className={cn(
              'flex-1 px-2',
              snapshot.isDraggingOver && 'bg-muted/30'
            )}
          >
            <div className="py-2 space-y-2 min-h-[400px]">
              {isLoading ? (
                // Loading skeletons
                <>
                  {[...Array(3)].map((_, i) => (
                    <Card key={i} className="p-3">
                      <div className="space-y-2">
                        <Skeleton className="h-4 w-3/4" />
                        <Skeleton className="h-3 w-full" />
                        <Skeleton className="h-3 w-2/3" />
                        <div className="flex items-center gap-2 pt-2">
                          <Skeleton className="h-6 w-6 rounded-full" />
                          <Skeleton className="h-4 w-16" />
                        </div>
                      </div>
                    </Card>
                  ))}
                </>
              ) : tasks.length === 0 ? (
                // Empty state
                <div className="flex flex-col items-center justify-center py-12 text-center">
                  <Icon className={cn('h-12 w-12 mb-3 opacity-50', column.headerText)} />
                  <p className="text-sm text-muted-foreground">
                    {column.key === 'Todo' && 'No tasks to do'}
                    {column.key === 'InProgress' && 'No tasks in progress'}
                    {column.key === 'Done' && 'No completed tasks'}
                  </p>
                </div>
              ) : (
                // Task cards
                tasks.map((task, index) => (
                  <TaskCard
                    key={task.id}
                    task={task}
                    index={index}
                    onClick={onTaskClick}
                    isDragDisabled={isDragDisabled}
                    blockingInfo={blockingMap.get(task.id)}
                  />
                ))
              )}
              {provided.placeholder}
            </div>
            <ScrollBar orientation="vertical" />
          </ScrollArea>
        )}
      </Droppable>
    </div>
  );
}

/**
 * Props for TaskCard component.
 */
interface TaskCardProps {
  task: TaskBoardTask | Task;
  index: number;
  onClick?: (taskId: number) => void;
  isDragDisabled: boolean;
  blockingInfo?: { isBlocked: boolean; blockedByCount: number; blockingTasks: Array<{ taskId: number; title: string; status: string }> };
}

/**
 * TaskCard component representing a single draggable task card.
 */
function TaskCard({ task, index, onClick, isDragDisabled, blockingInfo }: TaskCardProps) {
  // Get assignee info
  const getAssigneeInfo = () => {
    if ('assignee' in task && task.assignee) {
      return {
        id: task.assignee.id,
        name: task.assignee.firstName && task.assignee.lastName
          ? `${task.assignee.firstName} ${task.assignee.lastName}`
          : task.assignee.username,
        initials: task.assignee.firstName?.[0] || task.assignee.username[0] || 'U',
      };
    }
    if (task.assigneeName) {
      const nameParts = task.assigneeName.split(' ');
      return {
        id: task.assigneeId || 0,
        name: task.assigneeName,
        initials: nameParts.map((p) => p[0]).join('').toUpperCase().slice(0, 2) || 'U',
      };
    }
    return null;
  };

  // Check if task is overdue
  const isOverdue = () => {
    if (!task.dueDate) return false;
    return isAfter(new Date(), new Date(task.dueDate));
  };

  const assignee = getAssigneeInfo();
  const overdue = isOverdue();
  const isBlocked = blockingInfo?.isBlocked ?? false;

  return (
    <Draggable
      draggableId={`task-${task.id}`}
      index={index}
      isDragDisabled={isDragDisabled}
    >
      {(provided, snapshot) => (
        <Card
          ref={provided.innerRef}
          {...provided.draggableProps}
          className={cn(
            'cursor-pointer transition-all hover:shadow-md border-l-4',
            priorityColors[task.priority],
            isBlocked && 'border-l-red-500 border-l-4',
            snapshot.isDragging && 'shadow-lg rotate-1 opacity-90'
          )}
          onClick={() => onClick?.(task.id)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              onClick?.(task.id);
            }
          }}
          role="button"
          tabIndex={0}
          aria-label={`Task: ${task.title}. Priority: ${task.priority}. ${assignee ? `Assigned to ${assignee.name}` : 'Unassigned'}. ${overdue ? 'Overdue' : ''}. ${isBlocked ? 'Blocked' : ''}`}
          data-task-id={task.id}
          data-task-status={task.status}
          data-task-priority={task.priority}
        >
          <CardContent className="p-3 space-y-2">
            {/* Drag handle */}
            <div
              {...provided.dragHandleProps}
              className="flex items-start gap-2"
            >
              <GripVertical className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0 cursor-grab active:cursor-grabbing" />
              <div className="flex-1 min-w-0">
                {/* Title with Blocked Badge */}
                <div className="flex items-center gap-2 flex-wrap">
                  <h4 className="text-sm font-medium line-clamp-2 leading-tight flex-1 min-w-0">
                    {task.title}
                  </h4>
                  {isBlocked && blockingInfo && (
                    <BlockedBadge
                      blockedByCount={blockingInfo.blockedByCount}
                      blockingTasks={blockingInfo.blockingTasks}
                      variant="sm"
                    />
                  )}
                </div>

                {/* Description */}
                {task.description && (
                  <p className="text-xs text-muted-foreground line-clamp-2 mt-1">
                    {task.description}
                  </p>
                )}
              </div>
            </div>

            {/* Footer: Assignee, Story Points, Due Date */}
            <div className="flex items-center justify-between pt-2 border-t">
              <div className="flex items-center gap-2">
                {assignee ? (
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <Avatar className="h-6 w-6">
                        <AvatarFallback className="text-xs bg-primary/10 text-primary">
                          {assignee.initials}
                        </AvatarFallback>
                      </Avatar>
                    </TooltipTrigger>
                    <TooltipContent>
                      <p>{assignee.name}</p>
                    </TooltipContent>
                  </Tooltip>
                ) : (
                  <div className="h-6 w-6 rounded-full border-2 border-dashed border-muted-foreground/30 flex items-center justify-center">
                    <User className="h-3 w-3 text-muted-foreground" />
                  </div>
                )}
              </div>

              <div className="flex items-center gap-2">
                {/* Story Points */}
                {task.storyPoints && (
                  <Badge variant="outline" className="text-xs">
                    {task.storyPoints} SP
                  </Badge>
                )}

                {/* Due Date */}
                {task.dueDate && (
                  <div
                    className={cn(
                      'flex items-center gap-1 text-xs',
                      overdue ? 'text-red-500' : 'text-muted-foreground'
                    )}
                    title={overdue ? 'Overdue' : `Due ${formatDistanceToNow(new Date(task.dueDate), { addSuffix: true })}`}
                  >
                    {overdue && <AlertCircle className="h-3 w-3" />}
                    <Clock className="h-3 w-3" />
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </Draggable>
  );
}
