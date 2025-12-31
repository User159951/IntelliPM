import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import type { Project } from '@/types';
import { agentsApi } from '@/api/agents';
import { projectsApi } from '@/api/projects';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
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
  runFn: (projectId: number) => Promise<unknown>;
}

export default function Agents() {
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [agentResults, setAgentResults] = useState<Record<string, { result: string; timestamp: Date } | null>>({});
  const [runningAgents, setRunningAgents] = useState<Set<string>>(new Set());

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const projectId = selectedProjectId || projectsData?.items?.[0]?.id?.toString();

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

    setRunningAgents((prev) => new Set(prev).add(agent.id));
    
    try {
      const result = await agent.runFn(parseInt(projectId));
      setAgentResults((prev) => ({
        ...prev,
        [agent.id]: {
          result: result.message || JSON.stringify(result.data, null, 2),
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
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">AI Agents</h1>
          <p className="text-muted-foreground">Run intelligent agents to analyze your project</p>
        </div>
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
                  
                  {result && (
                    <div className="mt-4 rounded-lg bg-muted p-3 max-h-32 overflow-auto">
                      <pre className="text-xs whitespace-pre-wrap">{result.result}</pre>
                    </div>
                  )}
                  
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
    </div>
  );
}
