import { apiClient } from './client';

// Aligned with backend Comment entity and AddCommentResponse
// NOTE: CommentsController does not exist yet in backend - endpoints will fail until controller is created
export interface Comment {
  id: number;
  entityType: string;
  entityId: number;
  content: string;
  authorId: number;
  authorName: string; // Not in entity, populated from Author navigation
  createdAt: string; // Backend: DateTimeOffset -> ISO string
  updatedAt?: string | null; // Backend: DateTimeOffset? -> ISO string | null
  isEdited: boolean;
  parentCommentId?: number | null;
}

export interface AddCommentRequest {
  entityType: string;
  entityId: number;
  content: string;
  parentCommentId?: number;
}

// Aligned with backend AddCommentResponse
export interface AddCommentResponse {
  commentId: number; // Backend: CommentId
  authorId: number; // Backend: AuthorId
  authorName: string; // Backend: AuthorName
  content: string; // Backend: Content
  createdAt: string; // Backend: DateTimeOffset -> ISO string
  mentionedUserIds: number[]; // Backend: List<int> MentionedUserIds
}

export interface UpdateCommentRequest {
  content: string;
}

export const commentsApi = {
  /**
   * Get all comments for a specific entity
   * Uses query parameters: /Comments?entityType={type}&entityId={id}
   * 
   * @param entityType - Type of entity (e.g., 'Task', 'Project', 'Sprint')
   * @param entityId - ID of the entity
   * @returns Array of comments for the specified entity
   * 
   * @example
   * // Get all comments for a task
   * const comments = await commentsApi.getAll('Task', 123);
   * 
   * // Get all comments for a project
   * const comments = await commentsApi.getAll('Project', 456);
   */
  getAll: async (entityType: string, entityId: number): Promise<Comment[]> => {
    // Build query parameters (not path parameters)
    // Endpoint: GET /api/v1/Comments?entityType={type}&entityId={id}
    const params = new URLSearchParams();
    params.append('entityType', entityType);
    params.append('entityId', entityId.toString());
    
    const queryString = params.toString();
    return apiClient.get<Comment[]>(`/Comments?${queryString}`);
  },

  /**
   * Add a new comment to an entity
   * Uses POST body with entityType and entityId (not path parameters)
   * 
   * @param entityType - Type of entity (e.g., 'Task', 'Project', 'Sprint')
   * @param entityId - ID of the entity
   * @param data - Comment data (content and optional parentCommentId)
   * @returns Created comment response
   * 
   * @example
   * // Add a comment to a task
   * const comment = await commentsApi.add('Task', 123, {
   *   content: 'This is a comment',
   *   parentCommentId: undefined // Optional: for threaded comments
   * });
   */
  add: async (
    entityType: string,
    entityId: number,
    data: { content: string; parentCommentId?: number }
  ): Promise<AddCommentResponse> => {
    // Endpoint: POST /api/v1/Comments
    // Body: { entityType, entityId, content, parentCommentId? }
    return apiClient.post<AddCommentResponse>('/Comments', {
      entityType,
      entityId,
      ...data,
    });
  },

  /**
   * Update an existing comment
   * Uses path parameter for commentId: PUT /Comments/{commentId}
   * 
   * @param commentId - ID of the comment to update
   * @param content - New content for the comment
   * @returns Updated comment
   */
  update: async (commentId: number, content: string): Promise<Comment> => {
    // Endpoint: PUT /api/v1/Comments/{commentId}
    // Body: { content }
    return apiClient.put<Comment>(`/Comments/${commentId}`, { content });
  },

  /**
   * Delete a comment
   * Uses path parameter for commentId: DELETE /Comments/{commentId}
   * 
   * @param commentId - ID of the comment to delete
   */
  delete: async (commentId: number): Promise<void> => {
    // Endpoint: DELETE /api/v1/Comments/{commentId}
    return apiClient.delete(`/Comments/${commentId}`);
  },
};

