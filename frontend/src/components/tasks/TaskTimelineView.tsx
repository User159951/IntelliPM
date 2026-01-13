import { useState, useMemo } from 'react';
import { startOfWeek, endOfWeek, addDays, differenceInDays, isWithinInterval, isPast, format } from 'date-fns';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatDate, DateFormats } from '@/utils/dateFormat';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { ZoomIn, ZoomOut } from 'lucide-react';
import type { Task, TaskStatus } from '@/types';

interface TaskTimelineViewProps {
  tasks: Task[];
  sprints?: Array<{ id: number; name: string; startDate: string; endDate: string }>;
  onTaskClick?: (task: Task) => void;
}

type GroupBy = 'sprint' | 'assignee' | 'priority' | 'none';
type ZoomLevel = 'week' | 'month' | 'quarter';

const statusColors: Record<TaskStatus, string> = {
  Todo: 'bg-blue-500',
  InProgress: 'bg-orange-500',
  Blocked: 'bg-red-500',
  Done: 'bg-green-500',
};

const statusHoverColors: Record<TaskStatus, string> = {
  Todo: 'bg-blue-600',
  InProgress: 'bg-orange-600',
  Blocked: 'bg-red-600',
  Done: 'bg-green-600',
};

export function TaskTimelineView({
  tasks,
  sprints = [],
  onTaskClick,
}: TaskTimelineViewProps) {
  const { language } = useLanguage();
  const [groupBy, setGroupBy] = useState<GroupBy>('sprint');
  const [zoomLevel, setZoomLevel] = useState<ZoomLevel>('month');
  const [dateRange, setDateRange] = useState<{ start: Date; end: Date }>(() => {
    const today = new Date();
    const start = startOfWeek(today, { weekStartsOn: 1 });
    const end = endOfWeek(addDays(today, 30), { weekStartsOn: 1 });
    return { start, end };
  });

  // Filter tasks that have dates or can use sprint dates
  const tasksWithDates = useMemo(() => {
    return tasks
      .map((task) => {
        let startDate: Date;
        let dueDate: Date;

        // Try to get dates from task (if they exist in the future)
        // For now, we'll use sprint dates or created date as fallback
        if (task.sprintId) {
          const sprint = sprints.find((s) => s.id === task.sprintId);
          if (sprint) {
            startDate = new Date(sprint.startDate);
            dueDate = new Date(sprint.endDate);
          } else {
            // Fallback to created date if sprint not found
            startDate = new Date(task.createdAt);
            dueDate = addDays(startDate, 7); // Default 1 week duration
          }
        } else {
          // Fallback to created date if no sprint
          startDate = new Date(task.createdAt);
          dueDate = addDays(startDate, 7); // Default 1 week duration
        }

        return {
          task,
          startDate,
          dueDate,
        };
      })
      .filter((item) => {
        // Only show tasks within the visible date range
        return isWithinInterval(item.startDate, { start: dateRange.start, end: dateRange.end }) ||
               isWithinInterval(item.dueDate, { start: dateRange.start, end: dateRange.end }) ||
               (item.startDate <= dateRange.start && item.dueDate >= dateRange.end);
      });
  }, [tasks, sprints, dateRange]);

  // Group tasks
  const groupedTasks = useMemo(() => {
    if (groupBy === 'none') {
      return { 'All Tasks': tasksWithDates };
    }

    const groups: Record<string, typeof tasksWithDates> = {};

    tasksWithDates.forEach((item) => {
      let key: string;

      switch (groupBy) {
        case 'sprint':
          if (item.task.sprintId) {
            const sprint = sprints.find((s) => s.id === item.task.sprintId);
            key = sprint?.name || `Sprint ${item.task.sprintId}`;
          } else {
            key = 'Backlog';
          }
          break;
        case 'assignee':
          key = item.task.assigneeName || 'Unassigned';
          break;
        case 'priority':
          key = item.task.priority;
          break;
        default:
          key = 'All Tasks';
      }

      if (!groups[key]) {
        groups[key] = [];
      }
      groups[key].push(item);
    });

    return groups;
  }, [tasksWithDates, groupBy, sprints]);

  // Calculate timeline metrics
  const totalDays = differenceInDays(dateRange.end, dateRange.start);
  const today = new Date();
  const todayPosition = isWithinInterval(today, { start: dateRange.start, end: dateRange.end })
    ? ((differenceInDays(today, dateRange.start) / totalDays) * 100)
    : null;

  // Generate date headers
  const dateHeaders = useMemo(() => {
    const headers: Date[] = [];
    let current = new Date(dateRange.start);
    const step = zoomLevel === 'week' ? 1 : zoomLevel === 'month' ? 7 : 30;

    while (current <= dateRange.end) {
      headers.push(new Date(current));
      current = addDays(current, step);
    }

    return headers;
  }, [dateRange, zoomLevel]);

  const handleZoomIn = () => {
    const days = differenceInDays(dateRange.end, dateRange.start);
    const center = addDays(dateRange.start, days / 2);
    const newDays = Math.max(7, Math.floor(days * 0.7));
    setDateRange({
      start: addDays(center, -newDays / 2),
      end: addDays(center, newDays / 2),
    });
  };

  const handleZoomOut = () => {
    const days = differenceInDays(dateRange.end, dateRange.start);
    const center = addDays(dateRange.start, days / 2);
    const newDays = Math.floor(days * 1.5);
    setDateRange({
      start: addDays(center, -newDays / 2),
      end: addDays(center, newDays / 2),
    });
  };

  const getTaskPosition = (item: typeof tasksWithDates[0]) => {
    const taskStart = item.startDate < dateRange.start ? dateRange.start : item.startDate;
    const taskEnd = item.dueDate > dateRange.end ? dateRange.end : item.dueDate;
    const left = ((differenceInDays(taskStart, dateRange.start) / totalDays) * 100);
    const width = ((differenceInDays(taskEnd, taskStart) / totalDays) * 100);
    return { left: Math.max(0, left), width: Math.max(2, width) };
  };

  const isOverdue = (item: typeof tasksWithDates[0]) => {
    return isPast(item.dueDate) && item.task.status !== 'Done';
  };

  return (
    <div className="space-y-4">
      {/* Controls */}
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div className="flex items-center gap-2">
          <Select value={groupBy} onValueChange={(v) => setGroupBy(v as GroupBy)}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Group by" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="none">No Grouping</SelectItem>
              <SelectItem value="sprint">Sprint</SelectItem>
              <SelectItem value="assignee">Assignee</SelectItem>
              <SelectItem value="priority">Priority</SelectItem>
            </SelectContent>
          </Select>
          <Select value={zoomLevel} onValueChange={(v) => setZoomLevel(v as ZoomLevel)}>
            <SelectTrigger className="w-[120px]">
              <SelectValue placeholder="Zoom" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="week">Week</SelectItem>
              <SelectItem value="month">Month</SelectItem>
              <SelectItem value="quarter">Quarter</SelectItem>
            </SelectContent>
          </Select>
          <div className="flex items-center gap-1 border rounded-md">
            <Button variant="ghost" size="icon" className="h-8 w-8" onClick={handleZoomOut}>
              <ZoomOut className="h-4 w-4" />
            </Button>
            <Button variant="ghost" size="icon" className="h-8 w-8" onClick={handleZoomIn}>
              <ZoomIn className="h-4 w-4" />
            </Button>
          </div>
        </div>
        <div className="text-sm text-muted-foreground">
          {format(dateRange.start, 'MMM d')} - {format(dateRange.end, 'MMM d, yyyy')}
        </div>
      </div>

      {/* Timeline */}
      <Card className="p-4">
        <div className="overflow-x-auto">
          <div className="min-w-full" style={{ minWidth: `${dateHeaders.length * 100}px` }}>
            {/* Date Headers */}
            <div className="flex border-b mb-2 pb-2">
              <div className="w-48 flex-shrink-0 font-medium text-sm">Group / Task</div>
              <div className="flex-1 relative">
                <div className="flex">
                  {dateHeaders.map((date, idx) => (
                    <div
                      key={idx}
                      className="flex-1 text-xs text-muted-foreground text-center border-l first:border-l-0"
                      style={{ minWidth: '100px' }}
                    >
                      <div>{formatDate(date, DateFormats.MONTH_DAY(language), language)}</div>
                      <div className="text-[10px]">{formatDate(date, DateFormats.DAY_OF_WEEK(language), language)}</div>
                    </div>
                  ))}
                </div>
                {/* Today Line */}
                {todayPosition !== null && (
                  <div
                    className="absolute top-0 bottom-0 w-0.5 bg-red-500 z-10"
                    style={{ left: `${todayPosition}%` }}
                  >
                    <div className="absolute -top-4 left-1/2 transform -translate-x-1/2 text-xs font-medium text-red-500 bg-background px-1">
                      Today
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Task Rows */}
            <div className="space-y-1">
              {Object.entries(groupedTasks).map(([groupName, groupTasks]) => (
                <div key={groupName} className="space-y-1">
                  {groupBy !== 'none' && (
                    <div className="flex items-center py-2 border-b font-medium text-sm">
                      <div className="w-48 flex-shrink-0">{groupName}</div>
                      <div className="flex-1"></div>
                    </div>
                  )}
                  {groupTasks.map((item) => {
                    const { left, width } = getTaskPosition(item);
                    const overdue = isOverdue(item);
                    const statusColor = overdue ? 'bg-red-600' : statusColors[item.task.status];
                    const hoverColor = overdue ? 'bg-red-700' : statusHoverColors[item.task.status];

                    return (
                      <TooltipProvider key={item.task.id}>
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <button
                              type="button"
                              className="flex items-center py-1 hover:bg-muted/50 transition-colors cursor-pointer w-full text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                              onClick={() => onTaskClick?.(item.task)}
                              onKeyDown={(e) => {
                                if (e.key === 'Enter' || e.key === ' ') {
                                  e.preventDefault();
                                  onTaskClick?.(item.task);
                                }
                              }}
                              aria-label={`View task ${item.task.id}: ${item.task.title}`}
                            >
                              <div className="w-48 flex-shrink-0 flex items-center gap-2">
                                <span className="text-xs font-mono text-muted-foreground">#{item.task.id}</span>
                                <span className="text-sm truncate">{item.task.title}</span>
                              </div>
                              <div className="flex-1 relative h-8">
                                <div
                                  className={`absolute h-6 rounded ${statusColor} ${hoverColor} transition-colors shadow-sm flex items-center px-2 text-xs text-white font-medium`}
                                  style={{
                                    left: `${left}%`,
                                    width: `${width}%`,
                                    minWidth: '60px',
                                  }}
                                >
                                  <span className="truncate">{item.task.title}</span>
                                </div>
                                {overdue && (
                                  <div className="absolute left-0 top-0 bottom-0 w-1 bg-red-800"></div>
                                )}
                              </div>
                            </button>
                          </TooltipTrigger>
                          <TooltipContent side="right" className="max-w-xs">
                            <div className="space-y-1">
                              <div className="font-medium">{item.task.title}</div>
                              <div className="text-xs text-muted-foreground">
                                {format(item.startDate, 'MMM d')} - {format(item.dueDate, 'MMM d, yyyy')}
                              </div>
                              <div className="flex items-center gap-2 text-xs">
                                <Badge variant="outline" className={statusColors[item.task.status]}>
                                  {item.task.status}
                                </Badge>
                                <Badge variant="outline">{item.task.priority}</Badge>
                                {item.task.storyPoints && (
                                  <span className="text-muted-foreground">{item.task.storyPoints} SP</span>
                                )}
                              </div>
                              {item.task.description && (
                                <div className="text-xs text-muted-foreground line-clamp-2">
                                  {item.task.description}
                                </div>
                              )}
                            </div>
                          </TooltipContent>
                        </Tooltip>
                      </TooltipProvider>
                    );
                  })}
                </div>
              ))}
            </div>
          </div>
        </div>
      </Card>

      {/* Legend */}
      <div className="flex items-center gap-4 text-xs">
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded bg-blue-500"></div>
          <span>Todo</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded bg-orange-500"></div>
          <span>In Progress</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded bg-green-500"></div>
          <span>Done</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded bg-red-500"></div>
          <span>Blocked</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded bg-red-600 border-2 border-red-800"></div>
          <span>Overdue</span>
        </div>
      </div>
    </div>
  );
}
