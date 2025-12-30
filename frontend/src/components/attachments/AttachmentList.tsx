import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { format } from 'date-fns';
import {
  File,
  FileText,
  FileImage,
  FileSpreadsheet,
  FileArchive,
  Download,
  Trash2,
  Upload,
  X,
  Paperclip,
} from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { attachmentsApi, type Attachment } from '@/api/attachments';
import { showToast, showError, showConfirm } from '@/lib/sweetalert';
import AttachmentUpload from './AttachmentUpload';
import EmptyState from '@/components/ui/empty-state';

interface AttachmentListProps {
  entityType: 'Task' | 'Project' | 'Comment' | 'Defect';
  entityId: number;
  showUpload?: boolean;
}

function AttachmentListSkeleton() {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Skeleton className="h-6 w-32" />
        <Skeleton className="h-9 w-36" />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        {[...Array(4)].map((_, i) => (
          <Skeleton key={i} className="h-24" />
        ))}
      </div>
    </div>
  );
}

export default function AttachmentList({
  entityType,
  entityId,
  showUpload = true,
}: AttachmentListProps) {
  const [showUploadForm, setShowUploadForm] = useState(false);
  const { user } = useAuth();

  // Fetch attachments
  const { data: attachments, isLoading, refetch } = useQuery({
    queryKey: ['attachments', entityType, entityId],
    queryFn: () => attachmentsApi.getAll(entityType, entityId),
    staleTime: 1000 * 60, // 1 minute
  });

  // Delete attachment mutation
  const deleteMutation = useMutation({
    mutationFn: (attachmentId: number) => attachmentsApi.delete(attachmentId),
    onSuccess: () => {
      showToast('Attachment deleted successfully', 'success');
      refetch();
    },
    onError: (error: any) => {
      showError('Failed to delete attachment', error.message || 'An error occurred');
    },
  });

  const handleDownload = (attachment: Attachment) => {
    attachmentsApi.download(attachment.id);
  };

  const handleDelete = async (attachment: Attachment) => {
    const confirmed = await showConfirm(
      'Delete Attachment',
      `Are you sure you want to delete "${attachment.fileName}"?`
    );

    if (confirmed) {
      deleteMutation.mutate(attachment.id);
    }
  };

  const canDelete = (attachment: Attachment) => {
    return user?.userId === attachment.uploadedById || user?.globalRole === 'Admin';
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  };

  const getFileIcon = (extension: string) => {
    const iconClass = 'h-8 w-8';

    switch (extension.toLowerCase()) {
      case '.pdf':
        return <FileText className={`${iconClass} text-red-500`} />;
      case '.doc':
      case '.docx':
        return <FileText className={`${iconClass} text-blue-500`} />;
      case '.xls':
      case '.xlsx':
        return <FileSpreadsheet className={`${iconClass} text-green-500`} />;
      case '.ppt':
      case '.pptx':
        return <FileText className={`${iconClass} text-orange-500`} />;
      case '.jpg':
      case '.jpeg':
      case '.png':
      case '.gif':
      case '.bmp':
        return <FileImage className={`${iconClass} text-purple-500`} />;
      case '.zip':
      case '.rar':
      case '.7z':
        return <FileArchive className={`${iconClass} text-yellow-500`} />;
      default:
        return <File className={`${iconClass} text-gray-500`} />;
    }
  };

  if (isLoading) {
    return <AttachmentListSkeleton />;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">
          Attachments ({attachments?.length || 0})
        </h3>
        {showUpload && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowUploadForm(!showUploadForm)}
          >
            {showUploadForm ? (
              <>
                <X className="h-4 w-4 mr-2" />
                Cancel
              </>
            ) : (
              <>
                <Upload className="h-4 w-4 mr-2" />
                Add Attachment
              </>
            )}
          </Button>
        )}
      </div>

      {showUploadForm && (
        <AttachmentUpload
          entityType={entityType}
          entityId={entityId}
          onUploadComplete={() => {
            refetch();
            setShowUploadForm(false);
          }}
        />
      )}

      {attachments && attachments.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          {attachments.map((attachment) => (
            <Card key={attachment.id} className="hover:shadow-md transition-shadow">
              <CardContent className="p-4">
                <div className="flex items-start gap-3">
                  <div className="flex-shrink-0">
                    {getFileIcon(attachment.fileExtension)}
                  </div>

                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate" title={attachment.fileName}>
                      {attachment.fileName}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {formatFileSize(attachment.fileSizeBytes)}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      Uploaded by {attachment.uploadedBy}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {format(new Date(attachment.uploadedAt), 'MMM d, yyyy')}
                    </p>
                  </div>

                  <div className="flex flex-col gap-1">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleDownload(attachment)}
                      title="Download"
                      className="h-8 w-8 p-0"
                    >
                      <Download className="h-4 w-4" />
                    </Button>

                    {canDelete(attachment) && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDelete(attachment)}
                        title="Delete"
                        disabled={deleteMutation.isPending}
                        className="h-8 w-8 p-0"
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <EmptyState
          icon={Paperclip}
          message="No attachments"
          description={showUpload ? "Click 'Add Attachment' to upload files" : ''}
        />
      )}
    </div>
  );
}

