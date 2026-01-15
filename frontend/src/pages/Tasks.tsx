import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tasksApi } from '@/api/tasks';
import { projectsApi } from '@/api/projects';
import { sprintsApi } from '@/api/sprints';
import { Button } from '@/components/ui/button';
import { PermissionButton } from '@/components/ui/permission-button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { EmptyState, LoadingState } from '@/components/states';
import { showError } from "@/lib/sweetalert";
import { Plus, GripVertical, AlertTriangle, ArrowUpDown, Search, X, LayoutGrid, List, Calendar, ListTodo } from 'lucide-react';
import { TaskDetailSheet } from '@/components/tasks/TaskDetailSheet';
import { TaskFiltersComponent, type TaskFilters } from '@/components/tasks/TaskFilters';
import { CreateTaskDialog } from '@/components/tasks/CreateTaskDialog';
import { TaskListView } from '@/components/tasks/TaskListView';
import { TaskTimelineView } from '@/components/tasks/TaskTimelineView';
import { useDebounce } from '@/hooks/use-debounce';
import { useProjectPermissions } from '@/hooks/useProjectPermissions';
import { useTranslation } from 'react-i18next';
import type { Task, TaskStatus, TaskPriority, Project } from '@/types';

const getStatusColumns = (t: (key: string) => string): { key: TaskStatus; label: string; color: string }[] => [
  { key: 'Todo', label: t('columns.todo'), color: 'bg-muted' },
  { key: 'InProgress', label: t('columns.inProgress'), color: 'bg-blue-500/10' },
  { key: 'Blocked', label: t('columns.blocked'), color: 'bg-red-500/10' },
  { key: 'Done', label: t('columns.done'), color: 'bg-green-500/10' },
];

const priorityColors: Record<TaskPriority, string> = {
  Low: 'bg-slate-500/10 text-slate-500',
  Medium: 'bg-blue-500/10 text-blue-500',
  High: 'bg-orange-500/10 text-orange-500',
  Critical: 'bg-red-500/10 text-red-500',
};

