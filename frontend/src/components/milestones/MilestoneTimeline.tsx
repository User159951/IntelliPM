import { useMemo, useRef, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { milestonesApi } from '@/api/milestones';
import { Rocket, Flag, Calendar, Star, AlertCircle } from 'lucide-react';
import { isBefore, isAfter, isToday } from 'date-fns';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatDate, DateFormats } from '@/utils/dateFormat';
import { cn } from '@/lib/utils';
import type { MilestoneDto } from '@/types/milestones';

interface MilestoneTimelineProps {
  projectId: number;
  onMilestoneClick?: (milestone: MilestoneDto) => void;
}

/**
 * Get icon component based on milestone type.
 */
function getTypeIcon(type: string) {
  switch (type) {
    case 'Release':
      return Rocket;
    case 'Sprint':
      return Flag;
    case 'Deadline':
      return Calendar;
    case 'Custom':
      return Star;
    default:
      return Star;
  }
}

/**
 * Get status color for milestone.
 */
function getStatusColor(status: string): string {
  switch (status) {
    case 'Completed':
      return 'bg-green-500 dark:bg-green-600';
    case 'Pending':
      return 'bg-blue-500 dark:bg-blue-600';
    case 'InProgress':
      return 'bg-orange-500 dark:bg-orange-600';
    case 'Missed':
      return 'bg-red-500 dark:bg-red-600';
    case 'Cancelled':
      return 'bg-gray-500 dark:bg-gray-600';
    default:
      return 'bg-gray-500';
  }
}

/**
 * Horizontal timeline visualization component for project milestones.
 * Displays milestones along a time axis with today's date marker.
 */
