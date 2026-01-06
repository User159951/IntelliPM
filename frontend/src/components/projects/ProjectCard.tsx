import { useNavigate } from 'react-router-dom';
import { formatDistanceToNow } from 'date-fns';
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Progress } from '@/components/ui/progress';
// Tooltip import removed as not used in current implementation
import {
  Calendar,
  Users,
  CheckCircle2,
  Clock,
  Trello,
  Pencil,
  Trash2,
  Eye,
} from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import type { Project, ProjectType, ProjectStatus } from '@/types';

/**
 * Extended project interface for ProjectCard component.
 */
export interface ProjectCardProject {
  id: number;
  name: string;
  description: string;
  type: ProjectType;
  status: ProjectStatus;
  ownerId: number;
  ownerName?: string;
  memberCount?: number;
  taskCount?: number;
  completedTaskCount?: number;
  currentSprint?: {
    id: number;
    name: string;
    startDate: string;
    endDate: string;
  };
  createdAt: string;
  updatedAt?: string;
  members?: Array<{
    userId: number;
    firstName?: string;
    lastName?: string;
    avatar?: string | null;
  }>;
}

/**
 * Props for the ProjectCard component.
 */
export interface ProjectCardProps {
  /** Project data to display */
  project: ProjectCardProject | Project;
  /** Visual variant of the card */
  variant?: 'default' | 'compact';
  /** Whether to show action buttons */
  showActions?: boolean;
  /** Edit handler */
  onEdit?: (projectId: number) => void;
  /** Delete handler */
  onDelete?: (projectId: number) => void;
  /** Additional CSS classes */
  className?: string;
}

/**
 * ProjectCard component for displaying project information in grid/list views.
 * 
 * Features:
 * - Click to navigate to project detail
 * - Visual status indicators
 * - Progress visualization
 * - Member count and sprint info
 * - Action buttons (edit, delete, view)
 * - Responsive design
 * - Keyboard accessible
 * 
 * @example
 * ```tsx
 * <ProjectCard
 *   project={projectData}
 *   showActions={canEdit}
 *   onEdit={handleEdit}
 *   onDelete={handleDelete}
 * />
 * ```
 */
