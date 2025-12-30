import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { defectsApi } from '@/api/defects';
import { usersApi } from '@/api/users';
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
import { Loader2 } from 'lucide-react';
import type { CreateDefectRequest, DefectSeverity } from '@/types';

interface CreateDefectDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  projectId: number;
}

export function CreateDefectDialog({
  open,
  onOpenChange,
  projectId,
}: CreateDefectDialogProps) {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<CreateDefectRequest>({
    title: '',
    description: '',
    severity: 'Medium',
    foundInEnvironment: '',
    stepsToReproduce: '',
  });

  const { data: usersData } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(),
    enabled: open,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateDefectRequest) => defectsApi.create(projectId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['defects'] });
      setFormData({
        title: '',
        description: '',
        severity: 'Medium',
        foundInEnvironment: '',
        stepsToReproduce: '',
      });
      onOpenChange(false);
      showSuccess("Defect reported successfully");
    },
    onError: (error) => {
      showError('Failed to report defect');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate(formData);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Report a Defect</DialogTitle>
            <DialogDescription>
              Document a bug or issue found in the project.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="title">
                Title <span className="text-destructive">*</span>
              </Label>
              <Input
                id="title"
                name="title"
                placeholder="Brief description of the defect"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">
                Description <span className="text-destructive">*</span>
              </Label>
              <Textarea
                id="description"
                name="description"
                placeholder="Detailed description of the issue"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={3}
                required
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="severity">
                  Severity <span className="text-destructive">*</span>
                </Label>
                <Select
                  value={formData.severity}
                  onValueChange={(value: DefectSeverity) =>
                    setFormData({ ...formData, severity: value })
                  }
                >
                  <SelectTrigger id="severity">
                    <SelectValue placeholder="Select severity" />
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
                <Label htmlFor="assignee">Assignee</Label>
                <Select
                  value={formData.assignedToId?.toString() || 'unassigned'}
                  onValueChange={(value) =>
                    setFormData({
                      ...formData,
                      assignedToId: value === 'unassigned' ? undefined : parseInt(value),
                    })
                  }
                >
                  <SelectTrigger id="assignee">
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
            </div>
            <div className="space-y-2">
              <Label htmlFor="environment">Environment</Label>
              <Input
                id="environment"
                name="environment"
                placeholder="e.g., Production, Staging, Chrome 120, Windows 11"
                value={formData.foundInEnvironment}
                onChange={(e) =>
                  setFormData({ ...formData, foundInEnvironment: e.target.value })
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="stepsToReproduce">Steps to Reproduce</Label>
              <Textarea
                id="stepsToReproduce"
                name="stepsToReproduce"
                placeholder="1. Go to...&#10;2. Click on...&#10;3. Observe that..."
                value={formData.stepsToReproduce}
                onChange={(e) =>
                  setFormData({ ...formData, stepsToReproduce: e.target.value })
                }
                rows={4}
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={createMutation.isPending}>
              {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Report Defect
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
