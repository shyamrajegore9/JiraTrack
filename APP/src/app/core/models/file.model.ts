export type AttachmentEntityType = 'Task' | 'Bug' | 'Comment' | 'User';

export type FileTypeCategory = 'Image' | 'Document' | 'Video';

export interface Attachment {
  id: number;
  entityType: string;
  entityId: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  fileType: FileTypeCategory | number;
  uploadedBy: number;
  uploadedByName: string;
  uploadedDate: string;
}

export const FILE_ACCEPT =
  '.jpg,.jpeg,.png,.gif,.webp,.pdf,.docx,.xlsx,.mp4,.webm';

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function isImageAttachment(attachment: Attachment): boolean {
  return attachment.contentType.startsWith('image/') ||
    attachment.fileType === 'Image' ||
    attachment.fileType === 1;
}

export function isVideoAttachment(attachment: Attachment): boolean {
  return attachment.contentType.startsWith('video/') ||
    attachment.fileType === 'Video' ||
    attachment.fileType === 3;
}
