import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { ReportService } from '../../../core/services/report.service';
import { ProjectService } from '../../../core/services/project.service';
import { UserService } from '../../../core/services/user.service';
import { DeveloperReport, ReportFilter } from '../../../core/models/report.model';
import { ProjectListItem } from '../../../core/models/project.model';
import { UserListItem } from '../../../core/models/user.model';

@Component({
  selector: 'app-developer-report',
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatIconModule
  ],
  templateUrl: './developer-report.component.html',
  styleUrl: './developer-report.component.scss'
})
export class DeveloperReportComponent implements OnInit {
  private fb = inject(FormBuilder);
  private reportService = inject(ReportService);
  private projectService = inject(ProjectService);
  private userService = inject(UserService);

  projects = signal<ProjectListItem[]>([]);
  users = signal<UserListItem[]>([]);
  report = signal<DeveloperReport | null>(null);
  loading = signal(false);

  filterForm = this.fb.group({
    projectId: [null as number | null],
    userId: [null as number | null],
    startDate: [null as Date | null],
    endDate: [null as Date | null]
  });

  taskColumns = ['taskKey', 'title', 'projectName', 'completedDate'];

  ngOnInit(): void {
    this.projectService.getProjects({ pageSize: 100 }).subscribe(r => this.projects.set(r.items));
    this.userService.getUsers({ pageSize: 100 }).subscribe(r => this.users.set(r.items));
  }

  runReport(): void {
    const filter = this.buildFilter();
    if (!filter.userId) return;
    this.loading.set(true);
    this.reportService.getDeveloperReport(filter).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  exportPdf(): void {
    const filter = this.buildFilter();
    if (filter.userId) this.reportService.exportDeveloperPdf(filter);
  }

  exportExcel(): void {
    const filter = this.buildFilter();
    if (filter.userId) this.reportService.exportDeveloperExcel(filter);
  }

  private buildFilter(): ReportFilter {
    const v = this.filterForm.getRawValue();
    return {
      projectId: v.projectId ?? undefined,
      userId: v.userId ?? undefined,
      startDate: v.startDate ? v.startDate.toISOString().split('T')[0] : undefined,
      endDate: v.endDate ? v.endDate.toISOString().split('T')[0] : undefined
    };
  }
}
