import { useQuery } from '@tanstack/react-query';
import { aiGovernanceApi } from '@/api/aiGovernance';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertTriangle } from 'lucide-react';

function AIGovernanceOverviewSkeleton() {
  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {[...Array(4)].map((_, i) => (
          <Card key={i}>
            <CardHeader className="pb-2">
              <Skeleton className="h-4 w-24" />
            </CardHeader>
            <CardContent>
              <Skeleton className="h-8 w-16 mb-2" />
              <Skeleton className="h-3 w-32" />
            </CardContent>
          </Card>
        ))}
      </div>
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-64 mt-2" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-64 w-full" />
        </CardContent>
      </Card>
    </div>
  );
}

export function AIGovernanceOverview() {
  const { data: stats, isLoading } = useQuery({
    queryKey: ['ai-governance-stats'],
    queryFn: () => aiGovernanceApi.getOverviewStats(),
    staleTime: 1000 * 60, // 1 minute
  });

  if (isLoading) {
    return <AIGovernanceOverviewSkeleton />;
  }

  return (
    <div className="space-y-6">
      {/* Key metrics cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Total Decisions</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalDecisions || 0}</div>
            <p className="text-xs text-muted-foreground">
              {stats?.decisionsLast24h || 0} in last 24h
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Token Usage</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {(stats?.totalTokens || 0).toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground">
              {((stats?.tokensLast24h || 0) / 1000).toFixed(1)}K in last 24h
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Active Organizations</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.activeOrganizations || 0}</div>
            <p className="text-xs text-muted-foreground">
              {stats?.totalOrganizations || 0} total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Quotas Exceeded</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-destructive">
              {stats?.exceededQuotas || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              {stats?.alertsSent || 0} alerts sent
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Usage by agent type */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Usage by Agent Type</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.usageByAgent && Object.keys(stats.usageByAgent).length > 0 ? (
              <div className="space-y-2">
                {Object.entries(stats.usageByAgent).map(([agent, usage]) => (
                  <div key={agent} className="flex items-center justify-between">
                    <span className="text-sm">{agent}</span>
                    <div className="text-right">
                      <div className="text-sm font-medium">
                        {usage.tokensUsed.toLocaleString()} tokens
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {usage.requestsCount} requests
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No usage data available</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Decision Types Distribution</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.decisionTypes && Object.keys(stats.decisionTypes).length > 0 ? (
              <div className="space-y-2">
                {Object.entries(stats.decisionTypes).map(([type, count]) => (
                  <div key={type} className="flex items-center justify-between">
                    <span className="text-sm">{type}</span>
                    <span className="text-sm font-medium">{count}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No decision data available</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Organizations at risk */}
      {stats?.organizationsAtRisk && stats.organizationsAtRisk.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-destructive" />
              Organizations Requiring Attention
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {stats.organizationsAtRisk.map((org) => (
                <div
                  key={org.organizationId}
                  className="flex items-center justify-between p-2 border rounded"
                >
                  <span className="text-sm font-medium">{org.organizationName}</span>
                  <div className="text-right">
                    <div className="text-sm font-medium">
                      {org.quotaPercentage.toFixed(1)}% used
                    </div>
                    {org.isExceeded && (
                      <div className="text-xs text-destructive">Quota exceeded</div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

