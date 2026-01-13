import { Badge } from '@/components/ui/badge';
import { Shield, User } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useTranslation } from 'react-i18next';

/**
 * Props for the RoleBadge component.
 */
export interface RoleBadgeProps {
  /** The user role to display */
  role: 'Admin' | 'User';
  /** Size variant of the badge */
  size?: 'sm' | 'md' | 'lg';
  /** Additional CSS classes */
  className?: string;
}

const getRoleConfig = (t: (key: string) => string): Record<
  'Admin' | 'User',
  {
    label: string;
    icon: React.ComponentType<{ className?: string }>;
    variant: 'default' | 'secondary' | 'outline' | 'destructive';
    customClassName: string;
  }
> => ({
  Admin: {
    label: t('common:roles.admin'),
    icon: Shield,
    variant: 'default',
    customClassName: 'bg-red-600 text-white border-transparent hover:bg-red-600/80 dark:bg-red-500 dark:hover:bg-red-500/80',
  },
  User: {
    label: t('common:roles.user'),
    icon: User,
    variant: 'secondary',
    customClassName: 'bg-blue-600 text-white border-transparent hover:bg-blue-600/80 dark:bg-blue-500 dark:hover:bg-blue-500/80',
  },
});

const sizeClasses = {
  sm: 'text-xs px-1.5 py-0.5',
  md: 'text-sm px-2 py-1',
  lg: 'text-base px-2.5 py-1.5',
};

const iconSizeClasses = {
  sm: 'h-3 w-3',
  md: 'h-3.5 w-3.5',
  lg: 'h-4 w-4',
};

/**
 * RoleBadge component for displaying user roles (Admin/User).
 * 
 * @example
 * ```tsx
 * <RoleBadge role="Admin" size="md" />
 * <RoleBadge role="User" size="sm" />
 * ```
 */
export function RoleBadge({ role, size = 'md', className }: RoleBadgeProps) {
  const { t } = useTranslation('common');
  const roleConfig = getRoleConfig(t);
  const config = roleConfig[role];
  const Icon = config.icon;

  return (
    <Badge
      variant={config.variant}
      className={cn(
        'flex items-center gap-1.5 font-medium',
        config.customClassName,
        sizeClasses[size],
        className
      )}
      aria-label={`Role: ${config.label}`}
    >
      <Icon className={iconSizeClasses[size]} aria-hidden="true" />
      <span>{config.label}</span>
    </Badge>
  );
}

