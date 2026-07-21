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
import { BugService } from '../../../core/services/bug.service';
import { ProjectService } from '../../../core/services/project.service';
import {
  BugListItem,
  BUG_PRIORITIES,
  BUG_SEVERITIES,
  BUG_STATUSES,
  STATUS_LABELS,
  SEVERITY_COLORS,
  PRIORITY_COLORS
} from '../../../core/models/bug.model';
import { PagedResponse } from '../../../core/models/api-response.model';
import { ProjectMember } from '../../../core/models/project.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-bug-list',
  imports: [
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
  templateUrl: './bug-list.component.html',
  styleUrl: './bug-list.component.scss'
})
export class BugListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private bugService = inject(BugService);
  private projectService = inject(ProjectService);
  private tokenService = inject(TokenService);

  canWrite = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager, ROLES.Developer, ROLES.QA]);
  canDelete = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  statuses = BUG_STATUSES;
  severities = BUG_SEVERITIES;
  priorities = BUG_PRIORITIES;
  statusLabels = STATUS_LABELS;
  severityColors = SEVERITY_COLORS;
  priorityColors = PRIORITY_COLORS;

  projectId = 0;
  bugs = signal<BugListItem[]>([]);
  members = signal<ProjectMember[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  pageSize = 20;
  pageNumber = 1;
  displayedColumns = ['bugKey', 'title', 'status', 'severity', 'priority', 'developerName', 'actions'];

  filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    status: [''],
    severity: [''],
    priority: [''],
    developerId: [null as number | null]
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.projectService.getMembers(this.projectId).subscribe(m => this.members.set(m));
    this.loadBugs();
  }

  loadBugs(): void {
    this.loading.set(true);
    const filter = this.filterForm.getRawValue();
    this.bugService.getBugs(this.projectId, {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: filter.searchTerm || undefined,
      status: filter.status || undefined,
      severity: filter.severity || undefined,
      priority: filter.priority || undefined,
      developerId: filter.developerId ?? undefined
    }).subscribe({
      next: (res: PagedResponse<BugListItem>) => {
        this.bugs.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPage(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadBugs();
  }

  deleteBug(bug: BugListItem): void {
    if (!confirm(`Delete bug ${bug.bugKey}?`)) return;
    this.bugService.deleteBug(this.projectId, bug.id).subscribe(() => this.loadBugs());
  }
}
