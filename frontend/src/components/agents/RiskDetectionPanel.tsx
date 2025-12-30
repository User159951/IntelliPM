import { useState } from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, AlertTriangle } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
interface Props {
  projectId: number;
}

export function RiskDetectionPanel({ projectId }: Props) {
  const [loading, setLoading] = useState(false);
  const [analysis, setAnalysis] = useState<string | null>(null);
  const handleDetect = async () => {
    setLoading(true);

    try {
      const response = await agentsApi.detectRisks(projectId);
      const content = (response as any).content ?? (response as any).Content;
      const time = (response as any).executionTimeMs ?? (response as any).ExecutionTimeMs;

      setAnalysis(content || 'No risks detected');
      showToast(time ? `Risk Detection Complete - Completed in ${time}ms` : 'Risk Detection Complete', 'success');
    } catch (error) {
      showError("Risk Detection Failed", "Unable to detect risks for this project");
    } finally {
      setLoading(false);
    }
  };

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
          <Button onClick={handleDetect} disabled={loading}>
            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {loading ? 'Analyzing...' : 'Detect Risks'}
          </Button>
        </div>
      </CardHeader>

      {analysis && (
        <CardContent>
          <Alert>
            <AlertDescription className="whitespace-pre-line">{analysis}</AlertDescription>
          </Alert>
        </CardContent>
      )}
    </Card>
  );
}

