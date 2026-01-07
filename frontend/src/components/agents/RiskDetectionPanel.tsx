import { useState, useCallback, useEffect } from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, AlertTriangle, RefreshCw, AlertCircle } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from "@/lib/sweetalert";
import { useRequestDeduplication } from '@/hooks/useRequestDeduplication';
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';
import { CollapsibleAIResponse } from './CollapsibleAIResponse';
interface Props {
  projectId: number;
}

export function RiskDetectionPanel({ projectId }: Props) {
  const [loading, setLoading] = useState(false);
  const [analysis, setAnalysis] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [canRetry, setCanRetry] = useState(false);
  const { executeWithDeduplication, isRequestInFlight } = useRequestDeduplication();
  const { handleError, executeWithErrorHandling, isBlocked, getErrorMessage } = useAIErrorHandler({
    showToast: false, // We'll handle toasts manually
  });

  // Check if blocked on mount
  useEffect(() => {
    if (isBlocked()) {
      const errorMsg = getErrorMessage();
      if (errorMsg) {
        setError(errorMsg);
      }
    }
  }, [isBlocked, getErrorMessage]);

  const handleDetect = useCallback(async () => {
    const requestKey = `risk-detection-${projectId}`;
    
    // Prevent double execution
    if (isRequestInFlight(requestKey)) {
      showToast('Risk detection already in progress', 'info');
      return;
    }

    if (isBlocked()) {
      const errorMsg = getErrorMessage();
      if (errorMsg) {
        setError(errorMsg);
      }
      return;
    }

    setLoading(true);
    setError(null);
    setCanRetry(false);

    try {
      const result = await executeWithDeduplication(requestKey, async () => {
        return await executeWithErrorHandling(
          () => agentsApi.detectRisks(projectId),
          40000 // Estimated 40 seconds
        );
      });

      if (result === null) {
        setLoading(false);
        return;
      }

      const apiResponse = result as { content?: string; Content?: string; executionTimeMs?: number; ExecutionTimeMs?: number };
      const content = apiResponse.content ?? apiResponse.Content;
      const time = apiResponse.executionTimeMs ?? apiResponse.ExecutionTimeMs;

      setAnalysis(content || 'No risks detected');
      showToast(time ? `Risk Detection Complete - Completed in ${time}ms` : 'Risk Detection Complete', 'success');
    } catch (error) {
      const errorResult = handleError(error as Error);
      setError((error as Error).message || 'Unable to detect risks for this project');
      setCanRetry(errorResult.canRetry);
      
      if (!errorResult.isQuotaExceeded && !errorResult.isAIDisabled) {
        showError("Risk Detection Failed", (error as Error).message || "Unable to detect risks for this project");
      }
    } finally {
      setLoading(false);
    }
  }, [projectId, isRequestInFlight, executeWithDeduplication, executeWithErrorHandling, handleError, isBlocked, getErrorMessage]);

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-amber-500" />
              Risk Detection
            </CardTitle>
            <CardDescription>Proactive risk and blocker identification</CardDescription>
          </div>
          <Button onClick={handleDetect} disabled={loading || isBlocked()}>
            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {loading ? 'Analyzing... (up to 40s)' : 'Detect Risks'}
          </Button>
        </div>
      </CardHeader>

      {error && !loading && (
        <CardContent>
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription className="space-y-2">
              <p>{error}</p>
              {canRetry && (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={handleDetect}
                  className="mt-2"
                >
                  <RefreshCw className="mr-2 h-3 w-3" />
                  Réessayer
                </Button>
              )}
            </AlertDescription>
          </Alert>
        </CardContent>
      )}

      {analysis && (
        <CardContent>
          <Alert>
            <AlertDescription>
              <CollapsibleAIResponse
                content={analysis}
                storageKey={`risk-detection-${projectId}`}
              />
            </AlertDescription>
          </Alert>
        </CardContent>
      )}
    </Card>
  );
}

