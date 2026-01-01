import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, Cell } from 'recharts';
import type { QAAgentOutput, DefectPattern } from '@/types/agents';
import { cn } from '@/lib/utils';

interface QAAgentResultsProps {
  output: QAAgentOutput;
}

const SEVERITY_COLORS: Record<string, string> = {
  critical: '#ef4444', // red-500
  high: '#f97316', // orange-500
  medium: '#eab308', // yellow-500
  low: '#6b7280', // gray-500
};

const SEVERITY_BADGE_CLASSES: Record<string, string> = {
  critical: 'bg-red-500/10 text-red-500 border-red-500/20',
  high: 'bg-orange-500/10 text-orange-500 border-orange-500/20',
  medium: 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20',
  low: 'bg-gray-500/10 text-gray-500 border-gray-500/20',
};

export function QAAgentResults({ output }: QAAgentResultsProps) {
  const chartData = output.patterns.map((pattern) => ({
    name: pattern.pattern.length > 20 ? pattern.pattern.substring(0, 20) + '...' : pattern.pattern,
    fullName: pattern.pattern,
    frequency: pattern.frequency,
    severity: pattern.severity,
  }));

  return (
    <div className="space-y-4" role="region" aria-label="QA Agent Results">
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

      {typeof output.overallQuality === 'number' && (
        <Card>
          <CardHeader>
            <CardTitle>Overall Quality Score</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-4">
              <div className="text-4xl font-bold">
                {(output.overallQuality * 100).toFixed(0)}%
              </div>
              <div className="flex-1">
                <div className="h-4 w-full bg-muted rounded-full overflow-hidden">
                  <div
                    className="h-full bg-primary transition-all duration-500"
                    style={{ width: `${output.overallQuality * 100}%` }}
                  />
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Defect Pattern Frequency</CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 60 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis
                dataKey="name"
                angle={-45}
                textAnchor="end"
                height={100}
                tick={{ fontSize: 12 }}
                className="stroke-muted-foreground"
              />
              <YAxis tick={{ fontSize: 12 }} className="stroke-muted-foreground" />
              <RechartsTooltip
                content={({ active, payload }) => {
                  if (active && payload && payload.length) {
                    const data = payload[0].payload;
                    return (
                      <div className="rounded-lg border bg-background p-3 shadow-md">
                        <p className="font-medium">{data.fullName}</p>
                        <p className="text-sm text-muted-foreground">
                          Frequency: {data.frequency}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          Severity: {data.severity}
                        </p>
                      </div>
                    );
                  }
                  return null;
                }}
              />
              <Bar dataKey="frequency" radius={[4, 4, 0, 0]}>
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={SEVERITY_COLORS[entry.severity]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      <div className="grid gap-4 md:grid-cols-2">
        {output.patterns.map((pattern, index) => (
          <PatternCard key={index} pattern={pattern} />
        ))}
      </div>
    </div>
  );
}

function PatternCard({ pattern }: { pattern: DefectPattern }) {
  return (
    <Card className="transition-all hover:shadow-md">
      <CardHeader>
        <div className="flex items-start justify-between gap-4">
          <CardTitle className="text-lg">{pattern.pattern}</CardTitle>
          <Badge
            variant="outline"
            className={cn(SEVERITY_BADGE_CLASSES[pattern.severity])}
          >
            {pattern.severity.toUpperCase()}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <div className="text-sm font-medium text-muted-foreground mb-1">Frequency</div>
          <div className="text-2xl font-bold">{pattern.frequency}</div>
        </div>

        {pattern.affectedAreas.length > 0 && (
          <div>
            <div className="text-sm font-medium text-muted-foreground mb-2">
              Affected Areas
            </div>
            <div className="flex flex-wrap gap-2">
              {pattern.affectedAreas.map((area, idx) => (
                <Badge key={idx} variant="secondary" className="text-xs">
                  {area}
                </Badge>
              ))}
            </div>
          </div>
        )}

        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <div className="text-sm text-muted-foreground cursor-help line-clamp-2">
                <span className="font-medium">Recommendation: </span>
                {pattern.recommendation}
              </div>
            </TooltipTrigger>
            <TooltipContent className="max-w-md">
              <p>{pattern.recommendation}</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      </CardContent>
    </Card>
  );
}

