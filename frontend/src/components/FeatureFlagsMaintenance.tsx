import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { AlertTriangle, RefreshCw } from 'lucide-react';

interface FeatureFlagsMaintenanceProps {
  error: Error;
  onRetry: () => void;
}

/**
 * Maintenance message component shown when feature flags API fails.
 * This ensures we don't show features with assumed/default values.
 * 
 * @example
 * ```tsx
 * <FeatureFlagsMaintenance 
 *   error={error} 
 *   onRetry={() => refetch()} 
 * />
 * ```
 */
export function FeatureFlagsMaintenance({ error, onRetry }: FeatureFlagsMaintenanceProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <div className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            <CardTitle>Service Unavailable</CardTitle>
          </div>
          <CardDescription>
            We're unable to load feature configuration at this time. 
            Please try again in a few moments.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {import.meta.env.DEV && (
            <div className="rounded-md bg-muted p-3">
              <p className="text-sm font-mono text-muted-foreground">{error.message}</p>
            </div>
          )}
          <div className="flex flex-col gap-2">
            <Button onClick={onRetry} className="w-full">
              <RefreshCw className="mr-2 h-4 w-4" />
              Retry
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