export default function ProjectCard({
  project,
  variant = 'default',
  showActions = false,
  onEdit,
  onDelete,
  className,
}: ProjectCardProps) {
  const navigate = useNavigate();

  // Normalize project data
  const normalizedProject: ProjectCardProject = {
    id: project.id,
    name: project.name,
    description: project.description || '',
    type: project.type,
    status: project.status,
    ownerId: project.ownerId,
    ownerName: project.ownerName,
    memberCount: 'memberCount' in project ? project.memberCount : project.members?.length,
    taskCount: 'taskCount' in project ? project.taskCount : undefined,
    completedTaskCount: 'completedTaskCount' in project ? project.completedTaskCount : undefined,
    currentSprint: 'currentSprint' in project ? project.currentSprint : undefined,
    createdAt: project.createdAt,
    updatedAt: 'updatedAt' in project ? project.updatedAt : undefined,
    members: 'members' in project ? project.members : undefined,
  };

  const handleCardClick = () => {
    navigate(`/projects/${normalizedProject.id}`);
  };

  const handleActionClick = (e: React.MouseEvent, action: 'edit' | 'delete' | 'view') => {
    e.stopPropagation();
    if (action === 'edit' && onEdit) {
      onEdit(normalizedProject.id);
    } else if (action === 'delete' && onDelete) {
      onDelete(normalizedProject.id);
    } else if (action === 'view') {
      handleCardClick();
    }
  };

  // Calculate progress
  const progressPercentage =
    normalizedProject.taskCount && normalizedProject.taskCount > 0
      ? Math.round(
          ((normalizedProject.completedTaskCount || 0) / normalizedProject.taskCount) * 100
        )
      : 0;

  // Status colors
  const statusColors = {
    Active: 'bg-green-500/10 text-green-500 border-green-500/20',
    OnHold: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
    Archived: 'bg-muted text-muted-foreground border-muted',
    Completed: 'bg-blue-500/10 text-blue-500 border-blue-500/20',
  };

  // Type icons
  const TypeIcon = normalizedProject.type === 'Scrum' ? Calendar : Trello;

  // Get owner initials
  const getOwnerInitials = () => {
    if (normalizedProject.ownerName) {
      const parts = normalizedProject.ownerName.split(' ');
      return parts
        .map((p) => p[0])
        .join('')
        .toUpperCase()
        .slice(0, 2);
    }
    return 'PO';
  };

  // Compact variant
  if (variant === 'compact') {
    return (
      <Card
        className={cn(
          'transition-all hover:shadow-md hover:scale-[1.02]',
          className
        )}
      >
        <button
          type="button"
          onClick={handleCardClick}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              handleCardClick();
            }
          }}
          className="w-full text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 rounded-lg transition-all hover:shadow-md"
          aria-label={`View project ${normalizedProject.name}`}
        >
          <CardHeader className="pb-3">
            <div className="flex items-start justify-between gap-2">
              <div className="flex-1 min-w-0">
                <CardTitle className="text-base truncate" title={normalizedProject.name}>
                  {normalizedProject.name}
                </CardTitle>
              </div>
              <Badge
                variant="outline"
                className={cn('text-xs flex-shrink-0', statusColors[normalizedProject.status])}
              >
                {normalizedProject.status}
              </Badge>
            </div>
          </CardHeader>
          <CardContent className="pt-0">
            <div className="flex items-center gap-4 text-sm text-muted-foreground">
              {normalizedProject.memberCount !== undefined && (
                <div className="flex items-center gap-1.5">
                  <Users className="h-4 w-4" />
                  <span>{normalizedProject.memberCount}</span>
                </div>
              )}
              {normalizedProject.taskCount !== undefined && (
                <div className="flex items-center gap-1.5">
                  <CheckCircle2 className="h-4 w-4" />
                  <span>{normalizedProject.completedTaskCount || 0}/{normalizedProject.taskCount}</span>
                </div>
              )}
            </div>
          </CardContent>
        </button>
      </Card>
    );
  }

  // Default variant
  return (
    <Card
      className={cn(
        'transition-all duration-200 hover:shadow-lg hover:scale-[1.02]',
        className
      )}
    >
      <button
        type="button"
        onClick={handleCardClick}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            handleCardClick();
          }
        }}
        className="w-full text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 rounded-lg transition-all hover:shadow-md"
        aria-label={`View project ${normalizedProject.name}`}
      >
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between gap-2">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-2">
                <div className="rounded-lg bg-primary/10 p-1.5">
                  <TypeIcon className="h-4 w-4 text-primary" />
                </div>
                <CardTitle className="text-lg truncate" title={normalizedProject.name}>
                  {normalizedProject.name}
                </CardTitle>
              </div>
              <div className="flex items-center gap-2 flex-wrap">
                <Badge variant="outline" className={cn('text-xs', statusColors[normalizedProject.status])}>
                  {normalizedProject.status}
                </Badge>
                <Badge variant="secondary" className="text-xs">
                  {normalizedProject.type}
                </Badge>
              </div>
            </div>
          </div>
        </CardHeader>

        <CardContent className="space-y-4">
          {/* Description */}
          {normalizedProject.description && (
            <CardDescription className="line-clamp-2 text-sm">
              {normalizedProject.description}
            </CardDescription>
          )}

          {/* Metrics */}
          <div className="space-y-3">
            {/* Member count */}
            {normalizedProject.memberCount !== undefined && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Users className="h-4 w-4" />
                <span>{normalizedProject.memberCount} member{normalizedProject.memberCount !== 1 ? 's' : ''}</span>
              </div>
            )}

            {/* Task progress */}
            {normalizedProject.taskCount !== undefined && normalizedProject.taskCount > 0 && (
              <div className="space-y-1.5">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted-foreground">Tasks</span>
                  <span className="font-medium">
                    {normalizedProject.completedTaskCount || 0}/{normalizedProject.taskCount} ({progressPercentage}%)
                  </span>
                </div>
                <Progress value={progressPercentage} className="h-2" />
              </div>
            )}

            {/* Current sprint */}
            {normalizedProject.currentSprint && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Calendar className="h-4 w-4" />
                <span className="truncate">{normalizedProject.currentSprint.name}</span>
              </div>
            )}
          </div>

          {/* Owner info */}
          <div className="flex items-center gap-2 pt-2 border-t">
            <Avatar className="h-6 w-6">
              <AvatarFallback className="text-xs bg-primary/10 text-primary">
                {getOwnerInitials()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1 min-w-0">
              <p className="text-xs text-muted-foreground truncate">
                {normalizedProject.ownerName || 'Project Owner'}
              </p>
            </div>
          </div>

          {/* Last updated */}
          {normalizedProject.updatedAt && (
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <Clock className="h-3 w-3" />
              <span>
                Updated {formatDistanceToNow(new Date(normalizedProject.updatedAt), { addSuffix: true })}
              </span>
            </div>
          )}
        </CardContent>
      </button>

      {/* Footer with actions */}
      {showActions && (
        <CardFooter className="flex items-center justify-between pt-4 border-t gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => handleActionClick(e, 'view')}
            className="flex-1"
          >
            <Eye className="h-4 w-4 mr-2" />
            View
          </Button>
          {onEdit && (
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => handleActionClick(e, 'edit')}
              className="flex-1"
            >
              <Pencil className="h-4 w-4 mr-2" />
              Edit
            </Button>
          )}
          {onDelete && (
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => handleActionClick(e, 'delete')}
              className="flex-1 text-destructive hover:text-destructive"
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete
            </Button>
          )}
        </CardFooter>
      )}
    </Card>
  );
}

/**
 * Skeleton loader for ProjectCard component.
 * Used during loading states.
 */
export function ProjectCardSkeleton({ variant = 'default' }: { variant?: 'default' | 'compact' }) {
  if (variant === 'compact') {
    return (
      <Card>
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between gap-2">
            <Skeleton className="h-5 w-3/4" />
            <Skeleton className="h-5 w-16" />
          </div>
        </CardHeader>
        <CardContent className="pt-0">
          <div className="flex items-center gap-4">
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-4 w-16" />
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1 space-y-2">
            <div className="flex items-center gap-2">
              <Skeleton className="h-6 w-6 rounded-lg" />
              <Skeleton className="h-6 w-3/4" />
            </div>
            <div className="flex items-center gap-2">
              <Skeleton className="h-5 w-16" />
              <Skeleton className="h-5 w-20" />
            </div>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-5/6" />
        <div className="space-y-2">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-2 w-full" />
        </div>
        <div className="flex items-center gap-2 pt-2 border-t">
          <Skeleton className="h-6 w-6 rounded-full" />
          <Skeleton className="h-4 w-32" />
        </div>
      </CardContent>
      <CardFooter className="pt-4 border-t">
        <Skeleton className="h-9 w-full" />
      </CardFooter>
    </Card>
  );
}

