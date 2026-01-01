import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, Legend, BarChart, Bar } from 'recharts';
import { useAuth } from '@/contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Calendar, TrendingUp, Zap, AlertCircle, ArrowLeft } from 'lucide-react';
import type { QuotaDetails, QuotaStatus } from '@/types/aiGovernance';
import { cn } from '@/lib/utils';
import { apiClient } from '@/api/client';

// Mock API calls - will be replaced with actual endpoints when available
async function getQuotaDetails(organizationId: number): Promise<QuotaDetails> {
  // Temporary mock implementation
  const status: QuotaStatus = {
    organizationId,
    tierName: 'Free',
    maxRequests: 100,
    maxTokens: 100000,
    maxDecisions: 50,
    currentRequests: 75,
    currentTokens: 45000,
    currentDecisions: 30,
    resetDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
    isAlertThreshold: true,
    isDisabled: false,
    requestsPercentage: 75,
    tokensPercentage: 45,
    decisionsPercentage: 60,
  };

  // Generate mock history (30 days)
  const usageHistory = Array.from({ length: 30 }, (_, i) => {
    const date = new Date();
    date.setDate(date.getDate() - (29 - i));
    return {
      date: date.toISOString().split('T')[0],
      requests: Math.floor(Math.random() * 5) + 1,
      tokens: Math.floor(Math.random() * 5000) + 1000,
      decisions: Math.floor(Math.random() * 2),
    };
  });

  const breakdownByAgent = [
    { agentType: 'Product', requests: 25, tokens: 15000, decisions: 10 },
    { agentType: 'QA', requests: 20, tokens: 12000, decisions: 8 },
    { agentType: 'Business', requests: 15, tokens: 10000, decisions: 7 },
    { agentType: 'Manager', requests: 10, tokens: 6000, decisions: 3 },
    { agentType: 'Delivery', requests: 5, tokens: 2000, decisions: 2 },
  ];

  return {
    status,
    usageHistory,
    breakdownByAgent,
  };
}

function getPercentageColor(percentage: number): string {
  if (percentage >= 80) return 'bg-red-500';
  if (percentage >= 50) return 'bg-yellow-500';
  return 'bg-green-500';
}

function getTierBadgeClass(tierName: string): string {
  switch (tierName.toLowerCase()) {
    case 'enterprise':
      return 'bg-purple-500/10 text-purple-500 border-purple-500/20';
    case 'pro':
      return 'bg-blue-500/10 text-blue-500 border-blue-500/20';
    case 'free':
      return 'bg-gray-500/10 text-gray-500 border-gray-500/20';
    default:
      return 'bg-red-500/10 text-red-500 border-red-500/20';
  }
}

const TIER_COMPARISON = [
  {
    tier: 'Free',
    requests: 100,
    tokens: 100000,
    decisions: 50,
    features: ['Basic AI agents', 'Standard support'],
  },
  {
    tier: 'Pro',
    requests: 1000,
    tokens: 1000000,
    decisions: 500,
    features: ['All AI agents', 'Priority support', 'Advanced analytics'],
  },
  {
    tier: 'Enterprise',
    requests: 10000,
    tokens: 10000000,
    decisions: 5000,
    features: ['All AI agents', 'Dedicated support', 'Custom integrations', 'SLA guarantee'],
  },
];

