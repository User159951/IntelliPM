import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { AlertTriangle, Home, RefreshCw } from 'lucide-react';
import * as Sentry from '@sentry/react';
import { showSuccess } from '@/lib/sweetalert';

interface ErrorFallbackProps {
  error: unknown;
  resetError: () => void;
}

export function ErrorFallback({ error, resetError }: ErrorFallbackProps) {
  const navigate = useNavigate();
  const errorObj = error instanceof Error ? error : new Error(String(error));
  
  const handleReportError = async () => {
    Sentry.captureException(errorObj);
    // Show feedback that error was reported
    await showSuccess('Error reported', 'Thank you for helping us improve!');
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <div className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            <CardTitle>Something went wrong</CardTitle>
          </div>
          <CardDescription>
            We're sorry, but something unexpected happened. Our team has been notified.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {import.meta.env.DEV && (
            <div className="rounded-md bg-muted p-3">
              <p className="text-sm font-mono text-muted-foreground">{errorObj.message}</p>
            </div>
          )}
          <div className="flex flex-col gap-2">
            <Button onClick={resetError} className="w-full">
              <RefreshCw className="mr-2 h-4 w-4" />
              Try again
            </Button>
            {import.meta.env.VITE_SENTRY_DSN && (
              <Button onClick={handleReportError} variant="outline" className="w-full">
                Report this error
              </Button>
            )}
            <Button
              onClick={() => navigate('/')}
              variant="outline"
              className="w-full"
            >
              <Home className="mr-2 h-4 w-4" />
              Go back home
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

