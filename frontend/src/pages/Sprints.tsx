import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { sprintsApi } from '@/api/sprints';
import { projectsApi } from '@/api/projects';
import { useProjectPermissions } from '@/hooks/useProjectPermissions';
import { usePermissionsWithProject } from '@/hooks/usePermissions';
import { PermissionGuard } from '@/components/guards/PermissionGuard';
import { Button } from '@/components/ui/button';
import { PermissionButton } from '@/components/ui/permission-button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import type { Project } from '@/types';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { showSuccess, showError } from "@/lib/sweetalert";
import { Plus, Loader2, Play, CheckCircle2, Calendar, ListPlus } from 'lucide-react';
import { SprintPlanningAssistant } from '@/components/agents/SprintPlanningAssistant';
import { StartSprintDialog } from '@/components/sprints/StartSprintDialog';
import { CompleteSprintDialog } from '@/components/sprints/CompleteSprintDialog';
import { AddTasksToSprintDialog } from '@/components/sprints/AddTasksToSprintDialog';
import type { CreateSprintRequest, Sprint } from '@/types';

export default function Sprints() {
  const queryClient = useQueryClient();
  const [selectedProjectId, setSelectedProjectId] = useState<number | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [startingSprint, setStartingSprint] = useState<Sprint | null>(null);
  const [completingSprint, setCompletingSprint] = useState<Sprint | null>(null);
  const [addingTasksSprint, setAddingTasksSprint] = useState<Sprint | null>(null);
  const [formData, setFormData] = useState<Omit<CreateSprintRequest, 'projectId'>>({
    name: '',
    startDate: '',
    endDate: '',
    capacity: 40,
    goal: '',
  });

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const projectId = selectedProjectId || projectsData?.items?.[0]?.id;

  const permissions = useProjectPermissions(projectId || 0);
  const { can } = usePermissionsWithProject(projectId);

  const { data: sprintsData, isLoading } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId!),
    enabled: !!projectId,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateSprintRequest) => sprintsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sprints'] });
      setIsDialogOpen(false);
      setFormData({ name: '', startDate: '', endDate: '', capacity: 40, goal: '' });
      showSuccess("Sprint created");
    },
    onError: () => {
      showError('Failed to create sprint');
    },
  });

  const startMutation = useMutation({
    mutationFn: (sprintId: number) => sprintsApi.start(sprintId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sprints'] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setStartingSprint(null);
      showSuccess("Sprint started successfully");
    },
    onError: () => {
      showError('Failed to start sprint');
    },
  });

  const completeMutation = useMutation({
    mutationFn: ({ sprintId, incompleteTasksAction }: { sprintId: number; incompleteTasksAction?: 'next_sprint' | 'backlog' | 'keep' }) =>
      sprintsApi.complete(sprintId, incompleteTasksAction),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sprints'] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      setCompletingSprint(null);
      showSuccess("Sprint completed successfully");
    },
    onError: () => {
      showError('Failed to complete sprint');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!projectId) return;
    createMutation.mutate({ ...formData, projectId });
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active':
        return 'bg-green-500/10 text-green-500 border-green-500/20';
      case 'Completed':
        return 'bg-blue-500/10 text-blue-500 border-blue-500/20';
      default:
        return 'bg-muted text-muted-foreground';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Sprints</h1>
          <p className="text-muted-foreground">Manage your project sprints</p>
        </div>
        <div className="flex items-center gap-4">
          <Select
            value={projectId?.toString()}
            onValueChange={(value) => setSelectedProjectId(parseInt(value))}
          >
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
          <PermissionGuard 
            requiredPermission="sprints.create" 
            projectId={projectId || undefined}
            fallback={null}
            showNotification={false}
          >
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
              <DialogTrigger asChild>
                <Button disabled={!projectId}>
                  <Plus className="mr-2 h-4 w-4" />
                  Create Sprint
                </Button>
              </DialogTrigger>
            <DialogContent className="sm:max-w-[500px]">
              <form onSubmit={handleSubmit}>
                <DialogHeader>
                  <DialogTitle>Create new sprint</DialogTitle>
                  <DialogDescription>Plan your next sprint iteration.</DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                  <div className="space-y-2">
                    <Label htmlFor="sprint-name">Sprint name</Label>
                    <Input
                      id="sprint-name"
                      name="name"
                      placeholder="Sprint 1"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      required
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="sprint-startDate">Start date</Label>
                      <Input
                        id="sprint-startDate"
                        name="startDate"
                        type="date"
                        value={formData.startDate}
                        onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="sprint-endDate">End date</Label>
                      <Input
                        id="sprint-endDate"
                        name="endDate"
                        type="date"
                        value={formData.endDate}
                        onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="sprint-capacity">Capacity (story points)</Label>
                    <Input
                      id="sprint-capacity"
                      name="capacity"
                      type="number"
                      min={1}
                      value={formData.capacity}
                      onChange={(e) => setFormData({ ...formData, capacity: parseInt(e.target.value) || 40 })}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="sprint-goal">Sprint goal (optional)</Label>
                    <Textarea
                      id="sprint-goal"
                      name="goal"
                      placeholder="What do you want to achieve?"
                      value={formData.goal}
                      onChange={(e) => setFormData({ ...formData, goal: e.target.value })}
                      rows={2}
                    />
                  </div>
                </div>
                <DialogFooter>
                  <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>
                    Cancel
                  </Button>
                  <Button type="submit" disabled={createMutation.isPending}>
                    {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Create sprint
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
          </PermissionGuard>
        </div>
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
      ) : !projectId ? (
        <Card className="py-16 text-center">
          <p className="text-muted-foreground">Select a project to view sprints</p>
        </Card>
      ) : sprintsData?.sprints?.length === 0 ? (
        <Card className="flex flex-col items-center justify-center py-16">
          <Calendar className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium">No sprints yet</h3>
          <p className="text-muted-foreground mb-4">Create your first sprint to get started</p>
          <PermissionGuard 
            requiredPermission="sprints.create" 
            projectId={projectId || undefined}
            fallback={null}
            showNotification={false}
          >
            <Button onClick={() => setIsDialogOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Create Sprint
            </Button>
          </PermissionGuard>
        </Card>
      ) : (
        <div className="space-y-6">
        <div className="space-y-4">
          {sprintsData?.sprints?.map((sprint) => (
            <Card key={sprint.id}>
              <CardHeader className="flex flex-row items-start justify-between">
                <div className="space-y-1">
                  <div className="flex items-center gap-3">
                    <CardTitle className="text-lg">{sprint.name}</CardTitle>
                    <Badge className={getStatusColor(sprint.status)}>{sprint.status}</Badge>
                  </div>
                  <CardDescription className="flex items-center gap-4">
                      <span className="flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                      {new Date(sprint.startDate).toLocaleDateString()} -{' '}
                      {new Date(sprint.endDate).toLocaleDateString()}
                      </span>
                      <span>Capacity: {sprint.capacity} pts</span>
                  </CardDescription>
                </div>
                <div className="flex items-center gap-2">
                  <PermissionGuard 
                    requiredPermission="sprints.edit" 
                    projectId={projectId || undefined}
                    fallback={null}
                    showNotification={false}
                  >
                    {sprint.status !== 'Completed' && (
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setAddingTasksSprint(sprint)}
                      >
                        <ListPlus className="mr-2 h-3 w-3" />
                        Add Tasks
                      </Button>
                    )}
                  </PermissionGuard>
                  <PermissionGuard 
                    requiredPermission="sprints.manage" 
                    projectId={projectId || undefined}
                    fallback={null}
                    showNotification={false}
                  >
                    {sprint.status === 'Planned' && (
                      <Button
                        size="sm"
                        onClick={() => setStartingSprint(sprint)}
                      >
                        <Play className="mr-2 h-3 w-3" />
                        Start Sprint
                      </Button>
                    )}
                    {sprint.status === 'Active' && (
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setCompletingSprint(sprint)}
                      >
                        <CheckCircle2 className="mr-2 h-3 w-3" />
                        Complete
                      </Button>
                    )}
                  </PermissionGuard>
                </div>
              </CardHeader>
              {sprint.goal && (
                <CardContent>
                  <div className="text-sm">
                    <span className="font-medium">Goal: </span>
                    <span className="text-muted-foreground">{sprint.goal}</span>
                  </div>
                </CardContent>
              )}
            </Card>
          ))}
          </div>

          {/* AI Sprint Planning Assistant for the first active sprint (if any) */}
          {sprintsData?.sprints
            ?.filter((s) => s.status === 'Active')
            .slice(0, 1)
            .map((sprint) => (
              <SprintPlanningAssistant key={sprint.id} sprintId={sprint.id} />
            ))}
        </div>
      )}

      {startingSprint && (
        <StartSprintDialog
          open={!!startingSprint}
          onOpenChange={(open) => !open && setStartingSprint(null)}
          sprint={startingSprint}
          onConfirm={() => startMutation.mutate(startingSprint.id)}
          isLoading={startMutation.isPending}
        />
      )}

      {completingSprint && (
        <CompleteSprintDialog
          open={!!completingSprint}
          onOpenChange={(open) => !open && setCompletingSprint(null)}
          sprint={completingSprint}
          onConfirm={(incompleteTasksAction) =>
            completeMutation.mutate({ sprintId: completingSprint.id, incompleteTasksAction })
          }
          isLoading={completeMutation.isPending}
        />
      )}

      {addingTasksSprint && (
        <AddTasksToSprintDialog
          open={!!addingTasksSprint}
          onOpenChange={(open) => !open && setAddingTasksSprint(null)}
          sprint={addingTasksSprint}
        />
      )}
    </div>
  );
}
