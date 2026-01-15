import { ReactNode } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Inbox } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
  className?: string;
}

export function EmptyState({
  icon,
  title,
  description,
  action,
  className,
}: EmptyStateProps) {
  const DefaultIcon = Inbox;

  return (
    <Card
      className={cn(
        'flex flex-col items-center justify-center py-16',
        className
      )}
      role="status"
      aria-live="polite"
    >
      <CardContent className="flex flex-col items-center justify-center text-center">
        <div className="mb-4 text-muted-foreground" aria-hidden="true">
          {icon ? (
            icon
          ) : (
            <DefaultIcon className="h-12 w-12 opacity-50" />
          )}
        </div>
        <h3 className="text-lg font-medium mb-2">{title}</h3>
        {description && (
          <p className="text-sm text-muted-foreground mb-4 max-w-md">
            {description}
          </p>
        )}
        {action && (
          <Button onClick={action.onClick} aria-label={action.label}>
            {action.label}
          </Button>
        )}
      </CardContent>
    </Card>
  );
}
