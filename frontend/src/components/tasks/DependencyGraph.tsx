import { useEffect, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  Node,
  Edge,
  Handle,
  Position,
  MarkerType,
  useReactFlow,
  ReactFlowProvider,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { AlertCircle, RefreshCw } from 'lucide-react';
import { dependenciesApi } from '@/api/dependencies';
import type { DependencyGraphNodeDto, DependencyGraphEdgeDto } from '@/types/dependencies';
import type { TaskStatus } from '@/types';
import StatusBadge from './StatusBadge';
import { cn } from '@/lib/utils';
import dagre from 'dagre';

interface DependencyGraphProps {
  projectId: number;
}

// Custom node component
interface CustomTaskNodeData {
  label: string;
  status: TaskStatus;
  assigneeName: string | null;
}

function CustomTaskNode({ data }: { data: CustomTaskNodeData }) {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Todo':
        return 'bg-gray-100 border-gray-300 dark:bg-gray-800 dark:border-gray-600';
      case 'InProgress':
        return 'bg-blue-50 border-blue-500 dark:bg-blue-900 dark:border-blue-700';
      case 'Done':
        return 'bg-green-50 border-green-500 dark:bg-green-900 dark:border-green-700';
      case 'Blocked':
        return 'bg-red-50 border-red-500 dark:bg-red-900 dark:border-red-700';
      default:
        return 'bg-gray-100 border-gray-300 dark:bg-gray-800 dark:border-gray-600';
    }
  };

  return (
    <div
      className={cn(
        'rounded-lg border-2 p-3 shadow-md min-w-[200px] max-w-[250px]',
        getStatusColor(data.status)
      )}
    >
      {/* Input handles (top) */}
      <Handle type="target" position={Position.Top} className="w-3 h-3" />
      
      <div className="space-y-2">
        {/* Task title */}
        <p className="text-sm font-semibold truncate" title={data.label}>
          {data.label}
        </p>
        
        {/* Status badge */}
        <div>
          <StatusBadge status={data.status} size="sm" />
        </div>
        
        {/* Assignee */}
        {data.assigneeName && (
          <div className="flex items-center gap-2">
            <Avatar className="h-6 w-6">
              <AvatarFallback className="text-xs">
                {data.assigneeName
                  .split(' ')
                  .map((n) => n[0])
                  .join('')
                  .toUpperCase()
                  .slice(0, 2)}
              </AvatarFallback>
            </Avatar>
            <span className="text-xs text-muted-foreground truncate">{data.assigneeName}</span>
          </div>
        )}
      </div>
      
      {/* Output handles (bottom) */}
      <Handle type="source" position={Position.Bottom} className="w-3 h-3" />
    </div>
  );
}

const nodeTypes = {
  custom: CustomTaskNode,
};

/**
 * Inner component that uses ReactFlow hooks (must be inside ReactFlowProvider)
 */
function DependencyGraphFlow({
  nodes,
  edges,
  nodeTypes,
}: {
  nodes: Node[];
  edges: Edge[];
  nodeTypes: Record<string, React.ComponentType<unknown>>;
}) {
  const { fitView } = useReactFlow();
  
  useEffect(() => {
    if (nodes.length > 0) {
      setTimeout(() => {
        fitView({ padding: 0.2, duration: 500 });
      }, 100);
    }
  }, [nodes, fitView]);

  return (
    <ReactFlow
      nodes={nodes}
      edges={edges}
      nodeTypes={nodeTypes}
      fitView
      panOnDrag
      zoomOnScroll
      className="bg-background"
    >
      <Background />
      <Controls />
      <MiniMap
        className="hidden md:block"
        nodeColor={(node) => {
          const data = node.data as unknown as CustomTaskNodeData;
          const status = data?.status;
          switch (status) {
            case 'Todo':
              return '#9ca3af';
            case 'InProgress':
              return '#3b82f6';
            case 'Done':
              return '#10b981';
            case 'Blocked':
              return '#ef4444';
            default:
              return '#6b7280';
          }
        }}
      />
    </ReactFlow>
  );
}

/**
 * Component for visualizing task dependencies as a graph.
 * Uses react-flow for interactive graph visualization with auto-layout.
 */
