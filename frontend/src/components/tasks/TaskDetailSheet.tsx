import { useState, useEffect, useRef, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Checkbox } from '@/components/ui/checkbox';
import { Separator } from '@/components/ui/separator';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { showToast, showSuccess, showError, showWarning } from "@/lib/sweetalert";
import { useAuth } from '@/contexts/AuthContext';
import { tasksApi } from '@/api/tasks';
import { sprintsApi } from '@/api/sprints';
import { useProjectPermissions } from '@/hooks/useProjectPermissions';
import { useTaskDependencies } from '@/hooks/useTaskDependencies';
import { apiClient } from '@/api/client';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import BlockedBadge from './BlockedBadge';
import {
  Plus,
  Trash2,
  Upload,
  Download,
  Image as ImageIcon,
  FileText,
  MessageSquare,
  Loader2,
  AlertCircle,
} from 'lucide-react';
import type { Task, TaskStatus, TaskPriority, UpdateTaskRequest } from '@/types';
import { formatDistanceToNow } from 'date-fns';

interface TaskDetailSheetProps {
  task: Task | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  projectId: number;
  onTaskUpdated?: () => void;
  onTaskDeleted?: () => void;
}

export function TaskDetailSheet({
  task,
  open,
  onOpenChange,
  projectId,
  onTaskUpdated,
  onTaskDeleted,
}: TaskDetailSheetProps) {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const permissions = useProjectPermissions(projectId);
  const [isSaving, setIsSaving] = useState(false);
  const saveTimeoutRef = useRef<NodeJS.Timeout>();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Local state for editable fields
  const [localTask, setLocalTask] = useState<Task | null>(null);
  const [acceptanceCriteria, setAcceptanceCriteria] = useState<string[]>([]);
  const [newCriterion, setNewCriterion] = useState('');
  const [newComment, setNewComment] = useState('');

  // Load full task data when opened
  const { data: fullTask } = useQuery({
    queryKey: ['task', task?.id],
    queryFn: () => tasksApi.getById(task!.id),
    enabled: !!task?.id && open,
  });

  // Load related data
  const { data: sprintsData } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId),
    enabled: open && !!projectId,
  });

  const { data: commentsData } = useQuery({
    queryKey: ['task-comments', task?.id],
    queryFn: () => tasksApi.getComments(task!.id),
    enabled: !!task?.id && open,
  });

  const { data: attachmentsData } = useQuery({
    queryKey: ['task-attachments', task?.id],
    queryFn: () => tasksApi.getAttachments(task!.id),
    enabled: !!task?.id && open,
  });

  const { data: activityData } = useQuery({
    queryKey: ['task-activity', task?.id],
    queryFn: () => tasksApi.getActivity(task!.id),
    enabled: !!task?.id && open,
  });

  // Initialize local state when task loads
  useEffect(() => {
    if (fullTask) {
      setLocalTask(fullTask);
      // Parse acceptance criteria from description or use empty array
      // In a real app, this would come from a separate field
      setAcceptanceCriteria([]);
    } else if (task) {
      setLocalTask(task);
      setAcceptanceCriteria([]);
    }
  }, [fullTask, task]);

  // Auto-save debounced function
  const debouncedSave = useCallback(
    (updates: UpdateTaskRequest) => {
      if (!localTask) return;

      if (saveTimeoutRef.current) {
        clearTimeout(saveTimeoutRef.current);
      }

      saveTimeoutRef.current = setTimeout(async () => {
        setIsSaving(true);
        try {
          await tasksApi.update(localTask.id, updates);
          queryClient.invalidateQueries({ queryKey: ['task', localTask.id] });
          queryClient.invalidateQueries({ queryKey: ['tasks', projectId] });
          onTaskUpdated?.();
        } catch (error) {
          showError('Failed to save');
        } finally {
          setIsSaving(false);
        }
      }, 1000);
    },
    [localTask, queryClient, projectId, onTaskUpdated]
  );

  // Update handlers with auto-save
  const handleFieldChange = <K extends keyof UpdateTaskRequest>(
    field: K,
    value: UpdateTaskRequest[K]
  ) => {
    if (!localTask) return;
    setLocalTask({ ...localTask, [field]: value });
    debouncedSave({ [field]: value });
  };

  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!localTask) throw new Error('No task selected');
      return apiClient.delete(`/api/Tasks/${localTask.id}`);
    },
    onSuccess: () => {
      showSuccess("Task deleted");
      queryClient.invalidateQueries({ queryKey: ['tasks', projectId] });
      onOpenChange(false);
      onTaskDeleted?.();
    },
    onError: (error) => {
      showError('Failed to delete task');
    },
  });

  const addCommentMutation = useMutation({
    mutationFn: (content: string) => tasksApi.addComment(localTask!.id, content),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task-comments', localTask!.id] });
      setNewComment('');
      showSuccess("Comment added");
    },
    onError: (error) => {
      showError('Failed to add comment');
    },
  });

  const uploadAttachmentMutation = useMutation({
    mutationFn: (file: File) => tasksApi.uploadAttachment(localTask!.id, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['task-attachments', localTask!.id] });
      showSuccess("File uploaded");
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    },
    onError: (error) => {
      showError('Upload failed');
    },
  });

  const handleAddCriterion = () => {
    if (!newCriterion.trim()) return;
    const updated = [...acceptanceCriteria, newCriterion.trim()];
    setAcceptanceCriteria(updated);
    setNewCriterion('');
    // Save acceptance criteria (would need backend support)
    debouncedSave({ acceptanceCriteria: updated });
  };

  const handleRemoveCriterion = (index: number) => {
    const updated = acceptanceCriteria.filter((_, i) => i !== index);
    setAcceptanceCriteria(updated);
    debouncedSave({ acceptanceCriteria: updated });
  };

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file && localTask) {
      uploadAttachmentMutation.mutate(file);
    }
  };

  const handleAddComment = () => {
    if (!newComment.trim() || !localTask) return;
    addCommentMutation.mutate(newComment.trim());
  };

  // Check if task is blocked by dependencies (must be called before any early returns)
  const { isBlocked, blockedByCount, blockingTasks } = useTaskDependencies(
    localTask?.id ?? 0,
    // Build task map from fullTask if available
    fullTask
      ? new Map([[fullTask.id, { status: fullTask.status, title: fullTask.title }]])
      : undefined
  );

  if (!localTask) return null;

  const comments = commentsData?.comments || [];
  const attachments = attachmentsData?.attachments || [];
  const activities = activityData?.activities || [];

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="w-full sm:max-w-4xl overflow-y-auto">
        <SheetHeader className="sticky top-0 bg-background z-10 pb-4 border-b">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <SheetTitle className="text-lg font-semibold">
                  #{localTask.id}
                </SheetTitle>
                {isSaving && (
                  <Badge variant="outline" className="text-xs">
                    <Loader2 className="h-3 w-3 mr-1 animate-spin" />
                    Saving...
                  </Badge>
                )}
                {isBlocked && (
                  <BlockedBadge
                    blockedByCount={blockedByCount}
                    blockingTasks={blockingTasks}
                    variant="md"
                  />
                )}
              </div>
              <Input
                value={localTask.title}
                onChange={(e) => handleFieldChange('title', e.target.value)}
                className="text-xl font-bold border-none p-0 h-auto focus-visible:ring-0"
                placeholder="Task title"
                disabled={permissions.isViewer}
              />
            </div>
          </div>
        </SheetHeader>

        <div className="grid grid-cols-1 lg:grid-cols-[1fr_300px] gap-6 mt-6">
          {/* Blocked Alert */}
          {isBlocked && (
            <div className="lg:col-span-2">
              <Alert variant="destructive" className="mb-4">
                <AlertCircle className="h-4 w-4" />
                <AlertTitle>This task is blocked</AlertTitle>
                <AlertDescription>
                  This task cannot start until {blockedByCount}{' '}
                  {blockedByCount === 1 ? 'dependency is' : 'dependencies are'} completed.
                  {blockingTasks.length > 0 && (
                    <div className="mt-2">
                      <p className="font-semibold text-sm mb-1">Blocking tasks:</p>
                      <ul className="list-disc list-inside text-sm space-y-1">
                        {blockingTasks.map((task) => (
                          <li key={task.taskId}>
                            {task.title} <span className="text-muted-foreground">({task.status})</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                </AlertDescription>
              </Alert>
            </div>
          )}
          {/* Left Panel (70%) */}
          <div className="space-y-6">
            {/* Description */}
            <div className="space-y-2">
              <Label>Description</Label>
              <Textarea
                value={localTask.description}
                onChange={(e) => handleFieldChange('description', e.target.value)}
                rows={6}
                placeholder="Describe the task..."
                className="resize-none"
                disabled={permissions.isViewer}
              />
            </div>

            {/* Acceptance Criteria */}
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label>Acceptance Criteria</Label>
                {!permissions.isViewer && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={handleAddCriterion}
                  >
                    <Plus className="h-4 w-4 mr-1" />
                    Add
                  </Button>
                )}
              </div>
              <div className="space-y-2">
                {acceptanceCriteria.map((criterion, index) => (
                  <div key={index} className="flex items-center gap-2">
                    <Checkbox checked={false} disabled />
                    <Input
                      value={criterion}
                      onChange={(e) => {
                        const updated = [...acceptanceCriteria];
                        updated[index] = e.target.value;
                        setAcceptanceCriteria(updated);
                        debouncedSave({ acceptanceCriteria: updated });
                      }}
                      placeholder="Criterion"
                      disabled={permissions.isViewer}
                    />
                    {!permissions.isViewer && (
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        onClick={() => handleRemoveCriterion(index)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                ))}
                <div className="flex items-center gap-2">
                  <Input
                    value={newCriterion}
                    onChange={(e) => setNewCriterion(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        e.preventDefault();
                        handleAddCriterion();
                      }
                    }}
                    placeholder="Add acceptance criterion..."
                    disabled={permissions.isViewer}
                  />
                </div>
              </div>
            </div>

            <Separator />

            {/* Attachments */}
            <div className="space-y-2">
              <Label>Attachments</Label>
              <div
                className="border-2 border-dashed rounded-lg p-6 text-center cursor-pointer hover:bg-muted/50 transition-colors"
                onClick={() => fileInputRef.current?.click()}
              >
                <Upload className="h-8 w-8 mx-auto mb-2 text-muted-foreground" />
                <p className="text-sm text-muted-foreground">
                  Drag & drop files here or click to upload
                </p>
                <input
                  ref={fileInputRef}
                  type="file"
                  className="hidden"
                  onChange={handleFileUpload}
                />
              </div>
              {attachments.length > 0 && (
                <div className="space-y-2 mt-4">
                  {attachments.map((attachment) => (
                    <div
                      key={attachment.id}
                      className="flex items-center gap-3 p-2 rounded-lg border hover:bg-muted/50"
                    >
                      {attachment.fileType.startsWith('image/') ? (
                        <ImageIcon className="h-5 w-5 text-muted-foreground" />
                      ) : (
                        <FileText className="h-5 w-5 text-muted-foreground" />
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">{attachment.fileName}</p>
                        <p className="text-xs text-muted-foreground">
                          {(attachment.fileSize / 1024).toFixed(1)} KB
                        </p>
                      </div>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => window.open(attachment.fileUrl, '_blank')}
                      >
                        <Download className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <Separator />

            {/* Activity Log */}
            <div className="space-y-2">
              <Label>Activity</Label>
              <div className="space-y-3">
                {activities.length > 0 ? (
                  activities.map((activity) => (
                    <div key={activity.id} className="flex gap-3 text-sm">
                      <Avatar className="h-6 w-6">
                        <AvatarFallback className="text-xs">
                          {activity.userName[0].toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1">
                        <p>
                          <span className="font-medium">{activity.userName}</span>{' '}
                          {activity.action}
                          {activity.field && (
                            <>
                              {' '}
                              <span className="text-muted-foreground">{activity.field}</span>
                              {activity.oldValue && activity.newValue && (
                                <>
                                  {' '}
                                  from <span className="font-mono text-xs">{activity.oldValue}</span> to{' '}
                                  <span className="font-mono text-xs">{activity.newValue}</span>
                                </>
                              )}
                            </>
                          )}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {formatDistanceToNow(new Date(activity.createdAt), { addSuffix: true })}
                        </p>
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="text-sm text-muted-foreground italic">No activity yet</p>
                )}
              </div>
            </div>

            <Separator />

            {/* Comments */}
            <div className="space-y-2">
              <Label>Comments</Label>
              <div className="space-y-4">
                {comments.map((comment) => (
                  <div key={comment.id} className="flex gap-3">
                    <Avatar className="h-8 w-8">
                      <AvatarFallback>
                        {comment.userName[0].toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 space-y-1">
                      <div className="flex items-center gap-2">
                        <span className="text-sm font-medium">{comment.userName}</span>
                        <span className="text-xs text-muted-foreground">
                          {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
                        </span>
                      </div>
                      <p className="text-sm">{comment.content}</p>
                    </div>
                  </div>
                ))}
                <div className="flex gap-2">
                  <Avatar className="h-8 w-8">
                    <AvatarFallback>
                      {user?.firstName?.[0] || user?.username[0] || 'U'}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1 space-y-2">
                    <Textarea
                      value={newComment}
                      onChange={(e) => setNewComment(e.target.value)}
                      placeholder="Add a comment..."
                      rows={2}
                    />
                    <Button
                      size="sm"
                      onClick={handleAddComment}
                      disabled={!newComment.trim() || addCommentMutation.isPending}
                    >
                      {addCommentMutation.isPending ? (
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      ) : (
                        <MessageSquare className="h-4 w-4 mr-2" />
                      )}
                      Comment
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Right Panel (30%) */}
          <div className="space-y-4 lg:sticky lg:top-20 lg:self-start">
            {/* Status */}
            <div className="space-y-2">
              <Label>Status</Label>
              <Select
                value={localTask.status}
                onValueChange={(value: TaskStatus) => {
                  if (localTask && !permissions.isViewer) {
                    tasksApi.changeStatus(localTask.id, value).then(() => {
                      queryClient.invalidateQueries({ queryKey: ['task', localTask.id] });
                      queryClient.invalidateQueries({ queryKey: ['tasks', projectId] });
                      onTaskUpdated?.();
                    }).catch((error) => {
                      showError('Failed to update status');
                    });
                  }
                }}
                disabled={permissions.isViewer}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Todo">Todo</SelectItem>
                  <SelectItem value="InProgress">In Progress</SelectItem>
                  <SelectItem value="Blocked">Blocked</SelectItem>
                  <SelectItem value="Done">Done</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Priority */}
            <div className="space-y-2">
              <Label>Priority</Label>
              <Select
                value={localTask.priority}
                onValueChange={(value: TaskPriority) => handleFieldChange('priority', value)}
                disabled={permissions.isViewer}
              >
                <SelectTrigger>
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

            {/* Assignee */}
            <div className="space-y-2">
              <Label>Assignee</Label>
              <Select
                value={localTask.assigneeId?.toString() || 'unassigned'}
                onValueChange={(value) =>
                  handleFieldChange('assigneeId', value === 'unassigned' ? undefined : parseInt(value))
                }
                disabled={permissions.isViewer}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Unassigned" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="unassigned">Unassigned</SelectItem>
                  {/* In a real app, load users from API */}
                </SelectContent>
              </Select>
            </div>

            {/* Story Points */}
            <div className="space-y-2">
              <Label>Story Points</Label>
              <Input
                type="number"
                min={0}
                max={100}
                value={localTask.storyPoints || ''}
                onChange={(e) =>
                  handleFieldChange('storyPoints', e.target.value ? parseInt(e.target.value) : undefined)
                }
                placeholder="0"
                disabled={permissions.isViewer}
              />
            </div>

            {/* Sprint */}
            <div className="space-y-2">
              <Label>Sprint</Label>
              <Select
                value={localTask.sprintId?.toString() || 'backlog'}
                onValueChange={(value) =>
                  handleFieldChange('sprintId', value === 'backlog' ? undefined : parseInt(value))
                }
                disabled={permissions.isViewer}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="backlog">Backlog</SelectItem>
                  {sprintsData?.sprints?.map((sprint) => (
                    <SelectItem key={sprint.id} value={sprint.id.toString()}>
                      {sprint.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Due Date */}
            <div className="space-y-2">
              <Label>Due Date</Label>
              <Input
                type="date"
                value={
                  localTask.dueDate
                    ? new Date(localTask.dueDate).toISOString().split('T')[0]
                    : ''
                }
                onChange={(e) =>
                  handleFieldChange('dueDate', e.target.value ? e.target.value : undefined)
                }
                disabled={permissions.isViewer}
              />
            </div>

            {/* Type */}
            <div className="space-y-2">
              <Label>Type</Label>
              <Select
                value={localTask.type || 'Task'}
                onValueChange={(value: 'Bug' | 'Feature' | 'Task') =>
                  handleFieldChange('type', value)
                }
                disabled={permissions.isViewer}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Bug">Bug</SelectItem>
                  <SelectItem value="Feature">Feature</SelectItem>
                  <SelectItem value="Task">Task</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <Separator />

            {/* Metadata (Read-only) */}
            <div className="space-y-3 text-sm">
              <div>
                <Label className="text-muted-foreground">Created</Label>
                <p className="text-xs text-muted-foreground">
                  {formatDistanceToNow(new Date(localTask.createdAt), { addSuffix: true })}
                </p>
              </div>
              {localTask.updatedAt && (
                <div>
                  <Label className="text-muted-foreground">Updated</Label>
                  <p className="text-xs text-muted-foreground">
                    {formatDistanceToNow(new Date(localTask.updatedAt), { addSuffix: true })}
                  </p>
                </div>
              )}
            </div>

            <Separator />

            {/* Delete Button */}
            {permissions.canDeleteTasks && (
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button variant="destructive" className="w-full">
                    <Trash2 className="h-4 w-4 mr-2" />
                    Delete Task
                  </Button>
                </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Delete Task?</AlertDialogTitle>
                  <AlertDialogDescription>
                    This action cannot be undone. This will permanently delete the task.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction
                    onClick={() => deleteMutation.mutate()}
                    className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                  >
                    Delete
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
            )}
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}
