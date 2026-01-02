import { useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { GitBranch, AlertCircle, Loader2 } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from '@/lib/sweetalert';
import { useState } from 'react';
import type { AgentResponse } from '@/types';

interface DependencyAnalyzerPanelProps {
  projectId: number;
}

interface TaskDependency {
  taskId: number;
  dependsOn: number[];
  blockedBy: number[];
  criticalPath: boolean;
}

interface CircularDependency {
  tasks: number[];
  description: string;
}

interface DependencyAnalysis {
  dependencies: TaskDependency[];
  circularDependencies: CircularDependency[];
  recommendations: string[];
}

export function DependencyAnalyzerPanel({ projectId }: DependencyAnalyzerPanelProps) {
  const [analysis, setAnalysis] = useState<DependencyAnalysis | null>(null);

  const analyzeMutation = useMutation({
    mutationFn: () => agentsApi.analyzeDependencies(projectId),
    onSuccess: (data: AgentResponse) => {
      try {
        const parsed = JSON.parse(data.content);
        setAnalysis(parsed as DependencyAnalysis);
        showToast('Analyse des dépendances terminée', 'success');
      } catch {
        const parsed = (data.metadata as DependencyAnalysis) || null;
        if (parsed) {
          setAnalysis(parsed);
          showToast('Analyse des dépendances terminée', 'success');
        } else {
          showError('Erreur', 'Impossible de parser l\'analyse des dépendances');
        }
      }
    },
    onError: (error: Error) => {
      showError('Erreur lors de l\'analyse', error.message);
    },
  });

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold flex items-center gap-2">
          <GitBranch className="h-6 w-6 text-blue-500" />
          Analyse des Dépendances
        </h2>
        <Button
          onClick={() => analyzeMutation.mutate()}
          disabled={analyzeMutation.isPending}
        >
          {analyzeMutation.isPending ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Analyse...
            </>
          ) : (
            <>
              <GitBranch className="mr-2 h-4 w-4" />
              Analyser
            </>
          )}
        </Button>
      </div>

      {analyzeMutation.isPending && <Skeleton className="h-96" />}

      {analysis && (
        <>
          {/* Circular Dependencies */}
          {analysis.circularDependencies && analysis.circularDependencies.length > 0 && (
            <Card className="border-red-500">
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-red-600">
                  <AlertCircle className="h-5 w-5" />
                  Dépendances Circulaires Détectées
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {analysis.circularDependencies.map((circular, idx) => (
                    <div key={idx} className="bg-red-50 dark:bg-red-950 p-3 rounded-md">
                      <p className="text-sm font-medium">
                        Tâches : {circular.tasks.join(' → ')}
                      </p>
                      <p className="text-xs text-muted-foreground mt-1">
                        {circular.description}
                      </p>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Critical Path */}
          {analysis.dependencies && analysis.dependencies.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Chemin Critique</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {analysis.dependencies
                    .filter((dep) => dep.criticalPath)
                    .map((dep) => (
                      <div key={dep.taskId} className="flex items-center gap-2">
                        <Badge variant="destructive">Critique</Badge>
                        <span className="text-sm">Tâche #{dep.taskId}</span>
                      </div>
                    ))}
                  {analysis.dependencies.filter((dep) => dep.criticalPath).length === 0 && (
                    <p className="text-sm text-muted-foreground">Aucun chemin critique détecté</p>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Recommendations */}
          {analysis.recommendations && analysis.recommendations.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Recommandations</CardTitle>
              </CardHeader>
              <CardContent>
                <ul className="space-y-2">
                  {analysis.recommendations.map((rec, idx) => (
                    <li key={idx} className="flex items-start gap-2">
                      <span className="text-green-500">✓</span>
                      <span className="text-sm">{rec}</span>
                    </li>
                  ))}
                </ul>
              </CardContent>
            </Card>
          )}
        </>
      )}

      {!analyzeMutation.isPending && !analysis && (
        <Card>
          <CardContent className="pt-6 text-center text-muted-foreground">
            Cliquez sur "Analyser" pour analyser les dépendances de tâches du projet
          </CardContent>
        </Card>
      )}
    </div>
  );
}

