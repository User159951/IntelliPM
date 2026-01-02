import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertCircle, Zap } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { cn } from '@/lib/utils';
import { aiGovernanceApi } from '@/api/aiGovernance';

interface QuotaStatusWidgetProps {
  organizationId?: number;
  compact?: boolean;
}

function getPercentageColor(percentage: number): string {
  if (percentage >= 80) return 'bg-red-500';
  if (percentage >= 50) return 'bg-yellow-500';
  return 'bg-green-500';
}

function getTierBadgeClass(tierName: string): string {
  switch (tierName.toLowerCase()) {
    case 'enterprise':
      return 'bg-purple-500/10 text-purple-500 border-purple-500/20';
    case 'pro':
      return 'bg-blue-500/10 text-blue-500 border-blue-500/20';
    case 'free':
      return 'bg-gray-500/10 text-gray-500 border-gray-500/20';
    default:
      return 'bg-red-500/10 text-red-500 border-red-500/20';
  }
}

export function QuotaStatusWidget({ organizationId: propOrgId, compact = false }: QuotaStatusWidgetProps) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const organizationId = propOrgId || user?.organizationId || 0;

  const { data: quota, isLoading } = useQuery({
    queryKey: ['ai-quota', organizationId],
    queryFn: () => aiGovernanceApi.getQuotaStatus(organizationId),
    enabled: organizationId > 0,
    refetchInterval: 60000, // Refresh every 60 seconds
  });

  if (!organizationId) {
    return null;
  }

  if (isLoading) {
    if (compact) {
      return (
        <div className="p-2 space-y-2">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-2 w-full" />
        </div>
      );
    }
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <Skeleton className="h-6 w-1/3" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-2 w-full" />
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!quota) {
    return null;
  }

  if (compact) {
    const maxPercentage = Math.max(
      quota.requestsPercentage,
      quota.tokensPercentage,
      quota.decisionsPercentage
    );

    return (
      <div className="p-2 space-y-2 border-t">
        <div className="flex items-center justify-between text-xs">
          <div className="flex items-center gap-1.5">
            <Zap className="h-3 w-3 text-muted-foreground" />
            <span className="text-muted-foreground">AI Quota</span>
          </div>
          <Badge
            variant="outline"
            className={cn('text-xs', getTierBadgeClass(quota.tierName))}
          >
            {quota.tierName}
          </Badge>
        </div>
        <div className="space-y-1">
          <div className="flex items-center justify-between text-xs">
            <span className="text-muted-foreground">Usage</span>
            <span className={cn(
              'font-medium',
              maxPercentage >= 80 ? 'text-red-500' : maxPercentage >= 50 ? 'text-yellow-500' : 'text-green-500'
            )}>
              {maxPercentage.toFixed(0)}%
            </span>
          </div>
          <Progress value={maxPercentage} className="h-1.5" />
        </div>
        {quota.isAlertThreshold && (
          <Button
            variant="outline"
            size="sm"
            className="w-full h-7 text-xs"
            onClick={() => navigate('/settings/ai-quota')}
          >
            Upgrade
          </Button>
        )}
      </div>
    );
  }

  const canUpgrade = quota.tierName === 'Free' || quota.tierName === 'Pro';

  return (
    <Card className="w-full">
      <CardHeader>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <CardTitle>AI Quota Status</CardTitle>
            <Badge variant="outline" className={getTierBadgeClass(quota.tierName)}>
              {quota.tierName}
            </Badge>
          </div>
          {canUpgrade && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => navigate('/settings/billing')}
            >
              Upgrade
            </Button>
          )}
        </div>
        <CardDescription>
          Reset date: {new Date(quota.resetDate).toLocaleDateString()}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Requests */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="font-medium">Requests</span>
            <span className={cn(
              'text-sm',
              quota.requestsPercentage >= 80 ? 'text-red-500 font-semibold' :
              quota.requestsPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
            )}>
              {quota.currentRequests.toLocaleString()} / {quota.maxRequests.toLocaleString()}
              {' '}({quota.requestsPercentage.toFixed(0)}%)
            </span>
          </div>
          <Progress
            value={quota.requestsPercentage}
            className={cn('h-2', getPercentageColor(quota.requestsPercentage))}
          />
        </div>

        {/* Tokens */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="font-medium">Tokens</span>
            <span className={cn(
              'text-sm',
              quota.tokensPercentage >= 80 ? 'text-red-500 font-semibold' :
              quota.tokensPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
            )}>
              {quota.currentTokens.toLocaleString()} / {quota.maxTokens.toLocaleString()}
              {' '}({quota.tokensPercentage.toFixed(0)}%)
            </span>
          </div>
          <Progress
            value={quota.tokensPercentage}
            className={cn('h-2', getPercentageColor(quota.tokensPercentage))}
          />
        </div>

        {/* Decisions */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="font-medium">Decisions</span>
            <span className={cn(
              'text-sm',
              quota.decisionsPercentage >= 80 ? 'text-red-500 font-semibold' :
              quota.decisionsPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
            )}>
              {quota.currentDecisions.toLocaleString()} / {quota.maxDecisions.toLocaleString()}
              {' '}({quota.decisionsPercentage.toFixed(0)}%)
            </span>
          </div>
          <Progress
            value={quota.decisionsPercentage}
            className={cn('h-2', getPercentageColor(quota.decisionsPercentage))}
          />
        </div>

        {quota.isAlertThreshold && (
          <div className="flex items-center gap-2 p-3 rounded-lg bg-yellow-500/10 border border-yellow-500/20">
            <AlertCircle className="h-4 w-4 text-yellow-500" />
            <span className="text-sm text-yellow-600 dark:text-yellow-400">
              You've used over 80% of your monthly quota. Consider upgrading to continue using AI features.
            </span>
          </div>
        )}

        {/* View Details Button */}
        <div className="pt-2">
          <Button
            variant="outline"
            size="sm"
            className="w-full"
            onClick={() => navigate('/settings/ai-quota')}
          >
            View Details
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

