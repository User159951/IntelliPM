import { Badge } from '@/components/ui/badge';
import { Crown, Users, Code, Bug, Eye } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ProjectRole } from '@/types';
import { useTranslation } from 'react-i18next';

interface RoleBadgeProps {
  role: string;
  className?: string;
}

const getRoleConfig = (t: (key: string) => string): Record<
  ProjectRole,
  {
    label: string;
    icon: React.ComponentType<{ className?: string }>;
    variant: 'default' | 'secondary' | 'outline' | 'destructive';
    customClassName?: string;
  }
> => ({
  ProductOwner: {
    label: t('common:roles.productOwner'),
    icon: Crown,
    variant: 'default',
    customClassName: 'bg-blue-600 text-white border-transparent hover:bg-blue-600/80',
  },
  ScrumMaster: {
    label: t('common:roles.scrumMaster'),
    icon: Users,
    variant: 'secondary',
    customClassName: 'bg-purple-600 text-white border-transparent hover:bg-purple-600/80',
  },
  Developer: {
    label: t('common:roles.developer'),
    icon: Code,
    variant: 'outline',
    customClassName: 'border-green-500 text-green-600 dark:text-green-400 bg-green-500/10',
  },
  Tester: {
    label: t('common:roles.tester'),
    icon: Bug,
    variant: 'outline',
    customClassName: 'border-orange-500 text-orange-600 dark:text-orange-400 bg-orange-500/10',
  },
  Viewer: {
    label: t('common:roles.viewer'),
    icon: Eye,
    variant: 'secondary',
    customClassName: 'bg-gray-500/10 text-gray-600 dark:text-gray-400 border-gray-500/20',
  },
});

export default function RoleBadge({ role, className }: RoleBadgeProps) {
  const { t } = useTranslation('common');
  const normalizedRole = role as ProjectRole;
  const roleConfig = getRoleConfig(t);
  const config = roleConfig[normalizedRole];

  if (!config) {
    // Fallback for unknown roles
    return (
      <Badge variant="secondary" className={cn('flex items-center gap-1', className)}>
        {role}
      </Badge>
    );
  }

  const Icon = config.icon;

  return (
    <Badge
      variant={config.variant}
      className={cn(
        'flex items-center gap-1',
        config.customClassName,
        className
      )}
    >
      <Icon className="h-3 w-3" />
      {config.label}
    </Badge>
  );
}

