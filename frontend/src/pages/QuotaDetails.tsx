import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, Legend, BarChart, Bar } from 'recharts';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, AlertCircle, RefreshCw } from 'lucide-react';
import { aiGovernanceApi } from '@/api/aiGovernance';
import { cn } from '@/lib/utils';

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


export default function QuotaDetails() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isRefreshing, setIsRefreshing] = useState(false);

  const { data: quotaStatus, isLoading, error } = useQuery({
    queryKey: ['ai-quota-status'],
    queryFn: () => aiGovernanceApi.getQuotaStatus(),
    refetchInterval: 60000, // Rafraîchir toutes les minutes
  });

  const handleRefresh = async () => {
    setIsRefreshing(true);
    try {
      await queryClient.invalidateQueries({ queryKey: ['ai-quota'] });
      await queryClient.invalidateQueries({ queryKey: ['quota-status'] });
      await queryClient.invalidateQueries({ queryKey: ['user-quota'] });
      await queryClient.invalidateQueries({ queryKey: ['ai-quota-status'] });
    } finally {
      setIsRefreshing(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-64 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <AlertCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold mb-2">Erreur de Chargement</h3>
              <p className="text-muted-foreground">
                Impossible de charger les détails du quota AI.
              </p>
              <Button 
                variant="ghost"
                size="icon"
                onClick={handleRefresh}
                disabled={isRefreshing}
                aria-label="Refresh quota data"
                className="mt-4"
              >
                <RefreshCw className={cn('h-4 w-4', isRefreshing && 'animate-spin')} />
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!quotaStatus) {
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

  // Map backend structure to frontend display structure
  const status = quotaStatus;
  const usage = status.usage;
  
  // NOTE: Mock data for usage history and breakdown
  // TODO: Replace with real endpoints when available:
  // - GET /api/admin/ai-quota/usage-history?startDate=...&endDate=...
  // - GET /api/admin/ai-quota/breakdown?period=...
  // Generate mock history (30 days) - TODO: Replace with real endpoint when available
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

  // Generate mock breakdown - TODO: Replace with real endpoint when available
  const breakdownByAgent = [
    { agentType: 'Product', requests: 25, tokens: 15000, decisions: 10 },
    { agentType: 'QA', requests: 20, tokens: 12000, decisions: 8 },
    { agentType: 'Business', requests: 15, tokens: 10000, decisions: 7 },
    { agentType: 'Manager', requests: 10, tokens: 6000, decisions: 3 },
    { agentType: 'Delivery', requests: 5, tokens: 2000, decisions: 2 },
  ];

  const chartData = usageHistory.map((entry) => ({
    date: new Date(entry.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    requests: entry.requests,
    tokens: entry.tokens / 1000, // Convert to thousands for better display
    decisions: entry.decisions,
  }));


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
            Reset date: {new Date(status.periodEndDate).toLocaleDateString()} ({status.daysRemaining} days remaining)
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Requests */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="font-medium">Requests</span>
              <span className={cn(
                'text-sm',
                usage.requestsPercentage >= 80 ? 'text-red-500 font-semibold' :
                usage.requestsPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
              )}>
                {usage.requestsUsed.toLocaleString()} / {usage.requestsLimit.toLocaleString()}
                {' '}({usage.requestsPercentage.toFixed(1)}%)
              </span>
            </div>
            <Progress
              value={usage.requestsPercentage}
              className={cn('h-3', getPercentageColor(usage.requestsPercentage))}
            />
          </div>

          {/* Tokens */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="font-medium">Tokens</span>
              <span className={cn(
                'text-sm',
                usage.tokensPercentage >= 80 ? 'text-red-500 font-semibold' :
                usage.tokensPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
              )}>
                {usage.tokensUsed.toLocaleString()} / {usage.tokensLimit.toLocaleString()}
                {' '}({usage.tokensPercentage.toFixed(1)}%)
              </span>
            </div>
            <Progress
              value={usage.tokensPercentage}
              className={cn('h-3', getPercentageColor(usage.tokensPercentage))}
            />
          </div>

          {/* Cost */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="font-medium">Cost</span>
              <span className={cn(
                'text-sm',
                usage.costPercentage >= 80 ? 'text-red-500 font-semibold' :
                usage.costPercentage >= 50 ? 'text-yellow-500' : 'text-muted-foreground'
              )}>
                ${usage.costAccumulated.toFixed(2)} / ${usage.costLimit.toFixed(2)}
                {' '}({usage.costPercentage.toFixed(1)}%)
              </span>
            </div>
            <Progress
              value={usage.costPercentage}
              className={cn('h-3', getPercentageColor(usage.costPercentage))}
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

    </div>
  );
}

