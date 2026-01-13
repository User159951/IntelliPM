import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsApi } from '@/api/projects';
import { useProjectPermissions } from '@/hooks/useProjectPermissions';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { showSuccess, showError } from "@/lib/sweetalert";
import { Loader2 } from 'lucide-react';
import type { Project, UpdateProjectRequest, ProjectType, ProjectStatus } from '@/types';
import { useProjectTypes } from '@/hooks/useLookups';
import { Skeleton } from '@/components/ui/skeleton';
import { useTranslation } from 'react-i18next';

interface EditProjectDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  project: Project;
}

export function EditProjectDialog({ open, onOpenChange, project }: EditProjectDialogProps) {
  const queryClient = useQueryClient();
  const permissions = useProjectPermissions(project.id);
  const { projectTypes, isLoading: isLoadingProjectTypes } = useProjectTypes();
  const { t } = useTranslation('common');
  const [formData, setFormData] = useState<UpdateProjectRequest>({
    name: project.name,
    description: project.description,
    status: project.status,
    type: project.type,
    sprintDurationDays: project.sprintDurationDays,
  });

  // Update form data when project changes
  useEffect(() => {
    if (project) {
      // Convert OnHold/Completed to Active for backend compatibility
      const backendStatus = project.status === 'OnHold' || project.status === 'Completed' 
        ? 'Active' 
        : project.status;
      
      setFormData({
        name: project.name,
        description: project.description,
        status: backendStatus as ProjectStatus,
        type: project.type,
        sprintDurationDays: project.sprintDurationDays,
      });
    }
  }, [project]);

  const updateMutation = useMutation({
    mutationFn: (data: UpdateProjectRequest) => projectsApi.update(project.id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['project', project.id] });
      onOpenChange(false);
      showSuccess(t('messages.success.projectUpdated'), t('messages.success.projectUpdatedDesc'));
    },
    onError: () => {
      showError(t('messages.error.failedToUpdate'));
    },
  });

  // Prevent opening if user doesn't have permission
  useEffect(() => {
    if (open && !permissions.canEditProject) {
      onOpenChange(false);
      showError(t('messages.error.accessDenied'), t('messages.error.accessDeniedDesc'));
    }
  }, [open, permissions.canEditProject, onOpenChange, t]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!permissions.canEditProject) {
      showError(t('messages.error.accessDenied'), t('messages.error.accessDeniedDesc'));
      return;
    }
    
    // Validate name is not empty
    if (!formData.name || formData.name.trim() === '') {
      showError(t('messages.error.validationError'), t('messages.error.nameRequired'));
      return;
    }

    // Validate name length (max 200 chars)
    if (formData.name && formData.name.length > 200) {
      showError(t('messages.error.validationError'), t('messages.error.nameMaxLength'));
      return;
    }

    // Only send fields that have changed
    const changes: UpdateProjectRequest = {};
    if (formData.name !== project.name) changes.name = formData.name;
    if (formData.description !== project.description) changes.description = formData.description;
    
    // Handle status: compare with backend-compatible status
    const projectBackendStatus = project.status === 'OnHold' || project.status === 'Completed' 
      ? 'Active' 
      : project.status;
    if (formData.status !== projectBackendStatus) {
      changes.status = formData.status as 'Active' | 'Archived';
    }
    
    if (formData.type !== project.type) changes.type = formData.type;
    if (formData.sprintDurationDays !== project.sprintDurationDays) {
      changes.sprintDurationDays = formData.sprintDurationDays;
    }

    // If no changes, just close the dialog
    if (Object.keys(changes).length === 0) {
      onOpenChange(false);
      return;
    }

    updateMutation.mutate(changes);
  };

  const handleCancel = () => {
    // Reset form data to original project values (convert OnHold/Completed to Active)
    const backendStatus = project.status === 'OnHold' || project.status === 'Completed' 
      ? 'Active' 
      : project.status;
    
    setFormData({
      name: project.name,
      description: project.description,
      status: backendStatus as ProjectStatus,
      type: project.type,
      sprintDurationDays: project.sprintDurationDays,
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{t('buttons.edit')} {t('labels.name').toLowerCase()}</DialogTitle>
            <DialogDescription>
              {t('descriptions.updateProjectDetails')}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="edit-name">{t('labels.projectName')} *</Label>
              <Input
                id="edit-name"
                name="name"
                placeholder={t('placeholders.enterProjectName')}
                value={formData.name || ''}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
                maxLength={200}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-description">{t('labels.description')}</Label>
              <Textarea
                id="edit-description"
                name="description"
                placeholder={t('placeholders.describeProject')}
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={3}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-type">{t('labels.projectType')}</Label>
                {isLoadingProjectTypes ? (
                  <Skeleton className="h-10 w-full" />
                ) : (
                  <Select
                    value={formData.type || 'Scrum'}
                    onValueChange={(value: ProjectType) => setFormData({ ...formData, type: value })}
                  >
                    <SelectTrigger id="edit-type">
                      <SelectValue placeholder={t('placeholders.selectType')} />
                    </SelectTrigger>
                    <SelectContent>
                      {projectTypes.map((type) => (
                        <SelectItem key={type.value} value={type.value}>
                          {type.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-sprintDuration">{t('labels.sprintDuration')}</Label>
                <Input
                  id="edit-sprintDuration"
                  name="sprintDurationDays"
                  type="number"
                  min={1}
                  max={30}
                  value={formData.sprintDurationDays || 14}
                  onChange={(e) => setFormData({ ...formData, sprintDurationDays: parseInt(e.target.value) || 14 })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-status">{t('labels.status')}</Label>
              <Select
                value={formData.status === 'OnHold' || formData.status === 'Completed' ? 'Active' : (formData.status || 'Active')}
                onValueChange={(value: 'Active' | 'Archived') => setFormData({ ...formData, status: value })}
                disabled={project.status === 'Archived'}
              >
                <SelectTrigger id="edit-status">
                  <SelectValue placeholder={t('placeholders.selectStatus')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Active">{t('status.active')}</SelectItem>
                  <SelectItem value="Archived">{t('status.inactive')}</SelectItem>
                </SelectContent>
              </Select>
              {(project.status === 'OnHold' || project.status === 'Completed') && (
                <p className="text-xs text-muted-foreground">
                  {t('descriptions.currentStatusNote', { status: project.status })}
                </p>
              )}
              {project.status === 'Archived' && (
                <p className="text-xs text-muted-foreground">
                  {t('descriptions.archivedCannotEdit')}
                </p>
              )}
            </div>
            {project.status === 'Archived' && (
              <div className="rounded-md bg-muted p-3">
                <p className="text-sm text-muted-foreground">
                  {t('descriptions.archivedProjectNote')}
                </p>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleCancel} disabled={updateMutation.isPending}>
              {t('buttons.cancel')}
            </Button>
            <Button type="submit" disabled={updateMutation.isPending || project.status === 'Archived' || !permissions.canEditProject}>
              {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {t('buttons.savingChanges')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
