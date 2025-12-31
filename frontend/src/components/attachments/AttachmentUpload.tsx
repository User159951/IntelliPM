import { useState, useRef } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Upload, FileIcon, X, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { cn } from '@/lib/utils';
import { attachmentsApi } from '@/api/attachments';
import { showToast, showError } from '@/lib/sweetalert';

interface AttachmentUploadProps {
  entityType: 'Task' | 'Project' | 'Comment' | 'Defect';
  entityId: number;
  onUploadComplete?: () => void;
  maxFileSize?: number; // in MB
  allowedExtensions?: string[];
}

export default function AttachmentUpload({
  entityType,
  entityId,
  onUploadComplete,
  maxFileSize = 10,
  allowedExtensions = [
    '.pdf',
    '.doc',
    '.docx',
    '.xls',
    '.xlsx',
    '.ppt',
    '.pptx',
    '.txt',
    '.csv',
    '.jpg',
    '.jpeg',
    '.png',
    '.gif',
    '.zip',
  ],
}: AttachmentUploadProps) {
  const [isDragging, setIsDragging] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [uploadProgress, setUploadProgress] = useState<Record<string, number>>({});
  const fileInputRef = useRef<HTMLInputElement>(null);

  const uploadMutation = useMutation({
    mutationFn: (file: File) => {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('entityType', entityType);
      formData.append('entityId', entityId.toString());

      return attachmentsApi.upload(formData, (progress) => {
        setUploadProgress((prev) => ({ ...prev, [file.name]: progress }));
      });
    },
    onSuccess: (_data, file) => {
      showToast(`${file.name} uploaded successfully`, 'success');
      setSelectedFiles((prev) => prev.filter((f) => f.name !== file.name));
      setUploadProgress((prev) => {
        const newProgress = { ...prev };
        delete newProgress[file.name];
        return newProgress;
      });
      onUploadComplete?.();
    },
    onError: (error: unknown, file: File) => {
      const apiError = error as { message?: string };
      showError(`Failed to upload ${file.name}`, apiError.message || 'An error occurred');
      setUploadProgress((prev) => {
        const newProgress = { ...prev };
        delete newProgress[file.name];
        return newProgress;
      });
    },
  });

  const validateFile = (file: File): string | null => {
    // Check file extension
    const extension = `.${file.name.split('.').pop()?.toLowerCase()}`;
    if (!allowedExtensions.includes(extension)) {
      return `File type ${extension} is not allowed`;
    }

    // Check file size
    const maxSizeBytes = maxFileSize * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      return `File size exceeds ${maxFileSize}MB limit`;
    }

    return null;
  };

  const handleFiles = (files: FileList | null) => {
    if (!files) return;

    const validFiles: File[] = [];

    Array.from(files).forEach((file) => {
      const error = validateFile(file);
      if (error) {
        showError(`Invalid file: ${file.name}`, error);
      } else {
        validFiles.push(file);
      }
    });

    if (validFiles.length > 0) {
      setSelectedFiles((prev) => [...prev, ...validFiles]);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    handleFiles(e.dataTransfer.files);
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    handleFiles(e.target.files);
    // Reset input to allow selecting the same file again
    if (e.target) {
      e.target.value = '';
    }
  };

  const handleUploadAll = () => {
    selectedFiles.forEach((file) => {
      uploadMutation.mutate(file);
    });
  };

  const handleRemoveFile = (fileName: string) => {
    setSelectedFiles((prev) => prev.filter((f) => f.name !== fileName));
    setUploadProgress((prev) => {
      const newProgress = { ...prev };
      delete newProgress[fileName];
      return newProgress;
    });
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  };

  const hasUploadingFiles = Object.values(uploadProgress).some((progress) => progress > 0 && progress < 100);

  return (
    <div className="space-y-4">
      {/* Drop zone */}
      <div
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={() => fileInputRef.current?.click()}
        className={cn(
          'border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors',
          isDragging
            ? 'border-primary bg-primary/10'
            : 'border-muted-foreground/25 hover:border-primary/50'
        )}
      >
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept={allowedExtensions.join(',')}
          onChange={handleFileInputChange}
          className="hidden"
        />

        <Upload className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
        <p className="text-lg font-medium mb-2">
          {isDragging ? 'Drop files here' : 'Drop files or click to upload'}
        </p>
        <p className="text-sm text-muted-foreground">
          Supported formats: {allowedExtensions.join(', ')}
        </p>
        <p className="text-xs text-muted-foreground mt-1">
          Maximum file size: {maxFileSize}MB
        </p>
      </div>

      {/* Selected files list */}
      {selectedFiles.length > 0 && (
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <h4 className="font-medium">
              Selected Files ({selectedFiles.length})
            </h4>
            <Button
              onClick={handleUploadAll}
              disabled={uploadMutation.isPending || hasUploadingFiles}
            >
              {uploadMutation.isPending || hasUploadingFiles ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Uploading...
                </>
              ) : (
                <>
                  <Upload className="h-4 w-4 mr-2" />
                  Upload All
                </>
              )}
            </Button>
          </div>

          <div className="space-y-2">
            {selectedFiles.map((file) => {
              const progress = uploadProgress[file.name] || 0;
              const isUploading = progress > 0 && progress < 100;

              return (
                <Card key={file.name}>
                  <CardContent className="p-3">
                    <div className="flex items-center gap-3">
                      <FileIcon className="h-8 w-8 text-muted-foreground flex-shrink-0" />

                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">{file.name}</p>
                        <p className="text-xs text-muted-foreground">
                          {formatFileSize(file.size)}
                        </p>

                        {isUploading && (
                          <div className="mt-2">
                            <Progress value={progress} className="h-1" />
                            <p className="text-xs text-muted-foreground mt-1">
                              {progress.toFixed(0)}% uploaded
                            </p>
                          </div>
                        )}
                      </div>

                      {!isUploading && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRemoveFile(file.name)}
                          className="flex-shrink-0"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}

