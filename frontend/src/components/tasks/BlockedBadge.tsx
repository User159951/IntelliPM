import { Lock } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { cn } from '@/lib/utils';

interface BlockedBadgeProps {
  /** Number of tasks blocking this task */
  blockedByCount: number;
  /** Optional array of blocking tasks for tooltip */
  blockingTasks?: Array<{ taskId: number; title: string; status: string }>;
  /** Size variant */
  variant?: 'sm' | 'md' | 'lg';
  /** Additional CSS classes */
  className?: string;
}

const variantConfig = {
  sm: {
    badge: 'h-5 px-1.5 text-xs',
    icon: 'h-3 w-3',
  },
  md: {
    badge: 'h-6 px-2 text-sm',
    icon: 'h-3.5 w-3.5',
  },
  lg: {
    badge: 'h-7 px-3 text-base',
    icon: 'h-4 w-4',
  },
};

/**
 * Badge component for displaying blocked task status.
 * Shows a red badge with lock icon indicating the task is blocked by dependencies.
 * 
 * @example
 * ```tsx
 * // Simple usage
 * <BlockedBadge blockedByCount={2} />
 * 
 * // With tooltip showing blocking tasks
 * <BlockedBadge 
 *   blockedByCount={2}
 *   blockingTasks={[
 *     { taskId: 1, title: "Task A", status: "InProgress" },
 *     { taskId: 2, title: "Task B", status: "Todo" }
 *   ]}
 * />
 * ```
 */
export default function BlockedBadge({
  blockedByCount,
  blockingTasks,
  variant = 'md',
  className,
}: BlockedBadgeProps) {
  const config = variantConfig[variant];

  const badgeContent = (
    <Badge
      className={cn(
        'inline-flex items-center gap-1 rounded-full bg-red-500 text-white dark:bg-red-600',
        config.badge,
        className
      )}
    >
      {variant === 'sm' ? (
        <span>🔒</span>
      ) : (
        <>
          <Lock className={config.icon} />
          <span>
            {variant === 'md' ? 'Blocked' : `Blocked by ${blockedByCount}`}
          </span>
        </>
      )}
    </Badge>
  );

  // Build tooltip content
  const tooltipContent = blockingTasks && blockingTasks.length > 0
    ? (() => {
        const maxShown = 5;
        const shown = blockingTasks.slice(0, maxShown);
        const remaining = blockingTasks.length - maxShown;

        return (
          <div className="space-y-1">
            <p className="font-semibold text-sm">Blocked by:</p>
            <ul className="list-disc list-inside space-y-0.5 text-xs">
              {shown.map((task) => (
                <li key={task.taskId}>
                  {task.title} <span className="text-muted-foreground">({task.status})</span>
                </li>
              ))}
            </ul>
            {remaining > 0 && (
              <p className="text-xs text-muted-foreground mt-1">
                + {remaining} more
              </p>
            )}
          </div>
        );
      })()
    : `This task is blocked by ${blockedByCount} ${blockedByCount === 1 ? 'dependency' : 'dependencies'}`;

  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>
          {badgeContent}
        </TooltipTrigger>
        <TooltipContent side="top" className="max-w-xs">
          {typeof tooltipContent === 'string' ? (
            <p className="text-sm">{tooltipContent}</p>
          ) : (
            tooltipContent
          )}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}

