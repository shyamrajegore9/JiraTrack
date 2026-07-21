import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TaskService } from '../../../core/services/task.service';
import { ProjectService } from '../../../core/services/project.service';
import { TaskListItem, TASK_PRIORITIES, TASK_STATUSES, STATUS_LABELS, PRIORITY_COLORS } from '../../../core/models/task.model';
import { PagedResponse } from '../../../core/models/api-response.model';
import { ProjectMember } from '../../../core/models/project.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-task-list',
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private taskService = inject(TaskService);
  private projectService = inject(ProjectService);
  private tokenService = inject(TokenService);

  canWrite = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager, ROLES.Developer, ROLES.QA]);
  canDelete = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  statuses = TASK_STATUSES;
  priorities = TASK_PRIORITIES;
  statusLabels = STATUS_LABELS;
  priorityColors = PRIORITY_COLORS;

  projectId = 0;
  tasks = signal<TaskListItem[]>([]);
  members = signal<ProjectMember[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  pageSize = 20;
  pageNumber = 1;
  displayedColumns = ['taskKey', 'title', 'status', 'priority', 'assigneeName', 'dueDate', 'actions'];

  filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    status: [''],
    priority: [''],
    assigneeId: [null as number | null]
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.projectService.getMembers(this.projectId).subscribe(m => this.members.set(m));
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading.set(true);
    const filter = this.filterForm.getRawValue();
    this.taskService.getTasks(this.projectId, {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: filter.searchTerm || undefined,
      status: filter.status || undefined,
      priority: filter.priority || undefined,
      assigneeId: filter.assigneeId ?? undefined,
      parentOnly: true
    }).subscribe({
      next: (res: PagedResponse<TaskListItem>) => {
        this.tasks.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPage(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadTasks();
  }

  deleteTask(task: TaskListItem): void {
    if (!confirm(`Delete task ${task.taskKey}?`)) return;
    this.taskService.deleteTask(this.projectId, task.id).subscribe(() => this.loadTasks());
  }
}
