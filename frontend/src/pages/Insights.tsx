import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import type { Project } from '@/types';
import { insightsApi } from '@/api/insights';
import { agentsApi } from '@/api/agents';
import { projectsApi } from '@/api/projects';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { showSuccess, showError } from "@/lib/sweetalert";
import { 
  Lightbulb, 
  AlertTriangle, 
  TrendingUp, 
  Shield, 
  Loader2,
  Sparkles,
  Copy,
} from 'lucide-react';

const insightTypeIcons: Record<string, React.ComponentType<{ className?: string }>> = {
  risk: AlertTriangle,
  opportunity: TrendingUp,
  delivery: Lightbulb,
  default: Sparkles,
};

const insightTypeColors: Record<string, string> = {
  risk: 'bg-red-500/10 text-red-500 border-red-500/20',
  opportunity: 'bg-green-500/10 text-green-500 border-green-500/20',
  delivery: 'bg-blue-500/10 text-blue-500 border-blue-500/20',
  default: 'bg-primary/10 text-primary border-primary/20',
};

export default function Insights() {
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [riskAnalysis, setRiskAnalysis] = useState<string | null>(null);

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const projectId = selectedProjectId || projectsData?.items?.[0]?.id?.toString();

  const { data: insightsData, isLoading } = useQuery({
    queryKey: ['insights', projectId, statusFilter],
    queryFn: () => 
      insightsApi.getByProject(
        parseInt(projectId!),
        statusFilter !== 'all' ? statusFilter : undefined
      ),
    enabled: !!projectId,
  });

  const analyzeRisksMutation = useMutation({
    mutationFn: () => agentsApi.analyzeRisks(parseInt(projectId!)),
    onSuccess: (data: { content?: string; metadata?: Record<string, unknown> }) => {
      const content = data.content || '';
      const metadata = data.metadata ? JSON.stringify(data.metadata, null, 2) : '';
      setRiskAnalysis(content || metadata || 'Risk analysis completed');
      showSuccess("Risk analysis complete");
    },
    onError: () => {
      showError('Failed to analyze risks');
    },
  });

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    showSuccess("Copied to clipboard");
  };

  const getIcon = (type: string) => {
    const Icon = insightTypeIcons[type.toLowerCase()] || insightTypeIcons.default;
    return Icon;
  };

  const getColor = (type: string) => {
    return insightTypeColors[type.toLowerCase()] || insightTypeColors.default;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Insights</h1>
          <p className="text-muted-foreground">AI-powered insights and recommendations</p>
        </div>
        <div className="flex items-center gap-4">
          <Select value={projectId} onValueChange={setSelectedProjectId}>
            <SelectTrigger className="w-[200px]">
              <SelectValue placeholder="Select project" />
            </SelectTrigger>
            <SelectContent>
              {projectsData?.items?.map((project: Project) => (
                <SelectItem key={project.id} value={project.id.toString()}>
                  {project.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="Filter status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="open">Open</SelectItem>
              <SelectItem value="resolved">Resolved</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Risk Analysis
            </CardTitle>
            <CardDescription>
              Get AI-powered risk analysis for your project
            </CardDescription>
          </div>
          <Button
            onClick={() => analyzeRisksMutation.mutate()}
            disabled={!projectId || analyzeRisksMutation.isPending}
          >
            {analyzeRisksMutation.isPending ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Sparkles className="mr-2 h-4 w-4" />
            )}
            Analyze Risks
          </Button>
        </CardHeader>
        {riskAnalysis && (
          <CardContent>
            <div className="relative rounded-lg bg-muted p-4">
              <Button
                variant="ghost"
                size="icon"
                className="absolute right-2 top-2"
                onClick={() => copyToClipboard(riskAnalysis)}
              >
                <Copy className="h-4 w-4" />
              </Button>
              <pre className="whitespace-pre-wrap text-sm">{riskAnalysis}</pre>
            </div>
          </CardContent>
        )}
      </Card>

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
      ) : !projectId ? (
        <Card className="py-16 text-center">
          <p className="text-muted-foreground">Select a project to view insights</p>
        </Card>
      ) : insightsData?.insights?.length === 0 ? (
        <Card className="flex flex-col items-center justify-center py-16">
          <Lightbulb className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium">No insights yet</h3>
          <p className="text-muted-foreground mb-4">
            Run AI agents to generate insights for your project
          </p>
        </Card>
      ) : (
        <div className="space-y-4">
          {insightsData?.insights?.map((insight) => {
            const Icon = getIcon(insight.type);
            return (
              <Card key={insight.id}>
                <CardHeader className="flex flex-row items-start gap-4">
                  <div className={`rounded-lg p-2 ${getColor(insight.type)}`}>
                    <Icon className="h-5 w-5" />
                  </div>
                  <div className="flex-1 space-y-1">
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-lg">{insight.title}</CardTitle>
                      <div className="flex items-center gap-2">
                        <Badge variant="outline">{insight.type}</Badge>
                        <Badge
                          variant={insight.status === 'open' ? 'default' : 'secondary'}
                        >
                          {insight.status}
                        </Badge>
                      </div>
                    </div>
                    <CardDescription>
                      {new Date(insight.createdAt).toLocaleDateString()}
                      {insight.agentType && ` • Generated by ${insight.agentType}`}
                    </CardDescription>
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">{insight.description}</p>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
