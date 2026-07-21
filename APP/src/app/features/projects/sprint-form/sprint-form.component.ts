import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { SprintService } from '../../../core/services/sprint.service';

@Component({
  selector: 'app-sprint-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './sprint-form.component.html',
  styleUrl: './sprint-form.component.scss'
})
export class SprintFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private sprintService = inject(SprintService);

  isEdit = false;
  projectId = 0;
  sprintId?: number;
  loading = false;

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    goal: [''],
    startDate: [null as Date | null],
    endDate: [null as Date | null]
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    const sprintIdParam = this.route.snapshot.paramMap.get('sprintId');
    if (sprintIdParam && sprintIdParam !== 'new') {
      this.isEdit = true;
      this.sprintId = +sprintIdParam;
      this.sprintService.getSprint(this.projectId, this.sprintId).subscribe(sprint => {
        this.form.patchValue({
          name: sprint.name,
          goal: sprint.goal ?? '',
          startDate: sprint.startDate ? new Date(sprint.startDate) : null,
          endDate: sprint.endDate ? new Date(sprint.endDate) : null
        });
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const value = this.form.getRawValue();
    const payload = {
      name: value.name,
      goal: value.goal || undefined,
      startDate: value.startDate?.toISOString(),
      endDate: value.endDate?.toISOString()
    };

    if (this.isEdit && this.sprintId) {
      this.sprintService.updateSprint(this.projectId, this.sprintId, payload).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/app/projects', this.projectId, 'sprints', this.sprintId]);
        },
        error: () => this.loading = false
      });
    } else {
      this.sprintService.createSprint(this.projectId, payload).subscribe({
        next: sprint => {
          this.loading = false;
          this.router.navigate(['/app/projects', this.projectId, 'sprints', sprint.id]);
        },
        error: () => this.loading = false
      });
    }
  }
}
