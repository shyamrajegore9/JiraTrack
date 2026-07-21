import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { AuthService } from '../../core/services/auth.service';
import { ProfilePictureUploadComponent } from './profile-picture-upload/profile-picture-upload.component';

@Component({
  selector: 'app-profile',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTabsModule,
    ProfilePictureUploadComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  profilePictureUrl = signal<string | undefined>(undefined);

  profileForm = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: [''],
    timeZone: ['UTC', Validators.required]
  });

  passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  });

  ngOnInit(): void {
    this.authService.getProfile().subscribe(user => {
      this.profilePictureUrl.set(user.profilePictureUrl);
      this.profileForm.patchValue({
        firstName: user.firstName,
        lastName: user.lastName,
        phoneNumber: user.phoneNumber ?? '',
        timeZone: user.timeZone
      });
    });
  }

  onPictureUploaded(url: string): void {
    this.profilePictureUrl.set(url || undefined);
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.authService.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: () => this.snackBar.open('Profile updated', 'Close', { duration: 3000 })
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;
    const value = this.passwordForm.getRawValue();
    if (value.newPassword !== value.confirmPassword) {
      this.snackBar.open('Passwords do not match', 'Close', { duration: 3000 });
      return;
    }
    this.authService.changePassword(value).subscribe({
      next: () => {
        this.snackBar.open('Password changed', 'Close', { duration: 3000 });
        this.passwordForm.reset();
      }
    });
  }
}