export default function QuotaDetails() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const organizationId = user?.organizationId || 0;

  const { data: quotaDetails, isLoading } = useQuery({
    queryKey: ['ai-quota-details', organizationId],
    queryFn: () => getQuotaDetails(organizationId),
    enabled: organizationId > 0,
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-64 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (!quotaDetails) {
    return (
      <div className="space-y-6">
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground">No quota data available</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const { status, usageHistory, breakdownByAgent } = quotaDetails;

  const chartData = usageHistory.map((entry) => ({
    date: new Date(entry.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    requests: entry.requests,
    tokens: entry.tokens / 1000, // Convert to thousands for better display
    decisions: entry.decisions,
  }));

  const canUpgrade = status.tierName === 'Free' || status.tierName === 'Pro';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <div>
            <h1 className="text-2xl font-bold text-foreground">AI Quota Details</h1>
            <p className="text-muted-foreground">Monitor your AI usage and quota limits</p>
          </div>
        </div>
        {canUpgrade && (
          <Button onClick={() => navigate('/settings/billing')}>
            Upgrade Plan
          </Button>
        )}
      </div>

      {/* Current Status */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Current Quota Status</CardTitle>
            <Badge variant="outline" className={getTierBadgeClass(status.tierName)}>
              {status.tierName}
            </Badge>
          </div>
          <CardDescription>
            Reset date: {new Date(status.resetDate).toLocaleDateString()}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Requests */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="font-medium">Requests</span>
              <span className={cn(
                'text-sm',
                status.requestsPercentage >= 80 ? 'text-red-500 font-semibold' :
                status.requestsPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
              )}>
                {status.currentRequests.toLocaleString()} / {status.maxRequests.toLocaleString()}
                {' '}({status.requestsPercentage.toFixed(0)}%)
              </span>
            </div>
            <Progress
              value={status.requestsPercentage}
              className={cn('h-3', getPercentageColor(status.requestsPercentage))}
            />
          </div>

          {/* Tokens */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="font-medium">Tokens</span>
              <span className={cn(
                'text-sm',
                status.tokensPercentage >= 80 ? 'text-red-500 font-semibold' :
                status.tokensPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
              )}>
                {status.currentTokens.toLocaleString()} / {status.maxTokens.toLocaleString()}
                {' '}({status.tokensPercentage.toFixed(0)}%)
              </span>
            </div>
            <Progress
              value={status.tokensPercentage}
              className={cn('h-3', getPercentageColor(status.tokensPercentage))}
            />
          </div>

          {/* Decisions */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="font-medium">Decisions</span>
              <span className={cn(
                'text-sm',
                status.decisionsPercentage >= 80 ? 'text-red-500 font-semibold' :
                status.decisionsPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
              )}>
                {status.currentDecisions.toLocaleString()} / {status.maxDecisions.toLocaleString()}
                {' '}({status.decisionsPercentage.toFixed(0)}%)
              </span>
            </div>
            <Progress
              value={status.decisionsPercentage}
              className={cn('h-3', getPercentageColor(status.decisionsPercentage))}
            />
          </div>
        </CardContent>
      </Card>

      {/* Usage History Chart */}
      <Card>
        <CardHeader>
          <CardTitle>Usage History (Last 30 Days)</CardTitle>
          <CardDescription>Daily usage breakdown</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis
                dataKey="date"
                tick={{ fontSize: 12 }}
                className="stroke-muted-foreground"
                angle={-45}
                textAnchor="end"
                height={80}
              />
              <YAxis tick={{ fontSize: 12 }} className="stroke-muted-foreground" />
              <RechartsTooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="requests"
                stroke="hsl(var(--primary))"
                strokeWidth={2}
                dot={{ r: 3 }}
                name="Requests"
              />
              <Line
                type="monotone"
                dataKey="tokens"
                stroke="#8b5cf6"
                strokeWidth={2}
                dot={{ r: 3 }}
                name="Tokens (thousands)"
              />
              <Line
                type="monotone"
                dataKey="decisions"
                stroke="#10b981"
                strokeWidth={2}
                dot={{ r: 3 }}
                name="Decisions"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Breakdown by Agent */}
      <Card>
        <CardHeader>
          <CardTitle>Usage by Agent Type</CardTitle>
          <CardDescription>Breakdown of usage across different AI agents</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={breakdownByAgent} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis
                dataKey="agentType"
                tick={{ fontSize: 12 }}
                className="stroke-muted-foreground"
              />
              <YAxis tick={{ fontSize: 12 }} className="stroke-muted-foreground" />
              <RechartsTooltip />
              <Legend />
              <Bar dataKey="requests" fill="hsl(var(--primary))" name="Requests" />
              <Bar dataKey="tokens" fill="#8b5cf6" name="Tokens (thousands)" />
              <Bar dataKey="decisions" fill="#10b981" name="Decisions" />
            </BarChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Tier Comparison */}
      <Card>
        <CardHeader>
          <CardTitle>Plan Comparison</CardTitle>
          <CardDescription>Compare features across different tiers</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            {TIER_COMPARISON.map((tier) => (
              <Card
                key={tier.tier}
                className={cn(
                  'relative',
                  tier.tier === status.tierName && 'border-primary ring-2 ring-primary'
                )}
              >
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">{tier.tier}</CardTitle>
                    {tier.tier === status.tierName && (
                      <Badge variant="outline">Current</Badge>
                    )}
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-2">
                    <div className="text-sm">
                      <span className="font-medium">Requests:</span>{' '}
                      {tier.requests.toLocaleString()}/month
                    </div>
                    <div className="text-sm">
                      <span className="font-medium">Tokens:</span>{' '}
                      {(tier.tokens / 1000).toLocaleString()}K/month
                    </div>
                    <div className="text-sm">
                      <span className="font-medium">Decisions:</span>{' '}
                      {tier.decisions.toLocaleString()}/month
                    </div>
                  </div>
                  <div className="space-y-2">
                    <div className="text-sm font-medium">Features:</div>
                    <ul className="space-y-1 text-sm text-muted-foreground">
                      {tier.features.map((feature, idx) => (
                        <li key={idx} className="flex items-start gap-2">
                          <span className="text-primary mt-0.5">•</span>
                          <span>{feature}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                  {tier.tier !== status.tierName && (
                    <Button
                      variant="outline"
                      className="w-full"
                      onClick={() => navigate('/settings/billing')}
                    >
                      {tier.tier === 'Free' ? 'Downgrade' : 'Upgrade'}
                    </Button>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

