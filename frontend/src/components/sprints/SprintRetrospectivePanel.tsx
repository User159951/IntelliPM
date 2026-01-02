import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { MessageSquare, ThumbsUp, AlertTriangle, CheckCircle2, Loader2 } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast } from '@/lib/sweetalert';
import type { SprintRetrospective } from '@/types/agents';

interface SprintRetrospectivePanelProps {
  sprintId: number;
  sprintName?: string;
}

export function SprintRetrospectivePanel({ sprintId }: SprintRetrospectivePanelProps) {
  const [retrospective, setRetrospective] = useState<SprintRetrospective | null>(null);

  const generateMutation = useMutation({
    mutationFn: () => agentsApi.generateRetrospective(sprintId),
    onSuccess: (data) => {
      setRetrospective(data);
      showToast('Rétrospective générée avec succès', 'success');
    },
    onError: () => {
      showToast('Erreur lors de la génération de la rétrospective', 'error');
    },
  });

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
          onClick={() => generateMutation.mutate()}
          disabled={generateMutation.isPending}
        >
          {generateMutation.isPending ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Génération...
            </>
          ) : (
            <>
              <MessageSquare className="mr-2 h-4 w-4" />
              Générer la Rétrospective
            </>
          )}
        </Button>
      </div>

      {generateMutation.isPending && <Skeleton className="h-96" />}

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

