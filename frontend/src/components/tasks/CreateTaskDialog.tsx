import { useState, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tasksApi } from '@/api/tasks';
import { projectsApi } from '@/api/projects';
import { sprintsApi } from '@/api/sprints';
import type { ProjectMember } from '@/types';
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
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Calendar } from '@/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { showSuccess, showError } from "@/lib/sweetalert";
import { format } from 'date-fns';
import {
  Loader2,
  Bug,
  Sparkles,
  CheckSquare,
  FileText,
  X,
  Plus,
  ChevronDown,
  ChevronUp,
  CalendarIcon,
  Upload,
  AlertCircle,
} from 'lucide-react';
import type { CreateTaskRequest, TaskPriority } from '@/types';
import { cn } from '@/lib/utils';
import { AITaskImproverDialog, type ImprovedTask } from './AITaskImproverDialog';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { useTaskPriorities } from '@/hooks/useLookups';
import { Skeleton } from '@/components/ui/skeleton';

interface CreateTaskDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  projectId: number;
  defaultSprintId?: number;
}

const typeIcons = {
  Bug: Bug,
  Feature: Sparkles,
  Task: CheckSquare,
  Epic: FileText,
};

const tagColors = [
  'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
  'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
  'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
  'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
  'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200',
];

export function CreateTaskDialog({
  open,
  onOpenChange,
  projectId,
  defaultSprintId,
}: CreateTaskDialogProps) {
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { priorities, isLoading: isLoadingPriorities } = useTaskPriorities();

  const [formData, setFormData] = useState<CreateTaskRequest & { files: File[] }>({
    title: '',
    description: '',
    projectId,
    priority: 'Medium',
    type: 'Task',
    storyPoints: undefined,
    assigneeId: undefined,
    sprintId: defaultSprintId,
    dueDate: undefined,
    tags: [],
    acceptanceCriteria: [],
    files: [],
  });

  const [newTag, setNewTag] = useState('');
  const [newCriterion, setNewCriterion] = useState('');
  const [isCriteriaOpen, setIsCriteriaOpen] = useState(false);
  const [isAttachmentsOpen, setIsAttachmentsOpen] = useState(false);
  const [aiImproverOpen, setAiImproverOpen] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const { data: membersData } = useQuery({
    queryKey: ['project-members', projectId],
    queryFn: () => projectsApi.getMembers(projectId),
    enabled: !!projectId && open,
  });

  const { data: sprintsData } = useQuery({
    queryKey: ['sprints', projectId],
    queryFn: () => sprintsApi.getByProject(projectId),
    enabled: !!projectId && open,
  });

  const activeSprint = sprintsData?.sprints?.find((s) => s.status === 'Active');

  const createMutation = useMutation({
    mutationFn: async (data: CreateTaskRequest) => {
      const task = await tasksApi.create(data);
      
      // Upload files after task creation
      if (formData.files.length > 0) {
        const uploadPromises = formData.files.map((file) =>
          tasksApi.uploadAttachment(task.id, file).catch(() => {
            // File upload failed, continue with other files
            return null;
          })
        );
        await Promise.all(uploadPromises);
      }
      
      return task;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks', projectId] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      onOpenChange(false);
      resetForm();
      showSuccess("Task created successfully");
    },
    onError: () => {
      showError('Failed to create task');
    },
  });

  const resetForm = () => {
    setFormData({
      title: '',
      description: '',
      projectId,
      priority: 'Medium',
      type: 'Task',
      storyPoints: undefined,
      assigneeId: undefined,
      sprintId: defaultSprintId,
      dueDate: undefined,
      tags: [],
      acceptanceCriteria: [],
      files: [],
    });
    setNewTag('');
    setNewCriterion('');
    setErrors({});
    setIsCriteriaOpen(false);
    setIsAttachmentsOpen(false);
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.title.trim()) {
      newErrors.title = 'Title is required';
    } else if (formData.title.length > 200) {
      newErrors.title = 'Title must be 200 characters or less';
    }

    if (formData.dueDate) {
      const dueDate = new Date(formData.dueDate);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (dueDate < today) {
        newErrors.dueDate = 'Due date must be today or in the future';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    const { files, ...taskData } = formData;
    createMutation.mutate(taskData);
  };

  const handleAddTag = () => {
    if (newTag.trim() && !formData.tags?.includes(newTag.trim())) {
      setFormData({
        ...formData,
        tags: [...(formData.tags || []), newTag.trim()],
      });
      setNewTag('');
    }
  };

  const handleRemoveTag = (tag: string) => {
    setFormData({
      ...formData,
      tags: formData.tags?.filter((t) => t !== tag) || [],
    });
  };

  const handleAddCriterion = () => {
    if (newCriterion.trim()) {
      setFormData({
        ...formData,
        acceptanceCriteria: [...(formData.acceptanceCriteria || []), newCriterion.trim()],
      });
      setNewCriterion('');
    }
  };

  const handleRemoveCriterion = (index: number) => {
    setFormData({
      ...formData,
      acceptanceCriteria: formData.acceptanceCriteria?.filter((_, i) => i !== index) || [],
    });
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    const validFiles: File[] = [];
    const invalidFiles: string[] = [];

    files.forEach((file) => {
      if (file.size > 10 * 1024 * 1024) {
        invalidFiles.push(`${file.name} (exceeds 10MB)`);
      } else {
        validFiles.push(file);
      }
    });

    if (invalidFiles.length > 0) {
      showError('Some files were rejected');
    }

    if (validFiles.length > 0) {
      setFormData({
        ...formData,
        files: [...formData.files, ...validFiles],
      });
    }

    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleRemoveFile = (index: number) => {
    setFormData({
      ...formData,
      files: formData.files.filter((_, i) => i !== index),
    });
  };

  const handleApplyImprovedTask = (improved: ImprovedTask) => {
    setFormData({
      ...formData,
      title: improved.title,
      description: improved.description,
      acceptanceCriteria: improved.acceptanceCriteria,
      storyPoints: improved.storyPoints,
    });
    showSuccess('Améliorations appliquées avec succès');
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  const getTagColor = (index: number) => {
    return tagColors[index % tagColors.length];
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const selectedMember = membersData?.find((m: ProjectMember) => m.userId === formData.assigneeId);
  const selectedSprint = sprintsData?.sprints?.find((s) => s.id === formData.sprintId);
  const dueDateObj = formData.dueDate ? new Date(formData.dueDate) : null;
  const isDueSoon = dueDateObj && dueDateObj.getTime() - Date.now() < 7 * 24 * 60 * 60 * 1000;

  const TypeIcon = formData.type ? typeIcons[formData.type] : CheckSquare;

  return (
    <Dialog open={open} onOpenChange={(open) => {
      if (!open) resetForm();
      onOpenChange(open);
    }}>
      <DialogContent className="sm:max-w-[700px] max-h-[90vh] overflow-y-auto">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Create New Task</DialogTitle>
            <DialogDescription>Fill in the details to create a new task for this project.</DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            {/* Title */}
            <div className="space-y-2">
              <Label htmlFor="title">
                Title <span className="text-destructive">*</span>
              </Label>
              <Input
                id="title"
                placeholder="Enter task title"
                value={formData.title}
                onChange={(e) => {
                  setFormData({ ...formData, title: e.target.value });
                  if (errors.title) {
                    const newErrors = { ...errors };
                    delete newErrors.title;
                    setErrors(newErrors);
                  }
                }}
                maxLength={200}
                required
              />
              <div className="flex justify-between text-xs text-muted-foreground">
                {errors.title && <span className="text-destructive">{errors.title}</span>}
                <span>{formData.title.length}/200</span>
              </div>
            </div>

            {/* Description */}
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="description">Description</Label>
                <TooltipProvider>
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <span>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => setAiImproverOpen(true)}
                          disabled={!formData.title?.trim() || !formData.description?.trim()}
                        >
                          <Sparkles className="mr-2 h-4 w-4" />
                          Improve with AI
                        </Button>
                      </span>
                    </TooltipTrigger>
                    {(!formData.title?.trim() || !formData.description?.trim()) && (
                      <TooltipContent>
                        <p>Add a title and description to use AI improvements</p>
                      </TooltipContent>
                    )}
                  </Tooltip>
                </TooltipProvider>
              </div>
              <Textarea
                id="description"
                placeholder="Describe the task in detail"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={4}
              />
            </div>

            {/* Type and Priority */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="type">Type</Label>
                <Select
                  value={formData.type}
                  onValueChange={(value: string) => {
                    const taskType = value as 'Bug' | 'Feature' | 'Task' | 'Epic';
                    setFormData({ ...formData, type: taskType });
                  }}
                >
                  <SelectTrigger>
                    <div className="flex items-center gap-2">
                      <TypeIcon className="h-4 w-4" />
                      <SelectValue />
                    </div>
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Task">
                      <div className="flex items-center gap-2">
                        <CheckSquare className="h-4 w-4" />
                        Task
                      </div>
                    </SelectItem>
                    <SelectItem value="Bug">
                      <div className="flex items-center gap-2">
                        <Bug className="h-4 w-4" />
                        Bug
                      </div>
                    </SelectItem>
                    <SelectItem value="Feature">
                      <div className="flex items-center gap-2">
                        <Sparkles className="h-4 w-4" />
                        Feature
                      </div>
                    </SelectItem>
                    <SelectItem value="Epic">
                      <div className="flex items-center gap-2">
                        <FileText className="h-4 w-4" />
                        Epic
                      </div>
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="priority">Priority</Label>
                {isLoadingPriorities ? (
                  <Skeleton className="h-10 w-full" />
                ) : (
                  <Select
                    value={formData.priority}
                    onValueChange={(value: TaskPriority) =>
                      setFormData({ ...formData, priority: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {priorities.map((priority) => (
                        <SelectItem key={priority.value} value={priority.value}>
                          {priority.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              </div>
            </div>

            {/* Assignee and Sprint */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="assignee">Assignee</Label>
                <Select
                  value={formData.assigneeId?.toString() || 'unassigned'}
                  onValueChange={(value) =>
                    setFormData({
                      ...formData,
                      assigneeId: value === 'unassigned' ? undefined : parseInt(value),
                    })
                  }
                >
                  <SelectTrigger>
                    {selectedMember ? (
                      <div className="flex items-center gap-2">
                        <Avatar className="h-6 w-6">
                          <AvatarImage src={selectedMember.avatar || undefined} />
                          <AvatarFallback className="text-xs">
                            {getInitials(`${selectedMember.firstName} ${selectedMember.lastName}`)}
                          </AvatarFallback>
                        </Avatar>
                        <span>{selectedMember.firstName} {selectedMember.lastName}</span>
                      </div>
                    ) : (
                      <SelectValue placeholder="Unassigned" />
                    )}
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="unassigned">Unassigned</SelectItem>
                    {membersData?.map((member: ProjectMember) => (
                      <SelectItem key={member.userId} value={member.userId.toString()}>
                        <div className="flex items-center gap-2">
                          <Avatar className="h-6 w-6">
                            <AvatarImage src={member.avatar || undefined} />
                            <AvatarFallback className="text-xs">
                              {getInitials(`${member.firstName} ${member.lastName}`)}
                            </AvatarFallback>
                          </Avatar>
                          <span>{member.firstName} {member.lastName}</span>
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="sprint">Sprint</Label>
                <Select
                  value={formData.sprintId?.toString() || 'backlog'}
                  onValueChange={(value) => {
                    if (value === 'backlog') {
                      setFormData({ ...formData, sprintId: undefined });
                    } else if (value === 'current' && activeSprint) {
                      setFormData({ ...formData, sprintId: activeSprint.id });
                    } else {
                      setFormData({ ...formData, sprintId: parseInt(value) });
                    }
                  }}
                >
                  <SelectTrigger>
                    <SelectValue>
                      {selectedSprint && selectedSprint.startDate && selectedSprint.endDate
                        ? `${selectedSprint.name} (${format(new Date(selectedSprint.startDate), 'MMM d')} - ${format(new Date(selectedSprint.endDate), 'MMM d')})`
                        : 'Backlog'}
                    </SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="backlog">Backlog</SelectItem>
                    {activeSprint && activeSprint.startDate && activeSprint.endDate && (
                      <SelectItem value="current">
                        Current Sprint: {activeSprint.name} ({format(new Date(activeSprint.startDate), 'MMM d')} - {format(new Date(activeSprint.endDate), 'MMM d')})
                      </SelectItem>
                    )}
                    {sprintsData?.sprints
                      ?.filter((s) => s.status !== 'Active' && s.startDate && s.endDate)
                      .map((sprint) => (
                        <SelectItem key={sprint.id} value={sprint.id.toString()}>
                          {sprint.name} ({format(new Date(sprint.startDate), 'MMM d')} - {format(new Date(sprint.endDate), 'MMM d')})
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            {/* Story Points and Due Date */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="storyPoints">Story Points</Label>
                <Input
                  id="storyPoints"
                  type="number"
                  min={0}
                  placeholder="Optional"
                  value={formData.storyPoints || ''}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      storyPoints: e.target.value ? parseInt(e.target.value) : undefined,
                    })
                  }
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="dueDate">Due Date</Label>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className={cn(
                        'w-full justify-start text-left font-normal',
                        !dueDateObj && 'text-muted-foreground',
                        isDueSoon && dueDateObj && 'border-orange-500'
                      )}
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {dueDateObj ? format(dueDateObj, 'PPP') : <span>Pick a date</span>}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0" align="start">
                    <Calendar
                      mode="single"
                      selected={dueDateObj || undefined}
                      onSelect={(date) => {
                        if (date) {
                          setFormData({
                            ...formData,
                            dueDate: date.toISOString().split('T')[0],
                          });
                          if (errors.dueDate) {
                            const newErrors = { ...errors };
                            delete newErrors.dueDate;
                            setErrors(newErrors);
                          }
                        }
                      }}
                      disabled={(date) => date < new Date(new Date().setHours(0, 0, 0, 0))}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
                {errors.dueDate && (
                  <p className="text-xs text-destructive">{errors.dueDate}</p>
                )}
                {isDueSoon && dueDateObj && (
                  <div className="flex items-center gap-1 text-xs text-orange-600 dark:text-orange-400">
                    <AlertCircle className="h-3 w-3" />
                    Due date is soon
                  </div>
                )}
              </div>
            </div>

            {/* Tags */}
            <div className="space-y-2">
              <Label>Tags</Label>
              <div className="flex flex-wrap gap-2 mb-2">
                {formData.tags?.map((tag, index) => (
                  <Badge key={tag} variant="secondary" className={getTagColor(index)}>
                    {tag}
                    <button
                      type="button"
                      onClick={() => handleRemoveTag(tag)}
                      className="ml-1 hover:bg-black/20 rounded-full p-0.5"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
              <div className="flex gap-2">
                <Input
                  placeholder="Add tag (e.g., Frontend, Backend, Urgent)"
                  value={newTag}
                  onChange={(e) => setNewTag(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault();
                      handleAddTag();
                    }
                  }}
                />
                <Button type="button" variant="outline" size="icon" onClick={handleAddTag}>
                  <Plus className="h-4 w-4" />
                </Button>
              </div>
            </div>

            {/* Acceptance Criteria */}
            <Collapsible open={isCriteriaOpen} onOpenChange={setIsCriteriaOpen}>
              <CollapsibleTrigger asChild>
                <Button type="button" variant="ghost" className="w-full justify-between">
                  <span className="flex items-center gap-2">
                    <CheckSquare className="h-4 w-4" />
                    Acceptance Criteria ({formData.acceptanceCriteria?.length || 0})
                  </span>
                  {isCriteriaOpen ? (
                    <ChevronUp className="h-4 w-4" />
                  ) : (
                    <ChevronDown className="h-4 w-4" />
                  )}
                </Button>
              </CollapsibleTrigger>
              <CollapsibleContent className="space-y-2">
                <div className="space-y-2 p-4 border rounded-lg bg-muted/50">
                  {formData.acceptanceCriteria?.map((criterion, index) => (
                    <div key={index} className="flex items-start gap-2">
                      <div className="flex-1 text-sm">{criterion}</div>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="h-6 w-6"
                        onClick={() => handleRemoveCriterion(index)}
                      >
                        <X className="h-3 w-3" />
                      </Button>
                    </div>
                  ))}
                  <div className="flex gap-2 mt-2">
                    <Input
                      placeholder="Enter acceptance criterion"
                      value={newCriterion}
                      onChange={(e) => setNewCriterion(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                          e.preventDefault();
                          handleAddCriterion();
                        }
                      }}
                    />
                    <Button type="button" variant="outline" onClick={handleAddCriterion}>
                      <Plus className="mr-2 h-4 w-4" />
                      Add
                    </Button>
                  </div>
                  {formData.acceptanceCriteria?.length === 0 && (
                    <p className="text-xs text-muted-foreground mt-2">
                      At least 1 acceptance criterion is recommended
                    </p>
                  )}
                </div>
              </CollapsibleContent>
            </Collapsible>

            {/* Attachments */}
            <Collapsible open={isAttachmentsOpen} onOpenChange={setIsAttachmentsOpen}>
              <CollapsibleTrigger asChild>
                <Button type="button" variant="ghost" className="w-full justify-between">
                  <span className="flex items-center gap-2">
                    <Upload className="h-4 w-4" />
                    Attachments ({formData.files.length})
                  </span>
                  {isAttachmentsOpen ? (
                    <ChevronUp className="h-4 w-4" />
                  ) : (
                    <ChevronDown className="h-4 w-4" />
                  )}
                </Button>
              </CollapsibleTrigger>
              <CollapsibleContent className="space-y-2">
                <div
                  className="border-2 border-dashed rounded-lg p-6 text-center cursor-pointer hover:bg-muted/50 transition-colors"
                  onDragOver={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                  }}
                  onDrop={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    const files = Array.from(e.dataTransfer.files);
                    const validFiles: File[] = [];
                    files.forEach((file) => {
                      if (file.size <= 10 * 1024 * 1024) {
                        validFiles.push(file);
                      }
                    });
                    if (validFiles.length > 0) {
                      setFormData({
                        ...formData,
                        files: [...formData.files, ...validFiles],
                      });
                    }
                  }}
                  onClick={() => fileInputRef.current?.click()}
                >
                  <Upload className="h-8 w-8 mx-auto mb-2 text-muted-foreground" />
                  <p className="text-sm text-muted-foreground">
                    Drag & drop files here, or click to browse
                  </p>
                  <p className="text-xs text-muted-foreground mt-1">
                    Max 10MB per file. Images, PDFs, and documents allowed.
                  </p>
                  <input
                    ref={fileInputRef}
                    type="file"
                    multiple
                    className="hidden"
                    onChange={handleFileSelect}
                    accept="image/*,.pdf,.doc,.docx,.txt"
                  />
                </div>
                {formData.files.length > 0 && (
                  <div className="space-y-2">
                    {formData.files.map((file, index) => (
                      <div key={index} className="flex items-center justify-between p-2 border rounded-lg">
                        <div className="flex items-center gap-2 flex-1 min-w-0">
                          <FileText className="h-4 w-4 text-muted-foreground flex-shrink-0" />
                          <span className="text-sm truncate">{file.name}</span>
                          <span className="text-xs text-muted-foreground flex-shrink-0">
                            ({formatFileSize(file.size)})
                          </span>
                        </div>
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          className="h-6 w-6"
                          onClick={() => handleRemoveFile(index)}
                        >
                          <X className="h-3 w-3" />
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </CollapsibleContent>
            </Collapsible>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={createMutation.isPending}>
              {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Create Task
            </Button>
          </DialogFooter>
        </form>

        <AITaskImproverDialog
          open={aiImproverOpen}
          onOpenChange={setAiImproverOpen}
          initialTitle={formData.title}
          initialDescription={formData.description}
          projectId={projectId}
          onApply={handleApplyImprovedTask}
        />
      </DialogContent>
    </Dialog>
  );
}
