import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Loader2, Sparkles } from 'lucide-react';
import { agentsApi } from '@/api/agents';
import { showToast, showError } from '@/lib/sweetalert';
import type { AgentResponse } from '@/types';

interface TaskImproverDialogProps {
  taskId?: number;
  currentDescription: string;
  currentTitle?: string;
  projectId?: number;
  isOpen: boolean;
  onClose: () => void;
  onApply: (improvedDescription: string) => void;
}

export function TaskImproverDialog({
  taskId,
  currentDescription,
  currentTitle,
  projectId,
  isOpen,
  onClose,
  onApply,
}: TaskImproverDialogProps) {
  const [improvedDescription, setImprovedDescription] = useState<string>('');

  const improveMutation = useMutation({
    mutationFn: () =>
      agentsApi.improveTask({
        description: currentDescription,
        title: currentTitle,
        projectId,
      }),
    onSuccess: (data: AgentResponse) => {
      // Extract improved description from AgentResponse
      // The content might be JSON or plain text
      try {
        const parsed = JSON.parse(data.content);
        setImprovedDescription(parsed.description || parsed.improvedDescription || data.content);
      } catch {
        // If not JSON, use content directly
        setImprovedDescription(data.content);
      }
      showToast('Description améliorée avec succès', 'success');
    },
    onError: (error: Error) => {
      showError('Erreur lors de l\'amélioration', error.message);
    },
  });

  const handleApply = () => {
    if (improvedDescription) {
      onApply(improvedDescription);
      onClose();
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Sparkles className="h-5 w-5 text-purple-500" />
            Améliorer la description avec l'IA
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Description actuelle */}
          <div>
            <Label className="text-sm font-medium">Description actuelle</Label>
            <Textarea value={currentDescription} disabled className="mt-1" rows={4} />
          </div>

          {/* Bouton pour améliorer */}
          {!improvedDescription && (
            <Button
              onClick={() => improveMutation.mutate()}
              disabled={improveMutation.isPending}
              className="w-full"
            >
              {improveMutation.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Amélioration en cours...
                </>
              ) : (
                <>
                  <Sparkles className="mr-2 h-4 w-4" />
                  Améliorer avec l'IA
                </>
              )}
            </Button>
          )}

          {/* Description améliorée */}
          {improvedDescription && (
            <div>
              <Label className="text-sm font-medium">Description améliorée</Label>
              <Textarea
                value={improvedDescription}
                onChange={(e) => setImprovedDescription(e.target.value)}
                className="mt-1"
                rows={6}
              />
            </div>
          )}

          {/* Actions */}
          {improvedDescription && (
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={onClose}>
                Annuler
              </Button>
              <Button onClick={handleApply}>
                Appliquer
              </Button>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

