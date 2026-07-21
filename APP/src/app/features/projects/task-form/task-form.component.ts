import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { TaskService } from '../../../core/services/task.service';
import { ProjectService } from '../../../core/services/project.service';
import { ProjectMember } from '../../../core/models/project.model';
import { TASK_PRIORITIES, TASK_STATUSES, STATUS_LABELS } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private projectService = inject(ProjectService);

  isEdit = false;
  projectId = 0;
  taskId?: number;
  members: ProjectMember[] = [];
  loading = false;
  statuses = TASK_STATUSES;
  priorities = TASK_PRIORITIES;
  statusLabels = STATUS_LABELS;

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    description: [''],
    acceptanceCriteria: [''],
    status: ['Todo'],
    priority: ['Medium'],
    assigneeId: [null as number | null],
    storyPoints: [null as number | null],
    estimatedHours: [null as number | null],
    actualHours: [0],
    startDate: [null as Date | null],
    dueDate: [null as Date | null]
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.projectService.getMembers(this.projectId).subscribe(m => this.members = m);

    const taskIdParam = this.route.snapshot.paramMap.get('taskId');
    if (taskIdParam && taskIdParam !== 'new') {
      this.isEdit = true;
      this.taskId = +taskIdParam;
      this.form.controls.status.disable();
      this.taskService.getTask(this.projectId, this.taskId).subscribe(task => {
        this.form.patchValue({
          title: task.title,
          description: task.description ?? '',
          acceptanceCriteria: task.acceptanceCriteria ?? '',
          status: task.status,
          priority: task.priority,
          assigneeId: task.assigneeId ?? null,
          storyPoints: task.storyPoints ?? null,
          estimatedHours: task.estimatedHours ?? null,
          actualHours: task.actualHours,
          startDate: task.startDate ? new Date(task.startDate) : null,
          dueDate: task.dueDate ? new Date(task.dueDate) : null
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

    if (this.isEdit && this.taskId) {
      this.taskService.updateTask(this.projectId, this.taskId, {
        title: value.title,
        description: value.description || undefined,
        acceptanceCriteria: value.acceptanceCriteria || undefined,
        priority: value.priority,
        assigneeId: value.assigneeId ?? undefined,
        storyPoints: value.storyPoints ?? undefined,
        estimatedHours: value.estimatedHours ?? undefined,
        actualHours: value.actualHours,
        startDate: value.startDate?.toISOString(),
        dueDate: value.dueDate?.toISOString()
      }).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/app/projects', this.projectId, 'tasks', this.taskId]);
        },
        error: () => this.loading = false
      });
    } else {
      this.taskService.createTask(this.projectId, {
        title: value.title,
        description: value.description || undefined,
        acceptanceCriteria: value.acceptanceCriteria || undefined,
        status: value.status,
        priority: value.priority,
        assigneeId: value.assigneeId ?? undefined,
        storyPoints: value.storyPoints ?? undefined,
        estimatedHours: value.estimatedHours ?? undefined,
        startDate: value.startDate?.toISOString(),
        dueDate: value.dueDate?.toISOString()
      }).subscribe({
        next: task => {
          this.loading = false;
          this.router.navigate(['/app/projects', this.projectId, 'tasks', task.id]);
        },
        error: () => this.loading = false
      });
    }
  }
}
