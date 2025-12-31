import { useState, useMemo } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Separator } from '@/components/ui/separator';
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  Edit,
  Rocket,
  Trash,
  Calendar,
  Tag,
  LayoutGrid,
  ShieldCheck,
  FileText,
  Eye,
  CheckCircle,
  AlertCircle,
} from 'lucide-react';
import { format } from 'date-fns';
import { releasesApi } from '@/api/releases';
import { projectsApi } from '@/api/projects';
import { showToast } from '@/lib/sweetalert';
import type { ReleaseSprintDto } from '@/types/releases';
import { QualityGatesPanel } from '@/components/releases/QualityGatesPanel';
import { EditReleaseDialog } from '@/components/releases/EditReleaseDialog';
import { DeployReleaseDialog } from '@/components/releases/DeployReleaseDialog';
import { ReleaseNotesViewer } from '@/components/releases/ReleaseNotesViewer';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';

interface SprintCardProps {
  sprint: ReleaseSprintDto;
}

function SprintCard({ sprint }: SprintCardProps) {
  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-4">
        <div className="flex items-start justify-between mb-2">
          <div>
            <h4 className="font-semibold text-sm">{sprint.name}</h4>
            <p className="text-xs text-muted-foreground mt-1">
              {format(new Date(sprint.startDate), 'MMM d')} -{' '}
              {format(new Date(sprint.endDate), 'MMM d, yyyy')}
            </p>
          </div>
          <Badge variant="outline">{sprint.status}</Badge>
        </div>
        <div className="flex items-center gap-2 mt-3">
          <span className="text-xs text-muted-foreground">
            {sprint.completedTasksCount}/{sprint.totalTasksCount} tasks
          </span>
          <div className="flex-1 h-2 bg-muted rounded-full overflow-hidden">
            <div
              className="h-full bg-primary transition-all"
              style={{ width: `${sprint.completionPercentage}%` }}
            />
          </div>
          <span className="text-xs font-medium">
            {sprint.completionPercentage}%
          </span>
        </div>
      </CardContent>
    </Card>
  );
}

interface SprintDetailItemProps {
  sprint: ReleaseSprintDto;
}

function SprintDetailItem({ sprint }: SprintDetailItemProps) {
  return (
    <div className="flex items-center justify-between p-4 rounded-lg border">
      <div className="flex-1">
        <div className="flex items-center gap-3 mb-2">
          <h4 className="font-semibold">{sprint.name}</h4>
          <Badge variant="outline">{sprint.status}</Badge>
        </div>
        <p className="text-sm text-muted-foreground">
          {format(new Date(sprint.startDate), 'MMM d, yyyy')} -{' '}
          {format(new Date(sprint.endDate), 'MMM d, yyyy')}
        </p>
        <div className="flex items-center gap-2 mt-2">
          <span className="text-sm text-muted-foreground">
            {sprint.completedTasksCount}/{sprint.totalTasksCount} tasks completed
          </span>
        </div>
      </div>
      <div className="text-right">
        <div className="text-2xl font-bold">{sprint.completionPercentage}%</div>
        <div className="text-xs text-muted-foreground">Complete</div>
      </div>
    </div>
  );
}

function getTypeBadgeVariant(type: string): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (type) {
    case 'Major':
      return 'destructive';
    case 'Minor':
      return 'default';
    case 'Patch':
      return 'secondary';
    case 'Hotfix':
      return 'outline';
    default:
      return 'outline';
  }
}

function getStatusBadgeVariant(status: string): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (status) {
    case 'Deployed':
      return 'default';
    case 'Planned':
      return 'secondary';
    case 'InProgress':
    case 'Testing':
    case 'ReadyForDeployment':
      return 'outline';
    case 'Failed':
      return 'destructive';
    default:
      return 'outline';
  }
}

function getQualityBadgeVariant(status: string | null): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (status) {
    case 'Passed':
      return 'default';
    case 'Warning':
      return 'secondary';
    case 'Failed':
      return 'destructive';
    default:
      return 'outline';
  }
}

/**
 * Full page component for viewing release details.
 * Displays release information, quality gates, notes, changelog, and sprints.
 */
