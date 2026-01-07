import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import type { Project } from '@/types';
import { agentsApi } from '@/api/agents';
import { projectsApi } from '@/api/projects';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { showToast, showError } from "@/lib/sweetalert";
import { QuotaExceededAlert } from '@/components/ai-governance/QuotaExceededAlert';
import { AIDisabledAlert } from '@/components/ai-governance/AIDisabledAlert';
import { QuotaStatusWidget } from '@/components/ai-governance/QuotaStatusWidget';
import { QuotaAlertBanner } from '@/components/ai-governance/QuotaAlertBanner';
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';
import { useQuotaNotifications } from '@/hooks/useQuotaNotifications';
import { useRequestDeduplication } from '@/hooks/useRequestDeduplication';
import { AgentResultsDisplay } from '@/components/agents/results/AgentResultsDisplay';
import type { AgentResponse } from '@/types';
import type { AgentType } from '@/types/agents';
import { 
  Bot, 
  Loader2, 
  Play,
  Package,
  Truck,
  UserCog,
  TestTube,
  Briefcase,
  CheckCircle2,
  Clock,
} from 'lucide-react';

interface AgentConfig {
  id: string;
  name: string;
  description: string;
  icon: React.ComponentType<{ className?: string }>;
  runFn: (projectId: number) => Promise<AgentResponse>;
}

export default function Agents() {
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [agentResults, setAgentResults] = useState<Record<string, { result: AgentResponse; timestamp: Date } | null>>({});
  const [runningAgents, setRunningAgents] = useState<Set<string>>(new Set());
  
  // Handle AI errors with toast notifications
  useAIErrorHandler();
  
  // Handle quota notifications
  useQuotaNotifications();

  // Request deduplication to prevent double execution
  const { executeWithDeduplication, isRequestInFlight } = useRequestDeduplication();

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  // Initialize selectedProjectId with the first project when projectsData is loaded
  useEffect(() => {
    if (!selectedProjectId && projectsData?.items?.[0]?.id) {
      setSelectedProjectId(projectsData.items[0].id.toString());
    }
  }, [projectsData, selectedProjectId]);

  const projectId = selectedProjectId || projectsData?.items?.[0]?.id?.toString() || '';

  const agents: AgentConfig[] = [
    {
      id: 'product',
      name: 'Product Agent',
      description: 'Analyzes product requirements and suggests improvements for user stories and features.',
      icon: Package,
      runFn: (pid) => agentsApi.runProductAgent(pid),
    },
    {
      id: 'delivery',
      name: 'Delivery Agent',
      description: 'Monitors delivery metrics and provides recommendations to improve sprint outcomes.',
      icon: Truck,
      runFn: (pid) => agentsApi.runDeliveryAgent(pid),
    },
    {
      id: 'manager',
      name: 'Manager Agent',
      description: 'Provides management insights and helps with resource allocation and planning.',
      icon: UserCog,
      runFn: (pid) => agentsApi.runManagerAgent(pid),
    },
    {
      id: 'qa',
      name: 'QA Agent',
      description: 'Analyzes quality metrics and suggests testing strategies and improvements.',
      icon: TestTube,
      runFn: (pid) => agentsApi.runQAAgent(pid),
    },
    {
      id: 'business',
      name: 'Business Agent',
      description: 'Evaluates business impact and provides ROI analysis for features and initiatives.',
      icon: Briefcase,
      runFn: (pid) => agentsApi.runBusinessAgent(pid),
    },
  ];

  const runAgent = async (agent: AgentConfig) => {
    if (!projectId) return;

    // Check if request is already in flight
    const requestKey = `${agent.id}-${projectId}`;
    if (isRequestInFlight(requestKey)) {
      showToast('Request already in progress', 'info');
      return;
    }

    setRunningAgents((prev) => new Set(prev).add(agent.id));
    
    const result = await executeWithDeduplication(requestKey, async () => {
      return await agent.runFn(parseInt(projectId));
    });

    // If request was deduplicated (result is null), don't update state
    if (result === null) {
      setRunningAgents((prev) => {
        const next = new Set(prev);
        next.delete(agent.id);
        return next;
      });
      return;
    }

    try {
      setAgentResults((prev) => ({
        ...prev,
        [agent.id]: {
          result: result,
          timestamp: new Date(),
        },
      }));
      showToast(`${agent.name} completed successfully`, 'success');
    } catch (error) {
      showError(`${agent.name} failed`, error instanceof Error ? error.message : 'Please try again');
    } finally {
      setRunningAgents((prev) => {
        const next = new Set(prev);
        next.delete(agent.id);
        return next;
      });
    }
  };

  return (
    <div className="space-y-6">
      <QuotaExceededAlert />
      <AIDisabledAlert />
      <QuotaAlertBanner />
      <QuotaStatusWidget />
      
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">AI Agents</h1>
          <p className="text-muted-foreground">Run intelligent agents to analyze your project</p>
        </div>
        <Select value={selectedProjectId || ''} onValueChange={setSelectedProjectId}>
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
      </div>

      {!projectId ? (
        <Card className="py-16 text-center">
          <Bot className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <p className="text-muted-foreground">Select a project to run AI agents</p>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {agents.map((agent) => {
            const isRunning = runningAgents.has(agent.id);
            const result = agentResults[agent.id];
            const Icon = agent.icon;

            return (
              <Card key={agent.id} className="flex flex-col">
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-3">
                      <div className="rounded-lg bg-primary/10 p-2">
                        <Icon className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <CardTitle className="text-lg">{agent.name}</CardTitle>
                        {result && (
                          <div className="flex items-center gap-1 text-xs text-muted-foreground mt-1">
                            <Clock className="h-3 w-3" />
                            Last run: {result.timestamp.toLocaleTimeString()}
                          </div>
                        )}
                      </div>
                    </div>
                    {result && (
                      <Badge variant="outline" className="bg-green-500/10 text-green-500 border-green-500/20">
                        <CheckCircle2 className="h-3 w-3 mr-1" />
                        Completed
                      </Badge>
                    )}
                  </div>
                </CardHeader>
                <CardContent className="flex-1 flex flex-col">
                  <CardDescription className="flex-1">{agent.description}</CardDescription>
                  
                  <Button
                    className="mt-4 w-full"
                    onClick={() => runAgent(agent)}
                    disabled={isRunning}
                  >
                    {isRunning ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Running...
                      </>
                    ) : (
                      <>
                        <Play className="mr-2 h-4 w-4" />
                        Run Agent
                      </>
                    )}
                  </Button>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* Results Section */}
      {Object.entries(agentResults).map(([agentId, agentResult]) => {
        if (!agentResult) return null;
        
        const agent = agents.find(a => a.id === agentId);
        if (!agent) return null;

        return (
          <Card key={`result-${agentId}`} className="mt-6">
            <CardHeader>
              <CardTitle>{agent.name} - Results</CardTitle>
            </CardHeader>
            <CardContent>
              <AgentResultsDisplay
                agentType={agentId as AgentType}
                result={agentResult.result}
                isLoading={runningAgents.has(agentId)}
              />
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
