import { useState, useRef, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Loader2 } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { usersApi } from '@/api/users';

interface CommentFormProps {
  onSubmit: (content: string) => void;
  isSubmitting: boolean;
  initialValue?: string;
  placeholder?: string;
  autoFocus?: boolean;
}

// Helper function to get caret coordinates
function getCaretCoordinates(
  element: HTMLTextAreaElement,
  position: number
): { top: number; left: number } {
  const div = document.createElement('div');
  const style = getComputedStyle(element);

  // Copy styles
  Array.from(style).forEach((prop) => {
    div.style.setProperty(prop, style.getPropertyValue(prop));
  });

  div.style.position = 'absolute';
  div.style.visibility = 'hidden';
  div.style.whiteSpace = 'pre-wrap';
  div.style.wordWrap = 'break-word';
  div.style.width = `${element.offsetWidth}px`;
  div.textContent = element.value.substring(0, position);

  const span = document.createElement('span');
  span.textContent = element.value.substring(position) || '.';
  div.appendChild(span);

  document.body.appendChild(div);

  const rect = element.getBoundingClientRect();
  const spanRect = span.getBoundingClientRect();

  const top = spanRect.top - rect.top + element.scrollTop;
  const left = spanRect.left - rect.left + element.scrollLeft;

  document.body.removeChild(div);

  return { top, left };
}

export default function CommentForm({
  onSubmit,
  isSubmitting,
  initialValue = '',
  placeholder = 'Write a comment... Use @ to mention someone',
  autoFocus = false,
}: CommentFormProps) {
  const [content, setContent] = useState(initialValue);
  const [showMentionSuggestions, setShowMentionSuggestions] = useState(false);
  const [mentionSearch, setMentionSearch] = useState('');
  const [mentionPosition, setMentionPosition] = useState({ top: 0, left: 0 });
  const [cursorPosition, setCursorPosition] = useState(0);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Fetch team members for mentions
  const { data: teamMembersResponse } = useQuery({
    queryKey: ['team-members'],
    queryFn: () => usersApi.getAll(),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const teamMembers = teamMembersResponse?.users || [];

  // Filter users based on mention search
  const filteredUsers = useMemo(() => {
    if (!teamMembers || !mentionSearch) return [];

    const search = mentionSearch.toLowerCase();
    return teamMembers
      .filter(
        (user) =>
          user.username.toLowerCase().includes(search) ||
          user.firstName?.toLowerCase().includes(search) ||
          user.lastName?.toLowerCase().includes(search)
      )
      .slice(0, 5); // Limit to 5 suggestions
  }, [teamMembers, mentionSearch]);

  const handleContentChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const value = e.target.value;
    const cursorPos = e.target.selectionStart || 0;

    setContent(value);
    setCursorPosition(cursorPos);

    // Check for @ mention
    const textBeforeCursor = value.substring(0, cursorPos);
    const lastAtIndex = textBeforeCursor.lastIndexOf('@');

    if (lastAtIndex !== -1) {
      const textAfterAt = textBeforeCursor.substring(lastAtIndex + 1);

      // Check if there's a space after @
      if (!textAfterAt.includes(' ') && !textAfterAt.includes('\n')) {
        setMentionSearch(textAfterAt);
        setShowMentionSuggestions(true);

        // Calculate position for dropdown
        if (textareaRef.current) {
          const coords = getCaretCoordinates(textareaRef.current, cursorPos);
          const rect = textareaRef.current.getBoundingClientRect();
          setMentionPosition({
            top: coords.top + rect.top + window.scrollY,
            left: coords.left + rect.left + window.scrollX,
          });
        }
      } else {
        setShowMentionSuggestions(false);
      }
    } else {
      setShowMentionSuggestions(false);
    }
  };

  const handleMentionSelect = (username: string) => {
    const textBeforeCursor = content.substring(0, cursorPosition);
    const lastAtIndex = textBeforeCursor.lastIndexOf('@');
    const textAfterCursor = content.substring(cursorPosition);

    const newContent =
      content.substring(0, lastAtIndex) + `@${username} ` + textAfterCursor;

    setContent(newContent);
    setShowMentionSuggestions(false);

    // Focus back on textarea
    setTimeout(() => {
      if (textareaRef.current) {
        const newCursorPos = lastAtIndex + username.length + 2;
        textareaRef.current.focus();
        textareaRef.current.setSelectionRange(newCursorPos, newCursorPos);
      }
    }, 0);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!content.trim()) return;

    onSubmit(content);
    setContent('');
    setShowMentionSuggestions(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    // Submit on Ctrl+Enter or Cmd+Enter
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
      e.preventDefault();
      handleSubmit(e);
    }

    // Handle arrow keys in mention suggestions
    if (showMentionSuggestions && filteredUsers.length > 0) {
      // This could be enhanced to navigate suggestions with arrow keys
    }
  };

  return (
    <form onSubmit={handleSubmit} className="relative">
      <Textarea
        id="comment-content"
        name="comment-content"
        ref={textareaRef}
        value={content}
        onChange={handleContentChange}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        disabled={isSubmitting}
        autoFocus={autoFocus}
        autoComplete="off"
        className="min-h-[100px] resize-none"
      />

      {/* Mention suggestions dropdown */}
      {showMentionSuggestions && filteredUsers.length > 0 && (
        <Card
          className="fixed z-50 w-64 shadow-lg"
          style={{
            top: `${mentionPosition.top + 20}px`,
            left: `${mentionPosition.left}px`,
          }}
        >
          <CardContent className="p-0">
            {filteredUsers.map((user) => (
              <button
                key={user.id}
                type="button"
                onClick={() => handleMentionSelect(user.username)}
                className="flex items-center gap-2 w-full p-2 hover:bg-accent text-left transition-colors"
              >
                <Avatar className="h-6 w-6">
                  <AvatarFallback className="text-xs">
                    {user.firstName?.[0] || ''}
                    {user.lastName?.[0] || ''}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">
                    {user.firstName} {user.lastName}
                  </p>
                  <p className="text-xs text-muted-foreground truncate">
                    @{user.username}
                  </p>
                </div>
              </button>
            ))}
          </CardContent>
        </Card>
      )}

      <div className="flex items-center justify-between mt-2">
        <p className="text-xs text-muted-foreground">
          Press Ctrl+Enter to submit
        </p>
        <Button type="submit" disabled={isSubmitting || !content.trim()}>
          {isSubmitting ? (
            <>
              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
              Posting...
            </>
          ) : (
            'Post Comment'
          )}
        </Button>
      </div>
    </form>
  );
}

