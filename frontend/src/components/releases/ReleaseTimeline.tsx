import { useMemo, useRef, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { releasesApi } from '@/api/releases';
import { Package, Calendar } from 'lucide-react';
import { parseISO, isPast } from 'date-fns';
import { useLanguage } from '@/contexts/LanguageContext';
import { formatDate, DateFormats } from '@/utils/dateFormat';
import { cn } from '@/lib/utils';
import type { ReleaseDto } from '@/types/releases';

interface ReleaseTimelineProps {
  projectId: number;
  className?: string;
}

interface TimelineNodeProps {
  release: ReleaseDto;
  isLeft: boolean;
  onClick: () => void;
  isPast: boolean;
}

const getStatusColor = (status: string): string => {
  switch (status) {
    case 'Deployed':
      return '#10b981'; // green
    case 'Planned':
      return '#3b82f6'; // blue
    case 'InProgress':
      return '#f59e0b'; // orange
    case 'Testing':
      return '#eab308'; // yellow
    case 'ReadyForDeployment':
      return '#a855f7'; // purple
    case 'Failed':
      return '#ef4444'; // red
    case 'Cancelled':
      return '#6b7280'; // gray
    default:
      return '#6b7280'; // gray
  }
};

const getTypeColor = (type: string): string => {
  switch (type) {
    case 'Major':
      return 'bg-red-500 text-white';
    case 'Minor':
      return 'bg-blue-500 text-white';
    case 'Patch':
      return 'bg-green-500 text-white';
    case 'Hotfix':
      return 'bg-orange-500 text-white';
    default:
      return 'bg-gray-500 text-white';
  }
};

const getQualityGateColor = (status: string | null): string => {
  switch (status) {
    case 'Passed':
      return '#10b981'; // green
    case 'Warning':
      return '#eab308'; // yellow
    case 'Failed':
      return '#ef4444'; // red
    default:
      return '#6b7280'; // gray
  }
};

function TimelineNode({ release, isLeft, onClick, isPast }: TimelineNodeProps) {
  const { language } = useLanguage();
  const statusColor = getStatusColor(release.status);
  const qualityGateColor = getQualityGateColor(release.overallQualityStatus);
  const completionPercentage =
    release.totalTasksCount > 0
      ? (release.completedTasksCount / release.totalTasksCount) * 100
      : 0;

  const releaseDate = release.actualReleaseDate
    ? parseISO(release.actualReleaseDate)
    : parseISO(release.plannedDate);

  return (
    <div
      className={cn(
        'relative flex items-center',
        'flex-row pl-12', // Mobile: all on right side
        'md:pl-0 md:justify-start',
        isLeft ? 'md:flex-row' : 'md:flex-row-reverse',
        'mb-8 md:mb-12'
      )}
    >
      {/* Node Card */}
      <button
        onClick={onClick}
        className={cn(
          'relative z-10 w-full md:w-80 lg:w-96',
          'transition-all hover:scale-105 hover:shadow-lg',
          'focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary',
          'text-left'
        )}
        aria-label={`Release ${release.version}: ${release.name}`}
      >
        <Card
          className={cn(
            'border-2 transition-all',
            isPast ? 'border-opacity-100' : 'border-dashed border-opacity-60'
          )}
          style={{ borderColor: statusColor }}
        >
          <CardHeader className="pb-3">
            <div className="flex items-start justify-between gap-2">
              <div className="flex-1 min-w-0">
                <CardTitle className="text-xl font-bold mb-1">{release.version}</CardTitle>
                <CardDescription className="line-clamp-2">{release.name}</CardDescription>
              </div>
              <div className="flex flex-col gap-1 shrink-0">
                <Badge className={getTypeColor(release.type)}>{release.type}</Badge>
                <Badge
                  className="text-xs"
                  style={{
                    backgroundColor: statusColor,
                    color: 'white',
                  }}
                >
                  {release.status}
                </Badge>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-3">
            {/* Date */}
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Calendar className="h-4 w-4" />
              <time dateTime={releaseDate.toISOString()}>
                {formatDate(releaseDate, DateFormats.LONG(language), language)}
              </time>
            </div>

            {/* Quality Gate Indicator */}
            <div className="flex items-center gap-2">
              <div
                className="h-3 w-3 rounded-full"
                style={{ backgroundColor: qualityGateColor }}
                aria-label={`Quality gate status: ${release.overallQualityStatus || 'Pending'}`}
              />
              <span className="text-sm text-muted-foreground">
                {release.overallQualityStatus || 'Pending'}
              </span>
            </div>

            {/* Task Progress */}
            {release.totalTasksCount > 0 && (
              <div className="space-y-1">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Tasks</span>
                  <span className="font-medium">
                    {release.completedTasksCount}/{release.totalTasksCount} (
                    {Math.round(completionPercentage)}%)
                  </span>
                </div>
                <Progress value={completionPercentage} className="h-2" />
              </div>
            )}

            {/* Pre-release badge */}
            {release.isPreRelease && (
              <Badge variant="outline" className="bg-yellow-50 text-yellow-800 border-yellow-300">
                Pre-release
              </Badge>
            )}
          </CardContent>
        </Card>
      </button>

      {/* Mobile: Connector Line to Left */}
      <div
        className={cn(
          'absolute top-1/2 left-4 h-0.5 bg-gray-300 dark:bg-gray-700 w-8',
          'md:hidden'
        )}
        style={{ transform: 'translateY(-50%)' }}
      />

      {/* Mobile: Node Marker on Left */}
      <div
        className={cn(
          'absolute top-1/2 left-4 transform -translate-x-1/2 -translate-y-1/2',
          'w-5 h-5 rounded-full border-4 border-white dark:border-gray-900',
          'shadow-lg z-20 transition-all hover:scale-110',
          'md:hidden'
        )}
        style={{
          backgroundColor: statusColor,
          borderStyle: isPast ? 'solid' : 'dashed',
        }}
        aria-label={`Release ${release.version} marker`}
      />

      {/* Desktop: Connector Line to Center */}
      <div
        className={cn(
          'absolute top-1/2 h-0.5 bg-gray-300 dark:bg-gray-700',
          'hidden md:block',
          isLeft ? 'left-full' : 'right-full',
          'w-12'
        )}
        style={{ transform: 'translateY(-50%)' }}
      />

      {/* Desktop: Center Node Marker */}
      <div
        className={cn(
          'absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2',
          'w-5 h-5 rounded-full border-4 border-white dark:border-gray-900',
          'shadow-lg z-20 transition-all hover:scale-110',
          'hidden md:block'
        )}
        style={{
          backgroundColor: statusColor,
          borderStyle: isPast ? 'solid' : 'dashed',
        }}
        aria-label={`Release ${release.version} marker`}
      />
    </div>
  );
}

/**
 * Vertical timeline visualization component for project releases.
 * Displays releases chronologically with a "Today" marker.
 */
export function ReleaseTimeline({ projectId, className }: ReleaseTimelineProps) {
  const navigate = useNavigate();
  const todayRef = useRef<HTMLDivElement>(null);
  const timelineRef = useRef<HTMLDivElement>(null);

  const { data: releases, isLoading } = useQuery({
    queryKey: ['projectReleases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  // Sort releases by date (oldest at bottom, newest at top)
  const sortedReleases = useMemo(() => {
    if (!releases) return [];

    return [...releases].sort((a, b) => {
      const dateA = a.actualReleaseDate || a.plannedDate;
      const dateB = b.actualReleaseDate || b.plannedDate;
      return new Date(dateB).getTime() - new Date(dateA).getTime(); // Descending (newest first)
    });
  }, [releases]);

  // Find "today" position - find first release that is in the future
  const todayPosition = useMemo(() => {
    if (!sortedReleases.length) return null;
    const now = new Date();
    
    // Find the index of the first future release
    const futureIndex = sortedReleases.findIndex((r) => {
      const releaseDate = r.actualReleaseDate
        ? parseISO(r.actualReleaseDate)
        : parseISO(r.plannedDate);
      return releaseDate > now;
    });

    // If no future releases, today marker goes at the end
    if (futureIndex === -1) {
      return sortedReleases.length;
    }

    return futureIndex;
  }, [sortedReleases]);

  // Auto-scroll to today marker on mount
  useEffect(() => {
    if (todayRef.current && timelineRef.current) {
      setTimeout(() => {
        todayRef.current?.scrollIntoView({
          behavior: 'smooth',
          block: 'center',
        });
      }, 100);
    }
  }, [sortedReleases]);

  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle>Release Timeline</CardTitle>
          <CardDescription>Loading releases...</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-8">
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex items-center gap-4">
                <Skeleton className="h-5 w-5 rounded-full" />
                <Skeleton className="h-32 w-full md:w-80" />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!releases || releases.length === 0) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle>Release Timeline</CardTitle>
          <CardDescription>No releases to display</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="text-center py-12">
            <Package className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
            <p className="text-muted-foreground mb-4">
              No releases yet. Create your first release!
            </p>
            <Button
              onClick={() => navigate(`/projects/${projectId}/releases/new`)}
              variant="default"
            >
              Create Release
            </Button>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>Release Timeline</CardTitle>
        <CardDescription>
          Chronological view of all project releases. Past releases are shown with solid borders,
          planned releases with dashed borders.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div
          ref={timelineRef}
          className="relative py-8 overflow-y-auto max-h-[800px]"
          style={{ scrollBehavior: 'smooth' }}
        >
          {/* Vertical Center Line */}
          <div className="absolute left-1/2 top-0 bottom-0 w-0.5 bg-gray-300 dark:bg-gray-700 transform -translate-x-1/2 hidden md:block" />

          {/* Releases */}
          <div className="relative">
            {sortedReleases.map((release, index) => {
              const releaseDate = release.actualReleaseDate
                ? parseISO(release.actualReleaseDate)
                : parseISO(release.plannedDate);
              const isPastRelease = isPast(releaseDate);

              return (
                <div key={release.id}>
                  {/* Today Marker - Show before first future release */}
                  {todayPosition === index && (
                    <div
                      ref={todayRef}
                      className="relative my-8 py-2 border-t-2 border-dashed border-red-500 z-30"
                    >
                      <span className="absolute right-4 -top-3 text-sm font-medium text-red-500 bg-white dark:bg-gray-900 px-2">
                        Today
                      </span>
                    </div>
                  )}

                  {/* Release Node */}
                  <TimelineNode
                    release={release}
                    isLeft={index % 2 === 0}
                    onClick={() => navigate(`/projects/${projectId}/releases/${release.id}`)}
                    isPast={isPastRelease}
                  />
                </div>
              );
            })}

            {/* Today Marker at the end if all releases are past */}
            {todayPosition === sortedReleases.length && (
              <div
                ref={todayRef}
                className="relative my-8 py-2 border-t-2 border-dashed border-red-500 z-30"
              >
                <span className="absolute right-4 -top-3 text-sm font-medium text-red-500 bg-white dark:bg-gray-900 px-2">
                  Today
                </span>
              </div>
            )}
          </div>

          {/* Mobile: Show vertical line on left side */}
          <div className="absolute left-4 top-0 bottom-0 w-0.5 bg-gray-300 dark:bg-gray-700 md:hidden" />
        </div>
      </CardContent>
    </Card>
  );
}