export function DependencyGraph({ projectId }: DependencyGraphProps) {
  // Fetch dependency graph data
  const { data: graphData, isLoading, error, refetch } = useQuery({
    queryKey: ['projectDependencyGraph', projectId],
    queryFn: () => dependenciesApi.getProjectDependencyGraph(projectId),
    enabled: !!projectId,
  });

  // Transform backend data to react-flow format with auto-layout
  const { nodes, edges } = useMemo(() => {
    if (!graphData || graphData.nodes.length === 0) {
      return { nodes: [], edges: [] };
    }

    // Create dagre graph
    const g = new dagre.graphlib.Graph();
    g.setDefaultEdgeLabel(() => ({}));
    g.setGraph({ rankdir: 'LR', nodesep: 150, ranksep: 200 });

    // Add nodes
    graphData.nodes.forEach((node) => {
      g.setNode(node.taskId.toString(), {
        width: 250,
        height: 120,
      });
    });

    // Add edges
    graphData.edges.forEach((edge) => {
      g.setEdge(
        edge.sourceTaskId.toString(),
        edge.dependentTaskId.toString()
      );
    });

    // Calculate layout
    dagre.layout(g);

    // Transform to react-flow nodes
    const flowNodes: Node[] = graphData.nodes.map((node: DependencyGraphNodeDto) => {
      const nodeWithPosition = g.node(node.taskId.toString());
      return {
        id: node.taskId.toString(),
        type: 'custom',
        position: {
          x: nodeWithPosition.x - 125, // Center the node
          y: nodeWithPosition.y - 60,
        },
        data: {
          label: node.title,
          status: node.status,
          assigneeName: node.assigneeName,
        },
      };
    });

    // Transform to react-flow edges with custom styling
    const flowEdges: Edge[] = graphData.edges.map((edge: DependencyGraphEdgeDto) => {
      const edgeStyle = getEdgeStyle(edge.dependencyType);
      return {
        id: edge.id.toString(),
        source: edge.sourceTaskId.toString(),
        target: edge.dependentTaskId.toString(),
        label: (
          <Badge variant="outline" className="text-xs">
            {edge.label}
          </Badge>
        ),
        type: 'smoothstep',
        animated: true,
        markerEnd: {
          type: MarkerType.ArrowClosed,
        },
        style: edgeStyle,
        labelStyle: {
          fontSize: '12px',
        },
        labelBgStyle: {
          fill: 'white',
          fillOpacity: 0.8,
        },
      };
    });

    return { nodes: flowNodes, edges: flowEdges };
  }, [graphData]);

  // Get edge style based on dependency type
  const getEdgeStyle = (dependencyType: string) => {
    switch (dependencyType) {
      case 'FinishToStart':
        return { stroke: '#3b82f6', strokeWidth: 2 };
      case 'StartToStart':
        return { stroke: '#10b981', strokeWidth: 2, strokeDasharray: '5,5' };
      case 'FinishToFinish':
        return { stroke: '#a855f7', strokeWidth: 2, strokeDasharray: '2,2' };
      case 'StartToFinish':
        return { stroke: '#f97316', strokeWidth: 2, strokeDasharray: '8,4,2,4' };
      default:
        return { stroke: '#6b7280', strokeWidth: 2 };
    }
  };


  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Dependency Graph</CardTitle>
          <CardDescription>Loading dependency graph...</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="h-[600px] md:h-[800px] space-y-4">
            <Skeleton className="h-full w-full" />
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Dependency Graph</CardTitle>
          <CardDescription>Error loading dependency graph</CardDescription>
        </CardHeader>
        <CardContent>
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>Error</AlertTitle>
            <AlertDescription>
              {error instanceof Error ? error.message : 'Failed to load dependency graph'}
              <Button variant="outline" size="sm" className="mt-2" onClick={() => refetch()}>
                <RefreshCw className="h-4 w-4 mr-2" />
                Retry
              </Button>
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  if (!graphData || graphData.nodes.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Dependency Graph</CardTitle>
          <CardDescription>No dependencies in this project</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="h-[600px] md:h-[800px] flex items-center justify-center">
            <div className="text-center text-muted-foreground">
              <AlertCircle className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p className="text-sm">No dependencies found in this project</p>
              <p className="text-xs mt-2">Add dependencies between tasks to see them visualized here</p>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Dependency Graph</CardTitle>
        <CardDescription>
          Visual representation of task dependencies. Drag to pan, scroll to zoom.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="h-[600px] md:h-[800px] border rounded-lg overflow-hidden">
          <ReactFlowProvider>
            <DependencyGraphFlow nodes={nodes} edges={edges} nodeTypes={nodeTypes} />
          </ReactFlowProvider>
        </div>
        
        {/* Legend */}
        <div className="mt-4 p-4 bg-muted rounded-lg">
          <p className="text-sm font-semibold mb-2">Dependency Types:</p>
          <div className="flex flex-wrap gap-4 text-xs">
            <div className="flex items-center gap-2">
              <div className="w-8 h-0.5 bg-blue-500" />
              <span>Finish-to-Start (FS)</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-8 h-0.5 bg-green-500 border-dashed border-t-2" />
              <span>Start-to-Start (SS)</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-8 h-0.5 bg-purple-500 border-dashed border-t-2" />
              <span>Finish-to-Finish (FF)</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-8 h-0.5 bg-orange-500 border-dashed border-t-2" />
              <span>Start-to-Finish (SF)</span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

