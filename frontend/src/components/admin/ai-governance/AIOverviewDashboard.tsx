import { useQuery } from '@tanstack/react-query';
import { aiGovernanceApi } from '@/api/aiGovernance';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertCircle, Clock, TrendingUp } from 'lucide-react';
import {
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

const COLORS = ['#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6'];

function AIDashboardSkeleton() {
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
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-64 w-full" />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-64 w-full" />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export function AIOverviewDashboard() {
  const { data: stats, isLoading, error } = useQuery({
    queryKey: ['ai-governance', 'overview'],
    queryFn: () => aiGovernanceApi.getOverviewStats(),
    staleTime: 1000 * 60 * 5, // 5 minutes
    refetchOnWindowFocus: false,
  });

  if (isLoading) {
    return <AIDashboardSkeleton />;
  }

  if (error) {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          Failed to load AI governance statistics. Please try again later.
        </AlertDescription>
      </Alert>
    );
  }

  if (!stats) {
    return null;
  }

  // Prepare data for pie chart (Organizations by AI status)
  const orgStatusData = [
    { name: 'AI Enabled', value: stats.organizationsWithAIEnabled, color: COLORS[2] },
    { name: 'AI Disabled', value: stats.organizationsWithAIDisabled, color: COLORS[1] },
  ];

  // Prepare data for bar chart (Top agents)
  const topAgentsData = stats.topAgents.map((agent) => ({
    name: agent.agentType.replace('Agent', ''),
    decisions: agent.decisionCount,
    tokens: Math.round(agent.totalTokensUsed / 1000), // Convert to K
  }));

  // Prepare data for quota by tier
  const quotaByTierData = stats.quotaByTier.map((tier) => ({
    name: tier.tierName,
    organizations: tier.organizationCount,
    avgUsage: Math.round(tier.averageUsagePercentage),
    exceeded: tier.exceededCount,
  }));

  const RADIAN = Math.PI / 180;
  interface PieLabelProps {
    cx: number;
    cy: number;
    midAngle: number;
    innerRadius: number;
    outerRadius: number;
    percent: number;
  }
  const renderCustomLabel = ({
    cx,
    cy,
    midAngle,
    innerRadius,
    outerRadius,
    percent,
  }: PieLabelProps) => {
    const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
    const x = cx + radius * Math.cos(-midAngle * RADIAN);
    const y = cy + radius * Math.sin(-midAngle * RADIAN);

    return (
      <text
        x={x}
        y={y}
        fill="white"
        textAnchor={x > cx ? 'start' : 'end'}
        dominantBaseline="central"
        fontSize={12}
        fontWeight="bold"
      >
        {`${(percent * 100).toFixed(0)}%`}
      </text>
    );
  };

  return (
    <div className="space-y-6" role="main" aria-label="AI Governance Overview Dashboard">
      {/* Key Metrics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Total Organizations</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalOrganizations}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {stats.organizationsWithAIEnabled} with AI enabled
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2 flex flex-row items-center justify-between">
            <CardTitle className="text-sm font-medium">Pending Approvals</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold flex items-center gap-2">
              {stats.pendingApprovals}
              {stats.pendingApprovals > 0 && (
                <span className="text-xs font-normal text-destructive">Requires attention</span>
              )}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {stats.approvedDecisions} approved, {stats.rejectedDecisions} rejected
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Decisions (30 days)</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalDecisionsLast30Days}</div>
            <p className="text-xs text-muted-foreground mt-1">
              Avg confidence: {(stats.averageConfidenceScore * 100).toFixed(1)}%
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2 flex flex-row items-center justify-between">
            <CardTitle className="text-sm font-medium">Average Confidence</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {(stats.averageConfidenceScore * 100).toFixed(1)}%
            </div>
            <div className="mt-2 w-full bg-secondary rounded-full h-2">
              <div
                className="bg-primary h-2 rounded-full transition-all"
                style={{ width: `${stats.averageConfidenceScore * 100}%` }}
                role="progressbar"
                aria-valuenow={stats.averageConfidenceScore * 100}
                aria-valuemin={0}
                aria-valuemax={100}
                aria-label="Average confidence score"
              />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Organizations by AI Status - Pie Chart */}
        <Card>
          <CardHeader>
            <CardTitle>Organizations by AI Status</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={orgStatusData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={renderCustomLabel}
                  outerRadius={100}
                  fill="#8884d8"
                  dataKey="value"
                  aria-label="Organization AI status distribution"
                >
                  {orgStatusData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend
                  formatter={(value) => {
                    const item = orgStatusData.find((d) => d.name === value);
                    return `${value} (${item?.value || 0})`;
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Top Agents - Bar Chart */}
        <Card>
          <CardHeader>
            <CardTitle>Top 5 Agents by Usage</CardTitle>
          </CardHeader>
          <CardContent>
            {topAgentsData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={topAgentsData} aria-label="Top agents by decision count">
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="name"
                    angle={-45}
                    textAnchor="end"
                    height={80}
                    tick={{ fontSize: 12 }}
                  />
                  <YAxis tick={{ fontSize: 12 }} />
                  <Tooltip
                    formatter={(value: number, name: string) => {
                      if (name === 'tokens') {
                        return [`${value}K tokens`, 'Tokens Used'];
                      }
                      return [value, 'Decisions'];
                    }}
                  />
                  <Legend />
                  <Bar dataKey="decisions" fill={COLORS[0]} name="Decisions" />
                  <Bar dataKey="tokens" fill={COLORS[3]} name="Tokens (K)" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="flex items-center justify-center h-[300px] text-muted-foreground">
                No agent usage data available
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Quota by Tier */}
      <Card>
        <CardHeader>
          <CardTitle>Quota Usage by Tier</CardTitle>
        </CardHeader>
        <CardContent>
          {quotaByTierData.length > 0 ? (
            <div className="space-y-4">
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={quotaByTierData} aria-label="Quota usage by tier">
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                  <YAxis yAxisId="left" tick={{ fontSize: 12 }} />
                  <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12 }} />
                  <Tooltip />
                  <Legend />
                  <Bar
                    yAxisId="left"
                    dataKey="organizations"
                    fill={COLORS[0]}
                    name="Organizations"
                  />
                  <Bar
                    yAxisId="right"
                    dataKey="avgUsage"
                    fill={COLORS[2]}
                    name="Avg Usage %"
                  />
                  <Bar yAxisId="left" dataKey="exceeded" fill={COLORS[1]} name="Exceeded" />
                </BarChart>
              </ResponsiveContainer>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
                {quotaByTierData.map((tier) => (
                  <div
                    key={tier.name}
                    className="border rounded-lg p-4"
                    role="region"
                    aria-label={`${tier.name} tier statistics`}
                  >
                    <div className="font-semibold text-lg">{tier.name}</div>
                    <div className="text-sm text-muted-foreground mt-2">
                      <div>Organizations: {tier.organizations}</div>
                      <div>Avg Usage: {tier.avgUsage}%</div>
                      {tier.exceeded > 0 && (
                        <div className="text-destructive mt-1">
                          Exceeded: {tier.exceeded}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className="flex items-center justify-center h-[300px] text-muted-foreground">
              No quota data available
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

