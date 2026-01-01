import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { AlertCircle } from 'lucide-react';
import { getLastAIDisabledError, clearAIDisabledError, AIDisabledErrorDetails } from '@/api/client';
import { useEffect, useState } from 'react';

export function AIDisabledAlert() {
  const [aiDisabledError, setAIDisabledError] = useState<AIDisabledErrorDetails | null>(null);

  useEffect(() => {
    // Check for AI disabled error on mount and when storage changes
    const error = getLastAIDisabledError();
    setAIDisabledError(error);

    // Listen for AI disabled error updates (can be triggered by API client)
    const interval = setInterval(() => {
      const latestError = getLastAIDisabledError();
      if (latestError !== aiDisabledError) {
        setAIDisabledError(latestError);
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [aiDisabledError]);

  if (!aiDisabledError) {
    return null;
  }

  const handleDismiss = () => {
    clearAIDisabledError();
    setAIDisabledError(null);
  };

  return (
    <Alert variant="destructive" className="mb-4">
      <AlertCircle className="h-4 w-4" />
      <AlertTitle>IA désactivée</AlertTitle>
      <AlertDescription className="mt-2">
        <p>
          L&apos;IA a été désactivée pour votre organisation. Contactez un administrateur pour plus d&apos;informations.
        </p>
        {aiDisabledError.reason && (
          <p className="text-sm mt-2 text-muted-foreground">
            Raison: {aiDisabledError.reason}
          </p>
        )}
        <button
          onClick={handleDismiss}
          className="text-sm underline mt-2 hover:text-foreground"
        >
          Fermer
        </button>
      </AlertDescription>
    </Alert>
  );
}