export default function ReleaseDetailPage() {
  const { projectId, releaseId } = useParams<{
    projectId: string;
    releaseId: string;
  }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeployOpen, setIsDeployOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [isNotesViewerOpen, setIsNotesViewerOpen] = useState(false);
  const [_notesMode, setNotesMode] = useState<'notes' | 'changelog'>('notes');

  // Fetch release data
  const {
    data: release,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['release', Number(releaseId)],
    queryFn: () => releasesApi.getRelease(Number(releaseId)),
    enabled: !!releaseId,
  });

  // Fetch project data
  const { data: project } = useQuery({
    queryKey: ['project', Number(projectId)],
    queryFn: () => projectsApi.getById(Number(projectId)),
    enabled: !!projectId,
  });

  const canDeploy = useMemo(() => {
    if (!release) return false;
    return (
      release.status === 'ReadyForDeployment' &&
      release.overallQualityStatus === 'Passed'
    );
  }, [release]);

  const handleDelete = async () => {
    try {
      await releasesApi.deleteRelease(Number(releaseId));
      showToast('Release deleted successfully', 'success');
      navigate(`/projects/${projectId}/releases`);
    } catch (error) {
      showToast('Failed to delete release', 'error');
    }
  };

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Breadcrumb */}
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink asChild>
              <Link to="/projects">Projects</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbLink asChild>
              <Link to={`/projects/${projectId}`}>
                {project?.name || 'Project'}
              </Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbLink asChild>
              <Link to={`/projects/${projectId}/releases`}>Releases</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbPage>
              {release?.version || 'Release'}
            </BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      {/* Loading state */}
      {isLoading && (
        <div className="space-y-4">
          <Skeleton className="h-32 w-full" />
          <Skeleton className="h-64 w-full" />
        </div>
      )}

      {/* Error state */}
      {error && (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <AlertCircle className="h-12 w-12 text-destructive mb-4" />
            <h3 className="text-lg font-semibold mb-2">Release Not Found</h3>
            <p className="text-sm text-muted-foreground mb-4">
              The release you're looking for doesn't exist or has been deleted.
            </p>
            <Button
              onClick={() => navigate(`/projects/${projectId}/releases`)}
            >
              Back to Releases
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Content */}
      {release && (
        <>
          {/* Header Section */}
          <Card>
            <CardContent className="p-6">
              <div className="flex items-start justify-between">
                {/* Left: Version info */}
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <h1 className="text-3xl font-bold">{release.version}</h1>
                    <Badge variant={getTypeBadgeVariant(release.type)}>
                      {release.type}
                    </Badge>
                    {release.isPreRelease && (
                      <Badge variant="secondary">Pre-release</Badge>
                    )}
                  </div>
                  <h2 className="text-xl text-muted-foreground">
                    {release.name}
                  </h2>

                  <div className="flex items-center gap-4 mt-4">
                    <div className="flex items-center gap-2">
                      <Badge variant={getStatusBadgeVariant(release.status)}>
                        {release.status}
                      </Badge>
                    </div>

                    <Separator orientation="vertical" className="h-4" />

                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <Calendar className="h-4 w-4" />
                      <span>
                        {release.actualReleaseDate
                          ? `Released ${format(new Date(release.actualReleaseDate), 'MMM d, yyyy')}`
                          : `Planned ${format(new Date(release.plannedDate), 'MMM d, yyyy')}`}
                      </span>
                    </div>

                    {release.tagName && (
                      <>
                        <Separator orientation="vertical" className="h-4" />
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <Tag className="h-4 w-4" />
                          <span>{release.tagName}</span>
                        </div>
                      </>
                    )}
                  </div>
                </div>

                {/* Right: Action buttons */}
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setIsEditOpen(true)}
                  >
                    <Edit className="h-4 w-4 mr-2" />
                    Edit
                  </Button>

                  {canDeploy && (
                    <Button size="sm" onClick={() => setIsDeployOpen(true)}>
                      <Rocket className="h-4 w-4 mr-2" />
                      Deploy
                    </Button>
                  )}

                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => setIsDeleteOpen(true)}
                  >
                    <Trash className="h-4 w-4 mr-2" />
                    Delete
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Tabs Section */}
          <Tabs defaultValue="overview" className="space-y-4">
            <TabsList>
              <TabsTrigger value="overview">
                <LayoutGrid className="h-4 w-4 mr-2" />
                Overview
              </TabsTrigger>
              <TabsTrigger value="quality">
                <ShieldCheck className="h-4 w-4 mr-2" />
                Quality Gates
              </TabsTrigger>
              <TabsTrigger value="notes">
                <FileText className="h-4 w-4 mr-2" />
                Release Notes
              </TabsTrigger>
              <TabsTrigger value="changelog">
                <FileText className="h-4 w-4 mr-2" />
                Changelog
              </TabsTrigger>
              <TabsTrigger value="sprints">
                <Calendar className="h-4 w-4 mr-2" />
                Sprints
              </TabsTrigger>
            </TabsList>

            {/* Overview Tab */}
            <TabsContent value="overview" className="space-y-4">
              {/* Description */}
              {release.description && (
                <Card>
                  <CardHeader>
                    <CardTitle>Description</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <p className="text-muted-foreground">
                      {release.description}
                    </p>
                  </CardContent>
                </Card>
              )}

              {/* Key Metrics */}
              <Card>
                <CardHeader>
                  <CardTitle>Key Metrics</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-full bg-blue-100 dark:bg-blue-900">
                        <Calendar className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Sprints</p>
                        <p className="text-2xl font-bold">
                          {release.sprintCount}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-full bg-green-100 dark:bg-green-900">
                        <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400" />
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">
                          Tasks Completed
                        </p>
                        <p className="text-2xl font-bold">
                          {release.completedTasksCount}/
                          {release.totalTasksCount}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-full bg-purple-100 dark:bg-purple-900">
                        <ShieldCheck className="h-5 w-5 text-purple-600 dark:text-purple-400" />
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">
                          Quality Status
                        </p>
                        <Badge
                          variant={getQualityBadgeVariant(
                            release.overallQualityStatus
                          )}
                        >
                          {release.overallQualityStatus || 'Pending'}
                        </Badge>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Sprints Grid */}
              {release.sprints && release.sprints.length > 0 && (
                <Card>
                  <CardHeader>
                    <CardTitle>Included Sprints</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      {release.sprints.map((sprint) => (
                        <SprintCard key={sprint.id} sprint={sprint} />
                      ))}
                    </div>
                  </CardContent>
                </Card>
              )}
            </TabsContent>

            {/* Quality Gates Tab */}
            <TabsContent value="quality">
              <QualityGatesPanel release={release} />
            </TabsContent>

            {/* Release Notes Tab */}
            <TabsContent value="notes">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                  <CardTitle>Release Notes</CardTitle>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setNotesMode('notes');
                      setIsNotesViewerOpen(true);
                    }}
                  >
                    <Eye className="h-4 w-4 mr-2" />
                    View Full
                  </Button>
                </CardHeader>
                <CardContent>
                  {release.releaseNotes ? (
                    <div className="prose prose-sm dark:prose-invert max-w-none">
                      <ReactMarkdown
                        remarkPlugins={[remarkGfm]}
                        rehypePlugins={[rehypeHighlight]}
                      >
                        {release.releaseNotes}
                      </ReactMarkdown>
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">
                      No release notes available.
                    </p>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            {/* Changelog Tab */}
            <TabsContent value="changelog">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                  <CardTitle>Changelog</CardTitle>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setNotesMode('changelog');
                      setIsNotesViewerOpen(true);
                    }}
                  >
                    <Eye className="h-4 w-4 mr-2" />
                    View Full
                  </Button>
                </CardHeader>
                <CardContent>
                  {release.changeLog ? (
                    <div className="prose prose-sm dark:prose-invert max-w-none">
                      <ReactMarkdown
                        remarkPlugins={[remarkGfm]}
                        rehypePlugins={[rehypeHighlight]}
                      >
                        {release.changeLog}
                      </ReactMarkdown>
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">
                      No changelog available.
                    </p>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            {/* Sprints Tab */}
            <TabsContent value="sprints">
              <Card>
                <CardHeader>
                  <CardTitle>Sprint Details</CardTitle>
                </CardHeader>
                <CardContent>
                  {release.sprints && release.sprints.length > 0 ? (
                    <div className="space-y-3">
                      {release.sprints.map((sprint) => (
                        <SprintDetailItem key={sprint.id} sprint={sprint} />
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">
                      No sprints included in this release.
                    </p>
                  )}
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </>
      )}

      {/* Dialogs */}
      {release && (
        <>
          <EditReleaseDialog
            release={release}
            open={isEditOpen}
            onOpenChange={setIsEditOpen}
            onSuccess={() => {
              queryClient.invalidateQueries({
                queryKey: ['release', Number(releaseId)],
              });
            }}
          />

          <DeployReleaseDialog
            release={release}
            open={isDeployOpen}
            onOpenChange={setIsDeployOpen}
            onSuccess={() => {
              queryClient.invalidateQueries({
                queryKey: ['release', Number(releaseId)],
              });
            }}
          />

          <ReleaseNotesViewer
            release={release}
            open={isNotesViewerOpen}
            onOpenChange={setIsNotesViewerOpen}
            onEdit={() => {
              // TODO: Open editor
            }}
          />

          <AlertDialog open={isDeleteOpen} onOpenChange={setIsDeleteOpen}>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Delete Release</AlertDialogTitle>
                <AlertDialogDescription>
                  Are you sure you want to delete release {release.version}?
                  This action cannot be undone.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction
                  onClick={handleDelete}
                  className="bg-destructive hover:bg-destructive/90"
                >
                  Delete
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </>
      )}
    </div>
  );
}

