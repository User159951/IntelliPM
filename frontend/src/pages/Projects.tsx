import { useState, useMemo, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { projectsApi } from '@/api/projects';
import { usersApi, type User } from '@/api/users';
import { useAuth } from '@/contexts/AuthContext';
import { usePermissions } from '@/hooks/usePermissions';
import { PermissionGuard } from '@/components/guards/PermissionGuard';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Calendar } from '@/components/ui/calendar';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '@/components/ui/command';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Skeleton } from '@/components/ui/skeleton';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { showToast, showError, showWarning } from '@/lib/sweetalert';
import { Plus, FolderKanban, MoreHorizontal, Loader2, Pencil, Trash2, CalendarIcon, X, Check, ChevronsUpDown, ArrowUpDown } from 'lucide-react';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';
import { EditProjectDialog } from '@/components/projects/EditProjectDialog';
import { DeleteProjectDialog } from '@/components/projects/DeleteProjectDialog';
import { ProjectMembersModal } from '@/components/projects/ProjectMembersModal';
import { Pagination } from '@/components/ui/pagination';
import { cn } from '@/lib/utils';
import type { CreateProjectRequest, ProjectType, Project, ProjectStatus, ProjectMember } from '@/types';

export default function Projects() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const { can } = usePermissions();
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const [deletingProject, setDeletingProject] = useState<Project | null>(null);
  const [selectedMembers, setSelectedMembers] = useState<User[]>([]);
  const [memberSearchOpen, setMemberSearchOpen] = useState(false);
  const [startDate, setStartDate] = useState<Date | undefined>(new Date());
  const [membersModalOpen, setMembersModalOpen] = useState(false);
  const [selectedProjectMembers, setSelectedProjectMembers] = useState<ProjectMember[]>([]);
  const [selectedProjectName, setSelectedProjectName] = useState<string>('');
  const [sortBy, setSortBy] = useState<string>(() => {
    const saved = localStorage.getItem('projectsSortBy');
    return saved || 'most-recent';
  });
  const [statusFilter, setStatusFilter] = useState<'active' | 'archived' | 'all'>(() => {
    const saved = localStorage.getItem('projectsStatusFilter');
    return (saved as 'active' | 'archived' | 'all') || 'active';
  });
  const [formData, setFormData] = useState<CreateProjectRequest>({
    name: '',
    description: '',
    type: 'Scrum',
    sprintDurationDays: 14,
    status: 'Active',
  });

  const { data: usersData, isLoading: usersLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(true),
    enabled: isDialogOpen,
  });

  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 12; // 3 columns x 4 rows

  const { data, isLoading } = useQuery({
    queryKey: ['projects', currentPage, pageSize],
    queryFn: () => projectsApi.getAll(currentPage, pageSize),
  });

  // Calculate tab counts - Note: These are estimates based on current page
  // For accurate counts, we'd need separate endpoints or include counts in response
  const tabCounts = useMemo(() => {
    if (!data?.items) return { active: 0, archived: 0, all: data?.totalCount || 0 };
    return {
      active: data.items.filter(p => p.status !== 'Archived').length,
      archived: data.items.filter(p => p.status === 'Archived').length,
      all: data.totalCount,
    };
  }, [data?.items, data?.totalCount]);

  // Filter and sort projects based on selected options
  const filteredAndSortedProjects = useMemo(() => {
    if (!data?.items) return [];
    
    // First, filter by status
    let projects = [...data.items];
    
    switch (statusFilter) {
      case 'active':
        projects = projects.filter(p => p.status !== 'Archived');
        break;
      case 'archived':
        projects = projects.filter(p => p.status === 'Archived');
        break;
      case 'all':
        // No filter
        break;
    }
    
    // Then, sort the filtered projects
    switch (sortBy) {
      case 'most-recent':
        return projects.sort((a, b) => {
          const dateA = new Date(a.createdAt).getTime();
          const dateB = new Date(b.createdAt).getTime();
          return dateB - dateA; // DESC
        });
      
      case 'oldest':
        return projects.sort((a, b) => {
          const dateA = new Date(a.createdAt).getTime();
          const dateB = new Date(b.createdAt).getTime();
          return dateA - dateB; // ASC
        });
      
      case 'alphabetical-az':
        return projects.sort((a, b) => {
          const nameA = (a.name || '').toLowerCase();
          const nameB = (b.name || '').toLowerCase();
          return nameA.localeCompare(nameB); // ASC
        });
      
      case 'alphabetical-za':
        return projects.sort((a, b) => {
          const nameA = (a.name || '').toLowerCase();
          const nameB = (b.name || '').toLowerCase();
          return nameB.localeCompare(nameA); // DESC
        });
      
      case 'most-tasks':
        return projects.sort((a, b) => {
          const tasksA = a.openTasksCount ?? 0;
          const tasksB = b.openTasksCount ?? 0;
          return tasksB - tasksA; // DESC
        });
      
      case 'progress':
        return projects.sort((a, b) => {
          const tasksA = a.openTasksCount ?? null;
          const tasksB = b.openTasksCount ?? null;
          
          if (tasksA === null && tasksB === null) return 0;
          if (tasksA === null) return 1;
          if (tasksB === null) return -1;
          
          return tasksA - tasksB; // ASC (fewer tasks = more progress)
        });
      
      default:
        return projects;
    }
  }, [data?.items, sortBy, statusFilter]);

  // Reset to page 1 when filter changes
  useEffect(() => {
    setCurrentPage(1);
  }, [statusFilter, sortBy]);

  // Save sort preference to localStorage
  const handleSortChange = (value: string) => {
    setSortBy(value);
    localStorage.setItem('projectsSortBy', value);
  };

  // Handle status filter change
  const handleStatusFilterChange = (value: 'active' | 'archived' | 'all') => {
    setStatusFilter(value);
    localStorage.setItem('projectsStatusFilter', value);
  };

  const createMutation = useMutation({
    mutationFn: (data: CreateProjectRequest) => projectsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      setIsDialogOpen(false);
      setFormData({ name: '', description: '', type: 'Scrum', sprintDurationDays: 14, status: 'Active' });
      setSelectedMembers([]);
      setStartDate(new Date());
      showToast('Your new project is ready.', 'success');
    },
    onError: (error) => {
      showError(
        'Failed to create project',
        error instanceof Error ? error.message : 'Please try again'
      );
    },
  });

  const archiveMutation = useMutation({
    mutationFn: (id: number) => projectsApi.archive(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      showToast('Project archived', 'success');
    },
    onError: (error) => {
      showError(
        'Failed to archive project',
        error instanceof Error ? error.message : 'Please try again'
      );
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate start date
    if (startDate && startDate < new Date(new Date().setHours(0, 0, 0, 0))) {
      showWarning('Invalid start date', 'Start date must be today or in the future');
      return;
    }

    const submitData: CreateProjectRequest = {
      ...formData,
      startDate: startDate ? format(startDate, 'yyyy-MM-dd') : undefined,
      memberIds: selectedMembers.map(m => m.id),
    };

    createMutation.mutate(submitData);
  };

  const toggleMember = (member: User) => {
    if (selectedMembers.some(m => m.id === member.id)) {
      setSelectedMembers(selectedMembers.filter(m => m.id !== member.id));
    } else {
      setSelectedMembers([...selectedMembers, member]);
    }
  };

  const removeMember = (memberId: number) => {
    setSelectedMembers(selectedMembers.filter(m => m.id !== memberId));
  };

  const availableUsers = usersData?.users || [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Projects</h1>
          <p className="text-muted-foreground">
            {data?.items ? `Showing ${data.items.length} of ${data.totalCount} projects` : 'Manage your project portfolio'}
          </p>
        </div>
        <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
          <div className="flex items-center gap-2">
            <Label className="text-sm text-muted-foreground whitespace-nowrap">
              Sort by:
            </Label>
            <Select value={sortBy} onValueChange={handleSortChange}>
              <SelectTrigger id="sort-by" aria-label="Sort by" className="w-[180px]">
                <div className="flex items-center gap-2">
                  <ArrowUpDown className="h-4 w-4 text-muted-foreground" />
                  <SelectValue />
                </div>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="most-recent">Most recent</SelectItem>
                <SelectItem value="oldest">Oldest</SelectItem>
                <SelectItem value="alphabetical-az">Alphabetical (A-Z)</SelectItem>
                <SelectItem value="alphabetical-za">Alphabetical (Z-A)</SelectItem>
                <SelectItem value="most-tasks">Most tasks</SelectItem>
                <SelectItem value="progress">Progress (% complete)</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <PermissionGuard 
            requiredPermission="projects.create" 
            fallback={null}
            showNotification={false}
          >
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
              <DialogTrigger asChild>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  Create Project
                </Button>
              </DialogTrigger>
            <DialogContent className="sm:max-w-[500px]">
              <form onSubmit={handleSubmit}>
                <DialogHeader>
                  <DialogTitle>Create new project</DialogTitle>
                  <DialogDescription>
                    Set up a new project to start tracking your work.
                  </DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">Project name</Label>
                    <Input
                      id="name"
                      placeholder="Enter project name"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="description">Description</Label>
                    <Textarea
                      id="description"
                      placeholder="Describe your project"
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                      rows={3}
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="type" id="type-label">Project type</Label>
                      <Select
                        value={formData.type}
                        onValueChange={(value: ProjectType) => setFormData({ ...formData, type: value })}
                      >
                        <SelectTrigger id="type" aria-labelledby="type-label">
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
                      <Label htmlFor="sprintDuration">Sprint duration (days)</Label>
                      <Input
                        id="sprintDuration"
                        type="number"
                        min={1}
                        max={30}
                        value={formData.sprintDurationDays}
                        onChange={(e) => setFormData({ ...formData, sprintDurationDays: parseInt(e.target.value) || 14 })}
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="status" id="status-label">Status *</Label>
                    <Select
                      value={formData.status || 'Active'}
                      onValueChange={(value: ProjectStatus) => setFormData({ ...formData, status: value })}
                      required
                    >
                      <SelectTrigger id="status" aria-labelledby="status-label">
                        <SelectValue placeholder="Select status" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Active">Active</SelectItem>
                        <SelectItem value="Planned">Planned</SelectItem>
                        <SelectItem value="OnHold">On Hold</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="startDate" id="startDate-label">Start Date</Label>
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          id="startDate"
                          aria-labelledby="startDate-label"
                          variant="outline"
                          className={cn(
                            "w-full justify-start text-left font-normal",
                            !startDate && "text-muted-foreground"
                          )}
                        >
                          <CalendarIcon className="mr-2 h-4 w-4" />
                          {startDate ? format(startDate, "MMM dd, yyyy") : <span>Pick a date</span>}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={startDate}
                          onSelect={(date) => {
                            if (date) {
                              const today = new Date();
                              today.setHours(0, 0, 0, 0);
                              if (date >= today) {
                                setStartDate(date);
                              } else {
                                showWarning('Invalid date', 'Start date must be today or in the future');
                              }
                            }
                          }}
                          disabled={(date) => date < new Date(new Date().setHours(0, 0, 0, 0))}
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                  <div className="space-y-2">
                    <Label>Team Members</Label>
                    <Popover open={memberSearchOpen} onOpenChange={setMemberSearchOpen}>
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          role="combobox"
                          aria-expanded={memberSearchOpen}
                          className="w-full justify-between"
                          disabled={usersLoading}
                        >
                          {usersLoading ? (
                            'Loading users...'
                          ) : (
                            <>
                              Select members...
                              <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                            </>
                          )}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-full p-0" align="start">
                        <Command>
                          <CommandInput 
                            id="user-search"
                            name="user-search"
                            autoComplete="off"
                            placeholder="Search users..." 
                          />
                          <CommandList>
                            <CommandEmpty>No users found.</CommandEmpty>
                            <CommandGroup>
                              {availableUsers.map((user) => {
                                const isSelected = selectedMembers.some(m => m.id === user.id);
                                return (
                                  <CommandItem
                                    key={user.id}
                                    value={`${user.firstName} ${user.lastName} ${user.email}`}
                                    onSelect={() => toggleMember(user)}
                                  >
                                    <Check
                                      className={cn(
                                        "mr-2 h-4 w-4",
                                        isSelected ? "opacity-100" : "opacity-0"
                                      )}
                                    />
                                    <div className="flex items-center gap-2">
                                      <Avatar className="h-6 w-6">
                                        <AvatarFallback className="text-xs">
                                          {user.firstName[0]}{user.lastName[0]}
                                        </AvatarFallback>
                                      </Avatar>
                                      <div className="flex flex-col">
                                        <span className="text-sm">
                                          {user.firstName} {user.lastName}
                                        </span>
                                        <span className="text-xs text-muted-foreground">{user.email}</span>
                                      </div>
                                    </div>
                                  </CommandItem>
                                );
                              })}
                            </CommandGroup>
                          </CommandList>
                        </Command>
                      </PopoverContent>
                    </Popover>
                    {user && (
                      <div className="mt-2 flex flex-wrap gap-2">
                        <Badge variant="secondary" className="gap-1">
                          <Avatar className="h-4 w-4">
                            <AvatarFallback className="text-xs">
                              {user.firstName?.[0] || ''}{user.lastName?.[0] || ''}
                            </AvatarFallback>
                          </Avatar>
                          {user.firstName} {user.lastName} (Owner)
                        </Badge>
                        {selectedMembers.map((member) => (
                          <Badge key={member.id} variant="outline" className="gap-1">
                            <Avatar className="h-4 w-4">
                              <AvatarFallback className="text-xs">
                                {member.firstName?.[0] ?? ''}{member.lastName?.[0] ?? ''}
                              </AvatarFallback>
                            </Avatar>
                            {member.firstName} {member.lastName}
                            <button
                              type="button"
                              onClick={() => removeMember(member.id)}
                              className="ml-1 rounded-full hover:bg-muted"
                            >
                              <X className="h-3 w-3" />
                            </button>
                          </Badge>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
                <DialogFooter>
                  <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>
                    Cancel
                  </Button>
                  <Button type="submit" disabled={createMutation.isPending}>
                    {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Create project
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
          </PermissionGuard>
        </div>
      </div>

      <Tabs value={statusFilter} onValueChange={(value) => handleStatusFilterChange(value as 'active' | 'archived' | 'all')} className="w-full">
        <TabsList className="grid w-full max-w-md grid-cols-3">
          <TabsTrigger value="active">
            Active ({tabCounts.active})
          </TabsTrigger>
          <TabsTrigger value="archived">
            Archived ({tabCounts.archived})
          </TabsTrigger>
          <TabsTrigger value="all">
            All ({tabCounts.all})
          </TabsTrigger>
        </TabsList>
        <TabsContent value="active" className="mt-4">
          {isLoading ? (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {[1, 2, 3, 4, 5, 6].map((i) => (
                <Card key={i}>
                  <CardHeader>
                    <Skeleton className="h-5 w-32" />
                    <Skeleton className="h-4 w-48" />
                  </CardHeader>
                  <CardContent>
                    <Skeleton className="h-4 w-full" />
                  </CardContent>
                </Card>
              ))}
            </div>
          ) : filteredAndSortedProjects.length === 0 ? (
            <Card className="flex flex-col items-center justify-center py-16">
              <FolderKanban className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium">No active projects</h3>
              <p className="text-muted-foreground mb-4">Create your first project to get started</p>
              <Button onClick={() => setIsDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Create Project
              </Button>
            </Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {filteredAndSortedProjects.map((project) => (
                <Card
                  key={project.id}
                  className="cursor-pointer transition-all hover:shadow-md hover:border-primary/50"
                  onClick={() => navigate(`/projects/${project.id}`)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      navigate(`/projects/${project.id}`);
                    }
                  }}
                  role="button"
                  tabIndex={0}
                  aria-label={`View project ${project.name}`}
                >
                  <CardHeader className="flex flex-row items-start justify-between space-y-0">
                    <div className="space-y-1">
                      <CardTitle className="text-lg">{project.name}</CardTitle>
                      <CardDescription className="line-clamp-2">
                        {project.description || 'No description'}
                      </CardDescription>
                    </div>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
                        <Button variant="ghost" size="icon" className="h-8 w-8">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={(e) => {
                          e.stopPropagation();
                          navigate(`/projects/${project.id}`);
                        }}>
                          View details
                        </DropdownMenuItem>
                        <DropdownMenuItem 
                          onClick={(e) => {
                            e.stopPropagation();
                            setEditingProject(project);
                          }}
                        >
                          <Pencil className="mr-2 h-4 w-4" />
                          Edit project
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem 
                          className="text-destructive"
                          onClick={(e) => {
                            e.stopPropagation();
                            archiveMutation.mutate(project.id);
                          }}
                        >
                          Archive
                        </DropdownMenuItem>
                        <DropdownMenuItem 
                          className="text-destructive"
                          onClick={(e) => {
                            e.stopPropagation();
                            setDeletingProject(project);
                          }}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          Delete project
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </CardHeader>
                  <CardContent>
                    <div className="flex items-center justify-between text-sm">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                          project.status === 'Active'
                            ? 'bg-green-500/10 text-green-500'
                            : project.status === 'OnHold'
                            ? 'bg-yellow-500/10 text-yellow-500'
                            : 'bg-muted text-muted-foreground'
                        }`}
                      >
                        {project.status}
                      </span>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <span>{project.type}</span>
                        <span>•</span>
                        <span>{project.sprintDurationDays}d sprints</span>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter className="flex items-center justify-between pt-4 border-t">
                    {project.members && project.members.length > 0 ? (
                      <div className="flex items-center -space-x-2">
                        {project.members.slice(0, 3).map((member) => (
                          <Tooltip key={member.userId}>
                            <TooltipTrigger asChild>
                              <Avatar className="h-8 w-8 border-2 border-background cursor-pointer">
                                {member.avatar ? (
                                  <img src={member.avatar} alt={`${member.firstName} ${member.lastName}`} />
                                ) : (
                                  <AvatarFallback className="text-xs">
                                    {member.firstName?.[0] ?? ''}{member.lastName?.[0] ?? ''}
                                  </AvatarFallback>
                                )}
                              </Avatar>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>{member.firstName} {member.lastName}</p>
                            </TooltipContent>
                          </Tooltip>
                        ))}
                        {project.members.length > 3 && (
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <button
                                type="button"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  setSelectedProjectMembers(project.members || []);
                                  setSelectedProjectName(project.name);
                                  setMembersModalOpen(true);
                                }}
                                className="h-8 w-8 rounded-full border-2 border-background bg-muted flex items-center justify-center text-xs font-medium hover:bg-muted/80 transition-colors"
                              >
                                +{project.members.length - 3}
                              </button>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>View all {project.members.length} members</p>
                            </TooltipContent>
                          </Tooltip>
                        )}
                      </div>
                    ) : (
                      <span className="text-xs text-muted-foreground">No members</span>
                    )}
                  </CardFooter>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>
        <TabsContent value="archived" className="mt-4">
          {isLoading ? (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {[1, 2, 3, 4, 5, 6].map((i) => (
                <Card key={i}>
                  <CardHeader>
                    <Skeleton className="h-5 w-32" />
                    <Skeleton className="h-4 w-48" />
                  </CardHeader>
                  <CardContent>
                    <Skeleton className="h-4 w-full" />
                  </CardContent>
                </Card>
              ))}
            </div>
          ) : filteredAndSortedProjects.length === 0 ? (
            <Card className="flex flex-col items-center justify-center py-16">
              <FolderKanban className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium">No archived projects</h3>
              <p className="text-muted-foreground mb-4">Archived projects will appear here</p>
            </Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {filteredAndSortedProjects.map((project) => (
                <Card
                  key={project.id}
                  className="cursor-pointer transition-all hover:shadow-md hover:border-primary/50"
                  onClick={() => navigate(`/projects/${project.id}`)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      navigate(`/projects/${project.id}`);
                    }
                  }}
                  role="button"
                  tabIndex={0}
                  aria-label={`View project ${project.name}`}
                >
                  <CardHeader className="flex flex-row items-start justify-between space-y-0">
                    <div className="space-y-1">
                      <CardTitle className="text-lg">{project.name}</CardTitle>
                      <CardDescription className="line-clamp-2">
                        {project.description || 'No description'}
                      </CardDescription>
                    </div>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
                        <Button variant="ghost" size="icon" className="h-8 w-8">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={(e) => {
                          e.stopPropagation();
                          navigate(`/projects/${project.id}`);
                        }}>
                          View details
                        </DropdownMenuItem>
                        <PermissionGuard 
                          requiredPermission="projects.edit" 
                          projectId={project.id}
                          fallback={null}
                          showNotification={false}
                        >
                          <DropdownMenuItem 
                            onClick={(e) => {
                              e.stopPropagation();
                              setEditingProject(project);
                            }}
                          >
                            <Pencil className="mr-2 h-4 w-4" />
                            Edit project
                          </DropdownMenuItem>
                        </PermissionGuard>
                        <DropdownMenuSeparator />
                        <PermissionGuard 
                          requiredPermission="projects.delete" 
                          projectId={project.id}
                          fallback={null}
                          showNotification={false}
                        >
                          <DropdownMenuItem 
                            className="text-destructive"
                            onClick={(e) => {
                              e.stopPropagation();
                              archiveMutation.mutate(project.id);
                            }}
                          >
                            Archive
                          </DropdownMenuItem>
                          <DropdownMenuItem 
                            className="text-destructive"
                            onClick={(e) => {
                              e.stopPropagation();
                              setDeletingProject(project);
                            }}
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete project
                          </DropdownMenuItem>
                        </PermissionGuard>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </CardHeader>
                  <CardContent>
                    <div className="flex items-center justify-between text-sm">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                          project.status === 'Active'
                            ? 'bg-green-500/10 text-green-500'
                            : project.status === 'OnHold'
                            ? 'bg-yellow-500/10 text-yellow-500'
                            : 'bg-muted text-muted-foreground'
                        }`}
                      >
                        {project.status}
                      </span>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <span>{project.type}</span>
                        <span>•</span>
                        <span>{project.sprintDurationDays}d sprints</span>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter className="flex items-center justify-between pt-4 border-t">
                    {project.members && project.members.length > 0 ? (
                      <div className="flex items-center -space-x-2">
                        {project.members.slice(0, 3).map((member) => (
                          <Tooltip key={member.userId}>
                            <TooltipTrigger asChild>
                              <Avatar className="h-8 w-8 border-2 border-background cursor-pointer">
                                {member.avatar ? (
                                  <img src={member.avatar} alt={`${member.firstName} ${member.lastName}`} />
                                ) : (
                                  <AvatarFallback className="text-xs">
                                    {member.firstName?.[0] ?? ''}{member.lastName?.[0] ?? ''}
                                  </AvatarFallback>
                                )}
                              </Avatar>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>{member.firstName} {member.lastName}</p>
                            </TooltipContent>
                          </Tooltip>
                        ))}
                        {project.members.length > 3 && (
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <button
                                type="button"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  setSelectedProjectMembers(project.members || []);
                                  setSelectedProjectName(project.name);
                                  setMembersModalOpen(true);
                                }}
                                className="h-8 w-8 rounded-full border-2 border-background bg-muted flex items-center justify-center text-xs font-medium hover:bg-muted/80 transition-colors"
                              >
                                +{project.members.length - 3}
                              </button>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>View all {project.members.length} members</p>
                            </TooltipContent>
                          </Tooltip>
                        )}
                      </div>
                    ) : (
                      <span className="text-xs text-muted-foreground">No members</span>
                    )}
                  </CardFooter>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>
        <TabsContent value="all" className="mt-4">
          {isLoading ? (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {[1, 2, 3, 4, 5, 6].map((i) => (
                <Card key={i}>
                  <CardHeader>
                    <Skeleton className="h-5 w-32" />
                    <Skeleton className="h-4 w-48" />
                  </CardHeader>
                  <CardContent>
                    <Skeleton className="h-4 w-full" />
                  </CardContent>
                </Card>
              ))}
            </div>
          ) : filteredAndSortedProjects.length === 0 ? (
            <Card className="flex flex-col items-center justify-center py-16">
              <FolderKanban className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium">No projects yet</h3>
              <p className="text-muted-foreground mb-4">Create your first project to get started</p>
              <Button onClick={() => setIsDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Create Project
              </Button>
            </Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {filteredAndSortedProjects.map((project) => (
                <Card
                  key={project.id}
                  className="cursor-pointer transition-all hover:shadow-md hover:border-primary/50"
                  onClick={() => navigate(`/projects/${project.id}`)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      navigate(`/projects/${project.id}`);
                    }
                  }}
                  role="button"
                  tabIndex={0}
                  aria-label={`View project ${project.name}`}
                >
                  <CardHeader className="flex flex-row items-start justify-between space-y-0">
                    <div className="space-y-1">
                      <CardTitle className="text-lg">{project.name}</CardTitle>
                      <CardDescription className="line-clamp-2">
                        {project.description || 'No description'}
                      </CardDescription>
                    </div>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
                        <Button variant="ghost" size="icon" className="h-8 w-8">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={(e) => {
                          e.stopPropagation();
                          navigate(`/projects/${project.id}`);
                        }}>
                          View details
                        </DropdownMenuItem>
                        <PermissionGuard 
                          requiredPermission="projects.edit" 
                          projectId={project.id}
                          fallback={null}
                          showNotification={false}
                        >
                          <DropdownMenuItem 
                            onClick={(e) => {
                              e.stopPropagation();
                              setEditingProject(project);
                            }}
                          >
                            <Pencil className="mr-2 h-4 w-4" />
                            Edit project
                          </DropdownMenuItem>
                        </PermissionGuard>
                        <DropdownMenuSeparator />
                        <PermissionGuard 
                          requiredPermission="projects.delete" 
                          projectId={project.id}
                          fallback={null}
                          showNotification={false}
                        >
                          <DropdownMenuItem 
                            className="text-destructive"
                            onClick={(e) => {
                              e.stopPropagation();
                              archiveMutation.mutate(project.id);
                            }}
                          >
                            Archive
                          </DropdownMenuItem>
                          <DropdownMenuItem 
                            className="text-destructive"
                            onClick={(e) => {
                              e.stopPropagation();
                              setDeletingProject(project);
                            }}
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete project
                          </DropdownMenuItem>
                        </PermissionGuard>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </CardHeader>
                  <CardContent>
                    <div className="flex items-center justify-between text-sm">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                          project.status === 'Active'
                            ? 'bg-green-500/10 text-green-500'
                            : project.status === 'OnHold'
                            ? 'bg-yellow-500/10 text-yellow-500'
                            : 'bg-muted text-muted-foreground'
                        }`}
                      >
                        {project.status}
                      </span>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <span>{project.type}</span>
                        <span>•</span>
                        <span>{project.sprintDurationDays}d sprints</span>
                      </div>
                    </div>
                  </CardContent>
                  <CardFooter className="flex items-center justify-between pt-4 border-t">
                    {project.members && project.members.length > 0 ? (
                      <div className="flex items-center -space-x-2">
                        {project.members.slice(0, 3).map((member) => (
                          <Tooltip key={member.userId}>
                            <TooltipTrigger asChild>
                              <Avatar className="h-8 w-8 border-2 border-background cursor-pointer">
                                {member.avatar ? (
                                  <img src={member.avatar} alt={`${member.firstName} ${member.lastName}`} />
                                ) : (
                                  <AvatarFallback className="text-xs">
                                    {member.firstName?.[0] ?? ''}{member.lastName?.[0] ?? ''}
                                  </AvatarFallback>
                                )}
                              </Avatar>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>{member.firstName} {member.lastName}</p>
                            </TooltipContent>
                          </Tooltip>
                        ))}
                        {project.members.length > 3 && (
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <button
                                type="button"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  setSelectedProjectMembers(project.members || []);
                                  setSelectedProjectName(project.name);
                                  setMembersModalOpen(true);
                                }}
                                className="h-8 w-8 rounded-full border-2 border-background bg-muted flex items-center justify-center text-xs font-medium hover:bg-muted/80 transition-colors"
                              >
                                +{project.members.length - 3}
                              </button>
                            </TooltipTrigger>
                            <TooltipContent>
                              <p>View all {project.members.length} members</p>
                            </TooltipContent>
                          </Tooltip>
                        )}
                      </div>
                    ) : (
                      <span className="text-xs text-muted-foreground">No members</span>
                    )}
                  </CardFooter>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>

      {/* Pagination Controls */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between border-t pt-4">
          <p className="text-sm text-muted-foreground">
            Showing {data.items.length} of {data.totalCount} projects - Page {currentPage} of {data.totalPages}
          </p>
          
          <Pagination
            currentPage={currentPage}
            totalPages={data.totalPages}
            onPageChange={setCurrentPage}
            isLoading={isLoading}
          />
        </div>
      )}

      <ProjectMembersModal
        open={membersModalOpen}
        onOpenChange={setMembersModalOpen}
        members={selectedProjectMembers}
        projectName={selectedProjectName}
      />

      {editingProject && (
        <EditProjectDialog
          open={!!editingProject}
          onOpenChange={(open) => !open && setEditingProject(null)}
          project={editingProject}
        />
      )}

      {deletingProject && (
        <DeleteProjectDialog
          open={!!deletingProject}
          onOpenChange={(open) => !open && setDeletingProject(null)}
          project={deletingProject}
        />
      )}
    </div>
  );
}
