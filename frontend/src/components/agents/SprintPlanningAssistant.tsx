import { useState } from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, Sparkles } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from "@/lib/sweetalert";
interface Props {
  sprintId: number;
}

export function SprintPlanningAssistant({ sprintId }: Props) {
  const [loading, setLoading] = useState(false);
  const [plan, setPlan] = useState<string | null>(null);
  const handlePlan = async () => {
    setLoading(true);

    try {
      const response = await agentsApi.planSprint(sprintId);
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
      showError("Sprint Planning Failed", "Unable to generate sprint plan");
    } finally {
      setLoading(false);
    }
  };

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
        <Button onClick={handlePlan} disabled={loading}>
          {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {loading ? 'Planning...' : 'AI Assist'}
        </Button>
      </CardHeader>
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

