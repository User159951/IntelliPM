import { useState, useEffect, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Calendar } from 'lucide-react';
import { format } from 'date-fns';
import { cn } from '@/lib/utils';
import { releasesApi } from '@/api/releases';
import type { ReleaseSprintDto } from '@/types/releases';

interface SprintSelectorDialogProps {
  projectId: number;
  releaseId?: number;
  initialSelectedIds: number[];
  onConfirm: (sprintIds: number[]) => void;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

interface SprintItemProps {
  sprint: ReleaseSprintDto;
  isSelected: boolean;
  onToggle: () => void;
}

function SprintItem({ sprint, isSelected, onToggle }: SprintItemProps) {
  return (
    <div
      className={cn(
        'flex items-start gap-3 p-3 rounded-lg border cursor-pointer hover:bg-muted/50 transition-colors',
        isSelected && 'border-primary bg-primary/5'
      )}
      onClick={onToggle}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onToggle();
        }
      }}
      aria-checked={isSelected}
    >
      <Checkbox
        checked={isSelected}
        onCheckedChange={onToggle}
        onClick={(e) => e.stopPropagation()}
        className="mt-1"
        aria-label={`Select sprint ${sprint.name}`}
      />

      <div className="flex-1 space-y-1 min-w-0">
        <div className="flex items-center justify-between gap-2">
          <h4 className="font-semibold text-sm truncate">{sprint.name}</h4>
          <Badge variant="outline" className="shrink-0">
            {sprint.status}
          </Badge>
        </div>

        <p className="text-xs text-muted-foreground">
          {format(new Date(sprint.startDate), 'MMM d')} -{' '}
          {format(new Date(sprint.endDate), 'MMM d, yyyy')}
        </p>

        <div className="flex items-center gap-2 mt-2">
          <span className="text-xs text-muted-foreground shrink-0">
            {sprint.completedTasksCount}/{sprint.totalTasksCount} tasks
          </span>
          <Progress
            value={sprint.completionPercentage}
            className="h-1.5 flex-1 max-w-[100px]"
          />
          <span className="text-xs font-medium shrink-0">
            {sprint.completionPercentage}%
          </span>
        </div>
      </div>
    </div>
  );
}

/**
 * Dialog component for selecting sprints to include in a release.
 * Features filtering, select all/deselect all, and visual feedback.
 */
export function SprintSelectorDialog({
  projectId,
  releaseId,
  initialSelectedIds,
  onConfirm,
  open,
  onOpenChange,
}: SprintSelectorDialogProps) {
  const [selectedIds, setSelectedIds] = useState<number[]>(initialSelectedIds);
  const [statusFilter, setStatusFilter] = useState<string>('All');

  // Fetch available sprints
  const { data: sprints, isLoading } = useQuery({
    queryKey: ['available-sprints', projectId, releaseId],
    queryFn: () => releasesApi.getAvailableSprintsForRelease(projectId, releaseId),
    enabled: open && !!projectId,
  });

  // Reset when dialog opens
  useEffect(() => {
    if (open) {
      setSelectedIds(initialSelectedIds);
      setStatusFilter('All');
    }
  }, [open, initialSelectedIds]);

  // Filter sprints by status
  const filteredSprints = useMemo(() => {
    if (!sprints) return [];
    if (statusFilter === 'All') return sprints;
    return sprints.filter((s) => s.status === statusFilter);
  }, [sprints, statusFilter]);

  const handleToggle = (sprintId: number) => {
    setSelectedIds((prev) =>
      prev.includes(sprintId)
        ? prev.filter((id) => id !== sprintId)
        : [...prev, sprintId]
    );
  };

  const handleSelectAll = () => {
    const allIds = filteredSprints.map((s) => s.id);
    setSelectedIds(allIds);
  };

  const handleDeselectAll = () => {
    setSelectedIds([]);
  };

  const handleConfirm = () => {
    onConfirm(selectedIds);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[80vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Select Sprints for Release</DialogTitle>
          <DialogDescription>
            Choose sprints to include in this release
          </DialogDescription>
        </DialogHeader>

        {/* Filter and actions */}
        <div className="flex items-center justify-between gap-4 py-4 shrink-0">
          <div className="flex items-center gap-2">
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-[180px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="All">All Sprints</SelectItem>
                <SelectItem value="Completed">Completed</SelectItem>
                <SelectItem value="Active">Active</SelectItem>
                <SelectItem value="Planned">Planned</SelectItem>
              </SelectContent>
            </Select>

            <Badge variant="secondary">
              {selectedIds.length} selected
            </Badge>
          </div>

          <div className="flex gap-2">
            <Button variant="outline" size="sm" onClick={handleSelectAll}>
              Select All
            </Button>
            <Button variant="outline" size="sm" onClick={handleDeselectAll}>
              Deselect All
            </Button>
          </div>
        </div>

        {/* Sprint list */}
        <ScrollArea className="flex-1 border rounded-lg min-h-[400px]">
          {isLoading ? (
            <div className="space-y-2 p-4">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-20 w-full" />
              ))}
            </div>
          ) : filteredSprints.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-[200px] text-center p-4">
              <Calendar className="h-12 w-12 text-muted-foreground mb-2" />
              <p className="text-sm text-muted-foreground">
                No sprints available
              </p>
              <p className="text-xs text-muted-foreground mt-1">
                {statusFilter !== 'All'
                  ? `No ${statusFilter.toLowerCase()} sprints found`
                  : 'Create sprints or complete existing sprints first'}
              </p>
            </div>
          ) : (
            <div className="p-4 space-y-2">
              {filteredSprints.map((sprint) => (
                <SprintItem
                  key={sprint.id}
                  sprint={sprint}
                  isSelected={selectedIds.includes(sprint.id)}
                  onToggle={() => handleToggle(sprint.id)}
                />
              ))}
            </div>
          )}
        </ScrollArea>

        {/* Footer */}
        <DialogFooter className="shrink-0">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleConfirm} disabled={selectedIds.length === 0}>
            Add {selectedIds.length} Sprint{selectedIds.length !== 1 ? 's' : ''}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

