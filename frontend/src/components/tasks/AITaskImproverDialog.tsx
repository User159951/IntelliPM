import { useState, useEffect, useCallback } from 'react';
import { useMutation } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { QuotaAlertBanner } from '@/components/ai-governance/QuotaAlertBanner';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from '@/lib/sweetalert';
import { useAIErrorHandler } from '@/hooks/useAIErrorHandler';
import type { ImproveTaskRequest, AgentResponse } from '@/types';
import {
  Loader2,
  Sparkles,
  X,
  Plus,
  AlertCircle,
  CheckCircle2,
  RefreshCw,
} from 'lucide-react';
import { cn } from '@/lib/utils';

export interface ImprovedTask {
  title: string;
  description: string;
  acceptanceCriteria: string[];
  storyPoints: number;
  qualityScore: number;
  suggestions: string[];
}

interface AITaskImproverDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialTitle: string;
  initialDescription: string;
  projectId?: number;
  onApply: (improved: ImprovedTask) => void;
}

function getQualityScoreColor(score: number): string {
  if (score >= 8) return 'bg-green-500/10 text-green-500 border-green-500/20';
  if (score >= 6) return 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20';
  return 'bg-orange-500/10 text-orange-500 border-orange-500/20';
}

function parseImprovedTaskData(response: AgentResponse): ImprovedTask | null {
  try {
    // Try to parse JSON from content
    const jsonMatch = response.content.match(/\{[\s\S]*\}/);
    if (jsonMatch) {
      const data = JSON.parse(jsonMatch[0]);
      
      // Map ImprovedTaskData to ImprovedTask
      return {
        title: data.title || '',
        description: data.description || '',
        acceptanceCriteria: data.acceptanceCriteria || [],
        storyPoints: data.suggestedStoryPoints || data.storyPoints || 0,
        qualityScore: data.qualityScore || 7, // Default if not provided
        suggestions: data.suggestions || data.definitionOfDone || [],
      };
    }
  } catch (error) {
    // Error parsing improved task data, return null (fail-safe)
  }
  
  return null;
}

