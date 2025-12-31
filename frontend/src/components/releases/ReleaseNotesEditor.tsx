import { useState, useEffect, useRef } from 'react';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  Wand2,
  Bold,
  Italic,
  Heading,
  List,
  Link,
  Code,
  Save,
  Loader2,
} from 'lucide-react';
import { showToast } from '@/lib/sweetalert';
import { releasesApi } from '@/api/releases';
import { cn } from '@/lib/utils';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import * as React from 'react';

interface ReleaseNotesEditorProps {
  releaseId: number;
  initialContent: string;
  mode: 'notes' | 'changelog';
  onSave: () => void;
  onCancel: () => void;
}

const markdownComponents = {
  h1: ({ ...props }: React.ComponentPropsWithoutRef<'h1'>) => (
    <h1 className="text-3xl font-bold mb-4 mt-6 first:mt-0" {...props} />
  ),
  h2: ({ ...props }: React.ComponentPropsWithoutRef<'h2'>) => (
    <h2 className="text-2xl font-semibold mb-3 mt-6" {...props} />
  ),
  h3: ({ ...props }: React.ComponentPropsWithoutRef<'h3'>) => (
    <h3 className="text-xl font-semibold mb-2 mt-4" {...props} />
  ),
  p: ({ ...props }: React.ComponentPropsWithoutRef<'p'>) => (
    <p className="mb-4 leading-7" {...props} />
  ),
  ul: ({ ...props }: React.ComponentPropsWithoutRef<'ul'>) => (
    <ul className="list-disc list-inside mb-4 space-y-2" {...props} />
  ),
  ol: ({ ...props }: React.ComponentPropsWithoutRef<'ol'>) => (
    <ol className="list-decimal list-inside mb-4 space-y-2" {...props} />
  ),
  code: ({ inline, className, children, ...props }: React.ComponentPropsWithoutRef<'code'> & { inline?: boolean }) => {
    return inline ? (
      <code className="bg-muted px-1.5 py-0.5 rounded text-sm font-mono" {...props}>
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
  blockquote: ({ ...props }: React.ComponentPropsWithoutRef<'blockquote'>) => (
    <blockquote
      className="border-l-4 border-primary pl-4 italic my-4 text-muted-foreground"
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

/**
 * Markdown editor component for editing release notes and changelogs.
 * Features split view with live preview, formatting toolbar, and auto-generation.
 */
export function ReleaseNotesEditor({
  releaseId,
  initialContent,
  mode,
  onSave,
  onCancel,
}: ReleaseNotesEditorProps) {
  const [content, setContent] = useState(initialContent);
  const [isDirty, setIsDirty] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Track changes
  useEffect(() => {
    setIsDirty(content !== initialContent);
  }, [content, initialContent]);

  const insertMarkdown = (before: string, after: string) => {
    const textarea = textareaRef.current;
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selectedText = content.substring(start, end);

    const newText =
      content.substring(0, start) +
      before +
      selectedText +
      after +
      content.substring(end);

    setContent(newText);

    // Set cursor position after inserted text
    setTimeout(() => {
      textarea.focus();
      const newCursorPos = start + before.length + selectedText.length;
      textarea.setSelectionRange(newCursorPos, newCursorPos);
    }, 0);
  };

  const handleAutoGenerate = async () => {
    // Confirm if content exists
    if (
      content.trim() &&
      !window.confirm('This will replace existing content. Continue?')
    ) {
      return;
    }

    setIsGenerating(true);
    try {
      const generated =
        mode === 'notes'
          ? await releasesApi.generateReleaseNotes(releaseId)
          : await releasesApi.generateChangelog(releaseId);

      setContent(generated);
      showToast('Content generated successfully', 'success');
    } catch (error) {
      showToast('Failed to generate content', 'error');
    } finally {
      setIsGenerating(false);
    }
  };

  const handleSave = async () => {
    if (content.length > 5000) {
      showToast('Content exceeds 5000 characters', 'error');
      return;
    }

    setIsSaving(true);
    try {
      if (mode === 'notes') {
        await releasesApi.updateReleaseNotes(releaseId, content, false);
      } else {
        await releasesApi.updateChangelog(releaseId, content, false);
      }

      showToast(
        `${mode === 'notes' ? 'Release notes' : 'Changelog'} saved successfully`,
        'success'
      );
      setIsDirty(false);
      onSave(); // Trigger parent refresh
    } catch (error) {
      showToast('Failed to save content', 'error');
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    if (isDirty) {
      if (!window.confirm('You have unsaved changes. Discard them?')) {
        return;
      }
    }
    onCancel();
  };

  return (
    <div className="flex flex-col h-full">
      {/* Top bar with title and actions */}
      <div className="flex items-center justify-between p-4 border-b">
        <div>
          <h3 className="font-semibold">
            Editing {mode === 'notes' ? 'Release Notes' : 'Changelog'}
          </h3>
          <p className="text-sm text-muted-foreground">Markdown supported</p>
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handleAutoGenerate}
            disabled={isGenerating}
          >
            {isGenerating ? (
              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
            ) : (
              <Wand2 className="h-4 w-4 mr-2" />
            )}
            Auto-generate
          </Button>
          <span
            className={cn(
              'text-sm',
              content.length > 5000
                ? 'text-destructive'
                : 'text-muted-foreground'
            )}
          >
            {content.length} / 5000
          </span>
        </div>
      </div>

      {/* Toolbar with formatting buttons */}
      <div className="flex items-center gap-1 p-2 border-b bg-muted/30">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => insertMarkdown('**', '**')}
          title="Bold"
        >
          <Bold className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => insertMarkdown('*', '*')}
          title="Italic"
        >
          <Italic className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => insertMarkdown('# ', '')}
          title="Heading"
        >
          <Heading className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => insertMarkdown('- ', '')}
          title="List"
        >
          <List className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => insertMarkdown('[', '](url)')}
          title="Link"
        >
          <Link className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => insertMarkdown('`', '`')}
          title="Code"
        >
          <Code className="h-4 w-4" />
        </Button>
      </div>

      {/* Split view: Editor + Preview */}
      <div className="flex-1 flex overflow-hidden">
        {/* Editor pane */}
        <div className="flex-1 flex flex-col border-r overflow-hidden">
          <div className="p-2 bg-muted/30 border-b shrink-0">
            <span className="text-xs font-medium text-muted-foreground">
              Editor
            </span>
          </div>
          <div className="flex-1 p-4 overflow-y-auto">
            <Textarea
              id="release-notes-editor"
              name="release-notes-editor"
              ref={textareaRef}
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder={`Enter ${mode === 'notes' ? 'release notes' : 'changelog'} in Markdown...`}
              className="w-full h-full min-h-[400px] resize-none font-mono text-sm border-0 focus-visible:ring-0"
            />
          </div>
        </div>

        {/* Preview pane */}
        <div className="flex-1 flex flex-col overflow-hidden">
          <div className="p-2 bg-muted/30 border-b shrink-0">
            <span className="text-xs font-medium text-muted-foreground">
              Preview
            </span>
          </div>
          <ScrollArea className="flex-1">
            <div className="p-4 prose prose-sm dark:prose-invert max-w-none">
              {content ? (
                <ReactMarkdown
                  remarkPlugins={[remarkGfm]}
                  rehypePlugins={[rehypeHighlight]}
                  components={markdownComponents}
                >
                  {content}
                </ReactMarkdown>
              ) : (
                <p className="text-muted-foreground">
                  Preview will appear here...
                </p>
              )}
            </div>
          </ScrollArea>
        </div>
      </div>

      {/* Bottom bar with actions */}
      <div className="flex items-center justify-between p-4 border-t bg-background">
        <div className="text-sm text-muted-foreground">
          {isDirty && '-  Unsaved changes'}
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleCancel}>
            Cancel
          </Button>
          <Button onClick={handleSave} disabled={isSaving || content.length > 5000}>
            {isSaving ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Saving...
              </>
            ) : (
              <>
                <Save className="h-4 w-4 mr-2" />
                Save
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}

