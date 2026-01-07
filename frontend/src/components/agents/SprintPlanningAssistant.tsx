import { useState, useCallback, useEffect } from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, Sparkles, RefreshCw, AlertCircle } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from "@/lib/sweetalert";
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';
interface Props {
  sprintId: number;
}

export function SprintPlanningAssistant({ sprintId }: Props) {
  const [loading, setLoading] = useState(false);
  const [plan, setPlan] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [canRetry, setCanRetry] = useState(false);
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

  const handlePlan = useCallback(async () => {
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
      const response = await executeWithErrorHandling(
        () => agentsApi.planSprint(sprintId),
        45000 // Estimated 45 seconds
      );
      
      const apiResponse = response as { content?: string; Content?: string; executionTimeMs?: number; ExecutionTimeMs?: number; requiresApproval?: boolean; RequiresApproval?: boolean };
      const content = apiResponse.content ?? apiResponse.Content;
      const time = apiResponse.executionTimeMs ?? apiResponse.ExecutionTimeMs;
      const requiresApproval = apiResponse.requiresApproval ?? apiResponse.RequiresApproval;

      let text = content || 'No sprint plan generated';
      if (requiresApproval) {
        text += '\n\nNote: This plan is a proposal and requires human approval before execution.';
      }

      setPlan(text);
      showToast(time ? `Sprint Plan Ready - Generated in ${time}ms` : 'Sprint Plan Ready', 'success');
    } catch (error) {
      const errorResult = handleError(error as Error);
      setError((error as Error).message || 'Unable to generate sprint plan');
      setCanRetry(errorResult.canRetry);
      
      if (!errorResult.isQuotaExceeded && !errorResult.isAIDisabled) {
        showError("Sprint Planning Failed", (error as Error).message || "Unable to generate sprint plan");
      }
    } finally {
      setLoading(false);
    }
  }, [sprintId, executeWithErrorHandling, handleError, isBlocked, getErrorMessage]);

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle className="flex items-center gap-2">
            <Sparkles className="h-5 w-5 text-primary" />
            Sprint Planning Assistant
          </CardTitle>
          <CardDescription>
            AI-assisted sprint planning and task assignment proposals
          </CardDescription>
        </div>
        <Button onClick={handlePlan} disabled={loading || isBlocked()}>
          {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {loading ? 'Planning... (up to 45s)' : 'AI Assist'}
        </Button>
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
                  onClick={handlePlan}
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
      {plan && (
        <CardContent>
          <Alert>
            <AlertDescription className="whitespace-pre-line">{plan}</AlertDescription>
          </Alert>
        </CardContent>
      )}
    </Card>
  );
}

