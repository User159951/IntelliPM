import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { releasesApi } from '@/api/releases';

interface BlockedReleasesWidgetProps {
  projectId: number;
  className?: string;
}

/**
 * Widget displaying releases blocked by failed quality gates.
 * Shows releases that cannot proceed due to quality gate failures.
 */
export function BlockedReleasesWidget({
  projectId,
  className,
}: BlockedReleasesWidgetProps) {
  const navigate = useNavigate();

  const { data: releases, isLoading } = useQuery({
    queryKey: ['project-releases', projectId],
    queryFn: () => releasesApi.getProjectReleases(projectId),
    enabled: !!projectId,
  });

  const blockedReleases = useMemo(() => {
    if (!releases) return [];
    return releases.filter(
      (r) =>
        r.overallQualityStatus === 'Failed' &&
        (r.status === 'InProgress' || r.status === 'Testing')
    );
  }, [releases]);

  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle className="text-lg">Blocked Releases</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Loading...</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">Blocked Releases</CardTitle>
          <Badge variant="destructive">{blockedReleases.length}</Badge>
        </div>
      </CardHeader>
      <CardContent>
        {blockedReleases.length === 0 ? (
          <p className="text-sm text-muted-foreground">No blocked releases</p>
        ) : (
          <div className="space-y-2">
            {blockedReleases.map((release) => (
              <div
                key={release.id}
                className="flex items-center justify-between p-3 rounded-lg border border-destructive/50 bg-destructive/5"
              >
                <div>
                  <p className="font-medium text-sm">{release.version}</p>
                  <p className="text-xs text-muted-foreground">{release.name}</p>
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() =>
                    navigate(`/projects/${projectId}/releases/${release.id}`)
                  }
                >
                  View
                </Button>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

