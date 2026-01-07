import { useState, useEffect, useRef, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { showError, showSuccess } from "@/lib/sweetalert";
import { defectsApi, type DefectDetail } from '@/api/defects';
import { usersApi } from '@/api/users';
import {
  Trash2,
  Loader2,
  User,
  Calendar,
} from 'lucide-react';
import type { Defect, DefectSeverity, DefectStatus, UpdateDefectRequest } from '@/types';
import { format } from 'date-fns';

interface DefectDetailSheetProps {
  defect: Defect | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  projectId: number;
  onDefectUpdated?: () => void;
  onDefectDeleted?: () => void;
}


export function DefectDetailSheet({
  defect,
  open,
  onOpenChange,
  projectId,
  onDefectUpdated,
  onDefectDeleted,
}: DefectDetailSheetProps) {
  const queryClient = useQueryClient();
  const [isSaving, setIsSaving] = useState(false);
  const saveTimeoutRef = useRef<NodeJS.Timeout>();

  // Local state for editable fields
  const [localDefect, setLocalDefect] = useState<DefectDetail | null>(null);

  // Load full defect data when opened
  const { data: fullDefect, isLoading } = useQuery({
    queryKey: ['defect', defect?.id],
    queryFn: () => defectsApi.getById(projectId, defect!.id),
    enabled: !!defect?.id && open,
  });

  // Load related data
  const { data: usersData } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(),
    enabled: open,
  });


  // Initialize local state when defect loads
  useEffect(() => {
    if (fullDefect) {
      setLocalDefect(fullDefect);
    } else if (defect) {
      setLocalDefect(defect as DefectDetail);
    }
  }, [fullDefect, defect]);

  // Auto-save debounced function
  const debouncedSave = useCallback(
    (updates: UpdateDefectRequest) => {
      if (!localDefect) return;

      if (saveTimeoutRef.current) {
        clearTimeout(saveTimeoutRef.current);
      }

      saveTimeoutRef.current = setTimeout(async () => {
        setIsSaving(true);
        try {
          await defectsApi.update(projectId, localDefect.id, updates);
          queryClient.invalidateQueries({ queryKey: ['defect', localDefect.id] });
          queryClient.invalidateQueries({ queryKey: ['defects', projectId] });
          onDefectUpdated?.();
        } catch (error) {
          showError('Failed to save');
        } finally {
          setIsSaving(false);
        }
      }, 1000);
    },
    [localDefect, queryClient, projectId, onDefectUpdated]
  );

  // Update handlers with auto-save
  const handleFieldChange = <K extends keyof UpdateDefectRequest>(
    field: K,
    value: UpdateDefectRequest[K]
  ) => {
    if (!localDefect) return;
    setLocalDefect({ ...localDefect, [field]: value } as DefectDetail);
    debouncedSave({ [field]: value });
  };

  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!localDefect) throw new Error('No defect selected');
      return defectsApi.delete(projectId, localDefect.id);
    },
    onSuccess: () => {
      showSuccess("Defect deleted");
      queryClient.invalidateQueries({ queryKey: ['defects', projectId] });
      onOpenChange(false);
      onDefectDeleted?.();
    },
    onError: () => {
      showError('Failed to delete defect');
    },
  });

  if (!localDefect && !isLoading) return null;

  if (isLoading) {
    return (
      <Sheet open={open} onOpenChange={onOpenChange}>
        <SheetContent className="w-full sm:max-w-4xl overflow-y-auto">
          <div className="flex items-center justify-center h-full">
            <Loader2 className="h-6 w-6 animate-spin" />
          </div>
        </SheetContent>
      </Sheet>
    );
  }

  if (!localDefect) return null;

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="w-full sm:max-w-4xl overflow-y-auto">
        <SheetHeader className="sticky top-0 bg-background z-10 pb-4 border-b">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <SheetTitle className="text-lg font-semibold">
                  #{localDefect.id}
                </SheetTitle>
                {isSaving && (
                  <Badge variant="outline" className="text-xs">
                    <Loader2 className="h-3 w-3 mr-1 animate-spin" />
                    Saving...
                  </Badge>
                )}
              </div>
              <Input
                value={localDefect.title}
                onChange={(e) => handleFieldChange('title', e.target.value)}
                className="text-xl font-bold border-none p-0 h-auto focus-visible:ring-0"
                placeholder="Defect title"
              />
            </div>
          </div>
        </SheetHeader>

        <div className="grid grid-cols-1 lg:grid-cols-[1fr_300px] gap-6 mt-6">
          {/* Left Panel */}
          <div className="space-y-6">
            {/* Description */}
            <div className="space-y-2">
              <Label>Description</Label>
              <Textarea
                id="defect-description"
                name="description"
                value={localDefect.description}
                onChange={(e) => handleFieldChange('description', e.target.value)}
                rows={6}
                placeholder="Describe the defect..."
                className="resize-none"
              />
            </div>

            {/* Steps to Reproduce */}
            <div className="space-y-2">
              <Label>Steps to Reproduce</Label>
              <Textarea
                id="defect-steps-to-reproduce"
                name="stepsToReproduce"
                value={localDefect.stepsToReproduce || ''}
                onChange={(e) => handleFieldChange('stepsToReproduce', e.target.value)}
                rows={6}
                placeholder="1. Go to...&#10;2. Click on...&#10;3. Observe that..."
                className="resize-none font-mono text-sm"
              />
            </div>

            {/* Resolution */}
            {(localDefect.status === 'Resolved' || localDefect.status === 'Closed') && (
              <div className="space-y-2">
                <Label>Resolution</Label>
                <Textarea
                  id="defect-resolution"
                  name="resolution"
                  value={localDefect.resolution || ''}
                  onChange={(e) => handleFieldChange('resolution', e.target.value)}
                  rows={4}
                  placeholder="How was this defect resolved?"
                  className="resize-none"
                />
              </div>
            )}
          </div>

          {/* Right Panel */}
          <div className="space-y-6">
            {/* Status & Severity */}
            <div className="space-y-4 p-4 border rounded-lg">
              <div className="space-y-2">
                <Label>Status</Label>
                <Select
                  value={localDefect.status}
                  onValueChange={(value: DefectStatus) => handleFieldChange('status', value)}
                >
                  <SelectTrigger id="defect-status" name="status">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Open">Open</SelectItem>
                    <SelectItem value="InProgress">In Progress</SelectItem>
                    <SelectItem value="Resolved">Resolved</SelectItem>
                    <SelectItem value="Closed">Closed</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Severity</Label>
                <Select
                  value={localDefect.severity}
                  onValueChange={(value: DefectSeverity) => handleFieldChange('severity', value)}
                >
                  <SelectTrigger id="defect-severity" name="severity">
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
                <Label>Assignee</Label>
                <Select
                  value={localDefect.assignedToId?.toString() || 'unassigned'}
                  onValueChange={(value) =>
                    handleFieldChange('assignedToId', value === 'unassigned' ? undefined : parseInt(value))
                  }
                >
                  <SelectTrigger id="defect-assignee" name="assignedToId">
                    <SelectValue placeholder="Unassigned" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="unassigned">Unassigned</SelectItem>
                    {usersData?.users?.map((user) => (
                      <SelectItem key={user.id} value={user.id.toString()}>
                        {user.firstName} {user.lastName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Environment</Label>
                <Input
                  id="defect-environment"
                  name="foundInEnvironment"
                  value={localDefect.foundInEnvironment || ''}
                  onChange={(e) => handleFieldChange('foundInEnvironment', e.target.value)}
                  placeholder="e.g., Production, Chrome 120"
                />
              </div>
            </div>

            {/* Metadata */}
            <div className="space-y-3 p-4 border rounded-lg">
              <h3 className="text-sm font-medium">Metadata</h3>
              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2">
                  <User className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">Reported by:</span>
                  <span>{localDefect.reportedByName || 'Unknown'}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">Reported:</span>
                  <span>
                    {localDefect.reportedAt
                      ? format(new Date(localDefect.reportedAt), 'MMM d, yyyy')
                      : localDefect.createdAt
                      ? format(new Date(localDefect.createdAt), 'MMM d, yyyy')
                      : 'Unknown'}
                  </span>
                </div>
                {localDefect.resolvedAt && (
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    <span className="text-muted-foreground">Resolved:</span>
                    <span>{format(new Date(localDefect.resolvedAt), 'MMM d, yyyy')}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Actions */}
            <div className="space-y-2">
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button variant="destructive" className="w-full" size="sm">
                    <Trash2 className="mr-2 h-4 w-4" />
                    Delete Defect
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Delete Defect?</AlertDialogTitle>
                    <AlertDialogDescription>
                      This action cannot be undone. This will permanently delete the defect.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                    <AlertDialogAction
                      onClick={() => deleteMutation.mutate()}
                      className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                    >
                      {deleteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      Delete
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}
