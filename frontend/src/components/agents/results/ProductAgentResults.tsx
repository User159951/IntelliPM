import { useState, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { ArrowUpDown, ArrowUp, ArrowDown } from 'lucide-react';
import type { ProductAgentOutput } from '@/types/agents';
import { cn } from '@/lib/utils';

interface ProductAgentResultsProps {
  output: ProductAgentOutput;
}

type SortField = 'priority' | 'confidenceScore' | 'taskTitle';
type SortDirection = 'asc' | 'desc';

export function ProductAgentResults({ output }: ProductAgentResultsProps) {
  const [sortField, setSortField] = useState<SortField>('priority');
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc');

  const sortedItems = useMemo(() => {
    const sorted = [...output.items];
    sorted.sort((a, b) => {
      let comparison = 0;
      
      switch (sortField) {
        case 'priority':
          comparison = a.priority - b.priority;
          break;
        case 'confidenceScore':
          comparison = a.confidenceScore - b.confidenceScore;
          break;
        case 'taskTitle':
          comparison = a.taskTitle.localeCompare(b.taskTitle);
          break;
      }
      
      return sortDirection === 'asc' ? comparison : -comparison;
    });
    
    return sorted;
  }, [output.items, sortField, sortDirection]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const getConfidenceColor = (score: number) => {
    if (score >= 0.8) return 'bg-green-500/10 text-green-500 border-green-500/20';
    if (score >= 0.6) return 'bg-blue-500/10 text-blue-500 border-blue-500/20';
    if (score >= 0.4) return 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20';
    return 'bg-red-500/10 text-red-500 border-red-500/20';
  };

  return (
    <div className="space-y-4" role="region" aria-label="Product Agent Results">
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

      <Card>
        <CardHeader>
          <CardTitle>Prioritized Items ({output.items.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-20">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 -ml-3"
                      onClick={() => handleSort('priority')}
                      aria-label="Sort by priority"
                    >
                      Priority
                      {sortField === 'priority' ? (
                        sortDirection === 'asc' ? (
                          <ArrowUp className="ml-2 h-4 w-4" />
                        ) : (
                          <ArrowDown className="ml-2 h-4 w-4" />
                        )
                      ) : (
                        <ArrowUpDown className="ml-2 h-4 w-4" />
                      )}
                    </Button>
                  </TableHead>
                  <TableHead>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 -ml-3"
                      onClick={() => handleSort('taskTitle')}
                      aria-label="Sort by task title"
                    >
                      Task
                      {sortField === 'taskTitle' ? (
                        sortDirection === 'asc' ? (
                          <ArrowUp className="ml-2 h-4 w-4" />
                        ) : (
                          <ArrowDown className="ml-2 h-4 w-4" />
                        )
                      ) : (
                        <ArrowUpDown className="ml-2 h-4 w-4" />
                      )}
                    </Button>
                  </TableHead>
                  <TableHead>Rationale</TableHead>
                  <TableHead className="w-32">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 -ml-3"
                      onClick={() => handleSort('confidenceScore')}
                      aria-label="Sort by confidence score"
                    >
                      Confidence
                      {sortField === 'confidenceScore' ? (
                        sortDirection === 'asc' ? (
                          <ArrowUp className="ml-2 h-4 w-4" />
                        ) : (
                          <ArrowDown className="ml-2 h-4 w-4" />
                        )
                      ) : (
                        <ArrowUpDown className="ml-2 h-4 w-4" />
                      )}
                    </Button>
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedItems.map((item, index) => (
                  <TableRow key={item.taskId || index} className="transition-colors hover:bg-muted/50">
                    <TableCell>
                      <Badge variant="outline" className="font-mono">
                        {item.priority}
                      </Badge>
                    </TableCell>
                    <TableCell className="font-medium">{item.taskTitle}</TableCell>
                    <TableCell className="text-sm text-muted-foreground max-w-md">
                      {item.rationale}
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant="outline"
                        className={cn('font-mono', getConfidenceColor(item.confidenceScore))}
                      >
                        {(item.confidenceScore * 100).toFixed(0)}%
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

