import { useState, useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { defectsApi } from '@/api/defects';
import { projectsApi } from '@/api/projects';
import { usersApi } from '@/api/users';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Bug, ArrowUpDown, Search, X } from 'lucide-react';
import type { Defect, DefectSeverity, DefectStatus, Project } from '@/types';
import { CreateDefectDialog } from '@/components/defects/CreateDefectDialog';
import { DefectDetailSheet } from '@/components/defects/DefectDetailSheet';
import { useDebounce } from '@/hooks/use-debounce';
import { format } from 'date-fns';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { PermissionGuard } from '@/components/guards/PermissionGuard';

const severityColors: Record<DefectSeverity, string> = {
  Low: 'bg-green-500/10 text-green-500 border-green-500/20',
  Medium: 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20',
  High: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
  Critical: 'bg-red-500/10 text-red-500 border-red-500/20',
};

const statusColors: Record<DefectStatus, string> = {
  Open: 'bg-red-500/10 text-red-500',
  InProgress: 'bg-blue-500/10 text-blue-500',
  Resolved: 'bg-green-500/10 text-green-500',
  Closed: 'bg-muted text-muted-foreground',
};

type SortOption = 'severity' | 'created' | 'status' | 'title';

const severityOrder: Record<DefectSeverity, number> = {
  Critical: 4,
  High: 3,
  Medium: 2,
  Low: 1,
};

