import { memo } from 'react';
import {
  Circle,
  Clock,
  CheckCircle2,
  XCircle,
  type LucideIcon,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { TaskStatus } from '@/types';

/**
 * Props for the StatusBadge component.
 */
export interface StatusBadgeProps {
  /** Task status to display */
  status: TaskStatus;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
  /** Visual variant */
  variant?: 'default' | 'dot' | 'outline';
  /** Whether to show the status icon */
  showIcon?: boolean;
  /** Additional CSS classes */
  className?: string;
}

/**
 * Status configuration with colors, icons, and labels.
 */
interface StatusConfig {
  label: string;
  icon: LucideIcon;
  color: {
    bg: string;
    text: string;
    border: string;
    dot: string;
  };
}

const statusConfig: Record<TaskStatus, StatusConfig> = {
  Todo: {
    label: 'To Do',
    icon: Circle,
    color: {
      bg: 'bg-blue-100 dark:bg-blue-950',
      text: 'text-blue-700 dark:text-blue-300',
      border: 'border-blue-300 dark:border-blue-700',
      dot: 'bg-blue-500',
    },
  },
  InProgress: {
    label: 'In Progress',
    icon: Clock,
    color: {
      bg: 'bg-yellow-100 dark:bg-yellow-950',
      text: 'text-yellow-700 dark:text-yellow-300',
      border: 'border-yellow-300 dark:border-yellow-700',
      dot: 'bg-yellow-500',
    },
  },
  Done: {
    label: 'Done',
    icon: CheckCircle2,
    color: {
      bg: 'bg-green-100 dark:bg-green-950',
      text: 'text-green-700 dark:text-green-300',
      border: 'border-green-300 dark:border-green-700',
      dot: 'bg-green-500',
    },
  },
  Blocked: {
    label: 'Blocked',
    icon: XCircle,
    color: {
      bg: 'bg-red-100 dark:bg-red-950',
      text: 'text-red-700 dark:text-red-300',
      border: 'border-red-300 dark:border-red-700',
      dot: 'bg-red-500',
    },
  },
};

/**
 * Size configuration for different badge sizes.
 */
const sizeConfig = {
  sm: {
    badge: 'text-xs px-2 py-0.5',
    icon: 'h-3 w-3',
    dot: 'h-1.5 w-1.5',
  },
  md: {
    badge: 'text-sm px-2.5 py-1',
    icon: 'h-4 w-4',
    dot: 'h-2 w-2',
  },
  lg: {
    badge: 'text-base px-3 py-1.5',
    icon: 'h-5 w-5',
    dot: 'h-2.5 w-2.5',
  },
};

/**
 * StatusBadge component for displaying task status with appropriate styling.
 * 
 * Features:
 * - Consistent color scheme across the application
 * - Multiple size variants (sm, md, lg)
 * - Multiple visual variants (default, outline, dot)
 * - Icon support with toggle
 * - Dark mode support
 * - Accessible (role="status")
 * - Memoized for performance
 * 
 * @example
 * ```tsx
 * // Default variant
 * <StatusBadge status="InProgress" />
 * 
 * // Small size with outline variant
 * <StatusBadge status="Done" size="sm" variant="outline" />
 * 
 * // Large size without icon
 * <StatusBadge status="Blocked" size="lg" showIcon={false} />
 * 
 * // Dot variant
 * <StatusBadge status="Todo" variant="dot" />
 * ```
 */
function StatusBadge({
  status,
  size = 'md',
  variant = 'default',
  showIcon = true,
  className,
}: StatusBadgeProps) {
  const config = statusConfig[status];

  if (!config) {
    // Fallback for unknown status
    return (
      <span
        className={cn(
          'inline-flex items-center gap-1.5 rounded-full font-medium bg-muted text-muted-foreground',
          sizeConfig[size].badge,
          className
        )}
        role="status"
        aria-label={`Status: ${status}`}
      >
        {status}
      </span>
    );
  }

  const Icon = config.icon;
  const sizeStyles = sizeConfig[size];

  // Default variant: Filled background with colored text
  if (variant === 'default') {
    return (
      <span
        className={cn(
          'inline-flex items-center gap-1.5 rounded-full font-medium',
          config.color.bg,
          config.color.text,
          sizeStyles.badge,
          className
        )}
        role="status"
        aria-label={`Status: ${config.label}`}
      >
        {showIcon && <Icon className={sizeStyles.icon} aria-hidden="true" />}
        <span>{config.label}</span>
      </span>
    );
  }

  // Outline variant: Border only, transparent background
  if (variant === 'outline') {
    return (
      <span
        className={cn(
          'inline-flex items-center gap-1.5 rounded-full font-medium border-2 bg-transparent',
          config.color.text,
          config.color.border,
          sizeStyles.badge,
          className
        )}
        role="status"
        aria-label={`Status: ${config.label}`}
      >
        {showIcon && <Icon className={sizeStyles.icon} aria-hidden="true" />}
        <span>{config.label}</span>
      </span>
    );
  }

  // Dot variant: Colored dot with text (minimal)
  return (
    <span
      className={cn(
        'inline-flex items-center gap-2',
        sizeStyles.badge,
        className
      )}
      role="status"
      aria-label={`Status: ${config.label}`}
    >
      <span
        className={cn(
          'rounded-full flex-shrink-0',
          config.color.dot,
          sizeStyles.dot
        )}
        aria-hidden="true"
      />
      <span className="text-gray-700 dark:text-gray-300">{config.label}</span>
    </span>
  );
}

// Memoize component for performance when used in large lists
export default memo(StatusBadge);

/**
 * Get the text color class for a given status.
 * Useful for custom implementations or styling.
 * 
 * @param status - The task status
 * @returns Tailwind text color class
 * 
 * @example
 * ```tsx
 * <span className={getStatusColor('InProgress')}>
 *   Custom status display
 * </span>
 * ```
 */
// eslint-disable-next-line react-refresh/only-export-components
export function getStatusColor(status: TaskStatus): string {
  return statusConfig[status]?.color.text || 'text-gray-700 dark:text-gray-300';
}

/**
 * Get the display label for a given status.
 * 
 * @param status - The task status
 * @returns Human-readable label
 * 
 * @example
 * ```tsx
 * <span>{getStatusLabel(task.status)}</span>
 * ```
 */
// eslint-disable-next-line react-refresh/only-export-components
export function getStatusLabel(status: TaskStatus): string {
  return statusConfig[status]?.label || status;
}

/**
 * Get the icon component for a given status.
 * 
 * @param status - The task status
 * @returns Lucide icon component
 * 
 * @example
 * ```tsx
 * const StatusIcon = getStatusIcon('Done');
 * <StatusIcon className="h-4 w-4" />
 * ```
 */
// eslint-disable-next-line react-refresh/only-export-components
export function getStatusIcon(status: TaskStatus): LucideIcon {
  return statusConfig[status]?.icon || Circle;
}

