import { Skeleton } from '@/components/ui/skeleton';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { cn } from '@/lib/utils';

export interface LoadingStateProps {
  /**
   * Number of skeleton items to display
   * @default 3
   */
  count?: number;
  /**
   * Layout variant: 'grid' for grid layout, 'list' for vertical list
   * @default 'grid'
   */
  variant?: 'grid' | 'list';
  /**
   * Show card wrapper around skeletons
   * @default true
   */
  showCard?: boolean;
  /**
   * Custom className for container
   */
  className?: string;
  /**
   * Custom skeleton height
   */
  skeletonHeight?: string;
}

export function LoadingState({
  count = 3,
  variant = 'grid',
  showCard = true,
  className,
  skeletonHeight,
}: LoadingStateProps) {
  const skeletons = Array.from({ length: count }, (_, i) => i);

  const SkeletonItem = () => {
    if (showCard) {
      return (
        <Card>
          <CardHeader>
            <Skeleton className="h-5 w-32" />
            <Skeleton className="h-4 w-48" />
          </CardHeader>
          <CardContent>
            <Skeleton
              className={cn('h-4 w-full', skeletonHeight)}
              style={skeletonHeight ? { height: skeletonHeight } : undefined}
            />
          </CardContent>
        </Card>
      );
    }

    return (
      <Skeleton
        className={cn('w-full', skeletonHeight || 'h-32')}
        style={skeletonHeight ? { height: skeletonHeight } : undefined}
      />
    );
  };

  return (
    <div
      className={cn(
        variant === 'grid'
          ? 'grid gap-4 md:grid-cols-2 lg:grid-cols-3'
          : 'space-y-4',
        className
      )}
      role="status"
      aria-live="polite"
      aria-label="Loading content"
    >
      {skeletons.map((i) => (
        <SkeletonItem key={i} />
      ))}
    </div>
  );
}
