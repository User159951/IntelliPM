import { useMutation } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertTriangle, RefreshCw } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from '@/lib/sweetalert';
import { useState } from 'react';
import type { AgentResponse } from '@/types';

interface RiskDetectionDashboardProps {
  projectId: number;
}

interface Risk {
  id: string;
  title: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  description: string;
  mitigation: string;
  probability: number;
  impact: number;
}

interface RiskDetectionResult {
  risks: Risk[];
}

export function RiskDetectionDashboard({ projectId }: RiskDetectionDashboardProps) {
  const [risks, setRisks] = useState<Risk[]>([]);

  const detectMutation = useMutation({
    mutationFn: () => agentsApi.detectRisks(projectId),
    onSuccess: (data: AgentResponse) => {
      try {
        const parsed = JSON.parse(data.content);
        const result = parsed as RiskDetectionResult;
        setRisks(result.risks || []);
        showToast(`${result.risks?.length || 0} risque(s) détecté(s)`, 'success');
      } catch {
        // Try metadata
        const result = (data.metadata as RiskDetectionResult) || { risks: [] };
        setRisks(result.risks);
        showToast(`${result.risks.length} risque(s) détecté(s)`, 'success');
      }
    },
    onError: (error: Error) => {
      showError('Erreur lors de la détection des risques', error.message);
    },
  });

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical':
        return 'bg-red-500';
      case 'high':
        return 'bg-orange-500';
      case 'medium':
        return 'bg-yellow-500';
      case 'low':
        return 'bg-gray-500';
      default:
        return 'bg-gray-500';
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold flex items-center gap-2">
          <AlertTriangle className="h-6 w-6 text-red-500" />
          Détection des Risques
        </h2>
        <Button
          onClick={() => detectMutation.mutate()}
          disabled={detectMutation.isPending}
        >
          {detectMutation.isPending ? (
            <>
              <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
              Analyse en cours...
            </>
          ) : (
            <>
              <RefreshCw className="mr-2 h-4 w-4" />
              Analyser les risques
            </>
          )}
        </Button>
      </div>

      {detectMutation.isPending && <Skeleton className="h-96" />}

      {risks.length > 0 && (
        <div className="grid gap-4">
          {risks.map((risk) => (
            <Card key={risk.id}>
              <CardHeader>
                <CardTitle className="flex items-center justify-between">
                  <span>{risk.title}</span>
                  <Badge className={getSeverityColor(risk.severity)}>
                    {risk.severity}
                  </Badge>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <p className="text-sm text-muted-foreground">{risk.description}</p>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs font-medium">Probabilité</label>
                    <Progress value={risk.probability * 100} className="mt-1" />
                    <span className="text-xs text-muted-foreground">
                      {(risk.probability * 100).toFixed(0)}%
                    </span>
                  </div>
                  <div>
                    <label className="text-xs font-medium">Impact</label>
                    <Progress value={risk.impact * 100} className="mt-1" />
                    <span className="text-xs text-muted-foreground">
                      {(risk.impact * 100).toFixed(0)}%
                    </span>
                  </div>
                </div>

                <div className="bg-blue-50 dark:bg-blue-950 p-3 rounded-md">
                  <label className="text-xs font-semibold">Mitigation</label>
                  <p className="text-sm mt-1">{risk.mitigation}</p>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {!detectMutation.isPending && risks.length === 0 && (
        <Card>
          <CardContent className="pt-6 text-center text-muted-foreground">
            Cliquez sur "Analyser les risques" pour détecter les risques du projet
          </CardContent>
        </Card>
      )}
    </div>
  );
}

