import { useMemo } from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { AlertCircle } from 'lucide-react';
import type { AgentResponse } from '@/types';
import type { AgentType } from '@/types/agents';
import { ProductAgentResults } from './ProductAgentResults';
import { QAAgentResults } from './QAAgentResults';
import { BusinessAgentResults } from './BusinessAgentResults';
import { ManagerAgentResults } from './ManagerAgentResults';
import { DeliveryAgentResults } from './DeliveryAgentResults';
import { CollapsibleAIResponse } from '../CollapsibleAIResponse';

interface AgentResultsDisplayProps {
  agentType: AgentType;
  result: AgentResponse | null;
  isLoading?: boolean;
}

export function AgentResultsDisplay({ agentType, result, isLoading }: AgentResultsDisplayProps) {
  const parsedContent = useMemo(() => {
    if (!result?.content) return null;

    try {
      // Try to parse JSON first
      const jsonMatch = result.content.match(/\{[\s\S]*\}/);
      if (jsonMatch) {
        return JSON.parse(jsonMatch[0]);
      }
      return null;
    } catch (error) {
      // If parsing fails, return null to show fallback
      return null;
    }
  }, [result?.content]);

  if (isLoading) {
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <Skeleton className="h-8 w-1/3" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-32 w-full" />
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!result) {
    return null;
  }

  if (result.status === 'Error') {
    return (
      <Alert variant="destructive">
        <AlertCircle className="h-4 w-4" />
        <AlertTitle>Error</AlertTitle>
        <AlertDescription>
          {result.errorMessage || 'An error occurred while running the agent'}
        </AlertDescription>
      </Alert>
    );
  }

  // If we have parsed content, try to render the structured view
  if (parsedContent) {
    try {
      switch (agentType) {
        case 'product':
          return <ProductAgentResults output={parsedContent} />;
        case 'qa':
          return <QAAgentResults output={parsedContent} />;
        case 'business':
          return <BusinessAgentResults output={parsedContent} />;
        case 'manager':
          return <ManagerAgentResults output={parsedContent} />;
        case 'delivery':
          return <DeliveryAgentResults output={parsedContent} />;
        default:
          return <FallbackDisplay content={result.content} />;
      }
    } catch (error) {
      // If rendering fails, fall back to text display
      return <FallbackDisplay content={result.content} />;
    }
  }

  // Fallback to text display if parsing failed or content is not JSON
  return <FallbackDisplay content={result.content} />;
}

function FallbackDisplay({ content }: { content: string }) {
  return (
    <Card>
      <CardContent className="pt-6">
        <CollapsibleAIResponse
          content={content}
          storageKey="agent-fallback-response"
          className="font-mono bg-muted p-4 rounded-lg"
        />
      </CardContent>
    </Card>
  );
}

