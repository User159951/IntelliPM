import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { format } from 'date-fns';
import type { Sprint } from '@/types';

interface ProjectTimelineProps {
  sprints: Sprint[];
  projectStartDate?: string;
  projectEndDate?: string;
}

export function ProjectTimeline({ sprints, projectStartDate, projectEndDate }: ProjectTimelineProps) {
  const navigate = useNavigate();

  const { timelineStart, timelineEnd, currentDate } = useMemo(() => {
    const now = new Date();
    let start: Date;
    let end: Date;

    if (projectStartDate) {
      start = new Date(projectStartDate);
    } else if (sprints.length > 0) {
      // Use earliest sprint start date
      start = new Date(Math.min(...sprints.map((s) => new Date(s.startDate).getTime())));
    } else {
      // Fallback to 3 months ago
      start = new Date();
      start.setMonth(start.getMonth() - 3);
    }

    if (projectEndDate) {
      end = new Date(projectEndDate);
    } else if (sprints.length > 0) {
      // Use latest sprint end date
      end = new Date(Math.max(...sprints.map((s) => new Date(s.endDate).getTime())));
    } else {
      // Fallback to 3 months from now
      end = new Date();
      end.setMonth(end.getMonth() + 3);
    }

    // Add some padding (10% on each side)
    const totalDays = (end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24);
    const padding = totalDays * 0.1;
    start = new Date(start.getTime() - padding * 24 * 60 * 60 * 1000);
    end = new Date(end.getTime() + padding * 24 * 60 * 60 * 1000);

    return {
      timelineStart: start,
      timelineEnd: end,
      currentDate: now,
    };
  }, [sprints, projectStartDate, projectEndDate]);

  const timelineDuration = timelineEnd.getTime() - timelineStart.getTime();
  const currentDatePosition = ((currentDate.getTime() - timelineStart.getTime()) / timelineDuration) * 100;

  const getSprintPosition = (sprintStart: Date) => {
    return ((sprintStart.getTime() - timelineStart.getTime()) / timelineDuration) * 100;
  };

  const getSprintWidth = (sprintStart: Date, sprintEnd: Date) => {
    const sprintDuration = sprintEnd.getTime() - sprintStart.getTime();
    return (sprintDuration / timelineDuration) * 100;
  };

  const getSprintColor = (status: string) => {
    switch (status) {
      case 'Active':
        return 'bg-green-500 hover:bg-green-600';
      case 'Planned':
        return 'bg-blue-500 hover:bg-blue-600';
      case 'Completed':
        return 'bg-gray-400 hover:bg-gray-500';
      default:
        return 'bg-muted hover:bg-muted/80';
    }
  };

  if (sprints.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Project Timeline</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="py-8 text-center text-muted-foreground">
            <p>No sprints to display</p>
            <p className="text-sm mt-2">Create sprints to see the project timeline</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Project Timeline</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {/* Timeline container */}
          <div className="relative w-full overflow-x-auto pb-4">
            <div className="relative min-w-[600px] h-32">
              {/* Timeline background */}
              <div className="absolute inset-0 bg-muted/30 rounded-lg border border-border" />

              {/* Current date indicator */}
              {currentDate >= timelineStart && currentDate <= timelineEnd && (
                <div
                  className="absolute top-0 bottom-0 w-0.5 bg-red-500 z-20"
                  style={{ left: `${Math.max(0, Math.min(100, currentDatePosition))}%` }}
                >
                  <div className="absolute -top-2 left-1/2 -translate-x-1/2">
                    <div className="w-0 h-0 border-l-[4px] border-r-[4px] border-t-[6px] border-transparent border-t-red-500" />
                  </div>
                  <div className="absolute -bottom-6 left-1/2 -translate-x-1/2 whitespace-nowrap text-xs text-red-500 font-medium">
                    Today
                  </div>
                </div>
              )}

              {/* Sprint bars */}
              <div className="absolute inset-0 p-2">
                {sprints.map((sprint, index) => {
                  const sprintStart = new Date(sprint.startDate);
                  const sprintEnd = new Date(sprint.endDate);
                  const left = getSprintPosition(sprintStart);
                  const width = getSprintWidth(sprintStart, sprintEnd);
                  const top = (index % 3) * 36 + 4; // Stack sprints with some spacing

                  return (
                    <Tooltip key={sprint.id}>
                      <TooltipTrigger asChild>
                        <div
                          className={`absolute h-8 rounded cursor-pointer transition-all ${getSprintColor(
                            sprint.status
                          )} text-white text-xs font-medium flex items-center justify-center px-2 shadow-sm`}
                          style={{
                            left: `${left}%`,
                            width: `${width}%`,
                            top: `${top}px`,
                            minWidth: '60px',
                          }}
                          onClick={() => navigate(`/sprints`)}
                        >
                          <span className="truncate">{sprint.name}</span>
                        </div>
                      </TooltipTrigger>
                      <TooltipContent side="top" className="max-w-xs">
                        <div className="space-y-1">
                          <p className="font-semibold">{sprint.name}</p>
                          <p className="text-xs">
                            {format(sprintStart, 'MMM d, yyyy')} - {format(sprintEnd, 'MMM d, yyyy')}
                          </p>
                          <p className="text-xs">Status: {sprint.status}</p>
                          {sprint.goal && (
                            <p className="text-xs text-muted-foreground mt-1">Goal: {sprint.goal}</p>
                          )}
                        </div>
                      </TooltipContent>
                    </Tooltip>
                  );
                })}
              </div>
            </div>
          </div>

          {/* Date labels */}
          <div className="flex justify-between text-xs text-muted-foreground px-2">
            <span>{format(timelineStart, 'MMM d, yyyy')}</span>
            <span>{format(timelineEnd, 'MMM d, yyyy')}</span>
          </div>

          {/* Legend */}
          <div className="flex items-center gap-4 text-xs pt-2 border-t">
            <span className="text-muted-foreground">Status:</span>
            <div className="flex items-center gap-1">
              <div className="w-3 h-3 rounded bg-green-500" />
              <span>Active</span>
            </div>
            <div className="flex items-center gap-1">
              <div className="w-3 h-3 rounded bg-blue-500" />
              <span>Planned</span>
            </div>
            <div className="flex items-center gap-1">
              <div className="w-3 h-3 rounded bg-gray-400" />
              <span>Completed</span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
