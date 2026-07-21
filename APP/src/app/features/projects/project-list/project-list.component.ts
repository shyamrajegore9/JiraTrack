import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProjectService } from '../../../core/services/project.service';
import { ProjectListItem } from '../../../core/models/project.model';
import { PagedResponse } from '../../../core/models/api-response.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-project-list',
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
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss'
})
export class ProjectListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private projectService = inject(ProjectService);
  private tokenService = inject(TokenService);

  canManage = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  displayedColumns = ['key', 'name', 'leadName', 'memberCount', 'status', 'actions'];
  projects = signal<ProjectListItem[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  pageSize = 20;
  pageNumber = 1;

  filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    isArchived: [null as boolean | null]
  });

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.loading.set(true);
    const filter = this.filterForm.getRawValue();
    this.projectService.getProjects({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: filter.searchTerm || undefined,
      isArchived: filter.isArchived ?? undefined
    }).subscribe({
      next: (res: PagedResponse<ProjectListItem>) => {
        this.projects.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPage(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadProjects();
  }

  archiveProject(project: ProjectListItem): void {
    const action = project.isArchived
      ? this.projectService.unarchiveProject(project.id)
      : this.projectService.archiveProject(project.id);
    action.subscribe(() => this.loadProjects());
  }

  deleteProject(project: ProjectListItem): void {
    if (!confirm(`Delete project ${project.key}?`)) return;
    this.projectService.deleteProject(project.id).subscribe(() => this.loadProjects());
  }
}
