import { useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Sparkles, Loader2, AlertCircle, RefreshCw } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from '@/lib/sweetalert';
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';
import { useRequestDeduplication } from '@/hooks/useRequestDeduplication';
import { VirtualizedList } from '@/components/agents/VirtualizedList';
import { useState, useCallback, useEffect } from 'react';
import type { AgentResponse } from '@/types';

interface SprintPlanningAIProps {
  sprintId: number;
  onTasksSelected: (taskIds: number[]) => void;
}

interface SuggestedTask {
  taskId: number;
  title: string;
  priority: string;
  estimatedPoints: number;
  rationale: string;
}

interface SprintPlan {
  suggestedTasks: SuggestedTask[];
  totalPoints: number;
  teamCapacity: number;
  utilizationRate: number;
  warnings: string[];
}

export function SprintPlanningAI({ sprintId, onTasksSelected }: SprintPlanningAIProps) {
  const [plan, setPlan] = useState<SprintPlan | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [canRetry, setCanRetry] = useState(false);
  const [retryAfter, setRetryAfter] = useState<number | undefined>(undefined);

  const { handleError, executeWithErrorHandling, isBlocked, getErrorMessage } = useAIErrorHandler({
    showToast: false, // We'll handle toasts manually for better UX
  });

  const { isRequestInFlight } = useRequestDeduplication();

  // Check if blocked on mount
  useEffect(() => {
    if (isBlocked()) {
      const errorMsg = getErrorMessage();
      if (errorMsg) {
        setError(errorMsg);
      }
    }
  }, [isBlocked, getErrorMessage]);

  const planMutation = useMutation({
    mutationFn: async () => {
      return executeWithErrorHandling(
        () => agentsApi.planSprint(sprintId),
        45000 // Estimated 45 seconds for sprint planning
      );
    },
    onSuccess: (data: AgentResponse) => {
      try {
        const parsed = JSON.parse(data.content);
        setPlan(parsed as SprintPlan);
        setError(null);
        setCanRetry(false);
        showToast('Plan de sprint généré avec succès', 'success');
      } catch {
        const parsed = (data.metadata as unknown as SprintPlan) || null;
        if (parsed) {
          setPlan(parsed);
          setError(null);
          setCanRetry(false);
          showToast('Plan de sprint généré avec succès', 'success');
        } else {
          setError('Impossible de parser le plan de sprint');
          setCanRetry(true);
          showError('Erreur', 'Impossible de parser le plan de sprint');
        }
      }
    },
    onError: (error: Error) => {
      const errorResult = handleError(error);
      setError(error.message || 'Erreur lors de la génération du plan');
      setCanRetry(errorResult.canRetry);
      setRetryAfter(errorResult.retryAfter);
      
      if (!errorResult.isQuotaExceeded && !errorResult.isAIDisabled) {
        showError('Erreur lors de la génération du plan', error.message);
      }
    },
  });

  const handlePlanSprint = useCallback(() => {
    if (isBlocked()) {
      const errorMsg = getErrorMessage();
      if (errorMsg) {
        setError(errorMsg);
      }
      return;
    }

    const requestKey = `sprint-plan-${sprintId}`;
    if (isRequestInFlight(requestKey) || planMutation.isPending) {
      showToast('Génération déjà en cours', 'info');
      return;
    }

    setError(null);
    setCanRetry(false);
    setRetryAfter(undefined);
    planMutation.mutate();
  }, [isBlocked, getErrorMessage, planMutation, sprintId, isRequestInFlight]);

  const handleApplyPlan = () => {
    if (plan) {
      const taskIds = plan.suggestedTasks.map((t) => t.taskId);
      onTasksSelected(taskIds);
      showToast(`${taskIds.length} tâches ajoutées au sprint`, 'success');
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold flex items-center gap-2">
          <Sparkles className="h-5 w-5 text-purple-500" />
          Planification Intelligente
        </h3>
        <Button
          onClick={handlePlanSprint}
          disabled={planMutation.isPending || isBlocked()}
          variant="outline"
        >
          {planMutation.isPending ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Génération... (peut prendre jusqu&apos;à 45 secondes)
            </>
          ) : (
            <>
              <Sparkles className="mr-2 h-4 w-4" />
              Générer un plan
            </>
          )}
        </Button>
      </div>

      {/* Error State */}
      {error && !planMutation.isPending && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription className="space-y-2">
            <p>{error}</p>
            {canRetry && (
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handlePlanSprint}
                className="mt-2"
              >
                <RefreshCw className="mr-2 h-3 w-3" />
                Réessayer{retryAfter ? ` (dans ${retryAfter}s)` : ''}
              </Button>
            )}
          </AlertDescription>
        </Alert>
      )}

      {planMutation.isPending && (
        <div className="space-y-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" />
            <span>Génération du plan de sprint en cours... Cela peut prendre jusqu&apos;à 45 secondes.</span>
          </div>
          <Skeleton className="h-64" />
        </div>
      )}

      {plan && (
        <>
          {/* Capacity Overview */}
          <Card>
            <CardHeader>
              <CardTitle>Vue d'ensemble de la capacité</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex justify-between items-center">
                <span className="text-sm">Points suggérés</span>
                <Badge>{plan.totalPoints} pts</Badge>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm">Capacité de l'équipe</span>
                <Badge variant="outline">{plan.teamCapacity} pts</Badge>
              </div>
              <Progress value={plan.utilizationRate * 100} />
              <span className="text-xs text-muted-foreground">
                Taux d'utilisation : {(plan.utilizationRate * 100).toFixed(0)}%
              </span>
            </CardContent>
          </Card>

          {/* Suggested Tasks */}
          <Card>
            <CardHeader>
              <CardTitle>Tâches suggérées ({plan.suggestedTasks.length})</CardTitle>
            </CardHeader>
            <CardContent>
              {plan.suggestedTasks.length > 10 ? (
                <VirtualizedList
                  items={plan.suggestedTasks}
                  renderItem={(task) => (
                    <div className="border-l-4 border-purple-500 pl-3 mb-3">
                      <div className="flex items-center justify-between">
                        <span className="font-medium">{task.title}</span>
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{task.estimatedPoints} pts</Badge>
                          <Badge>{task.priority}</Badge>
                        </div>
                      </div>
                      <p className="text-xs text-muted-foreground mt-1">{task.rationale}</p>
                    </div>
                  )}
                  itemHeight={80}
                  maxHeight={400}
                />
              ) : (
                <div className="space-y-3">
                  {plan.suggestedTasks.map((task) => (
                    <div key={task.taskId} className="border-l-4 border-purple-500 pl-3">
                      <div className="flex items-center justify-between">
                        <span className="font-medium">{task.title}</span>
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{task.estimatedPoints} pts</Badge>
                          <Badge>{task.priority}</Badge>
                        </div>
                      </div>
                      <p className="text-xs text-muted-foreground mt-1">{task.rationale}</p>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Warnings */}
          {plan.warnings && plan.warnings.length > 0 && (
            <Card className="border-yellow-500">
              <CardHeader>
                <CardTitle className="text-yellow-600">Avertissements</CardTitle>
              </CardHeader>
              <CardContent>
                <ul className="space-y-1">
                  {plan.warnings.map((warning: string, idx: number) => (
                    <li key={idx} className="text-sm text-yellow-600">⚠️ {warning}</li>
                  ))}
                </ul>
              </CardContent>
            </Card>
          )}

          {/* Apply Button */}
          <Button onClick={handleApplyPlan} className="w-full">
            Appliquer ce plan au sprint
          </Button>
        </>
      )}

      {!planMutation.isPending && !plan && (
        <Card>
          <CardContent className="pt-6 text-center text-muted-foreground">
            Cliquez sur "Générer un plan" pour créer un plan de sprint intelligent
          </CardContent>
        </Card>
      )}
    </div>
  );
}

