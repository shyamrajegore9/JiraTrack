import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TaskService } from '../../../core/services/task.service';
import {
  TaskDetail,
  TASK_STATUSES,
  STATUS_LABELS,
  PRIORITY_COLORS,
  ChecklistItem
} from '../../../core/models/task.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';
import { CommentThreadComponent } from '../../comments/comment-thread/comment-thread.component';
import { AttachmentListComponent } from '../../../shared/components/attachment-list/attachment-list.component';

@Component({
  selector: 'app-task-detail',
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatCardModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    CommentThreadComponent,
    AttachmentListComponent
  ],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.scss'
})
export class TaskDetailComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private taskService = inject(TaskService);
  private tokenService = inject(TokenService);

  canWrite = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager, ROLES.Developer, ROLES.QA]);
  canDelete = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  statuses = TASK_STATUSES;
  statusLabels = STATUS_LABELS;
  priorityColors = PRIORITY_COLORS;

  projectId = 0;
  taskId = 0;
  task = signal<TaskDetail | null>(null);
  loading = signal(true);

  statusForm = this.fb.nonNullable.group({ status: ['Todo'] });
  checklistForm = this.fb.nonNullable.group({ text: ['', Validators.required] });
  timeLogForm = this.fb.nonNullable.group({
    hours: [1, [Validators.required, Validators.min(0.1)]],
    workDate: [new Date(), Validators.required],
    description: ['']
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.taskId = +this.route.snapshot.paramMap.get('taskId')!;
    this.loadTask();
  }

  loadTask(): void {
    this.loading.set(true);
    this.taskService.getTask(this.projectId, this.taskId).subscribe({
      next: t => {
        this.task.set(t);
        this.statusForm.patchValue({ status: t.status });
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  updateStatus(): void {
    const status = this.statusForm.getRawValue().status;
    this.taskService.updateStatus(this.projectId, this.taskId, { status }).subscribe(t => this.task.set(t));
  }

  addChecklistItem(): void {
    if (this.checklistForm.invalid) return;
    this.taskService.addChecklistItem(this.projectId, this.taskId, this.checklistForm.getRawValue()).subscribe(() => {
      this.checklistForm.reset({ text: '' });
      this.loadTask();
    });
  }

  toggleChecklistItem(item: ChecklistItem): void {
    this.taskService.updateChecklistItem(this.projectId, this.taskId, item.id, {
      text: item.text,
      isCompleted: !item.isCompleted
    }).subscribe(() => this.loadTask());
  }

  deleteChecklistItem(item: ChecklistItem): void {
    this.taskService.deleteChecklistItem(this.projectId, this.taskId, item.id).subscribe(() => this.loadTask());
  }

  addTimeLog(): void {
    if (this.timeLogForm.invalid) return;
    const value = this.timeLogForm.getRawValue();
    this.taskService.addTimeLog(this.projectId, this.taskId, {
      hours: value.hours,
      workDate: value.workDate.toISOString(),
      description: value.description || undefined
    }).subscribe(() => {
      this.timeLogForm.patchValue({ hours: 1, description: '' });
      this.loadTask();
    });
  }

  deleteTask(): void {
    const t = this.task();
    if (!t || !confirm(`Delete task ${t.taskKey}?`)) return;
    this.taskService.deleteTask(this.projectId, this.taskId).subscribe(() => {
      window.history.back();
    });
  }
}
