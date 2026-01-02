import { useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { Sparkles, Loader2 } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from '@/lib/sweetalert';
import { useState } from 'react';
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

  const planMutation = useMutation({
    mutationFn: () => agentsApi.planSprint(sprintId),
    onSuccess: (data: AgentResponse) => {
      try {
        const parsed = JSON.parse(data.content);
        setPlan(parsed as SprintPlan);
        showToast('Plan de sprint généré avec succès', 'success');
      } catch {
        const parsed = (data.metadata as unknown as SprintPlan) || null;
        if (parsed) {
          setPlan(parsed);
          showToast('Plan de sprint généré avec succès', 'success');
        } else {
          showError('Erreur', 'Impossible de parser le plan de sprint');
        }
      }
    },
    onError: (error: Error) => {
      showError('Erreur lors de la génération du plan', error.message);
    },
  });

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
          onClick={() => planMutation.mutate()}
          disabled={planMutation.isPending}
          variant="outline"
        >
          {planMutation.isPending ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Génération...
            </>
          ) : (
            <>
              <Sparkles className="mr-2 h-4 w-4" />
              Générer un plan
            </>
          )}
        </Button>
      </div>

      {planMutation.isPending && <Skeleton className="h-64" />}

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

