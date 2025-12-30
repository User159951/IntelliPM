import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Search, Grid, List, Plus } from 'lucide-react';
import { releasesApi } from '@/api/releases';
import { ReleaseCard } from './ReleaseCard';
import type { ReleaseDto } from '@/types/releases';
import { Card, CardContent } from '@/components/ui/card';

interface ReleasesListProps {
  projectId: number;
  status?: string;
  onCreateRelease?: () => void;
  onEditRelease?: (release: ReleaseDto) => void;
  onDeployRelease?: (release: ReleaseDto) => void;
  onDeleteRelease?: (release: ReleaseDto) => void;
  onViewNotes?: (release: ReleaseDto) => void;
}

export function ReleasesList({
  projectId,
  status: initialStatus,
  onCreateRelease,
  onEditRelease,
  onDeployRelease,
  onDeleteRelease,
  onViewNotes,
}: ReleasesListProps) {
  const [statusFilter, setStatusFilter] = useState<string>(initialStatus || 'all');
  const [searchQuery, setSearchQuery] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [sortBy, setSortBy] = useState<'version' | 'date' | 'name'>('date');

  const { data: releases, isLoading } = useQuery({
    queryKey: ['projectReleases', projectId, statusFilter === 'all' ? undefined : statusFilter],
    queryFn: () => releasesApi.getProjectReleases(projectId, statusFilter === 'all' ? undefined : statusFilter),
  });

  const filteredReleases = releases
    ?.filter((release) => {
      if (!searchQuery) return true;
      const query = searchQuery.toLowerCase();
      return (
        release.name.toLowerCase().includes(query) ||
        release.version.toLowerCase().includes(query)
      );
    })
    .sort((a, b) => {
      switch (sortBy) {
        case 'version':
          return b.version.localeCompare(a.version);
        case 'name':
          return a.name.localeCompare(b.name);
        case 'date':
        default:
          return new Date(b.plannedDate).getTime() - new Date(a.plannedDate).getTime();
      }
    }) || [];

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {[1, 2, 3].map((i) => (
          <Card key={i}>
            <CardContent className="p-6">
              <Skeleton className="h-6 w-32 mb-4" />
              <Skeleton className="h-4 w-full mb-2" />
              <Skeleton className="h-4 w-3/4" />
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Filters and Actions */}
      <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
        <div className="flex flex-1 gap-2 items-center">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search releases..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-8"
            />
          </div>
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Filter by status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Releases</SelectItem>
              <SelectItem value="Planned">Planned</SelectItem>
              <SelectItem value="InProgress">In Progress</SelectItem>
              <SelectItem value="Testing">Testing</SelectItem>
              <SelectItem value="ReadyForDeployment">Ready for Deployment</SelectItem>
              <SelectItem value="Deployed">Deployed</SelectItem>
              <SelectItem value="Failed">Failed</SelectItem>
            </SelectContent>
          </Select>
          <Select value={sortBy} onValueChange={(v) => setSortBy(v as any)}>
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="Sort by" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="date">Date (Newest)</SelectItem>
              <SelectItem value="version">Version</SelectItem>
              <SelectItem value="name">Name</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div className="flex gap-2">
          <div className="flex border rounded-md">
            <Button
              variant={viewMode === 'grid' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => setViewMode('grid')}
              className="rounded-r-none"
            >
              <Grid className="h-4 w-4" />
            </Button>
            <Button
              variant={viewMode === 'list' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => setViewMode('list')}
              className="rounded-l-none"
            >
              <List className="h-4 w-4" />
            </Button>
          </div>
          {onCreateRelease && (
            <Button onClick={onCreateRelease}>
              <Plus className="h-4 w-4 mr-2" />
              Create Release
            </Button>
          )}
        </div>
      </div>

      {/* Releases Grid/List */}
      {filteredReleases.length === 0 ? (
        <Card>
          <CardContent className="p-12 text-center">
            <p className="text-muted-foreground mb-4">
              {searchQuery || statusFilter !== 'all'
                ? 'No releases match your filters.'
                : 'No releases yet. Create your first release!'}
            </p>
            {onCreateRelease && (
              <Button onClick={onCreateRelease}>
                <Plus className="h-4 w-4 mr-2" />
                Create Release
              </Button>
            )}
          </CardContent>
        </Card>
      ) : (
        <div
          className={
            viewMode === 'grid'
              ? 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4'
              : 'space-y-4'
          }
        >
          {filteredReleases.map((release) => (
            <ReleaseCard
              key={release.id}
              release={release}
              onEdit={onEditRelease}
              onDeploy={onDeployRelease}
              onDelete={onDeleteRelease}
              onViewNotes={onViewNotes}
            />
          ))}
        </div>
      )}
    </div>
  );
}

