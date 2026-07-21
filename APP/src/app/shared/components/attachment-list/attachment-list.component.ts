import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FileService } from '../../../core/services/file.service';
import {
  Attachment,
  AttachmentEntityType,
  formatFileSize,
  isImageAttachment,
  isVideoAttachment
} from '../../../core/models/file.model';
import { FileUploadComponent } from '../file-upload/file-upload.component';
import { TokenService } from '../../../core/services/token.service';

@Component({
  selector: 'app-attachment-list',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    FileUploadComponent
  ],
  templateUrl: './attachment-list.component.html',
  styleUrl: './attachment-list.component.scss'
})
export class AttachmentListComponent implements OnInit, OnDestroy {
  private fileService = inject(FileService);
  private snackBar = inject(MatSnackBar);
  private tokenService = inject(TokenService);

  readonly entityType = input.required<AttachmentEntityType>();
  readonly entityId = input.required<number>();
  readonly canWrite = input(true);

  attachments = signal<Attachment[]>([]);
  loading = signal(false);
  uploading = signal(false);
  previewUrls = signal<Record<number, string>>({});

  readonly formatFileSize = formatFileSize;
  readonly isImage = isImageAttachment;
  readonly isVideo = isVideoAttachment;

  ngOnInit(): void {
    this.loadAttachments();
  }

  ngOnDestroy(): void {
    this.revokePreviews();
  }

  loadAttachments(): void {
    this.loading.set(true);
    this.fileService.getAttachments(this.entityType(), this.entityId()).subscribe({
      next: items => {
        this.revokePreviews();
        this.attachments.set(items);
        this.loadPreviews(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onFileSelected(file: File): void {
    this.uploading.set(true);
    this.fileService.upload(file, this.entityType(), this.entityId()).subscribe({
      next: () => {
        this.uploading.set(false);
        this.snackBar.open('File uploaded', 'Close', { duration: 3000 });
        this.loadAttachments();
      },
      error: err => {
        this.uploading.set(false);
        this.snackBar.open(err.message ?? 'Upload failed', 'Close', { duration: 4000 });
      }
    });
  }

  download(attachment: Attachment): void {
    this.fileService.downloadBlob(attachment.id).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = attachment.fileName;
      anchor.click();
      URL.revokeObjectURL(url);
    });
  }

  deleteAttachment(attachment: Attachment): void {
    if (!confirm(`Delete ${attachment.fileName}?`)) return;
    this.fileService.delete(attachment.id).subscribe({
      next: () => {
        this.snackBar.open('File deleted', 'Close', { duration: 3000 });
        this.loadAttachments();
      }
    });
  }

  canDelete(attachment: Attachment): boolean {
    const user = this.tokenService.currentUser();
    return !!user && (user.roles.includes('Admin') || attachment.uploadedBy === user.id);
  }

  private loadPreviews(items: Attachment[]): void {
    items.filter(isImageAttachment).forEach(item => {
      this.fileService.downloadBlob(item.id).subscribe(blob => {
        this.previewUrls.update(current => ({
          ...current,
          [item.id]: URL.createObjectURL(blob)
        }));
      });
    });
  }

  private revokePreviews(): void {
    Object.values(this.previewUrls()).forEach(url => URL.revokeObjectURL(url));
    this.previewUrls.set({});
  }
}
