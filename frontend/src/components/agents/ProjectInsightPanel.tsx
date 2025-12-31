import { useState } from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, TrendingUp } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
interface Props {
  projectId: number;
}

export function ProjectInsightPanel({ projectId }: Props) {
  const [loading, setLoading] = useState(false);
  const [insight, setInsight] = useState<string | null>(null);
  const handleAnalyze = async () => {
    setLoading(true);

    try {
      const response = await agentsApi.analyzeProject(projectId);
      // Backend returns Semantic Kernel AgentResponse (Content, ExecutionTimeMs, etc.)
      const apiResponse = response as { content?: string; Content?: string; executionTimeMs?: number; ExecutionTimeMs?: number };
      const content = apiResponse.content ?? apiResponse.Content;
      const time = apiResponse.executionTimeMs ?? apiResponse.ExecutionTimeMs;

      setInsight(content || 'No summary generated');
      showToast(time ? `Analysis Complete - Completed in ${time}ms` : 'Analysis Complete', 'success');
    } catch (error) {
      showError("Analysis Failed", "Unable to analyze project status");
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
              <TrendingUp className="h-5 w-5" />
              Project Insights
            </CardTitle>
            <CardDescription>AI-powered project status analysis</CardDescription>
          </div>
          <Button onClick={handleAnalyze} disabled={loading}>
            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {loading ? 'Analyzing...' : 'Analyze Status'}
          </Button>
        </div>
      </CardHeader>

      {insight && (
        <CardContent>
          <Alert>
            <AlertDescription className="whitespace-pre-line">{insight}</AlertDescription>
          </Alert>
        </CardContent>
      )}
    </Card>
  );
}