export default function Defects() {
  const queryClient = useQueryClient();
  const [selectedProjectId, setSelectedProjectId] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<DefectStatus | 'all'>('all');
  const [severityFilter, setSeverityFilter] = useState<DefectSeverity | 'all'>('all');
  const [assigneeFilter, setAssigneeFilter] = useState<number | 'all'>('all');
  const [sortBy, setSortBy] = useState<SortOption>('created');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedDefect, setSelectedDefect] = useState<Defect | null>(null);
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const debouncedSearch = useDebounce(searchQuery, 300);

  const { data: projectsData } = useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsApi.getAll(),
  });

  const projectId = selectedProjectId || projectsData?.items?.[0]?.id?.toString();
  const projectIdNum = projectId ? parseInt(projectId) : null;

  const { data: usersData } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.getAll(),
    enabled: !!projectIdNum,
  });

  const { data: defectsData, isLoading } = useQuery({
    queryKey: ['defects', projectIdNum, statusFilter, severityFilter, assigneeFilter],
    queryFn: () =>
      defectsApi.getByProject(projectIdNum!, {
        status: statusFilter !== 'all' ? statusFilter : undefined,
        severity: severityFilter !== 'all' ? severityFilter : undefined,
        assignedToId: assigneeFilter !== 'all' ? assigneeFilter : undefined,
      }),
    enabled: !!projectIdNum,
  });

  // Filter and sort defects
  const filteredAndSortedDefects = useMemo(() => {
    if (!defectsData?.defects) return [];

    let filtered = [...defectsData.defects];

    // Search filter
    if (debouncedSearch) {
      const query = debouncedSearch.toLowerCase();
      filtered = filtered.filter(
        (defect) =>
          defect.title.toLowerCase().includes(query) ||
          defect.description?.toLowerCase().includes(query) ||
          defect.id.toString().includes(query)
      );
    }

    // Sort
    filtered.sort((a, b) => {
      switch (sortBy) {
        case 'severity':
          return (severityOrder[b.severity] || 0) - (severityOrder[a.severity] || 0);
        case 'created':
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        case 'status':
          return a.status.localeCompare(b.status);
        case 'title':
          return a.title.localeCompare(b.title);
        default:
          return 0;
      }
    });

    return filtered;
  }, [defectsData?.defects, debouncedSearch, sortBy]);

  const activeFiltersCount = useMemo(() => {
    let count = 0;
    if (statusFilter !== 'all') count++;
    if (severityFilter !== 'all') count++;
    if (assigneeFilter !== 'all') count++;
    if (debouncedSearch) count++;
    return count;
  }, [statusFilter, severityFilter, assigneeFilter, debouncedSearch]);

  const clearAllFilters = () => {
    setStatusFilter('all');
    setSeverityFilter('all');
    setAssigneeFilter('all');
    setSearchQuery('');
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Defects</h1>
          <p className="text-muted-foreground">Track and manage bugs and issues</p>
        </div>
        <div className="flex items-center gap-4">
          <Select value={projectId} onValueChange={setSelectedProjectId}>
            <SelectTrigger className="w-[200px]" id="project-select" name="project">
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
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <span>
                  <PermissionGuard 
                    requiredPermission="defects.create" 
                    projectId={projectId || undefined}
                    fallback={null}
                    showNotification={false}
                  >
                    <Button disabled={!projectId} onClick={() => setIsCreateDialogOpen(true)}>
                      <Bug className="mr-2 h-4 w-4" />
                      Report Defect
                    </Button>
                  </PermissionGuard>
                </span>
              </TooltipTrigger>
              {!projectId && (
                <TooltipContent>
                  <p>Select a project first to report a defect</p>
                </TooltipContent>
              )}
            </Tooltip>
          </TooltipProvider>
        </div>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Defect List</CardTitle>
              <CardDescription>
                {filteredAndSortedDefects.length === defectsData?.defects?.length
                  ? `Showing all ${defectsData?.total || 0} defects`
                  : `Showing ${filteredAndSortedDefects.length} of ${defectsData?.total || 0} defects`}
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {/* Filters and Search */}
          <div className="flex flex-wrap items-center gap-3 mb-4">
            <div className="relative flex-1 min-w-[200px]">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                id="search-defects"
                name="search"
                placeholder="Search defects..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
              {searchQuery && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="absolute right-1 top-1/2 -translate-y-1/2 h-7 w-7 p-0"
                  onClick={() => setSearchQuery('')}
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </div>

            <Select value={statusFilter} onValueChange={(v) => setStatusFilter(v as DefectStatus | 'all')}>
              <SelectTrigger className="w-[150px]" id="status-filter" name="status">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="Open">Open</SelectItem>
                <SelectItem value="InProgress">In Progress</SelectItem>
                <SelectItem value="Resolved">Resolved</SelectItem>
                <SelectItem value="Closed">Closed</SelectItem>
              </SelectContent>
            </Select>

            <Select value={severityFilter} onValueChange={(v) => setSeverityFilter(v as DefectSeverity | 'all')}>
              <SelectTrigger className="w-[150px]" id="severity-filter" name="severity">
                <SelectValue placeholder="Severity" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Severity</SelectItem>
                <SelectItem value="Critical">Critical</SelectItem>
                <SelectItem value="High">High</SelectItem>
                <SelectItem value="Medium">Medium</SelectItem>
                <SelectItem value="Low">Low</SelectItem>
              </SelectContent>
            </Select>

            <Select
              value={assigneeFilter === 'all' ? 'all' : assigneeFilter.toString()}
              onValueChange={(v) => setAssigneeFilter(v === 'all' ? 'all' : parseInt(v))}
            >
              <SelectTrigger className="w-[150px]" id="assignee-filter" name="assignee">
                <SelectValue placeholder="Assignee" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Assignees</SelectItem>
                {usersData?.users?.map((user) => (
                  <SelectItem key={user.id} value={user.id.toString()}>
                    {user.firstName} {user.lastName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select value={sortBy} onValueChange={(v) => setSortBy(v as SortOption)}>
              <SelectTrigger className="w-[180px]" id="sort-by" name="sortBy">
                <SelectValue placeholder="Sort by" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="created">Recently Created</SelectItem>
                <SelectItem value="severity">Severity (High → Low)</SelectItem>
                <SelectItem value="status">Status</SelectItem>
                <SelectItem value="title">Title (A-Z)</SelectItem>
              </SelectContent>
            </Select>

            {activeFiltersCount > 0 && (
              <Button variant="ghost" size="sm" onClick={clearAllFilters}>
                <X className="mr-2 h-4 w-4" />
                Clear filters ({activeFiltersCount})
              </Button>
            )}
          </div>

          {isLoading ? (
            <div className="space-y-4">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : !projectId ? (
            <div className="py-8 text-center">
              <p className="text-muted-foreground">Select a project to view defects</p>
            </div>
          ) : filteredAndSortedDefects.length === 0 ? (
            <div className="py-16 text-center">
              <Bug className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium">No defects found</h3>
              <p className="text-muted-foreground">
                {activeFiltersCount > 0
                  ? 'Try adjusting your filters'
                  : 'No defects have been reported yet'}
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>ID</TableHead>
                  <TableHead>Title</TableHead>
                  <TableHead>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 -ml-3"
                      onClick={() => setSortBy(sortBy === 'severity' ? 'created' : 'severity')}
                    >
                      Severity
                      <ArrowUpDown className="ml-2 h-3 w-3" />
                    </Button>
                  </TableHead>
                  <TableHead>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 -ml-3"
                      onClick={() => setSortBy(sortBy === 'status' ? 'created' : 'status')}
                    >
                      Status
                      <ArrowUpDown className="ml-2 h-3 w-3" />
                    </Button>
                  </TableHead>
                  <TableHead>Assignee</TableHead>
                  <TableHead>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 -ml-3"
                      onClick={() => setSortBy(sortBy === 'created' ? 'severity' : 'created')}
                    >
                      Created
                      <ArrowUpDown className="ml-2 h-3 w-3" />
                    </Button>
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredAndSortedDefects.map((defect) => (
                  <TableRow
                    key={defect.id}
                    className="cursor-pointer hover:bg-accent/50"
                    onClick={() => setSelectedDefect(defect)}
                  >
                    <TableCell className="font-mono text-sm text-muted-foreground">
                      #{defect.id}
                    </TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{defect.title}</p>
                        <p className="text-sm text-muted-foreground line-clamp-1">
                          {defect.description}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge className={severityColors[defect.severity]} variant="outline">
                        {defect.severity}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge className={statusColors[defect.status]}>{defect.status}</Badge>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm text-muted-foreground">
                        {defect.assignedToName || 'Unassigned'}
                      </span>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm text-muted-foreground">
                        {format(new Date(defect.createdAt), 'MMM d, yyyy')}
                      </span>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Create Dialog */}
      {projectIdNum && (
        <CreateDefectDialog
          open={isCreateDialogOpen}
          onOpenChange={setIsCreateDialogOpen}
          projectId={projectIdNum}
        />
      )}

      {/* Detail Sheet */}
      {projectIdNum && selectedDefect && (
        <DefectDetailSheet
          defect={selectedDefect}
          open={!!selectedDefect}
          onOpenChange={(open) => !open && setSelectedDefect(null)}
          projectId={projectIdNum}
          onDefectUpdated={() => {
            queryClient.invalidateQueries({ queryKey: ['defects', projectIdNum] });
            setSelectedDefect(null);
          }}
          onDefectDeleted={() => {
            queryClient.invalidateQueries({ queryKey: ['defects', projectIdNum] });
            setSelectedDefect(null);
          }}
        />
      )}
    </div>
  );
}
