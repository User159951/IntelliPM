import { useState, useCallback, useEffect } from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, TrendingUp, RefreshCw, AlertCircle } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from "@/lib/sweetalert";
import { useRequestDeduplication } from '@/hooks/useRequestDeduplication';
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';
import { CollapsibleAIResponse } from './CollapsibleAIResponse';
interface Props {
  projectId: number;
}

export function ProjectInsightPanel({ projectId }: Props) {
  const [loading, setLoading] = useState(false);
  const [insight, setInsight] = useState<string | null>(null);
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

  const handleAnalyze = useCallback(async () => {
    const requestKey = `project-insight-${projectId}`;
    
    // Prevent double execution
    if (isRequestInFlight(requestKey)) {
      showToast('Analysis already in progress', 'info');
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
          () => agentsApi.analyzeProject(projectId),
          40000 // Estimated 40 seconds
        );
      });

      if (result === null) {
        setLoading(false);
        return;
      }

      // Backend returns Semantic Kernel AgentResponse (Content, ExecutionTimeMs, etc.)
      const apiResponse = result as { content?: string; Content?: string; executionTimeMs?: number; ExecutionTimeMs?: number };
      const content = apiResponse.content ?? apiResponse.Content;
      const time = apiResponse.executionTimeMs ?? apiResponse.ExecutionTimeMs;

      setInsight(content || 'No summary generated');
      showToast(time ? `Analysis Complete - Completed in ${time}ms` : 'Analysis Complete', 'success');
    } catch (error) {
      const errorResult = handleError(error as Error);
      setError((error as Error).message || 'Unable to analyze project status');
      setCanRetry(errorResult.canRetry);
      
      if (!errorResult.isQuotaExceeded && !errorResult.isAIDisabled) {
        showError("Analysis Failed", (error as Error).message || "Unable to analyze project status");
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
              <TrendingUp className="h-5 w-5" />
              Project Insights
            </CardTitle>
            <CardDescription>AI-powered project status analysis</CardDescription>
          </div>
          <Button onClick={handleAnalyze} disabled={loading || isBlocked()}>
            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {loading ? 'Analyzing... (up to 40s)' : 'Analyze Status'}
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
                  onClick={handleAnalyze}
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

      {insight && (
        <CardContent>
          <Alert>
            <AlertDescription>
              <CollapsibleAIResponse
                content={insight}
                storageKey={`project-insight-${projectId}`}
              />
            </AlertDescription>
          </Alert>
        </CardContent>
      )}
    </Card>
  );
}

