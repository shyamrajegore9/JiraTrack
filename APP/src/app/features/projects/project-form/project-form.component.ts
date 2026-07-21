import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ProjectService } from '../../../core/services/project.service';
import { UserLookupDto } from '../../../core/models/user.model';
import { TokenService } from '../../../core/services/token.service';

@Component({
  selector: 'app-project-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule
  ],
  templateUrl: './project-form.component.html',
  styleUrl: './project-form.component.scss'
})
export class ProjectFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private projectService = inject(ProjectService);
  private tokenService = inject(TokenService);

  isEdit = false;
  projectId?: number;
  users: UserLookupDto[] = [];
  loading = false;

  form = this.fb.nonNullable.group({
    key: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(10), Validators.pattern(/^[A-Z0-9]+$/)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    leadUserId: [0, Validators.required]
  });

  ngOnInit(): void {
    const currentUser = this.tokenService.currentUser();
    if (currentUser) {
      this.form.patchValue({ leadUserId: currentUser.id });
    }

    this.projectService.getUserLookup().subscribe(users => {
      this.users = users;
      if (!this.users.some(u => u.id === this.form.value.leadUserId) && this.users.length) {
        this.form.patchValue({ leadUserId: this.users[0].id });
      }
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit = true;
      this.projectId = +id;
      this.form.controls.key.disable();
      this.projectService.getProject(this.projectId).subscribe(project => {
        this.form.patchValue({
          key: project.key,
          name: project.name,
          description: project.description ?? '',
          leadUserId: project.leadUserId
        });
      });
    }
  }

  onKeyInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.form.controls.key.setValue(input.value.toUpperCase(), { emitEvent: false });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const value = this.form.getRawValue();

    if (this.isEdit && this.projectId) {
      this.projectService.updateProject(this.projectId, {
        name: value.name,
        description: value.description || undefined,
        leadUserId: value.leadUserId
      }).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/app/projects', this.projectId, 'dashboard']);
        },
        error: () => this.loading = false
      });
    } else {
      this.projectService.createProject({
        key: value.key,
        name: value.name,
        description: value.description || undefined,
        leadUserId: value.leadUserId
      }).subscribe({
        next: project => {
          this.loading = false;
          this.router.navigate(['/app/projects', project.id, 'dashboard']);
        },
        error: () => this.loading = false
      });
    }
  }
}
