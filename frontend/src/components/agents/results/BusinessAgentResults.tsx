import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, Legend } from 'recharts';
import type { BusinessAgentOutput, ValueMetric } from '@/types/agents';
import { ArrowUp, ArrowDown, Minus } from 'lucide-react';
import { cn } from '@/lib/utils';

interface BusinessAgentResultsProps {
  output: BusinessAgentOutput;
}

export function BusinessAgentResults({ output }: BusinessAgentResultsProps) {
  // Prepare data for line chart (trend visualization)
  const chartData = output.metrics.map((metric) => ({
    name: metric.metricName.length > 15 
      ? metric.metricName.substring(0, 15) + '...' 
      : metric.metricName,
    fullName: metric.metricName,
    current: metric.currentValue,
    target: metric.targetValue,
    unit: metric.unit,
  }));

  return (
    <div className="space-y-4" role="region" aria-label="Business Agent Results">
      {output.highlights.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Highlights</CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2">
              {output.highlights.map((highlight, index) => (
                <li key={index} className="flex items-start gap-2">
                  <span className="text-primary mt-1">•</span>
                  <span className="text-sm">{highlight}</span>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Value Metrics Overview</CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData} margin={{ top: 5, right: 30, left: 20, bottom: 60 }}>
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
                        <p className="text-sm">
                          Current: {data.current} {data.unit}
                        </p>
                        <p className="text-sm">
                          Target: {data.target} {data.unit}
                        </p>
                      </div>
                    );
                  }
                  return null;
                }}
              />
              <Legend />
              <Line
                type="monotone"
                dataKey="current"
                stroke="hsl(var(--primary))"
                strokeWidth={2}
                dot={{ r: 4 }}
                name="Current"
              />
              <Line
                type="monotone"
                dataKey="target"
                stroke="hsl(var(--muted-foreground))"
                strokeWidth={2}
                strokeDasharray="5 5"
                dot={{ r: 4 }}
                name="Target"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      <div className="grid gap-4 md:grid-cols-2">
        {output.metrics.map((metric, index) => (
          <MetricCard key={index} metric={metric} />
        ))}
      </div>

      {output.recommendations.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Recommendations</CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-3">
              {output.recommendations.map((rec, index) => (
                <li key={index} className="flex items-start gap-3">
                  <Badge variant="outline" className="mt-0.5">
                    {index + 1}
                  </Badge>
                  <span className="text-sm flex-1">{rec}</span>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function MetricCard({ metric }: { metric: ValueMetric }) {
  const progress = metric.targetValue > 0 
    ? Math.min((metric.currentValue / metric.targetValue) * 100, 100)
    : 0;
  
  const getTrendIcon = () => {
    switch (metric.trend) {
      case 'up':
        return <ArrowUp className="h-4 w-4 text-green-500" />;
      case 'down':
        return <ArrowDown className="h-4 w-4 text-red-500" />;
      default:
        return <Minus className="h-4 w-4 text-muted-foreground" />;
    }
  };

  const getTrendColor = () => {
    switch (metric.trend) {
      case 'up':
        return 'text-green-500';
      case 'down':
        return 'text-red-500';
      default:
        return 'text-muted-foreground';
    }
  };

  return (
    <Card className="transition-all hover:shadow-md">
      <CardHeader>
        <div className="flex items-start justify-between gap-4">
          <CardTitle className="text-lg">{metric.metricName}</CardTitle>
          <div className={cn('flex items-center gap-1', getTrendColor())}>
            {getTrendIcon()}
            <span className="text-xs font-medium uppercase">{metric.trend}</span>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-baseline justify-between">
            <span className="text-sm text-muted-foreground">Current</span>
            <span className="text-2xl font-bold">
              {metric.currentValue.toLocaleString()} {metric.unit}
            </span>
          </div>
          <div className="flex items-baseline justify-between">
            <span className="text-sm text-muted-foreground">Target</span>
            <span className="text-lg font-medium text-muted-foreground">
              {metric.targetValue.toLocaleString()} {metric.unit}
            </span>
          </div>
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>Progress</span>
            <span>{progress.toFixed(0)}%</span>
          </div>
          <Progress value={progress} className="h-2" />
        </div>
      </CardContent>
    </Card>
  );
}

