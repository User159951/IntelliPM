import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import type { ManagerAgentOutput } from '@/types/agents';

interface ManagerAgentResultsProps {
  output: ManagerAgentOutput;
}

export function ManagerAgentResults({ output }: ManagerAgentResultsProps) {
  const [checkedDecisions, setCheckedDecisions] = React.useState<Set<number>>(new Set());

  const toggleDecision = (index: number) => {
    setCheckedDecisions((prev) => {
      const next = new Set(prev);
      if (next.has(index)) {
        next.delete(index);
      } else {
        next.add(index);
      }
      return next;
    });
  };

  return (
    <div className="space-y-4" role="region" aria-label="Manager Agent Results">
      <Card className="border-primary/20 bg-primary/5">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            Executive Summary
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm leading-relaxed">{output.executiveSummary}</p>
        </CardContent>
      </Card>

      {output.keyDecisions.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Key Decisions</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {output.keyDecisions.map((decision, index) => (
                <div
                  key={index}
                  className="flex items-start gap-3 p-3 rounded-lg border transition-colors hover:bg-muted/50"
                >
                  <Checkbox
                    id={`decision-${index}`}
                    checked={checkedDecisions.has(index)}
                    onCheckedChange={() => toggleDecision(index)}
                    aria-label={`Mark decision ${index + 1} as reviewed`}
                  />
                  <label
                    htmlFor={`decision-${index}`}
                    className="flex-1 text-sm cursor-pointer"
                  >
                    {decision}
                  </label>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {output.highlights.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Highlights</CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2">
              {output.highlights.map((highlight, index) => (
                <li key={index} className="flex items-start gap-2">
                  <Badge variant="outline" className="mt-0.5 bg-blue-500/10 text-blue-500 border-blue-500/20">
                    {index + 1}
                  </Badge>
                  <span className="text-sm flex-1">{highlight}</span>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}

      {output.insights.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Insights</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {output.insights.map((insight, index) => (
                <div
                  key={index}
                  className="p-3 rounded-lg border-l-4 border-primary bg-muted/30"
                >
                  <p className="text-sm">{insight}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

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

