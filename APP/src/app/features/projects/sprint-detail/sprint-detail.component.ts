import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SprintService } from '../../../core/services/sprint.service';
import { TaskService } from '../../../core/services/task.service';
import {
  SprintDetail,
  SprintVelocity,
  Burndown,
  STATUS_LABELS,
  STATUS_COLORS
} from '../../../core/models/sprint.model';
import { TaskListItem } from '../../../core/models/task.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-sprint-detail',
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatCardModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './sprint-detail.component.html',
  styleUrl: './sprint-detail.component.scss'
})
export class SprintDetailComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private sprintService = inject(SprintService);
  private taskService = inject(TaskService);
  private tokenService = inject(TokenService);

  canManage = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  statusLabels = STATUS_LABELS;
  statusColors = STATUS_COLORS;
  projectId = 0;
  sprintId = 0;
  sprint = signal<SprintDetail | null>(null);
  velocity = signal<SprintVelocity | null>(null);
  burndown = signal<Burndown | null>(null);
  availableTasks = signal<TaskListItem[]>([]);
  loading = signal(true);
  backlogColumns = ['taskKey', 'title', 'status', 'assigneeName', 'storyPoints', 'actions'];

  addForm = this.fb.nonNullable.group({ taskId: [0] });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.sprintId = +this.route.snapshot.paramMap.get('sprintId')!;
    this.loadAll();
  }

  loadAll(): void {
    this.loading.set(true);
    this.sprintService.getSprint(this.projectId, this.sprintId).subscribe({
      next: s => {
        this.sprint.set(s);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
    this.sprintService.getVelocity(this.projectId, this.sprintId).subscribe(v => this.velocity.set(v));
    this.sprintService.getBurndown(this.projectId, this.sprintId).subscribe(b => this.burndown.set(b));
    if (this.canManage) {
      this.taskService.getTasks(this.projectId, { pageSize: 100, parentOnly: true }).subscribe(res => {
        this.availableTasks.set(res.items);
        if (res.items.length) this.addForm.patchValue({ taskId: res.items[0].id });
      });
    }
  }

  startSprint(): void {
    this.sprintService.startSprint(this.projectId, this.sprintId).subscribe(s => {
      this.sprint.set(s);
      this.loadAll();
    });
  }

  closeSprint(): void {
    if (!confirm('Close sprint? Incomplete tasks will return to the backlog.')) return;
    this.sprintService.closeSprint(this.projectId, this.sprintId).subscribe(s => {
      this.sprint.set(s);
      this.loadAll();
    });
  }

  addTask(): void {
    const taskId = this.addForm.getRawValue().taskId;
    if (!taskId) return;
    this.sprintService.addToBacklog(this.projectId, this.sprintId, { taskId }).subscribe(() => this.loadAll());
  }

  removeTask(taskId: number): void {
    this.sprintService.removeFromBacklog(this.projectId, this.sprintId, taskId).subscribe(() => this.loadAll());
  }

  burndownMax(): number {
    const b = this.burndown();
    if (!b?.points.length) return 1;
    return Math.max(...b.points.flatMap(p => [p.idealRemaining, p.actualRemaining]), 1);
  }
}
