import { useState, useEffect, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { DragDropContext, Droppable, Draggable, DropResult } from 'react-beautiful-dnd';
import { backlogApi } from '@/api/backlog';
import { sprintsApi } from '@/api/sprints';
import { projectsApi } from '@/api/projects';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { Badge } from '@/components/ui/badge';
import { showToast, showSuccess, showError, showWarning, showConfirm } from "@/lib/sweetalert";
import { Plus, Loader2, ChevronRight, Layers, FileText, BookOpen, GripVertical, AlertTriangle } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { CreateFeatureRequest, CreateStoryRequest, Project, Sprint } from '@/types';
import type { BacklogTaskDto } from '@/api/backlog';

type ItemType = 'epic' | 'feature' | 'story';

export default function Backlog() {
  const queryClient = useQueryClient();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [itemType, setItemType] = useState<ItemType>('story');
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    storyPoints: '',
    domainTag: '',
    acceptanceCriteria: '',
    epicId: '',
    featureId: '',
  });
  const [isMobile, setIsMobile] = useState(false);
  const [draggingTaskId, setDraggingTaskId] = useState<number | null>(null);

  // Check if mobile device (disable drag on mobile)
  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 768);
    };
    
    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const projectId = selectedProjectId ? parseInt(selectedProjectId) : null;

  // Fetch backlog tasks
  const { data: backlogData, isLoading: backlogLoading } = useQuery({
    queryKey: ['backlog', projectId],
    queryFn: () => backlogApi.getTasks(projectId!, { page: 1, pageSize: 100 }),
    enabled: !!projectId,
  });

  // Fetch sprints for drag-and-drop targets
  const { data: sprintsData } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId!),
    enabled: !!projectId,
  });

  // Mutations for adding/removing tasks from sprints
  const addTaskToSprintMutation = useMutation({
    mutationFn: ({ sprintId, taskIds }: { sprintId: number; taskIds: number[] }) =>
      sprintsApi.addTasksToSprint(sprintId, { taskIds }),
    onSuccess: (data) => {
      if (data.isOverCapacity) {
        showWarning('Sprint over capacity', data.capacityWarning || 'Sprint is over planned capacity');
      } else {
        showToast('Task added to sprint', 'success');
      }
      queryClient.invalidateQueries({ queryKey: ['backlog'] });
      queryClient.invalidateQueries({ queryKey: ['sprints'] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
    onError: (error: any) => {
      showError('Failed to add task to sprint', error.message || 'An error occurred');
      queryClient.invalidateQueries({ queryKey: ['backlog'] });
    },
  });

  const removeTaskFromSprintMutation = useMutation({
    mutationFn: ({ sprintId, taskIds }: { sprintId: number; taskIds: number[] }) =>
      sprintsApi.removeTasksFromSprint(sprintId, { taskIds }),
    onSuccess: () => {
      showToast('Task removed from sprint', 'success');
      queryClient.invalidateQueries({ queryKey: ['backlog'] });
      queryClient.invalidateQueries({ queryKey: ['sprints'] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
    onError: (error: any) => {
      showError('Failed to remove task from sprint', error.message || 'An error occurred');
      queryClient.invalidateQueries({ queryKey: ['sprints'] });
    },
  });

  // Handle drag end
  const handleDragEnd = async (result: DropResult) => {
    const { source, destination, draggableId } = result;

    setDraggingTaskId(null);

    if (!destination) return;

    // Extract task ID
    const taskId = parseInt(draggableId.replace('task-', ''));

    // If dropped on sprint droppable
    if (destination.droppableId.startsWith('sprint-')) {
      const sprintId = parseInt(destination.droppableId.replace('sprint-', ''));
      
      // Check if dropping multiple tasks (from backlog selection)
      const selectedTasks = [taskId]; // For now, single task drag
      
      // Show confirmation if dropping multiple tasks
      if (selectedTasks.length > 3) {
        const confirmed = await showConfirm(
          'Add multiple tasks?',
          `Are you sure you want to add ${selectedTasks.length} tasks to this sprint?`
        );
        if (!confirmed) return;
      }

      addTaskToSprintMutation.mutate({ sprintId, taskIds: selectedTasks });
    }

    // If dropped back on backlog (from sprint)
    if (destination.droppableId === 'backlog' && source.droppableId.startsWith('sprint-')) {
      const sprintId = parseInt(source.droppableId.replace('sprint-', ''));
      removeTaskFromSprintMutation.mutate({ sprintId, taskIds: [taskId] });
    }
  };

  const handleDragStart = (start: { draggableId: string }) => {
    const taskId = parseInt(start.draggableId.replace('task-', ''));
    setDraggingTaskId(taskId);
  };

  // Get active sprints for drop zones
  const activeSprints = useMemo(() => {
    if (!sprintsData?.sprints) return [];
    return sprintsData.sprints.filter((s: Sprint) => 
      s.status === 'Active' || s.status === 'Planned'
    );
  }, [sprintsData]);

  // Mock data for epics/features/stories (keeping existing functionality)
  const mockBacklog = {
    epics: [
      {
        id: 1,
        title: 'User Authentication System',
        description: 'Complete authentication and authorization features',
        features: [
          {
            id: 1,
            title: 'Social Login Integration',
            description: 'OAuth integration with major providers',
            storyPoints: 13,
            domainTag: 'Security',
            stories: [
              { id: 1, title: 'Google OAuth Setup', storyPoints: 5, domainTag: 'Security' },
              { id: 2, title: 'GitHub OAuth Setup', storyPoints: 5, domainTag: 'Security' },
              { id: 3, title: 'Session Management', storyPoints: 3, domainTag: 'Security' },
            ],
          },
          {
            id: 2,
            title: 'Two-Factor Authentication',
            description: '2FA support for enhanced security',
            storyPoints: 8,
            domainTag: 'Security',
            stories: [
              { id: 4, title: 'TOTP Implementation', storyPoints: 5, domainTag: 'Security' },
              { id: 5, title: 'Backup Codes', storyPoints: 3, domainTag: 'Security' },
            ],
          },
        ],
      },
      {
        id: 2,
        title: 'Dashboard Redesign',
        description: 'Modern dashboard with improved UX',
        features: [
          {
            id: 3,
            title: 'Widget System',
            description: 'Customizable dashboard widgets',
            storyPoints: 21,
            domainTag: 'UI',
            stories: [
              { id: 6, title: 'Widget Framework', storyPoints: 8, domainTag: 'UI' },
              { id: 7, title: 'Chart Widgets', storyPoints: 8, domainTag: 'UI' },
              { id: 8, title: 'Widget Persistence', storyPoints: 5, domainTag: 'Backend' },
            ],
          },
        ],
      },
    ],
  };

  const createEpicMutation = useMutation({
    mutationFn: (data: { title: string; description: string }) =>
      backlogApi.createEpic(parseInt(selectedProjectId!), data),
    onSuccess: () => {
      showSuccess("Epic created");
      setDialogOpen(false);
      resetForm();
    },
    onError: (error) => {
      showError('Failed to create epic');
    },
  });

  const createFeatureMutation = useMutation({
    mutationFn: (data: CreateFeatureRequest) => backlogApi.createFeature(parseInt(selectedProjectId!), data),
    onSuccess: () => {
      showSuccess("Feature created");
      setDialogOpen(false);
      resetForm();
    },
    onError: (error) => {
      showError('Failed to create feature');
    },
  });

  const createStoryMutation = useMutation({
    mutationFn: (data: CreateStoryRequest) => backlogApi.createStory(parseInt(selectedProjectId!), data),
    onSuccess: () => {
      showSuccess("Story created");
      setDialogOpen(false);
      resetForm();
    },
    onError: (error) => {
      showError('Failed to create story');
    },
  });

  const resetForm = () => {
    setFormData({
      title: '',
      description: '',
      storyPoints: '',
      domainTag: '',
      acceptanceCriteria: '',
      epicId: '',
      featureId: '',
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProjectId) return;

    switch (itemType) {
      case 'epic':
        createEpicMutation.mutate({
          title: formData.title,
          description: formData.description,
        });
        break;
      case 'feature':
        createFeatureMutation.mutate({
          title: formData.title,
          description: formData.description,
          storyPoints: formData.storyPoints ? parseInt(formData.storyPoints) : undefined,
          domainTag: formData.domainTag || undefined,
          epicId: parseInt(formData.epicId),
        });
        break;
      case 'story':
        createStoryMutation.mutate({
          title: formData.title,
          description: formData.description,
          storyPoints: formData.storyPoints ? parseInt(formData.storyPoints) : undefined,
          domainTag: formData.domainTag || undefined,
          featureId: parseInt(formData.featureId),
          acceptanceCriteria: formData.acceptanceCriteria || undefined,
        });
        break;
    }
  };

  const isPending = createEpicMutation.isPending || createFeatureMutation.isPending || createStoryMutation.isPending;

  const priorityColors: Record<string, string> = {
    Critical: 'border-l-red-500 bg-red-50 dark:bg-red-950/20',
    High: 'border-l-orange-500 bg-orange-50 dark:bg-orange-950/20',
    Medium: 'border-l-blue-500 bg-blue-50 dark:bg-blue-950/20',
    Low: 'border-l-slate-500 bg-slate-50 dark:bg-slate-950/20',
  };

  return (
    <DragDropContext onDragEnd={handleDragEnd} onDragStart={handleDragStart}>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-foreground">Backlog</h1>
            <p className="text-muted-foreground">Manage your epics, features, stories, and tasks</p>
          </div>
          <div className="flex items-center gap-4">
            <Select value={selectedProjectId} onValueChange={setSelectedProjectId}>
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
            <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
              <DialogTrigger asChild>
                <Button disabled={!selectedProjectId}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Item
                </Button>
              </DialogTrigger>
              <DialogContent className="sm:max-w-[500px]">
                <form onSubmit={handleSubmit}>
                  <DialogHeader>
                    <DialogTitle>Create backlog item</DialogTitle>
                    <DialogDescription>Add a new epic, feature, or story.</DialogDescription>
                  </DialogHeader>
                  <div className="grid gap-4 py-4">
                    <div className="space-y-2">
                      <Label>Item type</Label>
                      <Select value={itemType} onValueChange={(v: ItemType) => setItemType(v)}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="epic">Epic</SelectItem>
                          <SelectItem value="feature">Feature</SelectItem>
                          <SelectItem value="story">Story</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="title">Title</Label>
                      <Input
                        id="title"
                        placeholder="Enter title"
                        value={formData.title}
                        onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="description">Description</Label>
                      <Textarea
                        id="description"
                        placeholder="Describe this item"
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        rows={3}
                      />
                    </div>
                    {(itemType === 'feature' || itemType === 'story') && (
                      <>
                        <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-2">
                            <Label htmlFor="storyPoints">Story Points</Label>
                            <Input
                              id="storyPoints"
                              type="number"
                              min={0}
                              placeholder="Optional"
                              value={formData.storyPoints}
                              onChange={(e) => setFormData({ ...formData, storyPoints: e.target.value })}
                            />
                          </div>
                          <div className="space-y-2">
                            <Label htmlFor="domainTag">Domain Tag</Label>
                            <Input
                              id="domainTag"
                              placeholder="e.g., Security, UI"
                              value={formData.domainTag}
                              onChange={(e) => setFormData({ ...formData, domainTag: e.target.value })}
                            />
                          </div>
                        </div>
                      </>
                    )}
                    {itemType === 'feature' && (
                      <div className="space-y-2">
                        <Label htmlFor="epicId">Parent Epic</Label>
                        <Select
                          value={formData.epicId}
                          onValueChange={(v) => setFormData({ ...formData, epicId: v })}
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Select epic" />
                          </SelectTrigger>
                          <SelectContent>
                            {mockBacklog.epics.map((epic) => (
                              <SelectItem key={epic.id} value={epic.id.toString()}>
                                {epic.title}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    )}
                    {itemType === 'story' && (
                      <>
                        <div className="space-y-2">
                          <Label htmlFor="featureId">Parent Feature</Label>
                          <Select
                            value={formData.featureId}
                            onValueChange={(v) => setFormData({ ...formData, featureId: v })}
                          >
                            <SelectTrigger id="featureId">
                              <SelectValue placeholder="Select feature" />
                            </SelectTrigger>
                            <SelectContent>
                              {mockBacklog.epics.flatMap((epic) =>
                                epic.features.map((feature) => (
                                  <SelectItem key={feature.id} value={feature.id.toString()}>
                                    {feature.title}
                                  </SelectItem>
                                ))
                              )}
                            </SelectContent>
                          </Select>
                        </div>
                        <div className="space-y-2">
                          <Label htmlFor="acceptanceCriteria">Acceptance Criteria</Label>
                          <Textarea
                            id="acceptanceCriteria"
                            name="acceptanceCriteria"
                            placeholder="What must be true for this story to be complete?"
                            value={formData.acceptanceCriteria}
                            onChange={(e) => setFormData({ ...formData, acceptanceCriteria: e.target.value })}
                            rows={3}
                          />
                        </div>
                      </>
                    )}
                  </div>
                  <DialogFooter>
                    <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>
                      Cancel
                    </Button>
                    <Button type="submit" disabled={isPending}>
                      {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                      Create
                    </Button>
                  </DialogFooter>
                </form>
              </DialogContent>
            </Dialog>
          </div>
        </div>

        {!selectedProjectId ? (
          <Card className="py-16 text-center">
            <Layers className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <p className="text-muted-foreground">Select a project to view backlog</p>
          </Card>
        ) : (
          <div className="space-y-6">
            {/* Active Sprints Drop Zones */}
            {activeSprints.length > 0 && (
              <div className="space-y-2">
                <h2 className="text-lg font-semibold">Active Sprints</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {activeSprints.map((sprint: Sprint) => (
                    <Droppable
                      key={`sprint-${sprint.id}`}
                      droppableId={`sprint-${sprint.id}`}
                      isDropDisabled={isMobile || addTaskToSprintMutation.isPending}
                    >
                      {(provided, snapshot) => (
                        <Card
                          ref={provided.innerRef}
                          {...provided.droppableProps}
                          className={cn(
                            'min-h-[150px] transition-all',
                            snapshot.isDraggingOver && 'ring-2 ring-primary bg-primary/5',
                            snapshot.isDraggingOver && draggingTaskId && 'border-primary'
                          )}
                        >
                          <CardHeader className="pb-2">
                            <CardTitle className="text-sm">{sprint.name}</CardTitle>
                            <CardDescription className="text-xs">
                              {sprint.status} • Capacity: {sprint.capacity} pts
                            </CardDescription>
                          </CardHeader>
                          <CardContent>
                            {snapshot.isDraggingOver ? (
                              <div className="text-center py-8 text-muted-foreground">
                                <p className="font-medium">Drop task here</p>
                                <p className="text-xs mt-1">to add to sprint</p>
                              </div>
                            ) : (
                              <div className="text-center py-8 text-muted-foreground text-sm">
                                Drag tasks here
                              </div>
                            )}
                            {provided.placeholder}
                          </CardContent>
                        </Card>
                      )}
                    </Droppable>
                  ))}
                </div>
              </div>
            )}

            {/* Backlog Tasks */}
            <div className="space-y-2">
              <h2 className="text-lg font-semibold">Backlog Tasks</h2>
              {backlogLoading ? (
                <div className="space-y-2">
                  {[1, 2, 3].map((i) => (
                    <Card key={i} className="h-20 animate-pulse" />
                  ))}
                </div>
              ) : backlogData?.items && backlogData.items.length > 0 ? (
                <Droppable
                  droppableId="backlog"
                  isDropDisabled={isMobile || removeTaskFromSprintMutation.isPending}
                >
                  {(provided, snapshot) => (
                    <div
                      ref={provided.innerRef}
                      {...provided.droppableProps}
                      className={cn(
                        'space-y-2 min-h-[200px] p-4 rounded-lg border-2 border-dashed transition-all',
                        snapshot.isDraggingOver && 'border-primary bg-primary/5',
                        !snapshot.isDraggingOver && 'border-transparent'
                      )}
                    >
                      {backlogData.items.map((task: BacklogTaskDto, index: number) => (
                        <Draggable
                          key={`task-${task.id}`}
                          draggableId={`task-${task.id}`}
                          index={index}
                          isDragDisabled={isMobile || addTaskToSprintMutation.isPending}
                        >
                          {(provided, snapshot) => (
                            <Card
                              ref={provided.innerRef}
                              {...provided.draggableProps}
                              className={cn(
                                'cursor-grab active:cursor-grabbing transition-all border-l-4',
                                priorityColors[task.priority] || priorityColors.Medium,
                                snapshot.isDragging && 'shadow-lg rotate-1 opacity-90',
                                draggingTaskId === task.id && 'ring-2 ring-primary'
                              )}
                            >
                              <CardContent className="p-4">
                                <div className="flex items-start gap-3">
                                  <div {...provided.dragHandleProps} className="mt-1">
                                    <GripVertical className="h-4 w-4 text-muted-foreground" />
                                  </div>
                                  <div className="flex-1 min-w-0">
                                    <h3 className="font-medium text-sm truncate">{task.title}</h3>
                                    {task.description && (
                                      <p className="text-xs text-muted-foreground mt-1 line-clamp-2">
                                        {task.description}
                                      </p>
                                    )}
                                    <div className="flex items-center gap-2 mt-2">
                                      <Badge variant="outline" className="text-xs">
                                        {task.priority}
                                      </Badge>
                                      {task.storyPoints && (
                                        <Badge variant="secondary" className="text-xs">
                                          {task.storyPoints} pts
                                        </Badge>
                                      )}
                                      {task.assigneeName && (
                                        <Badge variant="outline" className="text-xs">
                                          {task.assigneeName}
                                        </Badge>
                                      )}
                                    </div>
                                  </div>
                                </div>
                              </CardContent>
                            </Card>
                          )}
                        </Draggable>
                      ))}
                      {provided.placeholder}
                      {snapshot.isDraggingOver && (
                        <div className="text-center py-8 text-muted-foreground border-2 border-dashed border-primary rounded-lg">
                          <p className="font-medium">Drop here to return to backlog</p>
                        </div>
                      )}
                    </div>
                  )}
                </Droppable>
              ) : (
                <Card className="py-8 text-center">
                  <p className="text-muted-foreground">No backlog tasks found</p>
                </Card>
              )}
            </div>

            {/* Epics/Features/Stories (existing functionality) */}
            <div className="space-y-4">
              <h2 className="text-lg font-semibold">Epics & Features</h2>
              {mockBacklog.epics.map((epic) => (
                <Collapsible key={epic.id} defaultOpen>
                  <Card>
                    <CollapsibleTrigger asChild>
                      <CardHeader className="cursor-pointer hover:bg-accent/50 transition-colors">
                        <div className="flex items-center gap-3">
                          <ChevronRight className="h-4 w-4 transition-transform [&[data-state=open]>svg]:rotate-90" />
                          <BookOpen className="h-5 w-5 text-primary" />
                          <div className="flex-1">
                            <CardTitle className="text-lg">{epic.title}</CardTitle>
                            <CardDescription>{epic.description}</CardDescription>
                          </div>
                          <Badge variant="outline">{epic.features.length} features</Badge>
                        </div>
                      </CardHeader>
                    </CollapsibleTrigger>
                    <CollapsibleContent>
                      <CardContent className="pt-0">
                        <div className="space-y-3 ml-8">
                          {epic.features.map((feature) => (
                            <Collapsible key={feature.id} defaultOpen>
                              <div className="border rounded-lg">
                                <CollapsibleTrigger asChild>
                                  <div className="p-3 cursor-pointer hover:bg-accent/50 transition-colors flex items-center gap-3">
                                    <ChevronRight className="h-4 w-4" />
                                    <FileText className="h-4 w-4 text-blue-500" />
                                    <div className="flex-1">
                                      <p className="font-medium">{feature.title}</p>
                                      <p className="text-sm text-muted-foreground">{feature.description}</p>
                                    </div>
                                    <div className="flex items-center gap-2">
                                      {feature.domainTag && (
                                        <Badge variant="secondary">{feature.domainTag}</Badge>
                                      )}
                                      <Badge variant="outline">{feature.storyPoints} pts</Badge>
                                    </div>
                                  </div>
                                </CollapsibleTrigger>
                                <CollapsibleContent>
                                  <div className="px-3 pb-3 ml-8 space-y-2">
                                    {feature.stories.map((story) => (
                                      <div
                                        key={story.id}
                                        className="p-2 border rounded bg-accent/30 flex items-center justify-between"
                                      >
                                        <div className="flex items-center gap-2">
                                          <div className="h-2 w-2 rounded-full bg-green-500" />
                                          <span className="text-sm">{story.title}</span>
                                        </div>
                                        <div className="flex items-center gap-2">
                                          {story.domainTag && (
                                            <Badge variant="secondary" className="text-xs">
                                              {story.domainTag}
                                            </Badge>
                                          )}
                                          <span className="text-xs text-muted-foreground">
                                            {story.storyPoints} pts
                                          </span>
                                        </div>
                                      </div>
                                    ))}
                                  </div>
                                </CollapsibleContent>
                              </div>
                            </Collapsible>
                          ))}
                        </div>
                      </CardContent>
                    </CollapsibleContent>
                  </Card>
                </Collapsible>
              ))}
            </div>
          </div>
        )}
      </div>
    </DragDropContext>
  );
}