export default function Tasks() {
  const queryClient = useQueryClient();
  const { t } = useTranslation('tasks');
  const [selectedProjectId, setSelectedProjectId] = useState<number | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  
  const statusColumns = getStatusColumns(t);
  const [filters, setFilters] = useState<TaskFilters>({
    priority: 'All',
    assignee: 'All',
    sprint: 'All',
    type: 'All',
    aiStatus: 'All',
  });
  const [sortBy, setSortBy] = useState<'priority' | 'updated' | 'dueDate' | 'points' | 'alpha'>('priority');
  const [viewMode, setViewMode] = useState<'board' | 'list' | 'timeline'>(() => {
    const saved = localStorage.getItem('taskViewMode');
    return (saved === 'list' || saved === 'board' || saved === 'timeline') ? saved : 'board';
  });
  const [searchQuery, setSearchQuery] = useState('');
  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const projectId = selectedProjectId || projectsData?.items?.[0]?.id;

  const permissions = useProjectPermissions(projectId || 0);

  const { data: tasksData, isLoading } = useQuery({
    queryKey: ['tasks', projectId],
    queryFn: () => tasksApi.getByProject(projectId!),
    enabled: !!projectId,
  });

  const { data: sprintsData } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId!),
    enabled: !!projectId,
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ taskId, status }: { taskId: number; status: TaskStatus }) =>
      tasksApi.changeStatus(taskId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
    onError: () => {
      showError('Failed to update task');
    },
  });


  const handleDragStart = (e: React.DragEvent, task: Task) => {
    e.dataTransfer.setData('taskId', task.id.toString());
  };

  const handleDrop = (e: React.DragEvent, status: TaskStatus) => {
    e.preventDefault();
    const taskId = parseInt(e.dataTransfer.getData('taskId'));
    updateStatusMutation.mutate({ taskId, status });
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  // Filter tasks based on active filters and search query
  const filteredTasks = useMemo(() => {
    if (!tasksData?.tasks) return [];
    
    const query = debouncedSearchQuery.trim().toLowerCase();
    
    return tasksData.tasks.filter((task) => {
      // Search filter
      if (query) {
        const matchesTitle = task.title.toLowerCase().includes(query);
        const matchesId = task.id.toString().includes(query);
        const matchesDescription = task.description?.toLowerCase().includes(query) || false;
        
        if (!matchesTitle && !matchesId && !matchesDescription) {
          return false;
        }
      }

      // Priority filter
      if (filters.priority !== 'All' && task.priority !== filters.priority) {
        return false;
      }

      // Assignee filter
      if (filters.assignee === 'Unassigned' && task.assigneeId) {
        return false;
      }
      if (filters.assignee !== 'All' && filters.assignee !== 'Unassigned' && task.assigneeId !== filters.assignee) {
        return false;
      }

      // Sprint filter
      if (filters.sprint === 'Backlog' && task.sprintId) {
        return false;
      }
      if (filters.sprint === 'Current') {
        const activeSprint = sprintsData?.sprints?.find((s) => s.status === 'Active');
        if (!activeSprint || task.sprintId !== activeSprint.id) {
          return false;
        }
      }
      if (filters.sprint !== 'All' && filters.sprint !== 'Current' && filters.sprint !== 'Backlog' && task.sprintId !== filters.sprint) {
        return false;
      }

      // Type filter - Note: type is not in Task interface, so we skip this for now
      // This would require backend to return task.type
      // if (filters.type !== 'All' && task.type !== filters.type) {
      //   return false;
      // }

      // AI Status filter - Note: aiStatus is not in Task interface
      // This would require backend to return task.aiStatus or a flag
      // if (filters.aiStatus !== 'All') {
      //   const isAIEnhanced = task.metadata?.aiEnhanced || false;
      //   if (filters.aiStatus === 'AI Enhanced' && !isAIEnhanced) return false;
      //   if (filters.aiStatus === 'Manual' && isAIEnhanced) return false;
      // }

      return true;
    });
  }, [tasksData?.tasks, filters, sprintsData?.sprints, debouncedSearchQuery]);

  // Sort tasks based on sortBy option
  const sortTasks = (tasks: Task[]): Task[] => {
    const sorted = [...tasks].sort((a, b) => {
      switch (sortBy) {
        case 'priority': {
          // Priority: Critical > High > Medium > Low
          const priorityOrder: Record<TaskPriority, number> = {
            Critical: 4,
            High: 3,
            Medium: 2,
            Low: 1,
          };
          const priorityDiff = priorityOrder[b.priority as TaskPriority] - priorityOrder[a.priority as TaskPriority];
          // Secondary sort: by title (alphabetical) for same priority
          if (priorityDiff !== 0) return priorityDiff;
          return a.title.localeCompare(b.title);
        }
        case 'updated': {
          // Recently updated: most recent first
          const aUpdated = a.updatedAt ? new Date(a.updatedAt).getTime() : 0;
          const bUpdated = b.updatedAt ? new Date(b.updatedAt).getTime() : 0;
          if (bUpdated !== aUpdated) return bUpdated - aUpdated;
          // Secondary sort: by priority, then title
          const priorityOrder: Record<TaskPriority, number> = {
            Critical: 4,
            High: 3,
            Medium: 2,
            Low: 1,
          };
          const priorityDiff = priorityOrder[b.priority as TaskPriority] - priorityOrder[a.priority as TaskPriority];
          if (priorityDiff !== 0) return priorityDiff;
          return a.title.localeCompare(b.title);
        }
        case 'dueDate': {
          // Due date: nearest first (null/undefined at end)
          // Note: dueDate is not in Task interface yet, so this is placeholder
          // If dueDate exists, it would be: a.dueDate vs b.dueDate
          // For now, fallback to updated date
          const aDate = a.updatedAt ? new Date(a.updatedAt).getTime() : Number.MAX_SAFE_INTEGER;
          const bDate = b.updatedAt ? new Date(b.updatedAt).getTime() : Number.MAX_SAFE_INTEGER;
          if (aDate !== bDate) return aDate - bDate;
          // Secondary sort: by priority
          const priorityOrder: Record<TaskPriority, number> = {
            Critical: 4,
            High: 3,
            Medium: 2,
            Low: 1,
          };
          return priorityOrder[b.priority as TaskPriority] - priorityOrder[a.priority as TaskPriority];
        }
        case 'points': {
          // Story points: high to low (null/undefined at end)
          const aPoints = a.storyPoints ?? -1;
          const bPoints = b.storyPoints ?? -1;
          if (bPoints !== aPoints) return bPoints - aPoints;
          // Secondary sort: by priority, then title
          const priorityOrder: Record<TaskPriority, number> = {
            Critical: 4,
            High: 3,
            Medium: 2,
            Low: 1,
          };
          const priorityDiff = priorityOrder[b.priority as TaskPriority] - priorityOrder[a.priority as TaskPriority];
          if (priorityDiff !== 0) return priorityDiff;
          return a.title.localeCompare(b.title);
        }
        case 'alpha': {
          // Alphabetical: A-Z
          const titleDiff = a.title.localeCompare(b.title);
          if (titleDiff !== 0) return titleDiff;
          // Secondary sort: by priority
          const priorityOrder: Record<TaskPriority, number> = {
            Critical: 4,
            High: 3,
            Medium: 2,
            Low: 1,
          };
          return priorityOrder[b.priority as TaskPriority] - priorityOrder[a.priority as TaskPriority];
        }
        default:
          return 0;
      }
    });
    return sorted;
  };

  const getTasksByStatus = (status: TaskStatus) => {
    const tasks = filteredTasks.filter((task) => task.status === status);
    return sortTasks(tasks);
  };

  const filteredCount = filteredTasks.length;
  const totalTasks = tasksData?.tasks?.length || 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">{t('title')}</h1>
          <p className="text-muted-foreground">
            {viewMode === 'board' && t('description.board')}
            {viewMode === 'list' && t('description.list')}
            {viewMode === 'timeline' && t('description.timeline')}
          </p>
        </div>
        <div className="flex items-center gap-4 flex-wrap">
          <div className="flex items-center gap-1 border rounded-md p-1">
            <Button
              variant={viewMode === 'board' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => {
                setViewMode('board');
                localStorage.setItem('taskViewMode', 'board');
              }}
            >
              <LayoutGrid className="mr-2 h-4 w-4" />
              {t('views.board')}
            </Button>
            <Button
              variant={viewMode === 'list' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => {
                setViewMode('list');
                localStorage.setItem('taskViewMode', 'list');
              }}
            >
              <List className="mr-2 h-4 w-4" />
              {t('views.list')}
            </Button>
            <Button
              variant={viewMode === 'timeline' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => {
                setViewMode('timeline');
                localStorage.setItem('taskViewMode', 'timeline');
              }}
            >
              <Calendar className="mr-2 h-4 w-4" />
              {t('views.timeline')}
            </Button>
          </div>
          {projectId && (
            <>
              <div className="relative w-full md:w-[250px]">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  id="task-search"
                  name="task-search"
                  type="search"
                  autoComplete="off"
                  placeholder={t('search.placeholder')}
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-9 pr-9"
                />
                {searchQuery && (
                  <Button
                    variant="ghost"
                    size="icon"
                    className="absolute right-1 top-1/2 h-6 w-6 -translate-y-1/2"
                    onClick={() => setSearchQuery('')}
                  >
                    <X className="h-3 w-3" />
                  </Button>
                )}
              </div>
              <TaskFiltersComponent
                projectId={projectId}
                filters={filters}
                onFiltersChange={setFilters}
                tasks={tasksData?.tasks || []}
              />
              <Select value={sortBy} onValueChange={(value: typeof sortBy) => setSortBy(value)}>
                <SelectTrigger className="w-[180px]">
                  <div className="flex items-center gap-2">
                    <ArrowUpDown className="h-4 w-4" />
                    <SelectValue placeholder={t('sort.label')} />
                  </div>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="priority">{t('sort.priority')}</SelectItem>
                  <SelectItem value="updated">{t('sort.updated')}</SelectItem>
                  <SelectItem value="dueDate">{t('sort.dueDate')}</SelectItem>
                  <SelectItem value="points">{t('sort.points')}</SelectItem>
                  <SelectItem value="alpha">{t('sort.alpha')}</SelectItem>
                </SelectContent>
              </Select>
            </>
          )}
          <Select
            value={projectId?.toString() ?? ''}
            onValueChange={(value) => setSelectedProjectId(value ? parseInt(value) : null)}
          >
            <SelectTrigger className="w-[200px]">
              <SelectValue placeholder={t('selectProject')} />
            </SelectTrigger>
            <SelectContent>
              {projectsData?.items?.map((project: Project) => (
                <SelectItem key={project.id} value={project.id.toString()}>
                  {project.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <PermissionButton
            hasPermission={permissions.canCreateTasks}
            permissionName="tasks.create"
            disabled={!projectId}
            disabledReason={!projectId ? 'Select a project first' : undefined}
            onClick={() => setIsDialogOpen(true)}
          >
            <Plus className="mr-2 h-4 w-4" />
            {t('create.button')}
          </PermissionButton>
          {projectId && (
            <CreateTaskDialog
              open={isDialogOpen}
              onOpenChange={setIsDialogOpen}
              projectId={projectId}
            />
          )}
        </div>
      </div>

      {isLoading ? (
        <LoadingState count={4} variant="grid" showCard={false} skeletonHeight="h-32" />
      ) : !projectId ? (
        <EmptyState
          icon={<ListTodo />}
          title={t('empty.noProject', 'No project selected')}
          description="Please select a project to view tasks"
        />
      ) : filteredTasks.length === 0 && (debouncedSearchQuery.trim() || filters.priority !== 'All' || filters.assignee !== 'All' || filters.sprint !== 'All') ? (
        <div className="space-y-4">
          <EmptyState
            icon={<Search />}
            title={debouncedSearchQuery.trim() ? t('empty.noResults', 'No tasks found') : t('empty.noFilters', 'No tasks match your filters')}
            description={debouncedSearchQuery.trim() ? 'Try adjusting your search query' : 'Try clearing your filters to see more tasks'}
          />
          <div className="flex gap-2 justify-center">
            {debouncedSearchQuery.trim() && (
              <Button
                variant="outline"
                onClick={() => setSearchQuery('')}
              >
                {t('empty.clearSearch', 'Clear Search')}
              </Button>
            )}
            {(filters.priority !== 'All' || filters.assignee !== 'All' || filters.sprint !== 'All') && (
              <Button
                variant="outline"
                onClick={() => setFilters({ priority: 'All', assignee: 'All', sprint: 'All', type: 'All', aiStatus: 'All' })}
              >
                {t('empty.clearFilters', 'Clear Filters')}
              </Button>
            )}
          </div>
        </div>
      ) : viewMode === 'list' ? (
        <TaskListView
          tasks={filteredTasks}
          projectId={projectId!}
          sprints={sprintsData?.sprints || []}
          onTaskClick={setSelectedTask}
        />
      ) : viewMode === 'timeline' ? (
        <TaskTimelineView
          tasks={filteredTasks}
          sprints={sprintsData?.sprints || []}
          onTaskClick={setSelectedTask}
        />
      ) : (
        <>
          {(debouncedSearchQuery.trim() || filters.priority !== 'All' || filters.assignee !== 'All' || filters.sprint !== 'All') && (
            <div className="flex items-center justify-between text-sm text-muted-foreground px-1">
              <span>
                {t('count.showing', { filtered: filteredCount, total: totalTasks, tasks: totalTasks === 1 ? t('count.task') : t('count.tasks') })}
              </span>
            </div>
          )}
          <div className="grid gap-4 md:grid-cols-4">
            {statusColumns.map((column) => (
            <div
              key={column.key}
              className="space-y-4"
              onDrop={(e) => handleDrop(e, column.key)}
              onDragOver={handleDragOver}
            >
              <div className="flex items-center justify-between">
                <h3 className="font-semibold">{column.label}</h3>
                <Badge variant="secondary">{getTasksByStatus(column.key).length}</Badge>
              </div>
              <div className={`min-h-[500px] rounded-lg border border-dashed border-border p-2 ${column.color}`}>
                <div className="space-y-2">
                  {getTasksByStatus(column.key).map((task) => (
                    <Card
                      key={task.id}
                      draggable
                      onDragStart={(e) => handleDragStart(e, task)}
                      onClick={() => setSelectedTask(task)}
                      className="cursor-pointer transition-all hover:shadow-md"
                    >
                      <CardHeader className="p-3 pb-2">
                        <div className="flex items-start gap-2">
                          <GripVertical className="h-4 w-4 text-muted-foreground mt-0.5 cursor-grab" />
                          <div className="flex-1 space-y-1">
                            <CardTitle className="text-sm font-medium leading-tight">
                              {task.title}
                            </CardTitle>
                            {task.status === 'Blocked' && (
                              <div className="flex items-center gap-1 text-xs text-red-500">
                                <AlertTriangle className="h-3 w-3" />
                                {t('status.blocked')}
                              </div>
                            )}
                          </div>
                        </div>
                      </CardHeader>
                      <CardContent className="p-3 pt-0">
                        <p className="text-xs text-muted-foreground line-clamp-2 mb-2">
                          {task.description}
                        </p>
                        <div className="flex items-center justify-between">
                          <Badge className={priorityColors[task.priority as TaskPriority]} variant="secondary">
                            {task.priority}
                          </Badge>
                          {task.storyPoints && (
                            <span className="text-xs text-muted-foreground">
                              {task.storyPoints} {t('points')}
                            </span>
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </div>
            </div>
          ))}
          </div>
        </>
      )}

      {projectId && selectedTask && (
        <TaskDetailSheet
          task={selectedTask}
          open={!!selectedTask}
          onOpenChange={(open) => !open && setSelectedTask(null)}
          projectId={projectId}
          onTaskUpdated={() => {
            queryClient.invalidateQueries({ queryKey: ['tasks', projectId] });
          }}
          onTaskDeleted={() => {
            queryClient.invalidateQueries({ queryKey: ['tasks', projectId] });
          }}
        />
      )}

    </div>
  );
}
