import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { ReportService } from '../../../core/services/report.service';
import { ProjectService } from '../../../core/services/project.service';
import { BugReport, ReportFilter } from '../../../core/models/report.model';
import { ChartSlice } from '../../../core/models/dashboard.model';
import { ProjectListItem } from '../../../core/models/project.model';

@Component({
  selector: 'app-bug-report',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  templateUrl: './bug-report.component.html',
  styleUrl: './bug-report.component.scss'
})
export class BugReportComponent implements OnInit {
  private fb = inject(FormBuilder);
  private reportService = inject(ReportService);
  private projectService = inject(ProjectService);

  projects = signal<ProjectListItem[]>([]);
  report = signal<BugReport | null>(null);
  loading = signal(false);

  filterForm = this.fb.group({
    projectId: [null as number | null],
    startDate: [null as Date | null],
    endDate: [null as Date | null]
  });

  ngOnInit(): void {
    this.projectService.getProjects({ pageSize: 100 }).subscribe(r => this.projects.set(r.items));
  }

  runReport(): void {
    this.loading.set(true);
    this.reportService.getBugReport(this.buildFilter()).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  exportPdf(): void { this.reportService.exportBugPdf(this.buildFilter()); }
  exportExcel(): void { this.reportService.exportBugExcel(this.buildFilter()); }

  barWidth(slice: ChartSlice, slices: ChartSlice[]): number {
    const max = Math.max(...slices.map(s => s.value), 1);
    return (slice.value / max) * 100;
  }

  private buildFilter(): ReportFilter {
    const v = this.filterForm.getRawValue();
    return {
      projectId: v.projectId ?? undefined,
      startDate: v.startDate ? v.startDate.toISOString().split('T')[0] : undefined,
      endDate: v.endDate ? v.endDate.toISOString().split('T')[0] : undefined
    };
  }
}
