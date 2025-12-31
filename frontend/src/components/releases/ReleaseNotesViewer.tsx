import { useState } from 'react';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { FileText, Printer, Copy, Edit, Wand2 } from 'lucide-react';
import { showToast } from '@/lib/sweetalert';
import type { ReleaseDto } from '@/types/releases';
import { cn } from '@/lib/utils';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import 'highlight.js/styles/github-dark.css';
import * as React from 'react';

interface ReleaseNotesViewerProps {
  release: ReleaseDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onEdit?: () => void;
  onGenerate?: (type: 'notes' | 'changelog') => void;
}

interface EmptyStateProps {
  type: 'notes' | 'changelog';
  onGenerate?: () => void;
}

function EmptyState({ type, onGenerate }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <FileText className="h-12 w-12 text-muted-foreground mb-4" />
      <p className="text-muted-foreground mb-4">
        No {type === 'notes' ? 'release notes' : 'changelog'} available yet.
      </p>
      {onGenerate && (
        <Button variant="outline" onClick={onGenerate}>
          <Wand2 className="h-4 w-4 mr-2" />
          Auto-generate {type === 'notes' ? 'Release Notes' : 'Changelog'}
        </Button>
      )}
    </div>
  );
}

const markdownComponents = {
  h1: ({ ...props }: React.ComponentPropsWithoutRef<'h1'>) => (
    <h1
      className="text-3xl font-bold mb-4 mt-6 first:mt-0 text-foreground"
      {...props}
    />
  ),
  h2: ({ ...props }: React.ComponentPropsWithoutRef<'h2'>) => (
    <h2 className="text-2xl font-semibold mb-3 mt-6 text-foreground" {...props} />
  ),
  h3: ({ ...props }: React.ComponentPropsWithoutRef<'h3'>) => (
    <h3 className="text-xl font-semibold mb-2 mt-4 text-foreground" {...props} />
  ),
  p: ({ ...props }: React.ComponentPropsWithoutRef<'p'>) => (
    <p className="mb-4 leading-7 text-muted-foreground" {...props} />
  ),
  ul: ({ ...props }: React.ComponentPropsWithoutRef<'ul'>) => (
    <ul className="list-disc list-inside mb-4 space-y-2 ml-4" {...props} />
  ),
  ol: ({ ...props }: React.ComponentPropsWithoutRef<'ol'>) => (
    <ol className="list-decimal list-inside mb-4 space-y-2 ml-4" {...props} />
  ),
  li: ({ ...props }: React.ComponentPropsWithoutRef<'li'>) => (
    <li className="text-muted-foreground" {...props} />
  ),
  blockquote: ({ ...props }: React.ComponentPropsWithoutRef<'blockquote'>) => (
    <blockquote
      className="border-l-4 border-primary pl-4 italic my-4 text-muted-foreground"
      {...props}
    />
  ),
  code: ({ inline, className, children, ...props }: React.ComponentPropsWithoutRef<'code'> & { inline?: boolean }) => {
    return inline ? (
      <code
        className="bg-muted px-1.5 py-0.5 rounded text-sm font-mono"
        {...props}
      >
        {children}
      </code>
    ) : (
      <code
        className={cn(
          'block bg-muted p-4 rounded-lg overflow-x-auto text-sm font-mono',
          className || ''
        )}
        {...props}
      >
        {children}
      </code>
    );
  },
  a: ({ ...props }: React.ComponentPropsWithoutRef<'a'>) => (
    <a
      className="text-primary hover:underline"
      target="_blank"
      rel="noopener noreferrer"
      {...props}
    />
  ),
  table: ({ ...props }: React.ComponentPropsWithoutRef<'table'>) => (
    <div className="overflow-x-auto my-4">
      <table
        className="min-w-full border-collapse border border-border"
        {...props}
      />
    </div>
  ),
  th: ({ ...props }: React.ComponentPropsWithoutRef<'th'>) => (
    <th
      className="border border-border px-4 py-2 bg-muted font-semibold text-left"
      {...props}
    />
  ),
  td: ({ ...props }: React.ComponentPropsWithoutRef<'td'>) => (
    <td className="border border-border px-4 py-2" {...props} />
  ),
  hr: ({ ...props }: React.ComponentPropsWithoutRef<'hr'>) => (
    <hr className="my-6 border-border" {...props} />
  ),
};

