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

interface EditProjectDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  project: Project;
}

export function EditProjectDialog({ open, onOpenChange, project }: EditProjectDialogProps) {
  const queryClient = useQueryClient();
  const permissions = useProjectPermissions(project.id);
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
      showSuccess("Project updated", "Your project has been successfully updated.");
    },
    onError: () => {
      showError('Failed to update project');
    },
  });

  // Prevent opening if user doesn't have permission
  useEffect(() => {
    if (open && !permissions.canEditProject) {
      onOpenChange(false);
      showError("Access denied", "You do not have permission to edit this project.");
    }
  }, [open, permissions.canEditProject, onOpenChange]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!permissions.canEditProject) {
      showError("Access denied", "You do not have permission to edit this project.");
      return;
    }
    
    // Validate name is not empty
    if (!formData.name || formData.name.trim() === '') {
      showError("Validation error", "Project name is required");
      return;
    }

    // Validate name length (max 200 chars)
    if (formData.name && formData.name.length > 200) {
      showError("Validation error", "Project name must not exceed 200 characters");
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
            <DialogTitle>Edit project</DialogTitle>
            <DialogDescription>
              Update project details. Changes will be saved immediately.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="edit-name">Project name *</Label>
              <Input
                id="edit-name"
                name="name"
                placeholder="Enter project name"
                value={formData.name || ''}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
                maxLength={200}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-description">Description</Label>
              <Textarea
                id="edit-description"
                name="description"
                placeholder="Describe your project"
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={3}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-type">Project type</Label>
                <Select
                  value={formData.type || 'Scrum'}
                  onValueChange={(value: ProjectType) => setFormData({ ...formData, type: value })}
                >
                  <SelectTrigger id="edit-type">
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Scrum">Scrum</SelectItem>
                    <SelectItem value="Kanban">Kanban</SelectItem>
                    <SelectItem value="Waterfall">Waterfall</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-sprintDuration">Sprint duration (days)</Label>
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
              <Label htmlFor="edit-status">Status</Label>
              <Select
                value={formData.status === 'OnHold' || formData.status === 'Completed' ? 'Active' : (formData.status || 'Active')}
                onValueChange={(value: 'Active' | 'Archived') => setFormData({ ...formData, status: value })}
                disabled={project.status === 'Archived'}
              >
                <SelectTrigger id="edit-status">
                  <SelectValue placeholder="Select status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Active">Active</SelectItem>
                  <SelectItem value="Archived">Archived</SelectItem>
                </SelectContent>
              </Select>
              {(project.status === 'OnHold' || project.status === 'Completed') && (
                <p className="text-xs text-muted-foreground">
                  Current status is "{project.status}". Backend only supports Active/Archived. Selecting Active will update the status.
                </p>
              )}
              {project.status === 'Archived' && (
                <p className="text-xs text-muted-foreground">
                  Archived projects cannot be edited. Restore the project first.
                </p>
              )}
            </div>
            {project.status === 'Archived' && (
              <div className="rounded-md bg-muted p-3">
                <p className="text-sm text-muted-foreground">
                  This project is archived. Most fields are read-only. To make changes, restore the project first.
                </p>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleCancel} disabled={updateMutation.isPending}>
              Cancel
            </Button>
            <Button type="submit" disabled={updateMutation.isPending || project.status === 'Archived' || !permissions.canEditProject}>
              {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Save changes
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
