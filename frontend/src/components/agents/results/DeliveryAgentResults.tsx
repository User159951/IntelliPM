import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import type { DeliveryAgentOutput, DeliveryMilestone, DeliveryRisk, DeliveryActionItem } from '@/types/agents';
import { Calendar, AlertTriangle, CheckCircle2, Clock, XCircle } from 'lucide-react';
import { cn } from '@/lib/utils';

interface DeliveryAgentResultsProps {
  output: DeliveryAgentOutput;
}

const MILESTONE_STATUS_COLORS: Record<string, string> = {
  'on-track': 'bg-green-500/10 text-green-500 border-green-500/20',
  'at-risk': 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20',
  'delayed': 'bg-red-500/10 text-red-500 border-red-500/20',
};

const RISK_SEVERITY_COLORS: Record<string, string> = {
  critical: 'bg-red-500/10 text-red-500 border-red-500/20',
  high: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
  medium: 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20',
  low: 'bg-gray-500/10 text-gray-500 border-gray-500/20',
};

const PRIORITY_COLORS: Record<string, string> = {
  critical: 'bg-red-500/10 text-red-500 border-red-500/20',
  high: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
  medium: 'bg-blue-500/10 text-blue-500 border-blue-500/20',
  low: 'bg-gray-500/10 text-gray-500 border-gray-500/20',
};

export function DeliveryAgentResults({ output }: DeliveryAgentResultsProps) {
  return (
    <div className="space-y-4" role="region" aria-label="Delivery Agent Results">
      {output.summary && (
        <Card>
          <CardHeader>
            <CardTitle>Summary</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">{output.summary}</p>
          </CardContent>
        </Card>
      )}

      {output.milestones.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Delivery Milestones</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {output.milestones.map((milestone) => (
                <MilestoneCard key={milestone.id} milestone={milestone} />
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {output.risks.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-yellow-500" />
              Risks ({output.risks.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              {output.risks.map((risk) => (
                <RiskCard key={risk.id} risk={risk} />
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {output.actionItems.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Action Items</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {output.actionItems
                .sort((a, b) => {
                  const priorityOrder = { critical: 4, high: 3, medium: 2, low: 1 };
                  return priorityOrder[b.priority] - priorityOrder[a.priority];
                })
                .map((item) => (
                  <ActionItemCard key={item.id} item={item} />
                ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function MilestoneCard({ milestone }: { milestone: DeliveryMilestone }) {
  const getStatusIcon = () => {
    switch (milestone.status) {
      case 'on-track':
        return <CheckCircle2 className="h-4 w-4 text-green-500" />;
      case 'at-risk':
        return <Clock className="h-4 w-4 text-yellow-500" />;
      case 'delayed':
        return <XCircle className="h-4 w-4 text-red-500" />;
    }
  };

  return (
    <div className="p-4 rounded-lg border transition-all hover:shadow-md">
      <div className="flex items-start justify-between gap-4 mb-3">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            {getStatusIcon()}
            <h4 className="font-semibold">{milestone.name}</h4>
          </div>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Calendar className="h-4 w-4" />
            <span>Target: {new Date(milestone.targetDate).toLocaleDateString()}</span>
          </div>
        </div>
        <Badge variant="outline" className={cn(MILESTONE_STATUS_COLORS[milestone.status])}>
          {milestone.status.replace('-', ' ').toUpperCase()}
        </Badge>
      </div>
      <div className="space-y-2">
        <div className="flex items-center justify-between text-xs text-muted-foreground">
          <span>Progress</span>
          <span>{milestone.progress}%</span>
        </div>
        <Progress value={milestone.progress} className="h-2" />
      </div>
    </div>
  );
}

function RiskCard({ risk }: { risk: DeliveryRisk }) {
  return (
    <Card className="transition-all hover:shadow-md">
      <CardHeader>
        <div className="flex items-start justify-between gap-4">
          <CardTitle className="text-base">{risk.description}</CardTitle>
          <Badge variant="outline" className={cn(RISK_SEVERITY_COLORS[risk.severity])}>
            {risk.severity.toUpperCase()}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        <div>
          <div className="text-xs font-medium text-muted-foreground mb-1">Impact</div>
          <p className="text-sm">{risk.impact}</p>
        </div>
        <div>
          <div className="text-xs font-medium text-muted-foreground mb-1">Mitigation</div>
          <p className="text-sm">{risk.mitigation}</p>
        </div>
      </CardContent>
    </Card>
  );
}

function ActionItemCard({ item }: { item: DeliveryActionItem }) {
  return (
    <div className="flex items-start gap-3 p-3 rounded-lg border transition-colors hover:bg-muted/50">
      <Badge variant="outline" className={cn('mt-0.5', PRIORITY_COLORS[item.priority])}>
        {item.priority.toUpperCase()}
      </Badge>
      <div className="flex-1">
        <p className="text-sm font-medium">{item.action}</p>
        {(item.owner || item.dueDate) && (
          <div className="flex items-center gap-4 mt-2 text-xs text-muted-foreground">
            {item.owner && <span>Owner: {item.owner}</span>}
            {item.dueDate && (
              <span className="flex items-center gap-1">
                <Calendar className="h-3 w-3" />
                Due: {new Date(item.dueDate).toLocaleDateString()}
              </span>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

