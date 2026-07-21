import { Component, OnInit, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSliderModule } from '@angular/material/slider';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile-picture-crop-dialog',
  imports: [MatDialogModule, MatButtonModule, MatSliderModule, FormsModule],
  templateUrl: './profile-picture-crop-dialog.component.html',
  styleUrl: './profile-picture-crop-dialog.component.scss'
})
export class ProfilePictureCropDialogComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<ProfilePictureCropDialogComponent>);
  readonly imageSrc = inject<string>(MAT_DIALOG_DATA);

  zoom = signal(1);
  private image = new Image();

  ngOnInit(): void {
    this.image.src = this.imageSrc;
  }

  onZoomChange(value: number): void {
    this.zoom.set(value);
  }

  save(): void {
    this.image.onload = () => this.exportCrop();
    if (this.image.complete) this.exportCrop();
    else this.image.src = this.imageSrc;
  }

  private exportCrop(): void {
    const size = 256;
    const canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    const ctx = canvas.getContext('2d')!;

    const zoom = this.zoom();
    const minSide = Math.min(this.image.width, this.image.height);
    const cropSize = minSide / zoom;
    const sx = (this.image.width - cropSize) / 2;
    const sy = (this.image.height - cropSize) / 2;

    ctx.drawImage(this.image, sx, sy, cropSize, cropSize, 0, 0, size, size);

    canvas.toBlob(blob => {
      if (blob) this.dialogRef.close(blob);
    }, 'image/jpeg', 0.92);
  }
}
