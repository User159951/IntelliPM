import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { format } from 'date-fns';
import { Clock, CheckCircle, Layers, Inbox } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  SelectSeparator,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { sprintsApi } from '@/api/sprints';
import type { Sprint } from '@/types';

interface SprintSelectorProps {
  projectId: number;
  value?: number | null; // Selected sprint ID
  onChange: (sprintId: number | null) => void;
  placeholder?: string;
  showAllOption?: boolean; // Show "All Sprints" option
  showBacklogOption?: boolean; // Show "Backlog (No Sprint)" option
  filter?: 'active' | 'completed' | 'all';
  disabled?: boolean;
}

interface SprintLabelProps {
  sprint?: Sprint | null;
  showDates?: boolean;
  showStatus?: boolean;
}

function SprintLabel({ sprint, showDates = true, showStatus = true }: SprintLabelProps) {
  if (!sprint) return null;

  const isActive = sprint.status === 'Active';
  const statusIcon = isActive ? (
    <Clock className="h-4 w-4 text-green-500" />
  ) : (
    <CheckCircle className="h-4 w-4 text-muted-foreground" />
  );

  return (
    <div className="flex items-center gap-2 w-full">
      {statusIcon}
      <div className="flex flex-col flex-1 min-w-0">
        <span className="font-medium truncate">{sprint.name}</span>
        {showDates && sprint.startDate && sprint.endDate && (
          <span className="text-xs text-muted-foreground">
            {format(new Date(sprint.startDate), 'MMM d')} -{' '}
            {format(new Date(sprint.endDate), 'MMM d, yyyy')}
          </span>
        )}
      </div>
      {showStatus && isActive && (
        <Badge variant="default" className="ml-auto shrink-0">
          Active
        </Badge>
      )}
    </div>
  );
}

export default function SprintSelector({
  projectId,
  value,
  onChange,
  placeholder = 'Select sprint',
  showAllOption = false,
  showBacklogOption = false,
  filter = 'all',
  disabled = false,
}: SprintSelectorProps) {
  // Fetch sprints data
  const { data: sprintsData, isLoading } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId),
    enabled: !!projectId,
  });

  const sprints = sprintsData?.sprints || [];

  // Filter sprints based on filter prop
  const filteredSprints = useMemo(() => {
    if (!sprints.length) return [];

    switch (filter) {
      case 'active':
        return sprints.filter((s) => s.status === 'Active');
      case 'completed':
        return sprints.filter((s) => s.status === 'Completed');
      default:
        return sprints;
    }
  }, [sprints, filter]);

  // Sort sprints: active first, then by start date descending
  const sortedSprints = useMemo(() => {
    return [...filteredSprints].sort((a, b) => {
      // Active sprints first
      if (a.status === 'Active' && b.status !== 'Active') return -1;
      if (a.status !== 'Active' && b.status === 'Active') return 1;
      
      // Then by start date (most recent first)
      if (a.startDate && b.startDate) {
        return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
      }
      
      return 0;
    });
  }, [filteredSprints]);

  const handleValueChange = (val: string) => {
    if (val === 'all') {
      onChange(null);
    } else if (val === 'backlog') {
      onChange(null);
    } else {
      onChange(parseInt(val));
    }
  };

  const selectedSprint = value ? sprints.find((s) => s.id === value) : null;

  return (
    <Select
      value={value?.toString() || ''}
      onValueChange={handleValueChange}
      disabled={disabled || isLoading}
    >
      <SelectTrigger className="w-full" aria-label="Select sprint">
        <SelectValue placeholder={placeholder}>
          {selectedSprint ? selectedSprint.name : placeholder}
        </SelectValue>
      </SelectTrigger>

      <SelectContent>
        {showAllOption && (
          <SelectItem value="all">
            <div className="flex items-center gap-2">
              <Layers className="h-4 w-4" />
              <span>All Sprints</span>
            </div>
          </SelectItem>
        )}

        {showBacklogOption && (
          <SelectItem value="backlog">
            <div className="flex items-center gap-2">
              <Inbox className="h-4 w-4" />
              <span>Backlog (No Sprint)</span>
            </div>
          </SelectItem>
        )}

        {sortedSprints.length > 0 && (showAllOption || showBacklogOption) && (
          <SelectSeparator />
        )}

        {sortedSprints.map((sprint) => (
          <SelectItem key={sprint.id} value={sprint.id.toString()}>
            <SprintLabel sprint={sprint} />
          </SelectItem>
        ))}

        {sortedSprints.length === 0 && !showAllOption && !showBacklogOption && (
          <div className="p-2 text-sm text-muted-foreground text-center">
            No sprints available
          </div>
        )}
      </SelectContent>
    </Select>
  );
}

