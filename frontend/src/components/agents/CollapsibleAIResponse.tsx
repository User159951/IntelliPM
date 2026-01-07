import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { cn } from '@/lib/utils';

interface CollapsibleAIResponseProps {
  content: string;
  title?: string;
  defaultCollapsed?: boolean;
  storageKey?: string;
  maxHeight?: number;
  className?: string;
}

const COLLAPSED_THRESHOLD = 500; // Characters threshold to auto-collapse

/**
 * Component that makes AI responses collapsible.
 * Automatically collapses long responses and saves user preference.
 */
export function CollapsibleAIResponse({
  content,
  title,
  defaultCollapsed,
  storageKey,
  maxHeight = 400,
  className,
}: CollapsibleAIResponseProps) {
  const storageKeyFinal = storageKey || `ai-response-collapsed-${title || 'default'}`;
  
  // Load saved preference or use default
  const getInitialState = (): boolean => {
    if (storageKeyFinal) {
      const saved = localStorage.getItem(storageKeyFinal);
      if (saved !== null) {
        return saved === 'true';
      }
    }
    // Auto-collapse if content is long
    if (defaultCollapsed === undefined) {
      return content.length > COLLAPSED_THRESHOLD;
    }
    return defaultCollapsed;
  };

  const [isCollapsed, setIsCollapsed] = useState(getInitialState);

  // Save preference when it changes
  useEffect(() => {
    if (storageKeyFinal) {
      localStorage.setItem(storageKeyFinal, String(isCollapsed));
    }
  }, [isCollapsed, storageKeyFinal]);

  const shouldShowCollapse = content.length > COLLAPSED_THRESHOLD;

  if (!shouldShowCollapse) {
    return (
      <div className={cn('whitespace-pre-wrap text-sm', className)}>
        {content}
      </div>
    );
  }

  return (
    <div className={cn('space-y-2', className)}>
      {title && (
        <div className="flex items-center justify-between">
          <h4 className="text-sm font-medium">{title}</h4>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setIsCollapsed(!isCollapsed)}
            className="h-8"
          >
            {isCollapsed ? (
              <>
                <ChevronDown className="h-4 w-4 mr-1" />
                Expand
              </>
            ) : (
              <>
                <ChevronUp className="h-4 w-4 mr-1" />
                Collapse
              </>
            )}
          </Button>
        </div>
      )}
      <div
        className={cn(
          'whitespace-pre-wrap text-sm transition-all duration-300 overflow-hidden',
          isCollapsed && 'max-h-24 relative'
        )}
        style={isCollapsed ? { maxHeight: `${maxHeight / 4}px` } : undefined}
      >
        {content}
        {isCollapsed && (
          <div className="absolute bottom-0 left-0 right-0 h-12 bg-gradient-to-t from-background to-transparent pointer-events-none" />
        )}
      </div>
      {!title && (
        <Button
          variant="ghost"
          size="sm"
          onClick={() => setIsCollapsed(!isCollapsed)}
          className="w-full"
        >
          {isCollapsed ? (
            <>
              <ChevronDown className="h-4 w-4 mr-1" />
              Show more
            </>
          ) : (
            <>
              <ChevronUp className="h-4 w-4 mr-1" />
              Show less
            </>
          )}
        </Button>
      )}
    </div>
  );
}

