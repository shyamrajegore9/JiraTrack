import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReportService } from '../../../core/services/report.service';
import { ProjectReport } from '../../../core/models/report.model';
import { ChartSlice } from '../../../core/models/dashboard.model';

@Component({
  selector: 'app-project-report',
  imports: [RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './project-report.component.html',
  styleUrl: './project-report.component.scss'
})
export class ProjectReportComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private reportService = inject(ReportService);

  report = signal<ProjectReport | null>(null);
  loading = signal(true);

  ngOnInit(): void {
    const projectId = +this.route.snapshot.paramMap.get('projectId')!;
    this.reportService.getProjectReport(projectId).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  barWidth(slice: ChartSlice, slices: ChartSlice[]): number {
    const max = Math.max(...slices.map(s => s.value), 1);
    return (slice.value / max) * 100;
  }
}
