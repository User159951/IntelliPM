import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Filter, X } from 'lucide-react';
import { usersApi } from '@/api/users';
import { sprintsApi } from '@/api/sprints';
import type { TaskPriority, Task } from '@/types';

export interface TaskFilters {
  priority: TaskPriority | 'All';
  assignee: number | 'All' | 'Unassigned';
  sprint: number | 'All' | 'Current' | 'Backlog';
  type: 'Bug' | 'Feature' | 'Task' | 'All';
  aiStatus: 'AI Enhanced' | 'Manual' | 'All';
}

const defaultFilters: TaskFilters = {
  priority: 'All',
  assignee: 'All',
  sprint: 'All',
  type: 'All',
  aiStatus: 'All',
};

interface TaskFiltersProps {
  projectId: number | null;
  filters: TaskFilters;
  onFiltersChange: (filters: TaskFilters) => void;
  tasks?: Task[];
}

export function TaskFiltersComponent({
  projectId,
  filters,
  onFiltersChange,
  tasks = [],
}: TaskFiltersProps) {
  const { data: usersData } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(),
    enabled: true,
  });

  const { data: sprintsData } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId!),
    enabled: !!projectId,
  });

  const activeSprint = sprintsData?.sprints?.find((s) => s.status === 'Active');

  // Get unique assignees from tasks
  const assigneesInTasks = new Set<number>();
  tasks.forEach((task) => {
    if (task.assigneeId) {
      assigneesInTasks.add(task.assigneeId);
    }
  });

  // Get unique sprints from tasks
  const sprintsInTasks = new Set<number>();
  tasks.forEach((task) => {
    if (task.sprintId) {
      sprintsInTasks.add(task.sprintId);
    }
  });

  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.priority !== 'All') count++;
    if (filters.assignee !== 'All' && filters.assignee !== 'Unassigned') count++;
    if (filters.assignee === 'Unassigned') count++;
    if (filters.sprint !== 'All' && filters.sprint !== 'Current' && filters.sprint !== 'Backlog') count++;
    if (filters.sprint === 'Current' || filters.sprint === 'Backlog') count++;
    if (filters.type !== 'All') count++;
    if (filters.aiStatus !== 'All') count++;
    return count;
  }, [filters]);

  const handleFilterChange = <K extends keyof TaskFilters>(key: K, value: TaskFilters[K]) => {
    onFiltersChange({ ...filters, [key]: value });
  };

  const clearAllFilters = () => {
    onFiltersChange(defaultFilters);
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="outline" className="gap-2">
          <Filter className="h-4 w-4" />
          Filters
          {activeFilterCount > 0 && (
            <Badge variant="secondary" className="ml-1 h-5 min-w-[20px] px-1.5">
              {activeFilterCount}
            </Badge>
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel>Priority</DropdownMenuLabel>
        <div className="space-y-2 p-2">
          {(['All', 'Low', 'Medium', 'High', 'Critical'] as const).map((priority) => (
            <div key={priority} className="flex items-center space-x-2">
              <Checkbox
                id={`priority-${priority}`}
                checked={filters.priority === priority}
                onCheckedChange={() => handleFilterChange('priority', priority)}
              />
              <label
                htmlFor={`priority-${priority}`}
                className="text-sm font-normal cursor-pointer flex-1"
              >
                {priority}
              </label>
            </div>
          ))}
        </div>

        <DropdownMenuSeparator />

        <DropdownMenuLabel>Assignee</DropdownMenuLabel>
        <div className="space-y-2 p-2 max-h-[200px] overflow-y-auto">
          <div className="flex items-center space-x-2">
            <Checkbox
              id="assignee-all"
              checked={filters.assignee === 'All'}
              onCheckedChange={() => handleFilterChange('assignee', 'All')}
            />
            <label htmlFor="assignee-all" className="text-sm font-normal cursor-pointer flex-1">
              All
            </label>
          </div>
          <div className="flex items-center space-x-2">
            <Checkbox
              id="assignee-unassigned"
              checked={filters.assignee === 'Unassigned'}
              onCheckedChange={() => handleFilterChange('assignee', 'Unassigned')}
            />
            <label
              htmlFor="assignee-unassigned"
              className="text-sm font-normal cursor-pointer flex-1"
            >
              Unassigned
            </label>
          </div>
          {usersData?.users
            ?.filter((user) => assigneesInTasks.has(user.id))
            .map((user) => (
              <div key={user.id} className="flex items-center space-x-2">
                <Checkbox
                  id={`assignee-${user.id}`}
                  checked={filters.assignee === user.id}
                  onCheckedChange={() => handleFilterChange('assignee', user.id)}
                />
                <label
                  htmlFor={`assignee-${user.id}`}
                  className="text-sm font-normal cursor-pointer flex-1"
                >
                  {user.firstName} {user.lastName}
                </label>
              </div>
            ))}
        </div>

        <DropdownMenuSeparator />

        <DropdownMenuLabel>Sprint</DropdownMenuLabel>
        <div className="space-y-2 p-2 max-h-[200px] overflow-y-auto">
          <div className="flex items-center space-x-2">
            <Checkbox
              id="sprint-all"
              checked={filters.sprint === 'All'}
              onCheckedChange={() => handleFilterChange('sprint', 'All')}
            />
            <label htmlFor="sprint-all" className="text-sm font-normal cursor-pointer flex-1">
              All
            </label>
          </div>
          {activeSprint && (
            <div className="flex items-center space-x-2">
              <Checkbox
                id="sprint-current"
                checked={filters.sprint === 'Current'}
                onCheckedChange={() => handleFilterChange('sprint', 'Current')}
              />
              <label
                htmlFor="sprint-current"
                className="text-sm font-normal cursor-pointer flex-1"
              >
                Current Sprint
              </label>
            </div>
          )}
          <div className="flex items-center space-x-2">
            <Checkbox
              id="sprint-backlog"
              checked={filters.sprint === 'Backlog'}
              onCheckedChange={() => handleFilterChange('sprint', 'Backlog')}
            />
            <label htmlFor="sprint-backlog" className="text-sm font-normal cursor-pointer flex-1">
              Backlog
            </label>
          </div>
          {sprintsData?.sprints
            ?.filter((sprint) => sprintsInTasks.has(sprint.id))
            .map((sprint) => (
              <div key={sprint.id} className="flex items-center space-x-2">
                <Checkbox
                  id={`sprint-${sprint.id}`}
                  checked={filters.sprint === sprint.id}
                  onCheckedChange={() => handleFilterChange('sprint', sprint.id)}
                />
                <label
                  htmlFor={`sprint-${sprint.id}`}
                  className="text-sm font-normal cursor-pointer flex-1"
                >
                  {sprint.name}
                </label>
              </div>
            ))}
        </div>

        <DropdownMenuSeparator />

        <DropdownMenuLabel>Type</DropdownMenuLabel>
        <div className="space-y-2 p-2">
          {(['All', 'Bug', 'Feature', 'Task'] as const).map((type) => (
            <div key={type} className="flex items-center space-x-2">
              <Checkbox
                id={`type-${type}`}
                checked={filters.type === type}
                onCheckedChange={() => handleFilterChange('type', type)}
              />
              <label
                htmlFor={`type-${type}`}
                className="text-sm font-normal cursor-pointer flex-1"
              >
                {type}
              </label>
            </div>
          ))}
        </div>

        <DropdownMenuSeparator />

        <DropdownMenuLabel>AI Status</DropdownMenuLabel>
        <div className="space-y-2 p-2">
          {(['All', 'AI Enhanced', 'Manual'] as const).map((status) => (
            <div key={status} className="flex items-center space-x-2">
              <Checkbox
                id={`aiStatus-${status}`}
                checked={filters.aiStatus === status}
                onCheckedChange={() => handleFilterChange('aiStatus', status)}
              />
              <label
                htmlFor={`aiStatus-${status}`}
                className="text-sm font-normal cursor-pointer flex-1"
              >
                {status}
              </label>
            </div>
          ))}
        </div>

        {activeFilterCount > 0 && (
          <>
            <DropdownMenuSeparator />
            <div className="p-2">
              <Button
                variant="ghost"
                size="sm"
                className="w-full justify-start"
                onClick={clearAllFilters}
              >
                <X className="mr-2 h-4 w-4" />
                Clear all filters
              </Button>
            </div>
          </>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
