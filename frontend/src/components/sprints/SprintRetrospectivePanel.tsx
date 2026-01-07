import { useState, useCallback, useEffect } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { MessageSquare, ThumbsUp, AlertTriangle, CheckCircle2, Loader2, RefreshCw, AlertCircle } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from '@/lib/sweetalert';
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';
import type { SprintRetrospective } from '@/types/agents';

interface SprintRetrospectivePanelProps {
  sprintId: number;
  sprintName?: string;
}

export function SprintRetrospectivePanel({ sprintId }: SprintRetrospectivePanelProps) {
  const [retrospective, setRetrospective] = useState<SprintRetrospective | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [canRetry, setCanRetry] = useState(false);
  const [retryAfter, setRetryAfter] = useState<number | undefined>(undefined);

  const { handleError, executeWithErrorHandling, isBlocked, getErrorMessage } = useAIErrorHandler({
    showToast: false, // We'll handle toasts manually for better UX
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

  const generateMutation = useMutation({
    mutationFn: async () => {
      return executeWithErrorHandling(
        () => agentsApi.generateRetrospective(sprintId),
        50000 // Estimated 50 seconds for retrospective generation
      );
    },
    onSuccess: (data) => {
      setRetrospective(data);
      setError(null);
      setCanRetry(false);
      showToast('Rétrospective générée avec succès', 'success');
    },
    onError: (error: Error) => {
      const errorResult = handleError(error);
      setError(error.message || 'Erreur lors de la génération de la rétrospective');
      setCanRetry(errorResult.canRetry);
      setRetryAfter(errorResult.retryAfter);
      
      if (!errorResult.isQuotaExceeded && !errorResult.isAIDisabled) {
        showError('Erreur lors de la génération de la rétrospective', error.message);
      }
    },
  });

  const handleGenerate = useCallback(() => {
    if (isBlocked()) {
      const errorMsg = getErrorMessage();
      if (errorMsg) {
        setError(errorMsg);
      }
      return;
    }

    setError(null);
    setCanRetry(false);
    setRetryAfter(undefined);
    generateMutation.mutate();
  }, [isBlocked, getErrorMessage, generateMutation]);

  const getSentimentColor = (sentiment: string) => {
    switch (sentiment) {
      case 'positive': return 'bg-green-500';
      case 'neutral': return 'bg-yellow-500';
      case 'negative': return 'bg-red-500';
      default: return 'bg-gray-500';
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return 'bg-red-500';
      case 'medium': return 'bg-orange-500';
      case 'low': return 'bg-blue-500';
      default: return 'bg-gray-500';
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold flex items-center gap-2">
          <MessageSquare className="h-6 w-6 text-blue-500" />
          Rétrospective de Sprint
        </h2>
        <Button
          onClick={handleGenerate}
          disabled={generateMutation.isPending || isBlocked()}
        >
          {generateMutation.isPending ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Génération... (peut prendre jusqu&apos;à 50 secondes)
            </>
          ) : (
            <>
              <MessageSquare className="mr-2 h-4 w-4" />
              Générer la Rétrospective
            </>
          )}
        </Button>
      </div>

      {/* Error State */}
      {error && !generateMutation.isPending && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription className="space-y-2">
            <p>{error}</p>
            {canRetry && (
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handleGenerate}
                className="mt-2"
              >
                <RefreshCw className="mr-2 h-3 w-3" />
                Réessayer{retryAfter ? ` (dans ${retryAfter}s)` : ''}
              </Button>
            )}
          </AlertDescription>
        </Alert>
      )}

      {generateMutation.isPending && (
        <div className="space-y-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" />
            <span>Génération de la rétrospective en cours... Cela peut prendre jusqu&apos;à 50 secondes.</span>
          </div>
          <Skeleton className="h-96" />
        </div>
      )}

      {retrospective && (
        <div className="space-y-4">
          {/* Sprint Info */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>{retrospective.sprintName}</CardTitle>
                <Badge className={getSentimentColor(retrospective.overallSentiment)}>
                  {retrospective.overallSentiment}
                </Badge>
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">{retrospective.sprintGoal}</p>
            </CardContent>
          </Card>

          {/* Key Metrics */}
          <Card>
            <CardHeader>
              <CardTitle>Métriques Clés</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div>
                <div className="flex justify-between items-center mb-1">
                  <span className="text-sm">Tâches Complétées</span>
                  <span className="text-sm font-medium">
                    {retrospective.keyMetrics.completedTasks} / {retrospective.keyMetrics.totalTasks}
                  </span>
                </div>
                <Progress 
                  value={(retrospective.keyMetrics.completedTasks / retrospective.keyMetrics.totalTasks) * 100} 
                />
              </div>
              <div>
                <div className="flex justify-between items-center mb-1">
                  <span className="text-sm">Vélocité</span>
                  <span className="text-sm font-medium">
                    {retrospective.keyMetrics.velocityAchieved} / {retrospective.keyMetrics.velocityPlanned} pts
                  </span>
                </div>
                <Progress 
                  value={(retrospective.keyMetrics.velocityAchieved / retrospective.keyMetrics.velocityPlanned) * 100} 
                />
              </div>
            </CardContent>
          </Card>

          {/* What Went Well */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <ThumbsUp className="h-5 w-5 text-green-500" />
                Ce qui a Bien Fonctionné
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="space-y-2">
                {retrospective.whatWentWell.map((item, idx) => (
                  <li key={idx} className="flex items-start gap-2">
                    <CheckCircle2 className="h-5 w-5 text-green-500 mt-0.5" />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>

          {/* What Can Be Improved */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <AlertTriangle className="h-5 w-5 text-orange-500" />
                Ce qui Peut Être Amélioré
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="space-y-2">
                {retrospective.whatCanBeImproved.map((item, idx) => (
                  <li key={idx} className="flex items-start gap-2">
                    <AlertTriangle className="h-5 w-5 text-orange-500 mt-0.5" />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>

          {/* Action Items */}
          <Card>
            <CardHeader>
              <CardTitle>Actions à Prendre</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {retrospective.actionItems.map((action, idx) => (
                  <div key={idx} className="border-l-4 border-blue-500 pl-3">
                    <div className="flex items-center justify-between">
                      <span className="font-medium">{action.description}</span>
                      <Badge className={getPriorityColor(action.priority)}>
                        {action.priority}
                      </Badge>
                    </div>
                    {action.assignee && (
                      <p className="text-xs text-muted-foreground mt-1">
                        Assigné à : {action.assignee}
                      </p>
                    )}
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Recommendations */}
          <Card>
            <CardHeader>
              <CardTitle>Recommandations pour le Prochain Sprint</CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="space-y-2">
                {retrospective.recommendations.map((rec, idx) => (
                  <li key={idx} className="flex items-start gap-2">
                    <span className="text-blue-500">→</span>
                    <span>{rec}</span>
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}

