import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { milestonesApi } from '@/api/milestones';
import { MilestoneCard } from './MilestoneCard';
import { CreateMilestoneDialog } from './CreateMilestoneDialog';
import { EditMilestoneDialog } from './EditMilestoneDialog';
import { CompleteMilestoneDialog } from './CompleteMilestoneDialog';
import { Plus } from 'lucide-react';
import { MySwal } from '@/lib/sweetalert';
import { useProjectPermissions } from '@/hooks/useProjectPermissions';
import type { MilestoneDto } from '@/types/milestones';

interface MilestonesListProps {
  projectId: number;
  status?: string;
  includeCompleted?: boolean;
}

/**
 * List view component for displaying project milestones.
 * Supports filtering by status and toggling completed milestones.
 */
export function MilestonesList({
  projectId,
  status: initialStatus,
  includeCompleted: initialIncludeCompleted = false,
}: MilestonesListProps) {
  const [statusFilter, setStatusFilter] = useState<string | undefined>(initialStatus);
  const [includeCompleted, setIncludeCompleted] = useState(initialIncludeCompleted);
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [editingMilestone, setEditingMilestone] = useState<MilestoneDto | null>(null);
  const [completingMilestone, setCompletingMilestone] = useState<MilestoneDto | null>(null);

  const permissions = useProjectPermissions(projectId);
  const canCreate = permissions.canCreateMilestone;
  const canEdit = permissions.canEditMilestone;
  const canComplete = permissions.canCompleteMilestone;
  const canDelete = permissions.canDeleteMilestone;

  const { data: milestones, isLoading } = useQuery({
    queryKey: ['projectMilestones', projectId, statusFilter, includeCompleted],
    queryFn: () => milestonesApi.getProjectMilestones(projectId, statusFilter, includeCompleted),
    enabled: !!projectId,
  });

  const handleDelete = async (milestone: MilestoneDto) => {
    const confirmed = await MySwal.fire({
      title: 'Delete milestone?',
      text: `Are you sure you want to delete "${milestone.name}"? This action cannot be undone.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel',
      confirmButtonColor: '#ef4444',
    });

    if (confirmed.isConfirmed) {
      try {
        await milestonesApi.deleteMilestone(milestone.id);
        MySwal.fire({
          icon: 'success',
          title: 'Milestone deleted',
          timer: 2000,
          showConfirmButton: false,
        });
      } catch (error: unknown) {
        const apiError = error as { response?: { data?: { error?: string } }; message?: string };
        MySwal.fire({
          icon: 'error',
          title: 'Error',
          text: apiError?.response?.data?.error || apiError?.message || 'Failed to delete milestone',
        });
      }
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-32 w-full" />
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Filters and Actions */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Milestones</CardTitle>
              <CardDescription>Manage project milestones and deadlines</CardDescription>
            </div>
            {canCreate && (
              <Button onClick={() => setIsCreateDialogOpen(true)}>
                <Plus className="h-4 w-4 mr-2" />
                Create Milestone
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-4 flex-wrap">
            <div className="flex items-center gap-2">
              <Label htmlFor="status-filter" className="text-sm">Filter by status:</Label>
              <Select value={statusFilter || 'all'} onValueChange={(value) => setStatusFilter(value === 'all' ? undefined : value)}>
                <SelectTrigger id="status-filter" className="w-[180px]">
                  <SelectValue placeholder="All statuses" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Statuses</SelectItem>
                  <SelectItem value="Pending">Pending</SelectItem>
                  <SelectItem value="InProgress">In Progress</SelectItem>
                  <SelectItem value="Completed">Completed</SelectItem>
                  <SelectItem value="Missed">Missed</SelectItem>
                  <SelectItem value="Cancelled">Cancelled</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center gap-2">
              <Switch
                id="include-completed"
                checked={includeCompleted}
                onCheckedChange={setIncludeCompleted}
              />
              <Label htmlFor="include-completed" className="text-sm cursor-pointer">
                Show completed
              </Label>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Milestones List */}
      {milestones && milestones.length > 0 ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {milestones.map((milestone) => (
            <MilestoneCard
              key={milestone.id}
              milestone={milestone}
              onEdit={() => setEditingMilestone(milestone)}
              onComplete={() => setCompletingMilestone(milestone)}
              onDelete={() => handleDelete(milestone)}
              canEdit={canEdit}
              canComplete={canComplete}
              canDelete={canDelete}
            />
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <p className="text-muted-foreground mb-4">No milestones yet.</p>
            {canCreate && (
              <Button onClick={() => setIsCreateDialogOpen(true)}>
                <Plus className="h-4 w-4 mr-2" />
                Create First Milestone
              </Button>
            )}
          </CardContent>
        </Card>
      )}

      {/* Dialogs */}
      <CreateMilestoneDialog
        projectId={projectId}
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
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
  );
}

