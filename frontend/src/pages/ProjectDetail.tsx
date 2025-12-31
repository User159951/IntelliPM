import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsApi } from '@/api/projects';
import { memberService } from '@/api/memberService';
import { useProjectPermissions } from '@/hooks/useProjectPermissions';
import { sprintsApi } from '@/api/sprints';
import { tasksApi } from '@/api/tasks';
import { milestonesApi } from '@/api/milestones';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import { Link } from 'react-router-dom';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { showSuccess, showError } from "@/lib/sweetalert";
import { ArrowLeft, Settings, Play, CheckCircle2, Sparkles, Plus, Pencil, Trash2, Package } from 'lucide-react';
import { ProjectInsightPanel } from '@/components/agents/ProjectInsightPanel';
import { RiskDetectionPanel } from '@/components/agents/RiskDetectionPanel';
import { SprintPlanningAssistant } from '@/components/agents/SprintPlanningAssistant';
import { AITaskImproverDialog } from '@/components/tasks/AITaskImproverDialog';
import { EditProjectDialog } from '@/components/projects/EditProjectDialog';
import { DeleteProjectDialog } from '@/components/projects/DeleteProjectDialog';
import { ProjectTimeline } from '@/components/projects/ProjectTimeline';
import { TeamMembersList } from '@/components/projects/TeamMembersList';
import RoleBadge from '@/components/projects/RoleBadge';
import { ProjectProvider } from '@/contexts/ProjectContext';
import { MilestoneTimeline } from '@/components/milestones/MilestoneTimeline';
import { MilestonesList } from '@/components/milestones/MilestonesList';
import { MilestoneStatistics } from '@/components/milestones/MilestoneStatistics';
import { NextMilestone } from '@/components/milestones/NextMilestone';
import { CreateMilestoneDialog } from '@/components/milestones/CreateMilestoneDialog';
import { EditMilestoneDialog } from '@/components/milestones/EditMilestoneDialog';
import { CompleteMilestoneDialog } from '@/components/milestones/CompleteMilestoneDialog';
import { ReleasesList } from '@/components/releases/ReleasesList';
import { CreateReleaseDialog } from '@/components/releases/CreateReleaseDialog';
import { NextReleaseWidget } from '@/components/releases/NextReleaseWidget';
import type { MilestoneDto } from '@/types/milestones';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';

