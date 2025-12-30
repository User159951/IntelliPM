import { useState, useEffect } from 'react';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Loader2, Sparkles, Plus, Trash2 } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { tasksApi } from '@/api/tasks';
import type { AgentResponse, ImprovedTaskData, TaskPriority, CreateTaskRequest } from '@/types';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
interface AITaskImproverDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  projectId: number;
  onTaskCreated?: () => void;
}

type Step = 'input' | 'loading' | 'result';

export function AITaskImproverDialog({
  open,
  onOpenChange,
  projectId,
  onTaskCreated,
}: AITaskImproverDialogProps) {
  const [step, setStep] = useState<Step>('input');
  const [taskDescription, setTaskDescription] = useState('');
  const [agentResponse, setAgentResponse] = useState<AgentResponse | null>(null);
  const [improvedTask, setImprovedTask] = useState<ImprovedTaskData | null>(null);
  const [isCreating, setIsCreating] = useState(false);

  // Reset when dialog closes
  useEffect(() => {
    if (!open) {
      setStep('input');
      setTaskDescription('');
      setAgentResponse(null);
      setImprovedTask(null);
    }
  }, [open]);

  // Parse AI response content to extract structured data
  const parseAIResponse = (content: string): ImprovedTaskData => {
    // Default values
    const defaultTask: ImprovedTaskData = {
      title: '',
      description: content,
      acceptanceCriteria: [],
      suggestedPriority: 'Medium',
      suggestedStoryPoints: 5,
      type: 'Task',
    };

    try {
      // Try to extract title (usually first line or after "Title:")
      const titleMatch = content.match(/(?:Title|Task):\s*(.+?)(?:\n|$)/i) || 
                        content.match(/^#\s*(.+?)$/m) ||
                        content.match(/^(.+?)$/m);
      if (titleMatch) {
        defaultTask.title = titleMatch[1].trim();
      }

      // Extract description (everything after title, before Acceptance Criteria)
      const descMatch = content.match(/(?:Description|Details):\s*([\s\S]+?)(?=Acceptance|Definition|Priority|Story|$)/i);
      if (descMatch) {
        defaultTask.description = descMatch[1].trim();
      }

      // Extract acceptance criteria (bullet points)
      const criteriaMatches = content.match(/(?:Acceptance Criteria|Criteria):\s*([\s\S]+?)(?=Definition|Priority|Story|$)/i);
      if (criteriaMatches) {
        const criteriaText = criteriaMatches[1];
        defaultTask.acceptanceCriteria = criteriaText
          .split(/\n/)
          .map(line => line.replace(/^[-*•]\s*/, '').replace(/^\d+\.\s*/, '').trim())
          .filter(line => line.length > 0);
      }

      // Extract priority
      const priorityMatch = content.match(/(?:Priority|Suggested Priority):\s*(Low|Medium|High|Critical)/i);
      if (priorityMatch) {
        defaultTask.suggestedPriority = priorityMatch[1] as TaskPriority;
      }

      // Extract story points
      const pointsMatch = content.match(/(?:Story Points|Points|SP):\s*(\d+)/i);
      if (pointsMatch) {
        defaultTask.suggestedStoryPoints = parseInt(pointsMatch[1], 10);
      }

      // Extract type
      const typeMatch = content.match(/(?:Type|Category):\s*(Bug|Feature|Task)/i);
      if (typeMatch) {
        defaultTask.type = typeMatch[1] as 'Bug' | 'Feature' | 'Task';
      }
    } catch (error) {
      console.error('Error parsing AI response:', error);
    }

    return defaultTask;
  };

  const handleImprove = async () => {
    if (taskDescription.trim().length < 10) {
      showError("Description too short", "Please provide at least 10 characters");
      return;
    }

    setStep('loading');

    try {
      const response = await agentsApi.improveTask({
        description: taskDescription,
        projectId,
      });

      // Handle both camelCase and PascalCase from backend
      // The response should already be AgentResponse, but handle both cases
      const normalizedResponse: AgentResponse = {
        content: (response as AgentResponse & { Content?: string }).Content || response.content || '',
        status: ((response as AgentResponse & { Status?: string }).Status || response.status || 'Success') as 'Success' | 'Error' | 'ProposalReady',
        requiresApproval: (response as AgentResponse & { RequiresApproval?: boolean }).RequiresApproval ?? response.requiresApproval ?? false,
        executionCostUsd: (response as AgentResponse & { ExecutionCostUsd?: number }).ExecutionCostUsd ?? response.executionCostUsd ?? 0,
        executionTimeMs: (response as AgentResponse & { ExecutionTimeMs?: number }).ExecutionTimeMs ?? response.executionTimeMs ?? 0,
        toolsCalled: (response as AgentResponse & { ToolsCalled?: string[] }).ToolsCalled || response.toolsCalled || [],
        timestamp: (response as AgentResponse & { Timestamp?: string }).Timestamp || response.timestamp || new Date().toISOString(),
        errorMessage: (response as AgentResponse & { ErrorMessage?: string }).ErrorMessage || response.errorMessage,
        metadata: (response as AgentResponse & { Metadata?: Record<string, unknown> }).Metadata || response.metadata,
      };

      setAgentResponse(normalizedResponse);
      const parsed = parseAIResponse(normalizedResponse.content);
      setImprovedTask(parsed);
      setStep('result');
    } catch (error) {
      showError('AI improvement failed');
      setStep('input');
    }
  };

  const handleApproveAndCreate = async () => {
    if (!improvedTask || !improvedTask.title.trim()) {
      showError("Invalid task data", "Please ensure the task has a title");
      return;
    }

    setIsCreating(true);

    try {
      const taskData: CreateTaskRequest = {
        title: improvedTask.title,
        description: improvedTask.description,
        projectId,
        priority: improvedTask.suggestedPriority,
        storyPoints: improvedTask.suggestedStoryPoints,
      };

      await tasksApi.create(taskData);
      
      showSuccess("Task created", "Task created successfully with AI assistance ✨");

      onTaskCreated?.();
      onOpenChange(false);
    } catch (error) {
      showError('Failed to create task');
    } finally {
      setIsCreating(false);
    }
  };

  const handleAddCriterion = () => {
    if (!improvedTask) return;
    setImprovedTask({
      ...improvedTask,
      acceptanceCriteria: [...improvedTask.acceptanceCriteria, ''],
    });
  };

  const handleRemoveCriterion = (index: number) => {
    if (!improvedTask) return;
    setImprovedTask({
      ...improvedTask,
      acceptanceCriteria: improvedTask.acceptanceCriteria.filter((_, i) => i !== index),
    });
  };

  const handleUpdateCriterion = (index: number, value: string) => {
    if (!improvedTask) return;
    const updated = [...improvedTask.acceptanceCriteria];
    updated[index] = value;
    setImprovedTask({
      ...improvedTask,
      acceptanceCriteria: updated,
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        {step === 'input' && (
          <>
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <Sparkles className="h-5 w-5 text-cyan-500" />
                Create Task with AI Assistant
              </DialogTitle>
              <DialogDescription>
                Describe your task in natural language. AI will help structure it with clear requirements and acceptance criteria.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="taskDescription">Task Description</Label>
                <Textarea
                  id="taskDescription"
                  placeholder="e.g., fix payment bug users complaining checkout broken on mobile iphone"
                  value={taskDescription}
                  onChange={(e) => setTaskDescription(e.target.value)}
                  rows={6}
                  className="resize-none"
                />
                <p className="text-xs text-muted-foreground">
                  Minimum 10 characters. Be as descriptive as possible.
                </p>
              </div>
              <div className="rounded-lg bg-muted/50 p-4 space-y-2">
                <p className="text-sm font-medium">Examples:</p>
                <ul className="text-sm text-muted-foreground space-y-1 list-disc list-inside">
                  <li>"add login with google and facebook"</li>
                  <li>"dashboard showing user stats and graphs"</li>
                  <li>"fix crash when uploading large files"</li>
                </ul>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button onClick={handleImprove} disabled={taskDescription.trim().length < 10}>
                <Sparkles className="mr-2 h-4 w-4" />
                Let AI Improve This
              </Button>
            </DialogFooter>
          </>
        )}

        {step === 'loading' && (
          <>
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <Sparkles className="h-5 w-5 text-cyan-500 animate-pulse" />
                AI is analyzing your task...
              </DialogTitle>
              <DialogDescription>
                This usually takes 2-5 seconds
              </DialogDescription>
            </DialogHeader>
            <div className="flex flex-col items-center justify-center py-12 space-y-4">
              <Loader2 className="h-12 w-12 animate-spin text-cyan-500" />
              <div className="space-y-2 text-center">
                <p className="text-sm font-medium">Analyzing task description...</p>
                <p className="text-xs text-muted-foreground">
                  Structuring requirements and generating acceptance criteria
                </p>
              </div>
            </div>
          </>
        )}

        {step === 'result' && improvedTask && agentResponse && (
          <>
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <Sparkles className="h-5 w-5 text-cyan-500" />
                AI-Enhanced Task Proposal
              </DialogTitle>
              <DialogDescription>
                Review and edit the AI-generated task before creating it.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-6 py-4">
              {/* Improved Task Section */}
              <div className="space-y-4 rounded-lg border p-4">
                <div className="flex items-center gap-2 mb-2">
                  <Badge variant="outline" className="bg-cyan-500/10 text-cyan-700 dark:text-cyan-400">
                    ✨ IMPROVED TASK
                  </Badge>
                </div>

                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="ai-title">Title</Label>
                    <Input
                      id="ai-title"
                      value={improvedTask.title}
                      onChange={(e) =>
                        setImprovedTask({ ...improvedTask, title: e.target.value })
                      }
                      placeholder="Task title"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="ai-description">Description</Label>
                    <Textarea
                      id="ai-description"
                      value={improvedTask.description}
                      onChange={(e) =>
                        setImprovedTask({ ...improvedTask, description: e.target.value })
                      }
                      rows={4}
                      placeholder="Task description"
                    />
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <Label>Acceptance Criteria</Label>
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={handleAddCriterion}
                      >
                        <Plus className="h-4 w-4 mr-1" />
                        Add
                      </Button>
                    </div>
                    <div className="space-y-2">
                      {improvedTask.acceptanceCriteria.map((criterion, index) => (
                        <div key={index} className="flex items-center gap-2">
                          <Input
                            value={criterion}
                            onChange={(e) => handleUpdateCriterion(index, e.target.value)}
                            placeholder={`Criterion ${index + 1}`}
                          />
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            onClick={() => handleRemoveCriterion(index)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      ))}
                      {improvedTask.acceptanceCriteria.length === 0 && (
                        <p className="text-sm text-muted-foreground italic">
                          No acceptance criteria. Click "Add" to add some.
                        </p>
                      )}
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="ai-priority">Priority</Label>
                      <Select
                        value={improvedTask.suggestedPriority}
                        onValueChange={(value: TaskPriority) =>
                          setImprovedTask({ ...improvedTask, suggestedPriority: value })
                        }
                      >
                        <SelectTrigger id="ai-priority">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Low">Low</SelectItem>
                          <SelectItem value="Medium">Medium</SelectItem>
                          <SelectItem value="High">High</SelectItem>
                          <SelectItem value="Critical">Critical</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="ai-story-points">Story Points</Label>
                      <Input
                        id="ai-story-points"
                        type="number"
                        min={0}
                        max={100}
                        value={improvedTask.suggestedStoryPoints}
                        onChange={(e) =>
                          setImprovedTask({
                            ...improvedTask,
                            suggestedStoryPoints: parseInt(e.target.value) || 0,
                          })
                        }
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="ai-type">Type</Label>
                    <Select
                      value={improvedTask.type}
                      onValueChange={(value: 'Bug' | 'Feature' | 'Task') =>
                        setImprovedTask({ ...improvedTask, type: value })
                      }
                    >
                      <SelectTrigger id="ai-type">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Bug">Bug</SelectItem>
                        <SelectItem value="Feature">Feature</SelectItem>
                        <SelectItem value="Task">Task</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </div>

              {/* AI Metadata */}
              <div className="rounded-lg border bg-muted/30 p-4 space-y-2">
                <p className="text-sm font-medium">AI Metadata</p>
                <div className="grid grid-cols-2 gap-4 text-xs text-muted-foreground">
                  <div>
                    <span className="font-medium">Status:</span> {agentResponse.status}
                  </div>
                  <div>
                    <span className="font-medium">Execution Time:</span>{' '}
                    {(agentResponse.executionTimeMs / 1000).toFixed(2)}s
                  </div>
                  <div>
                    <span className="font-medium">Cost:</span> ${agentResponse.executionCostUsd.toFixed(2)}
                  </div>
                  <div>
                    <span className="font-medium">Tools:</span>{' '}
                    {agentResponse.toolsCalled.join(', ') || 'N/A'}
                  </div>
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                Discard
              </Button>
              <Button variant="outline" onClick={() => setStep('input')}>
                Edit in Classic Mode
              </Button>
              <Button onClick={handleApproveAndCreate} disabled={isCreating || !improvedTask.title.trim()}>
                {isCreating ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Creating...
                  </>
                ) : (
                  <>
                    <Sparkles className="mr-2 h-4 w-4" />
                    Approve & Create Task
                  </>
                )}
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}
