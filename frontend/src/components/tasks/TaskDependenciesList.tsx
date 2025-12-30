import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Separator } from '@/components/ui/separator';
import { Trash2, Plus, AlertCircle } from 'lucide-react';
import { dependenciesApi } from '@/api/dependencies';
import type { TaskDependencyDto } from '@/types/dependencies';
import { useAuth } from '@/contexts/AuthContext';
import { MySwal } from '@/lib/sweetalert';
import { AddDependencyDialog } from './AddDependencyDialog';
import { cn } from '@/lib/utils';

interface TaskDependenciesListProps {
  taskId: number;
  projectId: number;
}

/**
 * Component for displaying and managing task dependencies.
 * Shows dependencies where the task is either the source or dependent task.
 */
export function TaskDependenciesList({ taskId, projectId }: TaskDependenciesListProps) {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);

  // Check if user has permission to delete dependencies
  const canDelete = user?.permissions?.includes('tasks.dependencies.delete') ?? false;
  const canCreate = user?.permissions?.includes('tasks.dependencies.create') ?? false;

  // Fetch dependencies
  const { data: dependencies, isLoading, error, refetch } = useQuery({
    queryKey: ['taskDependencies', taskId],
    queryFn: () => dependenciesApi.getTaskDependencies(taskId),
    enabled: !!taskId,
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (dependencyId: number) => dependenciesApi.removeTaskDependency(dependencyId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskDependencies', taskId] });
      queryClient.invalidateQueries({ queryKey: ['projectDependencyGraph', projectId] });
      queryClient.invalidateQueries({ queryKey: ['task', taskId] });
    },
  });

  const handleDelete = async (dependencyId: number, sourceTitle: string, dependentTitle: string) => {
    const result = await MySwal.fire({
      title: 'Remove dependency?',
      text: `This will remove the dependency between "${sourceTitle}" and "${dependentTitle}"`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Remove',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#ef4444',
    });

    if (result.isConfirmed) {
      try {
        await deleteMutation.mutateAsync(dependencyId);
        MySwal.fire({
          icon: 'success',
          title: 'Dependency removed',
          timer: 2000,
          showConfirmButton: false,
        });
      } catch (error) {
        MySwal.fire({
          icon: 'error',
          title: 'Failed to remove dependency',
          text: error instanceof Error ? error.message : 'Unknown error',
        });
      }
    }
  };

  // Group dependencies
  const outgoingDeps = dependencies?.filter((d) => d.sourceTaskId === taskId) ?? [];
  const incomingDeps = dependencies?.filter((d) => d.dependentTaskId === taskId) ?? [];

  // Get dependency type badge color
  const getDependencyTypeColor = (type: string) => {
    switch (type) {
      case 'FinishToStart':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200';
      case 'StartToStart':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      case 'FinishToFinish':
        return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200';
      case 'StartToFinish':
        return 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200';
    }
  };

  // Get dependency type label
  const getDependencyTypeLabel = (type: string) => {
    switch (type) {
      case 'FinishToStart':
        return 'FS';
      case 'StartToStart':
        return 'SS';
      case 'FinishToFinish':
        return 'FF';
      case 'StartToFinish':
        return 'SF';
      default:
        return type;
    }
  };

  const DependencyItem = ({ dependency, isOutgoing }: { dependency: TaskDependencyDto; isOutgoing: boolean }) => {
    const otherTaskTitle = isOutgoing ? dependency.dependentTaskTitle : dependency.sourceTaskTitle;
    const otherTaskId = isOutgoing ? dependency.dependentTaskId : dependency.sourceTaskId;

    return (
      <div className="flex items-center justify-between gap-4 p-3 rounded-lg border bg-card hover:bg-accent/50 transition-colors">
        <div className="flex-1 min-w-0">
          <button
            onClick={() => {
              // TODO: Open task detail or navigate to task
              console.log('Navigate to task', otherTaskId);
            }}
            className="text-sm font-medium text-left hover:underline truncate"
          >
            {otherTaskTitle}
          </button>
        </div>
        <Badge className={cn('text-xs', getDependencyTypeColor(dependency.dependencyType))}>
          {getDependencyTypeLabel(dependency.dependencyType)}
        </Badge>
        {canDelete && (
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive hover:text-destructive"
            onClick={() => handleDelete(dependency.id, dependency.sourceTaskTitle, dependency.dependentTaskTitle)}
            disabled={deleteMutation.isPending}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        )}
      </div>
    );
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Task Dependencies</CardTitle>
            <CardDescription>Manage dependencies between tasks</CardDescription>
          </div>
          {canCreate && (
            <Button size="sm" onClick={() => setIsAddDialogOpen(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Add Dependency
            </Button>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        {isLoading ? (
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-16 w-full" />
            ))}
          </div>
        ) : error ? (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>Error</AlertTitle>
            <AlertDescription>
              {error instanceof Error ? error.message : 'Failed to load dependencies'}
              <Button variant="outline" size="sm" className="mt-2" onClick={() => refetch()}>
                Retry
              </Button>
            </AlertDescription>
          </Alert>
        ) : dependencies && dependencies.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            <p className="text-sm">No dependencies yet</p>
            {canCreate && (
              <Button variant="outline" size="sm" className="mt-4" onClick={() => setIsAddDialogOpen(true)}>
                <Plus className="h-4 w-4 mr-2" />
                Add First Dependency
              </Button>
            )}
          </div>
        ) : (
          <>
            {/* Outgoing dependencies - tasks this task depends on */}
            {outgoingDeps.length > 0 && (
              <div className="space-y-2">
                <h4 className="text-sm font-semibold">This task depends on</h4>
                <p className="text-xs text-muted-foreground">
                  These tasks must be completed before this task can proceed
                </p>
                <div className="space-y-2">
                  {outgoingDeps.map((dep) => {
                    // Check if this dependency is blocking
                    const isBlocking = (() => {
                      switch (dep.dependencyType) {
                        case 'FinishToStart':
                          // Need to check dependent task status - for now assume blocking if not Done
                          // In a real implementation, you'd fetch the task status
                          return true; // Conservative: show as blocking
                        case 'StartToStart':
                          return true; // Conservative: show as blocking
                        case 'FinishToFinish':
                          return true; // Conservative: show as blocking
                        case 'StartToFinish':
                          return true; // Conservative: show as blocking
                        default:
                          return false;
                      }
                    })();

                    return (
                      <div key={dep.id} className={cn(isBlocking && 'border-l-4 border-l-red-500 pl-2')}>
                        <DependencyItem dependency={dep} isOutgoing={true} />
                        {isBlocking && (
                          <Badge variant="destructive" className="mt-1 text-xs">
                            ⚠️ Blocking
                          </Badge>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            )}

            {/* Incoming dependencies - tasks that depend on this task */}
            {incomingDeps.length > 0 && (
              <>
                {outgoingDeps.length > 0 && <Separator />}
                <div className="space-y-2">
                  <h4 className="text-sm font-semibold">Tasks that depend on this</h4>
                  <p className="text-xs text-muted-foreground">
                    These tasks are waiting for this task to be completed
                  </p>
                  <div className="space-y-2">
                    {incomingDeps.map((dep) => (
                      <DependencyItem key={dep.id} dependency={dep} isOutgoing={false} />
                    ))}
                  </div>
                </div>
              </>
            )}
          </>
        )}
      </CardContent>

      <AddDependencyDialog
        taskId={taskId}
        projectId={projectId}
        open={isAddDialogOpen}
        onOpenChange={setIsAddDialogOpen}
        onSuccess={() => {
          refetch();
        }}
      />
    </Card>
  );
}

