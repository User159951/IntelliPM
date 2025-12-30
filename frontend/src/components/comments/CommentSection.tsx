import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { MessageSquare } from 'lucide-react';
import { commentsApi, type Comment } from '@/api/comments';
import { showToast, showError } from '@/lib/sweetalert';
import CommentForm from './CommentForm';
import CommentItem from './CommentItem';
import { Skeleton } from '@/components/ui/skeleton';
import { Card, CardContent } from '@/components/ui/card';

interface CommentSectionProps {
  entityType: 'Task' | 'Project' | 'Sprint' | 'Defect';
  entityId: number;
}

function CommentSectionSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="h-6 w-32" />
      <Skeleton className="h-24 w-full" />
      <div className="space-y-3">
        <Skeleton className="h-20 w-full" />
        <Skeleton className="h-20 w-full" />
      </div>
    </div>
  );
}

function EmptyState() {
  return (
    <Card>
      <CardContent className="flex flex-col items-center justify-center py-8 text-center">
        <MessageSquare className="h-12 w-12 text-muted-foreground mb-4" />
        <p className="text-sm font-medium text-muted-foreground">No comments yet</p>
        <p className="text-xs text-muted-foreground mt-1">Be the first to comment!</p>
      </CardContent>
    </Card>
  );
}

export default function CommentSection({ entityType, entityId }: CommentSectionProps) {
  const queryClient = useQueryClient();

  const { data: comments, isLoading, refetch } = useQuery({
    queryKey: ['comments', entityType, entityId],
    queryFn: () => commentsApi.getAll(entityType, entityId),
    staleTime: 1000 * 30, // 30 seconds
    refetchInterval: 1000 * 60, // Auto-refresh every 1 minute
  });

  const addCommentMutation = useMutation({
    mutationFn: (data: { content: string; parentCommentId?: number }) =>
      commentsApi.add(entityType, entityId, data),
    onSuccess: () => {
      showToast('Comment added successfully', 'success');
      queryClient.invalidateQueries({ queryKey: ['comments', entityType, entityId] });
    },
    onError: (error: any) => {
      showError('Failed to add comment', error.message || 'An error occurred');
    },
  });

  const updateCommentMutation = useMutation({
    mutationFn: ({ commentId, content }: { commentId: number; content: string }) =>
      commentsApi.update(commentId, content),
    onSuccess: () => {
      showToast('Comment updated successfully', 'success');
      queryClient.invalidateQueries({ queryKey: ['comments', entityType, entityId] });
    },
    onError: (error: any) => {
      showError('Failed to update comment', error.message || 'An error occurred');
    },
  });

  const deleteCommentMutation = useMutation({
    mutationFn: (commentId: number) => commentsApi.delete(commentId),
    onSuccess: () => {
      showToast('Comment deleted successfully', 'success');
      queryClient.invalidateQueries({ queryKey: ['comments', entityType, entityId] });
    },
    onError: (error: any) => {
      showError('Failed to delete comment', error.message || 'An error occurred');
    },
  });

  if (isLoading) {
    return <CommentSectionSkeleton />;
  }

  const topLevelComments = comments?.filter((c) => !c.parentCommentId) || [];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">
          Comments ({comments?.length || 0})
        </h3>
      </div>

      {/* Add comment form */}
      <CommentForm
        onSubmit={(content) => addCommentMutation.mutate({ content })}
        isSubmitting={addCommentMutation.isPending}
      />

      {/* Comment list */}
      <div className="space-y-4">
        {topLevelComments.length > 0 ? (
          topLevelComments.map((comment) => (
            <CommentItem
              key={comment.id}
              comment={comment}
              allComments={comments || []}
              onReply={(parentId, content) =>
                addCommentMutation.mutate({ content, parentCommentId: parentId })
              }
              onEdit={(commentId, content) => {
                updateCommentMutation.mutate({ commentId, content });
              }}
              onDelete={(commentId) => {
                deleteCommentMutation.mutate(commentId);
              }}
            />
          ))
        ) : (
          <EmptyState />
        )}
      </div>
    </div>
  );
}

