import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProjectService } from '../../../core/services/project.service';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-project-settings',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule
  ],
  templateUrl: './project-settings.component.html',
  styleUrl: './project-settings.component.scss'
})
export class ProjectSettingsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);
  private snackBar = inject(MatSnackBar);
  private tokenService = inject(TokenService);

  canManage = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  projectId = 0;
  loading = false;

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['']
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.projectService.getProject(this.projectId).subscribe(project => {
      this.form.patchValue({
        name: project.name,
        description: project.description ?? ''
      });
      if (!this.canManage) this.form.disable();
    });
  }

  submit(): void {
    if (this.form.invalid || !this.canManage) return;
    this.loading = true;
    const value = this.form.getRawValue();
    this.projectService.updateSettings(this.projectId, {
      name: value.name,
      description: value.description || undefined
    }).subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open('Settings saved', 'Close', { duration: 3000 });
      },
      error: () => this.loading = false
    });
  }
}
