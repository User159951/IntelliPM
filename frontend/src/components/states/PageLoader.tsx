import { Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface PageLoaderProps {
  /**
   * Custom message to display
   */
  message?: string;
  /**
   * Custom className
   */
  className?: string;
  /**
   * Size of the loader
   */
  size?: 'sm' | 'md' | 'lg';
}

const sizeClasses = {
  sm: 'h-4 w-4',
  md: 'h-8 w-8',
  lg: 'h-12 w-12',
};

export function PageLoader({
  message = 'Loading...',
  className,
  size = 'lg',
}: PageLoaderProps) {
  return (
    <div
      className={cn(
        'flex min-h-[400px] flex-col items-center justify-center',
        className
      )}
      role="status"
      aria-live="polite"
      aria-label={message}
    >
      <Loader2
        className={cn('animate-spin text-primary', sizeClasses[size])}
        aria-hidden="true"
      />
      {message && (
        <p className="mt-4 text-sm text-muted-foreground">{message}</p>
      )}
    </div>
  );
}
