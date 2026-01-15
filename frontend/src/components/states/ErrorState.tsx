import { ReactNode } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { AlertCircle, RefreshCw } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface ErrorStateProps {
  /**
   * Error title
   */
  title?: string;
  /**
   * Error message/description
   */
  message?: string;
  /**
   * Custom error icon
   */
  icon?: ReactNode;
  /**
   * Retry function to call when retry button is clicked
   */
  onRetry?: () => void;
  /**
   * Custom retry button label
   */
  retryLabel?: string;
  /**
   * Additional actions to display
   */
  actions?: ReactNode;
  /**
   * Custom className
   */
  className?: string;
}

export function ErrorState({
  title = 'Something went wrong',
  message = 'We encountered an error while loading this content. Please try again.',
  icon,
  onRetry,
  retryLabel = 'Try again',
  actions,
  className,
}: ErrorStateProps) {
  const IconComponent = icon || AlertCircle;

  return (
    <Card
      className={cn('flex flex-col items-center justify-center py-16', className)}
      role="alert"
      aria-live="assertive"
    >
      <CardHeader className="text-center">
        <div className="mb-4 flex justify-center">
          <div className="text-destructive" aria-hidden="true">
            {typeof IconComponent === 'function' ? (
              <IconComponent className="h-12 w-12" />
            ) : (
              IconComponent
            )}
          </div>
        </div>
        <CardTitle>{title}</CardTitle>
        {message && <CardDescription className="mt-2">{message}</CardDescription>}
      </CardHeader>
      <CardContent className="flex flex-col items-center gap-2">
        {onRetry && (
          <Button onClick={onRetry} variant="default" aria-label={retryLabel}>
            <RefreshCw className="mr-2 h-4 w-4" />
            {retryLabel}
          </Button>
        )}
        {actions}
      </CardContent>
    </Card>
  );
}