export function AITaskImproverDialog({
  open,
  onOpenChange,
  initialTitle,
  initialDescription,
  projectId,
  onApply,
}: AITaskImproverDialogProps) {
  const [improvedTask, setImprovedTask] = useState<ImprovedTask | null>(null);
  const [editedTask, setEditedTask] = useState<ImprovedTask | null>(null);
  const [error, setError] = useState<string | null>(null);
  const { isRequestInFlight } = useRequestDeduplication();
  const [canRetry, setCanRetry] = useState(false);
  const [retryAfter, setRetryAfter] = useState<number | undefined>(undefined);

  const { handleError, executeWithErrorHandling, isBlocked, getErrorMessage } = useAIErrorHandler({
    showToast: false, // We'll handle toasts manually for better UX
  });

  // Reset state when dialog opens/closes
  useEffect(() => {
    if (!open) {
      setImprovedTask(null);
      setEditedTask(null);
      setError(null);
      setCanRetry(false);
      setRetryAfter(undefined);
    }
  }, [open]);

  // Check if blocked when dialog opens
  useEffect(() => {
    if (open && isBlocked()) {
      const errorMsg = getErrorMessage();
      if (errorMsg) {
        setError(errorMsg);
      }
    }
  }, [open, isBlocked, getErrorMessage]);

  const improveMutation = useMutation({
    mutationFn: async (data: ImproveTaskRequest) => {
      return executeWithErrorHandling(
        () => agentsApi.improveTask(data),
        30000 // Estimated 30 seconds
      );
    },
    onSuccess: (result) => {
      const parsed = parseImprovedTaskData(result);
      if (parsed) {
        setImprovedTask(parsed);
        setEditedTask(parsed);
        setError(null);
        setCanRetry(false);
        showToast('Analyse terminée!', 'success');
      } else {
        setError('Impossible de parser les données améliorées. Réponse reçue mais format inattendu.');
        setCanRetry(true);
      }
    },
    onError: (error: Error) => {
      const errorResult = handleError(error);
      setError(error.message || 'Échec de l\'analyse');
      setCanRetry(errorResult.canRetry);
      setRetryAfter(errorResult.retryAfter);
      
      if (!errorResult.isQuotaExceeded && !errorResult.isAIDisabled) {
        showError('Échec de l\'analyse', error.message);
      }
    },
  });

  const handleAnalyze = useCallback(() => {
    if (!initialTitle.trim() && !initialDescription.trim()) {
      showError('Erreur', 'Veuillez fournir au moins un titre ou une description');
      return;
    }

    if (isBlocked()) {
      const errorMsg = getErrorMessage();
      if (errorMsg) {
        setError(errorMsg);
      }
      return;
    }

    setError(null);
    setCanRetry(false);
    setRetryAfter(undefined);
    improveMutation.mutate({
      title: initialTitle,
      description: initialDescription,
      projectId,
    });
  }, [initialTitle, initialDescription, projectId, isBlocked, getErrorMessage, improveMutation]);

  const handleApply = () => {
    if (editedTask) {
      onApply(editedTask);
      onOpenChange(false);
    }
  };

  const handleAddCriterion = () => {
    if (editedTask) {
      setEditedTask({
        ...editedTask,
        acceptanceCriteria: [...editedTask.acceptanceCriteria, ''],
      });
    }
  };

  const handleUpdateCriterion = (index: number, value: string) => {
    if (editedTask) {
      const newCriteria = [...editedTask.acceptanceCriteria];
      newCriteria[index] = value;
      setEditedTask({
        ...editedTask,
        acceptanceCriteria: newCriteria,
      });
    }
  };

  const handleRemoveCriterion = (index: number) => {
    if (editedTask) {
      setEditedTask({
        ...editedTask,
        acceptanceCriteria: editedTask.acceptanceCriteria.filter((_, i) => i !== index),
      });
    }
  };

  const isAnalyzing = improveMutation.isPending;
  const hasResult = !!improvedTask && !!editedTask;
  const isBlockedState = isBlocked();

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[700px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Sparkles className="h-5 w-5 text-primary" />
            Améliorer la tâche avec l&apos;IA
          </DialogTitle>
          <DialogDescription>
            Analysez et améliorez votre tâche avec l&apos;intelligence artificielle
          </DialogDescription>
        </DialogHeader>

        <QuotaAlertBanner />

        <div className="space-y-6">
          {/* Input Section */}
          <div className="space-y-4">
            <div>
              <Label className="text-sm font-medium">Entrée</Label>
              <div className="mt-2 space-y-3 p-4 border rounded-lg bg-muted/30">
                <div>
                  <Label className="text-xs text-muted-foreground">Titre</Label>
                  <p className="mt-1 text-sm font-medium">{initialTitle || <span className="text-muted-foreground italic">(vide)</span>}</p>
                </div>
                <div>
                  <Label className="text-xs text-muted-foreground">Description</Label>
                  <p className="mt-1 text-sm whitespace-pre-wrap">{initialDescription || <span className="text-muted-foreground italic">(vide)</span>}</p>
                </div>
              </div>
            </div>

            <Button
              type="button"
              onClick={handleAnalyze}
              disabled={isAnalyzing || isBlocked() || (!initialTitle.trim() && !initialDescription.trim())}
              className="w-full"
            >
              {isAnalyzing ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Analyse en cours...
                </>
              ) : (
                <>
                  <Sparkles className="mr-2 h-4 w-4" />
                  Analyser
                </>
              )}
            </Button>
          </div>

          {/* Loading State */}
          {isAnalyzing && (
            <div className="space-y-4 animate-in fade-in duration-300">
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin" />
                <span>L&apos;IA analyse votre tâche... Cela peut prendre jusqu&apos;à 30 secondes.</span>
              </div>
              <div className="space-y-2">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-32 w-full" />
                <Skeleton className="h-24 w-full" />
              </div>
            </div>
          )}

          {/* Error State */}
          {error && !isAnalyzing && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription className="space-y-2">
                <p>{error}</p>
                {canRetry && (
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleAnalyze}
                    className="mt-2"
                  >
                    <RefreshCw className="mr-2 h-3 w-3" />
                    Réessayer{retryAfter ? ` (dans ${retryAfter}s)` : ''}
                  </Button>
                )}
              </AlertDescription>
            </Alert>
          )}

          {/* Result Section */}
          {hasResult && !isAnalyzing && (
            <div className="space-y-4 animate-in fade-in duration-300">
              <div className="flex items-center justify-between">
                <Label className="text-sm font-medium">Résultat</Label>
                {improvedTask.qualityScore !== undefined && (
                  <Badge
                    variant="outline"
                    className={cn('font-mono', getQualityScoreColor(improvedTask.qualityScore))}
                  >
                    Score: {improvedTask.qualityScore}/10
                  </Badge>
                )}
              </div>

              {/* Improved Title */}
              <div className="space-y-2">
                <Label htmlFor="improved-title">Titre amélioré</Label>
                <Input
                  id="improved-title"
                  value={editedTask.title}
                  onChange={(e) =>
                    setEditedTask({ ...editedTask, title: e.target.value })
                  }
                  className={cn(
                    editedTask.title !== improvedTask.title &&
                      'border-primary ring-1 ring-primary'
                  )}
                />
                {editedTask.title !== improvedTask.title && (
                  <p className="text-xs text-muted-foreground">
                    Modifié depuis la suggestion originale
                  </p>
                )}
              </div>

              {/* Improved Description */}
              <div className="space-y-2">
                <Label htmlFor="improved-description">Description améliorée</Label>
                <Textarea
                  id="improved-description"
                  value={editedTask.description}
                  onChange={(e) =>
                    setEditedTask({ ...editedTask, description: e.target.value })
                  }
                  rows={6}
                  className={cn(
                    editedTask.description !== improvedTask.description &&
                      'border-primary ring-1 ring-primary'
                  )}
                />
                {editedTask.description !== improvedTask.description && (
                  <p className="text-xs text-muted-foreground">
                    Modifié depuis la suggestion originale
                  </p>
                )}
              </div>

              {/* Acceptance Criteria */}
              <div className="space-y-2">
                <Label>Critères d&apos;acceptation</Label>
                <div className="space-y-2">
                  {editedTask.acceptanceCriteria.map((criterion, index) => (
                    <div key={index} className="flex gap-2">
                      <Input
                        value={criterion}
                        onChange={(e) => handleUpdateCriterion(index, e.target.value)}
                        placeholder="Critère d'acceptation"
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        onClick={() => handleRemoveCriterion(index)}
                        className="flex-shrink-0"
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleAddCriterion}
                    className="w-full"
                  >
                    <Plus className="mr-2 h-4 w-4" />
                    Ajouter un critère
                  </Button>
                </div>
              </div>

              {/* Story Points */}
              <div className="space-y-2">
                <Label htmlFor="improved-story-points">Story Points suggérés</Label>
                <Input
                  id="improved-story-points"
                  type="number"
                  min={0}
                  value={editedTask.storyPoints}
                  onChange={(e) =>
                    setEditedTask({
                      ...editedTask,
                      storyPoints: parseInt(e.target.value) || 0,
                    })
                  }
                />
              </div>

              {/* Suggestions */}
              {editedTask.suggestions && editedTask.suggestions.length > 0 && (
                <div className="space-y-2">
                  <Label>Suggestions d&apos;amélioration</Label>
                  <div className="space-y-2 p-4 border rounded-lg bg-muted/30">
                    {editedTask.suggestions.map((suggestion, index) => (
                      <div key={index} className="flex items-start gap-2">
                        <CheckCircle2 className="h-4 w-4 mt-0.5 text-primary flex-shrink-0" />
                        <p className="text-sm flex-1">{suggestion}</p>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isAnalyzing}
          >
            Annuler
          </Button>
          <Button
            type="button"
            onClick={handleApply}
            disabled={!hasResult || isAnalyzing || isBlockedState}
          >
            <CheckCircle2 className="mr-2 h-4 w-4" />
            Appliquer
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
