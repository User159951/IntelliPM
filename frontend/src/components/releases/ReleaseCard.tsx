import { Card, CardContent, CardFooter, CardHeader } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Package, Eye, Rocket, Edit, Trash2, Calendar } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ReleaseDto } from '@/types/releases';
import { format } from 'date-fns';

interface ReleaseCardProps {
  release: ReleaseDto;
  onEdit?: (release: ReleaseDto) => void;
  onDeploy?: (release: ReleaseDto) => void;
  onDelete?: (release: ReleaseDto) => void;
  onViewNotes?: (release: ReleaseDto) => void;
}

const getTypeColor = (type: string) => {
  switch (type) {
    case 'Major':
      return 'bg-red-500 text-white';
    case 'Minor':
      return 'bg-blue-500 text-white';
    case 'Patch':
      return 'bg-green-500 text-white';
    case 'Hotfix':
      return 'bg-orange-500 text-white';
    default:
      return 'bg-gray-500 text-white';
  }
};

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Deployed':
      return 'bg-green-500 text-white';
    case 'ReadyForDeployment':
      return 'bg-blue-500 text-white';
    case 'InProgress':
      return 'bg-orange-500 text-white';
    case 'Testing':
      return 'bg-purple-500 text-white';
    case 'Failed':
      return 'bg-red-500 text-white';
    case 'Cancelled':
      return 'bg-gray-500 text-white';
    default:
      return 'bg-gray-400 text-white';
  }
};

const getQualityGateColor = (status: string | null) => {
  switch (status) {
    case 'Passed':
      return 'bg-green-500';
    case 'Warning':
      return 'bg-yellow-500';
    case 'Failed':
      return 'bg-red-500';
    default:
      return 'bg-gray-400';
  }
};

export function ReleaseCard({
  release,
  onEdit,
  onDeploy,
  onDelete,
  onViewNotes,
}: ReleaseCardProps) {
  const completionPercentage =
    release.totalTasksCount > 0
      ? (release.completedTasksCount / release.totalTasksCount) * 100
      : 0;

  const plannedDate = release.plannedDate
    ? format(new Date(release.plannedDate), 'MMM dd, yyyy')
    : 'TBD';

  const actualDate = release.actualReleaseDate
    ? format(new Date(release.actualReleaseDate), 'MMM dd, yyyy')
    : null;

  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-2">
            <Package className="h-5 w-5 text-muted-foreground" />
            <Badge className={getTypeColor(release.type)}>{release.type}</Badge>
            <span className="font-semibold text-lg">{release.version}</span>
          </div>
          <Badge className={getStatusColor(release.status)}>{release.status}</Badge>
        </div>
        <h3 className="font-semibold text-lg mt-2">{release.name}</h3>
        {release.description && (
          <p className="text-sm text-muted-foreground line-clamp-2">
            {release.description}
          </p>
        )}
      </CardHeader>

      <CardContent className="space-y-4">
        {/* Quality Gate Indicator */}
        <div className="flex items-center gap-2">
          <div
            className={cn(
              'h-3 w-3 rounded-full',
              getQualityGateColor(release.overallQualityStatus)
            )}
          />
          <span className="text-sm text-muted-foreground">
            {release.overallQualityStatus || 'Pending'}
          </span>
        </div>

        {/* Dates */}
        <div className="flex items-center gap-4 text-sm">
          <div className="flex items-center gap-1">
            <Calendar className="h-4 w-4 text-muted-foreground" />
            <span className="text-muted-foreground">
              {actualDate ? `Released: ${actualDate}` : `Planned: ${plannedDate}`}
            </span>
          </div>
        </div>

        {/* Sprint Count */}
        <div className="flex items-center gap-2">
          <Badge variant="outline">{release.sprintCount} sprints</Badge>
          {release.isPreRelease && (
            <Badge variant="outline" className="bg-yellow-100 text-yellow-800">
              Pre-release
            </Badge>
          )}
        </div>

        {/* Tasks Progress */}
        {release.totalTasksCount > 0 && (
          <div className="space-y-1">
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Tasks</span>
              <span className="font-medium">
                {release.completedTasksCount}/{release.totalTasksCount}
              </span>
            </div>
            <Progress value={completionPercentage} className="h-2" />
          </div>
        )}
      </CardContent>

      <CardFooter className="flex gap-2">
        {onViewNotes && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => onViewNotes(release)}
            className="flex-1"
          >
            <Eye className="h-4 w-4 mr-1" />
            Notes
          </Button>
        )}
        {onEdit && release.status !== 'Deployed' && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => onEdit(release)}
            className="flex-1"
          >
            <Edit className="h-4 w-4 mr-1" />
            Edit
          </Button>
        )}
        {onDeploy && release.status === 'ReadyForDeployment' && (
          <Button
            variant="default"
            size="sm"
            onClick={() => onDeploy(release)}
            className="flex-1"
          >
            <Rocket className="h-4 w-4 mr-1" />
            Deploy
          </Button>
        )}
        {onDelete && release.status !== 'Deployed' && (
          <Button
            variant="destructive"
            size="sm"
            onClick={() => onDelete(release)}
            className="flex-1"
          >
            <Trash2 className="h-4 w-4 mr-1" />
            Delete
          </Button>
        )}
      </CardFooter>
    </Card>
  );
}