export default function ProjectDetail() {
  const { id } = useParams<{ id: string }>();
  const projectId = parseInt(id || '0');
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState('overview');
  const [isAIDialogOpen, setIsAIDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [isCreateMilestoneDialogOpen, setIsCreateMilestoneDialogOpen] = useState(false);
  const [editingMilestone, setEditingMilestone] = useState<MilestoneDto | null>(null);
  const [completingMilestone, setCompletingMilestone] = useState<MilestoneDto | null>(null);
  const [milestoneView, setMilestoneView] = useState<'timeline' | 'list'>('timeline');
  const [isCreateReleaseOpen, setIsCreateReleaseOpen] = useState(false);

  const { data: project, isLoading: projectLoading } = useQuery({
    queryKey: ['project', projectId],
    queryFn: () => projectsApi.getById(projectId),
    enabled: !!projectId,
  });

  const { data: sprintsData, isLoading: sprintsLoading } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId),
    enabled: !!projectId,
  });

  const { data: tasksData, isLoading: tasksLoading } = useQuery({
    queryKey: ['tasks', projectId],
    queryFn: () => tasksApi.getByProject(projectId),
    enabled: !!projectId,
  });

  const { data: userRole } = useQuery({
    queryKey: ['user-role', projectId],
    queryFn: () => memberService.getUserRole(projectId),
    enabled: !!projectId,
  });

  const permissions = useProjectPermissions(projectId);

  const archiveMutation = useMutation({
    mutationFn: () => projectsApi.archive(projectId),
    onSuccess: () => {
      showSuccess("Project archived");
      navigate('/projects');
    },
    onError: () => {
      showError('Failed to archive project');
    },
  });

  if (projectLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-96" />
        <div className="grid gap-4 md:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-24" />
          ))}
        </div>
      </div>
    );
  }

  if (!project) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <h2 className="text-xl font-semibold">Project not found</h2>
        <Button variant="link" onClick={() => navigate('/projects')}>
          Back to projects
        </Button>
      </div>
    );
  }

  const activeSprint = sprintsData?.sprints?.find((s) => s.status === 'Active');
  const taskStats = {
    total: tasksData?.tasks?.length || 0,
    done: tasksData?.tasks?.filter((t) => t.status === 'Done').length || 0,
    blocked: tasksData?.tasks?.filter((t) => t.status === 'Blocked').length || 0,
    inProgress: tasksData?.tasks?.filter((t) => t.status === 'InProgress').length || 0,
  };

  return (
    <ProjectProvider projectId={projectId} userRole={userRole || null}>
      <div className="space-y-6">
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink asChild>
              <Link to="/projects">Projects</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbPage>{project.name}</BreadcrumbPage>
          </BreadcrumbItem>
          {userRole && (
            <>
              <BreadcrumbSeparator />
              <BreadcrumbItem>
                <RoleBadge role={userRole} />
              </BreadcrumbItem>
            </>
          )}
        </BreadcrumbList>
      </Breadcrumb>
      <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/projects')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-3 flex-wrap">
            <h1 className="text-2xl font-bold text-foreground break-words">{project.name}</h1>
            <Badge variant={project.status === 'Active' ? 'default' : 'secondary'}>
              {project.status}
            </Badge>
            {userRole && <RoleBadge role={userRole} />}
          </div>
          {project.description && (
            <p className="text-muted-foreground mt-1 break-words">{project.description}</p>
          )}
          <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground flex-wrap">
            {project.ownerName && (
              <span>Owner: {project.ownerName}</span>
            )}
            {project.createdAt && (
              <span>Created: {new Date(project.createdAt).toLocaleDateString()}</span>
            )}
            {project.startDate && (
              <span>Start: {new Date(project.startDate).toLocaleDateString()}</span>
            )}
            {project.endDate && (
              <span>End: {new Date(project.endDate).toLocaleDateString()}</span>
            )}
          </div>
        </div>
        {(permissions.canEditProject || permissions.canDeleteProject) && (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" className="w-full sm:w-auto">
                <Settings className="mr-2 h-4 w-4" />
                Settings
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {permissions.canEditProject && (
                <DropdownMenuItem onClick={() => setIsEditDialogOpen(true)}>
                  <Pencil className="mr-2 h-4 w-4" />
                  Edit project
                </DropdownMenuItem>
              )}
              {permissions.canEditProject && permissions.canDeleteProject && (
                <DropdownMenuSeparator />
              )}
              {permissions.canDeleteProject && (
                <DropdownMenuItem 
                  className="text-destructive"
                  onClick={() => setIsDeleteDialogOpen(true)}
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete project
                </DropdownMenuItem>
              )}
            <DropdownMenuSeparator />
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <DropdownMenuItem
                  className="text-destructive"
                  onSelect={(e) => e.preventDefault()}
                >
                  Archive project
                </DropdownMenuItem>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Archive this project?</AlertDialogTitle>
                  <AlertDialogDescription>
                    This action will archive the project. You can restore it later if needed.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={() => archiveMutation.mutate()}
                    className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                  >
                    Archive
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </DropdownMenuContent>
        </DropdownMenu>
        )}
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Project Type</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{project.type}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Sprint Duration</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{project.sprintDurationDays} days</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Total Tasks</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{taskStats.total}</div>
            <p className="text-xs text-muted-foreground">
              {taskStats.done} done • {taskStats.inProgress} in progress
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Active Sprint</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {activeSprint ? activeSprint.name : 'None'}
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="overflow-x-auto">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="timeline">Timeline</TabsTrigger>
          {permissions.canViewMilestones && (
            <TabsTrigger value="milestones">Milestones</TabsTrigger>
          )}
          <TabsTrigger value="releases">
            <Package className="h-4 w-4 mr-2" />
            Releases
          </TabsTrigger>
          <TabsTrigger value="members">Members</TabsTrigger>
          <TabsTrigger value="sprints">Sprints</TabsTrigger>
          <TabsTrigger value="tasks">Tasks</TabsTrigger>
          {permissions.canEditProject && (
            <TabsTrigger value="settings">Settings</TabsTrigger>
          )}
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <div className="md:col-span-2">
              <Card>
                <CardHeader>
                  <CardTitle>Project Summary</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <div>
                      <h4 className="font-medium mb-2">Task Progress</h4>
                      <div className="space-y-2">
                        <div className="flex justify-between text-sm">
                          <span>Completed</span>
                          <span>{taskStats.done}/{taskStats.total}</span>
                        </div>
                        <div className="h-2 bg-muted rounded-full overflow-hidden">
                          <div
                            className="h-full bg-primary transition-all"
                            style={{
                              width: `${taskStats.total > 0 ? (taskStats.done / taskStats.total) * 100 : 0}%`,
                            }}
                          />
                        </div>
                      </div>
                    </div>
                    <div>
                      <h4 className="font-medium mb-2">Sprint Status</h4>
                      <div className="flex items-center gap-2">
                        {activeSprint ? (
                          <>
                            <Play className="h-4 w-4 text-green-500" />
                            <span>{activeSprint.name} is active</span>
                          </>
                        ) : (
                          <>
                            <CheckCircle2 className="h-4 w-4 text-muted-foreground" />
                            <span className="text-muted-foreground">No active sprint</span>
                          </>
                        )}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
            <div className="space-y-4">
              {permissions.canViewMilestones && (
                <NextMilestone
                  projectId={projectId}
                  onCreateMilestone={() => setIsCreateMilestoneDialogOpen(true)}
                  onComplete={(milestoneId) => {
                    // Fetch milestone and open complete dialog
                    queryClient.fetchQuery({
                      queryKey: ['milestone', milestoneId],
                      queryFn: () => milestonesApi.getMilestone(milestoneId),
                    }).then((milestone) => {
                      setCompletingMilestone(milestone);
                    });
                  }}
                  canCreate={permissions.canCreateMilestone}
                  canComplete={permissions.canCompleteMilestone}
                />
              )}
              <NextReleaseWidget projectId={projectId} />
            </div>
          </div>
          {permissions.canViewMilestones && (
            <MilestoneStatistics projectId={projectId} />
          )}
        </TabsContent>

        <TabsContent value="timeline" className="space-y-4">
          <ProjectTimeline
            sprints={sprintsData?.sprints || []}
            projectStartDate={project.startDate}
            projectEndDate={project.endDate}
          />
        </TabsContent>

        {permissions.canViewMilestones && (
          <TabsContent value="milestones" className="space-y-4">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <Button
                  variant={milestoneView === 'timeline' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setMilestoneView('timeline')}
                >
                  Timeline
                </Button>
                <Button
                  variant={milestoneView === 'list' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setMilestoneView('list')}
                >
                  List
                </Button>
              </div>
            </div>
            {milestoneView === 'timeline' ? (
              <MilestoneTimeline
                projectId={projectId}
                onMilestoneClick={(milestone) => {
                  // Could open a detail dialog or navigate
                  setEditingMilestone(milestone);
                }}
              />
            ) : (
              <MilestonesList projectId={projectId} />
            )}
            <MilestoneStatistics projectId={projectId} />
          </TabsContent>
        )}

        <TabsContent value="releases" className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-2xl font-bold">Releases</h2>
              <p className="text-muted-foreground">
                Manage project releases and deployments
              </p>
            </div>
            <Button onClick={() => setIsCreateReleaseOpen(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Create Release
            </Button>
          </div>

          <ReleasesList projectId={project.id} />

          <CreateReleaseDialog
            projectId={project.id}
            open={isCreateReleaseOpen}
            onOpenChange={setIsCreateReleaseOpen}
            onSuccess={() => {
              queryClient.invalidateQueries({
                queryKey: ['project-releases', project.id],
              });
            }}
          />
        </TabsContent>

        <TabsContent value="members" className="space-y-4">
          <TeamMembersList projectId={projectId} ownerId={project.ownerId} />
        </TabsContent>

        <TabsContent value="sprints" className="space-y-4">
          {sprintsLoading ? (
            <div className="space-y-4">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-24" />
              ))}
            </div>
          ) : sprintsData?.sprints?.length === 0 ? (
            <Card className="py-8 text-center">
              <p className="text-muted-foreground">No sprints yet</p>
              <Button className="mt-4" onClick={() => navigate('/sprints')}>
                Create Sprint
              </Button>
            </Card>
          ) : (
            <div className="space-y-4">
              {sprintsData?.sprints?.map((sprint) => (
                <div key={sprint.id} className="space-y-4">
                  <Card>
                    <CardHeader className="flex flex-row items-center justify-between">
                      <div>
                        <CardTitle className="text-lg">{sprint.name}</CardTitle>
                        <CardDescription>
                          {new Date(sprint.startDate).toLocaleDateString()} - {new Date(sprint.endDate).toLocaleDateString()}
                        </CardDescription>
                      </div>
                      <Badge variant={sprint.status === 'Active' ? 'default' : 'secondary'}>
                        {sprint.status}
                      </Badge>
                    </CardHeader>
                    <CardContent>
                      <div className="flex items-center gap-4 text-sm text-muted-foreground">
                        <span>Capacity: {sprint.capacity}</span>
                        {sprint.goal && <span>Goal: {sprint.goal}</span>}
                      </div>
                    </CardContent>
                  </Card>
                  {sprint.status === 'Planned' && (
                    <SprintPlanningAssistant sprintId={sprint.id} />
                  )}
                </div>
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="tasks" className="space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold">Tasks</h3>
            <div className="flex items-center gap-2">
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    onClick={() => setIsAIDialogOpen(true)}
                    variant="secondary"
                    className="bg-cyan-500/10 hover:bg-cyan-500/20 text-cyan-700 dark:text-cyan-400 border-cyan-500/20"
                  >
                    <Sparkles className="mr-2 h-4 w-4" />
                    AI Create Task
                  </Button>
                </TooltipTrigger>
                <TooltipContent>
                  <p>Create task with AI assistance</p>
                </TooltipContent>
              </Tooltip>
              <Button
                variant="outline"
                onClick={() => navigate('/tasks')}
              >
                <Plus className="mr-2 h-4 w-4" />
                Create Task
              </Button>
            </div>
          </div>
          {tasksLoading ? (
            <div className="space-y-4">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-16" />
              ))}
            </div>
          ) : tasksData?.tasks?.length === 0 ? (
            <Card className="py-8 text-center">
              <p className="text-muted-foreground">No tasks yet</p>
              <div className="flex gap-2 justify-center mt-4">
                <Button variant="outline" onClick={() => navigate('/tasks')}>
                  Create Task
                </Button>
                <Button onClick={() => setIsAIDialogOpen(true)}>
                  <Sparkles className="mr-2 h-4 w-4" />
                  AI Create Task
                </Button>
              </div>
            </Card>
          ) : (
            <div className="space-y-2">
              {tasksData?.tasks?.slice(0, 10).map((task) => (
                <Card key={task.id} className="p-4">
                  <div className="flex items-center justify-between">
                    <div className="space-y-1">
                      <p className="font-medium">{task.title}</p>
                      <p className="text-sm text-muted-foreground line-clamp-1">
                        {task.description}
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline">{task.priority}</Badge>
                      <Badge variant={task.status === 'Done' ? 'default' : 'secondary'}>
                        {task.status}
                      </Badge>
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>

        {permissions.canEditProject && (
          <TabsContent value="settings" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Project Settings</CardTitle>
                <CardDescription>Manage project details and configuration</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">Project Information</p>
                      <p className="text-sm text-muted-foreground">Edit project name, description, and settings</p>
                    </div>
                    <Button onClick={() => setIsEditDialogOpen(true)}>
                      <Pencil className="mr-2 h-4 w-4" />
                      Edit Project
                    </Button>
                  </div>
                  <div className="grid gap-4 md:grid-cols-2 pt-4 border-t">
                    <div>
                      <p className="text-sm font-medium mb-1">Project Type</p>
                      <p className="text-sm text-muted-foreground">{project.type}</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium mb-1">Sprint Duration</p>
                      <p className="text-sm text-muted-foreground">{project.sprintDurationDays} days</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium mb-1">Status</p>
                      <Badge variant={project.status === 'Active' ? 'default' : 'secondary'}>
                        {project.status}
                      </Badge>
                    </div>
                    {project.ownerName && (
                      <div>
                        <p className="text-sm font-medium mb-1">Owner</p>
                        <p className="text-sm text-muted-foreground">{project.ownerName}</p>
                      </div>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        )}
      </Tabs>

      <div className="space-y-4">
        <ProjectInsightPanel projectId={projectId} />
        <RiskDetectionPanel projectId={projectId} />
      </div>

      <AITaskImproverDialog
        open={isAIDialogOpen}
        onOpenChange={setIsAIDialogOpen}
        projectId={projectId}
        onTaskCreated={() => {
          queryClient.invalidateQueries({ queryKey: ['tasks', projectId] });
        }}
      />

      {project && (
        <>
          <EditProjectDialog
            open={isEditDialogOpen}
            onOpenChange={setIsEditDialogOpen}
            project={project}
          />
          <DeleteProjectDialog
            open={isDeleteDialogOpen}
            onOpenChange={setIsDeleteDialogOpen}
            project={project}
            onDeleted={() => navigate('/projects')}
          />
        </>
      )}

      {/* Milestone Dialogs */}
      <CreateMilestoneDialog
        projectId={projectId}
        open={isCreateMilestoneDialogOpen}
        onOpenChange={setIsCreateMilestoneDialogOpen}
      />

      {editingMilestone && (
        <EditMilestoneDialog
          milestone={editingMilestone}
          open={!!editingMilestone}
          onOpenChange={(open) => !open && setEditingMilestone(null)}
        />
      )}

      {completingMilestone && (
        <CompleteMilestoneDialog
          milestone={completingMilestone}
          open={!!completingMilestone}
          onOpenChange={(open) => !open && setCompletingMilestone(null)}
        />
      )}
      </div>
    </ProjectProvider>
  );
}
