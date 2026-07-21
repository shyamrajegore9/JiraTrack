import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReportService } from '../../../core/services/report.service';
import { SprintReport } from '../../../core/models/report.model';

@Component({
  selector: 'app-sprint-report',
  imports: [DatePipe, RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './sprint-report.component.html',
  styleUrl: './sprint-report.component.scss'
})
export class SprintReportComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private reportService = inject(ReportService);

  report = signal<SprintReport | null>(null);
  loading = signal(true);

  ngOnInit(): void {
    const sprintId = +this.route.snapshot.paramMap.get('sprintId')!;
    const projectId = +this.route.snapshot.queryParamMap.get('projectId')!;
    this.reportService.getSprintReport(projectId, sprintId).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