export function ReleaseNotesViewer({
  release,
  open,
  onOpenChange,
  onEdit,
  onGenerate,
}: ReleaseNotesViewerProps) {
  const [activeTab, setActiveTab] = useState<'notes' | 'changelog'>('notes');

  const handlePrint = () => {
    window.print();
  };

  const handleCopy = async () => {
    const content =
      activeTab === 'notes' ? release.releaseNotes : release.changeLog;
    
    if (!content) {
      showToast('No content to copy', 'error');
      return;
    }

    try {
      await navigator.clipboard.writeText(content);
      showToast('Copied to clipboard', 'success');
    } catch (error) {
      showToast('Failed to copy to clipboard', 'error');
    }
  };

  const handleGenerate = () => {
    onGenerate?.(activeTab);
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'Major':
        return 'bg-red-500 text-white';
      case 'Minor':
        return 'bg-blue-500 text-white';
      case 'Patch':
        return 'bg-green-500 text-white';
      case 'Hotfix':
        return 'bg-orange-500 text-white';
      default:
        return 'bg-gray-500 text-white';
    }
  };

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent
        side="right"
        className="w-full sm:max-w-3xl overflow-y-auto no-print"
      >
        <SheetHeader>
          <SheetTitle className="flex items-center gap-2">
            <span>Release {release.version}</span>
            <Badge variant="outline" className={getTypeColor(release.type)}>
              {release.type}
            </Badge>
            {release.isPreRelease && (
              <Badge variant="secondary">Pre-release</Badge>
            )}
          </SheetTitle>
          <SheetDescription>{release.name}</SheetDescription>
        </SheetHeader>

        {/* Action buttons */}
        <div className="flex gap-2 my-4 no-print">
          <Button variant="outline" size="sm" onClick={handlePrint}>
            <Printer className="h-4 w-4 mr-2" />
            Print
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleCopy}
            disabled={
              activeTab === 'notes'
                ? !release.releaseNotes
                : !release.changeLog
            }
          >
            <Copy className="h-4 w-4 mr-2" />
            Copy
          </Button>
          {onEdit && (
            <Button variant="outline" size="sm" onClick={onEdit}>
              <Edit className="h-4 w-4 mr-2" />
              Edit
            </Button>
          )}
        </div>

        {/* Tabs */}
        <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as 'notes' | 'changelog')}>
          <TabsList className="w-full no-print">
            <TabsTrigger value="notes" className="flex-1">
              <FileText className="h-4 w-4 mr-2" />
              Release Notes
            </TabsTrigger>
            <TabsTrigger value="changelog" className="flex-1">
              <FileText className="h-4 w-4 mr-2" />
              Changelog
            </TabsTrigger>
          </TabsList>

          <TabsContent value="notes" className="mt-4">
            <ScrollArea className="h-[calc(100vh-300px)]">
              {release.releaseNotes ? (
                <div className="prose prose-sm dark:prose-invert max-w-none">
                  <ReactMarkdown
                    remarkPlugins={[remarkGfm]}
                    rehypePlugins={[rehypeHighlight]}
                    components={markdownComponents}
                  >
                    {release.releaseNotes}
                  </ReactMarkdown>
                </div>
              ) : (
                <EmptyState type="notes" onGenerate={handleGenerate} />
              )}
            </ScrollArea>
          </TabsContent>

          <TabsContent value="changelog" className="mt-4">
            <ScrollArea className="h-[calc(100vh-300px)]">
              {release.changeLog ? (
                <div className="prose prose-sm dark:prose-invert max-w-none">
                  <ReactMarkdown
                    remarkPlugins={[remarkGfm]}
                    rehypePlugins={[rehypeHighlight]}
                    components={markdownComponents}
                  >
                    {release.changeLog}
                  </ReactMarkdown>
                </div>
              ) : (
                <EmptyState type="changelog" onGenerate={handleGenerate} />
              )}
            </ScrollArea>
          </TabsContent>
        </Tabs>
      </SheetContent>
    </Sheet>
  );
}