export function MilestoneTimeline({ projectId, onMilestoneClick }: MilestoneTimelineProps) {
  const { language } = useLanguage();
  const timelineRef = useRef<HTMLDivElement>(null);
  const todayRef = useRef<HTMLDivElement>(null);

  const { data: milestones, isLoading } = useQuery({
    queryKey: ['projectMilestones', projectId, undefined, true], // Include completed
    queryFn: () => milestonesApi.getProjectMilestones(projectId, undefined, true),
    enabled: !!projectId,
  });

  // Calculate timeline dimensions
  const timelineData = useMemo(() => {
    if (!milestones || milestones.length === 0) {
      return null;
    }

    const now = new Date();
    const dates = milestones.map((m) => new Date(m.dueDate));
    const minDate = new Date(Math.min(...dates.map((d) => d.getTime())));
    const maxDate = new Date(Math.max(...dates.map((d) => d.getTime())));
    
    // Add padding (30 days before/after)
    const paddingDays = 30;
    const startDate = new Date(minDate);
    startDate.setDate(startDate.getDate() - paddingDays);
    const endDate = new Date(maxDate);
    endDate.setDate(endDate.getDate() + paddingDays);

    const totalDays = Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
    const todayPosition = ((now.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24) / totalDays) * 100;

    return {
      startDate,
      endDate,
      totalDays,
      todayPosition: Math.max(0, Math.min(100, todayPosition)),
      milestones: milestones.map((milestone) => {
        const milestoneDate = new Date(milestone.dueDate);
        const position = ((milestoneDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24) / totalDays) * 100;
        return {
          ...milestone,
          position: Math.max(0, Math.min(100, position)),
          date: milestoneDate,
        };
      }).sort((a, b) => a.date.getTime() - b.date.getTime()),
    };
  }, [milestones]);

  // Scroll to today's marker on mount
  useEffect(() => {
    if (todayRef.current && timelineRef.current) {
      const scrollPosition = todayRef.current.offsetLeft - timelineRef.current.offsetWidth / 2;
      timelineRef.current.scrollTo({ left: Math.max(0, scrollPosition), behavior: 'smooth' });
    }
  }, [timelineData]);

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-40" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-64 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (!milestones || milestones.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Milestone Timeline</CardTitle>
          <CardDescription>No milestones to display</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground text-center py-8">
            Create milestones to see them on the timeline.
          </p>
        </CardContent>
      </Card>
    );
  }

  if (!timelineData) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Milestone Timeline</CardTitle>
        <CardDescription>
          Timeline view of project milestones from {formatDate(timelineData.startDate, DateFormats.LONG(language), language)} to {formatDate(timelineData.endDate, DateFormats.LONG(language), language)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div
          ref={timelineRef}
          className="relative w-full overflow-x-auto pb-4"
          style={{ minHeight: '300px' }}
        >
          {/* Timeline Axis */}
          <div className="relative h-2 bg-gray-200 dark:bg-gray-800 rounded-full mb-8 min-w-full">
            {/* Today Marker */}
            <div
              ref={todayRef}
              className="absolute top-0 bottom-0 w-0.5 bg-blue-600 dark:bg-blue-400 z-10"
              style={{ left: `${timelineData.todayPosition}%` }}
            >
              <div className="absolute -top-6 left-1/2 transform -translate-x-1/2 whitespace-nowrap">
                <Badge variant="outline" className="bg-blue-50 dark:bg-blue-950 border-blue-600 dark:border-blue-400 text-blue-700 dark:text-blue-300">
                  Today
                </Badge>
              </div>
            </div>
          </div>

          {/* Milestones */}
          <div className="relative min-w-full" style={{ height: '200px' }}>
            {timelineData.milestones.map((milestone) => {
              const TypeIcon = getTypeIcon(milestone.type);
              // Date comparison for styling (variables kept for potential future use)
              void isBefore(milestone.date, new Date());
              void isAfter(milestone.date, new Date());
              void isToday(milestone.date);

              return (
                <div
                  key={milestone.id}
                  className="absolute transform -translate-x-1/2"
                  style={{ left: `${milestone.position}%` }}
                >
                  {/* Milestone Node */}
                  <div className="flex flex-col items-center">
                    {/* Milestone Marker */}
                    <button
                      onClick={() => onMilestoneClick?.(milestone)}
                      className={cn(
                        'relative w-12 h-12 rounded-full border-4 border-white dark:border-gray-900 shadow-lg',
                        'flex items-center justify-center transition-all hover:scale-110',
                        'cursor-pointer focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary',
                        getStatusColor(milestone.status)
                      )}
                      aria-label={`Milestone: ${milestone.name}`}
                    >
                      <TypeIcon className="h-6 w-6 text-white" />
                    </button>

                    {/* Connecting Line */}
                    <div
                      className={cn(
                        'w-0.5 mt-2',
                        milestone.status === 'Completed' && 'bg-green-500',
                        milestone.status === 'Pending' && 'bg-blue-500',
                        milestone.status === 'InProgress' && 'bg-orange-500',
                        milestone.status === 'Missed' && 'bg-red-500',
                        milestone.status === 'Cancelled' && 'bg-gray-500'
                      )}
                      style={{ height: '120px' }}
                    />

                    {/* Milestone Card */}
                    <div
                      className={cn(
                        'absolute top-16 w-64 p-3 rounded-lg shadow-md border',
                        'bg-white dark:bg-gray-900',
                        milestone.isOverdue && 'border-red-500 dark:border-red-600'
                      )}
                    >
                      <div className="space-y-2">
                        {/* Name and Status */}
                        <div className="flex items-start justify-between gap-2">
                          <h4 className="font-semibold text-sm line-clamp-2 flex-1">
                            {milestone.name}
                          </h4>
                          <Badge
                            className={cn(
                              'text-xs shrink-0',
                              getStatusColor(milestone.status),
                              'text-white'
                            )}
                          >
                            {milestone.status}
                          </Badge>
                        </div>

                        {/* Type */}
                        <div className="flex items-center gap-1 text-xs text-muted-foreground">
                          <TypeIcon className="h-3 w-3" />
                          <span>{milestone.type}</span>
                        </div>

                        {/* Due Date */}
                        <div className="text-xs text-muted-foreground">
                          {formatDate(milestone.date, DateFormats.LONG(language), language)}
                        </div>

                        {/* Days Until/Past */}
                        <div className={cn(
                          'text-xs font-medium',
                          milestone.daysUntilDue < 0 && 'text-red-600 dark:text-red-400',
                          milestone.daysUntilDue >= 0 && 'text-muted-foreground'
                        )}>
                          {milestone.daysUntilDue < 0
                            ? `${Math.abs(milestone.daysUntilDue)} days overdue`
                            : `${milestone.daysUntilDue} days left`}
                        </div>

                        {/* Progress Bar (if InProgress) */}
                        {milestone.status === 'InProgress' && (
                          <div className="space-y-1">
                            <div className="flex items-center justify-between text-xs">
                              <span className="text-muted-foreground">Progress</span>
                              <span className="font-medium">{milestone.progress}%</span>
                            </div>
                            <Progress value={milestone.progress} className="h-1.5" />
                          </div>
                        )}

                        {/* Overdue Warning */}
                        {milestone.isOverdue && (
                          <div className="flex items-center gap-1 text-xs text-red-600 dark:text-red-400">
                            <AlertCircle className="h-3 w-3" />
                            <span>Overdue</span>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

