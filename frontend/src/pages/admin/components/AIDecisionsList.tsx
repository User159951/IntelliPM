import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { aiGovernanceApi } from '@/api/aiGovernance';
import type { AIDecisionType, AIAgentType } from '@/types/generated/enums';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Brain } from 'lucide-react';
import { format } from 'date-fns';
import { Badge } from '@/components/ui/badge';
import * as React from 'react';

function EmptyState({
  icon: Icon,
  message,
  description,
}: {
  icon: React.ComponentType<{ className?: string }>;
  message: string;
  description?: string;
}) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <Icon className="h-12 w-12 text-muted-foreground mb-4" />
      <h3 className="text-lg font-semibold">{message}</h3>
      {description && <p className="text-sm text-muted-foreground mt-2">{description}</p>}
    </div>
  );
}

export function AIDecisionsList() {
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<{
    organizationId?: number;
    decisionType: 'all' | AIDecisionType;
    agentType: 'all' | AIAgentType;
    startDate?: string;
    endDate?: string;
  }>({
    organizationId: undefined,
    decisionType: 'all',
    agentType: 'all',
    startDate: undefined,
    endDate: undefined,
  });

  const { data, isLoading } = useQuery({
    queryKey: ['ai-decisions-admin', page, filters],
    queryFn: () =>
      aiGovernanceApi.getAllDecisions({
        page,
        pageSize: 20,
        ...(filters.decisionType !== 'all' && { decisionType: filters.decisionType }),
        ...(filters.agentType !== 'all' && { agentType: filters.agentType }),
        ...(filters.organizationId && { organizationId: filters.organizationId }),
        ...(filters.startDate && { startDate: filters.startDate }),
        ...(filters.endDate && { endDate: filters.endDate }),
      }),
    staleTime: 1000 * 30,
  });

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>AI Decisions</CardTitle>
            <CardDescription>Recent AI decisions across all organizations</CardDescription>
          </div>
        </div>
      </CardHeader>

      <CardContent className="space-y-4">
        {/* Filters */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Select
            value={filters.decisionType}
            onValueChange={(value) => setFilters({ ...filters, decisionType: value as 'all' | AIDecisionType })}
          >
            <SelectTrigger>
              <SelectValue placeholder="Decision Type" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Types</SelectItem>
              <SelectItem value="RiskDetection">Risk Detection</SelectItem>
              <SelectItem value="SprintPlanning">Sprint Planning</SelectItem>
              <SelectItem value="TaskPrioritization">Task Prioritization</SelectItem>
            </SelectContent>
          </Select>

          <Select
            value={filters.agentType}
            onValueChange={(value) => setFilters({ ...filters, agentType: value as 'all' | AIAgentType })}
          >
            <SelectTrigger>
              <SelectValue placeholder="Agent Type" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Agents</SelectItem>
              <SelectItem value="ProductAgent">Product Agent</SelectItem>
              <SelectItem value="DeliveryAgent">Delivery Agent</SelectItem>
              <SelectItem value="ManagerAgent">Manager Agent</SelectItem>
              <SelectItem value="QAAgent">QA Agent</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {isLoading ? (
          <div className="space-y-2">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-20" />
            ))}
          </div>
        ) : data && data.items.length > 0 ? (
          <>
            <div className="space-y-4">
              {data.items.map((decision) => (
                <div
                  key={decision.decisionId}
                  className="border rounded-lg p-4 space-y-2 hover:bg-accent/50 transition-colors"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <Badge variant="outline">{decision.decisionType}</Badge>
                        <Badge variant="secondary">{decision.agentType}</Badge>
                        <Badge
                          variant={
                            decision.status === 'Applied'
                              ? 'default'
                              : decision.status === 'Rejected'
                              ? 'destructive'
                              : 'secondary'
                          }
                        >
                          {decision.status}
                        </Badge>
                      </div>
                      <h4 className="font-medium">{decision.question}</h4>
                      <p className="text-sm text-muted-foreground line-clamp-2">
                        {decision.decision}
                      </p>
                      <div className="flex items-center gap-4 mt-2 text-xs text-muted-foreground">
                        <span>{decision.entityType}: {decision.entityName}</span>
                        <span>Confidence: {(decision.confidenceScore * 100).toFixed(0)}%</span>
                        <span>{decision.tokensUsed.toLocaleString()} tokens</span>
                        <span>{format(new Date(decision.createdAt), 'MMM d, yyyy HH:mm')}</span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <div className="flex items-center justify-between pt-4">
              <p className="text-sm text-muted-foreground">
                Showing {(page - 1) * 20 + 1} to {Math.min(page * 20, data.totalCount)} of{' '}
                {data.totalCount} decisions
              </p>
              <div className="flex items-center gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <span className="text-sm">
                  Page {page} of {data.totalPages}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                  disabled={page === data.totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          </>
        ) : (
          <EmptyState
            icon={Brain}
            message="No AI decisions found"
            description="Try adjusting your filters"
          />
        )}
      </CardContent>
    </Card>
  );
}

