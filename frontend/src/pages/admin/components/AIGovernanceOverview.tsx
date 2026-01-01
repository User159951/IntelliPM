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
            <div className="text-2xl font-bold">{stats?.totalDecisionsLast30Days || 0}</div>
            <p className="text-xs text-muted-foreground">
              Last 30 days
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Token Usage</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {stats?.topAgents?.reduce((sum, agent) => sum + agent.totalTokensUsed, 0).toLocaleString() || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              Total tokens used
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Active Organizations</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.organizationsWithAIEnabled || 0}</div>
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
              {stats?.quotaByTier?.reduce((sum, tier) => sum + tier.exceededCount, 0) || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              Quotas exceeded
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
            {stats?.topAgents && stats.topAgents.length > 0 ? (
              <div className="space-y-2">
                {stats.topAgents.map((agent) => (
                  <div key={agent.agentType} className="flex items-center justify-between">
                    <span className="text-sm">{agent.agentType}</span>
                    <div className="text-right">
                      <div className="text-sm font-medium">
                        {agent.totalTokensUsed.toLocaleString()} tokens
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {agent.decisionCount} decisions
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
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-sm">Approved</span>
                <span className="text-sm font-medium">{stats?.approvedDecisions || 0}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm">Rejected</span>
                <span className="text-sm font-medium">{stats?.rejectedDecisions || 0}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm">Pending</span>
                <span className="text-sm font-medium">{stats?.pendingApprovals || 0}</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Quota by tier */}
      {stats?.quotaByTier && stats.quotaByTier.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-destructive" />
              Quota Usage by Tier
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {stats.quotaByTier.map((tier) => (
                <div
                  key={tier.tierName}
                  className="flex items-center justify-between p-2 border rounded"
                >
                  <span className="text-sm font-medium">{tier.tierName}</span>
                  <div className="text-right">
                    <div className="text-sm font-medium">
                      {tier.averageUsagePercentage.toFixed(1)}% avg usage
                    </div>
                    {tier.exceededCount > 0 && (
                      <div className="text-xs text-destructive">
                        {tier.exceededCount} exceeded
                      </div>
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

