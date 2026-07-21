import { Component, OnDestroy, effect, inject, input, output, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FileService } from '../../../core/services/file.service';
import { TokenService } from '../../../core/services/token.service';
import { ProfilePictureCropDialogComponent } from './profile-picture-crop-dialog.component';

@Component({
  selector: 'app-profile-picture-upload',
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule],
  templateUrl: './profile-picture-upload.component.html',
  styleUrl: './profile-picture-upload.component.scss'
})
export class ProfilePictureUploadComponent implements OnDestroy {
  private fileService = inject(FileService);
  private tokenService = inject(TokenService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  readonly profilePictureUrl = input<string | undefined>();
  readonly uploaded = output<string>();

  previewUrl = signal<string | null>(null);
  uploading = signal(false);

  constructor() {
    effect(() => {
      this.loadPreviewFromUrl(this.profilePictureUrl());
    });
  }

  ngOnDestroy(): void {
    this.revokePreview();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || !file.type.startsWith('image/')) {
      this.snackBar.open('Please select an image file', 'Close', { duration: 3000 });
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const dialogRef = this.dialog.open(ProfilePictureCropDialogComponent, {
        data: reader.result as string,
        width: '420px'
      });

      dialogRef.afterClosed().subscribe((blob: Blob | undefined) => {
        if (blob) this.uploadBlob(blob, file.name);
      });
    };
    reader.readAsDataURL(file);
  }

  loadPreviewFromUrl(url?: string): void {
    this.revokePreview();
    if (!url) return;

    const fileId = this.fileService.extractFileIdFromUrl(url);
    if (fileId) {
      this.fileService.downloadBlob(fileId).subscribe(blob => {
        this.previewUrl.set(URL.createObjectURL(blob));
      });
    }
  }

  private uploadBlob(blob: Blob, originalName: string): void {
    this.uploading.set(true);
    this.fileService.uploadProfilePicture(blob, originalName).subscribe({
      next: user => {
        this.uploading.set(false);
        this.tokenService.updateUser(user);
        this.uploaded.emit(user.profilePictureUrl ?? '');
        this.loadPreviewFromUrl(user.profilePictureUrl);
        this.snackBar.open('Profile picture updated', 'Close', { duration: 3000 });
      },
      error: err => {
        this.uploading.set(false);
        this.snackBar.open(err.message ?? 'Upload failed', 'Close', { duration: 4000 });
      }
    });
  }

  private revokePreview(): void {
    const url = this.previewUrl();
    if (url) URL.revokeObjectURL(url);
    this.previewUrl.set(null);
  }
}
