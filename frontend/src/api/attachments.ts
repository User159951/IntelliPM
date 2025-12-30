import { apiClient } from './client';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001';
const API_VERSION = '/api/v1';

// Aligned with backend Attachment entity
// NOTE: AttachmentsController does not exist yet in backend - endpoints will fail until controller is created
export interface Attachment {
  id: number;
  entityType: string;
  entityId: number;
  fileName: string; // Backend: FileName (original filename)
  fileExtension: string; // Backend: FileExtension
  fileSizeBytes: number; // Backend: FileSizeBytes (long -> number)
  contentType: string; // Backend: ContentType
  uploadedById: number; // Backend: UploadedById
  uploadedBy: string; // Not in entity, populated from UploadedBy navigation
  uploadedAt: string; // Backend: DateTimeOffset -> ISO string
}

export const attachmentsApi = {
  /**
   * Get all attachments for a specific entity
   * Uses query parameters: /Attachments?entityType={type}&entityId={id}
   * 
   * @param entityType - Type of entity (e.g., 'Task', 'Project', 'Sprint')
   * @param entityId - ID of the entity
   * @returns Array of attachments for the specified entity
   * 
   * @example
   * // Get all attachments for a task
   * const attachments = await attachmentsApi.getAll('Task', 123);
   * 
   * // Get all attachments for a project
   * const attachments = await attachmentsApi.getAll('Project', 456);
   */
  getAll: (entityType: string, entityId: number): Promise<Attachment[]> => {
    // Build query parameters (not path parameters)
    // Endpoint: GET /api/v1/Attachments?entityType={type}&entityId={id}
    const params = new URLSearchParams();
    params.append('entityType', entityType);
    params.append('entityId', entityId.toString());
    
    const queryString = params.toString();
    return apiClient.get<Attachment[]>(`/Attachments?${queryString}`);
  },

  /**
   * Upload a new attachment
   * Uses POST with FormData: POST /Attachments/upload
   * 
   * @param formData - FormData containing the file and metadata (entityType, entityId)
   * @param onProgress - Optional callback for upload progress (0-100)
   * @returns Created attachment
   * 
   * @example
   * // Upload a file for a task
   * const formData = new FormData();
   * formData.append('file', file);
   * formData.append('entityType', 'Task');
   * formData.append('entityId', '123');
   * const attachment = await attachmentsApi.upload(formData, (progress) => {
   *   console.log(`Upload progress: ${progress}%`);
   * });
   */
  upload: (
    formData: FormData,
    onProgress?: (progress: number) => void
  ): Promise<Attachment> => {
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();

      // Upload progress
      xhr.upload.addEventListener('progress', (e) => {
        if (e.lengthComputable && onProgress) {
          const progress = (e.loaded / e.total) * 100;
          onProgress(progress);
        }
      });

      // Success
      xhr.addEventListener('load', () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          try {
            const response = JSON.parse(xhr.responseText);
            resolve(response);
          } catch (error) {
            reject(new Error('Failed to parse response'));
          }
        } else {
          try {
            const error = JSON.parse(xhr.responseText);
            reject(new Error(error.message || error.detail || `Upload failed: ${xhr.statusText}`));
          } catch {
            reject(new Error(`Upload failed: ${xhr.statusText}`));
          }
        }
      });

      // Error
      xhr.addEventListener('error', () => {
        reject(new Error('Upload failed: Network error'));
      });

      // Abort
      xhr.addEventListener('abort', () => {
        reject(new Error('Upload cancelled'));
      });

      // Build endpoint URL
      // Endpoint: POST /api/v1/Attachments/upload
      const endpoint = `/Attachments/upload`;
      const versionedEndpoint = endpoint.startsWith('/api/v')
        ? endpoint
        : endpoint.startsWith('/api/admin/')
          ? endpoint
          : endpoint.startsWith('/api/')
            ? endpoint.replace('/api/', `${API_VERSION}/`)
            : `${API_VERSION}${endpoint}`;

      xhr.open('POST', `${API_BASE_URL}${versionedEndpoint}`);
      
      // Don't set Content-Type header - browser will set it with boundary for FormData
      // Authentication is handled via cookies (credentials: 'include' is set by default)
      xhr.withCredentials = true; // Include cookies for CORS

      xhr.send(formData);
    });
  },

  /**
   * Download an attachment by ID
   * Uses path parameter: GET /Attachments/{id} (no /download suffix)
   * 
   * @param attachmentId - ID of the attachment to download
   * 
   * @example
   * // Download attachment with ID 123
   * attachmentsApi.download(123);
   */
  download: (attachmentId: number): void => {
    // Endpoint: GET /api/v1/Attachments/{id} (NOT /Attachments/{id}/download)
    const endpoint = `/Attachments/${attachmentId}`;
    const versionedEndpoint = endpoint.startsWith('/api/v')
      ? endpoint
      : endpoint.startsWith('/api/admin/')
        ? endpoint
        : endpoint.startsWith('/api/')
          ? endpoint.replace('/api/', `${API_VERSION}/`)
          : `${API_VERSION}${endpoint}`;

    const url = `${API_BASE_URL}${versionedEndpoint}`;

    // Open in new tab for download
    // Cookies will be sent automatically by the browser
    window.open(url, '_blank');
  },

  /**
   * Delete an attachment by ID
   * Uses path parameter: DELETE /Attachments/{id}
   * 
   * @param attachmentId - ID of the attachment to delete
   * 
   * @example
   * // Delete attachment with ID 123
   * await attachmentsApi.delete(123);
   */
  delete: (attachmentId: number): Promise<void> => {
    // Endpoint: DELETE /api/v1/Attachments/{id}
    return apiClient.delete(`/Attachments/${attachmentId}`);
  },
};

