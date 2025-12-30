import { useState } from 'react';
import { MessageSquare, MoreHorizontal, Pencil, Trash2 } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { useAuth } from '@/contexts/AuthContext';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { cn } from '@/lib/utils';
import CommentForm from './CommentForm';
import type { Comment } from '@/api/comments';

interface CommentItemProps {
  comment: Comment;
  allComments: Comment[];
  onReply: (parentId: number, content: string) => void;
  onEdit: (commentId: number, content: string) => void;
  onDelete: (commentId: number) => void;
  depth?: number;
}

export default function CommentItem({
  comment,
  allComments,
  onReply,
  onEdit,
  onDelete,
  depth = 0,
}: CommentItemProps) {
  const [showReplyForm, setShowReplyForm] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const { user } = useAuth();

  const replies = allComments.filter((c) => c.parentCommentId === comment.id);
  const isOwnComment = user?.userId === comment.authorId;
  const maxDepth = 3;

  const handleReply = (content: string) => {
    onReply(comment.id, content);
    setShowReplyForm(false);
  };

  const handleEdit = (content: string) => {
    onEdit(comment.id, content);
    setIsEditing(false);
  };

  // Render comment content with highlighted mentions
  const renderContent = (content: string) => {
    const mentionRegex = /@([a-zA-Z0-9._-]+)/g;
    const parts: (string | JSX.Element)[] = [];
    let lastIndex = 0;
    let match;
    let key = 0;

    while ((match = mentionRegex.exec(content)) !== null) {
      // Add text before mention
      if (match.index > lastIndex) {
        parts.push(content.substring(lastIndex, match.index));
      }

      // Add mention
      parts.push(
        <span key={key++} className="text-primary font-medium">
          @{match[1]}
        </span>
      );

      lastIndex = mentionRegex.lastIndex;
    }

    // Add remaining text
    if (lastIndex < content.length) {
      parts.push(content.substring(lastIndex));
    }

    return parts.length > 0 ? parts : content;
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .filter(Boolean)
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <div className={cn('flex gap-3', depth > 0 && 'ml-12')}>
      <Avatar className="h-8 w-8">
        <AvatarFallback className="text-xs">
          {getInitials(comment.authorName)}
        </AvatarFallback>
      </Avatar>

      <div className="flex-1 space-y-2">
        <div className="bg-muted rounded-lg p-3">
          <div className="flex items-center justify-between mb-1">
            <div className="flex items-center gap-2">
              <span className="font-semibold text-sm">{comment.authorName}</span>
              <span className="text-xs text-muted-foreground">
                {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
              </span>
              {comment.isEdited && (
                <span className="text-xs text-muted-foreground">(edited)</span>
              )}
            </div>

            {isOwnComment && (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="sm" className="h-6 w-6 p-0">
                    <MoreHorizontal className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => setIsEditing(true)}>
                    <Pencil className="h-4 w-4 mr-2" />
                    Edit
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onClick={() => onDelete(comment.id)}
                    className="text-destructive"
                  >
                    <Trash2 className="h-4 w-4 mr-2" />
                    Delete
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            )}
          </div>

          {isEditing ? (
            <CommentForm
              initialValue={comment.content}
              onSubmit={handleEdit}
              isSubmitting={false}
              placeholder="Edit your comment..."
              autoFocus
            />
          ) : (
            <p className="text-sm whitespace-pre-wrap">{renderContent(comment.content)}</p>
          )}
        </div>

        <div className="flex items-center gap-4 text-sm">
          {depth < maxDepth && (
            <button
              onClick={() => setShowReplyForm(!showReplyForm)}
              className="text-muted-foreground hover:text-foreground transition-colors"
            >
              <MessageSquare className="h-4 w-4 inline mr-1" />
              Reply
            </button>
          )}

          {replies.length > 0 && (
            <span className="text-muted-foreground">
              {replies.length} {replies.length === 1 ? 'reply' : 'replies'}
            </span>
          )}
        </div>

        {showReplyForm && (
          <div className="mt-2">
            <CommentForm
              onSubmit={handleReply}
              isSubmitting={false}
              placeholder="Write a reply..."
              autoFocus
            />
          </div>
        )}

        {/* Nested replies */}
        {replies.length > 0 && (
          <div className="space-y-4 mt-4">
            {replies.map((reply) => (
              <CommentItem
                key={reply.id}
                comment={reply}
                allComments={allComments}
                onReply={onReply}
                onEdit={onEdit}
                onDelete={onDelete}
                depth={depth + 1}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

