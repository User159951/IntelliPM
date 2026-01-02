import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertTriangle, CheckCircle2, TrendingUp } from 'lucide-react';
import { agentsApi } from '@/api/agents';

interface ProjectAnalysisPanelProps {
  projectId: number;
}

interface ProjectAnalysis {
  insights: string[];
  risks: Array<{
    title: string;
    severity: string;
    description: string;
  }>;
  recommendations: string[];
  overallHealth: string;
}

export function ProjectAnalysisPanel({ projectId }: ProjectAnalysisPanelProps) {
  const { data: response, isLoading } = useQuery({
    queryKey: ['project-analysis', projectId],
    queryFn: () => agentsApi.analyzeProject(projectId),
    enabled: projectId > 0,
  });

  if (isLoading) {
    return <Skeleton className="h-96" />;
  }

  if (!response || response.status === 'Error') {
    return (
      <Card>
        <CardContent className="pt-6">
          <p className="text-muted-foreground">
            {response?.errorMessage || 'Erreur lors de l\'analyse du projet'}
          </p>
        </CardContent>
      </Card>
    );
  }

  // Parse the analysis from AgentResponse content
  let analysis: ProjectAnalysis | null = null;
  try {
    const parsed = JSON.parse(response.content);
    analysis = parsed as ProjectAnalysis;
  } catch {
    // If content is not JSON, try to extract from metadata
    analysis = (response.metadata as unknown as ProjectAnalysis) || null;
  }

  if (!analysis) {
    return (
      <Card>
        <CardContent className="pt-6">
          <p className="text-muted-foreground">Impossible de parser l'analyse du projet</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {/* Overall Health */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CheckCircle2 className="h-5 w-5 text-green-500" />
            Santé Globale du Projet
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Badge variant={analysis.overallHealth === 'Good' ? 'default' : 'destructive'}>
            {analysis.overallHealth}
          </Badge>
        </CardContent>
      </Card>

      {/* Insights */}
      {analysis.insights && analysis.insights.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-blue-500" />
              Insights
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2">
              {analysis.insights.map((insight, idx) => (
                <li key={idx} className="flex items-start gap-2">
                  <span className="text-blue-500">- </span>
                  <span>{insight}</span>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}

      {/* Risks */}
      {analysis.risks && analysis.risks.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-red-500" />
              Risques Détectés
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {analysis.risks.map((risk, idx) => (
                <div key={idx} className="border-l-4 border-red-500 pl-3">
                  <div className="flex items-center gap-2">
                    <span className="font-semibold">{risk.title}</span>
                    <Badge variant="destructive">{risk.severity}</Badge>
                  </div>
                  <p className="text-sm text-muted-foreground mt-1">{risk.description}</p>
                </div>
              ))}
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
                  <span>{rec}</span>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

