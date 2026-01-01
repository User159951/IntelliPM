import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { AlertTriangle, ArrowRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { getLastQuotaError, clearQuotaError, QuotaErrorDetails } from '@/api/client';
import { useEffect, useState } from 'react';

export function QuotaExceededAlert() {
  const navigate = useNavigate();
  const [quotaError, setQuotaError] = useState<QuotaErrorDetails | null>(null);

  useEffect(() => {
    // Check for quota error on mount and when storage changes
    const error = getLastQuotaError();
    setQuotaError(error);

    // Listen for quota error updates (can be triggered by API client)
    const interval = setInterval(() => {
      const latestError = getLastQuotaError();
      if (latestError !== quotaError) {
        setQuotaError(latestError);
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [quotaError]);

  if (!quotaError) {
    return null;
  }

  const handleUpgrade = () => {
    clearQuotaError();
    navigate(quotaError.upgradeUrl);
  };

  const handleDismiss = () => {
    clearQuotaError();
    setQuotaError(null);
  };

  const quotaTypeDisplay = quotaError.quotaType === 'Requests' 
    ? 'requêtes' 
    : quotaError.quotaType === 'Tokens' 
    ? 'tokens' 
    : quotaError.quotaType.toLowerCase();

  const nextTier = quotaError.tierName === 'Free' 
    ? 'Pro' 
    : quotaError.tierName === 'Pro' 
    ? 'Enterprise' 
    : 'Enterprise';

  return (
    <Alert variant="destructive" className="mb-4">
      <AlertTriangle className="h-4 w-4" />
      <AlertTitle>Quota AI dépassé</AlertTitle>
      <AlertDescription className="mt-2">
        <div className="flex items-center justify-between flex-wrap gap-4">
          <div className="flex-1">
            <p className="mb-2">
              Vous avez atteint la limite mensuelle de {quotaTypeDisplay} ({quotaError.currentUsage.toLocaleString()}/{quotaError.maxLimit.toLocaleString()}).
            </p>
            <p className="text-sm">
              Passez au plan <strong>{nextTier}</strong> pour continuer à utiliser les fonctionnalités AI.
            </p>
            <div className="mt-2 flex items-center gap-2">
              <span className="text-sm text-muted-foreground">Plan actuel:</span>
              <Badge variant="outline">{quotaError.tierName}</Badge>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Button 
              variant="outline" 
              size="sm"
              onClick={handleDismiss}
            >
              Fermer
            </Button>
            <Button 
              variant="default" 
              size="sm"
              onClick={handleUpgrade}
            >
              Mettre à niveau
              <ArrowRight className="h-4 w-4 ml-2" />
            </Button>
          </div>
        </div>
      </AlertDescription>
    </Alert>
  );
}

