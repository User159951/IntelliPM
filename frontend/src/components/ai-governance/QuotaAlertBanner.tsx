import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { AlertTriangle, XCircle, Ban } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@/contexts/AuthContext';
import { aiGovernanceApi } from '@/api/aiGovernance';

interface QuotaAlertBannerProps {
  organizationId?: number;
}

export function QuotaAlertBanner({ organizationId: propOrgId }: QuotaAlertBannerProps) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const organizationId = propOrgId || user?.organizationId || 0;

  const { data: quota } = useQuery({
    queryKey: ['ai-quota', organizationId],
    queryFn: () => aiGovernanceApi.getQuotaStatus(organizationId),
    enabled: organizationId > 0,
    refetchInterval: 60000, // Refresh every 60 seconds
  });

  if (!quota || (!quota.isAlertThreshold && !quota.isDisabled)) {
    return null;
  }

  // Check if quota is exceeded (100%+)
  const requestsPct = quota.requestsPercentage ?? quota.usage.requestsPercentage;
  const tokensPct = quota.tokensPercentage ?? quota.usage.tokensPercentage;
  const decisionsPct = quota.decisionsPercentage ?? 0;
  const isExceeded = requestsPct >= 100 || tokensPct >= 100 || decisionsPct >= 100;

  if (quota.isDisabled) {
    return (
      <Alert variant="destructive" className="mb-4">
        <Ban className="h-4 w-4" />
        <AlertTitle>AI Disabled</AlertTitle>
        <AlertDescription className="flex items-center justify-between gap-4">
          <span>
            ❌ L&apos;IA a été désactivée pour votre organisation. Contactez{' '}
            <a href="mailto:support@intellipm.com" className="underline">
              support@intellipm.com
            </a>
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => navigate('/settings/ai-quota')}
          >
            View Details
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  if (isExceeded) {
    return (
      <Alert variant="destructive" className="mb-4">
        <XCircle className="h-4 w-4" />
        <AlertTitle>Quota Exceeded</AlertTitle>
        <AlertDescription className="flex items-center justify-between gap-4">
          <span>
            🚫 Quota mensuel dépassé. Contactez votre administrateur pour augmenter votre quota.
          </span>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => navigate('/settings/ai-quota')}
          >
            Voir détails
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  // Alert threshold (80%+)
  return (
    <Alert variant="default" className="mb-4 border-yellow-500 bg-yellow-500/10">
      <AlertTriangle className="h-4 w-4 text-yellow-600" />
      <AlertTitle className="text-yellow-800 dark:text-yellow-200">
        Quota Warning
      </AlertTitle>
      <AlertDescription className="flex items-center justify-between gap-4">
        <span className="text-yellow-700 dark:text-yellow-300">
          ⚠️ Vous avez utilisé {Math.max(
            quota.requestsPercentage ?? quota.usage.requestsPercentage,
            quota.tokensPercentage ?? quota.usage.tokensPercentage,
            quota.decisionsPercentage ?? 0
          ).toFixed(0)}% de votre quota mensuel. Contactez votre administrateur pour augmenter votre quota.
        </span>
        <Button
          variant="ghost"
          size="sm"
          className="text-yellow-700 hover:bg-yellow-500/20"
          onClick={() => navigate('/settings/ai-quota')}
        >
          Voir détails
        </Button>
      </AlertDescription>
    </Alert>
  );
}

